using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;
using Quartz;

namespace HanwhaClient.Server.BackgroundTask
{
    public class ReviveDeviceOfflineDataJob : IJob
    {
        private readonly IOfflineDevicesRepository _offlineDevicesRepository;
        private readonly IDeviceApiService _deviceApiService;
        private readonly IDeviceMasterService _deviceMasterService;
        private readonly IPeopleCountRepository _peopleCountRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public ReviveDeviceOfflineDataJob(IOfflineDevicesRepository offlineDevicesRepository,
            IDeviceApiService deviceApiService,
            IDeviceMasterService deviceMasterService,
            IPeopleCountRepository peopleCountRepository,
            IVehicleRepository vehicleRepository,
            IServiceProvider serviceProvider,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _offlineDevicesRepository = offlineDevicesRepository;
            _deviceApiService = deviceApiService;
            _deviceMasterService = deviceMasterService;
            _peopleCountRepository = peopleCountRepository;
            _vehicleRepository = vehicleRepository;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var unsyncDevices = await _offlineDevicesRepository.GetUnSyncOfflineDevices();
                var deviceDetils = await _deviceMasterService.GetDevicesByDeviceIdAsync(unsyncDevices.Select(x => x.DeviceId).ToList());

                if (unsyncDevices != null && unsyncDevices.Count() > 0)
                {
                    foreach (var device in unsyncDevices)
                        {
                            try
                            {
                                var deviceDetail = deviceDetils.Where(x => x.Id == device.DeviceId).FirstOrDefault();
                                if (device.OnlineTime != null && deviceDetail.IsOnline)
                                {
                                    var localOfflineTime = TimeZoneInfo.ConvertTimeFromUtc(device.OfflineTime, TimeZoneInfo.Local);
                                    var localOnlineTime = TimeZoneInfo.ConvertTimeFromUtc(device.OnlineTime.Value, TimeZoneInfo.Local);
                                    double diffDays = (localOnlineTime - localOfflineTime).TotalDays;
                                    if (diffDays > 1)
                                    { // for more then 1 day we can't recover data so we set offline time to near online date
                                        localOfflineTime = new DateTime(
                                                               localOnlineTime.Year,
                                                               localOnlineTime.Month,
                                                               localOnlineTime.Day,
                                                               0, 0, 0,
                                                               localOnlineTime.Kind
                                                           );

                                        var update = Builders<OfflineDevices>.Update.Set(x => x.Status, 4);
                                        await _offlineDevicesRepository.UpdateFieldsAsync(device.Id, update);
                                    }

                                    if (deviceDetail.ApiModel == "SUNAPI")
                                    {
                                        var token = await _deviceApiService.CallDeviceApi<OfflineDeviceTokenResponse>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + SunapiAPIConstant.PeopleOfflineDeviceGenerateToken + localOfflineTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") + "&ToDate=" + localOnlineTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") + "&Channel=0&Mode=Start", deviceDetail.UserName, deviceDetail.Password);
                                        if (token != null)
                                        {
                                            for (int i = 0; i < 5; i++)
                                            {
                                                var status = await _deviceApiService.CallDeviceApi<OfflineDeviceStatusResponse>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + SunapiAPIConstant.PeopleOfflineDeviceVerifyToken + token.SearchToken, deviceDetail.UserName, deviceDetail.Password);
                                                if (status != null && status.Status == "Completed")
                                                {
                                                    i = 10;
                                                    var data = await _deviceApiService.CallDeviceApi<PeopleCountSearchResponse>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + SunapiAPIConstant.PeopleOfflineDeviceGetData + token.SearchToken, deviceDetail.UserName, deviceDetail.Password);
                                                    PeopleCount latestPeopleCount = await _peopleCountRepository.GetLatestPeopleCountAsTimeAsync(device.DeviceId, 0, device.OfflineTime);
                                                    if (data != null && latestPeopleCount != null)
                                                    {
                                                        var latestPeopleCountDate = TimeZoneInfo.ConvertTimeFromUtc(latestPeopleCount.CreatedOn.Value, TimeZoneInfo.Local);
                                                        if (latestPeopleCountDate.Date != localOfflineTime.Date)
                                                        {
                                                            foreach (var linesData in latestPeopleCount.Lines)
                                                            {
                                                                linesData.InCount = 0;
                                                                linesData.OutCount = 0;
                                                                linesData.GenderInfo = null;
                                                                linesData.AgeInfo = null;
                                                            }
                                                        }

                                                        List<int> hourlyTimeList = GetCoveredHours(localOfflineTime, localOnlineTime);
                                                        foreach (int hour in hourlyTimeList)
                                                        {
                                                            var previousLines = latestPeopleCount?.Lines?
                                                                            .ToDictionary(x => x.Name, x => x)
                                                                            ?? new Dictionary<string, Line>();
                                                            var peopleCount = new PeopleCount
                                                            {
                                                                CameraIP = deviceDetail.IpAddress,
                                                                ChannelNo = 0,
                                                                DeviceId = device.DeviceId,
                                                                CreatedOn = TimeZoneInfo.ConvertTimeToUtc(new DateTime(localOfflineTime.Year,
                                                                            localOfflineTime.Month,
                                                                            localOfflineTime.Day,
                                                                            hour,
                                                                            localOfflineTime.Minute,
                                                                            localOfflineTime.Second,
                                                                            localOfflineTime.Kind).AddMinutes(2)),
                                                                Lines = data.PeopleCountSearchResults
                                                                        .FirstOrDefault()?.LineResults
                                                                        ?.Select(linedata =>
                                                                        {
                                                                            var previous = previousLines.TryGetValue(linedata.Line, out var line)
                                                                                ? line
                                                                                : null;

                                                                            return new Line
                                                                            {
                                                                                Name = linedata.Line,
                                                                                LineIndex = previous?.LineIndex ?? 0,

                                                                                InCount =
                                                                                    GetHourlyValue(
                                                                                        linedata.DirectionResults
                                                                                            .FirstOrDefault(d => d.Direction == "In")?.Result,
                                                                                        hour
                                                                                    ) + (previous?.InCount ?? 0),

                                                                                OutCount =
                                                                                    GetHourlyValue(
                                                                                        linedata.DirectionResults
                                                                                            .FirstOrDefault(d => d.Direction == "Out")?.Result,
                                                                                        hour
                                                                                    ) + (previous?.OutCount ?? 0)
                                                                            };
                                                                        })
                                                                        ?.ToList() ?? new List<Line>()
                                                            };

                                                            await _peopleCountRepository.InsertAsync(peopleCount);
                                                            latestPeopleCount = peopleCount;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        var vehicleToken = await _deviceApiService.CallDeviceApi<OfflineDeviceTokenResponse>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + SunapiAPIConstant.VehicleOfflineDeviceGenerateToken + localOfflineTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") + "&ToDate=" + localOnlineTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") + "&Channel=0&Mode=Start", deviceDetail.UserName, deviceDetail.Password);
                                        if (vehicleToken != null)
                                        {
                                            for (int i = 0; i < 5; i++)
                                            {
                                                var status = await _deviceApiService.CallDeviceApi<OfflineDeviceStatusResponse>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + SunapiAPIConstant.VehicleOfflineDeviceVerifyToken + vehicleToken.SearchToken, deviceDetail.UserName, deviceDetail.Password);
                                                if (status != null && status.Status == "Completed")
                                                {
                                                    i = 10;
                                                    var data = await _deviceApiService.CallDeviceApi<VehicleCountResponseSunapi>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + SunapiAPIConstant.VehicleOfflineDeviceGetData + vehicleToken.SearchToken + "&ShowAIStats=True", deviceDetail.UserName, deviceDetail.Password);
                                                    VehicleCount latestVehicleCount = await _vehicleRepository.GetLatestVehicleCountAsTimeAsync(device.DeviceId, 0, device.OfflineTime);
                                                    if (data != null && latestVehicleCount != null)
                                                    {
                                                        var latestVehicleCountDate = TimeZoneInfo.ConvertTimeFromUtc(latestVehicleCount.CreatedOn.Value, TimeZoneInfo.Local);
                                                        if (latestVehicleCountDate.Date != localOfflineTime.Date)
                                                        {
                                                            foreach (var vehicleCountData in latestVehicleCount.VehicleCounts)
                                                            {
                                                                foreach (var lineData in vehicleCountData.Lines)
                                                                {
                                                                    lineData.OutCount = 0;
                                                                    lineData.InCount = 0;
                                                                }
                                                            }
                                                        }

                                                        List<int> hourlyTimeList = GetCoveredHours(localOfflineTime, localOnlineTime);
                                                        var previousLines = latestVehicleCount?.VehicleCounts?
                                                                            .FirstOrDefault()?.Lines?
                                                                            .ToDictionary(x => x.Name, x => x.LineIndex)
                                                                            ?? new Dictionary<string, int>();

                                                        //change utc time in date insert check +1
                                                        foreach (int hour in hourlyTimeList)
                                                        {
                                                            var vehicleCount = new VehicleCount
                                                            {
                                                                CameraIP = deviceDetail.IpAddress,
                                                                ChannelNo = deviceDetail.ChannelNo,
                                                                DeviceId = device.DeviceId,
                                                                CreatedOn = TimeZoneInfo.ConvertTimeToUtc(
                                                                    new DateTime(
                                                                        localOfflineTime.Year,
                                                                        localOfflineTime.Month,
                                                                        localOfflineTime.Day,
                                                                        hour,
                                                                        localOfflineTime.Minute,
                                                                        localOfflineTime.Second,
                                                                        localOfflineTime.Kind
                                                                    ).AddMinutes(2)
                                                                ),
                                                                VehicleCounts = new List<VehicleCountData>
                                                        {
                                                            new VehicleCountData
                                                            {
                                                                Channel = deviceDetail.ChannelNo,
                                                                Lines = data.VehicleCountSearchResults
                                                                    .SelectMany(l => l.LineResults)
                                                                    .Select(lines => new Model.DbEntities.VehicleLine
                                                                    {
                                                                        Name = lines.Line,
                                                                        LineIndex = previousLines.TryGetValue(lines.Line, out var index)
                                                                            ? index
                                                                            : 0,

                                                                        InCount = GetHourlyValue(
                                                                            lines.DirectionResults.FirstOrDefault(d => d.Direction == "In")?.Result,
                                                                            hour
                                                                        ) + latestVehicleCount.VehicleCounts.SelectMany(l => l.Lines).Where(x => x.Name == lines.Line).First().InCount,

                                                                        OutCount = GetHourlyValue(
                                                                            lines.DirectionResults.FirstOrDefault(d => d.Direction == "Out")?.Result,
                                                                            hour
                                                                        ) + latestVehicleCount.VehicleCounts.SelectMany(l => l.Lines).Where(x => x.Name == lines.Line).First().OutCount
                                                                    })
                                                                    .ToList()
                                                            }
                                                        }
                                                            };

                                                            var result = await _vehicleRepository.InsertAsync(vehicleCount);
                                                            latestVehicleCount = vehicleCount;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        var update = Builders<OfflineDevices>.Update.Set(x => x.Status, 3);
                                        await _offlineDevicesRepository.UpdateFieldsAsync(device.Id, update);
                                    }
                                    else if (deviceDetail.ApiModel == "WiseAI")
                                    {
                                        foreach (var channelData in deviceDetail.ObjectConfiguration)
                                        {
                                            var personRuleIndex = deviceDetail.ObjectConfiguration
                                                                            .SelectMany(x => x.CountingRules)
                                                                            .Where(r => r.ObjectType.Equals("Person", StringComparison.OrdinalIgnoreCase))
                                                                            .Select(r => r.Index)
                                                                            .FirstOrDefault();
                                            var token = await _deviceApiService.CallDeviceApi<OfflineDeviceTokenResponse>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + WiseAPIConstant.OfflineDeviceGenerateTokenWise + localOfflineTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") + "&toDate=" + localOnlineTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") + "&channel=" + channelData.Channel + "&index=" + personRuleIndex, deviceDetail.UserName, deviceDetail.Password);
                                            if (token != null)
                                            {
                                                var status = await _deviceApiService.CallDeviceApi<OfflineDeviceStatusResponse>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + WiseAPIConstant.OfflineDeviceVerifyTokenWise + token.SearchToken, deviceDetail.UserName, deviceDetail.Password);
                                                if (status != null && status.Status == "Completed")
                                                {
                                                    var data = await _deviceApiService.CallDeviceApi<PeopleCountResponseWise>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + WiseAPIConstant.OfflineDeviceGetDataWise + token.SearchToken + "/results", deviceDetail.UserName, deviceDetail.Password);
                                                    PeopleCount latestPeopleCount = await _peopleCountRepository.GetLatestPeopleCountAsTimeAsync(device.DeviceId, channelData.Channel, device.OfflineTime);
                                                    if (data != null && latestPeopleCount != null)
                                                    {
                                                        var latestPeopleCountDate = TimeZoneInfo.ConvertTimeFromUtc(latestPeopleCount.CreatedOn.Value, TimeZoneInfo.Local);
                                                        if (latestPeopleCountDate.Date != localOfflineTime.Date)
                                                        {
                                                            foreach (var linesData in latestPeopleCount.Lines)
                                                            {
                                                                linesData.InCount = 0;
                                                                linesData.OutCount = 0;
                                                                linesData.GenderInfo = null;
                                                                linesData.AgeInfo = null;
                                                            }
                                                        }
                                                        var recoverDataPeople = data.CountingRules.Where(x => x.Index == personRuleIndex).FirstOrDefault();
                                                        if (recoverDataPeople != null)
                                                        {
                                                            List<int> hourlyTimeList = GetCoveredHours(localOfflineTime, localOnlineTime);
                                                            //change utc time in date insert check +1
                                                            foreach (int hour in hourlyTimeList)
                                                            {
                                                                var previousLines = latestPeopleCount?.Lines?
                                                                                    .ToDictionary(x => x.LineIndex, x => x)
                                                                                    ?? new Dictionary<int, Line>();

                                                                PeopleCount peopleCount = new PeopleCount
                                                                {
                                                                    CameraIP = deviceDetail.IpAddress,
                                                                    ChannelNo = channelData.Channel,
                                                                    DeviceId = device.DeviceId,
                                                                    CreatedOn = TimeZoneInfo.ConvertTimeToUtc(new DateTime(localOfflineTime.Year,
                                                                                localOfflineTime.Month,
                                                                                localOfflineTime.Day,
                                                                                hour,
                                                                                localOfflineTime.Minute,
                                                                                localOfflineTime.Second,
                                                                                localOfflineTime.Kind).AddMinutes(2)),

                                                                    Lines = recoverDataPeople.LineBasedResults
                                                                            .Select(linedata =>
                                                                            {
                                                                                previousLines.TryGetValue(linedata.Index, out var previous);

                                                                                var inCsv = linedata.DirectionBasedResult
                                                                                    .FirstOrDefault(d => d.Direction == "IN")?.Result;

                                                                                var outCsv = linedata.DirectionBasedResult
                                                                                    .FirstOrDefault(d => d.Direction == "OUT")?.Result;

                                                                                return new Line
                                                                                {
                                                                                    LineIndex = linedata.Index,
                                                                                    Name = previous?.Name ?? $"Line-{linedata.Index}",
                                                                                    InCount = inCsv[hour] + (previous?.InCount ?? 0),
                                                                                    OutCount = outCsv[hour] + (previous?.OutCount ?? 0)
                                                                                };
                                                                            })
                                                                            .ToList()
                                                                };
                                                                await _peopleCountRepository.InsertAsync(peopleCount);
                                                                latestPeopleCount = peopleCount;
                                                            }
                                                        }
                                                    }
                                                }
                                            }


                                            VehicleCount latestVehicleCount = await _vehicleRepository.GetLatestVehicleCountAsTimeAsync(device.DeviceId, channelData.Channel, device.OfflineTime);
                                            var vehicleRuleIndex = deviceDetail.ObjectConfiguration
                                                                    .SelectMany(x => x.CountingRules)
                                                                    .Where(r => r.ObjectType.Equals("Vehicle", StringComparison.OrdinalIgnoreCase))
                                                                    .Select(r => r.Index)
                                                                    .FirstOrDefault();
                                            var vehicleToken = await _deviceApiService.CallDeviceApi<OfflineDeviceTokenResponse>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + WiseAPIConstant.OfflineDeviceGenerateTokenWise + localOfflineTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") + "&toDate=" + localOnlineTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") + "&channel=" + channelData.Channel + "&index=" + vehicleRuleIndex, deviceDetail.UserName, deviceDetail.Password);
                                            if (vehicleToken != null)
                                            {
                                                var vehicleStatus = await _deviceApiService.CallDeviceApi<OfflineDeviceStatusResponse>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + WiseAPIConstant.OfflineDeviceVerifyTokenWise + vehicleToken.SearchToken, deviceDetail.UserName, deviceDetail.Password);
                                                if (vehicleStatus != null && vehicleStatus.Status == "Completed")
                                                {
                                                    var data = await _deviceApiService.CallDeviceApi<PeopleCountResponseWise>(((bool)deviceDetail.IsHttps ? "https://" : "http://") + deviceDetail.IpAddress + WiseAPIConstant.OfflineDeviceGetDataWise + vehicleToken.SearchToken + "/results", deviceDetail.UserName, deviceDetail.Password);
                                                    if (data != null && latestVehicleCount != null)
                                                    {
                                                        var latestVehicleCountDate = TimeZoneInfo.ConvertTimeFromUtc(latestVehicleCount.CreatedOn.Value, TimeZoneInfo.Local);
                                                        if (latestVehicleCountDate.Date != localOfflineTime.Date)
                                                        {
                                                            foreach (var vehicleCountData in latestVehicleCount.VehicleCounts)
                                                            {
                                                                foreach (var lineData in vehicleCountData.Lines)
                                                                {
                                                                    lineData.OutCount = 0;
                                                                    lineData.InCount = 0;
                                                                }
                                                            }
                                                        }

                                                        var recoverDataVehicle = data?.CountingRules.Where(x => x.Index == vehicleRuleIndex).FirstOrDefault();
                                                        if (recoverDataVehicle != null)
                                                        {
                                                            List<int> hourlyTimeList = GetCoveredHours(localOfflineTime, localOnlineTime);
                                                            foreach (int hour in hourlyTimeList)
                                                            {
                                                                var previousLines = latestVehicleCount?.VehicleCounts?
                                                                                    .SelectMany(vc => vc.Lines)
                                                                                    .ToDictionary(x => x.LineIndex, x => x)
                                                                                    ?? new Dictionary<int, Model.DbEntities.VehicleLine>();

                                                                VehicleCount vehicleCount = new VehicleCount
                                                                {
                                                                    CameraIP = deviceDetail.IpAddress,
                                                                    ChannelNo = channelData.Channel,
                                                                    DeviceId = device.DeviceId,
                                                                    CreatedOn = TimeZoneInfo.ConvertTimeToUtc(new DateTime(localOfflineTime.Year,
                                                                                localOfflineTime.Month,
                                                                                localOfflineTime.Day,
                                                                                hour,
                                                                                localOfflineTime.Minute,
                                                                                localOfflineTime.Second,
                                                                                localOfflineTime.Kind).AddMinutes(2)),

                                                                    VehicleCounts = new List<VehicleCountData>
                                                            {
                                                                new VehicleCountData
                                                                {
                                                                    Channel = deviceDetail.ChannelNo,
                                                                    Lines = recoverDataVehicle.LineBasedResults
                                                                        .Select(lines =>
                                                                        {
                                                                            previousLines.TryGetValue(lines.Index, out var previous);

                                                                            var inCsv = lines.DirectionBasedResult
                                                                                .FirstOrDefault(d => d.Direction == "IN")?.Result;

                                                                            var outCsv = lines.DirectionBasedResult
                                                                                .FirstOrDefault(d => d.Direction == "OUT")?.Result;

                                                                            return new VehicleLine
                                                                            {
                                                                                LineIndex = lines.Index,
                                                                                Name = previous.Name,
                                                                                InCount = inCsv[hour] + (previous?.InCount ?? 0),
                                                                                OutCount = outCsv[hour] + (previous?.OutCount ?? 0)
                                                                            };
                                                                        })
                                                                        .ToList()
                                                                }
                                                            }
                                                                };
                                                                await _vehicleRepository.InsertAsync(vehicleCount);
                                                                latestVehicleCount = vehicleCount;
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            var update = Builders<OfflineDevices>.Update.Set(x => x.Status, 3);
                                            await _offlineDevicesRepository.UpdateFieldsAsync(device.Id, update);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, ex.Message);
                                var exceptionLog2 = new ExceptionLog();
                                using (var scope = _serviceProvider.CreateScope())
                                {
                                    var exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLogService>();
                                    exceptionLog2.ExceptionMessage = ex.Message;
                                    exceptionLog2.StackTrace = ex.StackTrace;
                                    exceptionLog2.ExceptionType = ex.GetType().Name;
                                    exceptionLog2.LoggedAt = DateTime.Now;
                                    exceptionLog2.RequestPath = "Revive lost data background job";
                                    exceptionLog2.ResponseTime = DateTime.Now;
                                    exceptionLog2.IsSuccess = false;
                                    await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                                }
                                continue;
                            }
                        }   
                }
                return;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
                var exceptionLog2 = new ExceptionLog();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLogService>();
                    exceptionLog2.ExceptionMessage = ex.Message;
                    exceptionLog2.StackTrace = ex.StackTrace;
                    exceptionLog2.ExceptionType = ex.GetType().Name;
                    exceptionLog2.LoggedAt = DateTime.Now;
                    exceptionLog2.RequestPath = "Revive lost data background job";
                    exceptionLog2.ResponseTime = DateTime.Now;
                    exceptionLog2.IsSuccess = false;
                    await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                }
            }
        }

        public static List<int> GetCoveredHours(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("End time must be after start time");

            var hours = new List<int>();

            int startHour = start.Hour;
            int endHour = end.Hour;

            for (int hour = startHour; hour <= endHour; hour++)
            {
                hours.Add(hour);
            }

            return hours;
        }

        public int GetHourlyValue(string csv, int hour)
        {
            if (string.IsNullOrEmpty(csv)) return 0;

            var values = csv.Split(',');
            return hour < values.Length ? int.Parse(values[hour]) : 0;
        }

    }
}
