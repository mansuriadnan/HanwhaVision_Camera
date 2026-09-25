using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Infrastructure.Utilities;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.PeopleWidget;
using MongoDB.Driver;
using System.Diagnostics;
using System.Text.Json;

namespace HanwhaClient.Application.Services
{
    public class PeopleWidgetService : IPeopleWidgetService
    {
        private readonly IZoneCameraRepository _zoneCameraRepository;
        private readonly IPermissionService _permissionService;
        private readonly IDateConvert _dateConvert;
        private readonly IPeopleCountRepository _peopleCountRepository;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IUsersService _usersService;
        private readonly IZoneRepository _zoneRepository;
        private readonly IPeopleCountArchiveRepository _peopleCountArchiveRepository;
        private readonly IArchiveTimeService _archiveTimeService;
        private readonly IFloorService _floorService;

        public PeopleWidgetService(
            IZoneCameraRepository zoneCameraRepository,
            IDateConvert dateConvert,
            IUsersService usersService,
            IPeopleCountRepository peopleCountRepository,
            IDeviceMasterRepository deviceMasterRepository,
            IZoneRepository zoneRepository,
            IPermissionService permissionService,
            IPeopleCountArchiveRepository peopleCountArchiveRepository,
            IArchiveTimeService archiveTimeService,
            IFloorService floorService)
        {
            _zoneCameraRepository = zoneCameraRepository;
            _peopleCountRepository = peopleCountRepository;
            _dateConvert = dateConvert;
            _usersService = usersService;
            _deviceMasterRepository = deviceMasterRepository;
            _zoneRepository = zoneRepository;
            _permissionService = permissionService;
            _peopleCountArchiveRepository = peopleCountArchiveRepository;
            _archiveTimeService = archiveTimeService;
            _floorService = floorService;
        }

        public async Task<IEnumerable<GenderWisePeopleCounting>> GenderWisePeopleCounting(WidgetRequest widgetRequest)
        {
            //var zones = await _zoneRepository.GetZonesByMultipleFloorIdZoneIdAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
            var zones = await GetAllZoneByPermission(widgetRequest);

            GenderWisePeopleCounting malePeopleCounting = new GenderWisePeopleCounting() { Gender = "Male" };
            GenderWisePeopleCounting femalePeopleCounting = new GenderWisePeopleCounting() { Gender = "Female" };
            GenderWisePeopleCounting unknownPeopleCounting = new GenderWisePeopleCounting() { Gender = "Unknown" };

            if (zones != null && zones.Count() > 0)
            {
                var zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneIds(zones.Select(x => x.Id));
                var timeZone = await _usersService.GetTimeZone(widgetRequest.UserId);
                var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);

                var peopleCountResult = await _peopleCountRepository.GenderWisePeopleCountingAsync(widgetRequest, zoneCameraList, offsetTimeStamp);

                foreach (var item in peopleCountResult)
                {
                    foreach (var genderItem in item.peopleInCount.Lines)
                    {
                        if (genderItem.GenderInfo != null)
                        {
                            if (zoneCameraList.Where(x => x.PeopleLineIndex.Contains(genderItem.LineIndex)).Any())
                            {
                                var maleCount = genderItem.GenderInfo.Where(x => x.GenderType == "Male").Select(x => x.Count).FirstOrDefault();
                                var femaleCount = genderItem.GenderInfo.Where(x => x.GenderType == "Female").Select(x => x.Count).FirstOrDefault();
                                var UnknownCount = genderItem.GenderInfo.Where(x => x.GenderType == "Unknown").Select(x => x.Count).FirstOrDefault();

                                malePeopleCounting.Count += maleCount;
                                femalePeopleCounting.Count += femaleCount;
                                unknownPeopleCounting.Count += UnknownCount;

                                if (maleCount == 0) malePeopleCounting.MinDate = item.date;
                                if (femaleCount == 0) femalePeopleCounting.MinDate = item.date;
                                if (UnknownCount == 0) unknownPeopleCounting.MinDate = item.date;

                                if (maleCount > 0 && malePeopleCounting.MinCount >= maleCount)
                                {
                                    malePeopleCounting.MinCount = maleCount;
                                    malePeopleCounting.MinDate = item.date;
                                }
                                if (malePeopleCounting.MaxCount <= maleCount)
                                {
                                    malePeopleCounting.MaxCount = maleCount;
                                    malePeopleCounting.MaxDate = item.date;
                                }

                                if (femaleCount > 0 && femalePeopleCounting.MinCount >= femaleCount)
                                {
                                    femalePeopleCounting.MinCount = femaleCount;
                                    femalePeopleCounting.MinDate = item.date;
                                }
                                if (femalePeopleCounting.MaxCount <= femaleCount)
                                {
                                    femalePeopleCounting.MaxCount = femaleCount;
                                    femalePeopleCounting.MaxDate = item.date;
                                }

                                if (UnknownCount > 0 && unknownPeopleCounting.MinCount >= UnknownCount)
                                {
                                    unknownPeopleCounting.MinCount = UnknownCount;
                                    unknownPeopleCounting.MinDate = item.date;
                                }
                                if (unknownPeopleCounting.MaxCount <= UnknownCount)
                                {
                                    unknownPeopleCounting.MaxCount = UnknownCount;
                                    unknownPeopleCounting.MaxDate = item.date;
                                }
                            }
                        }
                    }
                }
            }
            malePeopleCounting.MinCount = malePeopleCounting.MinCount == int.MaxValue ? malePeopleCounting.MinCount = 0 : malePeopleCounting.MinCount;
            femalePeopleCounting.MinCount = femalePeopleCounting.MinCount == int.MaxValue ? femalePeopleCounting.MinCount = 0 : femalePeopleCounting.MinCount;
            unknownPeopleCounting.MinCount = unknownPeopleCounting.MinCount == int.MaxValue ? unknownPeopleCounting.MinCount = 0 : unknownPeopleCounting.MinCount;
            return [malePeopleCounting, femalePeopleCounting, unknownPeopleCounting];
        }

        public async Task<(IEnumerable<CameraCapacityUtilizationByZones>, UtilizationMostLeastDay)> PeopleCameraCapacityUtilizationByZoneAsync(WidgetRequest widgetRequest)
        {
            List<CameraCapacityUtilizationByZones> peopleCameraCapacityUtilizations = new List<CameraCapacityUtilizationByZones>();
            List<IEnumerable<CameraCapacityUtilizationByDevice>> cameraUtilizationLst = new List<IEnumerable<CameraCapacityUtilizationByDevice>>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            var timeZone = await _usersService.GetTimeZone(widgetRequest.UserId);
            foreach (var zone in zoneData)
            {
                var deepRequest = JsonSerializer.Deserialize<WidgetRequest>(JsonSerializer.Serialize(widgetRequest));
                deepRequest.ZoneIds = [zone.Id];
                var peopleData = await PeopleInOutCountAnalysisV2Async(deepRequest, true);

                var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
                var utilizationData = peopleData
                        .Select(kvp => new CountAnalysisData { DateTime = (kvp.DateTime.Value + offsetTimeStamp).Date, Count = kvp.InCount - kvp.OutCount })
                        .OrderBy(x => x.DateTime)
                        .ToList();

                var dayWiseUtilizationData = utilizationData.GroupBy(x => x.DateTime)
                    .Select(y => new
                    {
                        DateTime = y.Key,
                        Count = y.Max(z => z.Count)
                    }).ToList();

                var totalUtilizationData = dayWiseUtilizationData.Sum(x => x.Count);

                double percentage = 0;
                if (totalUtilizationData > 0 && zone.PeopleOccupancy > 0)
                {
                    percentage = (double)((totalUtilizationData * 100) / zone.PeopleOccupancy);
                }
                percentage = double.IsNaN(percentage) ? 0 : percentage;
                peopleCameraCapacityUtilizations.Add(new CameraCapacityUtilizationByZones
                {
                    ZoneName = zone.ZoneName,
                    MaxCapacity = zone.PeopleOccupancy != null ? (int)zone.PeopleOccupancy : 0,
                    Utilization = totalUtilizationData,
                    Percentage = percentage,
                    PeopleDefaultOccupancy = zone.PeopleDefaultOccupancy
                });

                //IEnumerable <ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
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
                //    List<CameraCapacityUtilizationByDevice> cameraCapacityLst = new List<CameraCapacityUtilizationByDevice>();
                //    var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
                //    foreach (var camera in zoneCameraList)
                //    {
                //        IEnumerable<CameraCapacityUtilizationByDevice> result = await _peopleCountRepository.GetPeopleCameraCapacityUtilizationByDeviceAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, offsetTimeStamp, camera.Channel, camera.PeopleLineIndex);
                //        if (result != null && result.Count() > 0)
                //        {
                //            cameraCapacityLst.Add(new CameraCapacityUtilizationByDevice { DeviceId = camera.DeviceId, UtilizationCount = result.Sum(x => x.UtilizationCount) });
                //            cameraUtilizationLst.Add(result);
                //        }
                //    }

                //    int utilizationCount = (int)cameraCapacityLst.Sum(x => x.UtilizationCount);
                //    int count = cameraCapacityLst.Count();
                //    int utilization = utilizationCount;
                //    double percentage = 0;

                //    if (count > 0 && zone.PeopleOccupancy > 0)
                //    {
                //        percentage = (double)((utilization * 100) / zone.PeopleOccupancy);
                //    }
                //    percentage = double.IsNaN(percentage) ? 0 : percentage;

                //    peopleCameraCapacityUtilizations.Add(new CameraCapacityUtilizationByZones
                //    {
                //        ZoneName = zone.ZoneName,
                //        MaxCapacity = zone.PeopleOccupancy != null ? (int)zone.PeopleOccupancy : 0,
                //        Utilization = utilization,
                //        Percentage = percentage
                //    });
                //}
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
            return (peopleCameraCapacityUtilizations, mostdayLeastday);
        }

        public async Task<CapacityUtilization> PeopleCapacityUtilizationAsync(WidgetRequest widgetRequest)
        {
            var peopleCameraCapacityUtilizationByZones = await PeopleCameraCapacityUtilizationByZoneAsync(widgetRequest);
            double percentage = (peopleCameraCapacityUtilizationByZones.Item1.Sum(x => x.Utilization) * 100) / peopleCameraCapacityUtilizationByZones.Item1.Sum(x => x.MaxCapacity);
            CapacityUtilization peopleCapacityUtilization = new CapacityUtilization();
            if (peopleCameraCapacityUtilizationByZones.Item1.Count() > 0)
            {
                peopleCapacityUtilization.Utilization = peopleCameraCapacityUtilizationByZones.Item1.Sum(x => x.Utilization);
                peopleCapacityUtilization.TotalCapacity = peopleCameraCapacityUtilizationByZones.Item1.Sum(x => x.MaxCapacity);
                peopleCapacityUtilization.Percentage = double.IsNaN(percentage) || double.IsInfinity(percentage) ? 0 : percentage;
                peopleCapacityUtilization.UtilizationMostLeastDay = peopleCameraCapacityUtilizationByZones.Item2;
                peopleCapacityUtilization.PeopleDefaultOccupancy = peopleCameraCapacityUtilizationByZones.Item1.Sum(x => x.PeopleDefaultOccupancy);
            }
            return peopleCapacityUtilization;
        }

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
                    //// Get cameras for zone
                    //var zoneCameraList = await GetZoneCamerasAsync(zone);
                    //if (zoneCameraList == null || !zoneCameraList.Any())
                    //{
                    //    return new CameraCapacityUtilizationAnalysisByZones
                    //    {
                    //        ZoneName = zone.ZoneName,
                    //        UtilizationData = new List<CountAnalysisData>()
                    //    };
                    //}

                    //// Process cameras in parallel with controlled concurrency
                    //var semaphore = new SemaphoreSlim(Environment.ProcessorCount); // Limit concurrent DB calls
                    //var cameraTasks = zoneCameraList.Select(async camera =>
                    //{
                    //    await semaphore.WaitAsync();
                    //    try
                    //    {
                    //        return await _peopleCountRepository.PeopleCameraCapacityUtilizationAnalysisByZones(
                    //            camera.DeviceId,
                    //            widgetRequest.StartDate,
                    //            widgetRequest.EndDate,
                    //            camera.Channel,
                    //            camera.PeopleLineIndex,
                    //            widgetRequest.IntervalMinute);
                    //    }
                    //    finally
                    //    {
                    //        semaphore.Release();
                    //    }
                    //});

                    //var cameraResults = await Task.WhenAll(cameraTasks);
                    //semaphore.Dispose();

                    //// Optimize aggregation using Dictionary for O(1) lookups instead of GroupBy
                    //var aggregatedData = new Dictionary<DateTime, int>();

                    //foreach (var result in cameraResults)
                    //{
                    //    if (result != null)
                    //    {
                    //        foreach (var item in result)
                    //        {
                    //            if (aggregatedData.TryGetValue(item.DateTime, out var existingCount))
                    //            {
                    //                aggregatedData[item.DateTime] = existingCount + item.Count;
                    //            }
                    //            else
                    //            {
                    //                aggregatedData[item.DateTime] = item.Count;
                    //            }
                    //        }
                    //    }
                    //}

                    var deepRequest = JsonSerializer.Deserialize<WidgetRequest>(JsonSerializer.Serialize(widgetRequest));
                    deepRequest.ZoneIds = [zone.Id];
                    var peopleData = await PeopleInOutCountAnalysisV2Async(deepRequest, true);

                    // Convert to final format and sort
                    var utilizationData = peopleData
                        .Select(kvp => new CountAnalysisData { DateTime = kvp.DateTime.Value, Count = kvp.InCount - kvp.OutCount })
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

        public async Task<NewVsTotalVisitorCountWidget> NewVsTotalVisitorCountAsync(WidgetRequest widgetRequest)
        {
            int totalNewInVisitorsCount = 0;

            var zoneData = await GetAllZoneByPermission(widgetRequest);
            var timeZone = await _usersService.GetTimeZone(widgetRequest.UserId);

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
                        var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
                        var deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
                        var filters = new List<FilterDefinition<PeopleCount>>();
                        filters.Add(Builders<PeopleCount>.Filter.In(x => x.DeviceId, deviceIds));
                        filters.Add(Builders<PeopleCount>.Filter.Gte(x => x.CreatedOn, widgetRequest.StartDate));
                        filters.Add(Builders<PeopleCount>.Filter.Lte(x => x.CreatedOn, widgetRequest.EndDate));

                        var filter = Builders<PeopleCount>.Filter.And(filters);

                        var latestPerDayPeopleCount = await _peopleCountRepository.GetLatestPeopleCountDetails(filter, offsetTimeStamp);

                        foreach (var data in latestPerDayPeopleCount)
                        {
                            var deviceFilter = zoneCameraList.FirstOrDefault(zc => zc.DeviceId == data.DeviceId && zc.Channel == data.ChannelNo);

                            var matchingCounts = deviceFilter != null ?
                                data.Lines.Where(line => deviceFilter.PeopleLineIndex.Contains(line.LineIndex)) :
                                Enumerable.Empty<Line>();

                            totalNewInVisitorsCount += matchingCounts.Sum(l => l.InCount);
                        }
                    }
                }
            }

            var totalPeopleDefaultOccupancy = zoneData
                .Where(x => (x.IsDeleted ?? true) == false)
                .Sum(x => x.PeopleDefaultOccupancy);

            var result = new NewVsTotalVisitorCountWidget
            {
                NewVisitorsCount = totalNewInVisitorsCount,
                TotalVisitorsCount = totalNewInVisitorsCount + (long)totalPeopleDefaultOccupancy
            };
            return result;
        }

        public async Task<IEnumerable<PeopleCountByZones>> PeopleCountByZones(WidgetRequest widgetRequest)
        {
            List<PeopleCountByZones> peopleCountByZones = new List<PeopleCountByZones>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            var zoneTasks = zoneData.Select(async zone =>
            {
                var deepRequest = JsonSerializer.Deserialize<WidgetRequest>(JsonSerializer.Serialize(widgetRequest));
                deepRequest.ZoneIds = [zone.Id];
                var peopleData = await PeopleInOutCountAnalysisV2Async(deepRequest, false);

                return new PeopleCountByZones
                {
                    ZoneName = zone.ZoneName,
                    PeopleInCount = peopleData.Sum(x => x.InCount),
                    PeopleOutCount = peopleData.Sum(x => x.OutCount)
                };
            });

            var results = await Task.WhenAll(zoneTasks);
            peopleCountByZones.AddRange(results);

            return peopleCountByZones;
        }

        //public async Task<IEnumerable<GenderWisePeopleAnalysisCount>> GenderWisePeopleCountAnalysisData(WidgetRequest widgetRequest)
        //{
        //    List<GenderWisePeopleAnalysisCount> genderCountAnalysis = new List<GenderWisePeopleAnalysisCount>();
        //    var zoneData = await GetAllZoneByPermission(widgetRequest);
        //    List<IEnumerable<GenderWisePeopleAnalysisCount>> genderCountAnalysisByDevice = new List<IEnumerable<GenderWisePeopleAnalysisCount>>();

        //    foreach (var zone in zoneData)
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
        //            foreach (var camera in zoneCameraList)
        //            {
        //                bool isArchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
        //                IEnumerable<GenderWisePeopleAnalysisCount> result = await _peopleCountRepository.GenderWisePeopleCountAnalysisData(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, isArchived, camera.PeopleLineIndex, widgetRequest.IntervalMinute);
        //                if (result != null)
        //                {
        //                    genderCountAnalysisByDevice.Add(result);
        //                }
        //            }
        //        }
        //    }

        //    genderCountAnalysis = genderCountAnalysisByDevice.SelectMany(x => x)
        //                        .GroupBy(x => x.DateTime)
        //                        .Select(g => new GenderWisePeopleAnalysisCount
        //                        {
        //                            DateTime = g.Key,
        //                            MaleCount = g.Sum(x => x.MaleCount),
        //                            FemaleCount = g.Sum(x => x.FemaleCount),
        //                            UndefinedCount = g.Sum(x => x.UndefinedCount)
        //                        }).OrderBy(x => x.DateTime).ToList();

        //    return genderCountAnalysis;
        //}

        public async Task<IEnumerable<AgeWisePeopleAnalysisCount>> AgeWisePeopleCountAnalysisData(WidgetRequest widgetRequest)
        {
            List<AgeWisePeopleAnalysisCount> ageCountAnalysis = new List<AgeWisePeopleAnalysisCount>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            List<IEnumerable<AgeWisePeopleAnalysisCount>> ageCountAnalysisByDevice = new List<IEnumerable<AgeWisePeopleAnalysisCount>>();
            IEnumerable<ZoneCamera> zoneCameraList;

            if (widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
            }
            else
            {
                zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneIds(zoneData.Select(x => x.Id));
            }

            if (!string.IsNullOrEmpty(widgetRequest.DeviceId) && widgetRequest.DeviceId != "6812332a6d517f8bfff611bb")
            {
                zoneCameraList = zoneCameraList.Where(x => x.DeviceId == widgetRequest.DeviceId).ToList();
            }
            bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            var data = await _peopleCountRepository.PeopleInOutCountAnalysisV2Async(zoneCameraList, widgetRequest.StartDate.AddHours(-1), widgetRequest.EndDate, isarchived);

            data = (from d in data
                    join f in zoneCameraList.AsEnumerable()
                    on new { DeviceId = d.DeviceId, ChannelNo = d.ChannelNo } equals new { DeviceId = f.DeviceId ?? "", ChannelNo = f.Channel }
                    select d).ToList();

            data = data
                .Join(
                    zoneCameraList,
                    pc => pc.DeviceId,
                    zc => zc.DeviceId,
                    (pc, zc) => new { PeopleCount = pc, ZoneCamera = zc }
                )
                .Select(x => new PeopleCount
                {
                    CreatedOn = GetBucketTime(x.PeopleCount.CreatedOn.Value, 10),
                    DeviceId = x.PeopleCount.DeviceId,
                    CameraIP = x.PeopleCount.CameraIP,
                    ChannelNo = x.PeopleCount.ChannelNo,
                    Lines = x.PeopleCount.Lines
                                .Where(l => x.ZoneCamera.PeopleLineIndex.Contains(l.LineIndex))
                                .ToList()
                })
                .Where(pc => pc.Lines.Any())
                .ToList();

            var deviceWiseData = data
                .GroupBy(pc => new
                {
                    pc.DeviceId,
                    CreatedOn = pc.CreatedOn
                })
                .Select(d => new AgeWisePeopleAnalysisCount
                {
                    DeviceId = d.Key.DeviceId,
                    DateTime = d.Key.CreatedOn,
                    YoungCount = d.Max(x => x.Lines.Sum(l => (l.AgeInfo != null ? l.AgeInfo.Where(a => a.AgeType == "Young").Sum(a => a.Count) : 0))),
                    AdultCount = d.Max(x => x.Lines.Sum(l => (l.AgeInfo != null ? l.AgeInfo.Where(a => a.AgeType == "Adult").Sum(a => a.Count): 0))),
                    SeniorCount = d.Max(x => x.Lines.Sum(l => (l.AgeInfo != null ? l.AgeInfo.Where(a => a.AgeType == "Senior").Sum(a => a.Count):0))),
                    UnknownCount = d.Max(x => x.Lines.Sum(l => (l.AgeInfo != null ? l.AgeInfo.Where(a => a.AgeType == "Unknown").Sum(a => a.Count) : 0)))
                })
                .OrderBy(x => x.DeviceId)
                .ThenBy(x => x.DateTime)
                .ToList();

            var deviceWiseDiff = deviceWiseData
                .GroupBy(x => x.DeviceId)
                .SelectMany(g =>
                    g.Select((x, i) => new AgeWisePeopleAnalysisCount
                    {
                        DeviceId = x.DeviceId,
                        DateTime = x.DateTime,
                        YoungCount = i == 0 || x.YoungCount < g.ElementAt(i - 1).YoungCount ? x.YoungCount :
                                  x.YoungCount - g.ElementAt(i - 1).YoungCount,
                        AdultCount = i == 0 || x.AdultCount < g.ElementAt(i - 1).AdultCount ? x.AdultCount :
                                   x.AdultCount - g.ElementAt(i - 1).AdultCount,
                        SeniorCount = i == 0 || x.SeniorCount < g.ElementAt(i - 1).SeniorCount ? x.SeniorCount :
                                   x.SeniorCount - g.ElementAt(i - 1).SeniorCount,
                        UnknownCount = i == 0 || x.UnknownCount < g.ElementAt(i - 1).UnknownCount ? x.UnknownCount :
                                   x.UnknownCount - g.ElementAt(i - 1).UnknownCount,
                    })
                ).Where(x => x.DateTime >= widgetRequest.StartDate).ToList();

            var result = deviceWiseDiff
                        .GroupBy(x => x.DateTime)
                        .Select(g => new AgeWisePeopleAnalysisCount
                        {
                            DateTime = g.Key,
                            YoungCount = g.Sum(x => x.YoungCount),
                            AdultCount = g.Sum(x => x.AdultCount),
                            SeniorCount = g.Sum(x => x.SeniorCount),
                            UnknownCount = g.Sum(x => x.UnknownCount)
                        })
                        .OrderBy(x => x.DateTime)
                        .AsEnumerable();

            return result;
        }

        public async Task<IEnumerable<GenderWisePeopleAnalysisCount>> GenderWisePeopleCountAnalysisData(WidgetRequest widgetRequest)
        {
            List<GenderWisePeopleAnalysisCount> genderCountAnalysis = new List<GenderWisePeopleAnalysisCount>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            
            IEnumerable<ZoneCamera> zoneCameraList;

            if (widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
            }
            else
            {
                zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneIds(zoneData.Select(x => x.Id));
            }

            if (!string.IsNullOrEmpty(widgetRequest.DeviceId) && widgetRequest.DeviceId != "6812332a6d517f8bfff611bb")
            {
                zoneCameraList = zoneCameraList.Where(x => x.DeviceId == widgetRequest.DeviceId).ToList();
            }

            bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            var data = await _peopleCountRepository.PeopleInOutCountAnalysisV2Async(zoneCameraList, widgetRequest.StartDate.AddHours(-1), widgetRequest.EndDate, isarchived);

            data = (from d in data
                    join f in zoneCameraList.AsEnumerable()
                    on new { DeviceId = d.DeviceId, ChannelNo = d.ChannelNo } equals new { DeviceId = f.DeviceId ?? "", ChannelNo = f.Channel }
                    select d).ToList();

            data = data
                .Join(
                    zoneCameraList,
                    pc => pc.DeviceId,
                    zc => zc.DeviceId,
                    (pc, zc) => new { PeopleCount = pc, ZoneCamera = zc }
                )
                .Select(x => new PeopleCount
                {
                    CreatedOn = GetBucketTime(x.PeopleCount.CreatedOn.Value, 10),
                    DeviceId = x.PeopleCount.DeviceId,
                    CameraIP = x.PeopleCount.CameraIP,
                    ChannelNo = x.PeopleCount.ChannelNo,
                    Lines = x.PeopleCount.Lines
                                .Where(l => x.ZoneCamera.PeopleLineIndex.Contains(l.LineIndex))
                                .ToList()
                })
                .Where(pc => pc.Lines.Any())
                .ToList();

            var deviceWiseData = data
                .GroupBy(pc => new
                {
                    pc.DeviceId,
                    CreatedOn = pc.CreatedOn
                })
                .Select(d => new GenderWisePeopleAnalysisCount
                {
                    DateTime = (DateTime)d.Key.CreatedOn,
                    DeviceId = d.Key.DeviceId,
                    MaleCount = d.Max(x => x.Lines.Sum(l => (l.GenderInfo != null ? l.GenderInfo.Where(a => a.GenderType == "Male").Sum(a => a.Count) : 0))),
                    FemaleCount = d.Max(x => x.Lines.Sum(l => (l.GenderInfo != null ? l.GenderInfo.Where(a => a.GenderType == "Female").Sum(a => a.Count) : 0))),
                    UndefinedCount = d.Max(x => x.Lines.Sum(l => (l.GenderInfo != null ? l.GenderInfo.Where(a => a.GenderType == "Unknown").Sum(a => a.Count) : 0))),
                })
                .OrderBy(x => x.DeviceId)
                .ThenBy(x => x.DateTime)
                .ToList();

            var deviceWiseDiff = deviceWiseData
                .GroupBy(x => x.DeviceId)
                .SelectMany(g =>
                    g.Select((x, i) => new GenderWisePeopleAnalysisCount
                    {
                        DeviceId = x.DeviceId,
                        DateTime = x.DateTime,
                        MaleCount = i == 0 || x.MaleCount < g.ElementAt(i - 1).MaleCount ? x.MaleCount :
                                  x.MaleCount - g.ElementAt(i - 1).MaleCount,
                        FemaleCount = i == 0 || x.FemaleCount < g.ElementAt(i - 1).FemaleCount ? x.FemaleCount :
                                   x.FemaleCount - g.ElementAt(i - 1).FemaleCount,
                        UndefinedCount = i == 0 || x.UndefinedCount < g.ElementAt(i - 1).UndefinedCount ? x.UndefinedCount :
                                   x.UndefinedCount - g.ElementAt(i - 1).UndefinedCount,
                    })
                ).Where(x => x.DateTime >= widgetRequest.StartDate).ToList();

            var result = deviceWiseDiff
            .GroupBy(x => x.DateTime)
            .Select(g => new GenderWisePeopleAnalysisCount
            {
                DateTime = g.Key,
                MaleCount = g.Sum(x => x.MaleCount),
                FemaleCount = g.Sum(x => x.FemaleCount),
                UndefinedCount = g.Sum(x => x.UndefinedCount),
            })
            .OrderBy(x => x.DateTime)
            .AsEnumerable();

            return result;

        }


        public async Task<PeopleVehicleInOutTotal> PeopleIOnOutTotalV2Async(WidgetRequest widgetRequest)
        {

            var zoneData = await GetAllZoneByPermission(widgetRequest);

            IEnumerable<ZoneCamera> zoneCameraList;

            if (widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
            }
            else
            {
                zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneIds(zoneData.Select(x => x.Id));
            }

            if (!string.IsNullOrEmpty(widgetRequest.DeviceId) && widgetRequest.DeviceId != "6812332a6d517f8bfff611bb")
            {
                zoneCameraList = zoneCameraList.Where(x => x.DeviceId == widgetRequest.DeviceId).ToList();
            }
            var timeZone = await _usersService.GetTimeZone(widgetRequest.UserId);
            bool isarchive = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            var data = await _peopleCountRepository.PeopleInOutCountAnalysisV2Async(zoneCameraList, widgetRequest.StartDate, widgetRequest.EndDate, isarchive);


            data = (from d in data
                    join f in zoneCameraList.AsEnumerable()
                    on new { DeviceId = d.DeviceId, ChannelNo = d.ChannelNo } equals new { DeviceId = f.DeviceId ?? "", ChannelNo = f.Channel }
                    select d).ToList();

            data = data
                .Join(
                    zoneCameraList,
                    pc => pc.DeviceId,
                    zc => zc.DeviceId,
                    (pc, zc) => new { PeopleCount = pc, ZoneCamera = zc }
                )
                .Select(x => new PeopleCount
                {
                    CreatedOn = GetBucketTime(x.PeopleCount.CreatedOn.Value, widgetRequest.IntervalMinute),
                    DeviceId = x.PeopleCount.DeviceId,
                    CameraIP = x.PeopleCount.CameraIP,
                    ChannelNo = x.PeopleCount.ChannelNo,
                    Lines = x.PeopleCount.Lines
                                .Where(l => x.ZoneCamera.PeopleLineIndex.Contains(l.LineIndex))
                                .ToList()
                })
                .Where(pc => pc.Lines.Any())
                .ToList();


            var deviceWiseData = data
                .GroupBy(pc => new
                {
                    pc.DeviceId,
                    CreatedOn = pc.CreatedOn
                }).Select(d => new PeopleVehicleInOutAvgChart
                {
                    DateTime = d.Key.CreatedOn,
                    InCount = d.Max(x => x.Lines.Sum(l => l.InCount)),
                    OutCount = d.Max(x => x.Lines.Sum(l => l.OutCount)),
                });


            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);

            var count = data.GroupBy(x => new
            {
                x.DeviceId,
                CreatedOn = (x.CreatedOn.Value + offsetTimeStamp).Date
            })
                 .Select(d => new
                 {
                     MaxInCount = d.Max(x => x.Lines.Sum(l => l.InCount)),
                     MaxOutCount = d.Max(x => x.Lines.Sum(l => l.OutCount)),
                 });
            var maxOutCount = count.Sum(x => x.MaxOutCount);
            var maxInCount = count.Sum(x => x.MaxInCount);

            var overallSummary = new PeopleVehicleInOutTotal
            {
                TotalInCount = maxInCount,
                TotalOutCount = maxOutCount,
            };
            return overallSummary;
        }

        public async Task<IEnumerable<PeopleVehicleInOutAvgChart>> PeopleInOutCountAnalysisV2Async(WidgetRequest widgetRequest, bool isCumulative = false)
        {
            var zoneData = await GetAllZoneByPermission(widgetRequest);

            IEnumerable<ZoneCamera> zoneCameraList;

            if (widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
            }
            else
            {
                zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneIds(zoneData.Select(x => x.Id));
            }

            if (!string.IsNullOrEmpty(widgetRequest.DeviceId) && widgetRequest.DeviceId != "6812332a6d517f8bfff611bb")
            {
                zoneCameraList = zoneCameraList.Where(x => x.DeviceId == widgetRequest.DeviceId).ToList();
            }
            bool isarchived = _archiveTimeService.CheckCollectionArchiceTime(widgetRequest.StartDate);
            var data = await _peopleCountRepository.PeopleInOutCountAnalysisV2Async(zoneCameraList, widgetRequest.StartDate.AddHours(-1), widgetRequest.EndDate, isarchived);

            data = (from d in data
                    join f in zoneCameraList.AsEnumerable()
                    on new { DeviceId = d.DeviceId, ChannelNo = d.ChannelNo } equals new { DeviceId = f.DeviceId ?? "", ChannelNo = f.Channel }
                    select d).ToList();

            data = data
                .Join(
                    zoneCameraList,
                    pc => pc.DeviceId,
                    zc => zc.DeviceId,
                    (pc, zc) => new { PeopleCount = pc, ZoneCamera = zc }
                )
                .Select(x => new PeopleCount
                {
                    CreatedOn = GetBucketTime(x.PeopleCount.CreatedOn.Value, 10),
                    DeviceId = x.PeopleCount.DeviceId,
                    CameraIP = x.PeopleCount.CameraIP,
                    ChannelNo = x.PeopleCount.ChannelNo,
                    Lines = x.PeopleCount.Lines
                                .Where(l => x.ZoneCamera.PeopleLineIndex.Contains(l.LineIndex))
                                .ToList()
                })
                .Where(pc => pc.Lines.Any())
                .ToList();

            //var deviceWiseData = data
            //    .GroupBy(pc => new
            //    {
            //        pc.DeviceId,
            //        CreatedOn = pc.CreatedOn
            //    }).Select(d => new PeopleVehicleInOutAvgChart
            //    {
            //        DateTime = d.Key.CreatedOn,
            //        InCount = d.Max(x => x.Lines.Sum(l => l.InCount)),
            //        OutCount = d.Max(x => x.Lines.Sum(l => l.OutCount)),
            //    });

            var deviceWiseData = data
                .GroupBy(pc => new
                {
                    pc.DeviceId,
                    pc.ChannelNo,
                    CreatedOn = pc.CreatedOn
                })
                .Select(d => new PeopleVehicleInOutAvgChartDevicewise
                {
                    DeviceId = d.Key.DeviceId,
                    DateTime = d.Key.CreatedOn,
                    ChannelNo = d.Key.ChannelNo,
                    InCount = d.Max(x => x.Lines.Sum(l => l.InCount)),
                    OutCount = d.Max(x => x.Lines.Sum(l => l.OutCount))
                })
                .OrderBy(x => x.DeviceId)
                .ThenBy(x => x.DateTime)
                .ToList();

            if (isCumulative)
            {
                var cumulativeResult = deviceWiseData
                    .GroupBy(x => x.DateTime)
                    .Select(g => new PeopleVehicleInOutAvgChart
                    {
                        DateTime = g.Key,
                        InCount = g.Sum(p => p.InCount),
                        OutCount = g.Sum(p => p.OutCount),
                    }).OrderBy(x => x.DateTime)
                    .AsEnumerable();
                return cumulativeResult.Where(x => x.DateTime >= widgetRequest.StartDate);
            }

            var deviceWiseDiff = deviceWiseData
            .GroupBy(x => new { x.DeviceId, x.ChannelNo })
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
            ).Where(x => x.DateTime >= widgetRequest.StartDate).ToList();

            //var dsdfsdf = deviceWiseData.GroupBy(x => x.DeviceId).Select(y => new { y.Key, count = y.Count() });

            //var result = deviceWiseData.GroupBy(x => x.DateTime)
            //    .Select(g => new PeopleVehicleInOutAvgChart
            //    {
            //        DateTime = g.Key,
            //        InCount = g.Sum(p => p.InCount),
            //        OutCount = g.Sum(p => p.OutCount),
            //    }).OrderBy(x => x.DateTime)
            //    .ToList();



            var result = deviceWiseDiff
                .GroupBy(x => x.DateTime)
                .Select(g => new PeopleVehicleInOutAvgChart
                {
                    DateTime = g.Key,
                    InCount = g.Sum(x => x.InCount),
                    OutCount = g.Sum(x => x.OutCount)
                })
                .OrderBy(x => x.DateTime)
                .AsEnumerable();

            return result;

            //if (isCumulative)
            //    return result.Where(x => x.DateTime >= widgetRequest.StartDate);

            //var diffData = result
            // .Select((x, i) => new PeopleVehicleInOutAvgChart
            // {
            //     DateTime = x.DateTime,
            //     InCount = i == 0 ? x.InCount : (x.InCount >= result[i - 1].InCount) ? x.InCount - result[i - 1].InCount : x.InCount,
            //     OutCount = i == 0 ? x.OutCount : (x.OutCount >= result[i - 1].OutCount) ? x.OutCount - result[i - 1].OutCount : x.OutCount,
            // })
            // .ToList().Where(x => x.InCount >= 0 && x.DateTime >= widgetRequest.StartDate);

            //return diffData;
        }

        public async Task<List<ChartAvgInOut>> NewVsTotalVisitorChartAsync(WidgetRequest widgetRequest)
        {
            //List<PeopleVehicleInOutAvgChart> peopleInOutCountAnalysis = new List<PeopleVehicleInOutAvgChart>();
            var zoneData = await GetAllZoneByPermission(widgetRequest);
            //List<IEnumerable<PeopleVehicleInOutAvgChart>> peopleInOutCountAnalysisByDevice = new List<IEnumerable<PeopleVehicleInOutAvgChart>>();

            //foreach (var zone in zoneData)
            //{
            //    IEnumerable<ZoneCamera> zoneCameraList = Enumerable.Empty<ZoneCamera>();
            //    if (zone.Id != null && zone.Id == "00")
            //    {
            //        zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
            //    }
            //    else
            //    {
            //        zoneCameraList = await _zoneCameraRepository.GetCamerasByZoneId(zone.Id);
            //    }

            //    if (!string.IsNullOrEmpty(widgetRequest.DeviceId) && widgetRequest.DeviceId != "6812332a6d517f8bfff611bb")
            //    {
            //        zoneCameraList = zoneCameraList.Where(x => x.DeviceId == widgetRequest.DeviceId);
            //    }
            //    if (zoneCameraList != null)
            //    {
            //        foreach (var camera in zoneCameraList)
            //        {
            //            IEnumerable<PeopleVehicleInOutAvgChart> result = await _peopleCountRepository.PeopleInOutCountAnalysisAsync(camera.DeviceId, widgetRequest.StartDate, widgetRequest.EndDate, camera.Channel, camera.PeopleLineIndex, widgetRequest.IntervalMinute);
            //            if (result != null)
            //            {
            //                peopleInOutCountAnalysisByDevice.Add(result);
            //            }
            //        }
            //    }
            //}

            var data = await PeopleInOutCountAnalysisV2Async(widgetRequest, false);

            var totalPeopleDefaultOccupancy = zoneData
           .Where(x => (x.IsDeleted ?? true) == false)
           .Sum(x => x.PeopleDefaultOccupancy);


            data = data.Select(x => x)
                                 .Select(g => new PeopleVehicleInOutAvgChart
                                 {
                                     DateTime = g.DateTime,
                                     InCount = g.InCount,
                                     OutCount = g.InCount + (int)totalPeopleDefaultOccupancy,
                                     DateTimeCsv = g.DateTime.ToString()
                                 }).OrderBy(x => x.DateTime).ToList();


            var finalResult = data.Select(x => new ChartAvgInOut
            {
                DateTime = x.DateTime,
                NewVisitor = x.InCount,
                TotalVisitor = (int)totalPeopleDefaultOccupancy,
            }).ToList();



            return finalResult;
        }

        private async Task<IEnumerable<ZoneMaster>> GetAllZoneByPermission(WidgetRequest widgetRequest)
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

        DateTime GetBucketTime(DateTime dt, int bucketSize = 10)
        {
            int bucketMinute = (dt.Minute / bucketSize) * bucketSize;
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, bucketMinute, 0);
        }

        public async Task<int> ProcessPeopleRetentionData(int retentionPeriod)
        {

        var data = await RetentionHelper.ProcessRetentionData<PeopleCount, PeopleCountArchive>(
                         _peopleCountRepository,                // IRepositoryBase<PeopleCount>
                         _peopleCountArchiveRepository,         // IRepositoryBase<PeopleCountArchive>
                         _peopleCountRepository,                // IRetentionRepository<PeopleCount>
                         x => new PeopleCountArchive
                        {
                            Id = x.Id,
                            DeviceId = x.DeviceId,
                            CameraIP = x.CameraIP,
                            ChannelNo = x.ChannelNo,
                            Lines = x.Lines,
                            CreatedOn = x.CreatedOn,
                            CreatedBy = x.CreatedBy,
                            UpdatedOn = x.UpdatedOn,
                            UpdatedBy = x.UpdatedBy
                        },
                        retentionPeriod: retentionPeriod);

            return data;
        }
    }
}
