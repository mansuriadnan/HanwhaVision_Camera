using AutoMapper;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Utilities;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Infrastructure.Utilities;
using HanwhaClient.Model.Auth;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Common.ReferenceData;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.PeopleWidget;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;


namespace HanwhaClient.Application.Services
{
    public class WidgetService : IWidgetService
    {
        private readonly IDashboardPreferenceRepository _dashboardPreferenceRepository;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IZoneCameraRepository _zoneCameraRepository;
        private readonly IPeopleCountRepository _peopleCountRepository;
        private readonly IZoneRepository _zoneRepository;
        private readonly IMapper _mapper;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IQueueManagementRepository _queueManagementRepository;
        private readonly IShoppingCartCountRepository _shoppingCartCountRepository;
        private readonly IForkliftCountRepository _forkliftCountRepository;
        private readonly IWidgetRepository _widgetRepository;
        private readonly IPermissionService _permissionService;
        private readonly IDeviceEventsRepository _deviceEventsRepository;
        private readonly IMultiLaneVehicleCountRepository _multiLaneVehicleCountRepository;
        private readonly IHeatMapRepository _heatMapRepository;
        private readonly IUsersService _usersService;
        private readonly IDateConvert _dateConvert;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDeviceApiService _deviceApiService;
        private readonly IPeopleWidgetService _peopleWidgetService;
        private readonly IDeviceMasterService _deviceMasterService;
        private readonly IQueueManagementArchiveRepository _queueManagementArchiveRepository;
        private readonly IDeviceEventsArchiveRepository _deviceEventsArchiveRepository;
        private readonly IHeatMapArchiveRepository _heatMapArchiveRepository;
        private readonly IOfflineDevicesRepository _offlineDevicesRepository;
        private readonly IMaintenanceScheduleRepository _maintenanceScheduleRepository;
        private readonly IRMAService _rmaService;
        private readonly IArchiveTimeService _archiveTimeService;
        private readonly IVehicleParkingCountRepository _vehicleParkingCountRepository;
        private readonly IDeviceExceptionWidgetService _deviceExeceptionWidgetService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRoleRepository _roleRepository;
        private readonly IViServerManagementRepository _serverManagementRepository;
        private readonly IRoleScreenMappingRepository _roleScreenMappingRepository;
        private readonly ICacheService _cacheService;
        private readonly IFloorService _floorService;



        public WidgetService(IDashboardPreferenceRepository DashboardPreferenceRepository,
            IDeviceMasterRepository deviceMasterRepository,
            IMapper mapper,
            IZoneCameraRepository zoneCameraRepository,
            IPeopleCountRepository peopleCountRepository,
            IZoneRepository zoneRepository,
            IVehicleRepository vehicleRepository,
            IQueueManagementRepository queueManagementRepository,
            IShoppingCartCountRepository shoppingCartCountRepository,
            IWidgetRepository widgetRepository,
            IPermissionService permissionService,
            IDeviceEventsRepository deviceEventsRepository,
            IMultiLaneVehicleCountRepository multiLaneVehicleCountRepository,
            IForkliftCountRepository forkliftCountRepository,
            IHeatMapRepository heatMapRepository,
            IUsersService usersService,
            IDateConvert dateConvert,
            ICurrentUserService currentUserService,
            IDeviceApiService deviceApiService,
            IPeopleWidgetService peopleWidgetService,
            IDeviceMasterService deviceMasterService,
            IQueueManagementArchiveRepository queueManagementArchiveRepository,
            IDeviceEventsArchiveRepository deviceEventsArchiveRepository,
            IHeatMapArchiveRepository heatMapArchiveRepository,
            IOfflineDevicesRepository offlineDevicesRepository,
            IRMAService rmaService,
            IMaintenanceScheduleRepository maintenanceScheduleRepository,
            IArchiveTimeService archiveTimeService,
            IVehicleParkingCountRepository vehicleParkingCountRepository,
            IDeviceExceptionWidgetService deviceExeceptionWidgetService,
            IHttpContextAccessor httpContextAccessor,
            IRoleRepository roleRepository,
            IViServerManagementRepository serverManagementRepository,
            IRoleScreenMappingRepository roleScreenMappingRepository,
            ICacheService cacheService,
            IFloorService floorService)
        {
            _dashboardPreferenceRepository = DashboardPreferenceRepository;
            _deviceMasterRepository = deviceMasterRepository;
            _mapper = mapper;
            _zoneCameraRepository = zoneCameraRepository;
            _peopleCountRepository = peopleCountRepository;
            _zoneRepository = zoneRepository;
            _vehicleRepository = vehicleRepository;
            _queueManagementRepository = queueManagementRepository;
            _shoppingCartCountRepository = shoppingCartCountRepository;
            _widgetRepository = widgetRepository;
            _permissionService = permissionService;
            _deviceEventsRepository = deviceEventsRepository;
            _multiLaneVehicleCountRepository = multiLaneVehicleCountRepository;
            _forkliftCountRepository = forkliftCountRepository;
            _heatMapRepository = heatMapRepository;
            _usersService = usersService;
            _dateConvert = dateConvert;
            _currentUserService = currentUserService;
            _deviceApiService = deviceApiService;
            _peopleWidgetService = peopleWidgetService;
            _deviceMasterService = deviceMasterService;
            _queueManagementArchiveRepository = queueManagementArchiveRepository;
            _deviceEventsArchiveRepository = deviceEventsArchiveRepository;
            _heatMapArchiveRepository = heatMapArchiveRepository;
            _offlineDevicesRepository = offlineDevicesRepository;
            _rmaService = rmaService;
            _maintenanceScheduleRepository = maintenanceScheduleRepository;
            _archiveTimeService = archiveTimeService;
            _vehicleParkingCountRepository = vehicleParkingCountRepository;
            _deviceExeceptionWidgetService = deviceExeceptionWidgetService;
            _httpContextAccessor = httpContextAccessor;
            _roleRepository = roleRepository;
            _serverManagementRepository = serverManagementRepository;
            _roleScreenMappingRepository = roleScreenMappingRepository;
            _cacheService = cacheService;
            _floorService = floorService;
        }

        public async Task<CameraCountResponse> GetTotalCameraCountAsync(IEnumerable<string> floorId, IEnumerable<string>? zoneId)
        {
            ProjectionDefinition<DeviceMaster> projection = Builders<DeviceMaster>.Projection
            .Include("channelEvent")
            .Include("isOnline")
            .Include("apiModel");

            IEnumerable<string> deviceIds = Enumerable.Empty<string>();

            if (floorId != null && floorId.FirstOrDefault() == "000000000000000000000000")
            {
                var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
            }
            else
            {
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneLinkedServerAsync(floorId, zoneId);
            }

            var deviceDetails = await _deviceMasterRepository.GetManyFromLinkedServerAsync(deviceIds, projection);
            int totalCount = deviceDetails.Sum(device =>
                             (device.DeviceType == "AIB" || device.DeviceType == "MLenses") && device.ChannelEvent != null
                             ? device.ChannelEvent.Count(x => x.Connected) : 1);

            int onlineCount = deviceDetails.Where(x => x.IsOnline == true).Sum(device =>
                             (device.DeviceType == "AIB" || device.DeviceType == "MLenses") && device.ChannelEvent != null
                             ? device.ChannelEvent.Count(x => x.Connected) : 1);

            int offlineCount = deviceDetails.Where(x => x.IsOnline == false).Sum(device =>
                             (device.DeviceType == "AIB" || device.DeviceType == "MLenses") && device.ChannelEvent != null
                             ? device.ChannelEvent.Count(x => x.Connected) : 1);

            return new CameraCountResponse
            {
                TotalCameraCount = totalCount,
                OnlineCameraCount = onlineCount,
                OflineCameraCount = offlineCount
            };
        }

        public async Task<IEnumerable<CameraSeriesCountResponse>> CameraCountByModelAsync(IEnumerable<string> floorId, IEnumerable<string>? zoneId)
        {
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();
            if (floorId != null && floorId.FirstOrDefault() == "000000000000000000000000")
            {
                var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
            }
            else
            {
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneLinkedServerAsync(floorId, zoneId);
            }
            var result = await _deviceMasterRepository.GetCameraSeriesCountAsync(deviceIds);
            if (!result.Any(x => x.SeriesName == "P Series"))
                result = result.Append(new CameraSeriesCountResponse { SeriesName = "P Series", TotalCount = 0 });
            if (!result.Any(x => x.SeriesName == "X Series"))
                result = result.Append(new CameraSeriesCountResponse { SeriesName = "X Series", TotalCount = 0 });
            if (!result.Any(x => x.SeriesName == "Q Series"))
                result = result.Append(new CameraSeriesCountResponse { SeriesName = "Q Series", TotalCount = 0 });
            if (!result.Any(x => x.SeriesName == "T Series"))
                result = result.Append(new CameraSeriesCountResponse { SeriesName = "T Series", TotalCount = 0 });
            if (!result.Any(x => x.SeriesName == "AI Box"))
                result = result.Append(new CameraSeriesCountResponse { SeriesName = "AI Box", TotalCount = 0 });

            return result;
        }

        public async Task<IEnumerable<CameraFeaturesCountResponse>> CameraCountByFeaturesAsync(IEnumerable<string> floorId, IEnumerable<string>? zoneId)
        {
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();
            if (floorId != null && floorId.FirstOrDefault() == "000000000000000000000000")
            {
                var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
            }
            else
            {
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneLinkedServerAsync(floorId, zoneId);
            }
            var result = await _deviceMasterRepository.CameraCountByFeaturesAsync(deviceIds);
            return result;
        }

        public async Task<CameraDisconnectedTrackerResponse> CameraDisconnectedTrackerAsync(WidgetRequest widgetRequest)
        {
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();
            if (widgetRequest.FloorIds != null && widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
            }
            else
            {
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
            }

            RMAWidgetRequest widgetRequest1 = new RMAWidgetRequest();
            widgetRequest1.StartDate = widgetRequest.StartDate;
            widgetRequest1.EndDate = widgetRequest.EndDate;
            widgetRequest1.DeviceIds = deviceIds;
            var data = await _offlineDevicesRepository.CameraDisconnectedTrackerAsync(widgetRequest1);
            var currentDateTime = DateTime.UtcNow;

            CameraDisconnectedTrackerResponse cameraDisconnectedTrackerResponse = new CameraDisconnectedTrackerResponse();
            cameraDisconnectedTrackerResponse.HoursCount = data.Where(x => x.OfflineTime <= currentDateTime && x.OfflineTime >= currentDateTime.AddHours(-1)).DistinctBy(y => y.DeviceId).Count();
            cameraDisconnectedTrackerResponse.DayCount = data.Where(x => x.OfflineTime <= currentDateTime && (x.OnlineTime == null || x.OnlineTime >= currentDateTime.AddDays(-1))).DistinctBy(x => x.DeviceId).Count();
            cameraDisconnectedTrackerResponse.WeekCount = data.Where(x => x.OfflineTime <= currentDateTime && (x.OnlineTime == null || x.OnlineTime >= currentDateTime.AddDays(-7))).DistinctBy(x => x.DeviceId).Count();
            cameraDisconnectedTrackerResponse.MonthCount = data.Where(x => x.OfflineTime <= currentDateTime && (x.OnlineTime == null || x.OnlineTime >= currentDateTime.AddMonths(-1))).DistinctBy(x => x.DeviceId).Count();

            return cameraDisconnectedTrackerResponse;
        }

        public async Task<(IEnumerable<CameraDisconnectedTrackerAnalsisResponse> trackerAnalsisRes, Dictionary<string, object> referenceData)> CameraDisconnectedTrackerAnalysisAsync(WidgetRequest widgetRequest)
        {
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();
            if (widgetRequest.FloorIds != null && widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
            }
            else
            {
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneLinkedServerAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
            }

            RMAWidgetRequest widgetRequest1 = new RMAWidgetRequest();
            widgetRequest1.StartDate = widgetRequest.StartDate;
            widgetRequest1.EndDate = widgetRequest.EndDate;
            widgetRequest1.DeviceIds = deviceIds;
            var data = await _offlineDevicesRepository.CameraDisconnectedTrackerAsync(widgetRequest1);
            var deviceIdRes = data
                .Select(o => o.DeviceId)
                .Distinct()
                .ToList();

            Dictionary<string, object> referenceData = new();
            referenceData.Add("IpAddress", await GetIpAddressReferenceData(deviceIdRes));
            var response = data.Select(x => new CameraDisconnectedTrackerAnalsisResponse
            {
                DeviceId = x.DeviceId,
                OfflineTime = x.OfflineTime,
                OnlineTime = x.OnlineTime
            }).AsEnumerable();

            return (response, referenceData);
        }

        public async Task<(IEnumerable<CameraCapacityUtilizationByZones>, UtilizationMostLeastDay)> VehicleCameraCapacityUtilizationByZoneAsync(WidgetRequest widgetRequest)
        {
            List<CameraCapacityUtilizationByZones> vehicleCameraCapacityUtilizations = new List<CameraCapacityUtilizationByZones>();
            List<IEnumerable<CameraCapacityUtilizationByDevice>> cameraUtilizationLst = new List<IEnumerable<CameraCapacityUtilizationByDevice>>();
            var timeZone = await _usersService.GetTimeZone(widgetRequest.UserId);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            foreach (var zone in zoneData)
            {
                var deepRequest = JsonSerializer.Deserialize<WidgetRequestForChart>(JsonSerializer.Serialize(widgetRequest));
                deepRequest.ZoneIds = [zone.Id];
                var result = await AverageVehicleCountChartAsync(deepRequest, true);
                var zoneWiseUtilization = result.Select(x => new CountAnalysisData
                {
                    DateTime = (x.DateTime.Value + offsetTimeStamp).Date,
                    Count = x.InCount - x.OutCount,
                }).Where(x => x.DateTime >= widgetRequest.StartDate).AsEnumerable();

                var dayWiseUtilizationData = zoneWiseUtilization.GroupBy(x => x.DateTime)
                    .Select(y => new
                    {
                        DateTime = y.Key,
                        Count = y.Max(z => z.Count)
                    }).ToList();

                int utilizationCount = (int)dayWiseUtilizationData.Sum(x => x.Count);
                int count = dayWiseUtilizationData.Count();
                int utilization = utilizationCount;
                double percentage = 0;

                if (count > 0 && zone.VehicleOccupancy > 0)
                {
                    percentage = (double)((utilization * 100) / zone.VehicleOccupancy);
                }
                percentage = double.IsNaN(percentage) ? 0 : percentage;

                vehicleCameraCapacityUtilizations.Add(new CameraCapacityUtilizationByZones
                {
                    ZoneName = zone.ZoneName,
                    MaxCapacity = zone.VehicleOccupancy != null ? (int)zone.VehicleOccupancy : 0,
                    Utilization = utilization,
                    Percentage = percentage,
                    VehicleDefaultOccupancy = zone.VehicleDefaultOccupancy
                });
            }

            UtilizationMostLeastDay mostdayLeastday = new UtilizationMostLeastDay();
            if (cameraUtilizationLst.Count() > 0)
            {
                var groupedUtilization = cameraUtilizationLst
                                        .SelectMany(x => x)
                                        .Where(x => x.Date.HasValue)
                                        .GroupBy(x => x.Date.Value)
                                        .Select(g => new
                                        {
                                            Date = g.Key,
                                            Utilization = g.Sum(x => x.UtilizationCount)
                                        })
                                        .ToList();
                mostdayLeastday.MostDayUtilization = groupedUtilization.Max(x => x.Utilization);
                mostdayLeastday.LeastDayUtilization = groupedUtilization.Min(x => x.Utilization);
                mostdayLeastday.MostDayUtilizationDay = groupedUtilization.FirstOrDefault(x => x.Utilization == mostdayLeastday.MostDayUtilization).Date;
                mostdayLeastday.LeastDayUtilizationDay = groupedUtilization.FirstOrDefault(x => x.Utilization == mostdayLeastday.LeastDayUtilization).Date;
            }

            return (vehicleCameraCapacityUtilizations, mostdayLeastday);
        }

        public async Task<CapacityUtilization> VehicleCapacityUtilizationAsync(WidgetRequest widgetRequest)
        {
            var peopleCameraCapacityUtilizationByZones = await VehicleCameraCapacityUtilizationByZoneAsync(widgetRequest);
            double percentage = (peopleCameraCapacityUtilizationByZones.Item1.Sum(x => x.Utilization) * 100) / peopleCameraCapacityUtilizationByZones.Item1.Sum(x => x.MaxCapacity);
            CapacityUtilization peopleCapacityUtilization = new CapacityUtilization();
            if (peopleCameraCapacityUtilizationByZones.Item1.Count() > 0)
            {
                peopleCapacityUtilization.Utilization = peopleCameraCapacityUtilizationByZones.Item1.Sum(x => x.Utilization);
                peopleCapacityUtilization.TotalCapacity = peopleCameraCapacityUtilizationByZones.Item1.Sum(x => x.MaxCapacity);
                peopleCapacityUtilization.Percentage = double.IsNaN(percentage) || double.IsInfinity(percentage) ? 0 : percentage;
                peopleCapacityUtilization.UtilizationMostLeastDay = peopleCameraCapacityUtilizationByZones.Item2;
                peopleCapacityUtilization.VehicleDefaultOccupancy = peopleCameraCapacityUtilizationByZones.Item1.Sum(x => x.VehicleDefaultOccupancy);

            }
            return peopleCapacityUtilization;
        }

        public async Task<InOutPeopleCountAverageWidgetDto> AveragePeopleCountAsync(WidgetRequest widgetRequest)
        {
            InOutPeopleCountAverageWidgetDto inOutPeopleCountAverageWidgetDto = new InOutPeopleCountAverageWidgetDto();
            var zoneSummaries = await GetZoneSummariesAsync(widgetRequest, "people", null, null);

            var allPeopleCounts = zoneSummaries.SelectMany(z => z.Value).ToList();

            var maxCountsByDate = allPeopleCounts
                                   .Where(x => x.Date.HasValue)
                                   .GroupBy(x => x.Date.Value.Date)
                                   .Select(g => new PeopleVehicleCountSummary
                                   {
                                       Date = g.Key,
                                       TotalInCount = g.Max(x => x.TotalInCount),
                                       TotalOutCount = g.Max(x => x.TotalOutCount)
                                   }).ToList();

            if (maxCountsByDate.Any())
            {
                var minIn = maxCountsByDate.MinBy(x => x.TotalInCount);
                var maxIn = maxCountsByDate.MaxBy(x => x.TotalInCount);
                var avgIn = maxCountsByDate.Average(x => x.TotalInCount);

                var minOut = maxCountsByDate.MinBy(x => x.TotalOutCount);
                var maxOut = maxCountsByDate.MaxBy(x => x.TotalOutCount);
                var avgOut = maxCountsByDate.Average(x => x.TotalOutCount);
                var totalInPeople = maxCountsByDate.Sum(x => x.TotalInCount);

                var overallSummary = new InOutPeopleCountAverageWidgetDto
                {
                    MinInCount = minIn.TotalInCount,
                    MinInDate = minIn.Date,

                    MaxInCount = maxIn.TotalInCount,
                    MaxInDate = maxIn.Date,

                    MinOutCount = minOut.TotalOutCount,
                    MinOutDate = minOut.Date,

                    MaxOutCount = maxOut.TotalOutCount,
                    MaxOutDate = maxOut.Date,

                    AverageInCount = Math.Round(avgIn, 2),
                    AverageOutCount = Math.Round(avgOut, 2),

                    TotalInPeople = totalInPeople
                };
                return overallSummary;
            }

            return inOutPeopleCountAverageWidgetDto;
        }

        public async Task<InOutVehicleCountAverageWidgetDto> AverageVehicleCountAsync(WidgetRequest widgetRequest)
        {
            InOutVehicleCountAverageWidgetDto inOutPeopleCountAverageWidgetDto = new InOutVehicleCountAverageWidgetDto();

            var zoneSummaries = await GetZoneSummariesAsync(widgetRequest, "vehicle", null, null);

            var allVehicleCounts = zoneSummaries.SelectMany(z => z.Value).ToList();

            if (allVehicleCounts.Any())
            {
                var minIn = allVehicleCounts.MinBy(x => x.TotalInCount);
                var maxIn = allVehicleCounts.MaxBy(x => x.TotalInCount);
                var avgIn = allVehicleCounts.Average(x => x.TotalInCount);

                var minOut = allVehicleCounts.MinBy(x => x.TotalOutCount);
                var maxOut = allVehicleCounts.MaxBy(x => x.TotalOutCount);
                var avgOut = allVehicleCounts.Average(x => x.TotalOutCount);
                var totalInVehicle = allVehicleCounts.Sum(x => x.TotalInCount);

                var overallSummary = new InOutVehicleCountAverageWidgetDto
                {
                    MinInCount = minIn.TotalInCount,
                    MinInDate = minIn.Date,

                    MaxInCount = maxIn.TotalInCount,
                    MaxInDate = maxIn.Date,

                    MinOutCount = minOut.TotalOutCount,
                    MinOutDate = minOut.Date,

                    MaxOutCount = maxOut.TotalOutCount,
                    MaxOutDate = maxOut.Date,

                    AverageInCount = Math.Round(avgIn, 2),
                    AverageOutCount = Math.Round(avgOut, 2),

                    TotalInVehicle = totalInVehicle
                };
                return overallSummary;
            }

            return inOutPeopleCountAverageWidgetDto;
        }
        public async Task<VehicleByTypeCountWidgetDto> VehicleByTypeCountAsync(WidgetRequest widgetRequest)
        {
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            int totalInCar = 0, totalInBus = 0, totalInTruck = 0, totalInMotorCycle = 0, totalInBicycle = 0;
            int totalOutCar = 0, totalOutBus = 0, totalOutTruck = 0, totalOutMotorCycle = 0, totalOutBicycle = 0;
            if (zoneData != null)
            {
                foreach (var zone in zoneData)
                {
                    IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                    if (zone.Id != null && zone.Id == "00")
                    {
                        zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                    }
                    else
                    {
                        zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                    }

                    if (zoneCameraList.Any())
                    {
                        var deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
                        var filters = new List<FilterDefinition<VehicleCount>>();
                        filters.Add(Builders<VehicleCount>.Filter.In(x => x.DeviceId, deviceIds));
                        filters.Add(Builders<VehicleCount>.Filter.Gte(x => x.CreatedOn, widgetRequest.StartDate));
                        filters.Add(Builders<VehicleCount>.Filter.Lt(x => x.CreatedOn, widgetRequest.EndDate.AddSeconds(1)));

                        var filter = Builders<VehicleCount>.Filter.And(filters);

                        var latestPerDayVehicleCount = await _vehicleRepository.GetLatestVehicleCountDetails(filter);

                        foreach (var data in latestPerDayVehicleCount)
                        {
                            var deviceFilter = zoneCameraList.FirstOrDefault(zc => zc.DeviceId == data.DeviceId);
                            if (deviceFilter != null)
                            {
                                var matchingCounts = data.VehicleCounts
                                    .Where(vc => vc.Channel == deviceFilter.Channel)
                                    .SelectMany(vc => vc.Lines.Where(line => deviceFilter.VehicleLineIndex.Contains(line.LineIndex)));

                                totalInCar += matchingCounts.Sum(l => l.In.Car);
                                totalInBus += matchingCounts.Sum(l => l.In.Bus);
                                totalInTruck += matchingCounts.Sum(l => l.In.Truck);
                                totalInMotorCycle += matchingCounts.Sum(l => l.In.Motorcycle);
                                totalInBicycle += matchingCounts.Sum(l => l.In.Bicycle);

                                totalOutCar += matchingCounts.Sum(l => l.Out.Car);
                                totalOutBus += matchingCounts.Sum(l => l.Out.Bus);
                                totalOutTruck += matchingCounts.Sum(l => l.Out.Truck);
                                totalOutMotorCycle += matchingCounts.Sum(l => l.Out.Motorcycle);
                                totalOutBicycle += matchingCounts.Sum(l => l.Out.Bicycle);
                            }
                        }
                    }
                }
            }
            var result = new VehicleByTypeCountWidgetDto
            {
                CarInCount = totalInCar,
                BusInCount = totalInBus,
                TruckInCount = totalInTruck,
                MotorCycleInCount = totalInMotorCycle,
                BicycleInCount = totalInBicycle,

                CarOutCount = totalOutCar,
                BicycleOutCount = totalOutBicycle,
                BusOutCount = totalOutBus,
                MotorCycleOutCount = totalOutMotorCycle,
                TruckOutCount = totalOutTruck,

                TotalOutVehicleCount = totalOutCar + totalOutBicycle + totalOutBus + totalOutMotorCycle + totalOutTruck,
                TotalInVehicleCount = totalInCar + totalInBus + totalInTruck + totalInMotorCycle + totalInBicycle,
            };
            return result;
        }


        public async Task<PeopleVehicleInOutTotal> PeopleIOnOutTotalAsync(WidgetRequest widgetRequest)
        {
            PeopleVehicleInOutTotal peopleVehicleInOutTotal = new PeopleVehicleInOutTotal();
            var zoneSummaries = await GetZoneSummariesAsync(widgetRequest, "people", null, null);
            var allPeopleCounts = zoneSummaries.SelectMany(z => z.Value).ToList();

            if (allPeopleCounts.Any())
            {
                var totalInPeople = allPeopleCounts.Sum(x => x.TotalInCount);
                var totalOutPeople = allPeopleCounts.Sum(x => x.TotalOutCount);

                var overallSummary = new PeopleVehicleInOutTotal
                {
                    TotalInCount = totalInPeople,
                    TotalOutCount = totalOutPeople,
                };
                return overallSummary;
            }
            return peopleVehicleInOutTotal;
        }

        public async Task<List<ChartAvgInOut>> NewVsTotalVisitorChartAsync(WidgetRequest widgetRequest)
        {
            List<PeopleVehicleInOutAvgChart> peopleInOutCountAnalysis = new List<PeopleVehicleInOutAvgChart>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            List<IEnumerable<PeopleVehicleInOutAvgChart>> peopleInOutCountAnalysisByDevice = new List<IEnumerable<PeopleVehicleInOutAvgChart>>();

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }

                if (!string.IsNullOrEmpty(widgetRequest.DeviceId) && widgetRequest.DeviceId != "6812332a6d517f8bfff611bb")
                {
                    zoneCameraList = zoneCameraList.Where(x => x.DeviceId == widgetRequest.DeviceId);
                }
                if (zoneCameraList != null)
                {
                    foreach (var camera in zoneCameraList)
                    {
                        IEnumerable<PeopleVehicleInOutAvgChart> result = await _peopleCountRepository.PeopleInOutCountAnalysisAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, camera.PeopleLineIndex, widgetRequest.IntervalMinute);
                        if (result != null)
                        {
                            peopleInOutCountAnalysisByDevice.Add(result);
                        }
                    }
                }
            }

            var totalPeopleDefaultOccupancy = zoneData
           .Where(x => (x.IsDeleted ?? true) == false)
           .Sum(x => x.PeopleDefaultOccupancy);


            peopleInOutCountAnalysis = peopleInOutCountAnalysisByDevice.SelectMany(x => x)
                                 .GroupBy(x => x.DateTime)
                                 .Select(g => new PeopleVehicleInOutAvgChart
                                 {
                                     DateTime = g.Key,
                                     InCount = g.Sum(x => x.InCount),
                                     OutCount = g.Sum(x => x.InCount) + (int)totalPeopleDefaultOccupancy,
                                     DateTimeCsv = g.Key.ToString()
                                 }).OrderBy(x => x.DateTime).ToList();


            var finalResult = peopleInOutCountAnalysis.Select(x => new ChartAvgInOut
            {
                DateTime = x.DateTime,
                NewVisitor = x.InCount,
                TotalVisitor = x.OutCount,
            }).ToList();



            return finalResult;
        }

        //public async Task<PeopleVehicleInOutTotal> PeopleIOnOutTotalV2Async(WidgetRequest widgetRequest)
        //{

        //    var zoneData = await GetAllZoneByPermission(widgetRequest);

        //    IEnumerable<ZoneCamera> zoneCameraList;

        //    if (widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
        //    {
        //        zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
        //    }
        //    else
        //    {
        //        zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneIds(zoneData.Select(x => x.Id));
        //    }

        //    if (!string.IsNullOrEmpty(widgetRequest.DeviceId) && widgetRequest.DeviceId != "6812332a6d517f8bfff611bb")
        //    {
        //        zoneCameraList = zoneCameraList.Where(x => x.DeviceId == widgetRequest.DeviceId).ToList();
        //    }
        //    var timeZone = await _usersService.GetTimeZone(widgetRequest.UserId);
        //    var data = await _peopleCountRepository.PeopleInOutCountAnalysisV2Async(zoneCameraList, widgetRequest.StartDate, widgetRequest.EndDate);


        //    data = (from d in data
        //            join f in zoneCameraList.AsEnumerable()
        //            on new { DeviceId = d.DeviceId, ChannelNo = d.ChannelNo } equals new { DeviceId = f.DeviceId ?? "", ChannelNo = f.Channel }
        //            select d).ToList();

        //    data = data
        //        .Join(
        //            zoneCameraList,
        //            pc => pc.DeviceId,
        //            zc => zc.DeviceId,
        //            (pc, zc) => new { PeopleCount = pc, ZoneCamera = zc }
        //        )
        //        .Select(x => new PeopleCount
        //        {
        //            CreatedOn = GetBucketTime(x.PeopleCount.CreatedOn.Value, widgetRequest.IntervalMinute),
        //            DeviceId = x.PeopleCount.DeviceId,
        //            CameraIP = x.PeopleCount.CameraIP,
        //            ChannelNo = x.PeopleCount.ChannelNo,
        //            Lines = x.PeopleCount.Lines
        //                        .Where(l => x.ZoneCamera.PeopleLineIndex.Contains(l.LineIndex))
        //                        .ToList()
        //        })
        //        .Where(pc => pc.Lines.Any())
        //        .ToList();


        //    var deviceWiseData = data
        //        .GroupBy(pc => new
        //        {
        //            pc.DeviceId,
        //            CreatedOn = pc.CreatedOn
        //        }).Select(d => new PeopleVehicleInOutAvgChart
        //        {
        //            DateTime = d.Key.CreatedOn,
        //            InCount = d.Max(x => x.Lines.Sum(l => l.InCount)),
        //            OutCount = d.Max(x => x.Lines.Sum(l => l.OutCount)),
        //        });


        //    var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);

        //    var count = data.GroupBy(x => new
        //    {
        //        x.DeviceId,
        //        CreatedOn = (x.CreatedOn.Value + offsetTimeStamp).Date
        //    })
        //         .Select(d => new
        //         {
        //             MaxInCount = d.Max(x => x.Lines.Sum(l => l.InCount)),
        //             MaxOutCount = d.Max(x => x.Lines.Sum(l => l.OutCount)),
        //         });
        //    var maxOutCount = count.Sum(x => x.MaxOutCount);
        //    var maxInCount = count.Sum(x => x.MaxInCount);

        //    var overallSummary = new PeopleVehicleInOutTotal
        //    {
        //        TotalInCount = maxInCount,
        //        TotalOutCount = maxOutCount,
        //    };
        //    return overallSummary;
        //}

        public async Task<PeopleVehicleInOutTotal> PeopleCountForMapAsync(MapWidgetRequest widgetRequest)
        {
            PeopleVehicleInOutTotal peopleVehicleInOutTotal = new PeopleVehicleInOutTotal();
            ((WidgetRequest)widgetRequest).DeviceId = widgetRequest.DeviceId;
            var zoneSummaries = await _peopleWidgetService.PeopleInOutCountAnalysisV2Async(widgetRequest);

            if (zoneSummaries.Any())
            {
                var totalInPeople = zoneSummaries.Sum(x => x.InCount);
                var totalOutPeople = zoneSummaries.Sum(x => x.OutCount);

                var overallSummary = new PeopleVehicleInOutTotal
                {
                    TotalInCount = totalInPeople,
                    TotalOutCount = totalOutPeople,
                };
                return overallSummary;
            }
            return peopleVehicleInOutTotal;
        }

        public async Task<PeopleVehicleInOutTotal> VehicleCountForMapAsync(MapWidgetRequest widgetRequest)
        {
            PeopleVehicleInOutTotal peopleVehicleInOutTotal = new PeopleVehicleInOutTotal();
            var zoneSummaries = await GetZoneSummariesAsync(widgetRequest, "vehicle", widgetRequest.DeviceId, widgetRequest.Channel);
            var allPeopleCounts = zoneSummaries.SelectMany(z => z.Value).ToList();

            if (allPeopleCounts.Any())
            {
                var totalInPeople = allPeopleCounts.Sum(x => x.TotalInCount);
                var totalOutPeople = allPeopleCounts.Sum(x => x.TotalOutCount);

                var overallSummary = new PeopleVehicleInOutTotal
                {
                    TotalInCount = totalInPeople,
                    TotalOutCount = totalOutPeople,
                };
                return overallSummary;
            }
            return peopleVehicleInOutTotal;
        }

        public async Task<PeopleVehicleInOutTotal> VehicleIOnOutTotalAsync(WidgetRequest widgetRequest)
        {
            PeopleVehicleInOutTotal peopleVehicleInOutTotal = new PeopleVehicleInOutTotal();
            var zoneSummaries = await GetZoneSummariesAsync(widgetRequest, "vehicle", null, null);
            var allPeopleCounts = zoneSummaries.SelectMany(z => z.Value).ToList();

            if (allPeopleCounts.Any())
            {
                var totalInPeople = allPeopleCounts.Sum(x => x.TotalInCount);
                var totalOutPeople = allPeopleCounts.Sum(x => x.TotalOutCount);

                var overallSummary = new PeopleVehicleInOutTotal
                {
                    TotalInCount = totalInPeople,
                    TotalOutCount = totalOutPeople,
                };
                return overallSummary;
            }
            return peopleVehicleInOutTotal;
        }

        public async Task<PeopleVehicleInOutChartResponse> PeopleIOnOutChartAsync(WidgetRequestForChart widgetRequestForChart)
        {
            var zoneSummaries = await GetZoneSumForPeopleChartAsync(widgetRequestForChart);

            // Step 1: Merge and group by exact DateTime (to remove duplicates)
            var allChartData = zoneSummaries
                               .SelectMany(z => z.Value)
                               .GroupBy(x => x.DateTime)
                               .Select(g => new TimeLineChartPeopleVehicleCount
                               {
                                   DateTime = g.Key,
                                   Count = g.Sum(x => x.Count),
                                   Hour = g.First().Hour,
                                   Time = g.First().Time,
                               }).ToList();

            // Step 2: Compute Value (sum of Count for same hour + same date)
            //var valueMap = allChartData
            //               .GroupBy(x =>
            //               {
            //                   var dt = DateTime.Parse(x.DateTime);
            //                   return new { x.Hour, Date = dt.Date };
            //               })
            //               .ToDictionary(
            //                   g => new { g.Key.Hour, g.Key.Date },
            //                   g => g.Sum(x => x.Count)
            //               );

            //// Step 3: Assign Value from valueMap
            //foreach (var item in allChartData)
            //{
            //    var dt = DateTime.Parse(item.DateTime);
            //    item.Value = valueMap[new { item.Hour, Date = dt.Date }];
            //}

            // Step 4: Return ordered list
            return new PeopleVehicleInOutChartResponse
            {
                Data = allChartData.OrderBy(x => x.DateTime).ToList()
            };
        }

        public async Task<PeopleVehicleInOutChartResponse> VehicleIOnOutChartAsync(WidgetRequestForChart widgetRequest)
        {
            var zoneSummaries = await GetZoneSummariesForChartAsync(widgetRequest);

            // Step 1: Merge and group by exact DateTime (to remove duplicates)
            var allChartData = zoneSummaries
                               .SelectMany(z => z.Value)
                               .GroupBy(x => x.DateTime)
                               .Select(g => new TimeLineChartPeopleVehicleCount
                               {
                                   DateTime = g.Key,
                                   Count = g.Sum(x => x.Count),
                                   Hour = g.First().Hour,
                                   Time = g.First().Time,
                               }).ToList();

            // Step 2: Compute Value (sum of Count for same hour + same date)
            //var valueMap = allChartData
            //               .GroupBy(x =>
            //               {
            //                   var dt = DateTime.Parse(x.DateTime);
            //                   return new { x.Hour, Date = dt.Date };
            //               })
            //               .ToDictionary(
            //                   g => new { g.Key.Hour, g.Key.Date },
            //                   g => g.Sum(x => x.Count)
            //               );

            //// Step 3: Assign Value from valueMap
            //foreach (var item in allChartData)
            //{
            //    var dt = DateTime.Parse(item.DateTime);
            //    item.Value = valueMap[new { item.Hour, Date = dt.Date }];
            //}

            // Step 4: Return ordered list
            return new PeopleVehicleInOutChartResponse
            {
                Data = allChartData.OrderBy(x => x.DateTime).ToList()
            };
        }



        public async Task<IEnumerable<EventQueueAnalysis>> VehicleQueueAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> VehicleQueueAnalysis = new List<EventQueueAnalysis>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        bool isArchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<EventQueueAnalysis> result = await _queueManagementRepository.VehicleQueueAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isArchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            VehicleQueueAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Max(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return VehicleQueueAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> ForkliftQueueAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> ForkliftQueueAnalysis = new List<EventQueueAnalysis>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        bool isArchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<EventQueueAnalysis> result = await _queueManagementRepository.ForkliftQueueAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isArchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            ForkliftQueueAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Max(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return ForkliftQueueAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> ShoppingCartQueueAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> ShoppingQueueAnalysis = new List<EventQueueAnalysis>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        IEnumerable<EventQueueAnalysis> result = await _queueManagementRepository.ShoppingQueueAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            ShoppingQueueAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Max(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return ShoppingQueueAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> PeopleQueueAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> peopleQueueAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        bool isArchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<EventQueueAnalysis> result = await _queueManagementRepository.PeopleQueueAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isArchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            peopleQueueAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Max(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return peopleQueueAnalysis;

        }

        public async Task<IEnumerable<EventQueueAnalysis>> PedestrianQueueAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> PedestrianQueueAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.PedestrianQueueAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            PedestrianQueueAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Sum(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return PedestrianQueueAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> VehicleParkingAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            zoneData = zoneData.Where(x => x.IsParkingZone == true);

            IEnumerable<ZoneCamera> zoneCameraList;

            if (widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
            }
            else
            {
                zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneIds(zoneData.Select(x => x.Id));
            }

            var data = await _vehicleParkingCountRepository.VehicleParkingAnalysisDataAsync(zoneCameraList, widgetRequest.StartDate, widgetRequest.EndDate);

            var validCameraKeys = zoneCameraList
                                    .SelectMany(z => z.VehicleLineIndex.Select(lineIndex => new
                                    {
                                        z.DeviceId,
                                        z.Channel,
                                        LineIndex = lineIndex
                                    })).ToHashSet();


            var filteredData = data
                              .Where(d => validCameraKeys.Contains(new
                              {
                                  d.DeviceId,
                                  d.Channel,
                                  d.LineIndex
                              }))
                              .ToList();

            var timeIntervalData = filteredData
                .Select(x => new VehicleParkingCount
                {
                    CreatedOn = GetBucketTime(x.CreatedOn.Value, 10),
                    DeviceId = x.DeviceId,
                    Channel = x.Channel,
                    LineIndex = x.LineIndex,
                    ParkingCount = x.ParkingCount

                }).ToList();

            var maxPerLine = timeIntervalData
                .GroupBy(x => new
                {
                    x.DeviceId,
                    x.CreatedOn,
                    x.Channel,
                    x.LineIndex
                })
                .Select(g => new
                {
                    g.Key.DeviceId,
                    g.Key.CreatedOn,
                    MaxParking = g.Max(x => x.ParkingCount)
                });


            var result = maxPerLine
                .GroupBy(x => new
                {
                    x.CreatedOn
                })
                .Select(g => new EventQueueAnalysis
                {
                    DateTime = (DateTime)g.Key.CreatedOn,
                    QueueCount = g.Sum(x => x.MaxParking)
                })
                .OrderBy(x => x.DateTime)
                .ToList();

            return result;
        }

        public async Task<IEnumerable<VehicleParking>> VehicleParkingByZonesAsync(WidgetRequest widgetRequest)
        {
            List<VehicleParking> vehicleParkingCountByZones = new List<VehicleParking>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            zoneData = zoneData.Where(x => x.IsParkingZone == true);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }

                var data = await _vehicleParkingCountRepository.VehicleParkingAnalysisDataAsync(zoneCameraList, widgetRequest.StartDate, widgetRequest.EndDate);

                var validCameraKeys = zoneCameraList
                    .SelectMany(z => z.VehicleLineIndex.Select(lineIndex => new
                    {
                        z.DeviceId,
                        z.Channel,
                        LineIndex = lineIndex
                    })).ToHashSet();

                var filteredData = data
                    .Where(d => validCameraKeys.Contains(new
                    {
                        d.DeviceId,
                        d.Channel,
                        d.LineIndex
                    }))
                    .ToList();

                var maxPerLine = filteredData
                    .GroupBy(x => new
                    {
                        x.DeviceId,
                        //x.CreatedOn,
                        x.Channel,
                        x.LineIndex
                    })
                    .Select(g => new
                    {
                        g.Key.DeviceId,
                        MaxParking = g.Max(x => x.ParkingCount)
                    });

                vehicleParkingCountByZones.Add(new VehicleParking
                {
                    ZoneName = zone.ZoneName,
                    ParkingCount = maxPerLine.Sum(x => x.MaxParking),
                    TotalParkingOccupancy = (int)zone.VehicleOccupancy
                });
            }

            return vehicleParkingCountByZones;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> ProxomityDetectionAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> PedestrianQueueAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.ProxomityDetectionAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            PedestrianQueueAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Sum(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return PedestrianQueueAnalysis;
        }

        public async Task<IEnumerable<StoppedVehicleByTypeData>> StoppedVehicleByTypeAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<StoppedVehicleByTypeData> stoppedVehicleByTypeAnalysis = new List<StoppedVehicleByTypeData>();
            List<IEnumerable<StoppedVehicleByTypeData>> queueCountByDevice = new List<IEnumerable<StoppedVehicleByTypeData>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                    foreach (var camera in zoneCameraList)
                    {
                        IEnumerable<StoppedVehicleByTypeData> result = await _deviceEventsRepository.StoppedVehicleByTypeAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            stoppedVehicleByTypeAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new StoppedVehicleByTypeData
                                {
                                    DateTime = g.Key,
                                    Car = g.Sum(x => x.Car),
                                    Bus = g.Sum(x => x.Bus),
                                    Bicycle = g.Sum(x => x.Bicycle),
                                    Motorcycle = g.Sum(x => x.Motorcycle),
                                    Truck = g.Sum(x => x.Truck),
                                }).OrderBy(x => x.DateTime).ToList();

            return stoppedVehicleByTypeAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> VehicleSpeedViolationAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> VehicleSpeedViolationAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        bool isArchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.VehicleSpeedViolationAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isArchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            VehicleSpeedViolationAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Sum(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return VehicleSpeedViolationAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> TrafficJamAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> TrafficJamAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.TrafficJamAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            TrafficJamAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Sum(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return TrafficJamAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> SlipFallQueueAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> SlipFallQueueAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.SlipFallQueueAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            SlipFallQueueAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Sum(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return SlipFallQueueAnalysis;
        }

        public async Task<IEnumerable<MaskDetectionAnalysis>> MaskDetectionAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<MaskDetectionAnalysis> maskDetectionAnalysis = new List<MaskDetectionAnalysis>();
            List<IEnumerable<MaskDetectionAnalysis>> queueCountByDevice = new List<IEnumerable<MaskDetectionAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    foreach (var camera in zoneCameraList)
                    {
                        bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<MaskDetectionAnalysis> result = await _deviceEventsRepository.MaskDetectionAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            maskDetectionAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new MaskDetectionAnalysis
                                {
                                    DateTime = g.Key,
                                    MaskCount = g.Sum(x => x.MaskCount),
                                    WithoutMaskCount = g.Sum(x => x.WithoutMaskCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return maskDetectionAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> WrongWayQueueAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> wrongWayQueueAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.WrongWayQueueAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null && result.Count() > 0)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            wrongWayQueueAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Sum(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return wrongWayQueueAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> BlockedExitAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> BlockedExitAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    foreach (var camera in zoneCameraList)
                    {
                        bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.BlockedExitAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            BlockedExitAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Sum(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return BlockedExitAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> VehicleUTurnAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> vehicleUTurnAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.VehicleUTurnAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            vehicleUTurnAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Sum(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return vehicleUTurnAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> ShoppingCartCountAnalysisData(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> shoppingCountAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        IEnumerable<EventQueueAnalysis> result = await _shoppingCartCountRepository.ShoppingCartCountAnalysisData(camera.DeviceId, widgetRequest.StartDate.AddHours(-1), widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            var deviceWiseDiff = queueCountByDevice
                .SelectMany(x => x)
                .GroupBy(x => x.DeviceId)
                .SelectMany(g => g.Select((x, i) => new EventQueueAnalysis
                {
                    DeviceId = x.DeviceId,
                    DateTime = x.DateTime,
                    QueueCount = i == 0 || x.QueueCount < g.ElementAt(i - 1).QueueCount ? x.QueueCount : x.QueueCount - g.ElementAt(i - 1).QueueCount,
                })).Where(x => x.DateTime >= widgetRequest.StartDate).ToList();

            shoppingCountAnalysis = deviceWiseDiff
                .GroupBy(x => x.DateTime)
                .Select(g => new EventQueueAnalysis
                {
                    DateTime = g.Key,
                    QueueCount = g.Sum(x => x.QueueCount)
                })
                .OrderBy(x => x.DateTime)
                .ToList();

            return shoppingCountAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> ForkliftCountAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> ForkliftCountAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    foreach (var camera in zoneCameraList)
                    {
                        bool isArchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<EventQueueAnalysis> result = await _forkliftCountRepository.ForkliftCountAnalysisDataAsync(camera.DeviceId, widgetRequest.StartDate.AddHours(-1), widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isArchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            var deviceWiseDiff = queueCountByDevice
                .SelectMany(x => x)
                .GroupBy(x => x.DeviceId)
                .SelectMany(g => g.Select((x, i) => new EventQueueAnalysis
                {
                    DeviceId = x.DeviceId,
                    DateTime = x.DateTime,
                    QueueCount = i == 0 || x.QueueCount < g.ElementAt(i - 1).QueueCount ? x.QueueCount : x.QueueCount - g.ElementAt(i - 1).QueueCount,
                })).Where(x => x.DateTime >= widgetRequest.StartDate).ToList();

            ForkliftCountAnalysis = deviceWiseDiff
                .GroupBy(x => x.DateTime)
                .Select(g => new EventQueueAnalysis
                {
                    DateTime = g.Key,
                    QueueCount = g.Sum(x => x.QueueCount)
                })
                .OrderBy(x => x.DateTime)
                .ToList();

            return ForkliftCountAnalysis;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> ForkliftSpeedDetectionAnalysisDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> ForkliftSpeedDetectionAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    foreach (var camera in zoneCameraList)
                    {
                        bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.ForkliftSpeedDetectionAnalysisAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }
            ForkliftSpeedDetectionAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Sum(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return ForkliftSpeedDetectionAnalysis;
        }


        private async Task<List<PeopleVehicleCountSummary>> GetAllDataPointsAsync(
        List<PeopleCountRawDto> allDataPoints, DateTime startDate,
        DateTime endDate, int intervalMinutes, int addMinute = 10)
        {
            var results = new List<PeopleVehicleCountSummary>();
            if (allDataPoints.Count > 0)
            {
                var latestDataTime = allDataPoints.Max(x => x.CreatedOn);
                var currentIntervalStart = RoundDownTo10Minutes(startDate);
                var currentDataIndex = 0;

                int previousInCount = 0;
                int previousOutCount = 0;
                DateTime previousDate = currentIntervalStart.Date;

                while (currentIntervalStart < latestDataTime)
                {
                    var intervalEnd = currentIntervalStart.AddMinutes(addMinute);

                    // Reset for new day
                    if (currentIntervalStart.Date != previousDate)
                    {
                        previousInCount = 0;
                        previousOutCount = 0;
                        previousDate = currentIntervalStart.Date;
                    }

                    // Reset latestValidIndex for each interval
                    int latestValidIndex = -1;

                    // Find the latest data point strictly within this interval
                    while (currentDataIndex < allDataPoints.Count &&
                           allDataPoints[currentDataIndex].CreatedOn < intervalEnd)
                    {
                        // Only consider data that is >= interval start (not from previous intervals)
                        if (allDataPoints[currentDataIndex].CreatedOn >= currentIntervalStart)
                        {
                            latestValidIndex = currentDataIndex;
                        }
                        currentDataIndex++;
                    }

                    int inCount = 0, outCount = 0;
                    int deltaIn = 0, deltaOut = 0;

                    // Only use data if found in this interval (latestValidIndex != -1)
                    if (latestValidIndex != -1)
                    {
                        var latestData = allDataPoints[latestValidIndex];
                        if (latestData.VehicleLine?.Any() == true)
                        {
                            inCount = latestData.VehicleLine.Last().InCount;
                            outCount = latestData.VehicleLine.Last().OutCount;
                        }
                        else if (latestData.Lines?.Any() == true)
                        {
                            inCount = latestData.Lines.Last().InCount;
                            outCount = latestData.Lines.Last().OutCount;
                        }
                    }
                    if (latestValidIndex == -1)
                    {
                        inCount = 0;
                        outCount = 0;
                    }
                    else
                    {
                        // Calculate deltas (difference from previous counts)
                        deltaIn = inCount - previousInCount;
                        deltaOut = outCount - previousOutCount;

                        if (deltaIn <= 0)
                        {
                            deltaIn = 0;
                            deltaOut = 0;
                        }

                        // Update previous counts for next iteration
                        previousInCount = inCount;
                        previousOutCount = outCount;

                    }
                    results.Add(new PeopleVehicleCountSummary
                    {
                        Date = currentIntervalStart,
                        TotalInCount = deltaIn,
                        TotalOutCount = deltaOut,
                        HourRange = GetHourRange(currentIntervalStart, intervalMinutes)
                    });

                    currentIntervalStart = currentIntervalStart.AddMinutes(addMinute);
                }
            }

            return results;
        }


        private async Task<List<PeopleVehicleCountSummary>> GetAllDataPointsForAvgAsync(
    List<PeopleCountRawDto> allDataPoints,
    DateTime startDate,
    DateTime endDate,
    int intervalMinutes = 30)
        {
            var results = new List<PeopleVehicleCountSummary>();

            if (allDataPoints.Count > 0)
            {
                var currentIntervalStart = RoundDownTo10Minutes(startDate);

                while (currentIntervalStart < endDate)
                {
                    var intervalEnd = currentIntervalStart.AddMinutes(intervalMinutes);

                    var intervalData = allDataPoints
                        .Where(dp => dp.CreatedOn >= currentIntervalStart && dp.CreatedOn < intervalEnd)
                        .ToList();

                    int totalIn = 0;
                    int totalOut = 0;

                    foreach (var dp in intervalData)
                    {
                        var lines = dp.Lines != null && dp.Lines.Any() ? dp.Lines : dp.Lines;

                        if (lines != null)
                        {
                            totalIn += lines.Sum(l => l.InCount);
                            totalOut += lines.Sum(l => l.OutCount);
                        }
                    }

                    results.Add(new PeopleVehicleCountSummary
                    {
                        Date = currentIntervalStart,
                        TotalInCount = totalIn,
                        TotalOutCount = totalOut,
                        HourRange = GetHourRange(currentIntervalStart, intervalMinutes)
                    });

                    currentIntervalStart = currentIntervalStart.AddMinutes(intervalMinutes);
                }
            }

            return results;
        }

        private async Task<Dictionary<string, List<TimeLineChartPeopleVehicleCount>>> GetZoneSummariesForChartAsync(WidgetRequestForChart obj)
        {
            var zoneSummaries = new Dictionary<string, List<TimeLineChartPeopleVehicleCount>>();

            if (obj != null)
            {
                var zoneData = await GetAllZoneByPermission(obj);

                foreach (var zone in zoneData)
                {
                    IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                    if (zone.Id != null && zone.Id == "00")
                    {
                        zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                    }
                    else
                    {
                        zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                    }
                    if (zoneCameraList?.Any() != true) continue; // Skip if no cameras are found

                    if (!string.IsNullOrEmpty(obj.DeviceId) && obj.DeviceId != "6812332a6d517f8bfff611bb")
                    {
                        zoneCameraList = zoneCameraList.Where(x => x.DeviceId == obj.DeviceId);
                    }

                    if (zoneCameraList.Count() > 0)
                    {
                        var summaryResults = await GetSummaryDataForCamerasChart(zoneCameraList, obj.StartDate, obj.EndDate, obj.FromSummary, obj.IntervalMinutes);

                        if (summaryResults.Count() > 0)
                        {
                            var mergedByDateTime = summaryResults
                                                               .GroupBy(x => x.Date)
                                                               .Select(g => new
                                                               {
                                                                   datetime = g.Key?.ToString("yyyy-MM-dd HH:mm") ?? "",
                                                                   date = g.Key?.Date,
                                                                   time = g.Key?.ToString("HH:mm") ?? "",
                                                                   count = obj.InOutType.ToLower() == "in" ? g.Sum(x => x.TotalInCount) : g.Sum(x => x.TotalOutCount),
                                                                   hour = g.First().HourRange
                                                               })
                                                               .ToList();

                            var valuePerHourPerDay = mergedByDateTime
                                .GroupBy(x => new { x.hour, x.date })
                                .ToDictionary(
                                    g => new { g.Key.hour, g.Key.date },
                                    g => g.Sum(x => x.count)
                                );

                            var finalResult = mergedByDateTime
                                .Select(x => new TimeLineChartPeopleVehicleCount
                                {
                                    DateTime = x.datetime,
                                    Count = x.count,
                                    Hour = x.hour,
                                    Value = valuePerHourPerDay[new { x.hour, x.date }],
                                    Time = x.time,
                                })
                                .ToList();

                            zoneSummaries[zone.Id] = finalResult;
                        }
                    }
                }
            }
            return zoneSummaries;
        }
        private async Task<Dictionary<string, List<TimeLineChartPeopleVehicleCount>>> GetZoneSumForPeopleChartAsync(WidgetRequestForChart obj)
        {
            var zoneSummaries = new Dictionary<string, List<TimeLineChartPeopleVehicleCount>>();

            if (obj != null)
            {
                var zoneData = await GetAllZoneByPermission(obj);

                foreach (var zone in zoneData)
                {
                    IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                    if (zone.Id != null && zone.Id == "00")
                    {
                        zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                    }
                    else
                    {
                        zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                    }
                    if (zoneCameraList?.Any() != true) continue; // Skip if no cameras are found

                    if (!string.IsNullOrEmpty(obj.DeviceId) && obj.DeviceId != "6812332a6d517f8bfff611bb")
                    {
                        zoneCameraList = zoneCameraList.Where(x => x.DeviceId == obj.DeviceId);
                    }

                    if (zoneCameraList.Count() > 0)
                    {
                        var summaryResults = await GetSumDataForPeopleVehicleChart(zoneCameraList, obj.StartDate, obj.EndDate, obj.FromSummary, obj.IntervalMinutes);

                        if (summaryResults.Count() > 0)
                        {
                            var mergedByDateTime = summaryResults
                                                               .GroupBy(x => x.Date)
                                                               .Select(g => new
                                                               {
                                                                   datetime = g.Key?.ToString("yyyy-MM-dd HH:mm") ?? "",
                                                                   date = g.Key?.Date,
                                                                   time = g.Key?.ToString("HH:mm") ?? "",
                                                                   count = obj.InOutType.ToLower() == "in" ? g.Sum(x => x.TotalInCount) : g.Sum(x => x.TotalOutCount),
                                                                   hour = g.First().HourRange
                                                               })
                                                               .ToList();

                            var valuePerHourPerDay = mergedByDateTime
                                .GroupBy(x => new { x.hour, x.date })
                                .ToDictionary(
                                    g => new { g.Key.hour, g.Key.date },
                                    g => g.Sum(x => x.count)
                                );

                            var finalResult = mergedByDateTime
                                .Select(x => new TimeLineChartPeopleVehicleCount
                                {
                                    DateTime = x.datetime,
                                    Count = x.count,
                                    Hour = x.hour,
                                    Value = valuePerHourPerDay[new { x.hour, x.date }],
                                    Time = x.time,
                                })
                                .ToList();

                            zoneSummaries[zone.Id] = finalResult;
                        }
                    }
                }
            }
            return zoneSummaries;
        }

        private async Task<List<PeopleVehicleCountSummary>> GetSummaryDataForCamerasChart(
        IEnumerable<ZoneCamera> zoneCameraList, DateTime startDate, DateTime endDate, string fromSummary, int intervalMinutes, int addMinute = 10)
        {
            var result = new List<PeopleVehicleCountSummary>();

            // Create a list of tasks
            var tasks = zoneCameraList
                .Where(camera => camera.PeopleLineIndex.Any() || camera.VehicleLineIndex.Any())
                .Select(async camera =>
                {
                    IEnumerable<PeopleVehicleCountSummary> cameraResult = Enumerable.Empty<PeopleVehicleCountSummary>();

                    if (fromSummary.ToLower() == "people")
                    {
                        cameraResult = await _peopleCountRepository.GetHourlyLatestPeopleCountAsync(
                            camera.DeviceId, startDate, endDate, camera.PeopleLineIndex, camera.Channel, intervalMinutes);
                    }
                    else
                    {
                        bool isArchived = _archiveTimeService.CheckCollectionArchiceTime(startDate);
                        cameraResult = await _vehicleRepository.GetAllVehicleChartDataAsync(
                            camera.DeviceId, startDate, endDate, camera.VehicleLineIndex, camera.Channel, intervalMinutes, isArchived);
                    }

                    return cameraResult ?? Enumerable.Empty<PeopleVehicleCountSummary>();
                })
                .ToList();

            // Wait for all tasks
            var allResults = await Task.WhenAll(tasks);
            foreach (var cameraResult in allResults)
            {
                if (cameraResult.Any())
                {
                    result.AddRange(cameraResult);
                }
            }

            return result;
        }

        private async Task<List<PeopleVehicleCountSummary>> GetSumDataForPeopleVehicleChart(
        IEnumerable<ZoneCamera> zoneCameraList, DateTime startDate, DateTime endDate, string fromSummary, int intervalMinutes, int addMinute = 10)
        {
            var result = new List<PeopleVehicleCountSummary>();

            foreach (var camera in zoneCameraList)
            {
                if (camera.PeopleLineIndex.Count() > 0)
                {
                    IEnumerable<PeopleVehicleCountSummary> cameraResult = Enumerable.Empty<PeopleVehicleCountSummary>();

                    var allDataPoints = new List<PeopleCountRawDto>();

                    if (fromSummary.ToLower() == "people")
                    {

                        cameraResult = await _peopleCountRepository.GetPeopleBasedAsync(camera.DeviceId, startDate, endDate, camera.PeopleLineIndex, camera.Channel);
                    }
                    else
                    {
                        cameraResult = await _vehicleRepository.GetAllVehicleChartDataAsync(camera.DeviceId, startDate, endDate, camera.PeopleLineIndex, camera.Channel);

                    }

                    //if (allDataPoints.Count() > 0)
                    //{
                    //    cameraResult = await GetAllDataPointsAsync(allDataPoints, startDate, endDate, intervalMinutes, addMinute);
                    //}

                    //cameraResult = await GetHourlyLatestPeopleCountAsync(
                    //    camera.DeviceId, startDate, endDate, camera.PeopleLineIndex, camera.Channel, intervalMinutes);

                    if (cameraResult?.Any() == true)
                    {
                        result.AddRange(cameraResult);
                    }
                }
            }

            return result;
        }

        private DateTime RoundDownTo10Minutes(DateTime dt)
        {
            return new DateTime(
                dt.Year, dt.Month, dt.Day,
                dt.Hour, (dt.Minute / 10) * 10, 0);
        }
        private string GetHourRange(DateTime? date, int intervalMinutes)
        {
            if (date == null || intervalMinutes <= 0 || intervalMinutes > 60)
                return string.Empty;

            // Round down to nearest interval
            var original = date.Value;
            var minutes = (original.Minute / intervalMinutes) * intervalMinutes;
            var start = new DateTime(original.Year, original.Month, original.Day, original.Hour, 0, 0)
                        .AddMinutes(minutes);
            var end = start.AddMinutes(intervalMinutes);

            string FormatTime(DateTime dt, bool isFullHourFormat)
            {
                if (isFullHourFormat)
                {
                    int hour = dt.Hour;
                    return hour == 0 ? "12am" :
                           hour < 12 ? $"{hour}am" :
                           hour == 12 ? "12pm" :
                           $"{hour - 12}pm";
                }
                else
                {
                    return dt.ToString("h:mmtt").ToLower(); // e.g., "4:15am"
                }
            }

            bool isFullHour = intervalMinutes == 60;

            return $"{FormatTime(start, isFullHour)} to {FormatTime(end, isFullHour)}";
        }

        private async Task<Dictionary<string, List<PeopleVehicleCountSummary>>> GetZoneSummariesAsync(WidgetRequest widgetRequest, string FromSummary = "", string? deviceId = "", int? channel = 0)
        {
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            var zoneSummaries = new Dictionary<string, List<PeopleVehicleCountSummary>>();

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList?.Any() != true) continue; // Skip if no cameras are found

                if (!string.IsNullOrEmpty(deviceId))
                {
                    zoneCameraList = zoneCameraList.Where(x => x.DeviceId == deviceId && x.Channel == channel);
                }

                var result = await GetSummaryDataForCameras(zoneCameraList, widgetRequest.StartDate, widgetRequest.EndDate, FromSummary);
                if (result != null && result.Any())
                {
                    var groupedResult = result
                        .Where(x => x.Date.HasValue)
                        .GroupBy(x => x.Date.Value.Date)
                        .Select(g => new PeopleVehicleCountSummary
                        {
                            Date = g.Key,
                            TotalInCount = g.Sum(x => x.TotalInCount),
                            TotalOutCount = g.Sum(x => x.TotalOutCount)
                        })
                        .OrderBy(x => x.Date)
                        .ToList();

                    zoneSummaries[zone.Id] = groupedResult;
                }
            }
            return zoneSummaries;
        }

        private async Task<List<PeopleVehicleCountSummary>> GetSummaryDataForCameras(
        IEnumerable<ZoneCamera> zoneCameraList, DateTime startDate, DateTime endDate, string FromSummary)
        {
            var result = new List<PeopleVehicleCountSummary>();
            var timeZone = await _usersService.GetTimeZone(_currentUserService.UserId);
            foreach (var camera in zoneCameraList)
            {
                if (camera.PeopleLineIndex.Count() > 0 || camera.VehicleLineIndex.Count() > 0)
                {

                    var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
                    IEnumerable<PeopleVehicleCountSummary> cameraResult;
                    if (FromSummary == "people")
                    {
                        cameraResult = await _peopleCountRepository.GetPeopleCountMinMaxAverageAsync(
                            camera.DeviceId, startDate, endDate, offsetTimeStamp, camera.PeopleLineIndex, camera.Channel);

                    }
                    else
                    {
                        cameraResult = await _vehicleRepository.GetVehicleCountMinMaxAverageAsync(
                            camera.DeviceId, startDate, endDate, offsetTimeStamp, camera.VehicleLineIndex, camera.Channel);
                    }

                    if (cameraResult?.Any() == true)
                    {
                        result.AddRange(cameraResult);
                    }
                }
            }

            return result;
        }

        public async Task<IEnumerable<ZoneMaster>> GetAllZoneByPermission(WidgetRequest widgetRequest)
        {
            IEnumerable<ZoneMaster> zoneData = Enumerable.Empty<ZoneMaster>();
            //if (widgetRequest != null && widgetRequest.ZoneIds != null && widgetRequest.ZoneIds.Count() == 0)
            //{
            zoneData = (await _zoneRepository.GetZonesByMultipleFloorIdZoneIdAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds)).ToList();
            //}
            //else
            //{
            //}
            if (widgetRequest != null)
            {
                IEnumerable<FloorDataAccessPermission> floorZonePermissions = _permissionService.GetFloorZonePermissionByRoles(widgetRequest.userRoles);

                var linkedServerFloorId = await _floorService.GetLinkedFloorsZoneByPermissionAsync();

                if (linkedServerFloorId != null && linkedServerFloorId.Any())
                {
                    floorZonePermissions = floorZonePermissions.Union(linkedServerFloorId);
                }

                IEnumerable<string> zoneIds = floorZonePermissions.Where(y => widgetRequest.FloorIds.Any(z => z == y.FloorId)).SelectMany(x => x.ZoneIds).Distinct();
                if (widgetRequest.ZoneIds != null && widgetRequest.ZoneIds.Count() > 0)
                {
                    zoneIds = zoneIds.Where(x => widgetRequest.ZoneIds.Any(z => z == x));
                }
                zoneData = await _zoneRepository.GetManyFromLinkedServerAsync(zoneIds);
            }

            if (widgetRequest != null && widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                zoneData = zoneData.Append(new ZoneMaster
                {
                    FloorId = "Default Floor",
                    ZoneName = "Default Zone",
                    Id = "00",
                    PeopleOccupancy = 1,
                    VehicleOccupancy = 1,
                    PeopleDefaultOccupancy = 1,
                    VehicleDefaultOccupancy = 1
                });
            }

            return zoneData;
        }

        //public async Task<IEnumerable<CameraCapacityUtilizationAnalysisByZones>> PeopleCameraCapacityUtilizationAnalysisByZones(WidgetRequest widgetRequest)
        //{
        //    var sw2 = Stopwatch.StartNew();
        //    List<CameraCapacityUtilizationAnalysisByZones> peopleCameraCapacityUtilizationsByZones = new List<CameraCapacityUtilizationAnalysisByZones>();
        //    var timeZone = await _usersService.GetTimeZone(widgetRequest.UserId);
        //    var Zones = await GetAllZoneByPermission(widgetRequest);
        //    foreach (var zone in Zones)
        //    {
        //        IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
        //        if (zone.Id != null && zone.Id == "00")
        //        {
        //            zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
        //        }
        //        else
        //        {
        //            zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
        //        }
        //        if (zoneCameraList != null)
        //        {
        //            List<IEnumerable<CountAnalysisData>> cameraCapacityLst = new List<IEnumerable<CountAnalysisData>>();
        //            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
        //            foreach (var camera in zoneCameraList)
        //            {
        //                var sw3 = Stopwatch.StartNew();
        //                IEnumerable<CountAnalysisData> result = await _peopleCountRepository.PeopleCameraCapacityUtilizationAnalysisByZones(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, camera.PeopleLineIndex, widgetRequest.IntervalMinute);

        //                if (result != null)
        //                {
        //                    cameraCapacityLst.Add(result);
        //                }
        //                sw3.Stop();
        //                Console.WriteLine($"Compression: {sw3.Elapsed.TotalSeconds:F1}s");
        //            }

        //            peopleCameraCapacityUtilizationsByZones.Add(new CameraCapacityUtilizationAnalysisByZones
        //            {
        //                ZoneName = zone.ZoneName,
        //                UtilizationData = cameraCapacityLst.Where(x => x.Count() > 0).SelectMany(x => x)
        //                        .GroupBy(x => x.DateTime)
        //                        .Select(g => new CountAnalysisData
        //                        {
        //                            DateTime = g.Key,
        //                            Count = g.Sum(x => x.Count)
        //                        }).OrderBy(x => x.DateTime).ToList()
        //            });
        //        }
        //    }
        //    sw2.Stop();
        //    Console.WriteLine($"Compression: {sw2.Elapsed.TotalSeconds:F1}s");
        //    return peopleCameraCapacityUtilizationsByZones;
        //}



        public async Task<IEnumerable<CameraCapacityUtilizationAnalysisByZones>> PeopleCameraCapacityUtilizationAnalysisByZones(WidgetRequest widgetRequest)
        {
            var timeZone = await _usersService.GetTimeZone(widgetRequest.UserId);
            var zones = await GetAllZoneByPermission(widgetRequest);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);

            // Pre-allocate with known capacity
            var peopleCameraCapacityUtilizationsByZones = new List<CameraCapacityUtilizationAnalysisByZones>(zones.Count());

            // Process zones in parallel
            var zoneTasks = zones.Select(async zone =>
            {
                try
                {
                    // Get cameras for zone
                    var zoneCameraList = await GetZoneCamerasAsync(zone);
                    if (zoneCameraList == null || !zoneCameraList.Any())
                    {
                        return new CameraCapacityUtilizationAnalysisByZones
                        {
                            ZoneName = zone.ZoneName,
                            UtilizationData = new List<CountAnalysisData>()
                        };
                    }

                    // Process cameras in parallel with controlled concurrency
                    var semaphore = new SemaphoreSlim(Environment.ProcessorCount); // Limit concurrent DB calls
                    var cameraTasks = zoneCameraList.Select(async camera =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            return await _peopleCountRepository.PeopleCameraCapacityUtilizationAnalysisByZones(
                                camera.DeviceId,
                                widgetRequest.StartDate,
                                widgetRequest.EndDate,
                                camera.Channel,
                                camera.PeopleLineIndex,
                                widgetRequest.IntervalMinute);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    var cameraResults = await Task.WhenAll(cameraTasks);
                    semaphore.Dispose();

                    // Optimize aggregation using Dictionary for O(1) lookups instead of GroupBy
                    var aggregatedData = new Dictionary<DateTime, int>();

                    foreach (var result in cameraResults)
                    {
                        if (result != null)
                        {
                            foreach (var item in result)
                            {
                                if (aggregatedData.TryGetValue(item.DateTime, out var existingCount))
                                {
                                    aggregatedData[item.DateTime] = existingCount + item.Count;
                                }
                                else
                                {
                                    aggregatedData[item.DateTime] = item.Count;
                                }
                            }
                        }
                    }

                    // Convert to final format and sort
                    var utilizationData = aggregatedData
                        .Select(kvp => new CountAnalysisData { DateTime = kvp.Key, Count = kvp.Value })
                        .OrderBy(x => x.DateTime)
                        .ToList();

                    return new CameraCapacityUtilizationAnalysisByZones
                    {
                        ZoneName = zone.ZoneName,
                        UtilizationData = utilizationData
                    };
                }
                catch (Exception ex)
                {
                    // Log exception and return empty result for this zone
                    Console.WriteLine($"Error processing zone {zone.ZoneName}: {ex.Message}");
                    return new CameraCapacityUtilizationAnalysisByZones
                    {
                        ZoneName = zone.ZoneName,
                        UtilizationData = new List<CountAnalysisData>()
                    };
                }
            });

            var results = await Task.WhenAll(zoneTasks);
            peopleCameraCapacityUtilizationsByZones.AddRange(results);

            return peopleCameraCapacityUtilizationsByZones;
        }

        // Helper method to encapsulate camera retrieval logic
        private async Task<IEnumerable<ZoneCamera>> GetZoneCamerasAsync(dynamic zone)
        {
            if (zone.Id != null && zone.Id == "00")
            {
                return await _deviceMasterRepository.GetUnMappeddevicesforWidget();
            }
            else
            {
                return await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
            }
        }

        public async Task<IEnumerable<CameraCapacityUtilizationAnalysisByZones>> VehicleCameraCapacityUtilizationAnalysisByZones(WidgetRequest widgetRequest)
        {
            List<CameraCapacityUtilizationAnalysisByZones> vehicleCameraCapacityUtilizationsByZones = new List<CameraCapacityUtilizationAnalysisByZones>();
            var Zones = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in Zones)
            {
                //IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                //if (zone.Id != null && zone.Id == "00")
                //{
                //    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                //}
                //else
                //{
                //    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                //}
                //if (zoneCameraList != null)
                //{
                //    List<IEnumerable<CountAnalysisData>> cameraCapacityLst = new List<IEnumerable<CountAnalysisData>>();
                //    foreach (var camera in zoneCameraList)
                //    {
                //        IEnumerable<CountAnalysisData> result = await _vehicleRepository.VehicleCameraCapacityUtilizationAnalysisByZones(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, camera.VehicleLineIndex, widgetRequest.IntervalMinute);
                //        if (result != null)
                //        {
                //            cameraCapacityLst.Add(result);
                //        }
                //    }

                //WidgetRequestForChart widgetRequest1 = new WidgetRequestForChart
                //{
                //    StartDate = widgetRequest.StartDate.AddHours(-1),
                //    EndDate = widgetRequest.EndDate,
                //    FloorIds = [zone.FloorId],
                //    ZoneIds = [zone.Id],
                //    userRoles = widgetRequest.userRoles,
                //    UserId = widgetRequest.UserId
                //};

                var deepRequest = JsonSerializer.Deserialize<WidgetRequestForChart>(JsonSerializer.Serialize(widgetRequest));
                deepRequest.ZoneIds = [zone.Id];

                var result = await AverageVehicleCountChartAsync(deepRequest, true);
                var zoneWiseUtilization = result.Select(x => new CountAnalysisData
                {
                    DateTime = (DateTime)x.DateTime,
                    Count = x.InCount - x.OutCount,
                }).Where(x => x.DateTime >= widgetRequest.StartDate).AsEnumerable();

                vehicleCameraCapacityUtilizationsByZones.Add(new CameraCapacityUtilizationAnalysisByZones
                {
                    ZoneName = zone.ZoneName,
                    UtilizationData = zoneWiseUtilization
                });

                //vehicleCameraCapacityUtilizationsByZones.Add(new CameraCapacityUtilizationAnalysisByZones
                //{
                //    ZoneName = zone.ZoneName,
                //    UtilizationData = cameraCapacityLst.Where(x => x.Count() > 0).SelectMany(x => x)
                //                .GroupBy(x => x.DateTime)
                //                .Select(g => new CountAnalysisData
                //                {
                //                    DateTime = g.Key,
                //                    Count = g.Sum(x => x.Count)
                //                }).OrderBy(x => x.DateTime).ToList()
                //});
            }
            return vehicleCameraCapacityUtilizationsByZones;
        }

        private async Task<Dictionary<string, List<PeopleVehicleInOutAvgChartDevicewise>>> GetZoneSummariesForAvgChartAsync(WidgetRequest obj)
        {
            var zoneSummaries = new Dictionary<string, List<PeopleVehicleInOutAvgChartDevicewise>>();

            if (obj != null)
            {
                var zoneData = await GetAllZoneByPermission(obj);

                foreach (var zone in zoneData)
                {
                    IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                    if (zone.Id != null && zone.Id == "00")
                    {
                        zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                    }
                    else
                    {
                        zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                    }

                    if (zoneCameraList?.Any() != true) continue;

                    if (!string.IsNullOrEmpty(obj.DeviceId) && obj.DeviceId != "6812332a6d517f8bfff611bb")
                    {
                        zoneCameraList = zoneCameraList.Where(x => x.DeviceId == obj.DeviceId);
                    }

                    if (zoneCameraList.Count() > 0)
                    {
                        var summaryResults = await GetSummaryDataForCamerasChart(zoneCameraList, obj.StartDate, obj.EndDate, obj.FromSummary, obj.IntervalMinute);

                        if (summaryResults.Any())
                        {
                            //var mergedByDateTime = summaryResults
                            //    .GroupBy(x => x.Date)
                            //    .Select(g => new
                            //    {
                            //        datetime = g.Key?.ToString("yyyy-MM-dd HH:mm") ?? "",
                            //        date = g.Key,
                            //        time = g.Key?.ToString("HH:mm") ?? "",
                            //        inCount = g.Sum(x => x.TotalInCount),
                            //        outCount = g.Sum(x => x.TotalOutCount),
                            //        // hour = g.First().HourRange
                            //    })
                            //    .ToList();

                            //var valuePerHourPerDay = mergedByDateTime
                            //    .GroupBy(x => new { x.hour, x.date })
                            //    .ToDictionary(
                            //        g => new { g.Key.hour, g.Key.date },
                            //        g => new
                            //        {
                            //            InTotal = g.Sum(x => x.inCount),
                            //            OutTotal = g.Sum(x => x.outCount)
                            //        }
                            //    );

                            var finalResult = summaryResults
                                .Select(x => new PeopleVehicleInOutAvgChartDevicewise
                                {
                                    DeviceId = x.DeviceId,
                                    DateTime = x.Date,
                                    DateTimeCsv = x.Date.Value.ToString("yyyy-MM-dd HH:mm") ?? "",
                                    InCount = x.TotalInCount,
                                    OutCount = x.TotalOutCount,
                                    //Hour = x.hour,
                                    //InValue = valuePerHourPerDay[new { x.hour, x.date }].InTotal,
                                    //OutValue = valuePerHourPerDay[new { x.hour, x.date }].OutTotal
                                })
                                .ToList();

                            zoneSummaries[zone.Id] = finalResult;
                        }
                    }
                }
            }

            return zoneSummaries;
        }



        public async Task<IEnumerable<VehicleTurningMovementResponse>> VehicleTurningMovementAnalysisData(WidgetRequest widgetRequest)
        {
            List<VehicleTurningMovementResponse> vehicleTurningMovementAnalysis = new List<VehicleTurningMovementResponse>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            List<IEnumerable<VehicleTurningMovementResponse>> vehicleTurningMovementAnalysisByDevice = new List<IEnumerable<VehicleTurningMovementResponse>>();

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                bool isarchive = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                if (zoneCameraList != null)
                {
                    foreach (var camera in zoneCameraList)
                    {
                        IEnumerable<VehicleTurningMovementResponse> result = await _multiLaneVehicleCountRepository.VehicleTurningMovementAnalysisData(camera.DeviceId, widgetRequest.StartDate.AddHours(-1), widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchive);
                        if (result != null)
                        {
                            vehicleTurningMovementAnalysisByDevice.Add(result);
                        }
                    }
                }
            }

            vehicleTurningMovementAnalysis = vehicleTurningMovementAnalysisByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new VehicleTurningMovementResponse
                                {
                                    DateTime = g.Key,
                                    Left = g.Sum(x => x.Left),
                                    Right = g.Sum(x => x.Right),
                                    Straight = g.Sum(x => x.Straight)
                                }).OrderBy(x => x.DateTime).ToList();
            var diffData = vehicleTurningMovementAnalysis.Select((x, i) => new VehicleTurningMovementResponse
            {
                DateTime = x.DateTime,
                Left = i == 0 ? 0 : (x.Left >= vehicleTurningMovementAnalysis[i - 1].Left) ? x.Left - vehicleTurningMovementAnalysis[i - 1].Left : x.Left,
                Right = i == 0 ? 0 : (x.Right >= vehicleTurningMovementAnalysis[i - 1].Right) ? x.Right - vehicleTurningMovementAnalysis[i - 1].Right : x.Right,
                Straight = i == 0 ? 0 : (x.Straight >= vehicleTurningMovementAnalysis[i - 1].Straight) ? x.Straight - vehicleTurningMovementAnalysis[i - 1].Straight : x.Straight,
            }).ToList();

            diffData = diffData.Where(x => x.DateTime >= widgetRequest.StartDate).ToList();

            return diffData;
        }



        //public async Task<IEnumerable<PeopleVehicleInOutAvgChart>> PeopleInOutCountAnalysisV2Async(WidgetRequest widgetRequest)
        //{
        //    var zoneData = await GetAllZoneByPermission(widgetRequest);

        //    IEnumerable<ZoneCamera> zoneCameraList;

        //    if (widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
        //    {
        //        zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
        //    }
        //    else
        //    {
        //        zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneIds(zoneData.Select(x => x.Id));
        //    }

        //    if (!string.IsNullOrEmpty(widgetRequest.DeviceId) && widgetRequest.DeviceId != "6812332a6d517f8bfff611bb")
        //    {
        //        zoneCameraList = zoneCameraList.Where(x => x.DeviceId == widgetRequest.DeviceId).ToList();
        //    }

        //    var data = await _peopleCountRepository.PeopleInOutCountAnalysisV2Async(zoneCameraList, widgetRequest.StartDate, widgetRequest.EndDate);

        //    data = (from d in data
        //            join f in zoneCameraList.AsEnumerable()
        //            on new { DeviceId = d.DeviceId, ChannelNo = d.ChannelNo } equals new { DeviceId = f.DeviceId ?? "", ChannelNo = f.Channel }
        //            select d).ToList();

        //    data = data
        //        .Join(
        //            zoneCameraList,
        //            pc => pc.DeviceId,
        //            zc => zc.DeviceId,
        //            (pc, zc) => new { PeopleCount = pc, ZoneCamera = zc }
        //        )
        //        .Select(x => new PeopleCount
        //        {
        //            CreatedOn = GetBucketTime(x.PeopleCount.CreatedOn.Value, widgetRequest.IntervalMinute),
        //            DeviceId = x.PeopleCount.DeviceId,
        //            CameraIP = x.PeopleCount.CameraIP,
        //            ChannelNo = x.PeopleCount.ChannelNo,
        //            Lines = x.PeopleCount.Lines
        //                        .Where(l => x.ZoneCamera.PeopleLineIndex.Contains(l.LineIndex))
        //                        .ToList()
        //        })
        //        .Where(pc => pc.Lines.Any())
        //        .ToList();

        //    var deviceWiseData = data
        //        .GroupBy(pc => new
        //        {
        //            pc.DeviceId,
        //            CreatedOn = pc.CreatedOn
        //        }).Select(d => new PeopleVehicleInOutAvgChart
        //        {
        //            DateTime = d.Key.CreatedOn,
        //            InCount = d.Max(x => x.Lines.Sum(l => l.InCount)),
        //            OutCount = d.Max(x => x.Lines.Sum(l => l.OutCount)),
        //        });



        //    var result = deviceWiseData.GroupBy(x => x.DateTime)
        //        .Select(g => new PeopleVehicleInOutAvgChart
        //        {
        //            DateTime = g.Key,
        //            InCount = g.Sum(p => p.InCount),
        //            OutCount = g.Sum(p => p.OutCount),
        //        });

        //    return result;
        //}

        DateTime GetBucketTime(DateTime dt, int bucketSize = 10)
        {
            int bucketMinute = (dt.Minute / bucketSize) * bucketSize;
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, bucketMinute, 0);
        }


        public async Task<List<ChartAvgInOut>> NewVsTotalVisitorChartAsync1(WidgetRequest widgetRequest)
        {
            var zoneSummaries = await GetZoneSummariesForNewVsTotalChartAsync(widgetRequest);

            NewVsTotalVisitorCountWidget obj = new NewVsTotalVisitorCountWidget();

            // Step 1: Merge and group by exact DateTime (to remove duplicates)
            var allChartData = zoneSummaries
                .SelectMany(z => z.Value)
                .GroupBy(x => x.DateTime)
                .Select(g => new PeopleVehicleInOutAvgChart
                {
                    DateTime = g.Key,
                    InCount = g.Sum(x => x.NewInCount),
                    OutCount = g.Sum(x => x.TotalCount),
                    Hour = g.First().Hour
                }).ToList();

            var finalResult = allChartData.Select(x => new ChartAvgInOut
            {
                DateTime = x.DateTime,
                NewVisitor = x.InCount,
                TotalVisitor = x.OutCount
            }).ToList();

            return finalResult;


            //var finalResult = new List<ChartAvgInOut> { peopleInData, peopleOutData };
            //return finalResult;
        }
        private async Task<Dictionary<string, List<NewVsTotalVisitorChart>>> GetZoneSummariesForNewVsTotalChartAsync(WidgetRequest obj)
        {
            var zoneSummaries = new Dictionary<string, List<NewVsTotalVisitorChart>>();

            if (obj != null)
            {
                var zoneData = await GetAllZoneByPermission(obj);

                foreach (var zone in zoneData)
                {
                    IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                    if (zone.Id != null && zone.Id == "00")
                    {
                        zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                    }
                    else
                    {
                        zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                    }
                    if (zoneCameraList?.Any() != true) continue;

                    var summaryResults = await GetSummaryDataForCamerasChart(zoneCameraList, obj.StartDate, obj.EndDate, "people", obj.IntervalMinute);

                    if (summaryResults.Any())
                    {
                        var mergedByDateTime = summaryResults
                            .GroupBy(x => x.Date)
                            .Select(g => new
                            {
                                datetime = g.Key?.ToString("yyyy-MM-dd HH:mm") ?? "",
                                date = g.Key,
                                time = g.Key?.ToString("HH:mm") ?? "",
                                inCount = g.Sum(x => x.TotalInCount),
                                outCount = g.Sum(x => x.TotalOutCount),
                                hour = g.First().HourRange
                            })
                            .ToList();

                        //var valuePerHourPerDay = mergedByDateTime
                        //    .GroupBy(x => new { x.hour, x.date })
                        //    .ToDictionary(
                        //        g => new { g.Key.hour, g.Key.date },
                        //        g => new
                        //        {
                        //            InTotal = g.Sum(x => x.inCount),
                        //            OutTotal = g.Sum(x => x.outCount)
                        //        }
                        //    );

                        var finalResult = mergedByDateTime
                            .Select(x => new NewVsTotalVisitorChart
                            {
                                DateTime = x.date,
                                DateTimeCsv = x.datetime,
                                NewInCount = x.inCount,
                                TotalCount = x.inCount + Convert.ToInt32(zone.PeopleDefaultOccupancy),
                                Hour = x.hour,
                                //InValue = valuePerHourPerDay[new { x.hour, x.date }].InTotal,
                            })
                            .ToList();

                        zoneSummaries[zone.Id] = finalResult;
                    }
                }
            }

            return zoneSummaries;
        }

        public async Task<IEnumerable<VehicleByTypeChartResponse>> VehicleByTypeLineChartData(WidgetRequest widgetRequest)
        {
            var zoneSummaries = await GetZoneSummariesForVehicleChartAsync(widgetRequest);

            VehicleByTypeChartResponse obj = new VehicleByTypeChartResponse();

            // Step 1: Merge and group by exact DateTime (to remove duplicates)
            var allChartData = zoneSummaries
                .SelectMany(z => z.Value)
                .GroupBy(x => x.DateTime)
                .Select(g => new VehicleByTypeChartResponse
                {
                    DateTime = g.Key,
                    TruckInCount = g.Sum(x => x.TruckInCount),
                    MotorCycleInCount = g.Sum(x => x.MotorCycleInCount),
                    BusInCount = g.Sum(x => x.BusInCount),
                    BicycleInCount = g.Sum(x => x.BicycleInCount),
                    CarInCount = g.Sum(x => x.CarInCount),
                    TruckOutCount = g.Sum(x => x.TruckOutCount),
                    MotorCycleOutCount = g.Sum(x => x.MotorCycleOutCount),
                    BusOutCount = g.Sum(x => x.BusOutCount),
                    BicycleOutCount = g.Sum(x => x.BicycleOutCount),
                    CarOutCount = g.Sum(x => x.CarOutCount),
                }).ToList();


            var vehicleByType = allChartData.Select(item => new VehicleByTypeChartResponse
            {
                TruckInCount = item.TruckInCount,
                MotorCycleInCount = item.MotorCycleInCount,
                BusInCount = item.BusInCount,
                BicycleInCount = item.BicycleInCount,
                CarInCount = item.CarInCount,
                TruckOutCount = item.TruckOutCount,
                MotorCycleOutCount = item.MotorCycleOutCount,
                BusOutCount = item.BusOutCount,
                BicycleOutCount = item.BicycleOutCount,
                CarOutCount = item.CarOutCount,
                DateTime = item.DateTime
            }).ToList();

            return vehicleByType;
        }

        private async Task<Dictionary<string, List<VehicleByTypeChartWidgetDto>>> GetZoneSummariesForVehicleChartAsync(WidgetRequest obj)
        {
            var zoneSummaries = new Dictionary<string, List<VehicleByTypeChartWidgetDto>>();

            if (obj != null)
            {
                var zoneData = await GetAllZoneByPermission(obj);

                foreach (var zone in zoneData)
                {
                    IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                    if (zone.Id != null && zone.Id == "00")
                    {
                        zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                    }
                    else
                    {
                        zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                    }
                    if (zoneCameraList?.Any() != true) continue;

                    var summaryResults = await GetVehicleByTypeLineChartSummary(zoneCameraList, obj.StartDate.AddHours(-1), obj.EndDate, "vehicle", obj.IntervalMinute, obj.IntervalMinute);

                    if (summaryResults.Any())
                    {
                        var deviceWiseDiff = summaryResults
                        .GroupBy(x => x.DeviceId)
                        .SelectMany(g => g.Select((x, i) => new VehicleByTypeChartSummary
                        {
                            DeviceId = x.DeviceId,
                            Date = x.Date,
                            BicycleInCount = i == 0 || x.BicycleInCount < g.ElementAt(i - 1).BicycleInCount ? x.BicycleInCount : x.BicycleInCount - g.ElementAt(i - 1).BicycleInCount,
                            BusInCount = i == 0 || x.BusInCount < g.ElementAt(i - 1).BusInCount ? x.BusInCount : x.BusInCount - g.ElementAt(i - 1).BusInCount,
                            MotorCycleInCount = i == 0 || x.MotorCycleInCount < g.ElementAt(i - 1).MotorCycleInCount ? x.MotorCycleInCount : x.MotorCycleInCount - g.ElementAt(i - 1).MotorCycleInCount,
                            CarInCount = i == 0 || x.CarInCount < g.ElementAt(i - 1).CarInCount ? x.CarInCount : x.CarInCount - g.ElementAt(i - 1).CarInCount,
                            TruckInCount = i == 0 || x.TruckInCount < g.ElementAt(i - 1).TruckInCount ? x.TruckInCount : x.TruckInCount - g.ElementAt(i - 1).TruckInCount,
                            BicycleOutCount = i == 0 || x.BicycleOutCount < g.ElementAt(i - 1).BicycleOutCount ? x.BicycleOutCount : x.BicycleOutCount - g.ElementAt(i - 1).BicycleOutCount,
                            BusOutCount = i == 0 || x.BusOutCount < g.ElementAt(i - 1).BusOutCount ? x.BusOutCount : x.BusOutCount - g.ElementAt(i - 1).BusOutCount,
                            CarOutCount = i == 0 || x.CarOutCount < g.ElementAt(i - 1).CarOutCount ? x.CarOutCount : x.CarOutCount - g.ElementAt(i - 1).CarOutCount,
                            MotorCycleOutCount = i == 0 || x.MotorCycleOutCount < g.ElementAt(i - 1).MotorCycleOutCount ? x.MotorCycleOutCount : x.MotorCycleOutCount - g.ElementAt(i - 1).MotorCycleOutCount,
                            TruckOutCount = i == 0 || x.TruckOutCount < g.ElementAt(i - 1).TruckOutCount ? x.TruckOutCount : x.TruckOutCount - g.ElementAt(i - 1).TruckOutCount,
                        })

                        ).Where(x => x.Date >= obj.StartDate).ToList();

                        var mergedByDateTime = deviceWiseDiff
                            .GroupBy(x => new
                            {
                                x.DeviceId,
                                CreatedOn = x.Date
                            })

                            .Select(g => new
                            {
                                date = g.Key?.CreatedOn,
                                carInCount = g.Sum(x => x.CarInCount),
                                bicycleInCount = g.Sum(x => x.BicycleInCount),
                                busInCount = g.Sum(x => x.BusInCount),
                                truckInCount = g.Sum(x => x.TruckInCount),
                                motorCycleInCount = g.Sum(x => x.MotorCycleInCount),
                                CarOutCount = g.Sum(x => x.CarOutCount),
                                BicycleOutCount = g.Sum(x => x.BicycleOutCount),
                                BusOutCount = g.Sum(x => x.BusOutCount),
                                TruckOutCount = g.Sum(x => x.TruckOutCount),
                                MotorCycleOutCount = g.Sum(x => x.MotorCycleOutCount),
                                hour = g.First().HourRange
                            })
                            .ToList();

                        var finalResult = mergedByDateTime
                            .Select(x => new VehicleByTypeChartWidgetDto
                            {
                                DateTime = x.date,
                                CarInCount = x.carInCount,
                                BicycleInCount = x.bicycleInCount,
                                BusInCount = x.busInCount,
                                TruckInCount = x.truckInCount,
                                MotorCycleInCount = x.motorCycleInCount,
                                CarOutCount = x.CarOutCount,
                                BicycleOutCount = x.BicycleOutCount,
                                BusOutCount = x.BusOutCount,
                                TruckOutCount = x.TruckOutCount,
                                MotorCycleOutCount = x.MotorCycleOutCount,
                            })
                            .ToList();

                        zoneSummaries[zone.Id] = finalResult;
                    }
                }
            }

            return zoneSummaries;
        }

        private async Task<List<VehicleByTypeChartSummary>> GetVehicleByTypeLineChartSummary(
IEnumerable<ZoneCamera> zoneCameraList, DateTime startDate, DateTime endDate, string fromSummary, int intervalMinutes, int addMinute)
        {
            var result = new List<VehicleByTypeChartSummary>();

            foreach (var camera in zoneCameraList)
            {
                //if (camera.PeopleLineIndex.Count() > 0)
                {
                    IEnumerable<VehicleByTypeChartSummary> cameraResult = Enumerable.Empty<VehicleByTypeChartSummary>();

                    var allDataPoints = new List<PeopleVehicleCountSummary>();
                    bool isarchive = _archiveTimeService.CheckCollectionArchiceTime(startDate);
                    cameraResult = await _vehicleRepository.VehicleByTypeLineChartAsync(camera.DeviceId, startDate, endDate, isarchive, camera.VehicleLineIndex, camera.Channel, intervalMinutes);

                    foreach (var x in cameraResult)
                    {
                        x.DeviceId = camera.DeviceId;
                    }
                    //if (allDataPoints.Count() > 0)
                    //{
                    //    cameraResult = await GetAllVehicleDataPointsAsync(allDataPoints, startDate, endDate, intervalMinutes, addMinute);
                    //}

                    //cameraResult = await GetHourlyLatestPeopleCountAsync(
                    //    camera.DeviceId, startDate, endDate, camera.PeopleLineIndex, camera.Channel, intervalMinutes);

                    if (cameraResult?.Any() == true)
                    {
                        result.AddRange(cameraResult);
                    }
                }
            }

            return result;
        }

        private async Task<List<VehicleByTypeChartSummary>> GetAllVehicleDataPointsAsync(
         List<PeopleCountRawDto> allDataPoints, DateTime startDate, DateTime endDate,
         int intervalMinutes, int addMinute)
        {
            var results = new List<VehicleByTypeChartSummary>();
            if (allDataPoints.Count > 0)
            {
                var currentIntervalStart = RoundDownTo10Minutes(startDate);
                var currentDataIndex = 0;
                int latestValidIndex = -1;

                while (startDate < endDate)
                {
                    var intervalEnd = startDate.AddMinutes(addMinute);

                    // Loop through data within this interval
                    while (currentDataIndex < allDataPoints.Count &&
                           allDataPoints[currentDataIndex].CreatedOn < intervalEnd)
                    {
                        latestValidIndex = currentDataIndex;
                        currentDataIndex++;
                    }

                    int carCount = 0, truckCount = 0, bicycleCount = 0, motorcycleCount = 0, busCount = 0;

                    if (latestValidIndex != -1)
                    {
                        var latestData = allDataPoints[latestValidIndex];

                        if (latestData.VehicleLine != null && latestData.VehicleLine.Any())
                        {
                            carCount = latestData.VehicleLine.Last().In.Car;
                            truckCount = latestData.VehicleLine.Last().In.Truck;
                            bicycleCount = latestData.VehicleLine.Last().In.Bicycle;
                            motorcycleCount = latestData.VehicleLine.Last().In.Motorcycle;
                            busCount = latestData.VehicleLine.Last().In.Bus;
                        }

                    }
                    results.Add(new VehicleByTypeChartSummary
                    {
                        Date = startDate,
                        CarInCount = carCount,
                        TruckInCount = truckCount,
                        BicycleInCount = bicycleCount,
                        MotorCycleInCount = motorcycleCount,
                        BusInCount = busCount,
                    });


                    startDate = startDate.AddMinutes(addMinute);
                }
            }

            return results;
        }

        public async Task<IEnumerable<DeviceData>> GetDeviceByZone(WidgetRequestDevice obj)
        {
            var zoneData = await GetAllZoneForDeivceByPermission(obj);
            var allDeviceDataList = new List<DeviceData>();

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList?.Any() != true) continue;

                if (zoneCameraList != null)
                {
                    var deviceIds = zoneCameraList.Select(x => x.DeviceId);
                    var getAllDeviceData = await _deviceMasterRepository.GetAllDeviceByDeviceIds(deviceIds);
                    if (getAllDeviceData != null)
                    {
                        var deviceDataList = getAllDeviceData.Select(device => new DeviceData
                        {
                            DeviceId = device.Id,
                            DeviceName = device.DeviceName
                        }).ToList();

                        allDeviceDataList.AddRange(deviceDataList);
                    }
                }
            }

            return allDeviceDataList;
        }

        public async Task<List<PeopleVehicleInOutAvgChart>> PeopleInOutCountAnalysisAsync(WidgetRequest widgetRequest)
        {
            List<PeopleVehicleInOutAvgChart> peopleInOutCountAnalysis = new List<PeopleVehicleInOutAvgChart>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            List<IEnumerable<PeopleVehicleInOutAvgChart>> peopleInOutCountAnalysisByDevice = new List<IEnumerable<PeopleVehicleInOutAvgChart>>();
            List<Task<IEnumerable<PeopleVehicleInOutAvgChart>>> cameraTaskList = new List<Task<IEnumerable<PeopleVehicleInOutAvgChart>>>();
            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }

                if (!string.IsNullOrEmpty(widgetRequest.DeviceId) && widgetRequest.DeviceId != "6812332a6d517f8bfff611bb")
                {
                    zoneCameraList = zoneCameraList.Where(x => x.DeviceId == widgetRequest.DeviceId);
                }
                if (zoneCameraList != null)
                {
                    var zoneTaskList = zoneCameraList
                            .Select(camera => _peopleCountRepository.PeopleInOutCountAnalysisAsync(
                                camera.DeviceId,
                                widgetRequest.StartDate,
                                widgetRequest.EndDate,
                                camera.Channel,
                                camera.PeopleLineIndex,
                                widgetRequest.IntervalMinute))
                            .ToList();
                    cameraTaskList.AddRange(zoneTaskList);
                }
            }

            var results = await Task.WhenAll(cameraTaskList);

            peopleInOutCountAnalysisByDevice.AddRange(results.Where(r => r != null));

            peopleInOutCountAnalysis = peopleInOutCountAnalysisByDevice.SelectMany(x => x)
                        .GroupBy(x => x.DateTime)
                        .Select(g => new PeopleVehicleInOutAvgChart
                        {
                            DateTime = g.Key,
                            InCount = g.Sum(x => x.InCount),
                            OutCount = g.Sum(x => x.OutCount),
                            DateTimeCsv = g.Key.ToString()
                        }).OrderBy(x => x.DateTime).ToList();

            return peopleInOutCountAnalysis;
        }

        public async Task<List<PeopleVehicleInOutAvgChart>> AverageVehicleCountChartAsync(WidgetRequestForChart widgetRequestForChart, bool isCumulative = false)
        {
            widgetRequestForChart.StartDate = widgetRequestForChart.StartDate.AddHours(-1);
            var zoneSummaries = await GetZoneSummariesForAvgChartAsync(widgetRequestForChart);
            widgetRequestForChart.StartDate = widgetRequestForChart.StartDate.AddHours(1);

            if (isCumulative)
            {
                var allChartData = zoneSummaries
                .SelectMany(z => z.Value)
                .GroupBy(x => x.DateTime)
                .Select(g => new PeopleVehicleInOutAvgChart
                {
                    DateTime = g.Key,
                    InCount = g.Sum(x => x.InCount),
                    OutCount = g.Sum(x => x.OutCount),
                    Hour = g.First().Hour
                }).OrderBy(x => x.DateTime).ToList();

                if (isCumulative)
                    return allChartData.Where(x => x.DateTime >= widgetRequestForChart.StartDate).ToList();
            }

            var deviceWiseDiff = zoneSummaries
                .SelectMany(x => x.Value)
            .GroupBy(x => x.DeviceId)
            .SelectMany(g =>
                g.Select((x, i) => new PeopleVehicleInOutAvgChartDevicewise
                {
                    DeviceId = x.DeviceId,
                    DateTime = x.DateTime,
                    InCount = i == 0 || x.InCount < g.ElementAt(i - 1).InCount ? x.InCount :
                              x.InCount - g.ElementAt(i - 1).InCount,
                    OutCount = i == 0 || x.OutCount < g.ElementAt(i - 1).OutCount ? x.OutCount :
                               x.OutCount - g.ElementAt(i - 1).OutCount
                })
            ).Where(x => x.DateTime >= widgetRequestForChart.StartDate).ToList();

            var result = deviceWiseDiff
                .GroupBy(x => x.DateTime)
                .Select(g => new PeopleVehicleInOutAvgChart
                {
                    DateTime = g.Key,
                    InCount = g.Sum(x => x.InCount),
                    OutCount = g.Sum(x => x.OutCount)
                })
                .OrderBy(x => x.DateTime)
                .ToList();
            return result;

            //var diffData = allChartData
            //                .Select((x, i) => new PeopleVehicleInOutAvgChart
            //                {
            //                    DateTime = x.DateTime,
            //                    DateTimeCsv = x.DateTimeCsv,
            //                    InCount = i == 0 ? x.InCount : (x.InCount >= allChartData[i - 1].InCount) ? x.InCount - allChartData[i - 1].InCount : x.InCount,
            //                    OutCount = i == 0 ? x.OutCount : (x.OutCount >= allChartData[i - 1].OutCount) ? x.OutCount - allChartData[i - 1].OutCount : x.OutCount,
            //                    Hour = x.Hour
            //                })
            //                .Where(x => x.InCount >= 0 && x.DateTime >= widgetRequestForChart.StartDate.AddHours(1)).ToList();
            //return diffData;


            // Step 2: Compute Value (sum of InCount and OutCount for same hour + same date)
            //var valueMap = allChartData
            //    .GroupBy(x =>
            //    {
            //        var dt = DateTime.Parse(x.DateTime);
            //        return new { x.Hour, Date = dt.Date };
            //    })
            //    .ToDictionary(
            //        g => new { g.Key.Hour, g.Key.Date },
            //        g => new
            //        {
            //            InValue = g.Sum(x => x.InCount),
            //            OutValue = g.Sum(x => x.OutCount)
            //        }
            //    );

            //// Step 3: Assign Value from valueMap
            //foreach (var item in allChartData)
            //{
            //    var dt = DateTime.Parse(item.DateTime);
            //    var values = valueMap[new { item.Hour, Date = dt.Date }];
            //    item.InValue = values.InValue;
            //    item.OutValue = values.OutValue;
            //}

            //var peopleInData = new ChartAvgInOut
            //{
            //    Name = "VehicleInData",
            //    Values = allChartData.Select(x => new ChartPoint
            //    {
            //        Date = x.DateTime,
            //        //  X = x.Hour,
            //        Y = x.InCount
            //    }).ToList()
            //};

            //var peopleOutData = new ChartAvgInOut
            //{
            //    Name = "VehicleOutData",
            //    Values = allChartData.Select(x => new ChartPoint
            //    {
            //        Date = x.DateTime,
            //        // X = x.Hour,
            //        Y = x.OutCount
            //    }).ToList()
            //};

            //var finalResult = new List<ChartAvgInOut> { peopleInData, peopleOutData };
            //return finalResult;

        }

        private async Task<IEnumerable<ZoneMaster>> GetAllZoneForDeivceByPermission(WidgetRequestDevice widgetRequest)
        {
            IEnumerable<ZoneMaster> zoneData = Enumerable.Empty<ZoneMaster>();
            if (widgetRequest != null && widgetRequest.ZoneIds != null && widgetRequest.ZoneIds.Count() == 0)
            {
                zoneData = (await _zoneRepository.GetZonesByMultipleFloorIdZoneIdAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds)).ToList();
            }
            else
            {
                if (widgetRequest != null)
                {
                    IEnumerable<FloorDataAccessPermission> floorZonePermissions = _permissionService.GetFloorZonePermissionByRoles(widgetRequest.userRoles);
                    IEnumerable<string> zoneIds = floorZonePermissions.Where(y => widgetRequest.FloorIds.Any(z => z == y.FloorId)).SelectMany(x => x.ZoneIds).Distinct();
                    zoneData = await _zoneRepository.GetManyAsync(zoneIds);
                }
            }
            return zoneData;
        }

        public async Task<IEnumerable<EventQueueAnalysis>> SpeedDetectionByVehicleDataAsync(WidgetRequest widgetRequest)
        {
            List<EventQueueAnalysis> PedestrianQueueAnalysis = new List<EventQueueAnalysis>();
            List<IEnumerable<EventQueueAnalysis>> queueCountByDevice = new List<IEnumerable<EventQueueAnalysis>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }
                if (zoneCameraList != null)
                {
                    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                    foreach (var camera in zoneCameraList)
                    {
                        bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
                        IEnumerable<EventQueueAnalysis> result = await _deviceEventsRepository.SpeedDetectionByVehicleAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, widgetRequest.IntervalMinute, isarchived);
                        if (result != null)
                        {
                            queueCountByDevice.Add(result);
                        }
                    }
                }
            }

            PedestrianQueueAnalysis = queueCountByDevice.SelectMany(x => x)
                                .GroupBy(x => x.DateTime)
                                .Select(g => new EventQueueAnalysis
                                {
                                    DateTime = g.Key,
                                    QueueCount = g.Sum(x => x.QueueCount)
                                }).OrderBy(x => x.DateTime).ToList();

            return PedestrianQueueAnalysis;
        }

        public async Task<StringBuilder> PeopleInOutCountCSVDownload(WidgetRequest widgetRequest)
        {
            //var zoneSummaries = await _peopleWidgetService.PeopleInOutCountAnalysisV2Async(widgetRequest);

            //// Step 1: Merge and group by exact DateTime (to remove duplicates)
            //var allChartData = zoneSummaries
            //    .GroupBy(x => x.DateTime)
            //    .Select(g => new CsvInOutResponseModel
            //    {
            //        DateTime = g.Key,  // <-- FIX: Use g.Key, not g.DateTime
            //        InCount = g.Sum(x => x.InCount),
            //        OutCount = g.Sum(x => x.OutCount),
            //    }).ToList();

            //var csvBuilder = await GenerateCSVForPeopleInOut(allChartData, widgetRequest?.WidgetName, widgetRequest);

            //return csvBuilder;

            var zoneSummaries = await _peopleWidgetService.PeopleInOutCountAnalysisV2Async(widgetRequest);

            var allChartData = new List<CsvInOutResponseModel>();
            if (widgetRequest.calculatorExportRule == null || widgetRequest.calculatorExportRule.Count() == 0)
            {
                allChartData = zoneSummaries
                .GroupBy(x => x.DateTime)
                .Select(g => new CsvInOutResponseModel
                {
                    DateTime = g.Key,  // <-- FIX: Use g.Key, not g.DateTimes
                    InCount = g.Sum(x => x.InCount),
                    OutCount = g.Sum(x => x.OutCount),
                }).ToList();
            }
            else
            {

                // Step 1: Merge and group by exact DateTime (to remove duplicates)
                allChartData = zoneSummaries
                    .GroupBy(x => x.DateTime)
                    .Select(g => new CsvInOutResponseModel
                    {
                        DateTime = g.Key,
                        InCount = CalculateInOutCount(g.Sum(x => x.InCount), widgetRequest?.calculatorExportRule),
                        OutCount = CalculateInOutCount(g.Sum(x => x.OutCount), widgetRequest?.calculatorExportRule),
                        //OutCount = g.Sum(x => x.OutCount),
                    }).ToList();

            }
            var csvBuilder = await GenerateCSVForPeopleInOut(allChartData, widgetRequest?.WidgetName, widgetRequest);
            return csvBuilder;
        }

        private double CalculateInOutCount(double originalInCount, CalculatorExportRule[] pricingRules)
        {
            if (pricingRules == null || pricingRules.Length == 0)
            {
                return originalInCount;
            }

            double currentValue = originalInCount;

            foreach (var rule in pricingRules)
            {
                if (rule == null ||
                    string.IsNullOrWhiteSpace(rule.Unit) ||
                    string.IsNullOrWhiteSpace(rule.Operation))
                {
                    continue;
                }

                int numericValue = rule.Value;

                double percentageAmount = 0;

                // ✅ Always calculate percentage from ORIGINAL value
                if (rule.Unit.Equals("Percentage", StringComparison.OrdinalIgnoreCase))
                {
                    percentageAmount = (originalInCount * numericValue) / 100;
                }
                else
                {
                    continue;
                }

                if (rule.Operation.Equals("Add", StringComparison.OrdinalIgnoreCase))
                {
                    currentValue += percentageAmount;
                }
                else if (rule.Operation.Equals("Subtract", StringComparison.OrdinalIgnoreCase))
                {
                    currentValue -= percentageAmount;
                }

                currentValue = currentValue < 0 ? 0 : currentValue;
            }

            return currentValue;
        }

        private double CalculateInOutCount1(double originalInCount, CalculatorExportRule[] pricingRules)
        {
            // Handle null or empty array
            if (pricingRules == null || pricingRules.Length == 0)
            {
                return originalInCount;
            }

            double currentValue = originalInCount;

            // Apply each pricing rule sequentially
            foreach (var rule in pricingRules)
            {
                double percentageDecrease = 0;
                // Skip invalid rules
                if (rule == null ||
                    string.IsNullOrWhiteSpace(rule.Unit) ||
                    string.IsNullOrWhiteSpace(rule.Operation))
                {
                    continue;
                }

                int numericValue = rule.Value;
                double baseValue = currentValue;

                // Step 1: Calculate base value based on Unit
                if (rule.Unit.Equals("Percentage", StringComparison.OrdinalIgnoreCase))
                {
                    // Decrease current value by NumericValue percent
                    // Example: currentValue = 100, NumericValue = 10 → baseValue = 90
                    percentageDecrease = (currentValue * numericValue) / 100;
                    //baseValue = currentValue - percentageDecrease;
                }
                else if (rule.Unit.Equals("Number", StringComparison.OrdinalIgnoreCase))
                {
                    // Use current value as base value (no percentage calculation)
                    baseValue = currentValue;
                }
                else
                {
                    // Invalid Unit, skip this rule
                    continue;
                }

                // Step 2: Apply Operation on base value
                if (rule.Operation.Equals("Add", StringComparison.OrdinalIgnoreCase))
                {
                    currentValue = baseValue + percentageDecrease;
                }
                else if (rule.Operation.Equals("Subtract", StringComparison.OrdinalIgnoreCase))
                {
                    currentValue = baseValue - percentageDecrease;
                }
                else
                {
                    // Invalid Operation, use base value
                    currentValue = baseValue;
                }

                // Ensure non-negative results after each rule application
                currentValue = currentValue < 0 ? 0 : currentValue;
            }

            return currentValue;
        }

        public async Task<StringBuilder> AveragePeopleCountCSVDownload(WidgetRequest widgetRequest)
        {
            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

            //var tenMinuteData = await PeopleInOutCountAnalysisAsync(widgetRequest);

            var tenMinuteData = await _peopleWidgetService.PeopleInOutCountAnalysisV2Async(widgetRequest);

            // Apply offset to each record
            var dataWithOffset = tenMinuteData.Select(x => new AvgPeopleInOutAvgChart
            {
                DateTime = x.DateTime.Value.Add(offsetTimeStamp),
                InCount = (double)x.InCount,
                OutCount = (double)x.OutCount,
                DateTimeCsv = x.DateTime.Value.Add(offsetTimeStamp).ToString()
            }).ToList();

            // Group by hour and calculate averages
            var hourlyAverages = dataWithOffset
                .GroupBy(x => new DateTime(x.DateTime.Value.Year, x.DateTime.Value.Month, x.DateTime.Value.Day, x.DateTime.Value.Hour, 0, 0, x.DateTime.Value.Kind))
                .Select(g => new AvgPeopleInOutAvgChart
                {
                    DateTime = g.Key,
                    DateTimeCsv = g.Key.ToString("dd/MM/yyyy h:mm:ss tt"),
                    InCount = Math.Round(g.Average(x => x.InCount), 2),
                    OutCount = Math.Round(g.Average(x => x.OutCount), 2),
                })
                .OrderBy(x => x.DateTime)
                .ToList();


            // Step 1: Merge and group by exact DateTime (to remove duplicates)
            var allChartData = hourlyAverages
                .GroupBy(x => x.DateTime)
                .Select(g => new CsvInOutResponseModel
                {
                    DateTime = g.Key.Value,//g.Key.Value,
                    InCount = g.Average(x => x.InCount),
                    OutCount = g.Average(x => x.OutCount),
                }).ToList();

            var csvBuilder = await GenerateAverageVehicleCSV(allChartData, widgetRequest.WidgetName, widgetRequest);

            return csvBuilder;
        }

        public async Task<StringBuilder> CumulativePeopleCountCSVDownload(WidgetRequest widgetRequest)
        {
            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

            //var tenMinuteData = await PeopleInOutCountAnalysisAsync(widgetRequest);
            var tenMinuteData = await _peopleWidgetService.PeopleInOutCountAnalysisV2Async(widgetRequest);

            var allChartData = tenMinuteData
                .GroupBy(x => x.DateTime)
                .Select(g => new CsvResponseModel
                {
                    DateTime = GetBucketTime(g.Key.Value.Add(offsetTimeStamp), widgetRequest.IntervalMinute).ToString("dd/MM/yyyy h:mm:ss tt"),
                    Value = g.Sum(x => x.InCount),
                }).ToList();

            //// Apply offset to each record
            //var dataWithOffset = tenMinuteData.Select(x => new PeopleVehicleInOutAvgChart
            //{
            //    DateTime = x.DateTime.Value.Add(offsetTimeStamp),
            //    InCount = x.InCount,
            //    OutCount = x.OutCount,
            //    DateTimeCsv = x.DateTime.Value.Add(offsetTimeStamp).ToString()
            //}).ToList();

            //// Group by hour and calculate max
            //var hourlyMax = dataWithOffset
            //    .GroupBy(x => new DateTime(x.DateTime.Value.Year, x.DateTime.Value.Month, x.DateTime.Value.Day, x.DateTime.Value.Hour, 0, 0, x.DateTime.Value.Kind))
            //    .Select(g => new PeopleVehicleInOutAvgChart
            //    {
            //        DateTime = g.Key,
            //        DateTimeCsv = g.Key.ToString("dd/MM/yyyy h:mm:ss tt"),
            //        InCount = g.Max(x => x.InCount),
            //        OutCount = g.Max(x => x.OutCount),
            //    })
            //    .OrderBy(x => x.DateTime)
            //    .ToList();


            //// Step 1: Merge and group by exact DateTime (to remove duplicates)
            //var allChartData = hourlyMax
            //    .GroupBy(x => x.DateTimeCsv)
            //    .Select(g => new CsvResponseModel
            //    {
            //        DateTime = g.Key,
            //        Value = (int)Math.Abs(g.Max(x => x.InCount)),
            //    }).ToList();

            var csvBuilder = await GenerateCumulativePeopleCSV(allChartData, widgetRequest.WidgetName, widgetRequest);

            return csvBuilder;
        }

        public async Task<StringBuilder> AvgVehicleCountCSVDownload(WidgetRequest widgetRequest)
        {
            widgetRequest.StartDate = widgetRequest.StartDate.AddHours(-1);
            int IntervalMinute = widgetRequest.IntervalMinute;
            widgetRequest.IntervalMinute = 10;

            var zoneSummaries = await GetZoneSummariesForAvgChartAsync(widgetRequest);

            widgetRequest.StartDate = widgetRequest.StartDate.AddHours(1);
            widgetRequest.IntervalMinute = IntervalMinute;

            // Get the offset timestamp
            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

            var deviceWiseDiff = zoneSummaries
                .SelectMany(x => x.Value)
                .GroupBy(x => x.DeviceId)
                .SelectMany(g => g.Select((x, i) => new PeopleVehicleInOutAvgChartDevicewise
                {
                    DeviceId = x.DeviceId,
                    DateTime = x.DateTime,
                    InCount = i == 0 || x.InCount < g.ElementAt(i - 1).InCount ? x.InCount :
                              x.InCount - g.ElementAt(i - 1).InCount,
                    OutCount = i == 0 || x.OutCount < g.ElementAt(i - 1).OutCount ? x.OutCount :
                               x.OutCount - g.ElementAt(i - 1).OutCount
                })
                ).Where(x => x.DateTime >= widgetRequest.StartDate).ToList();

            deviceWiseDiff = deviceWiseDiff
                .GroupBy(x => x.DateTime)
                .Select(x => new PeopleVehicleInOutAvgChartDevicewise
                {
                    DateTime = GetBucketTime(x.Key.Value + offsetTimeStamp, widgetRequest.AverageIntervalMinute),
                    InCount = x.Sum(a => a.InCount),
                    OutCount = x.Sum(a => a.OutCount),
                }).ToList();

            var result = deviceWiseDiff
                        .GroupBy(x => x.DateTime)
                        .Select(g => new CsvInOutResponseModel
                        {
                            DateTime = g.Key,//g.Key.Value,
                            InCount = Math.Round(g.Average(x => x.InCount), 2),
                            OutCount = Math.Round(g.Average(x => x.OutCount), 2)
                        })
                        .OrderBy(x => x.DateTime)
                        .ToList();

            var csvBuilder = await GenerateAverageVehicleCSV(result, widgetRequest.WidgetName, widgetRequest);

            return csvBuilder;
        }

        private async Task<StringBuilder> GenerateAverageVehicleCSV(List<CsvInOutResponseModel> data, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);

            string[] formats = {
                                "dd/MM/yyyy h:mm:ss tt",   // e.g. 26/08/2025 12:00:00 AM
                                "MM/dd/yyyy h:mm:ss tt",   // e.g. 04/07/2025 7:00:00 pm
                                "dd/MM/yyyy HH:mm:ss",      // e.g. 26/08/2025 00:00:00
                                "MM/dd/yyyy HH:mm:ss",     // e.g. 04/07/2025 19:00:00
                                "dd-MM-yyyy h:mm:ss tt",     // e.g. 04/07/2025 19:00:00
                                 };

            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            //// Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.AverageIntervalMinute;

            string headerData = "Type, Date";
            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.AverageIntervalMinute, slotsPerDay));

            // List of vehicle types
            var inOutTypes = new List<string> { "In", "Out" };

            // Process all data once and group by vehicle type
            var groupedByVehicleType = new Dictionary<string, Dictionary<DateTime, Dictionary<double, double>>>();

            // Initialize dictionary for each vehicle type
            foreach (var inOutType in inOutTypes)
            {
                groupedByVehicleType[inOutType] = new Dictionary<DateTime, Dictionary<double, double>>();
            }

            try
            {

                // Process data once for all vehicle types
                foreach (var record in data.Where(d => d.DateTime.HasValue))
                {
                    var dt = record.DateTime.Value;
                    int totalMinutes = dt.Hour * 60 + dt.Minute;
                    int slotIndex = totalMinutes / obj.AverageIntervalMinute;
                    var date = dt.Date;

                    foreach (var vehicleType in inOutTypes)
                    {
                        var count = GetCountByInOutType(record, vehicleType);
                        if (count > 0) // Only process if there's actual data
                        {
                            if (!groupedByVehicleType[vehicleType].ContainsKey(date))
                            {
                                groupedByVehicleType[vehicleType][date] = new Dictionary<double, double>();
                            }

                            if (!groupedByVehicleType[vehicleType][date].ContainsKey(slotIndex))
                            {
                                groupedByVehicleType[vehicleType][date][slotIndex] = 0;
                            }

                            groupedByVehicleType[vehicleType][date][slotIndex] += count;
                        }
                    }
                }
                // Generate data rows
                foreach (var vehicleType in inOutTypes)
                {
                    csvBuilder.Append(BindHourDataAvgForCsv(groupedByVehicleType[vehicleType], obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay, vehicleType));
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }

            return csvBuilder;
        }

        private string BindHourDataAvgForCsv(Dictionary<DateTime, Dictionary<double, double>> data, DateTime startDate, DateTime endDate, int slotsPerDay, string rowName = "", bool addTotal = false)
        {
            var csvBuilder = new StringBuilder();
            var currentDate = startDate.Date;
            //if (data.Count() > 0)
            //{
            while (currentDate <= endDate.Date)
            {
                var row = new List<string>();
                if (string.IsNullOrEmpty(rowName))
                {
                    row = new List<string> { currentDate.ToString("dd-MMM-yy") };
                }
                else
                {
                    row = new List<string> { rowName, currentDate.ToString("dd-MMM-yy") };
                }

                double totalCount = 0;
                for (int slot = 0; slot < slotsPerDay; slot++)
                {
                    if (data.TryGetValue(currentDate, out var slotData) &&
                        slotData.TryGetValue(slot, out var count))
                    {
                        row.Add(count.ToString());
                        totalCount += count;
                    }
                    else
                    {
                        row.Add("0");
                    }
                }

                if (addTotal)
                {
                    row.Add(totalCount.ToString());
                }

                csvBuilder.AppendLine(string.Join(",", row));
                currentDate = currentDate.AddDays(1);
            }
            //}
            return csvBuilder.ToString();
        }

        public async Task<StringBuilder> VehicleInOutCountCSVDownload(WidgetRequest widgetRequest)
        {
            widgetRequest.StartDate = widgetRequest.StartDate.AddHours(-1);
            int IntervalMinute = widgetRequest.IntervalMinute;
            widgetRequest.IntervalMinute = 10;
            var zoneSummaries = await GetZoneSummariesForAvgChartAsync(widgetRequest);

            widgetRequest.StartDate = widgetRequest.StartDate.AddHours(1);
            widgetRequest.IntervalMinute = IntervalMinute;
            var result = new List<CsvInOutResponseModel>();

            var deviceWiseDiff = zoneSummaries
                .SelectMany(x => x.Value)
                .GroupBy(x => x.DeviceId)
                .SelectMany(g => g.Select((x, i) => new PeopleVehicleInOutAvgChartDevicewise
                {
                    DeviceId = x.DeviceId,
                    DateTime = x.DateTime,
                    InCount = i == 0 || x.InCount < g.ElementAt(i - 1).InCount ? x.InCount :
                              x.InCount - g.ElementAt(i - 1).InCount,
                    OutCount = i == 0 || x.OutCount < g.ElementAt(i - 1).OutCount ? x.OutCount :
                               x.OutCount - g.ElementAt(i - 1).OutCount
                })
                ).Where(x => x.DateTime >= widgetRequest.StartDate).ToList();

            if (widgetRequest.calculatorExportRule == null || widgetRequest.calculatorExportRule.Count() == 0)
            {
                result = deviceWiseDiff
                .GroupBy(x => x.DateTime)
                .Select(g => new CsvInOutResponseModel
                {
                    DateTime = g.Key,
                    InCount = g.Sum(x => x.InCount),
                    OutCount = g.Sum(x => x.OutCount)
                })
                .OrderBy(x => x.DateTime)
                .ToList();
            }
            else
            {
                result = deviceWiseDiff
                .GroupBy(x => x.DateTime)
                .Select(g => new CsvInOutResponseModel
                {
                    DateTime = g.Key,
                    InCount = CalculateInOutCount(g.Sum(x => x.InCount), widgetRequest?.calculatorExportRule),
                    OutCount = CalculateInOutCount(g.Sum(x => x.OutCount), widgetRequest?.calculatorExportRule),
                })
                .OrderBy(x => x.DateTime)
                .ToList();
            }


            //// Step 1: Merge and group by exact DateTime (to remove duplicates)
            //var allChartData = zoneSummaries
            //    .SelectMany(z => z.Value)
            //    .GroupBy(x => x.DateTime)
            //    .Select(g => new CsvInOutResponseModel
            //    {
            //        DateTime = g.Key,
            //        InCount = g.Sum(x => x.InCount),
            //        OutCount = g.Sum(x => x.OutCount),
            //    }).ToList();

            //var diffData = allChartData
            //                .Select((x, i) => new CsvInOutResponseModel
            //                {
            //                    DateTime = x.DateTime,
            //                    InCount = i == 0 ? x.InCount : (x.InCount >= allChartData[i - 1].InCount) ? x.InCount - allChartData[i - 1].InCount : x.InCount,
            //                    OutCount = i == 0 ? x.OutCount : (x.OutCount >= allChartData[i - 1].OutCount) ? x.OutCount - allChartData[i - 1].OutCount : x.OutCount,
            //                })
            //                .Where(x => x.InCount >= 0 && x.DateTime >= widgetRequest.StartDate.AddHours(1)).ToList();


            var csvBuilder = await GenerateCSVForPeopleInOut(result, widgetRequest.WidgetName, widgetRequest);

            return csvBuilder;
        }     //else


        public async Task<StringBuilder> SlipFallQueueCsvDataAsync(WidgetRequest widgetRequest)
        {
            var data = await SlipFallQueueAnalysisDataAsync(widgetRequest);

            // Step 1: Merge and group by exact DateTime (to remove duplicates)
            var allChartData = data
                .Select(g => new CsvResponseModel
                {
                    DateTime = g.DateTime.ToString(),
                    Value = (int)g.QueueCount,
                }).ToList();

            var csvBuilder = await GenerateCSV(allChartData, widgetRequest.WidgetName, widgetRequest);

            return csvBuilder;
        }

        public async Task<IEnumerable<PeopleCountByZones>> PeopleCountByZones(WidgetRequest widgetRequest)
        {
            List<PeopleCountByZones> peopleCountByZones = new List<PeopleCountByZones>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            var timeZone = await _usersService.GetTimeZone(widgetRequest.UserId);

            foreach (var zone in zoneData)
            {
                IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
                if (zone.Id != null && zone.Id == "00")
                {
                    zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                }
                else
                {
                    zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
                }

                if (zoneCameraList != null)
                {
                    List<PeopleCountByDevice> peopleCountLst = new List<PeopleCountByDevice>();
                    var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
                    foreach (var camera in zoneCameraList)
                    {
                        PeopleCountByDevice result = await _peopleCountRepository.GetPeopleCountByDeviceAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, offsetTimeStamp, camera.PeopleLineIndex);
                        if (result != null)
                        {
                            peopleCountLst.Add(result);
                        }
                    }
                    peopleCountByZones.Add(new PeopleCountByZones
                    {
                        ZoneName = zone.ZoneName,
                        PeopleInCount = peopleCountLst.Sum(x => x.PeopleInCount),
                        PeopleOutCount = peopleCountLst.Sum(x => x.PeopelOutCount)
                    });
                }
            }

            return peopleCountByZones;
        }

        public async Task<StringBuilder> PeopleCountByZonesCsv(WidgetRequest widgetRequest)
        {
            var peopleCountByZoneData = await _peopleWidgetService.PeopleCountByZones(widgetRequest);
            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

            var csvBuilder = new StringBuilder();

            var offsetStartDate = widgetRequest.StartDate.Add(offsetTimeStamp);
            var offsetEndDate = widgetRequest.EndDate.Add(offsetTimeStamp);

            csvBuilder.Append(await GetTitleForCsv(widgetRequest.WidgetName, offsetStartDate, offsetEndDate, widgetRequest.UserId));

            //csvBuilder.AppendLine($"Widget name: {widgetRequest.WidgetName}");
            //csvBuilder.AppendLine($"\"Period: {widgetRequest.StartDate.Add(offsetTimeStamp):MMM dd, yyyy} - {widgetRequest.EndDate.Add(offsetTimeStamp):MMM dd, yyyy}\"");

            var header = new List<string> { "ZoneName", "People In", "People Out" };
            csvBuilder.AppendLine(string.Join(",", header));


            foreach (var item in peopleCountByZoneData)
            {
                var row = $"{item.ZoneName},{item.PeopleInCount},{item.PeopleOutCount}";
                csvBuilder.AppendLine(row);
            }


            return csvBuilder;
        }

        public async Task<StringBuilder> TotalCameraCountCsv(WidgetRequest widgetRequest)
        {

            IEnumerable<string> deviceIds = null;
            if (widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                deviceIds = await _deviceMasterRepository.GetUnMappeddevicesforWidgetCSV();
            }
            else
            {

                // ✅ Step 1: Get device IDs directly by floor + zone
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneLinkedServerAsync(
                    widgetRequest.FloorIds, widgetRequest.ZoneIds
                );
            }

            // ✅ Use HashSet for O(1) lookups
            var deviceIdSet = new HashSet<string>(deviceIds);

            // ✅ Step 2: Fetch ONLY the required devices (instead of always 10k)
            var allDevicesResponse = await _deviceMasterService.GetDevicesByDeviceIdCSVAsync(deviceIds);
            var allDevices = allDevicesResponse
                .Where(d => deviceIdSet.Contains(d.Id))
                .ToList();

            // ✅ Step 3: Get all zone camera details for these devices (already filtered)
            var zoneCameras = await _zoneCameraRepository.GetZoneCameraDetailsCSV(deviceIds);

            // ✅ Group zoneCameras by DeviceId once (avoid scanning in loop)
            var zoneCamerasByDevice = zoneCameras
                .GroupBy(zc => zc.DeviceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ✅ Step 4: Fetch all zones in one go and map them
            var zoneIds = zoneCameras.Select(zc => zc.ZoneId).Distinct().ToList();
            var zones = await _zoneRepository.GetZonesByIdsCSVAsync(zoneIds);
            var zoneMap = zones.ToDictionary(z => z.Id, z => z.ZoneName);

            // ✅ Step 5: Prepare CSV
            var csvBuilder = new StringBuilder();

            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);
            var offsetStartDate = widgetRequest.StartDate.Add(offsetTimeStamp);
            var offsetEndDate = widgetRequest.EndDate.Add(offsetTimeStamp);

            // Title
            csvBuilder.Append(
                await GetTitleForCsv(widgetRequest.WidgetName, offsetStartDate, offsetEndDate, widgetRequest.UserId)
            );

            // Header
            csvBuilder.AppendLine(string.Join(",", new[]
            {
        "No", "Device Name", "Model", "Ip Address", "Serial Number",
        "Mac Address", "Device Type", "Status", "Zone Name"
    }));

            // ✅ Step 6: Write rows
            int counter = 1;
            foreach (var device in allDevices)
            {
                string status = device.IsOnline ? "Online" : "Offline";
                string zoneNames = "";

                if (zoneCamerasByDevice.TryGetValue(device.Id, out var deviceZones))
                {
                    var names = deviceZones
                        .Where(zc => !string.IsNullOrEmpty(zc.ZoneId) && zoneMap.ContainsKey(zc.ZoneId))
                        .Select(zc => zoneMap[zc.ZoneId])
                        .Distinct();

                    zoneNames = string.Join(" | ", names);
                }

                csvBuilder.AppendLine(string.Join(",", new[]
                {
            counter.ToString(),
            device.DeviceName ?? "",
            device.Model ?? "",
            device.IpAddress ?? "",
            device.SerialNumber ?? "",
            device.MacAddress ?? "",
            device.DeviceType ?? "",
            status,
            zoneNames
        }));

                counter++;
            }

            return csvBuilder;
        }


        //public async Task<StringBuilder> TotalCameraCountCsv(WidgetRequest widgetRequest)
        //{
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    // Fetch devices
        //    DeviceRequest deviceRequest = new DeviceRequest
        //    {
        //        PageNo = 1,
        //        PageSize = 10000,
        //        SortBy = "deviceName",
        //        SortOrder = -1,
        //        SearchText = ""
        //    };

        //    var s1 = new Stopwatch();
        //    s1.Start();

        //    var allDevicesResponse = await _deviceMasterService.GetAllDevicesByFilterAsync(deviceRequest);

        //    var deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
        //    s1.Stop();
        //    Console.WriteLine($"Main Mathod Ended: {s1.Elapsed.TotalSeconds:F1}s");
        //    //var allDevices = allDevicesResponse.DeviceDetails;

        //    // Step 3: filter devices
        //    var allDevices = allDevicesResponse.DeviceDetails
        //        .Where(d => deviceIds.Contains(d.Id))
        //        .ToList();


        //    // Step 2: Get all zoneCamera records for these deviceIds
        //    //var deviceIds = allDevices.Select(d => d.Id).ToList();
        //    var zoneCameras = await _zoneCameraRepository.GetZoneCameraDetails(deviceIds);

        //    // Step 3: Get all unique zoneIds from zoneCameras
        //    var zoneIds = zoneCameras.Select(zc => zc.ZoneId).Distinct().ToList();

        //    // Step 4: Fetch all zones in one go
        //    var zones = await _zoneRepository.GetZonesByIdsAsync(zoneIds);

        //    // Build dictionary for quick lookup
        //    var zoneMap = zones.ToDictionary(z => z.Id, z => z.ZoneName);

        //    var csvBuilder = new StringBuilder();

        //    var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

        //    var offsetStartDate = widgetRequest.StartDate.Add(offsetTimeStamp);
        //    var offsetEndDate = widgetRequest.EndDate.Add(offsetTimeStamp);

        //    csvBuilder.Append(await GetTitleForCsv(widgetRequest.WidgetName, offsetStartDate, offsetEndDate, widgetRequest.UserId));

        //    // New header
        //    var header = new List<string>
        //                {
        //                    "No",
        //                    "Device Name",
        //                    "Model",
        //                    "Ip Address",
        //                    "Serial Number",
        //                    "Mac Address",
        //                    "Device Type",
        //                    "Status",
        //                    "Zone Name",
        //                };
        //    csvBuilder.AppendLine(string.Join(",", header));

        //    // Device rows
        //    int counter = 1;
        //    foreach (var device in allDevices)
        //    {
        //        string status = device.IsOnline ? "Online" : "Offline";

        //        // Find all zoneCamera entries for this device
        //        var deviceZones = zoneCameras.Where(zc => zc.DeviceId == device.Id).ToList();
        //        string zoneNames = "";

        //        if (deviceZones.Any())
        //        {
        //            var names = deviceZones
        //                .Where(zc => !string.IsNullOrEmpty(zc.ZoneId) && zoneMap.ContainsKey(zc.ZoneId))
        //                .Select(zc => zoneMap[zc.ZoneId])
        //                .Distinct();

        //            zoneNames = string.Join(" | ", names); // support multiple zones
        //        }

        //        var row = string.Join(",", new[]
        //        {
        //                  counter.ToString(),
        //                  device.DeviceName ?? "",
        //                  device.Model ?? "",
        //                  device.IpAddress ?? "",
        //                  device.SerialNumber ?? "",
        //                  device.MacAddress ?? "",
        //                  device.DeviceType ?? "",
        //                  status,
        //                  zoneNames,
        //        });

        //        csvBuilder.AppendLine(row);
        //        counter++;
        //    }
        //    stopwatch.Stop();
        //    Console.WriteLine($"Main Mathod Ended: {stopwatch.Elapsed.TotalSeconds:F1}s");
        //    return csvBuilder;
        //}

        public async Task<StringBuilder> CameraCountByModelCsv(WidgetRequest widgetRequest)
        {
            var cameraCountByModelData = await CameraCountByModelAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);

            var csvBuilder = new StringBuilder();

            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

            // Convert start and end dates to offset timezone
            var offsetStartDate = widgetRequest.StartDate.Add(offsetTimeStamp);
            var offsetEndDate = widgetRequest.EndDate.Add(offsetTimeStamp);

            csvBuilder.Append(await GetTitleForCsv(widgetRequest.WidgetName, offsetStartDate, offsetEndDate, widgetRequest.UserId));


            var header = new List<string> { "Series Name", "Total Count" };
            csvBuilder.AppendLine(string.Join(",", header));

            foreach (var item in cameraCountByModelData)
            {
                var row = $"{item.SeriesName},{item.TotalCount}";
                csvBuilder.AppendLine(row);
            }
            return csvBuilder;
        }

        public async Task<StringBuilder> CameraCountByFeaturesCsv(WidgetRequest widgetRequest)
        {
            var cameraCountByFeaturesData = await CameraCountByFeaturesAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);

            var csvBuilder = new StringBuilder();

            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

            var offsetStartDate = widgetRequest.StartDate.Add(offsetTimeStamp);
            var offsetEndDate = widgetRequest.EndDate.Add(offsetTimeStamp);

            csvBuilder.Append(await GetTitleForCsv(widgetRequest.WidgetName, offsetStartDate, offsetEndDate, widgetRequest.UserId));

            var header = new List<string> { "Feature Name", "Total Count" };
            csvBuilder.AppendLine(string.Join(",", header));

            foreach (var item in cameraCountByFeaturesData)
            {
                var row = $"{item.FeaturesName},{item.TotalCount}";
                csvBuilder.AppendLine(row);
            }

            return csvBuilder;
        }



        public async Task<StringBuilder> PeopleCapacityUtilizationCsv(WidgetRequest widgetRequest)
        {
            var tenMinuteData = await _peopleWidgetService.PeopleCameraCapacityUtilizationAnalysisByZones(widgetRequest);
            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);
            var csvBuilder = new StringBuilder();

            var processedData = new List<ZoneUtilizationDtoCSV>();
            // Apply offset to each record
            //var dataWithOffset = tenMinuteData.Select(x => new PeopleVehicleInOutAvgChart
            //{
            //    DateTime = x.DateTime.Value.Add(offsetTimeStamp),
            //    InCount = x.InCount,
            //    OutCount = x.OutCount,
            //    DateTimeCsv = x.DateTime.Value.Add(offsetTimeStamp).ToString()
            //}).ToList();

            foreach (var zoneData in tenMinuteData)
            {
                if (zoneData.UtilizationData != null && zoneData.UtilizationData.Any())
                {
                    var zoneMaxValues = zoneData.UtilizationData
                        .Select(x => new ZoneUtilizationDtoCSV
                        {
                            DateTime = GetBucketTime(x.DateTime.Add(offsetTimeStamp), widgetRequest.IntervalMinute),
                            MaxCount = x.Count
                        })
                         .GroupBy(x => new DateTime(x.DateTime.Value.Year, x.DateTime.Value.Month, x.DateTime.Value.Day, x.DateTime.Value.Hour, 0, 0, x.DateTime.Value.Kind))
                        .Select(g => new ZoneUtilizationDtoCSV
                        {
                            DateTime = g.Key,
                            MaxCount = g.Max(x => x.MaxCount)
                        })
                        .ToList();

                    processedData.AddRange(zoneMaxValues);
                }
            }

            var allUtilizationData = processedData
                .GroupBy(x => x.DateTime)
                .Select(g => new ZoneUtilizationDtoCSV
                {
                    DateTime = g.Key,
                    MaxCount = g.Sum(x => x.MaxCount)
                })
                .OrderBy(x => x.DateTime)
                .ToList();


            csvBuilder = await GenerateCSVForUtilization(allUtilizationData.ToList(), widgetRequest.WidgetName, widgetRequest, offsetTimeStamp);
            return csvBuilder;
        }



        public async Task<StringBuilder> VehicleCapacityUtilizationCsv(WidgetRequest widgetRequest)
        {
            var tempIntervalMinute = widgetRequest.IntervalMinute;
            widgetRequest.IntervalMinute = 10;

            var tenMinuteData = await VehicleCameraCapacityUtilizationAnalysisByZones(widgetRequest);
            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);
            var csvBuilder = new StringBuilder();

            var processedData = new List<ZoneUtilizationDtoCSV>();
            // Apply offset to each record
            //var dataWithOffset = tenMinuteData.Select(x => new PeopleVehicleInOutAvgChart
            //{
            //    DateTime = x.DateTime.Value.Add(offsetTimeStamp),
            //    InCount = x.InCount,
            //    OutCount = x.OutCount,
            //    DateTimeCsv = x.DateTime.Value.Add(offsetTimeStamp).ToString()
            //}).ToList();

            widgetRequest.IntervalMinute = tempIntervalMinute;

            foreach (var zoneData in tenMinuteData)
            {
                if (zoneData.UtilizationData != null && zoneData.UtilizationData.Any())
                {
                    var zoneMaxValues = zoneData.UtilizationData
                        .Select(x => new ZoneUtilizationDtoCSV
                        {
                            DateTime = GetBucketTime(x.DateTime.Add(offsetTimeStamp), widgetRequest.IntervalMinute),
                            MaxCount = x.Count
                        })
                         .GroupBy(x => new DateTime(x.DateTime.Value.Year, x.DateTime.Value.Month, x.DateTime.Value.Day, x.DateTime.Value.Hour, 0, 0, x.DateTime.Value.Kind))
                        .Select(g => new ZoneUtilizationDtoCSV
                        {
                            DateTime = g.Key,
                            MaxCount = g.Max(x => x.MaxCount)
                        })
                        .ToList();

                    processedData.AddRange(zoneMaxValues);
                }
            }

            var allUtilizationData = processedData
                .GroupBy(x => x.DateTime)
                .Select(g => new ZoneUtilizationDtoCSV
                {
                    DateTime = g.Key,
                    MaxCount = g.Sum(x => x.MaxCount)
                })
                .OrderBy(x => x.DateTime)
                .ToList();


            csvBuilder = await GenerateCSVForUtilization(allUtilizationData.ToList(), widgetRequest.WidgetName, widgetRequest, offsetTimeStamp);
            return csvBuilder;
        }

        public async Task<StringBuilder> GenderWisePeopleCountAnalysisDataCsv(WidgetRequest widgetRequest)
        {
            //var data = await GenderWisePeopleCountAnalysisData(widgetRequest);
            //var data = await _peopleWidgetService.GenderWisePeopleCounting(widgetRequest);
            var data = await _peopleWidgetService.GenderWisePeopleCountAnalysisData(widgetRequest);
            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

            var aggregatedData = new List<GenderWisePeopleAnalysisCount>();

            var istData = data
                .Where(x => x.DateTime.HasValue)
                .Select(x => new
                {
                    DateTime = x.DateTime.Value.Add(offsetTimeStamp), // ✅ converted to IST
                    x.MaleCount,
                    x.FemaleCount,
                    x.UndefinedCount
                })
                .ToList();
            try
            {
                if (widgetRequest.calculatorExportRule == null || widgetRequest.calculatorExportRule.Count() == 0)
                {


                    aggregatedData = istData
                                                 .GroupBy(x => x.DateTime.Date)
                                                 // .GroupBy(x => x.DateTime?.Date.Add(offsetTimeStamp))
                                                 .Select(group => new GenderWisePeopleAnalysisCount
                                                 {
                                                     DateTime = group.Key,
                                                     MaleCount = group.Sum(item => item.MaleCount),
                                                     FemaleCount = group.Sum(item => item.FemaleCount),
                                                     UndefinedCount = group.Sum(item => item.UndefinedCount)
                                                 })
                                                 .Where(x => x.MaleCount != 0 || x.FemaleCount != 0 || x.UndefinedCount != 0) // ✅ Exclude all-zero rows
                                                 .ToList();
                    //aggregatedData = data
                    //                 .Where(item => item.MaxDate.HasValue) // Ensure MinDate is not null
                    //                 .GroupBy(item => item.MaxDate.Value.Date) // Group only by Date
                    //                 .Select(group => new GenderWisePeopleAnalysisCount
                    //                 {
                    //                     DateTime = group.Key,
                    //                     MaleCount = group
                    //                         .Where(x => x.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
                    //                         .Any() ? group
                    //                         .Where(x => x.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
                    //                         .Max(x => x.MaxCount) : 0,
                    //                     FemaleCount = group
                    //                         .Where(x => x.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
                    //                         .Any() ? group
                    //                         .Where(x => x.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
                    //                         .Max(x => x.MaxCount) : 0,
                    //                     UndefinedCount = group
                    //                         .Where(x => x.Gender.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    //                         .Any() ? group
                    //                         .Where(x => x.Gender.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    //                         .Max(x => x.MaxCount) : 0
                    //                 }).ToList();
                }
                else
                {
                    //aggregatedData = data
                    //                .Where(item => item.MaxDate.HasValue) // Ensure MaxDate is not null
                    //                .GroupBy(item => item.MaxDate.Value.Date) // Group only by Date
                    //                .Select(group => new GenderWisePeopleAnalysisCount
                    //                {
                    //                    DateTime = group.Key,
                    //                    MaleCount = (double)CalculateInOutCount(
                    //                        group.Where(x => x.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
                    //                             .Any() ? group
                    //                             .Where(x => x.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
                    //                             .Max(x => x.MaxCount) : 0,
                    //                        widgetRequest.calculatorExportRule),
                    //                    FemaleCount = (double)CalculateInOutCount(
                    //                        group.Where(x => x.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
                    //                             .Any() ? group
                    //                             .Where(x => x.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
                    //                             .Max(x => x.MaxCount) : 0,
                    //                        widgetRequest.calculatorExportRule),
                    //                    UndefinedCount = (double)CalculateInOutCount(
                    //                        group.Where(x => x.Gender.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    //                             .Any() ? group
                    //                             .Where(x => x.Gender.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                    //                             .Max(x => x.MaxCount) : 0,
                    //                        widgetRequest.calculatorExportRule)
                    //                }).ToList();

                    aggregatedData = istData
                                     .GroupBy(x => x.DateTime.Date)
                                     .Select(group => new GenderWisePeopleAnalysisCount
                                     {
                                         DateTime = group.Key,
                                         MaleCount = (double)CalculateInOutCount(group.Sum(item => item.MaleCount), widgetRequest.calculatorExportRule),
                                         FemaleCount = (double)CalculateInOutCount(group.Sum(item => item.FemaleCount), widgetRequest.calculatorExportRule),
                                         UndefinedCount = (double)CalculateInOutCount(group.Sum(item => item.UndefinedCount), widgetRequest.calculatorExportRule),
                                     })
                                     .Where(x => x.MaleCount != 0 || x.FemaleCount != 0 || x.UndefinedCount != 0) // ✅ Exclude all-zero rows
                                     .ToList();

                }

                var csvBuilder = new StringBuilder();

                if (aggregatedData.Count() == 0)
                {
                    return csvBuilder;
                }

                // Convert start and end dates to offset timezone
                var offsetStartDate = widgetRequest.StartDate.Add(offsetTimeStamp);
                var offsetEndDate = widgetRequest.EndDate.Add(offsetTimeStamp);

                csvBuilder.Append(await GetTitleForCsv(widgetRequest.WidgetName, offsetStartDate, offsetEndDate, widgetRequest.UserId));

                //csvBuilder.AppendLine($"Widget name: {widgetRequest.WidgetName}");
                //csvBuilder.AppendLine($"\"Period: {widgetRequest.StartDate.Add(offsetTimeStamp):MMM dd, yyyy} - {widgetRequest.EndDate.Add(offsetTimeStamp):MMM dd, yyyy}\"");

                var header = new List<string> { "Date", "Male", "Female", "Unknown" };
                csvBuilder.AppendLine(string.Join(",", header));

                foreach (var item in aggregatedData)
                {
                    var row = $"\"{item.DateTime.Value.Add(offsetTimeStamp):dd-MMM-yy}\",{item.MaleCount},{item.FemaleCount},{item.UndefinedCount}";
                    csvBuilder.AppendLine(row);
                }

                return csvBuilder;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }
        }

        public async Task<StringBuilder> AgeWisePeopleCountAnalysisDataCsv(WidgetRequest widgetRequest)
        {
            try
            {
                var data = await _peopleWidgetService.AgeWisePeopleCountAnalysisData(widgetRequest);
                var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

                var timeStampData = data
              .Where(x => x.DateTime.HasValue)
              .Select(x => new
              {
                  DateTime = x.DateTime.Value.Add(offsetTimeStamp),
                  x.YoungCount,
                  x.AdultCount,
                  x.SeniorCount,
                  x.UnknownCount

              })
              .ToList();

                var result = timeStampData
                    .GroupBy(x => x.DateTime.Date)
                    .Select(g => new AgeWisePeopleAnalysisCount
                    {
                        DateTime = g.Key,
                        YoungCount = g.Sum(x => x.YoungCount),
                        AdultCount = g.Sum(x => x.AdultCount),
                        SeniorCount = g.Sum(x => x.SeniorCount),
                        UnknownCount = g.Sum(x => x.UnknownCount)
                    })
                    .OrderBy(x => x.DateTime)
                    .Where(x => x.YoungCount != 0 || x.AdultCount != 0 || x.SeniorCount != 0 || x.UnknownCount != 0)
                    .ToList();

                //var maxResult = new
                //{
                //    MaxYoung = result.OrderByDescending(x => x.YoungCount).First(),
                //    MaxAdult = result.OrderByDescending(x => x.AdultCount).First(),
                //    MaxSenior = result.OrderByDescending(x => x.SeniorCount).First(),
                //    MaxUnknown = result.OrderByDescending(x => x.UnknownCount).First(),
                //    MinYoung = result.OrderBy(x => x.YoungCount).First(),
                //    MinAdult = result.OrderBy(x => x.AdultCount).First(),
                //    MinSenior = result.OrderBy(x => x.SeniorCount).First(),
                //    MinUnknown = result.OrderBy(x => x.UnknownCount).First()
                //};

                var csvBuilder = new StringBuilder();

                // Convert start and end dates to offset timezone
                var offsetStartDate = widgetRequest.StartDate.Add(offsetTimeStamp);
                var offsetEndDate = widgetRequest.EndDate.Add(offsetTimeStamp);

                csvBuilder.Append(await GetTitleForCsv(widgetRequest.WidgetName, offsetStartDate, offsetEndDate, widgetRequest.UserId));

                //csvBuilder.AppendLine($"Widget name: {widgetRequest.WidgetName}");
                //csvBuilder.AppendLine($"\"Period: {widgetRequest.StartDate.Add(offsetTimeStamp):MMM dd, yyyy} - {widgetRequest.EndDate.Add(offsetTimeStamp):MMM dd, yyyy}\"");

                var header = new List<string> { "Date", "Young", "Adult", "Senior", "unknown" };
                csvBuilder.AppendLine(string.Join(",", header));

                foreach (var item in result)
                {
                    var row = $"\"{item.DateTime.Value.Add(offsetTimeStamp):dd-MMM-yy}\",{item.YoungCount},{item.AdultCount},{item.SeniorCount},{item.UnknownCount}";
                    csvBuilder.AppendLine(row);
                }


                //foreach (var item in aggregatedData)
                //{
                //var row = $"{maxResult.MaxYoung.YoungCount},{maxResult.MaxAdult.AdultCount},{maxResult.MaxSenior.SeniorCount}, {maxResult.MaxUnknown.UnknownCount}";
                //csvBuilder.AppendLine(row);
                //var row1 = $"{maxResult.MinYoung.YoungCount},{maxResult.MinAdult.AdultCount},{maxResult.MinSenior.SeniorCount}, {maxResult.MinUnknown.UnknownCount}";
                //csvBuilder.AppendLine(row1);
                //}

                return csvBuilder;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }
        }
        // Example usage with your existing method
        public async Task<StringBuilder> PeopleCameraCapacityUtilizationByZonesCsv(WidgetRequest widgetRequest)
        {
            var data = await _peopleWidgetService.PeopleCameraCapacityUtilizationAnalysisByZones(widgetRequest);

            var csvBuilder = new StringBuilder();
            if (data.Count() > 0)
            {
                var offsetTimeStamp = await GetOffset(widgetRequest.UserId);
                // Apply time offset using the generic method

                csvBuilder = await GenerateCSVForZone(data.ToList(), widgetRequest.WidgetName, widgetRequest, offsetTimeStamp);
            }
            return csvBuilder;
        }
        public async Task<StringBuilder> VehicleCameraCapacityUtilizationByZonesCsv(WidgetRequest widgetRequest)
        {
            var tempInterval = widgetRequest.IntervalMinute;
            widgetRequest.IntervalMinute = 10;
            var data = await VehicleCameraCapacityUtilizationAnalysisByZones(widgetRequest);
            widgetRequest.IntervalMinute = tempInterval;
            var csvBuilder = new StringBuilder();

            if (data.Count() > 0)
            {
                var offsetTimeStamp = await GetOffset(widgetRequest.UserId);
                csvBuilder = await GenerateCSVForZone(data.ToList(), widgetRequest.WidgetName, widgetRequest, offsetTimeStamp);
            }

            return csvBuilder;
        }

        public async Task<StringBuilder> VehicleByTypeLineChartCsv(WidgetRequest widgetRequest)
        {

            //widgetRequest.StartDate = widgetRequest.StartDate.AddHours(-1);
            int IntervalMinute = widgetRequest.IntervalMinute;
            widgetRequest.IntervalMinute = 10;

            var data = await VehicleByTypeLineChartData(widgetRequest);

            //widgetRequest.StartDate = widgetRequest.StartDate.AddHours(1);
            widgetRequest.IntervalMinute = IntervalMinute;

            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

            var csvBuilder = new StringBuilder();
            data = data
            .GroupBy(x => x.DateTime)
            .Select(x => new VehicleByTypeChartResponse
            {
                DateTime = GetBucketTime(x.Key.Value + offsetTimeStamp, widgetRequest.AverageIntervalMinute),

                CarInCount = x.Sum(a => a.CarInCount),
                CarOutCount = x.Sum(a => a.CarOutCount),
                MotorCycleInCount = x.Sum(a => a.MotorCycleInCount),
                MotorCycleOutCount = x.Sum(a => a.MotorCycleOutCount),
                BicycleInCount = x.Sum(a => a.BicycleInCount),
                BicycleOutCount = x.Sum(a => a.BicycleOutCount),
                BusInCount = x.Sum(a => a.BusInCount),
                BusOutCount = x.Sum(a => a.BusOutCount),
                TruckInCount = x.Sum(a => a.TruckInCount),
                TruckOutCount = x.Sum(a => a.TruckOutCount)
            }).ToList();

            var result = new List<VehicleByTypeChartResponse>();

            if (widgetRequest.calculatorExportRule == null || widgetRequest.calculatorExportRule.Count() == 0)
            {
                result = data
               .GroupBy(y => y.DateTime)
               .Select(x => new VehicleByTypeChartResponse
               {
                   DateTime = x.Key,//g.Key.Value,
                   CarInCount = x.Sum(a => a.CarInCount),
                   CarOutCount = x.Sum(a => a.CarOutCount),
                   MotorCycleInCount = x.Sum(a => a.MotorCycleInCount),
                   MotorCycleOutCount = x.Sum(a => a.MotorCycleOutCount),
                   BicycleInCount = x.Sum(a => a.BicycleInCount),
                   BicycleOutCount = x.Sum(a => a.BicycleOutCount),
                   BusInCount = x.Sum(a => a.BusInCount),
                   BusOutCount = x.Sum(a => a.BusOutCount),
                   TruckInCount = x.Sum(a => a.TruckInCount),
                   TruckOutCount = x.Sum(a => a.TruckOutCount),
               })
               .OrderBy(x => x.DateTime)
               .ToList();
            }
            else
            {
                result = data
               .GroupBy(y => y.DateTime)
               .Select(x => new VehicleByTypeChartResponse
               {
                   DateTime = x.Key,//g.Key.Value,
                   CarInCount = (double)CalculateInOutCount(x.Sum(a => a.CarInCount), widgetRequest.calculatorExportRule),
                   CarOutCount = (double)CalculateInOutCount(x.Sum(a => a.CarOutCount), widgetRequest.calculatorExportRule),
                   MotorCycleInCount = (double)CalculateInOutCount(x.Sum(a => a.MotorCycleInCount), widgetRequest.calculatorExportRule),
                   MotorCycleOutCount = (double)CalculateInOutCount(x.Sum(a => a.MotorCycleOutCount), widgetRequest.calculatorExportRule),
                   BicycleInCount = (double)CalculateInOutCount(x.Sum(a => a.BicycleInCount), widgetRequest.calculatorExportRule),
                   BicycleOutCount = (double)CalculateInOutCount(x.Sum(a => a.BicycleOutCount), widgetRequest.calculatorExportRule),
                   BusInCount = (double)CalculateInOutCount(x.Sum(a => a.BusInCount), widgetRequest.calculatorExportRule),
                   BusOutCount = (double)CalculateInOutCount(x.Sum(a => a.BusOutCount), widgetRequest.calculatorExportRule),
                   TruckInCount = (double)CalculateInOutCount(x.Sum(a => a.TruckInCount), widgetRequest.calculatorExportRule),
                   TruckOutCount = (double)CalculateInOutCount(x.Sum(a => a.TruckOutCount), widgetRequest.calculatorExportRule),
               })
               .OrderBy(x => x.DateTime)
               .ToList();

            }

            if (data.Count() > 0)
            {
                csvBuilder = await GenerateCSVForVehicle(result.ToList(), widgetRequest.WidgetName, widgetRequest);
            }

            return csvBuilder;
        }


        public async Task<StringBuilder> NewVsTotalVisitorCsv(WidgetRequest widgetRequest)
        {
            //var zoneSummaries = await GetZoneSummariesForNewVsTotalChartAsync(widgetRequest);
            var newVsTotaldata = await _peopleWidgetService.NewVsTotalVisitorChartAsync(widgetRequest);


            NewVsTotalVisitorExcel obj = new NewVsTotalVisitorExcel();
            var csvBuilder = new StringBuilder();

            // Step 1: Merge and group by exact DateTime (to remove duplicates)
            //var allChartData = zoneSummaries
            //    .SelectMany(z => z.Value)
            //    .GroupBy(x => x.DateTime)
            //    .Select(g => new NewVsTotalVisitorExcel
            //    {
            //        DateTime = g.Key,
            //        NewVisitorsCount = g.Sum(x => x.NewInCount),
            //        TotalVisitorsCount = g.Sum(x => x.TotalCount),
            //    }).ToList();



            var data = newVsTotaldata.Select(item => new NewVsTotalVisitorExcel
            {
                NewVisitorsCount = item.NewVisitor,
                TotalVisitorsCount = item.TotalVisitor,
                DateTime = item.DateTime
            });


            if (data.Count() > 0)
            {
                csvBuilder = await GenerateCSVForNewVsTotal(data.ToList(), widgetRequest.WidgetName, widgetRequest);
            }
            return csvBuilder;
        }
        public async Task<StringBuilder> WrongWayQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var data = await WrongWayQueueAnalysisDataAsync(widgetRequest);
            var csvBuilder = await GenerateCSVForEvent(data, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }

        public async Task<StringBuilder> VehicleUTurnAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var data = await VehicleUTurnAnalysisDataAsync(widgetRequest);
            var csvBuilder = await GenerateCSVForEvent(data, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }
        public async Task<StringBuilder> PedestrianQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var data = await PedestrianQueueAnalysisDataAsync(widgetRequest);
            var csvBuilder = await GenerateCSVForEvent(data, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }

        public async Task<StringBuilder> VehicleQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var data = await VehicleQueueAnalysisDataAsync(widgetRequest);
            var csvBuilder = await GenerateCSVForEvent(data, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }
        public async Task<StringBuilder> StoppedVehicleByTypeAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var stoppedVehicleList = await StoppedVehicleByTypeAnalysisDataAsync(widgetRequest);
            var converted = stoppedVehicleList.Select(x => new VehicleByTypeChartResponse
            {
                TruckInCount = x.Truck,
                MotorCycleInCount = x.Motorcycle,
                BusInCount = x.Bus,
                BicycleInCount = x.Bicycle,
                CarInCount = x.Car,
                DateTime = x.DateTime
            }).ToList();
            var csvBuilder = await GenerateCSVForVehicle(converted, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }

        public async Task<StringBuilder> VehicleTurningMovementAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var turningMovementList = await VehicleTurningMovementAnalysisData(widgetRequest);
            var csvBuilder = await GenerateCSVForVehicleTurningMovement(turningMovementList, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }

        public async Task<StringBuilder> ShoppingCartQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var shoppingCartList = await ShoppingCartQueueAnalysisDataAsync(widgetRequest);
            var csvBuilder = await GenerateCSVForEvent(shoppingCartList, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }

        public async Task<StringBuilder> ShoppingCartCountAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var shoppingCartList = await ShoppingCartCountAnalysisData(widgetRequest);
            var csvBuilder = await GenerateCSVForEvent(shoppingCartList, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }
        public async Task<StringBuilder> PeopleQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var peopleQueueList = await PeopleQueueAnalysisDataAsync(widgetRequest);
            var csvBuilder = await GenerateCSVForEvent(peopleQueueList, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }
        public async Task<StringBuilder> BlockedExitAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var blockedExitList = await BlockedExitAnalysisDataAsync(widgetRequest);

            var csvBuilder = await GenerateCSVForEvent(blockedExitList, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }
        public async Task<StringBuilder> VehicleSpeedViolationAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var vehicleSpeedViolationList = await VehicleSpeedViolationAnalysisDataAsync(widgetRequest);
            var csvBuilder = await GenerateCSVForEvent(vehicleSpeedViolationList, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }
        public async Task<StringBuilder> TrafficJamAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var trafficJamAnalysisList = await TrafficJamAnalysisDataAsync(widgetRequest);
            var csvBuilder = await GenerateCSVForEvent(trafficJamAnalysisList, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }
        public async Task<StringBuilder> ForkliftCountAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var forkliftAnalysisList = await ForkliftCountAnalysisDataAsync(widgetRequest);

            if (widgetRequest.calculatorExportRule != null && widgetRequest.calculatorExportRule.Count() > 0)
            {
                forkliftAnalysisList = forkliftAnalysisList
                      .GroupBy(x => x.DateTime)
                      .Select(g => new EventQueueAnalysis
                      {
                          DateTime = g.Key,
                          QueueCount = (double)CalculateInOutCount(g.Sum(x => x.QueueCount), widgetRequest.calculatorExportRule)
                      })
                      .OrderBy(x => x.DateTime)
                      .ToList();
            }

            var csvBuilder = await GenerateCSVForEvent(forkliftAnalysisList, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }

        public async Task<StringBuilder> ForkliftQueueAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var forkliftQueueAnalysisList = await ForkliftQueueAnalysisDataAsync(widgetRequest);
            var csvBuilder = await GenerateCSVForEvent(forkliftQueueAnalysisList, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }

        public async Task<StringBuilder> ProxomityDetectionAnalysisCsvData(WidgetRequest widgetRequest)
        {
            var forkliftQueueAnalysisList = (await ProxomityDetectionAnalysisDataAsync(widgetRequest)).ToList();
            var csvBuilder = await GenerateCSVForEvent(forkliftQueueAnalysisList, widgetRequest.WidgetName, widgetRequest);
            return csvBuilder;
        }

        private async Task<StringBuilder> GenerateCSVForZone(List<CameraCapacityUtilizationAnalysisByZones> data, string widgetName, WidgetRequest obj, TimeSpan offset)
        {
            var csvBuilder = new StringBuilder();

            //csvBuilder.AppendLine($"Widget name: {widgetName}");
            //csvBuilder.AppendLine($"\"Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}\"");

            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offset), obj.EndDate.Add(offset), obj.UserId));

            int slotsPerDay = (24 * 60) / obj.IntervalMinute;

            string headerData = "Zone Name, Date";

            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.IntervalMinute, slotsPerDay));

            foreach (var zone in data)
            {
                // Group data by date and time slot index
                var groupedByDate = zone.UtilizationData
                    .Select(d =>
                    {
                        var dt = GetBucketTime(d.DateTime.Add(offset), obj.IntervalMinute);
                        int totalMinutes = dt.Hour * 60 + dt.Minute;
                        int slotIndex = totalMinutes / obj.IntervalMinute;
                        return new
                        {
                            Date = dt.Date,
                            SlotIndex = slotIndex,
                            Count = d.Count
                        };
                    })
                    .GroupBy(x => x.Date)
                    .ToDictionary(
                        g => g.Key,
                        g => g.GroupBy(x => x.SlotIndex)
                              .ToDictionary(
                                  s => s.Key,
                                  s => Convert.ToInt32(s.Max(x => x.Count))
                              )
                    );

                csvBuilder.Append(BindHourDataForCsv(groupedByDate, obj.StartDate.Add(offset), obj.EndDate.Add(offset), slotsPerDay, zone.ZoneName));

            }
            return csvBuilder;
        }

        private async Task<StringBuilder> GenerateCSVForUtilization(List<ZoneUtilizationDtoCSV> data, string widgetName, WidgetRequest obj, TimeSpan offset)
        {

            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);

            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            //// Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.AverageIntervalMinute;

            csvBuilder.Append(GetHourHeaderForCsv("Date", obj.AverageIntervalMinute, slotsPerDay));

            try
            {
                // Preprocess: convert to (date, slot index) => count
                var groupedData = data
                                  .Select(item =>
                                  {
                                      var dt = item.DateTime;
                                      int totalMinutes = dt.Value.Hour * 60 + dt.Value.Minute;
                                      int slotIndex = totalMinutes / obj.AverageIntervalMinute;
                                      return new
                                      {
                                          Date = dt.Value.Date,
                                          SlotIndex = slotIndex,
                                          Count = item.MaxCount
                                      };
                                  })
                                  .GroupBy(x => x.Date)
                                  .ToDictionary(
                                      g => g.Key,
                                      g => g
                                          .GroupBy(x => x.SlotIndex)
                                          .ToDictionary(
                                              x => x.Key,
                                              x => x.Sum(v => v.Count) // resolve duplicate slotIndex by summing values
                                          )
                                  );


                // Generate data rows
                csvBuilder.Append(BindHourDataForCsv(groupedData, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay));
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }

            return csvBuilder;
        }

        public async Task<StringBuilder> GenerateCSVForVehicle(List<VehicleByTypeChartResponse> data, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);
            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            // Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.IntervalMinute;
            string headerData = "Vehicle Type, Date";
            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.IntervalMinute, slotsPerDay, "Total"));

            // List of vehicle types
            var vehicleTypes = new List<string> { "Truck", "MotorCycle", "Bus", "Bicycle", "Car" };

            // Process all data once and group by vehicle type
            var groupedByVehicleType = new Dictionary<string, Dictionary<DateTime, Dictionary<double, double>>>();

            // Initialize dictionary for each vehicle type
            foreach (var vehicleType in vehicleTypes)
            {
                groupedByVehicleType[vehicleType] = new Dictionary<DateTime, Dictionary<double, double>>();
            }

            // Process data once for all vehicle types
            bool isStoppedVehicle = widgetName == "Vehicle by Type"; // used by 2 widgets as vehicle by type widget has already processed time data.
            foreach (var record in data.Where(d => d.DateTime.HasValue))
            {
                var dt = isStoppedVehicle ? record.DateTime.Value : record.DateTime.Value.Add(offsetTimeStamp);
                int totalMinutes = dt.Hour * 60 + dt.Minute;
                int slotIndex = totalMinutes / obj.IntervalMinute;
                var date = dt.Date;

                foreach (var vehicleType in vehicleTypes)
                {
                    var count = GetCountByType(record, vehicleType);
                    if (count > 0) // Only process if there's actual data
                    {
                        if (!groupedByVehicleType[vehicleType].ContainsKey(date))
                        {
                            groupedByVehicleType[vehicleType][date] = new Dictionary<double, double>();
                        }

                        if (!groupedByVehicleType[vehicleType][date].ContainsKey(slotIndex))
                        {
                            groupedByVehicleType[vehicleType][date][slotIndex] = 0;
                        }

                        groupedByVehicleType[vehicleType][date][slotIndex] += count;
                    }
                }
            }

            // Generate CSV for each vehicle type
            foreach (var vehicleType in vehicleTypes)
            {
                csvBuilder.Append(BindHourDataCalcultorForCsv(groupedByVehicleType[vehicleType], obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay, vehicleType, true));
            }

            return csvBuilder;
        }
        public async Task<StringBuilder> GenerateCSVForNewVsTotal(List<NewVsTotalVisitorExcel> data, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);
            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            // Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.IntervalMinute;
            string headerData = "Visitor Type, Date";
            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.IntervalMinute, slotsPerDay, "Total"));

            // List of vehicle types
            var visitorTypes = new List<string> { "New Visitors", "Total Visitors" };

            // Process all data once and group by vehicle type
            var groupedByVisitor = new Dictionary<string, Dictionary<DateTime, Dictionary<int, int>>>();

            // Initialize dictionary for each vehicle type
            foreach (var vehicleType in visitorTypes)
            {
                groupedByVisitor[vehicleType] = new Dictionary<DateTime, Dictionary<int, int>>();
            }

            long defaultOccupancy = 0;
            if (data != null && data.Any())
            {
                defaultOccupancy = data.FirstOrDefault().TotalVisitorsCount;
            }

            data = data.Select(x => new NewVsTotalVisitorExcel
            {
                DateTime = GetBucketTime(x.DateTime.Value.Add(offsetTimeStamp), obj.IntervalMinute),
                NewVisitorsCount = x.NewVisitorsCount,
            }).GroupBy(x => x.DateTime)
            .Select(y => new NewVsTotalVisitorExcel
            {
                DateTime = y.Key,
                NewVisitorsCount = y.Sum(z => z.NewVisitorsCount),
                TotalVisitorsCount = y.Sum(z => z.NewVisitorsCount) + defaultOccupancy
            }).ToList();

            // Process data once for all vehicle types
            foreach (var record in data.Where(d => d.DateTime.HasValue))
            {
                var dt = record.DateTime.Value;
                int totalMinutes = dt.Hour * 60 + dt.Minute;
                int slotIndex = totalMinutes / obj.IntervalMinute;
                var date = dt.Date;

                foreach (var vehicleType in visitorTypes)
                {
                    var count = GetCountByVisitor(record, vehicleType);
                    if (count > 0) // Only process if there's actual data
                    {
                        if (!groupedByVisitor[vehicleType].ContainsKey(date))
                        {
                            groupedByVisitor[vehicleType][date] = new Dictionary<int, int>();
                        }

                        if (!groupedByVisitor[vehicleType][date].ContainsKey(slotIndex))
                        {
                            groupedByVisitor[vehicleType][date][slotIndex] = 0;
                        }

                        groupedByVisitor[vehicleType][date][slotIndex] += Convert.ToInt32(count);
                    }
                }
            }

            // Generate CSV for each vehicle type
            foreach (var vehicleType in visitorTypes)
            {
                csvBuilder.Append(BindHourDataForCsv(groupedByVisitor[vehicleType], obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay, vehicleType, true));
            }

            return csvBuilder;
        }

        // Helper method to dynamically get count by vehicle type
        private double GetCountByType(VehicleByTypeChartResponse entry, string vehicleType)
        {
            return vehicleType switch
            {
                "Truck" => entry.TruckInCount,
                "MotorCycle" => entry.MotorCycleInCount,
                "Bus" => entry.BusInCount,
                "Bicycle" => entry.BicycleInCount,
                "Car" => entry.CarInCount,
                _ => 0
            };
        }

        private double GetCountByInOutType(CsvInOutResponseModel entry, string vehicleType)
        {
            return vehicleType switch
            {
                "In" => entry.InCount,
                "Out" => entry.OutCount,
                _ => 0
            };
        }

        private long GetCountByVisitor(NewVsTotalVisitorExcel entry, string vehicleType)
        {
            return vehicleType switch
            {
                "New Visitors" => entry.NewVisitorsCount,
                "Total Visitors" => entry.TotalVisitorsCount,
                _ => 0
            };
        }

        private async Task<StringBuilder> GenerateCSV(List<CsvResponseModel> allChartData, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);

            string[] formats = {
    "dd/MM/yyyy h:mm:ss tt",   // e.g. 26/08/2025 12:00:00 AM
    "MM/dd/yyyy h:mm:ss tt",   // e.g. 04/07/2025 7:00:00 pm
    "dd/MM/yyyy HH:mm:ss",      // e.g. 26/08/2025 00:00:00
    "MM/dd/yyyy HH:mm:ss",     // e.g. 04/07/2025 19:00:00
    "dd-MM-yyyy h:mm:ss tt",     // e.g. 04/07/2025 19:00:00
};

            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            //// Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.AverageIntervalMinute;

            csvBuilder.Append(GetHourHeaderForCsv("Date", obj.AverageIntervalMinute, slotsPerDay));

            try
            {
                // Preprocess: convert to (date, slot index) => count
                //var groupedData = allChartData
                //                  .Select(item =>
                //                  {
                //                      var dt = DateTime.Parse(item.DateTime);
                //                      int totalMinutes = dt.Hour * 60 + dt.Minute;
                //                      int slotIndex = totalMinutes / obj.AverageIntervalMinute;
                //                      return new
                //                      {
                //                          Date = dt.Date,
                //                          SlotIndex = slotIndex,
                //                          Count = item.Value
                //                      };
                //                  })
                //                  .GroupBy(x => x.Date)
                //                  .ToDictionary(
                //                      g => g.Key,
                //                      g => g
                //                          .GroupBy(x => x.SlotIndex)
                //                          .ToDictionary(
                //                              x => x.Key,
                //                              x => x.Sum(v => v.Count) // resolve duplicate slotIndex by summing values
                //                          )
                //                  );


                var groupedData = allChartData
                                  .Select(item =>
                                  {
                                      // Adjust format according to your input string
                                      var dt = DateTime.ParseExact(
                                                item.DateTime,
                                                formats,
                                                CultureInfo.InvariantCulture,
                                                DateTimeStyles.None
                                            );
                                      int totalMinutes = dt.Hour * 60 + dt.Minute;
                                      int slotIndex = totalMinutes / obj.AverageIntervalMinute;
                                      return new
                                      {
                                          Date = dt.Date,
                                          SlotIndex = slotIndex,
                                          Count = item.Value
                                      };
                                  })
                                  .GroupBy(x => x.Date)
                                  .ToDictionary(
                                      g => g.Key,
                                      g => g
                                          .GroupBy(x => x.SlotIndex)
                                          .ToDictionary(
                                              x => x.Key,
                                              x => x.Sum(v => v.Count)
                                          )
                                  );

                // Generate data rows
                csvBuilder.Append(BindHourDataForCsv(groupedData, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay));
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }

            return csvBuilder;
        }

        private async Task<StringBuilder> GenerateCumulativePeopleCSV(List<CsvResponseModel> allChartData, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();

            string[] formats = {
    "dd/MM/yyyy h:mm:ss tt",   // e.g. 26/08/2025 12:00:00 AM
    "MM/dd/yyyy h:mm:ss tt",   // e.g. 04/07/2025 7:00:00 pm
    "MM/dd/yyyy HH:mm:ss",     // e.g. 04/07/2025 19:00:00
    "dd/MM/yyyy HH:mm:ss",     // e.g. 26/08/2025 00:00:00
    "dd-MM-yyyy h:mm:ss tt",   // e.g. 26-08-2025 12:00:00 AM
};
            var offsetTimeStamp = await GetOffset(obj.UserId);

            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            //// Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.AverageIntervalMinute;

            csvBuilder.Append(GetHourHeaderForCsv("Date", obj.AverageIntervalMinute, slotsPerDay));

            try
            {
                // Preprocess: convert to (date, slot index) => count
                var groupedData = allChartData
                                   .Select(item =>
                                   {
                                       //var dt = DateTime.Parse(item.DateTime);
                                       var dt = DateTime.ParseExact(
                                               item.DateTime,
                                               formats,
                                               CultureInfo.InvariantCulture,
                                               DateTimeStyles.None
                                           );
                                       int totalMinutes = dt.Hour * 60 + dt.Minute;
                                       int slotIndex = totalMinutes / obj.AverageIntervalMinute;
                                       return new
                                       {
                                           Date = dt.Date,
                                           SlotIndex = slotIndex,
                                           Count = item.Value
                                       };
                                   })
                                   .GroupBy(x => x.Date)
                                   .ToDictionary(
                                       g => g.Key,
                                       g => g
                                           .GroupBy(x => x.SlotIndex)
                                           .ToDictionary(
                                               x => x.Key,
                                               x => x.Sum(v => v.Count) // resolve duplicate slotIndex by summing values
                                           )
                                   );


                // ✅ Apply cumulative difference adjustment
                var adjustedData = new Dictionary<DateTime, Dictionary<int, int>>();
                foreach (var dayEntry in groupedData)
                {
                    var slots = new Dictionary<int, int>();
                    int lastRawValue = 0;      // raw incoming value (from data)
                    int lastCumulative = 0;    // adjusted cumulative value

                    for (int i = 0; i < slotsPerDay; i++)
                    {
                        int currentValue = dayEntry.Value.ContainsKey(i) ? dayEntry.Value[i] : 0;
                        if (i == 0)
                        {
                            lastCumulative = currentValue;
                            slots[i] = currentValue;
                        }
                        else
                        {

                            slots[i] = currentValue + lastCumulative;
                            lastCumulative = slots[i];
                        }

                        //if (i == 0)
                        //{
                        //    // First slot of the day → set as is
                        //    lastRawValue = currentValue;
                        //    lastCumulative = currentValue;
                        //    slots[i] = lastCumulative;
                        //}
                        //else
                        //{
                        //    if (currentValue < lastRawValue)
                        //    {
                        //        // Reset case → add current value to cumulative
                        //        lastCumulative += currentValue;
                        //    }
                        //    else
                        //    {
                        //        // Difference case → add (current - lastRawValue) to cumulative
                        //        lastCumulative += (currentValue - lastRawValue);
                        //    }

                        //    // Update trackers
                        //    lastRawValue = currentValue;
                        //    slots[i] = lastCumulative;
                        //}
                    }

                    adjustedData[dayEntry.Key] = slots;
                }

                // Generate data rows with adjusted values
                csvBuilder.Append(BindHourDataForCsv(adjustedData, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay));
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }

            return csvBuilder;
        }


        private async Task<StringBuilder> GenerateCSVForPeopleInOut(List<CsvInOutResponseModel> data, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);

            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            //// Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.IntervalMinute;
            string headerData = "Type, Date";

            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.IntervalMinute, slotsPerDay, "Total"));


            // List of vehicle types
            var inOutTypes = new List<string> { "In", "Out" };

            // Process all data once and group by vehicle type
            var groupedByVehicleType = new Dictionary<string, Dictionary<DateTime, Dictionary<double, double>>>();

            // Initialize dictionary for each vehicle type
            foreach (var vehicleType in inOutTypes)
            {
                groupedByVehicleType[vehicleType] = new Dictionary<DateTime, Dictionary<double, double>>();
            }

            data = data.Select(x => new CsvInOutResponseModel
            {
                DateTime = GetBucketTime(x.DateTime.Value.Add(offsetTimeStamp), obj.IntervalMinute),
                InCount = x.InCount,
                OutCount = x.OutCount
            }).GroupBy(x => x.DateTime)
            .Select(x => new CsvInOutResponseModel
            {
                DateTime = x.Key,
                InCount = Math.Round(x.Sum(y => y.InCount), 2),
                OutCount = Math.Round(x.Sum(y => y.OutCount), 2)
            }).ToList();

            // Process data once for all vehicle types
            foreach (var record in data.Where(d => d.DateTime.HasValue))
            {
                var dt = record.DateTime.Value;
                int totalMinutes = dt.Hour * 60 + dt.Minute;
                int slotIndex = totalMinutes / obj.IntervalMinute;
                var date = dt.Date;

                foreach (var vehicleType in inOutTypes)
                {
                    var count = GetCountByInOutType(record, vehicleType);
                    if (count > 0) // Only process if there's actual data
                    {
                        if (!groupedByVehicleType[vehicleType].ContainsKey(date))
                        {
                            groupedByVehicleType[vehicleType][date] = new Dictionary<double, double>();
                        }

                        if (!groupedByVehicleType[vehicleType][date].ContainsKey(slotIndex))
                        {
                            groupedByVehicleType[vehicleType][date][slotIndex] = 0;
                        }

                        groupedByVehicleType[vehicleType][date][slotIndex] += count;
                    }
                }
            }

            // Generate CSV for each vehicle type
            foreach (var vehicleType in inOutTypes)
            {
                csvBuilder.Append(BindHourDataCalcultorForCsv(groupedByVehicleType[vehicleType], obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay, vehicleType, true));
            }

            return csvBuilder;
        }

        private async Task<StringBuilder> GenerateCSVForEvent(IEnumerable<EventQueueAnalysis> data, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);

            // Convert start and end dates to offset timezone
            var offsetStartDate = obj.StartDate.Add(offsetTimeStamp);
            var offsetEndDate = obj.EndDate.Add(offsetTimeStamp);

            csvBuilder.Append(await GetTitleForCsv(widgetName, offsetStartDate, offsetEndDate, obj.UserId));

            int slotsPerDay = (24 * 60) / obj.IntervalMinute;
            csvBuilder.Append(GetHourHeaderForCsv("Date", obj.IntervalMinute, slotsPerDay, "Total"));

            // Convert all data to offset timezone FIRST, then group
            var offsetData = data.Select(d => new
            {
                OffsetDateTime = d.DateTime.Add(offsetTimeStamp),
                QueueCount = d.QueueCount
            });

            // Group by offset Date, then by time slot based on interval
            var groupedByDate = offsetData
                .GroupBy(d => d.OffsetDateTime.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(d => CalculateTimeSlot(d.OffsetDateTime, obj.IntervalMinute))
                          .ToDictionary(h => h.Key, h => h.Sum(x => x.QueueCount))
                );

            csvBuilder.Append(BindHourDataCalcultorForCsvEvent(groupedByDate, offsetStartDate, offsetEndDate, slotsPerDay, "", true));

            return csvBuilder;
        }

        // Helper method to calculate time slot based on interval
        private int CalculateTimeSlot(DateTime dateTime, int intervalMinutes)
        {
            // Calculate total minutes since start of day
            int totalMinutes = dateTime.Hour * 60 + dateTime.Minute;

            // Calculate which slot this time falls into
            int slotIndex = totalMinutes / intervalMinutes;

            return slotIndex;
        }
        //private async Task<StringBuilder> GenerateCSVForEvent(IEnumerable<EventQueueAnalysis> data, string widgetName, WidgetRequest obj)
        //{
        //    var csvBuilder = new StringBuilder();
        //    var offsetTimeStamp = await GetOffset(obj.UserId);

        //    csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

        //    int slotsPerDay = (24 * 60) / obj.IntervalMinute;

        //    csvBuilder.Append(GetHourHeaderForCsv("Date", obj.IntervalMinute, slotsPerDay));

        //    // Group by Date, then Hour
        //    var groupedByDate = data
        //        .GroupBy(d => d.DateTime.Add(offsetTimeStamp).Date)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.GroupBy(d => d.DateTime.Hour)
        //                  .ToDictionary(h => h.Key, h => h.Sum(x => x.QueueCount))
        //    );

        //    csvBuilder.Append(BindHourDataForCsv(groupedByDate, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay));

        //    return csvBuilder;
        //}

        private async Task<string> GetTitleForCsv(string widgetName, DateTime startDate, DateTime endDate, string userId)
        {
            var csvBuilder = new StringBuilder();
            var userData = await _usersService.GetUserAsync(userId);

            csvBuilder.AppendLine($"Widget name: {widgetName}");
            csvBuilder.AppendLine($"\"Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}\"");
            csvBuilder.AppendLine($"Exported By: {userData.Firstname} {userData.Lastname}");
            csvBuilder.AppendLine($"\"Exported On: {DateTime.Now:MMM dd, yyyy hh:mm tt}\"");

            return csvBuilder.ToString();
        }

        private string GetHourHeaderForCsv(string headerData, int minuteInterval, int slotsPerDay, string otherHeaders = "")
        {
            var csvBuilder = new StringBuilder();

            // Generate header
            var header = new List<string> { headerData };
            for (int i = 0; i < slotsPerDay; i++)
            {
                var startTime = TimeSpan.FromMinutes(i * minuteInterval);
                var endTime = startTime.Add(TimeSpan.FromMinutes(minuteInterval - 1));
                header.Add($"{startTime:hh\\:mm} - {endTime:hh\\:mm}");
            }
            if (!string.IsNullOrEmpty(otherHeaders))
            {
                header.Add(otherHeaders);
            }
            csvBuilder.AppendLine(string.Join(",", header));
            return csvBuilder.ToString();
        }
        private string BindHourDataForCsv(Dictionary<DateTime, Dictionary<int, int>> data, DateTime startDate, DateTime endDate, int slotsPerDay, string rowName = "", bool addTotal = false)
        {
            var csvBuilder = new StringBuilder();
            var currentDate = startDate.Date;
            //if (data.Count() > 0)
            //{
            while (currentDate <= endDate.Date)
            {
                var row = new List<string>();
                if (string.IsNullOrEmpty(rowName))
                {
                    row = new List<string> { currentDate.ToString("dd-MMM-yy") };
                }
                else
                {
                    row = new List<string> { rowName, currentDate.ToString("dd-MMM-yy") };
                }

                int totalCount = 0;
                for (int slot = 0; slot < slotsPerDay; slot++)
                {
                    if (data.TryGetValue(currentDate, out var slotData) &&
                        slotData.TryGetValue(slot, out var count))
                    {
                        row.Add(count.ToString());
                        totalCount += count;
                    }
                    else
                    {
                        row.Add("0");
                    }
                }

                if (addTotal)
                {
                    row.Add(totalCount.ToString());
                }

                csvBuilder.AppendLine(string.Join(",", row));
                currentDate = currentDate.AddDays(1);
            }
            //}
            return csvBuilder.ToString();
        }
        private string BindHourDataCalcultorForCsv(Dictionary<DateTime, Dictionary<double, double>> data, DateTime startDate, DateTime endDate, int slotsPerDay, string rowName = "", bool addTotal = false)
        {
            var csvBuilder = new StringBuilder();
            var currentDate = startDate.Date;
            //if (data.Count() > 0)
            //{
            while (currentDate <= endDate.Date)
            {
                var row = new List<string>();
                if (string.IsNullOrEmpty(rowName))
                {
                    row = new List<string> { currentDate.ToString("dd-MMM-yy") };
                }
                else
                {
                    row = new List<string> { rowName, currentDate.ToString("dd-MMM-yy") };
                }

                double totalCount = 0;
                for (int slot = 0; slot < slotsPerDay; slot++)
                {
                    if (data.TryGetValue(currentDate, out var slotData) &&
                        slotData.TryGetValue(slot, out var count))
                    {
                        row.Add(count.ToString());
                        totalCount += count;
                    }
                    else
                    {
                        row.Add("0");
                    }
                }

                if (addTotal)
                {
                    row.Add(totalCount.ToString("F2"));
                }

                csvBuilder.AppendLine(string.Join(",", row));
                currentDate = currentDate.AddDays(1);
            }
            //}
            return csvBuilder.ToString();
        }

        private string BindHourDataCalcultorForCsvEvent(Dictionary<DateTime, Dictionary<int, double>> data, DateTime startDate, DateTime endDate, int slotsPerDay, string rowName = "", bool addTotal = false)
        {
            var csvBuilder = new StringBuilder();
            var currentDate = startDate.Date;
            //if (data.Count() > 0)
            //{
            while (currentDate <= endDate.Date)
            {
                var row = new List<string>();
                if (string.IsNullOrEmpty(rowName))
                {
                    row = new List<string> { currentDate.ToString("dd-MMM-yy") };
                }
                else
                {
                    row = new List<string> { rowName, currentDate.ToString("dd-MMM-yy") };
                }

                double totalCount = 0;
                for (int slot = 0; slot < slotsPerDay; slot++)
                {
                    if (data.TryGetValue(currentDate, out var slotData) &&
                        slotData.TryGetValue(slot, out var count))
                    {
                        row.Add(count.ToString());
                        totalCount += count;
                    }
                    else
                    {
                        row.Add("0");
                    }
                }

                if (addTotal)
                {
                    row.Add(totalCount.ToString("F2"));
                }

                csvBuilder.AppendLine(string.Join(",", row));
                currentDate = currentDate.AddDays(1);
            }
            //}
            return csvBuilder.ToString();
        }

        public async Task<StringBuilder> GenerateCSVForVehicleTurningMovement(IEnumerable<VehicleTurningMovementResponse> data, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);
            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            // Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.IntervalMinute;
            string headerData = "Turning Movement, Date";
            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.IntervalMinute, slotsPerDay));

            // List of vehicle types
            var directionTypes = new List<string> { "Left", "Right", "Straight" };

            // Process all data once and group by vehicle type
            var groupedByVisitor = new Dictionary<string, Dictionary<DateTime, Dictionary<int, int>>>();

            // Initialize dictionary for each vehicle type
            foreach (var vehicleType in directionTypes)
            {
                groupedByVisitor[vehicleType] = new Dictionary<DateTime, Dictionary<int, int>>();
            }

            // Process data once for all vehicle types
            foreach (var record in data.Where(d => d.DateTime.HasValue))
            {
                var dt = record.DateTime.Value.Add(offsetTimeStamp);
                int totalMinutes = dt.Hour * 60 + dt.Minute;
                int slotIndex = totalMinutes / obj.IntervalMinute;
                var date = dt.Date;

                foreach (var vehicleType in directionTypes)
                {
                    var count = GetDirectionCount(record, vehicleType);
                    if (count > 0) // Only process if there's actual data
                    {
                        if (!groupedByVisitor[vehicleType].ContainsKey(date))
                        {
                            groupedByVisitor[vehicleType][date] = new Dictionary<int, int>();
                        }

                        if (!groupedByVisitor[vehicleType][date].ContainsKey(slotIndex))
                        {
                            groupedByVisitor[vehicleType][date][slotIndex] = 0;
                        }

                        groupedByVisitor[vehicleType][date][slotIndex] += Convert.ToInt32(count);
                    }
                }
            }

            // Generate CSV for each direction type
            foreach (var vehicleType in directionTypes)
            {
                csvBuilder.Append(BindHourDataForCsv(groupedByVisitor[vehicleType], obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay, vehicleType));
            }
            return csvBuilder;
        }

        private int GetDirectionCount(VehicleTurningMovementResponse entry, string direction)
        {
            return direction switch
            {
                "Left" => entry.Left,
                "Right" => entry.Right,
                "Straight" => entry.Straight,
                _ => 0
            };
        }

        public async Task<HeatmapWidgetResponse> HeatMapWidgetDataAsync(WidgetHeatmapRequest widgetRequest)
        {
            HeatmapWidgetResponse result = new HeatmapWidgetResponse();
            bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            var heatmapData = await _heatMapRepository.HeatMapWidgetDataAsync(widgetRequest, isarchived);
            var device = await _deviceMasterRepository.GetFromLinkedServerAsync(widgetRequest.DeviceId);
            if (device == null) return null;
            var heatmapImage = await GetDeviceHeatmapImage(device);

            result.ResolutionWidth = heatmapData.FirstOrDefault()?.ResolutionWidth ?? 0;
            result.ResolutionHeight = heatmapData.FirstOrDefault()?.ResolutionHeight ?? 0;
            int dataLength = result.ResolutionWidth * result.ResolutionHeight;
            result.HeatmapImage = heatmapImage;
            if (heatmapData.All(h => h.HeatMapData.Count == dataLength))
            {
                result.HeatMapData = Enumerable.Range(0, dataLength)
                    .Select(i => heatmapData.Sum(h => h.HeatMapData[i]))
                    .ToList();
            }
            else
            {
                result.HeatMapData = new List<int>();
            }
            return result;
        }


        public async Task<string> GetDeviceHeatmapImage(DeviceMaster device)
        {
            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(device.UserName, device.Password),
                PreAuthenticate = true
            };

            using (var httpClient = new HttpClient(handler))
            {
                try
                {
                    var response = await httpClient.GetAsync((device.IsHttps ? "https://" : "http://") + device.IpAddress + SunapiAPIConstant.HeatmapImage);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to fetch snapshot. Status: {response.StatusCode}");
                        return null;
                    }

                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    var base64Image = Convert.ToBase64String(imageBytes);

                    return $"data:image/jpeg;base64,{base64Image}";
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<PeopleVehicleInOutTotal> PeopleIOnOutTotalForReportAsync(DateTime startdate, DateTime enddate)
        {
            var data = await _peopleCountRepository.GetPeopleCountForReportAsync(startdate, enddate);
            return data;
        }

        public async Task<PeopleVehicleInOutTotal> VehicleInOutTotalForReportAsync(DateTime startdate, DateTime enddate)
        {
            var data = await _vehicleRepository.GetVehicleCountForReportAsync(startdate, enddate);
            return data;
        }

        //public async Task<StringBuilder> DownloadMultipleWidgetsCsvAsync(WidgetRequest widgetRequest)
        //{
        //    var combinedCsv = new StringBuilder();
        //    var sw2 = Stopwatch.StartNew();
        //    if (widgetRequest.WidgetTitleNames != null)
        //    {
        //        foreach (var widgetName in widgetRequest.WidgetTitleNames)
        //        {
        //            widgetRequest.WidgetName = widgetName.Title;

        //            var csvBuilder = widgetName.Id switch
        //            {
        //                //Camera
        //                "TotalCameraCount" => await TotalCameraCountCsv(widgetRequest),
        //                "CameraCountByModel" => await CameraCountByModelCsv(widgetRequest),
        //                "CameraCountByFeatures" => await CameraCountByFeaturesCsv(widgetRequest),

        //                //Site
        //                "PeopleCameraCapacityUtilizationAnalysisByZones" => await PeopleCameraCapacityUtilizationByZonesCsv(widgetRequest),
        //                "PeopleCapacityUtilization" => await PeopleCapacityUtilizationCsv(widgetRequest),
        //                "VehicleCapacityUtilization" => await VehicleCapacityUtilizationCsv(widgetRequest),
        //                "VehicleCameraCapacityUtilizationAnalysisByZones" => await VehicleCameraCapacityUtilizationByZonesCsv(widgetRequest),

        //                //People
        //                "PeopleCountChart" => await PeopleInOutCountCSVDownload(widgetRequest),
        //                "AveragePeopleCountChart" => await AveragePeopleCountCSVDownload(widgetRequest),
        //                "PeopleCountByZones" => await PeopleCountByZonesCsv(widgetRequest),
        //                "NewVsTotalVisitorChart" => await NewVsTotalVisitorCsv(widgetRequest),
        //                "SlipFallAnalysis" => await SlipFallQueueCsvDataAsync(widgetRequest),
        //                "GenderWisePeopleCountAnalysis" => await GenderWisePeopleCountAnalysisDataCsv(widgetRequest),
        //                "CumulativePeopleCountChart" => await CumulativePeopleCountCSVDownload(widgetRequest),

        //                //Vehicle
        //                "VehicleCountChart" => await VehicleInOutCountCSVDownload(widgetRequest),
        //                "AverageVehicleCountChart" => await AvgVehicleCountCSVDownload(widgetRequest),
        //                "VehicleByType" => await VehicleByTypeLineChartCsv(widgetRequest),
        //                "WrongWayAnalysis" => await WrongWayQueueAnalysisCsvData(widgetRequest),
        //                "VehicleUTurnAnalysis" => await VehicleUTurnAnalysisCsvData(widgetRequest),
        //                "PedestrianAnalysis" => await PedestrianQueueAnalysisCsvData(widgetRequest),
        //                "VehicleQueueAnalysis" => await VehicleQueueAnalysisCsvData(widgetRequest),
        //                "StoppedVehicleByTypeAnalysis" => await StoppedVehicleByTypeAnalysisCsvData(widgetRequest),
        //                "VehicleTurningMovementAnalysis" => await VehicleTurningMovementAnalysisCsvData(widgetRequest),
        //                "VehicleSpeedViolationAnalysis" => await VehicleSpeedViolationAnalysisCsvData(widgetRequest),
        //                "TrafficJamAnalysis" => await TrafficJamAnalysisCsvData(widgetRequest),

        //                //Retail
        //                "ShoppingCartQueueAnalysis" => await ShoppingCartQueueAnalysisCsvData(widgetRequest),
        //                "ShoppingCartCountAnalysis" => await ShoppingCartCountAnalysisCsvData(widgetRequest),
        //                "PeopleQueueAnalysis" => await PeopleQueueAnalysisCsvData(widgetRequest),
        //                "BlockedExitAnalysis" => await BlockedExitAnalysisCsvData(widgetRequest),

        //                //Factory
        //                "ForkliftCountAnalysis" => await ForkliftCountAnalysisCsvData(widgetRequest),
        //                "ForkliftQueueAnalysis" => await ForkliftQueueAnalysisCsvData(widgetRequest),
        //                "ProxomityDetectionAnalysis" => await ProxomityDetectionAnalysisCsvData(widgetRequest),
        //                "FactoryBlockedExitAnalysis" => await BlockedExitAnalysisCsvData(widgetRequest),
        //                // Add more cases if needed
        //                _ => null
        //            };

        //            if (csvBuilder != null)
        //            {
        //                combinedCsv.AppendLine(csvBuilder.ToString());
        //                combinedCsv.AppendLine(); // Optional spacing
        //            }
        //        }
        //    }
        //    sw2.Stop();
        //    Console.WriteLine($"Main Mathod Ended: {sw2.Elapsed.TotalMinutes:F1}s");
        //    return combinedCsv;
        //}

        public async Task<StringBuilder> DownloadMultipleWidgetsCsvAsync(WidgetRequest widgetRequest)
        {
            var combinedCsv = new StringBuilder();

            if (widgetRequest.WidgetTitleNames != null)
            {
                var tasks = widgetRequest.WidgetTitleNames.Select(widgetName =>
                {
                    // Clone or create a shallow copy of widgetRequest for each task
                    var localRequest = new WidgetRequest
                    {
                        // Copy relevant properties
                        UserId = widgetRequest.UserId,
                        FloorIds = widgetRequest.FloorIds,
                        ZoneIds = widgetRequest.ZoneIds,
                        userRoles = widgetRequest.userRoles,
                        StartDate = widgetRequest.StartDate,
                        EndDate = widgetRequest.EndDate,
                        AverageIntervalMinute = widgetRequest.AverageIntervalMinute,
                        IntervalMinute = widgetRequest.IntervalMinute,
                        // ... copy other needed fields ...
                        WidgetName = widgetName.Title,
                        WidgetTitleNames = null, // avoid recursion/duplication
                        calculatorExportRule = widgetRequest.calculatorExportRule

                    };

                    return widgetName.Id switch
                    {
                        "TotalCameraCount" => TotalCameraCountCsv(localRequest),
                        "CameraCountByModel" => CameraCountByModelCsv(localRequest),
                        "CameraCountByFeatures" => CameraCountByFeaturesCsv(localRequest),

                        "PeopleCameraCapacityUtilizationAnalysisByZones" => PeopleCameraCapacityUtilizationByZonesCsv(localRequest),
                        "PeopleCapacityUtilization" => PeopleCapacityUtilizationCsv(localRequest),
                        "VehicleCapacityUtilization" => VehicleCapacityUtilizationCsv(localRequest),
                        "VehicleCameraCapacityUtilizationAnalysisByZones" => VehicleCameraCapacityUtilizationByZonesCsv(localRequest),

                        "PeopleCountChart" => PeopleInOutCountCSVDownload(localRequest),
                        //"PeopleCountChart" => PeopleInOutCountForAdvanceCSVDownload(localRequest),
                        "AveragePeopleCountChart" => AveragePeopleCountCSVDownload(localRequest),
                        "PeopleCountByZones" => PeopleCountByZonesCsv(localRequest),
                        "NewVsTotalVisitorChart" => NewVsTotalVisitorCsv(localRequest),
                        "SlipFallAnalysis" => SlipFallQueueCsvDataAsync(localRequest),
                        "GenderWisePeopleCountAnalysis" => GenderWisePeopleCountAnalysisDataCsv(localRequest),
                        "CumulativePeopleCountChart" => CumulativePeopleCountCSVDownload(localRequest),
                        "Age" => AgeWisePeopleCountAnalysisDataCsv(localRequest),

                        "VehicleCountChart" => VehicleInOutCountCSVDownload(localRequest),
                        "AverageVehicleCountChart" => AvgVehicleCountCSVDownload(localRequest),
                        "VehicleByType" => VehicleByTypeLineChartCsv(localRequest),
                        "WrongWayAnalysis" => WrongWayQueueAnalysisCsvData(localRequest),
                        "VehicleUTurnAnalysis" => VehicleUTurnAnalysisCsvData(localRequest),
                        "PedestrianAnalysis" => PedestrianQueueAnalysisCsvData(localRequest),
                        "VehicleQueueAnalysis" => VehicleQueueAnalysisCsvData(localRequest),
                        "StoppedVehicleByTypeAnalysis" => StoppedVehicleByTypeAnalysisCsvData(localRequest),
                        "VehicleTurningMovementAnalysis" => VehicleTurningMovementAnalysisCsvData(localRequest),
                        "VehicleSpeedViolationAnalysis" => VehicleSpeedViolationAnalysisCsvData(localRequest),
                        "TrafficJamAnalysis" => TrafficJamAnalysisCsvData(localRequest),

                        "ShoppingCartQueueAnalysis" => ShoppingCartQueueAnalysisCsvData(localRequest),
                        "ShoppingCartCountAnalysis" => ShoppingCartCountAnalysisCsvData(localRequest),
                        "PeopleQueueAnalysis" => PeopleQueueAnalysisCsvData(localRequest),
                        "BlockedExitAnalysis" => BlockedExitAnalysisCsvData(localRequest),

                        "ForkliftCountAnalysis" => ForkliftCountAnalysisCsvData(localRequest),
                        "ForkliftQueueAnalysis" => ForkliftQueueAnalysisCsvData(localRequest),
                        "ProxomityDetectionAnalysis" => ProxomityDetectionAnalysisCsvData(localRequest),
                        "FactoryBlockedExitAnalysis" => BlockedExitAnalysisCsvData(localRequest),
                        "CamerasInRMA" => _rmaService.DownloadRmaMaintenanceCSV(localRequest),
                        "CameraMaintenanceStatus" => GetMaintenanceStatusCsv(localRequest),
                        "CamerasInMaintenance" => GetCameraInMaintenanceAsyncCsv(localRequest),
                        "CameraDisconnectionTracker" => CameraDisconnectedTrackerCSVData(localRequest),
                        "Parking" => VehicleParkingAnalysisDataCsvAsync(localRequest),
                        "APIError" => _deviceExeceptionWidgetService.DeviceExceptionDataCSVAsync(localRequest),

                        _ => Task.FromResult<StringBuilder>(null)
                    };
                });

                // Run all tasks in parallel
                var results = await Task.WhenAll(tasks);

                foreach (var csvBuilder in results)
                {
                    if (csvBuilder != null && csvBuilder.Length != 0)
                    {
                        combinedCsv.AppendLine(csvBuilder.ToString());
                        combinedCsv.AppendLine();
                    }
                }
            }
            return combinedCsv;
        }

        private async Task<TimeSpan> GetOffset(string userId)
        {
            var timeZone = await _usersService.GetTimeZone(userId);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
            return offsetTimeStamp;
        }

        public async Task<int> ProcessHetmapRetentionData(int retentionPeriod)
        {
            var data = await RetentionHelper.ProcessRetentionData<HeatMap, HeatMapArchive>(
                             _heatMapRepository,                // IRepositoryBase<VehicleCount>
                             _heatMapArchiveRepository,    // IRepositoryBase<VehicleCountArchive>
                             _heatMapRepository,                // IRetentionRepository<VehicleCount>
                             x => new HeatMapArchive
                             {
                                 Id = x.Id,
                                 DeviceId = x.DeviceId,
                                 CameraIP = x.CameraIP,
                                 ChannelNo = x.ChannelNo,
                                 HeatMapType = x.HeatMapType,
                                 ResolutionHeight = x.ResolutionHeight,
                                 ResolutionWidth = x.ResolutionWidth,
                                 HeatMapData = x.HeatMapData,
                                 IsDeleted = x.IsDeleted,
                                 DeletedOn = x.DeletedOn,
                                 CreatedOn = x.CreatedOn,
                                 CreatedBy = x.CreatedBy,
                                 UpdatedOn = x.UpdatedOn,
                                 UpdatedBy = x.UpdatedBy
                             },
                            retentionPeriod: retentionPeriod);

            return data;
        }
        public async Task<int> ProcessQueueManagementRetentionData(int retentionPeriod)
        {
            var data = await RetentionHelper.ProcessRetentionData<QueueManagement, QueueManagementArchive>(
                             _queueManagementRepository,                // IRepositoryBase<VehicleCount>
                             _queueManagementArchiveRepository,    // IRepositoryBase<VehicleCountArchive>
                             _queueManagementRepository,                // IRetentionRepository<VehicleCount>
                             x => new QueueManagementArchive
                             {
                                 Id = x.Id,
                                 DeviceId = x.DeviceId,
                                 ChannelNo = x.ChannelNo,
                                 RuleIndex = x.RuleIndex,
                                 Count = x.Count,
                                 RuleName = x.RuleName,
                                 EventName = x.EventName,
                                 IsDeleted = x.IsDeleted,
                                 DeletedOn = x.DeletedOn,
                                 CreatedOn = x.CreatedOn,
                                 CreatedBy = x.CreatedBy,
                                 UpdatedOn = x.UpdatedOn,
                                 UpdatedBy = x.UpdatedBy
                             },
                            retentionPeriod: retentionPeriod);

            return data;
        }
        public async Task<int> ProcessDeviceEventRetentionData(int retentionPeriod)
        {
            var data = await RetentionHelper.ProcessRetentionData<DeviceEvents, DeviceEventsArchive>(
                             _deviceEventsRepository,                // IRepositoryBase<VehicleCount>
                             _deviceEventsArchiveRepository,    // IRepositoryBase<VehicleCountArchive>
                             _deviceEventsRepository,                // IRetentionRepository<VehicleCount>
                             x => new DeviceEventsArchive
                             {
                                 Id = x.Id,
                                 EventName = x.EventName,
                                 DeviceId = x.DeviceId,
                                 ChannelNo = x.ChannelNo,
                                 DeviceTime = x.DeviceTime,
                                 RuleName = x.RuleName,
                                 VideoSourceToken = x.VideoSourceToken,
                                 PeopleQueueData = x.PeopleQueueData,
                                 ShoppingCartQueue = x.ShoppingCartQueue,
                                 ForkLiftQueue = x.ForkLiftQueue,
                                 SlipFallDetection = x.SlipFallDetection,
                                 WrongWayDetection = x.WrongWayDetection,
                                 PedestrianDetection = x.PedestrianDetection,
                                 BlockedExitDetection = x.BlockedExitDetection,
                                 VehicleSpeedDetection = x.VehicleSpeedDetection,
                                 TrafficJamDetection = x.TrafficJamDetection,
                                 StoppedVehicleByType = x.StoppedVehicleByType,
                                 ProximityDetection = x.ProximityDetection,
                                 ForkliftSpeedDetection = x.ForkliftSpeedDetection,
                                 FaceMaskDetection = x.FaceMaskDetection,
                                 IsAcknowledged = x.IsAcknowledged,
                                 IsDeleted = x.IsDeleted,
                                 DeletedOn = x.DeletedOn,
                                 CreatedOn = x.CreatedOn,
                                 CreatedBy = x.CreatedBy,
                                 UpdatedOn = x.UpdatedOn,
                                 UpdatedBy = x.UpdatedBy
                             },
                            retentionPeriod: retentionPeriod);

            return data;
        }

        public async Task<StringBuilder> CameraDisconnectedTrackerCSVData(WidgetRequest widgetRequest)
        {
            StringBuilder csvBuilder = new StringBuilder();
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();
            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);
            var disConnectedCameraDataResult = await CameraDisconnectedTrackerAnalysisAsync(widgetRequest);
            var disConnectedCameraData = disConnectedCameraDataResult.trackerAnalsisRes;

            if (disConnectedCameraData != null)
            {
                disConnectedCameraData = disConnectedCameraData.Select(r => new CameraDisconnectedTrackerAnalsisResponse
                {
                    DeviceId = r.DeviceId,
                    // Apply offset ONLY to StartDate
                    OfflineTime = r.OfflineTime + offsetTimeStamp,
                    OnlineTime = r.OnlineTime + offsetTimeStamp,
                    // EndDate remains unchanged
                    //EndDate = r.EndDate
                }).ToList();

                List<CameraDisconnectedTrackerCSV> lstDisconnectedCamera = new List<CameraDisconnectedTrackerCSV>();

                var dateRange = GenerateDatePoints(widgetRequest.StartDate + offsetTimeStamp, widgetRequest.EndDate + offsetTimeStamp, widgetRequest.AverageIntervalMinute);


                for (int i = 0; i < dateRange.Count(); i++)
                {
                    var bucketStart = dateRange[i];
                    int offlineCount = 0;

                    var intervalInMinutes =
                        (int)(dateRange[1] - dateRange[0]).TotalMinutes;

                    var bucketEnd = bucketStart.AddMinutes(intervalInMinutes);


                    foreach (var disConnectedCamera in disConnectedCameraData)
                    {
                        var startDate = disConnectedCamera.OfflineTime;

                        DateTime? endDate = disConnectedCamera.OnlineTime != null ? disConnectedCamera.OnlineTime : (DateTime?)null;


                        // If OnlineTime is null → still in progress → use current time
                        var effectiveEnd = endDate ?? DateTime.UtcNow + offsetTimeStamp;

                        if (bucketStart < effectiveEnd && bucketEnd > startDate)
                        {
                            offlineCount++;
                        }
                    }


                    // ✅ Single record per date
                    lstDisconnectedCamera.Add(new CameraDisconnectedTrackerCSV
                    {
                        DateTime = dateRange[i],
                        Offline = offlineCount,
                    });
                }

                if (lstDisconnectedCamera.Count() > 0)
                {
                    csvBuilder = await GenerateCSVForDisConnectedCamera(lstDisconnectedCamera, widgetRequest.WidgetName, widgetRequest);
                }

            }
            return csvBuilder;

        }

        private async Task<StringBuilder> GenerateCSVForDisConnectedCamera(List<CameraDisconnectedTrackerCSV> data, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);

            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            //// Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.AverageIntervalMinute;
            string headerData = "Type, Date";

            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.AverageIntervalMinute, slotsPerDay));


            // List of vehicle types
            var inOutTypes = new List<string> { "Offline" };

            // Process all data once and group by vehicle type
            var groupedByType = new Dictionary<string, Dictionary<DateTime, Dictionary<int, int>>>();

            // Initialize dictionary for each vehicle type
            foreach (var vehicleType in inOutTypes)
            {
                groupedByType[vehicleType] = new Dictionary<DateTime, Dictionary<int, int>>();
            }

            data = data.Select(x => new CameraDisconnectedTrackerCSV
            {
                DateTime = x.DateTime.Value,
                Offline = x.Offline,
            }).GroupBy(x => x.DateTime)
            .Select(x => new CameraDisconnectedTrackerCSV
            {
                DateTime = x.Key,
                Offline = x.Sum(y => y.Offline),
            }).ToList();

            // Process data once for all vehicle types
            foreach (var record in data.Where(d => d.DateTime.HasValue))
            {
                var dt = record.DateTime.Value;
                int totalMinutes = dt.Hour * 60 + dt.Minute;
                int slotIndex = totalMinutes / obj.AverageIntervalMinute;
                var date = dt.Date;

                foreach (var vehicleType in inOutTypes)
                {
                    var count = GetDisConnectedType(record, vehicleType);
                    if (count > 0) // Only process if there's actual data
                    {
                        if (!groupedByType[vehicleType].ContainsKey(date))
                        {
                            groupedByType[vehicleType][date] = new Dictionary<int, int>();
                        }

                        if (!groupedByType[vehicleType][date].ContainsKey(slotIndex))
                        {
                            groupedByType[vehicleType][date][slotIndex] = 0;
                        }

                        groupedByType[vehicleType][date][slotIndex] += Convert.ToInt32(count);
                    }
                }
            }

            // Generate CSV for each vehicle type
            foreach (var vehicleType in inOutTypes)
            {
                csvBuilder.Append(BindHourDataForCsv(groupedByType[vehicleType], obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay, vehicleType));
            }

            return csvBuilder;
        }

        public static List<DateTime> GenerateDatePoints(DateTime startDate, DateTime endDate, int intervalMinutes)
        {
            List<DateTime> datePoints = new List<DateTime>();

            DateTime currentDate = startDate;

            while (currentDate < endDate)
            {
                datePoints.Add(currentDate);
                currentDate = currentDate.AddMinutes(intervalMinutes);
            }

            return datePoints;
        }

        private double GetDisConnectedType(CameraDisconnectedTrackerCSV entry, string vehicleType)
        {
            return vehicleType switch
            {
                "Offline" => entry.Offline,
                _ => 0
            };
        }

        public async Task<List<MaintenanceStatusWidgetResponse>> GetMaintenanceStatusWidgetData(WidgetRequest widgetRequest)
        {
            RMAWidgetRequest widgetRequest1 = new RMAWidgetRequest();
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();
            if (widgetRequest.FloorIds != null && widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
            }
            else
            {
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneLinkedServerAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
            }

            widgetRequest1.StartDate = widgetRequest.StartDate;
            widgetRequest1.EndDate = widgetRequest.EndDate;
            widgetRequest1.DeviceIds = deviceIds;
            var maintenanceScheduleDetails = await _maintenanceScheduleRepository.GetMaintenanceScheduleForWidget(widgetRequest1);

            List<MaintenanceStatusWidgetResponse> response = new List<MaintenanceStatusWidgetResponse>();
            response = maintenanceScheduleDetails.Select(x => new MaintenanceStatusWidgetResponse
            {
                DeviceId = x.DeviceId,
                DueDate = x.DueDate,
                LatestStatus = x.LatestStatus,
                StatusHistory = x.StatusHistory
            }).ToList();

            return response;
        }

        public async Task<StringBuilder> GetCameraInMaintenanceAsyncCsv(WidgetRequest widgetRequest)
        {
            StringBuilder csvBuilder = new StringBuilder();
            var data = await GetMaintenanceStatusWidgetData(widgetRequest);

            var flattened = data
                           .SelectMany(item => item.StatusHistory.Select(history => new
                           {
                               history.Status,
                               history.StatusDatetime
                           }))
                           .ToList();

            var groupedCameraMaintenanceData = flattened
                .GroupBy(f => f.StatusDatetime)
                .Select(g => new CameraInMaintenanceCSV
                {
                    DateTime = g.Key.Value,
                    InProgress = g.Count(x => x.Status == "In Progress"),
                    Rework = g.Count(x => x.Status == "Rework"),
                }).ToList().OrderBy(x => x.DateTime);

            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);


            var resultWithOffset = groupedCameraMaintenanceData
                .Select(x => new CameraInMaintenanceCSV
                {
                    DateTime = x.DateTime.Value.Add(offsetTimeStamp),
                    InProgress = x.InProgress,
                    Rework = x.Rework,
                }).ToList();


            csvBuilder = await GenerateCSVForCameraInMaintenance(resultWithOffset, widgetRequest.WidgetName, widgetRequest);

            return csvBuilder;
        }

        private async Task<StringBuilder> GenerateCSVForCameraInMaintenance(List<CameraInMaintenanceCSV> data, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);

            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            //// Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.AverageIntervalMinute;
            string headerData = "Type, Date";

            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.AverageIntervalMinute, slotsPerDay));


            // List of vehicle types
            var inOutTypes = new List<string> { "In Progress", "Rework" };

            // Process all data once and group by vehicle type
            var groupedByType = new Dictionary<string, Dictionary<DateTime, Dictionary<int, int>>>();

            // Initialize dictionary for each vehicle type
            foreach (var cameraMaintenanceType in inOutTypes)
            {
                groupedByType[cameraMaintenanceType] = new Dictionary<DateTime, Dictionary<int, int>>();
            }

            data = data.Select(x => new CameraInMaintenanceCSV
            {
                DateTime = x.DateTime,
                InProgress = x.InProgress,
                Rework = x.Rework,
            }).GroupBy(x => x.DateTime)
            .Select(x => new CameraInMaintenanceCSV
            {
                DateTime = x.Key,
                InProgress = x.Sum(y => y.InProgress),
                Rework = x.Sum(y => y.Rework),
            }).ToList();

            // Process data once for all vehicle types
            foreach (var record in data.Where(d => d.DateTime.HasValue))
            {
                var dt = record.DateTime.Value;
                int totalMinutes = dt.Hour * 60 + dt.Minute;
                int slotIndex = totalMinutes / obj.AverageIntervalMinute;
                var date = dt.Date;

                foreach (var cameraMaintenanceType in inOutTypes)
                {
                    var count = GetCountCameraInMaintenanceType(record, cameraMaintenanceType);
                    if (count > 0) // Only process if there's actual data
                    {
                        if (!groupedByType[cameraMaintenanceType].ContainsKey(date))
                        {
                            groupedByType[cameraMaintenanceType][date] = new Dictionary<int, int>();
                        }

                        if (!groupedByType[cameraMaintenanceType][date].ContainsKey(slotIndex))
                        {
                            groupedByType[cameraMaintenanceType][date][slotIndex] = 0;
                        }

                        groupedByType[cameraMaintenanceType][date][slotIndex] += Convert.ToInt32(count);
                    }
                }
            }

            // Generate CSV for each vehicle type
            foreach (var cameraMaintenanceType in inOutTypes)
            {
                csvBuilder.Append(BindHourDataForCsv(groupedByType[cameraMaintenanceType], obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay, cameraMaintenanceType));
            }

            return csvBuilder;
        }

        private double GetCountCameraInMaintenanceType(CameraInMaintenanceCSV entry, string cameraMaintenanceType)
        {
            return cameraMaintenanceType switch
            {
                "In Progress" => entry.InProgress,
                "Rework" => entry.Rework,
                _ => 0
            };
        }


        public async Task<StringBuilder> GetMaintenanceStatusCsv(WidgetRequest widgetRequest)
        {
            StringBuilder csvBuilder = new StringBuilder();
            var data = await GetMaintenanceStatusWidgetData(widgetRequest);
            //var data = new List<MaintenanceSchedule>();

            var flattened = data
                           .SelectMany(item => item.StatusHistory.Select(history => new
                           {
                               history.Status,
                               history.StatusDatetime
                           }))
                           .ToList();

            var groupedCameraMaintenanceData = flattened
                .GroupBy(f => f.StatusDatetime)
                .Select(g => new CameraMaintenanceStatusCSV
                {
                    DateTime = g.Key.Value,
                    NotStartedCount = g.Count(x => x.Status == "Not Started"),
                    InProgressCount = g.Count(x => x.Status == "In Progress"),
                    ReworkCount = g.Count(x => x.Status == "Rework"),
                    DoneCount = g.Count(x => x.Status == "Done")
                }).ToList().OrderBy(x => x.DateTime);

            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);


            var resultWithOffset = groupedCameraMaintenanceData
                .Select(x => new CameraMaintenanceStatusCSV
                {
                    DateTime = x.DateTime.Value.Add(offsetTimeStamp),
                    NotStartedCount = x.NotStartedCount,
                    InProgressCount = x.InProgressCount,
                    ReworkCount = x.ReworkCount,
                    DoneCount = x.DoneCount
                }).ToList();

            csvBuilder = await GenerateCSVForCameraMaintenanceStatus(resultWithOffset, widgetRequest.WidgetName, widgetRequest);

            return csvBuilder;
        }

        private async Task<StringBuilder> GenerateCSVForCameraMaintenanceStatus(List<CameraMaintenanceStatusCSV> data, string widgetName, WidgetRequest obj)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);

            csvBuilder.Append(await GetTitleForCsv(widgetName, obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), obj.UserId));

            //// Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.AverageIntervalMinute;
            string headerData = "Type, Date";

            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.AverageIntervalMinute, slotsPerDay));


            // List of vehicle types
            var inOutTypes = new List<string> { "Not Started", "In Progress", "Rework", "Done" };

            // Process all data once and group by vehicle type
            var groupedByType = new Dictionary<string, Dictionary<DateTime, Dictionary<int, int>>>();

            // Initialize dictionary for each vehicle type
            foreach (var vehicleType in inOutTypes)
            {
                groupedByType[vehicleType] = new Dictionary<DateTime, Dictionary<int, int>>();
            }

            data = data.Select(x => new CameraMaintenanceStatusCSV
            {
                DateTime = x.DateTime,
                NotStartedCount = x.NotStartedCount,
                InProgressCount = x.InProgressCount,
                ReworkCount = x.ReworkCount,
                DoneCount = x.DoneCount,
            }).GroupBy(x => x.DateTime)
            .Select(x => new CameraMaintenanceStatusCSV
            {
                DateTime = x.Key,
                NotStartedCount = x.Sum(y => y.NotStartedCount),
                InProgressCount = x.Sum(y => y.InProgressCount),
                ReworkCount = x.Sum(y => y.ReworkCount),
                DoneCount = x.Sum(y => y.DoneCount)
            }).ToList();

            // Process data once for all vehicle types
            foreach (var record in data.Where(d => d.DateTime.HasValue))
            {
                var dt = record.DateTime.Value;
                int totalMinutes = dt.Hour * 60 + dt.Minute;
                int slotIndex = totalMinutes / obj.AverageIntervalMinute;
                var date = dt.Date;

                foreach (var vehicleType in inOutTypes)
                {
                    var count = GetCountByCameraMaintenanceStatus(record, vehicleType);
                    if (count > 0) // Only process if there's actual data
                    {
                        if (!groupedByType[vehicleType].ContainsKey(date))
                        {
                            groupedByType[vehicleType][date] = new Dictionary<int, int>();
                        }

                        if (!groupedByType[vehicleType][date].ContainsKey(slotIndex))
                        {
                            groupedByType[vehicleType][date][slotIndex] = 0;
                        }

                        groupedByType[vehicleType][date][slotIndex] += Convert.ToInt32(count);
                    }
                }
            }

            // Generate CSV for each vehicle type
            foreach (var vehicleType in inOutTypes)
            {
                csvBuilder.Append(BindHourDataForCsv(groupedByType[vehicleType], obj.StartDate.Add(offsetTimeStamp), obj.EndDate.Add(offsetTimeStamp), slotsPerDay, vehicleType));
            }

            return csvBuilder;
        }

        public async Task<CameraInMaintenanceResDto> GetCameraInMaintenanceAsync(WidgetRequest widgetRequest)
        {
            if (widgetRequest == null)
                throw new ArgumentNullException(nameof(widgetRequest));

            IEnumerable<string> deviceIds = Enumerable.Empty<string>();
            if (widgetRequest.FloorIds != null && widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
            }
            else
            {
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneLinkedServerAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
            }

            var searchModel = new CameraInMaintenanceSearchDto
            {
                StartDate = widgetRequest.StartDate,
                EndDate = widgetRequest.EndDate,
                DeviceId = deviceIds
            };

            var maintenanceData =
                await _maintenanceScheduleRepository.GetCameraInMaintenance(searchModel);

            // Map to CameraInMaintenance
            var cameras = maintenanceData.Select(ms =>
            {
                var lastStatus = ms.StatusHistory?.LastOrDefault();

                return new CameraInMaintenance
                {
                    Id = ms.Id,
                    DeviceId = ms.DeviceId,
                    DueDate = ms.DueDate,
                    Status = lastStatus?.Status,
                    Notes = lastStatus?.Notes,
                    StatusDatetime = lastStatus?.StatusDatetime
                };
            });

            // Remove duplicate DeviceId
            var distinctCameras = cameras
                .Where(x => !string.IsNullOrEmpty(x.DeviceId))
                .GroupBy(x => x.DeviceId)
                .Select(g => g.First())
                .ToList();

            return new CameraInMaintenanceResDto
            {
                CameraInMaintenanceCount = distinctCameras.Count(),
                CameraInMaintenance = distinctCameras
            };
        }

        private double GetCountByCameraMaintenanceStatus(CameraMaintenanceStatusCSV entry, string vehicleType)
        {
            return vehicleType switch
            {
                "Not Started" => entry.NotStartedCount,
                "In Progress" => entry.InProgressCount,
                "Rework" => entry.ReworkCount,
                "Done" => entry.DoneCount,
                _ => 0
            };
        }
        private async Task<List<OptionModel<string, string>>> GetIpAddressReferenceData(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<DeviceMaster> projection = Builders<DeviceMaster>.Projection
            .Include("ipAddress")
            .Include("_id");

            var devices = await _deviceMasterRepository.GetManyAsync(ids, projection);
            options = devices.Select(x => new OptionModel<string, string>(x.Id, x.IpAddress)).ToList();
            return options;
        }

        public async Task<StringBuilder> VehicleParkingAnalysisDataCsvAsync(WidgetRequest widgetRequest)
        {
            var zoneData = await VehicleParkingByZonesAsync(widgetRequest);

            var totalZone = zoneData.Sum(x => x.TotalParkingOccupancy);

            var result = await VehicleParkingAnalysisDataAsync(widgetRequest);

            var csvBuilder = await GenerateCSVForVehicleParking(result, widgetRequest.WidgetName, widgetRequest, totalZone);
            return csvBuilder;

        }

        private async Task<StringBuilder> GenerateCSVForVehicleParking(IEnumerable<EventQueueAnalysis> data, string widgetName, WidgetRequest obj, int totalZone)
        {
            var csvBuilder = new StringBuilder();
            var offsetTimeStamp = await GetOffset(obj.UserId);

            csvBuilder.Append(await GetTitleForCsv(
                widgetName,
                obj.StartDate.Add(offsetTimeStamp),
                obj.EndDate.Add(offsetTimeStamp),
                obj.UserId));
            // Total slots in 24 hours
            int slotsPerDay = (24 * 60) / obj.IntervalMinute;
            string headerData = "Type, Date";

            csvBuilder.Append(GetHourHeaderForCsv(headerData, obj.IntervalMinute, slotsPerDay));

            // Vehicle types
            var vehicleParkingTypes = new List<string> { "Available", "Occupied" };

            // Storage dictionary
            var groupedByVehicleType = new Dictionary<string, Dictionary<DateTime, Dictionary<int, int>>>();

            foreach (var type in vehicleParkingTypes)
            {
                groupedByVehicleType[type] = new Dictionary<DateTime, Dictionary<int, int>>();
            }

            // Bucket + hourly max calculation
            data = data
                .Select(x => new EventQueueAnalysis
                {
                    DateTime = GetBucketTime(x.DateTime.Add(offsetTimeStamp), obj.IntervalMinute),
                    QueueCount = x.QueueCount
                })
                .GroupBy(x => x.DateTime)
                .Select(x => new EventQueueAnalysis
                {
                    DateTime = x.Key,
                    QueueCount = x.Max(y => y.QueueCount)
                })
                .ToList();

            // Process records
            foreach (var record in data)
            {
                var dt = record.DateTime;
                var date = dt.Date;

                int totalMinutes = dt.Hour * 60 + dt.Minute;
                int slotIndex = totalMinutes / obj.IntervalMinute;

                int occupied = (int)record.QueueCount;
                int available = totalZone - occupied;

                if (available < 0)
                {
                    available = 0;
                }

                var values = new Dictionary<string, int>
                            {
                                { "Occupied", occupied },
                                { "Available", available }
                            };

                foreach (var type in vehicleParkingTypes)
                {
                    if (!groupedByVehicleType[type].ContainsKey(date))
                    {
                        groupedByVehicleType[type][date] = new Dictionary<int, int>();
                    }

                    groupedByVehicleType[type][date][slotIndex] = values[type];
                }
            }

            // If no data, fill default values
            if (data == null || !data.Any())
            {
                var startDateTime = obj.StartDate.Add(offsetTimeStamp);
                var endDateTime = obj.EndDate.Add(offsetTimeStamp);

                var startDate = startDateTime.Date;
                var endDate = endDateTime.Date;

                int endTotalMinutes = endDateTime.Hour * 60 + endDateTime.Minute;
                int lastSlotIndex = endTotalMinutes / obj.IntervalMinute;

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var availableSlots = new Dictionary<int, int>();
                    var occupiedSlots = new Dictionary<int, int>();

                    int maxSlot = slotsPerDay;

                    // If it's the last date, stop at EndTime
                    if (date == endDate)
                    {
                        maxSlot = lastSlotIndex + 1;
                    }

                    for (int slot = 0; slot < maxSlot; slot++)
                    {
                        availableSlots[slot] = totalZone;
                        occupiedSlots[slot] = 0;
                    }

                    groupedByVehicleType["Available"][date] = availableSlots;
                    groupedByVehicleType["Occupied"][date] = occupiedSlots;
                }
            }

            // Build CSV rows
            foreach (var vehicleType in vehicleParkingTypes)
            {
                csvBuilder.Append(
                    BindHourDataForCsv(
                        groupedByVehicleType[vehicleType],
                        obj.StartDate.Add(offsetTimeStamp),
                        obj.EndDate.Add(offsetTimeStamp),
                        slotsPerDay,
                        vehicleType,
                        false));
            }

            return csvBuilder;
        }

    }
}
