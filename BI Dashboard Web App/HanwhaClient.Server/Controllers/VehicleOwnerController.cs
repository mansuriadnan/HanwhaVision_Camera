using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.ComponentModel;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleOwnerController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IVehicleOwnerService _vehicleOwnerService;
        private readonly IDeviceMasterService _deviceMasterService;
        private readonly IStringLocalizer<AppMessages> _localizer;
        public VehicleOwnerController(ICurrentUserService currentUserService,
            IVehicleOwnerService vehicleOwnerService,
            IStringLocalizer<AppMessages> localizer,
            IDeviceMasterService deviceMasterService)
        {
            _vehicleOwnerService = vehicleOwnerService;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _deviceMasterService = deviceMasterService;
        }

        [HttpPost]
        [Route("")]
        [CustomAuthorize([ScreenNames.AddOrUpdateVehicleOwner])]
        public async Task<ActionResult<StandardAPIResponse<string>>> CreateVehicleOwnerAsync(VehicleOwnerRequest vehicleOwnerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = _currentUserService.UserId;
            var data = await _vehicleOwnerService.SaveVehicleOwnerAsync(vehicleOwnerRequest, userId);
            if (string.IsNullOrEmpty(data.Id))
            {
                return StandardAPIResponse<string>.ErrorResponse(string.Empty, data.ErrorMessage, StatusCodes.Status400BadRequest);
            }
            return StandardAPIResponse<string>.SuccessResponse(data.Id, string.IsNullOrEmpty(vehicleOwnerRequest.Id) ? AppMessageConstants.RecordAdded : AppMessageConstants.RecordUpdated);
        }

        [HttpPost]
        [Route("GetAllVehicleOwner")]
        [CustomAuthorize([ScreenNames.ViewVehicleOwner])]
        public async Task<ActionResult<StandardAPIResponse<AllVehicleOwnerListResponse>>> GetAllVehicleOwnerAsync(AllVehicleOwnerRequest request)
        {
            var result = await _vehicleOwnerService.GetAllVehicleOwnerAsync(request);
            var response = StandardAPIResponse<AllVehicleOwnerListResponse>.SuccessResponse(result, AppMessageConstants.RecordRetrieved, StatusCodes.Status200OK);
            return response;
        }

        [HttpPost]
        [Route("DeleteOwner")]
        [CustomAuthorize([ScreenNames.DeleteVehicleOwner])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteOwnerAsync(VehicleOwneDeleteRequest request)
        {
            var userId = _currentUserService.UserId;
            var data = await _vehicleOwnerService.DeleteOwnerAsync(request.Id, userId);
            if (!data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(false, AppMessageConstants.FailedToDeleteOwnerOrVehicle, StatusCodes.Status500InternalServerError);
            }
            return StandardAPIResponse<bool>.SuccessResponse(data, AppMessageConstants.RecordDeleted);
        }

        [HttpPost("UploadVehicleOwner")]
        [CustomAuthorize([ScreenNames.AddOrUpdateVehicleOwner])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> UploadVehicleOwnerExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("VehicleOwner Excel file is required.");

            using var stream = file.OpenReadStream();
            IWorkbook workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);
            if (sheet == null)
                return BadRequest("Sheet not found.");

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

            var colMap = BuildColumnIndexMap(headerRow);

            // check missing required columns
            var required = new[]
                           {
                             "RegistrationType","OwnerName","Building","BuildingUnit",
                             "Email","ContactNumber","AllowedVehicle"
                           };

            var missingColumns = new List<string>();
            foreach (var r in required)
            {
                if (!colMap.ContainsKey(r))
                    missingColumns.Add(r);
            }

            if (missingColumns.Any())
            {
                var joined = string.Join(", ", missingColumns);
                return StandardAPIResponse<bool>.ErrorResponse(false, string.Format(_localizer[MessageKeys.ExcelMissingColumn], joined), StatusCodes.Status400BadRequest);
            }

            // now read rows using the map (tolerant to order & misspellings)
            var owners = new List<VehicleOwnerRequest>();

            for (int rowIndex = dataStartRow; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null || IsRowEmpty(row, headerRow.LastCellNum))
                    continue;

                var owner = new VehicleOwnerRequest
                {
                    RegistrationType = GetString(row.GetCell(colMap["RegistrationType"])),
                    OwnerName = GetString(row.GetCell(colMap["OwnerName"])),
                    Building = GetString(row.GetCell(colMap["Building"])),
                    BuildingUnit = GetString(row.GetCell(colMap["BuildingUnit"])),
                    Email = GetString(row.GetCell(colMap["Email"])),
                    ContactNumber = GetString(row.GetCell(colMap["ContactNumber"])),
                    AllowedVehicle = GetInt(row.GetCell(colMap["AllowedVehicle"])),
                    AllowedFromTime = colMap.TryGetValue("AllowedFromTime", out var c1)
                                            ? GetDateTimeNullable(row.GetCell(c1)) : null,
                    AllowedToTime = colMap.TryGetValue("AllowedToTime", out var c2)
                                            ? GetDateTimeNullable(row.GetCell(c2)) : null,
                    AllowedGates = colMap.TryGetValue("AllowedGates", out var c3)
                                            ? GetString(row.GetCell(c3))
                                                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                            : null,
                    EnabledAlarmForOverstay = colMap.TryGetValue("EnabledAlarmForOverstay", out var c4)
                                                && GetBool(row.GetCell(c4)),
                    EnabledAlarmFor24HStay = colMap.TryGetValue("EnabledAlarmFor24HStay", out var c5)
                                                && GetBool(row.GetCell(c5))
                };

                // map IPs / gate identifiers -> deviceIds and assign back to owner.AllowedGates
                if (owner.AllowedGates.Count() > 0)
                {
                    var deviceIds = await _deviceMasterService.GetDeviceIdByIpAddressAsync(owner.AllowedGates);
                    owner.AllowedGates = deviceIds?.ToArray() ?? Array.Empty<string>();

                }

                owners.Add(owner);
            }

            var userId = _currentUserService.UserId;
            var failedDevices = new List<string>();
            // after loop, insert each record
            foreach (var owner in owners)
            {
                var result = await _vehicleOwnerService.SaveVehicleOwnerAsync(owner, userId);
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

        // Key = canonical property name, Value = allowed header texts (case-insensitive)
        private static readonly Dictionary<string, string[]> VehicleOwnerHeaderMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["RegistrationType"] = new[] { "RegistrationType", "RegType", "Registration Type" },
                ["OwnerName"] = new[] { "OwnerName", "Owner Name", "Name" },
                ["Building"] = new[] { "Building", "Block" },
                ["BuildingUnit"] = new[] { "BuildingUnit", "Flat", "Unit", "Apartment" },
                ["Email"] = new[] { "Email", "EmailId", "E-mail" },
                ["ContactNumber"] = new[] { "ContactNumber", "Mobile", "Phone", "Contact No" },
                ["AllowedVehicle"] = new[] { "AllowedVehicle", "VehicleLimit", "Allowed Vehicle" },
                ["AllowedFromTime"] = new[] { "AllowedFromTime", "FromTime", "Allowed From" },
                ["AllowedToTime"] = new[] { "AllowedToTime", "ToTime", "Allowed To" },
                ["AllowedGates"] = new[] { "AllowedGates", "Gates", "Allowed Gates" },
                ["EnabledAlarmForOverstay"] = new[] { "EnabledAlarmForOverstay", "OverstayAlarm", "Overstay Alarm" },
                ["EnabledAlarmFor24HStay"] = new[] { "EnabledAlarmFor24HStay", "Alarm24H", "24H Alarm" }
            };

        private static Dictionary<string, int> BuildColumnIndexMap(IRow headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                var raw = headerRow.GetCell(i)?.ToString() ?? string.Empty;
                var header = raw.Trim();

                foreach (var kvp in VehicleOwnerHeaderMap)
                {
                    foreach (var alias in kvp.Value)
                    {
                        if (string.Equals(header, alias, StringComparison.OrdinalIgnoreCase))
                        {
                            map[kvp.Key] = i;
                            break;
                        }
                    }
                }
            }

            return map;
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
        private static string GetString(ICell cell)
        {
            return cell == null ? string.Empty : cell.ToString()?.Trim() ?? string.Empty;
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
        private static bool GetBool(ICell cell)
        {
            if (cell == null)
                return false;

            if (cell.CellType == CellType.Boolean)
                return cell.BooleanCellValue;

            var text = cell.ToString()?.Trim().ToLowerInvariant();
            if (bool.TryParse(text, out var b))
                return b;

            return text is "yes" or "y" or "1";
        }

        [HttpGet("DownloadSampleFile")]
        [CustomAuthorize([ScreenNames.AddOrUpdateVehicleOwner])]
        public IActionResult DownloadSample()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "SampleFiles", "VehicleOwnerSample.xlsx");

            if (!System.IO.File.Exists(filePath))
                return NotFound(_localizer[MessageKeys.SampleFileNotFound]);

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName = "VehicleOwnerSample.xlsx";

            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }
    }
}
