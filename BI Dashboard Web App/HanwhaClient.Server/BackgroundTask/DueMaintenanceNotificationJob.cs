using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Infrastructure.Interfaces;
using Quartz;

namespace HanwhaClient.Server.BackgroundTask
{
    public class DueMaintenanceNotificationJob : IJob
    {
        private readonly IMaintenanceScheduleRepository _maintenanceScheduleRepository;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IOfflineDevicesRepository _offlineDevicesRepository;
        private readonly IDeviceMasterService _deviceMasterService;
        public DueMaintenanceNotificationJob(IMaintenanceScheduleRepository maintenanceScheduleRepository, IUserNotificationService userNotificationService, IDeviceMasterService deviceMasterService)
        {
            _maintenanceScheduleRepository = maintenanceScheduleRepository;
            _userNotificationService = userNotificationService;
            _deviceMasterService = deviceMasterService;
        }
        public async Task Execute(IJobExecutionContext context)
        {

            var dueCamera = await _maintenanceScheduleRepository.GetDueMaintenanceScheduleDevices();
            var deviceDetails = await _deviceMasterService.GetDevicesByDeviceIdAsync(dueCamera.Select(x => x.DeviceId).ToList());
            foreach (var device in dueCamera) {
                var deviceInfo = deviceDetails.Where(x => x.Id == device.DeviceId).FirstOrDefault();
                await _userNotificationService.AddUserNotification("Maintenance Due Alert : " + deviceInfo.DeviceName , "Maintenance Due Alert: " + deviceInfo.DeviceName, "", null);
            }
            return ;
        }
    }
}
