using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.PeopleWidget;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WidgetController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IWidgetService _widgetService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDeviceEventsRepository _deviceEventsRepository;
        private readonly IQueueManagementRepository _queueManagementRepository;
        private readonly IShoppingCartCountRepository _shoppingCartCountRepository;
        private readonly IForkliftCountRepository _forkliftCountRepository;
        private readonly IArchiveTimeService _archiveTimeService;
        private readonly IANPRVehicleService _aNPRVehicleService;

        public WidgetController(ICurrentUserService currentUserService,
            IWidgetService widgetService,
            IHttpContextAccessor httpContextAccessor,
            IDeviceEventsRepository deviceEventsRepository,
            IQueueManagementRepository queueManagementRepository,
            IShoppingCartCountRepository shoppingCartCountRepository,
            IForkliftCountRepository forkliftCountRepository,
            IArchiveTimeService archiveTimeService,
            IANPRVehicleService aNPRVehicleService)
        {
            _currentUserService = currentUserService;
            _widgetService = widgetService;
            _httpContextAccessor = httpContextAccessor;
            _deviceEventsRepository = deviceEventsRepository;
            _queueManagementRepository = queueManagementRepository;
            _shoppingCartCountRepository = shoppingCartCountRepository;
            _forkliftCountRepository = forkliftCountRepository;
            _archiveTimeService = archiveTimeService;
            _aNPRVehicleService = aNPRVehicleService;
        }

        [HttpPost]
        [Route("TotalCameraCount")]
        [CustomAuthorize([ScreenNames.CameraOnlineOffline], ScreenNames.Camera)]
        public async Task<ActionResult<StandardAPIResponse<CameraCountResponse>>> GetTotalCameraCount(WidgetRequest widgetRequest)
        {
            var result = await _widgetService.GetTotalCameraCountAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
            return StandardAPIResponse<CameraCountResponse>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("CameraCountByModel")]
        [CustomAuthorize([ScreenNames.ModalTypes], ScreenNames.Camera)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<CameraSeriesCountResponse>>>> CameraCountByModel(WidgetRequest widgetRequest)
        {
            var result = await _widgetService.CameraCountByModelAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
            return StandardAPIResponse<IEnumerable<CameraSeriesCountResponse>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("CameraCountByFeatures")]
        [CustomAuthorize([ScreenNames.FeatureTypes], ScreenNames.Camera)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<CameraFeaturesCountResponse>>>> CameraCountByFeatures(WidgetRequest widgetRequest)
        {
            var result = await _widgetService.CameraCountByFeaturesAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
            return StandardAPIResponse<IEnumerable<CameraFeaturesCountResponse>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("CameraDisconnectedTracker")]
        [CustomAuthorize([ScreenNames.CameraDisconnectionTracker], ScreenNames.Camera)]
        public async Task<ActionResult<StandardAPIResponse<CameraDisconnectedTrackerResponse>>> CameraDisconnectedTracker(WidgetRequest widgetRequest)
        {
            var result = await _widgetService.CameraDisconnectedTrackerAsync(widgetRequest);
            return StandardAPIResponse<CameraDisconnectedTrackerResponse>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("CameraDisconnectedTrackerAnalysis")]
        [CustomAuthorize([ScreenNames.CameraDisconnectionTracker], ScreenNames.Camera)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<CameraDisconnectedTrackerAnalsisResponse>>>> CameraDisconnectedTrackerAnalysis(WidgetRequest widgetRequest)
        {
            var result = await _widgetService.CameraDisconnectedTrackerAnalysisAsync(widgetRequest);
            var response = StandardAPIResponse<IEnumerable<CameraDisconnectedTrackerAnalsisResponse>>.SuccessResponse(result.trackerAnalsisRes, "", StatusCodes.Status200OK, ReferenceData: result.referenceData);
            return response;
        }


        [HttpPost]
        [Route("VehicleCameraCapacityUtilizationByZones")]
        [CustomAuthorize([ScreenNames.ZoneWiseCapacityUtilizationForVehicle], ScreenNames.Site)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<CameraCapacityUtilizationByZones>>>> VehicleCameraCapacityUtilizationByZones(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleCameraCapacityUtilizationByZoneAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<CameraCapacityUtilizationByZones>>.SuccessResponse(result.Item1, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleCapacityUtilization")]
        [CustomAuthorize([ScreenNames.CapacityUtilizationForVehicle], ScreenNames.Site)]
        public async Task<ActionResult<StandardAPIResponse<CapacityUtilization>>> VehicleCapacityUtilization(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleCapacityUtilizationAsync(widgetRequest);
            return StandardAPIResponse<CapacityUtilization>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleCameraCapacityUtilizationAnalysisByZones")]
        [CustomAuthorize([ScreenNames.ZoneWiseCapacityUtilizationForVehicle], ScreenNames.Site)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<CameraCapacityUtilizationAnalysisByZones>>>> VehicleCameraCapacityUtilizationAnalysisByZones(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleCameraCapacityUtilizationAnalysisByZones(widgetRequest);
            return StandardAPIResponse<IEnumerable<CameraCapacityUtilizationAnalysisByZones>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("AveragePeopleCount")]
        [CustomAuthorize([ScreenNames.AveragePeopleCounting], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<InOutPeopleCountAverageWidgetDto>>> AveragePeopleCount(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var result = await _widgetService.AveragePeopleCountAsync(widgetRequest);
            return StandardAPIResponse<InOutPeopleCountAverageWidgetDto>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("AverageVehicleCount")]
        [CustomAuthorize([ScreenNames.AverageVehicleCounting], ScreenNames.Vehicle)]
        public async Task<ActionResult<StandardAPIResponse<InOutVehicleCountAverageWidgetDto>>> AverageVehicleCount(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var result = await _widgetService.AverageVehicleCountAsync(widgetRequest);
            return StandardAPIResponse<InOutVehicleCountAverageWidgetDto>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ANPRVehicleParking")]
        [CustomAuthorize([ScreenNames.ANPRParking], ScreenNames.ANPR)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<ANPRParkingResponse>>>> ANPRVehicleParkingWidgetData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _aNPRVehicleService.ANPRVehicleParkingCountAsync(widgetRequest);
            if (result.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<ANPRParkingResponse>>.SuccessResponse(result, "");
            }
            return StandardAPIResponse<IEnumerable<ANPRParkingResponse>>.SuccessResponse(null, MessageKeys.RecordNotFound, StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ANPRVehicleParkingByZones")]
        [CustomAuthorize([ScreenNames.ANPRParking], ScreenNames.ANPR)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<VehicleParking>>>> ANPRVehicleParkingByZones(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _aNPRVehicleService.ANPRVehicleParkingCountByZonesAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<VehicleParking>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleByTypeCount")]
        [CustomAuthorize([ScreenNames.VehicleCountByType], ScreenNames.Vehicle)]
        public async Task<ActionResult<StandardAPIResponse<VehicleByTypeCountWidgetDto>>> VehicleByTypeCount(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleByTypeCountAsync(widgetRequest);
            return StandardAPIResponse<VehicleByTypeCountWidgetDto>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        //[HttpPost]
        //[Route("PeopleInOutTotal")]
        //[CustomAuthorize([ScreenNames.PeopleInOut], ScreenNames.People)]
        //public async Task<ActionResult<StandardAPIResponse<PeopleVehicleInOutTotal>>> PeopleInOutTotal(WidgetRequest widgetRequest)
        //{
        //    SetUserRole(widgetRequest);
        //    //var result = await _widgetService.PeopleIOnOutTotalAsync(widgetRequest);
        //    var result = await _widgetService.PeopleIOnOutTotalV2Async(widgetRequest);
        //    return StandardAPIResponse<PeopleVehicleInOutTotal>.SuccessResponse(result, "", StatusCodes.Status200OK);
        //}

        [HttpPost]
        [Route("VehicleInOutTotal")]
        [CustomAuthorize([ScreenNames.VehicleInOut], ScreenNames.Vehicle)]
        public async Task<ActionResult<StandardAPIResponse<PeopleVehicleInOutTotal>>> VehicleInOutTotal(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var result = await _widgetService.VehicleIOnOutTotalAsync(widgetRequest);
            return StandardAPIResponse<PeopleVehicleInOutTotal>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("PeopleInOutChart")]
        [CustomAuthorize([ScreenNames.PeopleInOut], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<PeopleVehicleInOutChartResponse>>> PeopleInOutChart(WidgetRequestForChart widgetRequest)
        {
            SetUserRole(widgetRequest);
            if (widgetRequest != null)
            {
                widgetRequest.FromSummary = "people";
            }
            var result = await _widgetService.PeopleIOnOutChartAsync(widgetRequest);
            return StandardAPIResponse<PeopleVehicleInOutChartResponse>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }


        [HttpPost]
        [Route("VehicleQueueAnalysis")]
        [CustomAuthorize([ScreenNames.VehicleQueueAnalysis], ScreenNames.Traffic)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> VehicleQueueAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleQueueAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleInOutChart")]
        [CustomAuthorize([ScreenNames.VehicleInOut], ScreenNames.Vehicle)]
        public async Task<ActionResult<StandardAPIResponse<PeopleVehicleInOutChartResponse>>> VehicleInOutChart(WidgetRequestForChart widgetRequest)
        {
            SetUserRole(widgetRequest);
            if (widgetRequest != null)
            {
                widgetRequest.FromSummary = "vehicle";
            }
            var result = await _widgetService.VehicleIOnOutChartAsync(widgetRequest);
            return StandardAPIResponse<PeopleVehicleInOutChartResponse>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ForkliftQueueAnalysis")]
        [CustomAuthorize([ScreenNames.QueueEventsForForklift], ScreenNames.Factory)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> ForkliftQueueAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.ForkliftQueueAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ShoppingCartQueueAnalysis")]
        [CustomAuthorize([ScreenNames.QueueEventForShopingCart], ScreenNames.Retail)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> ShoppingCartQueueAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.ShoppingCartQueueAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("PeopleQueueAnalysis")]
        [CustomAuthorize([ScreenNames.QueueEventForPeple], ScreenNames.Retail)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> PeopleQueueAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.PeopleQueueAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("PedestrianAnalysis")]
        [CustomAuthorize([ScreenNames.PedestrianDetection], ScreenNames.Traffic)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> PedestrianQueueAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.PedestrianQueueAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleParkingAnalysis")]
        [CustomAuthorize([ScreenNames.Parking], ScreenNames.Vehicle)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> VehicleParkingAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleParkingAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleParkingByZones")]
        [CustomAuthorize([ScreenNames.Parking], ScreenNames.Vehicle)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<VehicleParking>>>> VehicleParkingByZones(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleParkingByZonesAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<VehicleParking>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ProxomityDetectionAnalysis")]
        [CustomAuthorize([ScreenNames.DetectForklift], ScreenNames.Factory)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> ProxomityDetectionAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.ProxomityDetectionAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }


        [HttpPost]
        [Route("StoppedVehicleByTypeAnalysis")]
        [CustomAuthorize([ScreenNames.StoppedVehicleCountTime], ScreenNames.Traffic)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<StoppedVehicleByTypeData>>>> StoppedVehicleByTypeAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.StoppedVehicleByTypeAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<StoppedVehicleByTypeData>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleSpeedViolationAnalysis")]
        [CustomAuthorize([ScreenNames.SpeedViolationByVehicle], ScreenNames.Traffic)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> VehicleSpeedViolationAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleSpeedViolationAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("TrafficJamAnalysis")]
        [CustomAuthorize([ScreenNames.TrafficJamByDay], ScreenNames.Traffic)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> TrafficJamAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.TrafficJamAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("SlipFallAnalysis")]
        [CustomAuthorize([ScreenNames.SlipAndFallDetection], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> SlipFallQueueAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.SlipFallQueueAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("MaskDetectionAnalysis")]
        [CustomAuthorize([ScreenNames.SafetyMeasure], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<MaskDetectionAnalysis>>>> MaskDetectionAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.MaskDetectionAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<MaskDetectionAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("WrongWayAnalysis")]
        [CustomAuthorize([ScreenNames.VehicleInWrongDirection], ScreenNames.Traffic)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> WrongWayQueueAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.WrongWayQueueAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("BlockedExitAnalysis")]
        [CustomAuthorize([ScreenNames.BlockedExitDetection, ScreenNames.BlockedExitDetectionFactory], ScreenNames.Retail)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> BlockedExitAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.BlockedExitAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleUTurnAnalysis")]
        [CustomAuthorize([ScreenNames.VehicleUTurnDetection], ScreenNames.Traffic)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> VehicleUTurnAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleUTurnAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ShoppingCartCountAnalysis")]
        [CustomAuthorize([ScreenNames.ShoppingCartCounting], ScreenNames.Retail)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> ShoppingCartCountAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.ShoppingCartCountAnalysisData(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ForkliftCountAnalysis")]
        [CustomAuthorize([ScreenNames.CountingForForklift], ScreenNames.Factory)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> ForkliftCountAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.ForkliftCountAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ForkliftSpeedDetectionAnalysis")]
        [CustomAuthorize([ScreenNames.ForkliftSpeedDetection], ScreenNames.Factory)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> ForkliftSpeedDetectionAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.ForkliftSpeedDetectionAnalysisDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }



        [HttpPost]
        [Route("VehicleTurningMovementAnalysis")]
        [CustomAuthorize([ScreenNames.VehicleTurningMovementCounts], ScreenNames.Traffic)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<VehicleTurningMovementResponse>>>> VehicleTurningMovementAnalysisData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleTurningMovementAnalysisData(widgetRequest);
            return StandardAPIResponse<IEnumerable<VehicleTurningMovementResponse>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        //[HttpPost]
        //[Route("AveragePeopleCountChart")]
        //[CustomAuthorize([ScreenNames.AveragePeopleCounting], ScreenNames.People)]
        //public async Task<ActionResult<StandardAPIResponse<List<PeopleVehicleInOutAvgChart>>>> AveragePeopleCountChart(WidgetRequest widgetRequest)
        //{
        //    SetUserRole(widgetRequest);
        //    if (widgetRequest != null)
        //    {
        //        widgetRequest.FromSummary = "people";
        //        widgetRequest.IntervalMinute = 10;
        //    }

        //    var result = await _widgetService.AveragePeopleCountChartAsync(widgetRequest);
        //    return StandardAPIResponse<List<PeopleVehicleInOutAvgChart>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        //}

        //[HttpPost]
        //[Route("AveragePeopleCountChart")]
        //[CustomAuthorize([ScreenNames.AveragePeopleCounting], ScreenNames.People)]
        //public async Task<ActionResult<StandardAPIResponse<IEnumerable<PeopleVehicleInOutAvgChart>>>> AveragePeopleCountChartV2(WidgetRequest widgetRequest)
        //{
        //    SetUserRole(widgetRequest);
        //    var result = await _widgetService.PeopleInOutCountAnalysisV2Async(widgetRequest);
        //    return StandardAPIResponse<IEnumerable<PeopleVehicleInOutAvgChart>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        //}

        [HttpPost]
        [Route("AveragePeopleCountChartOriginal")]
        [CustomAuthorize([ScreenNames.AveragePeopleCounting], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<List<PeopleVehicleInOutAvgChart>>>> AveragePeopleCountChart(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.PeopleInOutCountAnalysisAsync(widgetRequest);
            return StandardAPIResponse<List<PeopleVehicleInOutAvgChart>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("NewVsTotalVisitorChart")]
        [CustomAuthorize([ScreenNames.NewVsTotalVisiotr], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<List<ChartAvgInOut>>>> NewVsTotalVisitorChart(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.NewVsTotalVisitorChartAsync(widgetRequest);
            return StandardAPIResponse<List<ChartAvgInOut>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleByTypeLineChartData")]
        [CustomAuthorize([ScreenNames.VehicleCountByType], ScreenNames.Vehicle)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<VehicleByTypeChartResponse>>>> VehicleByTypeLineChartData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleByTypeLineChartData(widgetRequest);
            return StandardAPIResponse<IEnumerable<VehicleByTypeChartResponse>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        //[HttpPost]
        //[Route("GetAllDeviceData")]
        //public async Task<ActionResult<StandardAPIResponse<IEnumerable<DeviceData>>>> GetAllDeviceByZone(WidgetRequestDevice widgetRequestDevice)
        //{
        //    var user = _httpContextAccessor.HttpContext?.User;
        //    widgetRequestDevice.userRoles = user?.Claims.Where(x => x.Type == "role").Select(y => y.Value);
        //    var result = await _widgetService.GetDeviceByZone(widgetRequestDevice);

        //    return StandardAPIResponse<IEnumerable<DeviceData>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        //}


        [HttpPost]
        [Route("AverageVehicleCountChart")]
        [CustomAuthorize([ScreenNames.AverageVehicleCounting], ScreenNames.Vehicle)]
        public async Task<ActionResult<StandardAPIResponse<List<PeopleVehicleInOutAvgChart>>>> AverageVehicleCountChart(WidgetRequestForChart widgetRequest)
        {
            SetUserRole(widgetRequest);
            if (widgetRequest != null)
            {
                widgetRequest.FromSummary = "vehicle";
            }
            //if (widgetRequest != null && widgetRequest.IntervalMinutes > 0)
            //{
            //    widgetRequest.AddMinutes = widgetRequest.IntervalMinutes;
            //}
            var result = await _widgetService.AverageVehicleCountChartAsync(widgetRequest);
            return StandardAPIResponse<List<PeopleVehicleInOutAvgChart>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }


        [HttpPost]
        [Route("SpeedDetectionByVehicle")]
        [CustomAuthorize([ScreenNames.SpeedViolationByVehicle], ScreenNames.Traffic)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> SpeedDetectionByVehicleData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.SpeedDetectionByVehicleDataAsync(widgetRequest);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("HeatMapWidgetData")]
        [CustomAuthorize([ScreenNames.PeopleCountingHeatmap, ScreenNames.VehicleDetectionHeatmap, ScreenNames.ShopingCartHeatmap, ScreenNames.ForkliftHeatmap], ScreenNames.Vehicle)] //Which widget belogs to this method?
        public async Task<ActionResult<StandardAPIResponse<HeatmapWidgetResponse>>> HeatMapWidgetData(WidgetHeatmapRequest widgetRequest)
        {
            var result = await _widgetService.HeatMapWidgetDataAsync(widgetRequest);
            return StandardAPIResponse<HeatmapWidgetResponse>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }


        #region Camera Csv 

        [HttpPost]
        [Route("TotalCameraCount/csv")]
        [CustomAuthorize([ScreenNames.CameraOnlineOffline], ScreenNames.Camera)]
        public async Task<IActionResult> GetTotalCameraCountCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.TotalCameraCountCsv(widgetRequest);

            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");

        }

        [HttpPost]
        [Route("CameraCountByModel/csv")]
        [CustomAuthorize([ScreenNames.ModalTypes], ScreenNames.Camera)]
        public async Task<IActionResult> CameraCountByModelCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.CameraCountByModelCsv(widgetRequest);

            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("CameraCountByFeatures/csv")]
        [CustomAuthorize([ScreenNames.FeatureTypes], ScreenNames.Camera)]
        public async Task<IActionResult> CameraCountByFeaturesCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var csvBuilder = await _widgetService.CameraCountByFeaturesCsv(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        #endregion

        #region Site Csv

        [HttpPost]
        [Route("PeopleCapacityUtilization/csv")]
        [CustomAuthorize([ScreenNames.CapacityUtilizationForPeople], ScreenNames.Site)]
        public async Task<IActionResult> PeopleCapacityUtilizationCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            //widgetRequest.IntervalMinute = 10;

            var csvBuilder = await _widgetService.PeopleCapacityUtilizationCsv(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("VehicleCapacityUtilization/csv")]
        [CustomAuthorize([ScreenNames.CapacityUtilizationForVehicle], ScreenNames.Site)]
        public async Task<IActionResult> VehicleCapacityUtilizationCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            //widgetRequest.IntervalMinute = 10;
            var csvBuilder = await _widgetService.VehicleCapacityUtilizationCsv(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("VehicleCameraCapacityUtilizationAnalysisByZones/csv")]
        [CustomAuthorize([ScreenNames.ZoneWiseCapacityUtilizationForVehicle], ScreenNames.Site)]
        public async Task<IActionResult> VehicleCameraCapacityUtilizationByZonesCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var csvBuilder = await _widgetService.VehicleCameraCapacityUtilizationByZonesCsv(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        #endregion

        #region People Csv

        [HttpPost]
        [Route("PeopleCameraCapacityUtilizationAnalysisByZones/csv")]
        [CustomAuthorize([ScreenNames.ZoneWiseCapacityUtilizationForPeople], ScreenNames.Site)]
        public async Task<IActionResult> PeopleCameraCapacityUtilizationByZonesCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var csvBuilder = await _widgetService.PeopleCameraCapacityUtilizationByZonesCsv(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }


        [HttpPost]
        [Route("PeopleInOutCountChart/csv")]
        [CustomAuthorize([ScreenNames.AveragePeopleCounting], ScreenNames.Vehicle)]
        public async Task<IActionResult> DownloadPeopleInOutCountCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            if (widgetRequest != null)
            {
                widgetRequest.FromSummary = "people";
            }

            var csvBuilder = await _widgetService.PeopleInOutCountCSVDownload(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("AveragePeopleCountChart/csv")]
        [CustomAuthorize([ScreenNames.AveragePeopleCounting], ScreenNames.Vehicle)]
        public async Task<IActionResult> DownloadAveragePeopleCountCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            if (widgetRequest != null)
            {
                widgetRequest.FromSummary = "people";
                widgetRequest.AverageIntervalMinute = widgetRequest.IntervalMinute;
                widgetRequest.IntervalMinute = 10;
            }

            var csvBuilder = await _widgetService.AveragePeopleCountCSVDownload(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }


        [HttpPost]
        [Route("CumulativePeopleCountChart/csv")]
        [CustomAuthorize([ScreenNames.CumulativePeopleCount], ScreenNames.Vehicle)]
        public async Task<IActionResult> CumulativePeopleCountCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            if (widgetRequest != null)
            {
                widgetRequest.FromSummary = "people";
                widgetRequest.AverageIntervalMinute = widgetRequest.IntervalMinute;
                widgetRequest.IntervalMinute = 10;
            }

            var csvBuilder = await _widgetService.CumulativePeopleCountCSVDownload(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("PeopleCountByZones/csv")]
        [CustomAuthorize([ScreenNames.ZoneWisePeopleCounting], ScreenNames.People)]
        public async Task<IActionResult> PeopleCountByZonesCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var csvBuilder = await _widgetService.PeopleCountByZonesCsv(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("NewVsTotalVisitorChart/csv")]
        [CustomAuthorize([ScreenNames.NewVsTotalVisiotr], ScreenNames.People)]
        public async Task<ActionResult<StandardAPIResponse<List<ChartAvgInOut>>>> NewVsTotalVisitorCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var csvBuilder = await _widgetService.NewVsTotalVisitorCsv(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("SlipFallAnalysis/csv")]
        [CustomAuthorize([ScreenNames.SlipAndFallDetection], ScreenNames.People)]
        public async Task<IActionResult> SlipFallQueueCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.SlipFallQueueCsvDataAsync(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("GenderWisePeopleCountAnalysis/csv")]
        [CustomAuthorize([ScreenNames.PeopleCountByGender], ScreenNames.People)]
        public async Task<IActionResult> GenderWisePeopleCountAnalysisDataCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.GenderWisePeopleCountAnalysisDataCsv(widgetRequest);
            if (csvBuilder == null || csvBuilder.Length == 0)
            {
                return NoContent(); // HTTP 204
            }
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("AgeWisePeopleCountAnalysis/csv")]
        [CustomAuthorize([ScreenNames.PeopleCountByGender], ScreenNames.People)]
        public async Task<IActionResult> AgeWisePeopleCountAnalysisDataCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.AgeWisePeopleCountAnalysisDataCsv(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        #endregion

        #region Vehicle Csv

        [HttpPost]
        [Route("VehicleInOutCountChart/csv")]
        [CustomAuthorize([ScreenNames.AverageVehicleCounting], ScreenNames.Vehicle)]
        public async Task<IActionResult> DownloadVehicleInOutCountCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            if (widgetRequest != null)
            {
                widgetRequest.FromSummary = "vehicle";
            }
            var csvBuilder = await _widgetService.VehicleInOutCountCSVDownload(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("AverageVehicleCountChart/csv")]
        [CustomAuthorize([ScreenNames.AverageVehicleCounting], ScreenNames.Vehicle)]
        public async Task<IActionResult> DownloadAvgVehicleCountCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            if (widgetRequest != null)
            {
                widgetRequest.FromSummary = "vehicle";
                widgetRequest.AverageIntervalMinute = widgetRequest.IntervalMinute;
                widgetRequest.IntervalMinute = 10;
            }
            var csvBuilder = await _widgetService.AvgVehicleCountCSVDownload(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("VehicleByTypeLineChartData/csv")]
        [CustomAuthorize([ScreenNames.VehicleCountByType], ScreenNames.Vehicle)]
        public async Task<IActionResult> VehicleByTypeLineChartCsv(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var csvBuilder = await _widgetService.VehicleByTypeLineChartCsv(widgetRequest);
            if (csvBuilder == null || csvBuilder.Length == 0)
            {
                return NoContent(); // HTTP 204
            }
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("WrongWayAnalysis/csv")]
        [CustomAuthorize([ScreenNames.VehicleInWrongDirection], ScreenNames.Traffic)]
        public async Task<IActionResult> WrongWayQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.WrongWayQueueAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("VehicleUTurnAnalysis/csv")]
        [CustomAuthorize([ScreenNames.VehicleUTurnDetection], ScreenNames.Traffic)]
        public async Task<IActionResult> VehicleUTurnCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.VehicleUTurnAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("PedestrianAnalysis/csv")]
        [CustomAuthorize([ScreenNames.PedestrianDetection], ScreenNames.Traffic)]
        public async Task<IActionResult> PedestrianQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.PedestrianQueueAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("VehicleQueueAnalysis/csv")]
        [CustomAuthorize([ScreenNames.VehicleQueueAnalysis], ScreenNames.Traffic)]
        public async Task<IActionResult> VehicleQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.VehicleQueueAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("StoppedVehicleByTypeAnalysis/csv")]
        [CustomAuthorize([ScreenNames.StoppedVehicleCountTime], ScreenNames.Traffic)]
        public async Task<IActionResult> StoppedVehicleByTypeAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.StoppedVehicleByTypeAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("VehicleTurningMovementAnalysis/csv")]
        [CustomAuthorize([ScreenNames.VehicleTurningMovementCounts], ScreenNames.Traffic)]
        public async Task<IActionResult> VehicleTurningMovementAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.VehicleTurningMovementAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("VehicleSpeedViolationAnalysis/csv")]
        [CustomAuthorize([ScreenNames.SpeedViolationByVehicle], ScreenNames.Traffic)]
        public async Task<IActionResult> VehicleSpeedViolationAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.VehicleSpeedViolationAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");

        }

        [HttpPost]
        [Route("TrafficJamAnalysis/csv")]
        [CustomAuthorize([ScreenNames.TrafficJamByDay], ScreenNames.Traffic)]
        public async Task<IActionResult> TrafficJamAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.TrafficJamAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");

        }

        #endregion

        #region Retail Csv

        [HttpPost]
        [Route("ShoppingCartQueueAnalysis/csv")]
        [CustomAuthorize([ScreenNames.QueueEventForShopingCart], ScreenNames.Retail)]
        public async Task<IActionResult> ShoppingCartQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var csvBuilder = await _widgetService.ShoppingCartQueueAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("ShoppingCartCountAnalysis/csv")]
        [CustomAuthorize([ScreenNames.ShoppingCartCounting], ScreenNames.Retail)]
        public async Task<IActionResult> ShoppingCartCountAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var csvBuilder = await _widgetService.ShoppingCartCountAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        //Queue Event For People
        [HttpPost]
        [Route("PeopleQueueAnalysis/csv")]
        [CustomAuthorize([ScreenNames.QueueEventForPeple], ScreenNames.Retail)]
        public async Task<IActionResult> PeopleQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var csvBuilder = await _widgetService.PeopleQueueAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }
        //Blocked Exit Detection
        [HttpPost]
        [Route("BlockedExitAnalysis/csv")]
        [CustomAuthorize([ScreenNames.BlockedExitDetection, ScreenNames.BlockedExitDetectionFactory], ScreenNames.Retail)]
        public async Task<IActionResult> BlockedExitAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var csvBuilder = await _widgetService.BlockedExitAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }
        #endregion

        #region Factory Csv

        [HttpPost]
        [Route("ForkliftCountAnalysis/csv")]
        [CustomAuthorize([ScreenNames.CountingForForklift], ScreenNames.Factory)]
        public async Task<IActionResult> ForkliftCountAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.ForkliftCountAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("ForkliftQueueAnalysis/csv")]
        [CustomAuthorize([ScreenNames.QueueEventsForForklift], ScreenNames.Factory)]
        public async Task<IActionResult> ForkliftQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.ForkliftQueueAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }


        [HttpPost]
        [Route("ProxomityDetectionAnalysis/csv")]
        [CustomAuthorize([ScreenNames.QueueEventsForForklift], ScreenNames.Factory)]
        public async Task<IActionResult> ProxomityDetectionAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.ProxomityDetectionAnalysisCsvData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        #endregion

        #region Map Floor Plan

        [HttpPost]
        [Route("PeopleCountingmap")]
        [CustomAuthorize([ScreenNames.FloorPlan, ScreenNames.MapPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<PeopleVehicleInOutTotal>>> PeopleCountForMap(MapWidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.PeopleCountForMapAsync(widgetRequest);
            return StandardAPIResponse<PeopleVehicleInOutTotal>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("SlipandFallDetectionForMap")]
        [CustomAuthorize([ScreenNames.FloorPlan, ScreenNames.MapPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> SlipandFallDetectionForMap(MapWidgetRequest widgetRequest)
        {
            bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.SlipFallQueueAnalysisDataAsync(widgetRequest.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, (int)widgetRequest.Channel, 10, isarchived);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("PedestrianDetectionForMap")]
        [CustomAuthorize([ScreenNames.FloorPlan, ScreenNames.MapPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> PedestrianDetectionForMap(MapWidgetRequest widgetRequest)
        {
            bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.PedestrianQueueAnalysisDataAsync(widgetRequest.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, (int)widgetRequest.Channel, 10, isarchived);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleQueueManagementForMap")]
        [CustomAuthorize([ScreenNames.FloorPlan, ScreenNames.MapPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> VehicleQueueManagementForMap(MapWidgetRequest widgetRequest)
        {
            bool isArchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            IEnumerable<EventQueueAnalysis> result = await _queueManagementRepository.VehicleQueueAnalysisDataAsync(widgetRequest.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, (int)widgetRequest.Channel, 10, isArchived);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleSpeedDetectionForMap")]
        [CustomAuthorize([ScreenNames.FloorPlan, ScreenNames.MapPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> VehicleSpeedDetectionForMap(MapWidgetRequest widgetRequest)
        {
            bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.VehicleSpeedViolationAnalysisDataAsync(widgetRequest.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, (int)widgetRequest.Channel, 10, isarchived);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("TrafficJamDetectionForMap")]
        [CustomAuthorize([ScreenNames.FloorPlan, ScreenNames.MapPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> TrafficJamDetectionForMap(MapWidgetRequest widgetRequest)
        {
            bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.TrafficJamAnalysisDataAsync(widgetRequest.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, (int)widgetRequest.Channel, 10, isarchived);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("VehicleCountForMap")]
        [CustomAuthorize([ScreenNames.FloorPlan, ScreenNames.MapPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<PeopleVehicleInOutTotal>>> VehicleCountForMap(MapWidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            var result = await _widgetService.VehicleCountForMapAsync(widgetRequest);
            return StandardAPIResponse<PeopleVehicleInOutTotal>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ShoppingCountForMap")]
        [CustomAuthorize([ScreenNames.FloorPlan, ScreenNames.MapPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> ShoppingCountForMap(MapWidgetRequest widgetRequest)
        {
            bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            SetUserRole(widgetRequest);
            var result = await _shoppingCartCountRepository.ShoppingCartCountAnalysisData(widgetRequest.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, (int)widgetRequest.Channel, 10, isarchived);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ForkliftCountForMap")]
        [CustomAuthorize([ScreenNames.FloorPlan, ScreenNames.MapPlan], ScreenNames.MapFloorPlan)]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<EventQueueAnalysis>>>> ForkliftCountForMap(MapWidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);
            bool isArchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            var result = await _forkliftCountRepository.ForkliftCountAnalysisDataAsync(widgetRequest.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, (int)widgetRequest.Channel, 10,isArchived);
            return StandardAPIResponse<IEnumerable<EventQueueAnalysis>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        #endregion

        #region Maintenance

        [HttpPost]
        [Route("CameraMaintenanceStatusWidget")]
        [CustomAuthorize([ScreenNames.CameraMaintenanceStatus], ScreenNames.Maintenance)]
        public async Task<ActionResult<StandardAPIResponse<List<MaintenanceStatusWidgetResponse>>>> CameraMaintenanceStatusWidget(WidgetRequest widgetRequest)
        {
            var result = await _widgetService.GetMaintenanceStatusWidgetData(widgetRequest);
            return StandardAPIResponse<List<MaintenanceStatusWidgetResponse>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("CameraInMaintenance")]
        [CustomAuthorize([ScreenNames.CamerasInMaintenance], ScreenNames.Maintenance)]
        public async Task<ActionResult<StandardAPIResponse<CameraInMaintenanceResDto>>> CameraInMaintenance(WidgetRequest widgetRequest)
        {
            widgetRequest.UserId = _currentUserService.UserId;
            var result = await _widgetService.GetCameraInMaintenanceAsync(widgetRequest);

            return StandardAPIResponse<CameraInMaintenanceResDto>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("CameraMaintenanceStatusWidget/csv")]
        [CustomAuthorize([ScreenNames.CameraMaintenanceStatus], ScreenNames.Maintenance)]
        public async Task<IActionResult> CameraMaintenanceStatusCsv(WidgetRequest widgetRequest)
        {
            widgetRequest.UserId = _currentUserService.UserId;
            var csvBuilder = await _widgetService.GetMaintenanceStatusCsv(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("CameraDisconnectedTrackerAnalysis/csv")]
        [CustomAuthorize([ScreenNames.FeatureTypes], ScreenNames.Camera)]
        public async Task<IActionResult> CameraDisconnectedTrackerAnalysisCSV(WidgetRequest widgetRequest)
        {
            widgetRequest.UserId = _currentUserService.UserId;
            var csvBuilder = await _widgetService.CameraDisconnectedTrackerCSVData(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("CameraInMaintenance/csv")]
        [CustomAuthorize([ScreenNames.CamerasInMaintenance], ScreenNames.Maintenance)]
        public async Task<IActionResult> CameraInMaintenanceCsv(WidgetRequest widgetRequest)
        {
            widgetRequest.UserId = _currentUserService.UserId;
            var csvBuilder = await _widgetService.GetCameraInMaintenanceAsyncCsv(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        #endregion

        [HttpPost]
        [Route("VehicleParkingAnalysis/csv")]
        [CustomAuthorize([ScreenNames.Parking], ScreenNames.Vehicle)]
        public async Task<IActionResult> VehicleParkingAnalysisCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _widgetService.VehicleParkingAnalysisDataCsvAsync(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("ANPRVehicleParking/csv")]
        [CustomAuthorize([ScreenNames.ANPRParking], ScreenNames.ANPR)]
        public async Task<IActionResult> ANPRVehicleParkingWidgetCsvData(WidgetRequest widgetRequest)
        {
            SetUserRole(widgetRequest);

            var csvBuilder = await _aNPRVehicleService.ANPRVehicleParkingCountCsvAsync(widgetRequest);
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"{widgetRequest.WidgetName}.csv");
        }

        [HttpPost]
        [Route("DownloadMultipleWidgetsCsv")]
        [CustomAuthorize([ScreenNames.CanDownloadCSVReports], ScreenNames.AdvanceExportReportMaster)]
        public async Task<IActionResult> DownloadMultipleWidgetsCsv([FromBody] WidgetRequest widgetRequest)
        {
            var combinedCsv = new StringBuilder();
            var fileName = $"Dashboard_Widgets_{DateTime.Now:dd_MM_yyyy}.csv";

            if (widgetRequest == null)
            {
                return File(Encoding.UTF8.GetBytes(string.Empty), "text/csv", $"Blank_{fileName}");
            }

            SetUserRole(widgetRequest);

            // First get CSV for all widgets from existing service
            var widgetCsv = await _widgetService.DownloadMultipleWidgetsCsvAsync(widgetRequest);

            if (widgetCsv != null && widgetCsv.Length > 0)
            {
                combinedCsv.AppendLine(widgetCsv.ToString());
            }

            // Now append ANPRParking separately if present
            var anprWidget = widgetRequest.WidgetTitleNames?
                                .FirstOrDefault(x => x.Id == "ANPRParking");

            if (anprWidget != null)
            {
                var localRequest = new WidgetRequest
                {
                    UserId = widgetRequest.UserId,
                    FloorIds = widgetRequest.FloorIds,
                    ZoneIds = widgetRequest.ZoneIds,
                    userRoles = widgetRequest.userRoles,
                    StartDate = widgetRequest.StartDate,
                    EndDate = widgetRequest.EndDate,
                    AverageIntervalMinute = widgetRequest.AverageIntervalMinute,
                    IntervalMinute = widgetRequest.IntervalMinute,
                    calculatorExportRule = widgetRequest.calculatorExportRule,
                    WidgetName = anprWidget.Title,
                    WidgetTitleNames = null
                };

                var anprCsv = await _aNPRVehicleService.ANPRVehicleParkingCountCsvAsync(localRequest);

                if (anprCsv != null && anprCsv.Length > 0)
                {
                    combinedCsv.AppendLine();
                    combinedCsv.AppendLine(anprCsv.ToString());
                }
            }

            if (combinedCsv == null || combinedCsv.Length == 0)
            {
                return NoContent(); // HTTP 204
            }

            var bytes = Encoding.UTF8.GetBytes(combinedCsv.ToString());
            return File(bytes, "text/csv", fileName);
        }

        [HttpPost]
        [Route("GeneratePDF")]
        [CustomAuthorize([ScreenNames.CanDownloadPDFReports], ScreenNames.AdvanceExportReportMaster)]
        //public async Task<ActionResult> GeneratePDF(GeneratePdfDataRequest generatePdfDataRequest)
        //{
        //    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Template/PdfTemplate.html");
        //    var htmlContent = System.IO.File.ReadAllText(htmlPath);
        //    htmlContent += "<h3>Date Time Range : " + generatePdfDataRequest.StartDate + " - " + generatePdfDataRequest.EndDate + "</h3>";
        //    foreach (var svgData in generatePdfDataRequest.SVGData)
        //    {
        //        htmlContent += "<h1>" + svgData.WidgetName + "</h1>";
        //        htmlContent += svgData.SVGData;
        //    }
        //    var pdf = await _pdfGenerator.GeneratePdfFromHtml(htmlContent);
        //    return File(pdf, "application/pdf", "SampleReport.pdf");
        //}


        private void SetUserRole(WidgetRequest widgetRequest)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            widgetRequest.userRoles = user?.Claims.Where(x => x.Type == "role").Select(y => y.Value);
            widgetRequest.UserId = _currentUserService.UserId;
        }
    }
}
