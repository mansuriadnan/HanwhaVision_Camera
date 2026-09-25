using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceExceptionController : ControllerBase
    {
        private readonly IDeviceExceptionWidgetService _deviceExeceptionWidgetService;
        private readonly ICurrentUserService _currentUserService;
        public DeviceExceptionController(IDeviceExceptionWidgetService deviceExceptionWidgetService, ICurrentUserService currentUserService)
        {
            _deviceExeceptionWidgetService = deviceExceptionWidgetService;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        [Route("DeviceException")]
        [CustomAuthorize([ScreenNames.MaintenanceApiError, ScreenNames.SiteApiError], ScreenNames.Site)]
        public async Task<ActionResult<StandardAPIResponse<DeviceExceptionResponse>>> DeviceExceptionDataAsync(WidgetRequest widgetRequest)
        {
            var result = await _deviceExeceptionWidgetService.DeviceExceptionDataAsync(widgetRequest);
            return StandardAPIResponse<DeviceExceptionResponse>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }
        
        [HttpPost]
        [Route("DeviceException/csv")]
        [CustomAuthorize([ScreenNames.MaintenanceApiError, ScreenNames.SiteApiError], ScreenNames.Site)]
        public async Task<IActionResult> DeviceExceptionDataCSV(WidgetRequest widgetRequest)
        {
            widgetRequest.UserId = _currentUserService.UserId;
            var csvBuilder = await _deviceExeceptionWidgetService.DeviceExceptionDataCSVAsync(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }
    }
}
