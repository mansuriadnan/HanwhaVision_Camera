using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize([ScreenNames.SsmMaster])]
    public class SSMDashboardController : ControllerBase
    {
        private readonly ISSMDashboardService _sSMDashboardService;
        private readonly IStringLocalizer<AppMessages> _localizer;
        private readonly ICurrentUserService _currentUserService;

        public SSMDashboardController(ISSMDashboardService sSMDashboardService, IStringLocalizer<AppMessages> localizer,
            ICurrentUserService currentUserService)
        {
            _sSMDashboardService = sSMDashboardService;
            _localizer = localizer;
            _currentUserService = currentUserService;
        }

        [HttpPost("ssm-server-hierarchy")]
        [CustomAuthorize([ScreenNames.ViewSsmDashboard])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<SsmServerHierarchyResponse>>>> GetHierarchy([FromBody] SsmHierarchyRequest request)
        {
            var userId = _currentUserService.UserId;
            var result = await _sSMDashboardService.GetSsmServerHierarchyAsync(
                request.ParentSiteIds, request.Date, userId);
            var response = StandardAPIResponse<IEnumerable<SsmServerHierarchyResponse>>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
            return response;
        }

        [HttpPost("ssm-server-availability")]
        [CustomAuthorize([ScreenNames.ViewSsmDashboard])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<SsmServerAvailabilityResponse>>>> GetAvailability([FromBody] SsmServerAvailabilityRequest request)
        {
            var userId = _currentUserService.UserId;
            var result = await _sSMDashboardService.GetServerAvailabilityAsync(request, userId);
            var response = StandardAPIResponse<IEnumerable<SsmServerAvailabilityResponse>>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
            return response;
        }

        [HttpPost("ssm-device-availability")]
        [CustomAuthorize([ScreenNames.ViewSsmDashboard])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<SsmDeviceAvailabilityResponse>>>> GetDeviceAvailability([FromBody] SsmDeviceAvailabilityRequest request)
        {
            var userId = _currentUserService.UserId;
            var result = await _sSMDashboardService.GetSsmDeviceAvailabilityAsync(request, userId);
            var response = StandardAPIResponse<IEnumerable<SsmDeviceAvailabilityResponse>>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
            return response;
        }

        [HttpPost("ssm-server-cpu-utilization")]
        [CustomAuthorize([ScreenNames.ViewSsmDashboard])]
        public async Task<ActionResult<StandardAPIResponse<SsmServerCpuUtilizationResponse>>> GetServerCpuUtilization([FromBody] SsmServerCpuUtilizationRequest request)
        {
            var userId = _currentUserService.UserId;
            var result = await _sSMDashboardService.GetSsmServerCpuUtilizationAsync(request, userId);
            var response = StandardAPIResponse<SsmServerCpuUtilizationResponse>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
            return response;
        }

        [HttpPost("ssm-server-ram-utilization")]
        [CustomAuthorize([ScreenNames.ViewSsmDashboard])]
        public async Task<ActionResult<StandardAPIResponse<SsmServerRamUtilizationResponse>>> GetServerRamUtilization([FromBody] SsmServerRamUtilizationRequest request)
        {
            var userId = _currentUserService.UserId;
            var result = await _sSMDashboardService.GetSsmServerRamUtilizationAsync(request, userId);
            var response = StandardAPIResponse<SsmServerRamUtilizationResponse>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
            return response;
        }
        [HttpPost("ssm-server-device-details")]
        [CustomAuthorize([ScreenNames.ViewSsmDashboard])]
        public async Task<ActionResult<StandardAPIResponse<SSMDeviceDetailsResponse>>> GetSsmDeviceDetails([FromBody] SSMDeviceDetailsRequest request)
        {
            var userId = _currentUserService.UserId;
            var data = await _sSMDashboardService.GetSsmDeviceDetailsAsync(request, userId);

            if (data.SSMDeviceDetails.Count() > 0)
            {
                return StandardAPIResponse<SSMDeviceDetailsResponse>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<SSMDeviceDetailsResponse>.SuccessResponse(null, AppMessageConstants.RecordNotFound, StatusCodes.Status404NotFound);
        }

        [HttpPost("ssm-server-health-report")]
        [CustomAuthorize([ScreenNames.ViewSsmReport])]
        public async Task<ActionResult<StandardAPIResponse<List<SsmServerHealthReportResponse>>>> GetSsmServerHealthReport([FromBody] SsmServerHealthReportRequest request)
        {
            var userId = _currentUserService.UserId;
            var data = await _sSMDashboardService.GetSsmServerHealthReportAsync(request, userId);

            if (data != null && data.Count > 0)
            {
                return StandardAPIResponse<List<SsmServerHealthReportResponse>>.SuccessResponse(data, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<List<SsmServerHealthReportResponse>>.SuccessResponse(new List<SsmServerHealthReportResponse>(), AppMessageConstants.RecordNotFound, StatusCodes.Status404NotFound);
        }
    }
}
