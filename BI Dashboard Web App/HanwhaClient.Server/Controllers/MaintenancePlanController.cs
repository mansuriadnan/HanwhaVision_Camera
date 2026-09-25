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

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize([ScreenNames.MaintenanceMaster])]
    public class MaintenancePlanController : ControllerBase
    {
        private readonly IMaintenancePlanService _maintenancePlanService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<AppMessages> _localizer;

        public MaintenancePlanController(IMaintenancePlanService service, ICurrentUserService currentUserService, IStringLocalizer<AppMessages> localizer)
        {
            _maintenancePlanService = service;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        [HttpPost]
        [Route("GetMaintenancePlan")]
        [CustomAuthorize([ScreenNames.ViewMaintenancePlan])]
        public async Task<ActionResult<StandardAPIResponse<PagedResult<MaintenancePlanDto>>>>
        GetAllMaintenancePlan([FromBody] MaintenancePlanSerachModel query)
        {
            var result = await _maintenancePlanService.GetMaintenancePlansAsync(query);

            if (result != null && result.Items.Any())
                return StandardAPIResponse<PagedResult<MaintenancePlanDto>>
                    .SuccessResponse(result, "");

            return StandardAPIResponse<PagedResult<MaintenancePlanDto>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status200OK);
        }

        [HttpPost]
        [CustomAuthorize([ScreenNames.AddOrUpdateMaintenancePlan])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> SaveMaintenancePlan(MaintenancePlanDto planDto)
        {
            var userId = _currentUserService.UserId;
            var savedPlanResult = await _maintenancePlanService.SaveMaintenancePlanAsync(planDto, userId);

            if (savedPlanResult.result)
            {
                return StandardAPIResponse<bool>.SuccessResponse(savedPlanResult.result, !string.IsNullOrEmpty(planDto.Id) ? _localizer[MessageKeys.RecordUpdated] : _localizer[MessageKeys.RecordAdded]);
            }
            return StandardAPIResponse<bool>.ErrorResponse(savedPlanResult.result, savedPlanResult.errorMessage, StatusCodes.Status400BadRequest);
        }

        [HttpPost]
        [Route("DeleteMaintenancePlan")]
        [CustomAuthorize([ScreenNames.DeleteMaintenancePlan])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteMaintenancePlan(DeleteMaintenancePlanDto dto)
        {
            var userId = _currentUserService.UserId;
            var result = await _maintenancePlanService.DeleteMaintenanceAsync(dto.Id, userId);

            if (result)
            {
                return StandardAPIResponse<bool>.SuccessResponse(result, _localizer[MessageKeys.RecordDeleted]);
            }
            return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status200OK);
        }

        [HttpGet]
        [Route("GetAllDeviceForMaintenancePlan")]
        [CustomAuthorize([ScreenNames.ViewMaintenancePlan])]
        public async Task<ActionResult<StandardAPIResponse<List<DeviceListByFloorZoneDto>>>> GetAllDeviceForMaintenancePlan()
        {
            var result = await _maintenancePlanService.GetDeviceListByFloorZoneAsync();

            if (result != null)
                return StandardAPIResponse<List<DeviceListByFloorZoneDto>>
                    .SuccessResponse(result, "");

            return StandardAPIResponse<List<DeviceListByFloorZoneDto>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status200OK);
        }
    }
}
