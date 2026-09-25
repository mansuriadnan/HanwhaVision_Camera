using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;

namespace HanwhaClient.Application.Services
{
    public class SSMDashboardService : ISSMDashboardService
    {
        private readonly ISSMServerManagementRepository _sSMServerManagementRepository;
        private readonly ISsmServerRepository _ssmServerRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly ISsmServerUtilizationRepository _ssmServerUtilizationRepository;
        private readonly ISsmOfflineServerRepository _ssmOfflineServerRepository;
        private readonly ISsmOfflinedeviceRepository _ssmOfflinedeviceRepository;
        private readonly ISsmDeviceStoppedRecordingRepository _ssmDeviceStoppedRecordingRepository;
        private readonly IUsersService _usersService;
        private readonly IDateConvert _dateConvert;
        private readonly ISSMDeviceDetailsRepository _sSMDeviceDetailsRepository;

        public SSMDashboardService(ISSMServerManagementRepository sSMServerManagementRepository,
            ISsmServerRepository ssmServerRepository,
            ISiteRepository siteRepository,
            ISsmServerUtilizationRepository ssmServerUtilizationRepository,
            ISsmOfflineServerRepository ssmOfflineServerRepository,
            ISsmOfflinedeviceRepository ssmOfflinedeviceRepository,
            ISsmDeviceStoppedRecordingRepository ssmDeviceStoppedRecordingRepository,
            IUsersService usersService,
            IDateConvert dateConvert,
            ISSMDeviceDetailsRepository sSMDeviceDetailsRepository)
        {
            _sSMServerManagementRepository = sSMServerManagementRepository;
            _ssmServerRepository = ssmServerRepository;
            _siteRepository = siteRepository;
            _ssmServerUtilizationRepository = ssmServerUtilizationRepository;
            _ssmOfflineServerRepository = ssmOfflineServerRepository;
            _ssmOfflinedeviceRepository = ssmOfflinedeviceRepository;
            _ssmDeviceStoppedRecordingRepository = ssmDeviceStoppedRecordingRepository;
            _usersService = usersService;
            _dateConvert = dateConvert;
            _sSMDeviceDetailsRepository = sSMDeviceDetailsRepository;
        }
        public async Task<List<SsmServerHierarchyResponse>> GetSsmServerHierarchyAsync(List<string> parentSiteIds, DateTime date, string userId)
        {
            var timeZone = await _usersService.GetTimeZone(userId);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);

            // Step 1: Get SiteMappings
            var siteMappings = await _sSMServerManagementRepository.GetByParentSiteIdsAsync(parentSiteIds);

            if (!siteMappings.Any())
                return new List<SsmServerHierarchyResponse>();

            // Get site names from SiteMaster
            var parentSites = await _siteRepository.GetManyAsync(parentSiteIds);

            // Convert to dictionary for fast lookup
            var siteDict = new Dictionary<string, string>();

            foreach (var site in parentSites)
            {
                var parentId = site.Id.ToString();

                if (site.IsDeleted == false)
                {
                    siteDict[parentId] = site.SiteName;
                }

                if (site.ChildSites != null)
                {
                    foreach (var child in site.ChildSites)
                    {
                        if (child.IsDeleted == false)
                        {
                            siteDict[child.Id.ToString()] = child.SiteName;
                        }
                    }
                }
            }

            // Get all ssmSiteIds
            var mappingIds = siteMappings.Select(x => x.Id).ToList();

            // Server Filter ssmServer table
            var servers = await _ssmServerRepository.GetBySsmSiteIdsAsync(mappingIds);

            // NEW: Fetch utilization data
            var serverIds = servers.Select(x => x.Id.ToString()).ToList();
            var dateRange = await GetDateRangeFromUtcByUserId(date, userId);
            var utilizations = await _ssmServerUtilizationRepository.GetByServerIdsAsync(serverIds, dateRange.startOfDayUTC, dateRange.endOfDayUTC);

            var utilizationDict = utilizations.GroupBy(x => x.ServerId.ToString()).ToDictionary(
                                    g => g.Key,
                                    g => g.OrderByDescending(x => x.CreatedOn).First()
                                );

            //Combine in memory
            var result = siteMappings
                .GroupBy(x => x.ParentSiteId)
                .Select(parentGroup =>
                {
                    var parentResponse = new SsmServerHierarchyResponse
                    {
                        ParentSiteId = parentGroup.Key,
                        ParentSiteName = siteDict.ContainsKey(parentGroup.Key)
                        ? siteDict[parentGroup.Key]
                        : null,

                        ParentServers = new List<SsmDashboardServerDto>(),
                        SubSites = new List<SubSiteServerDto>()
                    };

                    foreach (var mapping in parentGroup)
                    {
                        var relatedServers = servers
                            .Where(s => s.SsmSiteId.ToString() == mapping.Id)
                            .Select(s =>
                            {
                                utilizationDict.TryGetValue(s.Id.ToString(), out var u);

                                return new SsmDashboardServerDto
                                {
                                    Id = s.Id.ToString(),
                                    Name = s.Name,
                                    IpAddress = s.IpAddress,
                                    Port = s.Port,
                                    Status = s.ServerStatus,
                                    UpdatedOn = s.UpdatedOn,

                                    // FROM UTILIZATION COLLECTION
                                    TotalProcessorUsage = u?.TotalProcessorUsage ?? 0,
                                    TotalMemoryUsage = u?.TotalMemoryUsage ?? 0,
                                    CpuSystemUsage = u?.CpuSystemUsage ?? 0,
                                    MemorySystemUsage = u?.MemorySystemUsage ?? 0,
                                    CpuMediaUsage = u?.CpuMediaUsage ?? 0,
                                    MemoryMediaUsage = u?.MemoryMediaUsage ?? 0,

                                    Disks = u?.Disks ?? [],
                                    //CDiskTotalSize = u.CDiskTotalSize,
                                    //DDiskFreeSize = u.DDiskFreeSize,
                                    //DDiskTotalSize = u.DDiskTotalSize,
                                    //EDiskFreeSize = u.EDiskFreeSize,
                                    //EDiskTotalSize = u.EDiskTotalSize,

                                    DiskTotalSize = u?.DiskTotalSize ?? 0,
                                    DiskFreeSize = u?.DiskFreeSize ?? 0,
                                    DiskFreePercentage = u?.DiskFreePercentage ?? 0,

                                    TotalCameraCount = u?.TotalCameraCount ?? 0,
                                    FailureCameraCount = u?.FailureCameraCount ?? 0
                                };
                            }).ToList();

                        if (mapping.ChildSiteId == null)
                        {
                            // Parent server
                            if (parentResponse.ParentServers == null)
                                parentResponse.ParentServers = new List<SsmDashboardServerDto>();

                            parentResponse.ParentServers.AddRange(relatedServers);
                        }
                        else
                        {
                            // Subsite
                            var existingSubSite = parentResponse.SubSites
                                .FirstOrDefault(x => x.ChildSiteId == mapping.ChildSiteId);

                            if (existingSubSite != null)
                            {
                                existingSubSite.Servers.AddRange(relatedServers);
                            }
                            else
                            {
                                parentResponse.SubSites.Add(new SubSiteServerDto
                                {
                                    ChildSiteId = mapping.ChildSiteId,
                                    ChildSiteName = siteDict.ContainsKey(mapping.ChildSiteId)
                                                        ? siteDict[mapping.ChildSiteId]
                                                        : null,

                                    Servers = relatedServers
                                });
                            }
                        }
                    }

                    return parentResponse;
                }).ToList();

            return result;
        }
        public async Task<List<SsmServerAvailabilityResponse>> GetServerAvailabilityAsync(SsmServerAvailabilityRequest request, string userId)
        {
            var dateRange = await GetDateRangeFromUtcByUserId(request.SearchDate, userId);
            var history = await _ssmOfflineServerRepository.GetOfflineServerHistoryByDateAsync(request.ServerId, dateRange.startOfDayUTC, dateRange.endOfDayUTC);
            return history;
        }

        private async Task<(DateTime startOfDayUTC, DateTime endOfDayUTC)> GetDateRangeFromUtcByUserId(DateTime date, string userId)
        {
            var timeZone = await _usersService.GetTimeZone(userId);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
            
            // Convert UTC → User Local Time
            var localDate = date + offsetTimeStamp;

            // Get full day in user's timezone
            var startOfDayLocal = localDate.Date;
            var endOfDayLocal = startOfDayLocal.AddDays(1).AddTicks(-1);

            // Convert back to UTC
            var startOfDayUTC = startOfDayLocal - offsetTimeStamp;
            var endOfDayUTC = endOfDayLocal - offsetTimeStamp;
            return (startOfDayUTC,endOfDayUTC);
        }

        public async Task<List<SsmDeviceAvailabilityResponse>> GetSsmDeviceAvailabilityAsync(SsmDeviceAvailabilityRequest request, string userId)
        {
            var dateRange = await GetDateRangeFromUtcByUserId(request.SearchDate, userId);
            var history = await _ssmOfflinedeviceRepository.GetSsmOfflineDeviceHistoryByDateAsync(request.DeviceId, dateRange.startOfDayUTC, dateRange.endOfDayUTC);
            return history;
        }

        public async Task<SsmServerCpuUtilizationResponse> GetSsmServerCpuUtilizationAsync(SsmServerCpuUtilizationRequest request, string userId)
        {
            var dateRange = await GetDateRangeFromUtcByUserId(request.Date, userId);
            var utilizations = await _ssmServerUtilizationRepository.GetByServerIdsAsync(new List<string> { request.ServerId }, dateRange.startOfDayUTC, dateRange.endOfDayUTC);

            var sortedUtilizations = utilizations.OrderBy(x => x.CreatedOn).ToList();

            var chartData = sortedUtilizations.Select(x => new SsmServerCpuUtilizationData
            {
                CreatedOn = x.CreatedOn.Value,
                TotalUsage = x.TotalProcessorUsage
            }).ToList();

            var latest = sortedUtilizations.LastOrDefault();

            return new SsmServerCpuUtilizationResponse
            {
                ChartData = chartData,
                CpuSystemUsage = latest?.CpuSystemUsage ?? 0,
                CpuMediaUsage = latest?.CpuMediaUsage ?? 0
            };
        }

        public async Task<SsmServerRamUtilizationResponse> GetSsmServerRamUtilizationAsync(SsmServerRamUtilizationRequest request, string userId)
        {
            var dateRange = await GetDateRangeFromUtcByUserId(request.Date, userId);
            var utilizations = await _ssmServerUtilizationRepository.GetByServerIdsAsync(new List<string> { request.ServerId }, dateRange.startOfDayUTC, dateRange.endOfDayUTC);

            var sortedUtilizations = utilizations.OrderBy(x => x.CreatedOn).ToList();

            var chartData = sortedUtilizations.Select(x => new SsmServerRamUtilizationData
            {
                CreatedOn = x.CreatedOn.Value,
                TotalUsage = x.TotalMemoryUsage
            }).ToList();

            var latest = sortedUtilizations.LastOrDefault();

            return new SsmServerRamUtilizationResponse
            {
                ChartData = chartData,
                MemorySystemUsage = latest?.MemorySystemUsage ?? 0,
                MemoryMediaUsage = latest?.MemoryMediaUsage ?? 0
            };
        }

        public async Task<SSMDeviceDetailsResponse> GetSsmDeviceDetailsAsync(SSMDeviceDetailsRequest request, string userId)
        {
            var dateRange = await GetDateRangeFromUtcByUserId(request.DateFilter, userId);
            var ssmDeviceDetail = await _sSMDeviceDetailsRepository.GetSsmDeviceDetailsAsync(request, dateRange.startOfDayUTC, dateRange.endOfDayUTC);
            var deviceIds = ssmDeviceDetail.deviceDetails.Select(x => x.Id).ToList();

            // Fetch offline status   
            var offlineDict = await _ssmOfflinedeviceRepository.GetOfflineStatusAsync(request.ServerId, deviceIds, dateRange.startOfDayUTC, dateRange.endOfDayUTC);
           
            var response = new SSMDeviceDetailsResponse
            {
                SSMDeviceDetails = ssmDeviceDetail.deviceDetails.Select(x =>
                {
                    var status = "Connected";

                    if (offlineDict.TryGetValue(x.Id, out var records))
                    {
                        status = GetDeviceStatus(records, dateRange.startOfDayUTC, dateRange.endOfDayUTC);
                    }

                    return new SSMDeviceDetailsItem
                    {
                        Id = x.Id,
                        Name = x.Name,
                        CameraModel = "-",
                        IpAddress = x.IpAddress,
                        Location = x.Location,
                        RecordingStatus = x.RecordingStatus,
                        CameraStatus = status
                    };
                }).ToList(),

                TotalCount = ssmDeviceDetail.count
            };

            return response;
        }
        public async Task<List<SsmServerHealthReportResponse>> GetSsmServerHealthReportAsync(SsmServerHealthReportRequest request, string userId)
        {
            var result = new List<SsmServerHealthReportResponse>();

            var siteMappings = await _sSMServerManagementRepository.GetByParentSiteIdsAsync(request.SsmSiteIds);

            if (!siteMappings.Any())
                return result;

            var mappingIds = siteMappings.Select(x => x.Id).ToList();

            var servers = await _ssmServerRepository.GetBySsmSiteIdsAsync(mappingIds);

            if (!servers.Any())
                return result;

            var serverIds = servers.Select(x => x.Id.ToString()).ToList();

            var startOfDayUTC = request.StartDateUtc;
            var endOfDayUTC = request.EndDateUtc;
            var totalWindowMinutes = (endOfDayUTC - startOfDayUTC).TotalMinutes;

            if (totalWindowMinutes <= 0)
                return result;

            var offlineHistory = await _ssmOfflineServerRepository.GetOfflineServerHistoryByServerIdsAsync(serverIds, startOfDayUTC, endOfDayUTC);

            var utilizations = await _ssmServerUtilizationRepository.GetByServerIdsAsync(serverIds, startOfDayUTC, endOfDayUTC);

            var allDevices = await _sSMDeviceDetailsRepository.GetDevicesByServerIdsAsync(serverIds);
            var allDeviceOfflineHistory = await _ssmOfflinedeviceRepository.GetOfflineDeviceHistoryByServerIdsAsync(serverIds, startOfDayUTC, endOfDayUTC);
            var allDeviceStoppedRecording = await _ssmDeviceStoppedRecordingRepository.GetStoppedRecordingHistoryByServerIdsAsync(serverIds, startOfDayUTC, endOfDayUTC);

            var parentSites = await _siteRepository.GetManyAsync(request.SsmSiteIds);
            var siteDict = new Dictionary<string, string>();
            foreach (var site in parentSites)
            {
                if (site.IsDeleted == false)
                    siteDict[site.Id.ToString()] = site.SiteName;
                if (site.ChildSites != null)
                {
                    foreach (var child in site.ChildSites)
                    {
                        if (child.IsDeleted == false)
                            siteDict[child.Id.ToString()] = child.SiteName;
                    }
                }
            }

            double ParseUsage(string usage)
            {
                if (string.IsNullOrWhiteSpace(usage)) return 0;
                var clean = usage.Replace("%", "").Trim();
                if (double.TryParse(clean, out var val)) return val;
                return 0;
            }

            foreach (var server in servers)
            {
                var srvId = server.Id.ToString();
                var srvHistory = offlineHistory.Where(x => x.ServerId == srvId).OrderBy(x => x.OfflineTime).ToList();
                var srvUtil = utilizations.Where(x => x.ServerId.ToString() == srvId).OrderBy(x => x.CreatedOn).ToList();

                var mapping = siteMappings.FirstOrDefault(x => x.Id == server.SsmSiteId);
                var siteId = mapping?.ChildSiteId ?? mapping?.ParentSiteId;
                var siteName = siteId != null && siteDict.ContainsKey(siteId) ? siteDict[siteId] : null;

                var healthResponse = new SsmServerHealthReportResponse
                {
                    Id = srvId,
                    Name = server.Name,
                    Port = server.Port,
                    Status = server.ServerStatus,
                    IpAddress = server.IpAddress,
                    SiteId = siteId,
                    SiteName = siteName
                };

                double totalDowntimeMin = 0;
                foreach (var hist in srvHistory)
                {
                    var offlineStart = hist.OfflineTime < startOfDayUTC ? startOfDayUTC : hist.OfflineTime;
                    var onlineEnd = (hist.OnlineTime == null || hist.OnlineTime > endOfDayUTC) ? endOfDayUTC : hist.OnlineTime.Value;

                    if (onlineEnd > offlineStart)
                    {
                        totalDowntimeMin += (onlineEnd - offlineStart).TotalMinutes;
                    }

                    healthResponse.ServerOfflineOnlineData.Add(new OfflineOnlineData
                    {
                        Id = hist.Id.ToString(),
                        OfflineTime = hist.OfflineTime,
                        OnlineTime = hist.OnlineTime
                    });
                }

                healthResponse.DownTimeMin = Math.Round(totalDowntimeMin, 2);
                double uptimePercent = 100.0 - (totalDowntimeMin / totalWindowMinutes * 100.0);
                healthResponse.UptimePercent = Math.Round(Math.Max(0, uptimePercent), 2);

                if (srvUtil.Any())
                {
                    healthResponse.CpuAvgPercent = Math.Round(srvUtil.Average(x => (x.TotalProcessorUsage)), 2);
                    healthResponse.RamAvgPercent = Math.Round(srvUtil.Average(x => (x.TotalMemoryUsage)), 2);
                    healthResponse.CpuMaxUtilizationPercent = Math.Round(srvUtil.Max(x => x.TotalProcessorUsage), 2);
                    healthResponse.RamMaxUtilizationPercent = Math.Round(srvUtil.Max(x => x.TotalMemoryUsage), 2);
                }

                var latestUtil = srvUtil.LastOrDefault();
                if (latestUtil != null && latestUtil.Disks != null && latestUtil.Disks.Any())
                {
                    healthResponse.TotalDiskSpace = latestUtil.Disks.Sum(x => x.Total);
                    healthResponse.FreeDiskSpace = latestUtil.Disks.Sum(x => x.Free);
                    if (healthResponse.TotalDiskSpace > 0)
                    {
                        healthResponse.DiskUtilizationPercent = Math.Round(((healthResponse.TotalDiskSpace - healthResponse.FreeDiskSpace) / (decimal)healthResponse.TotalDiskSpace) * 100, 2);
                    }
                }

                CpuSpikeData currentCpuSpike = null;
                RamSpikeData currentRamSpike = null;
                List<DiskSpikeData> currentDiskSpikes = new List<DiskSpikeData>();

                foreach (var util in srvUtil)
                {
                    if (util.CreatedOn == null) continue;

                    decimal cpu = util.TotalProcessorUsage;
                    if (cpu > 85)
                    {
                        if (currentCpuSpike == null)
                        {
                            currentCpuSpike = new CpuSpikeData { Id = util.Id.ToString(), CpuSpikeStartDatetime = util.CreatedOn };
                            healthResponse.CpuSpikeData.Add(currentCpuSpike);
                        }
                    }
                    else
                    {
                        if (currentCpuSpike != null)
                        {
                            currentCpuSpike.CpuNormalDatetime = util.CreatedOn;
                            currentCpuSpike = null;
                        }
                    }

                    decimal ram = util.TotalMemoryUsage;
                    if (ram > 85)
                    { 
                        if (currentRamSpike == null)
                        {
                            currentRamSpike = new RamSpikeData { Id = util.Id.ToString(), RamSpikeStartDatetime = util.CreatedOn };
                            healthResponse.RamSpikeData.Add(currentRamSpike);
                        }
                    }
                    else
                    {
                        if (currentRamSpike != null)
                        {
                            currentRamSpike.RamNormalDatetime = util.CreatedOn;
                            currentRamSpike = null;
                        }
                    }

                    if (util.Disks != null)
                    {
                        foreach (var disk in util.Disks)
                        {
                            if (disk.Total > 0)
                            {
                                decimal diskUtil = ((disk.Total - disk.Free) / (decimal)disk.Total) * 100;
                                if (diskUtil > 85)
                                {
                                    var activeSpike = currentDiskSpikes.FirstOrDefault(s => s.Drive == disk.Drive);
                                    if (activeSpike == null)
                                    {
                                        var spike = new DiskSpikeData
                                        {
                                            Id = util.Id.ToString(),
                                            Drive = disk.Drive,
                                            IpAddress = server.IpAddress,
                                            DiskSpikeStartDatetime = util.CreatedOn
                                        };
                                        currentDiskSpikes.Add(spike);
                                        healthResponse.DiskSpikeData.Add(spike);
                                    }
                                }
                                else
                                {
                                    var activeSpike = currentDiskSpikes.FirstOrDefault(s => s.Drive == disk.Drive);
                                    if (activeSpike != null)
                                    {
                                        activeSpike.DiskNormalDatetime = util.CreatedOn;
                                        currentDiskSpikes.Remove(activeSpike);
                                    }
                                }
                            }
                        }
                    }
                }

                var srvDevices = allDevices.Where(x => x.ServerId == srvId).ToList();
                healthResponse.TotalCameras = srvDevices.Count;
                healthResponse.OfflineCameras = srvDevices.Count(x => x.Status == 0);

                var srvDeviceOfflineHistory = allDeviceOfflineHistory.Where(x => x.ServerId == srvId).ToList();
                var srvDeviceStoppedRecording = allDeviceStoppedRecording.Where(x => x.ServerId == srvId).ToList();

                foreach (var device in srvDevices)
                {
                    var devId = device.Id.ToString();
                    var devOfflineHistory = srvDeviceOfflineHistory.Where(x => x.DeviceId == devId).OrderBy(x => x.OfflineTime).ToList();
                    var devStoppedRecording = srvDeviceStoppedRecording.Where(x => x.DeviceId == devId).OrderBy(x => x.StopRecordingTime).ToList();

                    var deviceData = new DeviceOfflineData
                    {
                        Id = devId,
                        Ip = device.IpAddress,
                        Name = device.Name,
                        Model = "-", 
                        Status = device.CameraStatus
                    };

                    double devDowntimeMin = 0;
                    foreach (var hist in devOfflineHistory)
                    {
                        var offlineStart = hist.OfflineTime < startOfDayUTC ? startOfDayUTC : hist.OfflineTime;
                        var onlineEnd = (hist.OnlineTime == null || hist.OnlineTime > endOfDayUTC) ? endOfDayUTC : hist.OnlineTime.Value;

                        int duration = 0;
                        if (onlineEnd > offlineStart)
                        {
                            duration = (int)Math.Round((onlineEnd - offlineStart).TotalMinutes);
                            devDowntimeMin += duration;
                        }

                        deviceData.OfflineEvents.Add(new DeviceOfflineEvent
                        {
                            Id = hist.Id.ToString(),
                            OfflineTime = hist.OfflineTime,
                            OnlineTime = hist.OnlineTime,
                            Duration = duration
                        });
                    }

                    deviceData.OfflineIncidence = devOfflineHistory.Count;
                    deviceData.OfflineDuration = $"{devDowntimeMin} Min";
                    double devOnlinePercent = 100.0 - (devDowntimeMin / totalWindowMinutes * 100.0);
                    deviceData.Online = $"{Math.Round(Math.Max(0, devOnlinePercent), 2)}%";

                    double devNoRecMin = 0;
                    foreach (var rec in devStoppedRecording)
                    {
                        var stopStart = rec.StopRecordingTime < startOfDayUTC ? startOfDayUTC : rec.StopRecordingTime;
                        var recEnd = (rec.StartRecordingTime == null || rec.StartRecordingTime > endOfDayUTC) ? endOfDayUTC : rec.StartRecordingTime.Value;

                        int duration = 0;
                        if (recEnd > stopStart)
                        {
                            duration = (int)Math.Round((recEnd - stopStart).TotalMinutes);
                            devNoRecMin += duration;
                        }

                        deviceData.StopRecording.Add(new DeviceStopRecordingEvent
                        {
                            Id = rec.Id.ToString(),
                            StopRecordingTime = rec.StopRecordingTime,
                            StartRecordingTime = rec.StartRecordingTime,
                            Duration = duration
                        });
                    }

                    deviceData.NoRecIncidence = devStoppedRecording.Count;
                    deviceData.NoRecDuration = $"{devNoRecMin} Min";
                    double devRecPercent = 100.0 - (devNoRecMin / totalWindowMinutes * 100.0);
                    deviceData.Rec = $"{Math.Round(Math.Max(0, devRecPercent), 2)}%";

                    if(devOfflineHistory.Count > 0 || devStoppedRecording.Count > 0)
                    {
                        healthResponse.DeviceOffline.Add(deviceData);
                    }
                }

                result.Add(healthResponse);
            }

            return result;
        }
        private string GetDeviceStatus(List<SsmOfflinedevice> records, DateTime startOfDayUTC, DateTime endOfDayUTC)
        {
            if (records == null || !records.Any())
                return "Connected";

            bool hasOffline = records.Any(r =>r.OfflineTime < endOfDayUTC && (!r.OnlineTime.HasValue || r.OnlineTime >= startOfDayUTC));

            bool hasOnline = records.Any(r =>
                r.OnlineTime.HasValue &&
                r.OnlineTime >= startOfDayUTC &&
                r.OnlineTime < endOfDayUTC
            );

            if (hasOffline && hasOnline)
                return "Warning";

            if (hasOffline)
                return "Disconnected";

            return "Connected";
        }
    }
}
