using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;

namespace HanwhaClient.Server.BackgroundTask
{
    public class DeviceEventsMonitorJob : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDeviceEventsMonitorJobService _deviceEventsMonitorJobService;
        private CancellationTokenSource _globalCancellationTokenSource;
        private readonly IArchiveTimeService _archiveService;

        public DeviceEventsMonitorJob(IServiceProvider serviceProvider,
            IDeviceEventsMonitorJobService deviceEventsMonitorJobService,
            IArchiveTimeService archiveTimeService)
        {
            _serviceProvider = serviceProvider;
            _deviceEventsMonitorJobService = deviceEventsMonitorJobService;
            _archiveService = archiveTimeService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _globalCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Start monitoring device tasks
            await _archiveService.GetClinetSettings();
            await StartDeviceTasks();

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //// Cancel all running tasks
            //_globalCancellationTokenSource.Cancel();
            //_deviceEventsMonitorJobService.KillAllTaskofDevice();
            //_globalCancellationTokenSource.Dispose();
        }

        private async Task StartDeviceTasks()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var deviceMasterService = scope.ServiceProvider.GetRequiredService<IDeviceMasterService>();
                var devices = await deviceMasterService.GetAllDevicesAsync();

                foreach (var device in devices)
                {
                    _deviceEventsMonitorJobService.StartTaskForDevice(device);
                }
            }
        }
    }
}
