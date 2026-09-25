using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Text;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize([ScreenNames.MaintenanceMaster])]
    public class RMAController : ControllerBase
    {
        private readonly IRMAService _rmaService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<AppMessages> _localizer;

        public RMAController(IRMAService service, ICurrentUserService currentUserService, IStringLocalizer<AppMessages> localizer)
        {
            _rmaService = service;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        [HttpPost]
        [Route("GetRMA")]
        [CustomAuthorize([ScreenNames.ViewRMA])]
        public async Task<ActionResult<StandardAPIResponse<PagedResult<RMAResponseDto>>>>
        GetAllRMA([FromBody] RMASerachModel query)
        {
            var result = await _rmaService.GetRMAAsync(query);

            if (result != null && result.Items.Any())
                return StandardAPIResponse<PagedResult<RMAResponseDto>>
                    .SuccessResponse(result, "");

            return StandardAPIResponse<PagedResult<RMAResponseDto>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status200OK);
        }

        [HttpPost]
        [CustomAuthorize([ScreenNames.AddOrUpdateRMA])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> SaveRMA(RmaRequestDto planDto)
        {
            var userId = _currentUserService.UserId;
            var savedRMAResult = await _rmaService.SaveRMAsync(planDto, userId);

            if (savedRMAResult)
            {
                return StandardAPIResponse<bool>.SuccessResponse(savedRMAResult, !string.IsNullOrEmpty(planDto.Id) ? _localizer[MessageKeys.RecordUpdated] : _localizer[MessageKeys.RecordAdded]);
            }
            return StandardAPIResponse<bool>.ErrorResponse(savedRMAResult, "", StatusCodes.Status200OK);
        }


        [HttpPost]
        [Route("DeleteRMA")]
        [CustomAuthorize([ScreenNames.DeleteRMA])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteRMA(DeleteRMADto dto)
        {
            var userId = _currentUserService.UserId;
            var result = await _rmaService.DeleteRMAAsync(dto.Id, userId);

            if (result)
            {
                return StandardAPIResponse<bool>.SuccessResponse(result, _localizer[MessageKeys.RecordDeleted]);
            }
            return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status200OK);
        }
        [HttpPost]
        [Route("CamerasInRMAWidget")]
        [CustomAuthorize([ScreenNames.CamerasInMaintenance], ScreenNames.Maintenance)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<RMAWidgetResponse>>>> GetMaintenanceModeWidgetData(WidgetRequest widgetRequest)
        {
            widgetRequest.UserId = _currentUserService.UserId;
            var result = await _rmaService.GetRmaMaintenanceCount(widgetRequest);

            if (result.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<RMAWidgetResponse>>.SuccessResponse(result, "");
            }
            return StandardAPIResponse<IEnumerable<RMAWidgetResponse>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ExportCamerasInRMAWidgetCSV/csv")]
        [CustomAuthorize([ScreenNames.CamerasInMaintenance], ScreenNames.Maintenance)]
        public async Task<IActionResult> ExportCamerasInRMAWidgetCSV(WidgetRequest widgetRequest)
        {
            widgetRequest.UserId = _currentUserService.UserId;
            var csvBuilder = await _rmaService.DownloadRmaMaintenanceCSV(widgetRequest);

            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("ExportRMACSV")]
        [CustomAuthorize([ScreenNames.ViewRMA])]
        public async Task<IActionResult> ExportMaintenanceScheduleCSV(RMASerachModel rmaSerachModel)
        {
            var userId = _currentUserService.UserId;
            var csvBuilder = await _rmaService.ExportRMACSV(rmaSerachModel, userId);
            if (csvBuilder == null || csvBuilder.Length == 0)
            {
                return NoContent(); // HTTP 204
            }
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"RMA.csv");
        }
    }
}
