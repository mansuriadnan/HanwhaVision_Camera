using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.PeopleWidget;
using Microsoft.AspNetCore.Mvc;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize([ScreenNames.PeopleCountByGender], ScreenNames.People)]
    public class PeopleWidgetController : ControllerBase
    {
        private readonly IPeopleWidgetService _peopleWidgetService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService _currentUserService;

        public PeopleWidgetController(
            IPeopleWidgetService peopleWidgetService,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserService currentUserService)
        {
            _peopleWidgetService = peopleWidgetService;
            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        [Route("GenderWisePeopleCounting")]
        [CustomAuthorize([ScreenNames.PeopleCountByGender], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<GenderWisePeopleCounting>>>> GenderWisePeopleCounting(WidgetRequest widgetRequest)
        {
            var result = await _peopleWidgetService.GenderWisePeopleCounting(widgetRequest);
            return StandardAPIResponse<IEnumerable<GenderWisePeopleCounting>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("PeopleCameraCapacityUtilizationByZones")]
        [CustomAuthorize([ScreenNames.ZoneWiseCapacityUtilizationForPeople], ScreenNames.Site)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<CameraCapacityUtilizationByZones>>>> PeopleCameraCapacityUtilizationByZones(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _peopleWidgetService.PeopleCameraCapacityUtilizationByZoneAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<CameraCapacityUtilizationByZones>>.SuccessResponse(result.Item1, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("PeopleCameraCapacityUtilizationAnalysisByZones")]
        [CustomAuthorize([ScreenNames.ZoneWiseCapacityUtilizationForPeople], ScreenNames.Site)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<CameraCapacityUtilizationAnalysisByZones>>>> PeopleCameraCapacityUtilizationAnalysisByZones(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var userId = _currentUserService.UserId;
            var result = await _peopleWidgetService.PeopleCameraCapacityUtilizationAnalysisByZones(widgetRequest);
            return StandardAPIResponse<IEnumerable<CameraCapacityUtilizationAnalysisByZones>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }


        [HttpPost]
        [Route("PeopleCapacityUtilization")]
        [CustomAuthorize([ScreenNames.CapacityUtilizationForPeople], ScreenNames.Site)]
        public async Task<ActionResult<StandardAPIResponse<CapacityUtilization>>> PeopleCapacityUtilization(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var result = await _peopleWidgetService.PeopleCapacityUtilizationAsync(widgetRequest);
            return StandardAPIResponse<CapacityUtilization>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("NewVsTotalVisitorCount")]
        [CustomAuthorize([ScreenNames.NewVsTotalVisiotr], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<NewVsTotalVisitorCountWidget>>> NewVsTotalVisitorCount(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _peopleWidgetService.NewVsTotalVisitorCountAsync(widgetRequest);
            return StandardAPIResponse<NewVsTotalVisitorCountWidget>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("PeopleCountByZones")]
        [CustomAuthorize([ScreenNames.ZoneWisePeopleCounting], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<PeopleCountByZones>>>> PeopleCountByZones(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _peopleWidgetService.PeopleCountByZones(widgetRequest);
            return StandardAPIResponse<IEnumerable<PeopleCountByZones>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("GenderWisePeopleCountAnalysis")]
        [CustomAuthorize([ScreenNames.PeopleCountByGender], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<GenderWisePeopleAnalysisCount>>>> GenderWisePeopleCountAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _peopleWidgetService.GenderWisePeopleCountAnalysisData(widgetRequest);
            return StandardAPIResponse<IEnumerable<GenderWisePeopleAnalysisCount>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("AgeWisePeopleCountAnalysis")]
        [CustomAuthorize([ScreenNames.PeopleCountByAge], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<AgeWisePeopleAnalysisCount>>>> AgeWisePeopleCountAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _peopleWidgetService.AgeWisePeopleCountAnalysisData(widgetRequest);
            return StandardAPIResponse<IEnumerable<AgeWisePeopleAnalysisCount>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("PeopleInOutTotal")]
        [CustomAuthorize([ScreenNames.PeopleInOut], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<PeopleVehicleInOutTotal>>> PeopleInOutTotal(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            //var result = await _widgetService.PeopleIOnOutTotalAsync(widgetRequest);
            var result = await _peopleWidgetService.PeopleIOnOutTotalV2Async(widgetRequest);
            return StandardAPIResponse<PeopleVehicleInOutTotal>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("AveragePeopleCountChart")]
        [CustomAuthorize([ScreenNames.AveragePeopleCounting], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<PeopleVehicleInOutAvgChart>>>> AveragePeopleCountChartV2(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _peopleWidgetService.PeopleInOutCountAnalysisV2Async(widgetRequest);
            return StandardAPIResponse<IEnumerable<PeopleVehicleInOutAvgChart>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("NewVsTotalVisitorChart")]
        [CustomAuthorize([ScreenNames.NewVsTotalVisiotr], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<List<ChartAvgInOut>>>> NewVsTotalVisitorChart(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _peopleWidgetService.NewVsTotalVisitorChartAsync(widgetRequest);
            return StandardAPIResponse<List<ChartAvgInOut>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }


        private void SetUserRole(WidgetRequest widgetRequest)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            widgetRequest.userRoles = user?.Claims.Where(x => x.Type == "role").Select(y => y.Value);
            widgetRequest.UserId = _currentUserService.UserId;
        }
    }
}
