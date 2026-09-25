using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Net.NetworkInformation;

namespace HanwhaClient.BackgroundTask
{
    public class CheckDeviceStatusJob : IJob
    {
        private readonly IDeviceMasterService _deviceMasterService;
        private readonly IFileLogger _fileLogger;
        private readonly IDeviceEventsMonitorJobService _deviceEventsMonitorJobService;
        private readonly IOfflineDevicesRepository _offlineDevicesRepository;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;
        public CheckDeviceStatusJob(IDeviceMasterService deviceMasterService,
            IFileLogger fileLogger,
            IDeviceEventsMonitorJobService deviceEventsMonitorJobService,
            IOfflineDevicesRepository offlineDevicesRepository,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            _deviceMasterService = deviceMasterService;
            _fileLogger = fileLogger;
            _deviceEventsMonitorJobService = deviceEventsMonitorJobService;
            _offlineDevicesRepository = offlineDevicesRepository;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var devices = await _deviceMasterService.GetAllDevicesAsync();
                if (devices != null && devices.Count() > 0)
                {
                    var itemChunk = devices.Chunk(50);
                    foreach (var item in itemChunk)
                    {
                        foreach (var device in item)
                        {
                            //var hostUrl = device.IpAddress.Contains(':') ? device.IpAddress.Split(":")[0] : device.IpAddress;

                            //Ping ping = new Ping();

                            //PingReply result = ping.Send(hostUrl);
                            //var pingResult = result.Status;
                            //_deviceMasterService.ChangeDeviceStatusAsync(device.Id, pingResult == IPStatus.Success);
                            //HttpResponseMessage response = new HttpResponseMessage();
                            try
                            {
                                Uri uri = new Uri(((bool)device.IsHttps ? "https://" : "http://") + device.IpAddress);
                                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));

                                if (device.IsOnline == false && response.IsSuccessStatusCode)
                                {
                                    await _deviceMasterService.ChangeDeviceStatusAsync(device.Id, response.IsSuccessStatusCode);
                                    _deviceEventsMonitorJobService.StartTaskForDevice(device);
                                    await _offlineDevicesRepository.UpdateOfflineDeviceStatus(device.Id);
                                }

                                if (device.IsOnline == true && !response.IsSuccessStatusCode)
                                {
                                    await _deviceMasterService.ChangeDeviceStatusAsync(device.Id, response.IsSuccessStatusCode);
                                    await _offlineDevicesRepository.InsertAsync(new Model.DbEntities.OfflineDevices
                                    {
                                        DeviceId = device.Id,
                                        OfflineTime = DateTime.UtcNow,
                                        Status = 1,
                                        OnlineTime = null
                                    });

                                    _fileLogger.Log($"{device.IpAddress} ------> is offline");
                                }

                            }
                            catch (Exception ex)
                            {
                                if (device.IsOnline == true)
                                {
                                    await _deviceMasterService.ChangeDeviceStatusAsync(device.Id, false);
                                    await _offlineDevicesRepository.InsertAsync(new Model.DbEntities.OfflineDevices
                                    {
                                        DeviceId = device.Id,
                                        OfflineTime = DateTime.UtcNow,
                                        Status = 1,
                                        OnlineTime = null
                                    });
                                }

                                _logger.LogError(ex, ex.Message);
                                var exceptionLog2 = new ExceptionLog();
                                using (var scope = _serviceProvider.CreateScope())
                                {
                                    var exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLogService>();
                                    exceptionLog2.ExceptionMessage = ex.Message + device.IpAddress;
                                    exceptionLog2.StackTrace = ex.StackTrace;
                                    exceptionLog2.ExceptionType = ex.GetType().Name;
                                    exceptionLog2.LoggedAt = DateTime.Now;
                                    exceptionLog2.RequestPath = "Check device status job";
                                    exceptionLog2.ResponseTime = DateTime.Now;
                                    exceptionLog2.IsSuccess = false;
                                    await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                                }
                            }
                            await Task.Delay(500);
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
                    exceptionLog2.RequestPath = "Check device status job";
                    exceptionLog2.ResponseTime = DateTime.Now;
                    exceptionLog2.IsSuccess = false;
                    await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                }
            }

            return;
        }

    }
}
