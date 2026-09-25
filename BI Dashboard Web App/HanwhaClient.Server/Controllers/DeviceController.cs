using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DeviceApiResponse;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text;


namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceMasterService _deviceMasterService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDeviceApiService _deviceApiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStringLocalizer<AppMessages> _localizer;

        public DeviceController(IDeviceMasterService cameraMasterService, ICurrentUserService currentUserService, IDeviceApiService deviceApiService, IHttpContextAccessor httpContextAccessor, IStringLocalizer<AppMessages> localizer)
        {
            this._deviceMasterService = cameraMasterService;
            this._currentUserService = currentUserService;
            this._deviceApiService = deviceApiService;
            this._httpContextAccessor = httpContextAccessor;
            _localizer = localizer;
        }

        [HttpPost]
        [Route("")]
        [CustomAuthorize([ScreenNames.AddOrUpdateDevices])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> AddUpdateDevice(DeviceRequestDto deviceRequestDto)
        {
            var userId = _currentUserService.UserId;
            var result = await _deviceMasterService.AddUpdateDevices(deviceRequestDto, userId);
            if (result.isSuccess)
            {
                return StandardAPIResponse<bool>.SuccessResponse(result.isSuccess, string.IsNullOrEmpty(deviceRequestDto.Id) ? _localizer[MessageKeys.RecordAdded] : _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.ErrorResponse(result.isSuccess, result.ErrorMessage, StatusCodes.Status400BadRequest);
        }

        [HttpPost]
        [Route("GetAllDevices")]
        [CustomAuthorize([ScreenNames.ViewListofDevices])]
        public async Task<ActionResult<StandardAPIResponse<DeviceResponse>>> AllDevicesAsync(DeviceRequest deviceRequest)
        {
            var data = await _deviceMasterService.GetAllDevicesByFilterAsync(deviceRequest);
            if (data.DeviceDetails.Count() > 0)
            {
                return StandardAPIResponse<DeviceResponse>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<DeviceResponse>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }

        [HttpPost]
        [Route("DeleteDevices")]
        [CustomAuthorize([ScreenNames.DeleteDevices])]
        public async Task<ActionResult<StandardAPIResponse<long>>> DeleteDevicesAsync(IEnumerable<string> id)
        {
            var userId = _currentUserService.UserId;
            var data = await _deviceMasterService.DeleteDevicesAsync(id, userId);
            return StandardAPIResponse<long>.SuccessResponse(data, _localizer[MessageKeys.RecordDeleted]);
        }

        [HttpGet]
        [Route("GetDevicesWithoutZones")]
        [CustomAuthorize([ScreenNames.DeviceMaster, ScreenNames.ConfigureFloorPlanZoneMapCamera])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<DevicesWithoutZonesResponseDto>>>> GetDevicesWithoutZones()
        {
            var data = await _deviceMasterService.GetDevicesWithoutZones();
            if (data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<DevicesWithoutZonesResponseDto>>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<DevicesWithoutZonesResponseDto>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }

        [HttpGet]
        [Route("DeviceApiCall")]
        [Authorize]
        public async Task<ActionResult<StandardAPIResponse<dynamic>>> DeviceApiCall(string Url, string userName, string password)
        {
            var data = await _deviceApiService.CallDeviceApi<ObjectCountingLiveResponse>(Url, userName, password);
            return StandardAPIResponse<dynamic>.SuccessResponse("", "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("GetAllChannels")]
        [CustomAuthorize([ScreenNames.AddOrUpdateDevices])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<AllChannelsResDto>>>> GetAllChannels(AllChannelsReqDto dto)
        {
            var data = await _deviceMasterService.GetAllChannels(dto);
            if (data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<AllChannelsResDto>>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<AllChannelsResDto>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }
        [HttpPost]
        [Route("GetDeviceEventsLogs")]
        [CustomAuthorize([ScreenNames.ViewListofEvents])]
        public async Task<ActionResult<StandardAPIResponse<DeviceEventsLogsRes>>> GetDeviceEventsLogs([FromBody] DeviceEventsLogsRequest request)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            IEnumerable<string>? userRoles = user?.Claims.Where(x => x.Type == "role").Select(y => y.Value);
            var data = await _deviceMasterService.GetDeviceEventsLogsAsync1(request, userRoles);
            if (data.EventsLogsDetails.Count() > 0)
            {
                return StandardAPIResponse<DeviceEventsLogsRes>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<DeviceEventsLogsRes>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);

        }

        [HttpPost]
        [Route("UpdateDeviceEventsStatus")]
        [CustomAuthorize([ScreenNames.AcknowledgeEvent, ScreenNames.AcknowledgeNotification])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> UpdateDeviceEventsStatus(DeviceChangeStatusRequest deviceChangeStatusRequest)
        {
            var userId = _currentUserService.UserId;
            var data = await _deviceMasterService.UpdateDeviceEventsStatusAsync(deviceChangeStatusRequest, userId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }

        [HttpGet]
        [Route("MapCameraListByFeatures")]
        [CustomAuthorize([ScreenNames.FloorPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<MapCameraListByFeatures>>>> MapCameraListByFeatures([FromQuery] string feature, string? floorId)
        {
            var userId = _currentUserService.UserId;
            var data = await _deviceMasterService.MapCameraListByFeaturesAsync(feature, floorId);
            if (data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<MapCameraListByFeatures>>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<MapCameraListByFeatures>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }

        [HttpGet]
        [Route("CameraListHeatmap")]
        [CustomAuthorize([ScreenNames.FloorPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<MapCameraListByFeatures>>>> CameraListByHeatmap([FromQuery] string heatmapType)
        {
            var userId = _currentUserService.UserId;
            var data = await _deviceMasterService.GetCameraListByHeatmapTypeAsync(heatmapType);
            if (data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<MapCameraListByFeatures>>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<MapCameraListByFeatures>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }


        [HttpPost]
        [Route("GetDevicesByFloorAndZones")]
        [CustomAuthorize([ScreenNames.MapPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<MapCameraListByFeatures>>>> GetDevicesByFloorAndZones(DeviceByFloorsAndZonesRequest deviceByFloorsAndZonesRequest)
        {
            var userId = _currentUserService.UserId;
            var data = await _deviceMasterService.GetDevicesByFloorAndZonesAsync(deviceByFloorsAndZonesRequest.FloorIds, deviceByFloorsAndZonesRequest.ZoneIds);
            if (data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<MapCameraListByFeatures>>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<MapCameraListByFeatures>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }

        [HttpPost("UploadBulkDevice")]
        [CustomAuthorize([ScreenNames.AddOrUpdateDevices])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                // return BadRequest("No file uploaded.");
                return StandardAPIResponse<bool>.ErrorResponse(false, "No file uploaded.", StatusCodes.Status400BadRequest);

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls")
                return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.FileFormatInvalid], StatusCodes.Status400BadRequest);
            //return BadRequest("Invalid file format. Please upload a valid Excel file (.xlsx or .xls).");

            var expectedColumns = new List<string>
                                  {
                                      "DeviceType", "DeviceName", "IpAddress", "UserName",
                                      "Password", "DevicePort", "IsHttps", "Location"
                                  };

            var devices = new List<DeviceRequestDto>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                IWorkbook workbook;
                if (extension == ".xlsx")
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);
                if (sheet == null)
                    return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.ExcelFileNotSheet], StatusCodes.Status400BadRequest);

                // ✅ Read header row
                //var headerRow = sheet.GetRow(0);
                //if (headerRow == null)
                //    return StandardAPIResponse<bool>.ErrorResponse(false, "Header row not found in Excel file.", StatusCodes.Status400BadRequest);

                //var headerColumns = new List<string>();
                //for (int i = 0; i < headerRow.LastCellNum; i++)
                //{
                //    headerColumns.Add(headerRow.GetCell(i)?.ToString()?.Trim() ?? string.Empty);
                //}

                // ✔ Try to find header row within the first 10 rows
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


                // ✅ Validate required columns
                var missingColumns = expectedColumns
                    .Where(c => !headerColumns.Contains(c, System.StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (missingColumns.Any())
                {
                    var joined = string.Join(", ", missingColumns);
                    return StandardAPIResponse<bool>.ErrorResponse(false, string.Format(_localizer[MessageKeys.ExcelMissingColumn], joined), StatusCodes.Status400BadRequest);
                }

                // ✅ Read data rows
                for (int rowIndex = dataStartRow; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    var row = sheet.GetRow(rowIndex);
                    if (row == null) continue;

                    var dto = new DeviceRequestDto
                    {
                        DeviceType = GetCellValue(row, headerColumns, "DeviceType"),
                        DeviceName = GetCellValue(row, headerColumns, "DeviceName"),
                        IpAddress = GetCellValue(row, headerColumns, "IpAddress"),
                        UserName = GetCellValue(row, headerColumns, "UserName"),
                        Password = GetCellValue(row, headerColumns, "Password"),
                        DevicePort = GetCellValue(row, headerColumns, "DevicePort"),
                        IsHttps = bool.TryParse(GetCellValue(row, headerColumns, "IsHttps"), out bool isHttps) && isHttps,
                        Location = GetCellValue(row, headerColumns, "Location")
                    };

                    devices.Add(dto);
                }
            }

            if (!devices.Any())
                return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.NoValidDeviceRows], StatusCodes.Status400BadRequest);
            //return BadRequest("No valid device rows found in the Excel file.");

            var userId = _currentUserService.UserId;
            var failedDevices = new List<string>();

            foreach (var device in devices)
            {
                var result = await _deviceMasterService.AddUpdateDevices(device, userId);
                if (!result.isSuccess)
                    failedDevices.Add($"{device.DeviceName} - {result.ErrorMessage}");
            }

            if (failedDevices.Any())
            {
                var failedSummary = string.Join(Environment.NewLine, failedDevices);
                return StandardAPIResponse<bool>.ErrorResponse(false, failedSummary, StatusCodes.Status400BadRequest);
            }
            //return Ok(new
            //{
            //    Message = "Some devices failed to upload.",
            //    failedDevices = failedDevices.Add($"Some devices failed to upload."),
            //    FailedDevices = failedDevices
            //});

            //return Ok(new { Message = "All devices uploaded successfully." });
            return StandardAPIResponse<bool>.SuccessResponse(true, _localizer[MessageKeys.AllDeviceUploaded], StatusCodes.Status200OK);
        }

        private string GetCellValue(IRow row, List<string> headers, string columnName)
        {
            int index = headers.FindIndex(h => h.Equals(columnName, System.StringComparison.OrdinalIgnoreCase));
            if (index == -1) return string.Empty;
            var cell = row.GetCell(index);
            return cell?.ToString()?.Trim() ?? string.Empty;
        }

        [HttpGet("DownloadSampleFile")]
        [CustomAuthorize([ScreenNames.AddOrUpdateDevices])]
        public IActionResult DownloadSample()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot","SampleFiles","DeviceSample.xlsx");

            if (!System.IO.File.Exists(filePath))
                return NotFound(_localizer[MessageKeys.SampleFileNotFound]);

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName = "DeviceSample.xlsx";

            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }

        [HttpGet]
        [Route("GetDevicesAsync")]
        [CustomAuthorize([ScreenNames.ViewListofDevices])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<DeviceListRes>>>> GetDevicesAsync(string deviceType)
        {
            var data = await _deviceMasterService.GetDevicesAsync(deviceType);
            if (data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<DeviceListRes>>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<DeviceListRes>>.SuccessResponse(null, AppMessageConstants.RecordNotFound, StatusCodes.Status404NotFound);
        }

        [HttpPost]
        [Route("ExportDeviceCSV")]
        [CustomAuthorize([ScreenNames.ViewListofDevices])]
        public async Task<IActionResult> ExportDeviceCSV(DeviceRequest deviceRequest)
        {
            var csvBuilder = await _deviceMasterService.GetDevicesCSVAsync(deviceRequest);
            if (csvBuilder == null || csvBuilder.Length == 0)
            {
                return NoContent(); // HTTP 204
            }
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"Device.csv");
        }

        [HttpPost]
        [Route("UpdateDeviceMaintenanceStatus")]
        [CustomAuthorize([ScreenNames.AddOrUpdateDevices])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> UpdateDeviceMaintenanceStatus(DeviceMaintenanceStatusRequest deviceMaintenanceStatus)
        {
            var userId = _currentUserService.UserId;
            var result = await _deviceMasterService.UpdateDeviceMaintenanceStatus(deviceMaintenanceStatus, userId);
            if (result.isSuccess)
            {
                return StandardAPIResponse<bool>.SuccessResponse(result.isSuccess, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.ErrorResponse(result.isSuccess, result.ErrorMessage, StatusCodes.Status400BadRequest);

        }

        [HttpPost]
        [Route("ManuallyOpenGateBarrier")]
        [CustomAuthorize([ScreenNames.ManuallyOpenGate])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> ManuallyOpenGateBarrier([FromBody] OpenGateDeviceRequest request)
        {
            var userId = _currentUserService.UserId;
            var data = await _deviceMasterService.ManuallyOpenGateBarrierAsync(request.DeviceId, userId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.GateOpenSuccessManually], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.ErrorResponse(data, _localizer[MessageKeys.FailToOpenGateManually], StatusCodes.Status400BadRequest);
        }


        //[HttpGet]
        //[Route("StreamCamera/{id}")]
        //[AllowAnonymous]
        //public async Task StreamCamera(string id)
        //{
        //    try
        //    {
        //        //var cancellationToken = HttpContext.RequestAborted;

        //        //HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        //        //var cameraUrl = $"http://10.37.58.{id}/stw-cgi/video.cgi?msubmenu=stream&action=view&Profile=1&CodecType=MJPEG";
        //        //var cameraUsername = "admin";
        //        //var cameraPassword = "TeamIndia@2025";

        //        //var handler = new HttpClientHandler
        //        //{
        //        //    PreAuthenticate = true,
        //        //    UseDefaultCredentials = false,
        //        //    Credentials = new System.Net.NetworkCredential(cameraUsername, cameraPassword)
        //        //};

        //        //using var client = new HttpClient(handler);
        //        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/x-mixed-replace"));

        //        //var response = await client.GetAsync(cameraUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        //        //Console.WriteLine($"Camera content type: {response.Content.Headers.ContentType}");

        //        //if (!response.IsSuccessStatusCode)
        //        //{
        //        //    Response.StatusCode = (int)response.StatusCode;
        //        //    await Response.Body.WriteAsync(Encoding.UTF8.GetBytes("Failed to connect to camera stream"));
        //        //    return;
        //        //}

        //        //// Pass camera's exact content type (including boundary)
        //        //Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "multipart/x-mixed-replace; boundary=frame";

        //        //// Optional: prevent caching
        //        //Response.Headers["Cache-Control"] = "no-store";
        //        //Response.Headers["Pragma"] = "no-cache";
        //        //Response.Headers["Expires"] = "0";
        //        //Response.Headers["Connection"] = "keep-alive";
        //        //Response.Headers["X-Accel-Buffering"] = "no";
        //        //await Response.Body.FlushAsync(); // Flush headers early

        //        //await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        //        //var buffer = new byte[8192];
        //        //int bytesRead;

        //        //while (!cancellationToken.IsCancellationRequested &&
        //        //       (bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        //        //{
        //        //    await Response.Body.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        //        //    await Response.Body.FlushAsync(cancellationToken); // Important to force chunk to browser
        //        //}
        //        var cancellationToken = HttpContext.RequestAborted;
        //        HttpContext.Response.Headers.Append("X-Accel-Buffering", "no");
        //        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        //        var cameraUrl = $"http://10.37.58.{id}/stw-cgi/video.cgi?msubmenu=stream&action=view&Profile=1&CodecType=MJPEG";
        //        var cameraUsername = "admin";
        //        var cameraPassword = "TeamIndia@2025";

        //        var digestClient = new DigestHandler(cameraUsername, cameraPassword);
        //        var response = await digestClient.SendAsync(cameraUrl, cancellationToken);
        //        Console.WriteLine($"Camera content type: {response.Content.Headers.ContentType}");

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            Response.StatusCode = (int)response.StatusCode;
        //            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes("Failed to connect to camera stream"));
        //            return;
        //        }

        //        // Use exact content type (important for boundary parsing)
        //        Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "multipart/x-mixed-replace";

        //        // Set no-cache and flush early
        //        Response.Headers["Cache-Control"] = "no-store";
        //        Response.Headers["Pragma"] = "no-cache";
        //        Response.Headers["Expires"] = "0";
        //        Response.Headers["Connection"] = "keep-alive";
        //        await Response.Body.FlushAsync();

        //        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        //        var buffer = new byte[8192];
        //        int bytesRead;

        //        while (!cancellationToken.IsCancellationRequested &&
        //               (bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        //        {
        //            await Response.Body.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        //            await Response.Body.FlushAsync(cancellationToken);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Streaming error: {ex.Message}");
        //        Response.StatusCode = 500;
        //    }

        //}
    }
}
