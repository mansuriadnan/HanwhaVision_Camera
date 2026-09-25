using HanwhaClient.Application.Interfaces;
using Quartz;

namespace HanwhaClient.Server.BackgroundTask
{
    public class MaintenanceScheduleJob : IJob
    {
        private readonly ILogger<MaintenanceScheduleJob> _logger;
        private readonly IMaintenancePlanSchedulingService _schedulingService;

        public MaintenanceScheduleJob(
            ILogger<MaintenanceScheduleJob> logger,
            IMaintenancePlanSchedulingService schedulingService
            )
        {
            _logger = logger;
            _schedulingService = schedulingService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Maintenance Schedule Job started at {Time}", DateTime.UtcNow);

            try
            {
                await _schedulingService.ProcessMaintenancePlansAsync();
                _logger.LogInformation("Maintenance Schedule Job completed successfully at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Maintenance Schedule Job");
                throw;
            }
        }
    }
}
