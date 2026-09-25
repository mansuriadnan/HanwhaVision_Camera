using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize([ScreenNames.MonitoringMaster])]
    public class MonitoringController : ControllerBase
    {
        private readonly IMonitoringService _monitoringService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<AppMessages> _localizer;


        public MonitoringController(IMonitoringService monitoringService, ICurrentUserService currentUserService, IStringLocalizer<AppMessages> localizer)
        {
            _monitoringService = monitoringService;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        [HttpGet]
        [Route("GetMonitoring")]
        [CustomAuthorize([ScreenNames.ViewListofMonitorings])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<MonitoringResponseModel>>>> GetMonitoring()
        {
            var result = await _monitoringService.GetMonitoringAsync();
            return StandardAPIResponse<IEnumerable<MonitoringResponseModel>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpGet]
        [Route("GetMonitoringGroupAndItem")]
        [CustomAuthorize([ScreenNames.ViewListofMonitoringGroups])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<MonitoringGroupWithItemsResponse>>>> GetMonitoringGroupAndItem(string monitoringId)
        {
            var result = await _monitoringService.GetAllMonitoringGroupsAsync(monitoringId);
            return StandardAPIResponse<IEnumerable<MonitoringGroupWithItemsResponse>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("AddUpdateMonitoring")]
        [CustomAuthorize([ScreenNames.AddOrUpdateMonitoring])]
        public async Task<ActionResult<StandardAPIResponse<string>>> AddUpdateMonitoring(MonitoringRequestModel model)
        {
            var userId = _currentUserService.UserId;
            var data = await _monitoringService.AddUpdateMonitoringAsync(model, userId);

            if (data.Contains("already") || data.Contains("not found"))
            {
                return StandardAPIResponse<string>.ErrorResponse(null, data, StatusCodes.Status400BadRequest);
            }
            return StandardAPIResponse<string>.SuccessResponse(data, string.IsNullOrEmpty(model.MonitoringId) ? _localizer[MessageKeys.RecordAdded] : _localizer[MessageKeys.RecordUpdated]);
        }

        [HttpPost]
        [Route("DeleteMonitoring")]
        [CustomAuthorize([ScreenNames.DeleteMonitoring])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteMonitoring(MonitoringGroupReqDto dto)
        {
            var userId = _currentUserService.UserId;
            var data = await _monitoringService.DeleteMonitoringAsync(dto.MonitoringId, userId);
            if (!data)
            {
                return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
            }
            return StandardAPIResponse<bool>.SuccessResponse(true, _localizer[MessageKeys.RecordDeleted], StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("AddUpdateGroup")]
        [CustomAuthorize([ScreenNames.AddOrUpdateGroup])]
        public async Task<ActionResult<StandardAPIResponse<string>>> AddUpdateGroup(MonitoringGroupRequest model)
        {
            var userId = _currentUserService.UserId;
            var data = await _monitoringService.AddUpdateMonitoringGroupAsync(model, userId);
            if (data.Contains("not found") || data.Contains("required"))
            {
                return StandardAPIResponse<string>.ErrorResponse(null, data, StatusCodes.Status400BadRequest);
            }
            return StandardAPIResponse<string>.SuccessResponse(data, string.IsNullOrEmpty(model.GroupId) ? _localizer[MessageKeys.RecordAdded] : _localizer[MessageKeys.RecordUpdated]);
        }

        [HttpPost]
        [Route("AddUpdateGroupItem")]
        [CustomAuthorize([ScreenNames.AddOrUpdateURLPreview])]
        public async Task<ActionResult<StandardAPIResponse<string>>> AddUpdateGroupItem(MonitoringGroupItemRequest model)
        {
            var userId = _currentUserService.UserId;
            var data = await _monitoringService.AddUpdateMonitoringGroupItemAsync(model, userId);
            if (data.Contains("not found") || data.Contains("required"))
            {
                return StandardAPIResponse<string>.ErrorResponse(null, "", StatusCodes.Status400BadRequest);
            }
            return StandardAPIResponse<string>.SuccessResponse(data,  string.IsNullOrEmpty(model.GroupId) ? _localizer[MessageKeys.RecordAdded] : _localizer[MessageKeys.RecordUpdated]);
        }

        [HttpPost]
        [Route("DeleteMonitoringGroup")]
        [CustomAuthorize([ScreenNames.DeleteMonitoringGroup])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteMonitoringGroup(MonitoringGroupReqDto dto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                var data = await _monitoringService.DeleteMonitoringGroupAsync(dto.MonitoringId,dto.MonitoringGroupId);
                if (data.Contains("not found") || data.Contains("required"))
                {
                    return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
                }
                return StandardAPIResponse<bool>.SuccessResponse(true, _localizer[MessageKeys.RecordDeleted], StatusCodes.Status200OK);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("DeleteMonitoringGroupItemSite")]
        [CustomAuthorize([ScreenNames.DeleteURLPreviewMonitoring])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteMonitoringGroupItemSite(MonitoringGroupReqDto dto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                var data = await _monitoringService.DeleteMonitoringGroupItemAsync(dto.MonitoringId, dto.MonitoringGroupId, dto.MonitoringGroupItemId);
                if (data.Contains("not found") || data.Contains("required"))
                {
                    return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
                }
                return StandardAPIResponse<bool>.SuccessResponse(true, _localizer[MessageKeys.RecordDeleted], StatusCodes.Status200OK);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

    }
}