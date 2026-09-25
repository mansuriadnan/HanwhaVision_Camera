using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Threading;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExposeApiController : ControllerBase
    {
        private readonly IFloorService _floorService;
        private readonly IStringLocalizer<AppMessages> _localizer;
        private readonly IZoneService _zoneService;
        private readonly IPeopleWidgetService _peopleWidgetService;
        private readonly IUsersRepository _usersRepository;
        private readonly IWidgetService _widgetService;

        public ExposeApiController(
            IFloorService floorService,
            IStringLocalizer<AppMessages> localizer,
            IZoneService zoneService,
            IPeopleWidgetService peopleWidgetService,
            IUsersRepository usersRepository,
            IWidgetService widgetService)
        {
            _floorService = floorService;
            _localizer = localizer;
            _zoneService = zoneService;
            _peopleWidgetService = peopleWidgetService;
            _usersRepository = usersRepository;
            _widgetService = widgetService;
        }
        [HttpGet]
        [Route("GetAllFloors")]
        //[CustomAuthorize([ScreenNames.ViewListofFloors])]
        [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "viapi")]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<GetFloorDto>>>> GetAllFloorsAsync()
        {
            var data = await _floorService.GetAllFloorsAsync();
            if (data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<GetFloorDto>>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<GetFloorDto>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }
        [HttpGet]
        [Route("GetAllZoneByFloorId/{floorId}")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "viapi")]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<ZoneResponseDto>>>> GetZoneByFloorId(string floorId, bool IncludeMultiServer = false)
        {
            var data = await _zoneService.GetZoneByFloorIdAsync(floorId, IncludeMultiServer);
            if (data.Count() == 0)
            {
                return StandardAPIResponse<IEnumerable<ZoneResponseDto>>.SuccessResponse(data, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status200OK);
            }
            else if (data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<ZoneResponseDto>>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<ZoneResponseDto>>.ErrorResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status200OK);
        }
        [HttpPost]
        [Route("AveragePeopleCountChart")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "viapi")]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<PeopleVehicleInOutAvgChart>>>> AveragePeopleCountChartV2(WidgetRequest widgetRequest, CancellationToken cancellationToken)
        {
            SetUserRole(widgetRequest, cancellationToken);
            var result = await  _peopleWidgetService.PeopleInOutCountAnalysisV2Async(widgetRequest);
            return StandardAPIResponse<IEnumerable<PeopleVehicleInOutAvgChart>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }
        [HttpPost]
        [Route("AverageVehicleCountChart")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "viapi")]
        public async Task<ActionResult<StandardAPIResponse<List<PeopleVehicleInOutAvgChart>>>> AverageVehicleCountChart(WidgetRequestForChart widgetRequest, CancellationToken cancellationToken)
        {
            SetUserRole(widgetRequest, cancellationToken);
            if (widgetRequest != null)
            {
                widgetRequest.FromSummary = "vehicle";
            }
            var result = await _widgetService.AverageVehicleCountChartAsync(widgetRequest);
            return StandardAPIResponse<List<PeopleVehicleInOutAvgChart>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }
        private async void SetUserRole(WidgetRequest widgetRequest, CancellationToken cancellationToken)
        {
             
            widgetRequest.userRoles = new[] { "super admin" }; 
            var user = await _usersRepository.GetUserByUsernameAsync("superadmin",cancellationToken);
            widgetRequest.UserId = user != null ? user.Id : "";
        }
    }
}
