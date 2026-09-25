using DocumentFormat.OpenXml.Presentation;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.DeviceApiResponse;
using MongoDB.Driver;
using Quartz;

namespace HanwhaClient.Server.BackgroundTask
{
    public class ManageOperationalTimingJob : IJob
    {
        private readonly IClientSettingService _clientSettingService;
        private readonly IDeviceMasterService _deviceMasterService;
        private readonly IDeviceApiService _deviceApiService;
        private readonly IDeviceDataStoreService _deviceDataStoreService;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IVehicleParkingCountRepository _vehicleParkingCountRepository;
        private readonly IVehicleCurrentParkingCountRepository _vehicleCurrentParkingCountRepository;

        public ManageOperationalTimingJob(IClientSettingService clientSettingService,
            IDeviceApiService deviceApiService,
            IDeviceDataStoreService deviceDataStoreService,
            IDeviceMasterService deviceMasterService,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IServiceProvider serviceProvider,
            IVehicleRepository vehicleRepository,
            IVehicleParkingCountRepository vehicleParkingCountRepository,
            IVehicleCurrentParkingCountRepository vehicleCurrentParkingCountRepository)
        {
            _clientSettingService = clientSettingService;
            _deviceApiService = deviceApiService;
            _deviceMasterService = deviceMasterService;
            _deviceDataStoreService = deviceDataStoreService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _vehicleRepository = vehicleRepository;
            _vehicleParkingCountRepository = vehicleParkingCountRepository;
            _vehicleCurrentParkingCountRepository = vehicleCurrentParkingCountRepository;
        }
        public async Task Execute(IJobExecutionContext context) 
        {
            try
            {
                var data = await _clientSettingService.GetClientSetting();
                var vehicleDeviceDetails = await _deviceMasterService.GetVehicleDeviceListAsync();
                if (data != null && data.OperationalTiming != null)
                {
                    DateTime operationalStartTimeUTC = data.OperationalTiming.StartTime.AddMinutes(-1);
                    DateTime localStartTime = TimeZoneInfo.ConvertTimeFromUtc(operationalStartTimeUTC, TimeZoneInfo.Local);

                    var startTime = localStartTime.TimeOfDay;
                    var now = DateTime.Now.TimeOfDay;

                    if (startTime >= now && (startTime - now) <= TimeSpan.FromMinutes(5))
                    {
                        // Sleep until the endTime is reached                
                        var sleepDuration = startTime - now;
                        await Task.Delay(sleepDuration);
                        var deviceList = await _deviceMasterService.GetAllDevicesAsync();
                        if (deviceList != null && deviceList.Count() > 0)
                        {
                            foreach (var device in deviceList)
                            {
                                if (device.APIModel == "SUNAPI")
                                {
                                    _deviceApiService.CallDeviceApi<ResetDeviceCountResponse>((device.IsHttps ? "https://" : "http://") + device.IpAddress + SunapiAPIConstant.ResetDeviceCount, device.UserName, device.Password);
                                }
                                else if (device.APIModel == "WiseAI")
                                {
                                    foreach (var channel in device.ObjectCountingConfiguration)
                                    {
                                        _deviceApiService.DeleteCallDeviceApi<ResetDeviceCountResponse>((device.IsHttps ? "https://" : "http://") + device.IpAddress + WiseAPIConstant.ResetObjectCouting + channel.Channel, device.UserName, device.Password);
                                        _deviceApiService.DeleteCallDeviceApi<ResetDeviceCountResponse>((device.IsHttps ? "https://" : "http://") + device.IpAddress + WiseAPIConstant.ResetVehicleHeatMap + channel.Channel, device.UserName, device.Password);
                                        _deviceApiService.DeleteCallDeviceApi<ResetDeviceCountResponse>((device.IsHttps ? "https://" : "http://") + device.IpAddress + WiseAPIConstant.ResetShoppingCartHeatMap + channel.Channel, device.UserName, device.Password);
                                        _deviceApiService.DeleteCallDeviceApi<ResetDeviceCountResponse>((device.IsHttps ? "https://" : "http://") + device.IpAddress + WiseAPIConstant.ResetForkliftHeatMap + channel.Channel, device.UserName, device.Password);
                                    }
                                }
                            }
                        }

                        // for change in parking current count if operation time is there
                        if(vehicleDeviceDetails != null && vehicleDeviceDetails.Count() > 0)
                        {
                            var datetime = DateTime.UtcNow;
                            foreach (var device in vehicleDeviceDetails)
                            {
                                if (device.ApiModel == "SUNAPI")
                                {
                                    VehicleCount vehicleCount = new VehicleCount();
                                    vehicleCount = await _vehicleRepository.GetLatestVehicleCountAsTimeAsync(device.Id, 0, datetime);
                                    if (vehicleCount != null)
                                    {
                                        foreach (var line in vehicleCount.VehicleCounts.SelectMany(x => x.Lines))
                                        {
                                            var currentId = await _vehicleCurrentParkingCountRepository.GetCurrentParkingCountId(device.Id, 0, line.LineIndex);
                                            var lastUpdatedparkingCount = await _vehicleParkingCountRepository.GetLatestVehicleParkingCountAsync(device.Id, 0, line.LineIndex, datetime);
                                            if (lastUpdatedparkingCount != null)
                                            {
                                                var update = Builders<VehicleCurrentParkingCount>.Update
                                                         .Set(c => c.ParkingCount, lastUpdatedparkingCount.ParkingCount)
                                                         .Set(c => c.CreatedOn, datetime)
                                                         .Set(c => c.UpdatedOn, datetime);

                                                var result = await _vehicleCurrentParkingCountRepository.UpdateFieldsAsync(currentId.Id, update);
                                            }

                                        }
                                    }
                                }
                                else if (device.ApiModel == "WiseAI")
                                {
                                    foreach (var channel in device.ChannelIndexList.Select(x => x.Channel).Distinct())
                                    {
                                        VehicleCount vehicleCount = new VehicleCount();
                                        vehicleCount = await _vehicleRepository.GetLatestVehicleCountAsTimeAsync(device.Id, channel, datetime);
                                        if (vehicleCount != null)
                                        {

                                            foreach (var line in vehicleCount.VehicleCounts.SelectMany(x => x.Lines))
                                            {
                                                var currentId = await _vehicleCurrentParkingCountRepository.GetCurrentParkingCountId(device.Id, channel, line.LineIndex);
                                                var lastUpdatedparkingCount = await _vehicleParkingCountRepository.GetLatestVehicleParkingCountAsync(device.Id, channel, line.LineIndex, datetime);
                                                if (lastUpdatedparkingCount != null)
                                                {
                                                    var update = Builders<VehicleCurrentParkingCount>.Update
                                                             .Set(c => c.ParkingCount, lastUpdatedparkingCount.ParkingCount)
                                                             .Set(c => c.CreatedOn, datetime)
                                                             .Set(c => c.UpdatedOn, datetime);

                                                    var result = await _vehicleCurrentParkingCountRepository.UpdateFieldsAsync(currentId.Id, update);
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }

                        await _deviceDataStoreService.ClearCacheData();
                        Console.WriteLine("Time condition matched. Executing job.");
                    }

                    return;
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
                    exceptionLog2.LoggedAt = DateTime.UtcNow;
                    exceptionLog2.RequestPath = "Manage operational time job";
                    exceptionLog2.ResponseTime = DateTime.UtcNow;
                    exceptionLog2.IsSuccess = false;
                    exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                }
                return ;
            }
            
            
        }
    }
}
