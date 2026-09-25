using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ANPRVehicleController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IANPRVehicleService _anprVehicleService;
        private readonly IStringLocalizer<AppMessages> _localizer;
        public ANPRVehicleController(ICurrentUserService currentUserService,
            IANPRVehicleService anprVehicleService,
            IStringLocalizer<AppMessages> localizer)
        {
            _anprVehicleService = anprVehicleService;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }
        [HttpPost]
        [Route("")]
        [CustomAuthorize([ScreenNames.AddOrUpdateANPRVehicle])]
        public async Task<ActionResult<StandardAPIResponse<string>>> CreateANPRVehicleAsync(ANPRVehicleRequest anprVehicleRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _currentUserService.UserId;
            var data = await _anprVehicleService.SaveANPRVehicleAsync(anprVehicleRequest, userId);
            if (string.IsNullOrEmpty(data.Id))
            {
                return StandardAPIResponse<string>.ErrorResponse(string.Empty, data.ErrorMessage, StatusCodes.Status400BadRequest);
            }
            return StandardAPIResponse<string>.SuccessResponse(data.Id, string.IsNullOrEmpty(anprVehicleRequest.Id) ? AppMessageConstants.RecordAdded : AppMessageConstants.RecordUpdated);
        }
        [HttpPost]
        [Route("GetAllANPRVehicleByOwner")]
        [CustomAuthorize([ScreenNames.ViewANPRVehicle])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<ANPRVehicleList>>>> GetAllANPRVehicleByOwnerAsync(AllANPRVehicleRequest request)
        {
            var result = await _anprVehicleService.GetAllANPRVehicleByOwnerAsync(request);
            var response = StandardAPIResponse<IEnumerable<ANPRVehicleList>>.SuccessResponse(result, AppMessageConstants.RecordRetrieved, StatusCodes.Status200OK);
            return response;
        }
        [HttpPost]
        [Route("DeleteANPRVehicle")]
        [CustomAuthorize([ScreenNames.DeleteANPRVehicle])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteANPRVehiclesAsync(ANPRVehicleDeleteRequest request)
        {
            var userId = _currentUserService.UserId;
            var data = await _anprVehicleService.DeleteANPRVehiclesAsync(request.Id, userId);
            return StandardAPIResponse<bool>.SuccessResponse(data, AppMessageConstants.RecordDeleted);
        }

        [HttpPost("UploadANPRVehicle")]
        [CustomAuthorize([ScreenNames.AddOrUpdateANPRVehicle])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> UploadAnprExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("ANPR Excel file is required.");

            using var stream = file.OpenReadStream();
            IWorkbook workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);
            if (sheet == null)
                return BadRequest("ANPR sheet not found.");

            IRow headerRow = null;
            int headerRowIndex = -1;

            for (int r = 0; r < 10; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;

                // If the row contains any non-empty cell → treat as header
                bool hasData = false;
                for (int c = 0; c < row.LastCellNum; c++)
                {
                    var cellValue = row.GetCell(c)?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        hasData = true;
                        break;
                    }
                }

                if (hasData)
                {
                    headerRow = row;
                    headerRowIndex = r;
                    break;
                }
            }

            // var headerRow = sheet.GetRow(0);
            if (headerRow == null)
                return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.HeaderRowNotExits], StatusCodes.Status400BadRequest);

            // ✔ Read header columns
            var headerColumns = new List<string>();
            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                headerColumns.Add(headerRow.GetCell(i)?.ToString()?.Trim() ?? string.Empty);
            }

            // Now start reading data rows from the next row after detected header
            int dataStartRow = headerRowIndex + 1;

            // Build header map exactly like VehicleOwner (with aliases if you want)
            var colMap = BuildAnprColumnIndexMap(headerRow);

            var requiredColumns = new[]
                                    {
                                        "Building",
                                        "BuildingUnit",
                                        "Country",
                                        "State",
                                        "PlateCode",   // header text can be "Plate Code" via alias map
                                        "PlateCategory",
                                        "Make",
                                        "Model",
                                        "Color"
                                    };

            var missingColumns = new List<string>();
            foreach (var r in requiredColumns)
            {
                if (!colMap.ContainsKey(r))
                    missingColumns.Add(r);
            }

            if (missingColumns.Any())
            {
                var joined = string.Join(", ", missingColumns);
                return StandardAPIResponse<bool>.ErrorResponse(false, string.Format(_localizer[MessageKeys.ExcelMissingColumn], joined), StatusCodes.Status400BadRequest);
            }

            var anprList = new List<ANPRVehicleRequest>();

            var failedDevices = new List<string>();
            for (int rowIndex = dataStartRow; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null || IsRowEmpty(row, headerRow.LastCellNum))
                    continue;


                var building = GetString(row.GetCell(colMap["Building"]));
                var buildingUnit = GetString(row.GetCell(colMap["BuildingUnit"]));

                var getData = await _anprVehicleService.GetOwnerIdAndRegistrationTypeAsync(building, buildingUnit);

                if (getData.Count() == 0)
                {
                    var errorMsg = $"Owner {building}-{buildingUnit} doesn't exist.";
                    return StandardAPIResponse<bool>.ErrorResponse(false, errorMsg, StatusCodes.Status400BadRequest);
                }

                var isPermenant = getData.Where(x => x.RegistrationType?.ToLower() == "permanent").Any();

                var CountryName = GetString(row.GetCell(colMap["Country"]));
                var countryId = "";
                if (!string.IsNullOrEmpty(CountryName))
                {
                    var countryExits = await _anprVehicleService.CountryIdsByNameAsync(CountryName);

                    if (countryExits.Count() == 0)
                    {
                        var errorMsg = $"Country {CountryName} doesn't exist.";
                        return StandardAPIResponse<bool>.ErrorResponse(false, errorMsg, StatusCodes.Status400BadRequest);
                    }

                    countryId = countryExits.FirstOrDefault();
                }

                if (isPermenant && (countryId != ""))
                {
                    var anpr = new ANPRVehicleRequest
                    {
                        // you can later resolve VehicleOwnerId by Building + BuildingUnit
                        VehicleOwnerId = getData.Where(x => x.RegistrationType?.ToLower() == "permanent").FirstOrDefault().OwnerId,
                        Country = countryId,
                        State = GetString(row.GetCell(colMap["State"])),
                        PlateCode = GetString(row.GetCell(colMap["PlateCode"])),
                        PlateCategory = colMap.TryGetValue("PlateCategory", out var pcIndex)
                                                     ? GetString(row.GetCell(pcIndex))
                                                     : string.Empty,
                        Make = GetString(row.GetCell(colMap["Make"])),
                        Model = GetString(row.GetCell(colMap["Model"])),
                        Color = GetString(row.GetCell(colMap["Color"])),
                        VisitorValidFrom = colMap.TryGetValue("VisitorValidFrom", out var vfIndex)
                                                     ? GetDateTimeNullable(row.GetCell(vfIndex))
                                                     : null,
                        VisitorValidTo = colMap.TryGetValue("VisitorValidTo", out var vtIndex)
                                                     ? GetDateTimeNullable(row.GetCell(vtIndex))
                                                     : null
                    };

                    anprList.Add(anpr);
                }
            }

            var userId = _currentUserService.UserId;

            // then save each record
            foreach (var anpr in anprList)
            {
                var result = await _anprVehicleService.SaveANPRVehicleAsync(anpr, userId);
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                    failedDevices.Add($"{result.ErrorMessage}");
            }

            if (failedDevices.Any())
            {
                var failedSummary = string.Join(Environment.NewLine, failedDevices);
                return StandardAPIResponse<bool>.ErrorResponse(false, failedSummary, StatusCodes.Status400BadRequest);
            }
            return StandardAPIResponse<bool>.SuccessResponse(true, _localizer[MessageKeys.RecordAdded], StatusCodes.Status200OK);
        }

        [HttpGet("DownloadSampleFile")]
        [CustomAuthorize([ScreenNames.AddOrUpdateANPRVehicle])]
        public IActionResult DownloadSample()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "SampleFiles", "ANPRVehicleSample.xlsx");

            if (!System.IO.File.Exists(filePath))
                return NotFound(_localizer[MessageKeys.SampleFileNotFound]);

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName = "ANPRVehicleSample.xlsx";

            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }

        private static bool IsRowEmpty(IRow row, int colCount)
        {
            for (int i = 0; i < colCount; i++)
            {
                var cell = row.GetCell(i);
                if (cell != null && !string.IsNullOrWhiteSpace(cell.ToString()))
                    return false;
            }
            return true;
        }

        private static int GetInt(ICell cell)
        {
            if (cell == null)
                return 0;

            if (cell.CellType == CellType.Numeric)
                return (int)cell.NumericCellValue;

            if (int.TryParse(cell.ToString(), out var value))
                return value;

            return 0;
        }

        private static string GetString(ICell cell)
        {
            return cell == null ? string.Empty : cell.ToString()?.Trim() ?? string.Empty;
        }

        private static DateTime? GetDateTimeNullable(ICell cell)
        {
            if (cell == null)
                return null;

            if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                return cell.DateCellValue;

            if (DateTime.TryParse(cell.ToString(), out var dt))
                return dt;

            return null;
        }

        private static readonly Dictionary<string, string[]> AnprHeaderMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Building"] = new[] { "Building", "Block" },
            ["BuildingUnit"] = new[] { "BuildingUnit", "Flat", "Unit", "Apartment" },
            ["Country"] = new[] { "Country" },
            ["State"] = new[] { "State", "StateCode", "RTO" },
            ["PlateCode"] = new[] { "PlateCode", "Plate Code" },
            ["PlateCategory"] = new[] { "PlateCategory", "Category" },
            ["Make"] = new[] { "Make", "Brand" },
            ["Model"] = new[] { "Model" },
            ["Color"] = new[] { "Color" },
            ["VisitorValidFrom"] = new[] { "VisitorValidFrom", "ValidFrom" },
            ["VisitorValidTo"] = new[] { "VisitorValidTo", "ValidTo" }
        };

        private static Dictionary<string, int> BuildAnprColumnIndexMap(IRow headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                var header = headerRow.GetCell(i)?.ToString()?.Trim() ?? string.Empty;
                foreach (var kvp in AnprHeaderMap)
                {
                    foreach (var alias in kvp.Value)
                    {
                        if (header.Equals(alias, StringComparison.OrdinalIgnoreCase))
                        {
                            map[kvp.Key] = i;
                            break;
                        }
                    }
                }
            }

            return map;
        }


    }
}
