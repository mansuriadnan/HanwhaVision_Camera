using HanwhaClient.Application.Interfaces;
using Quartz;

namespace HanwhaClient.Server.BackgroundTask
{
    [DisallowConcurrentExecution]
    public class BackupDBJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackupDBJob> _logger;

        public BackupDBJob(IServiceProvider serviceProvider, ILogger<BackupDBJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var backupService = scope.ServiceProvider.GetRequiredService<IMongoService>();

                var deviceClientService = scope.ServiceProvider.GetRequiredService<IClientSettingService>();

                var backupDetails = await deviceClientService.GetClientSetting();
                try
                {
                    // Call your existing backup method. We pass targetPath but your implementation can ignore it if not needed.
                    var (IsSuccess, OutputPath, Error) = await backupService.BackupDatabaseAsync(backupDetails.BackupDBConfiguration == null ? null : backupDetails.BackupDBConfiguration.Path);

                    if (IsSuccess)
                    {
                        _logger.LogInformation("Backup succeeded. OutputPath: {OutputPath}", OutputPath);
                    }
                    else
                    {
                        _logger.LogError("Backup failed: {Error}", Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred while executing backup job for  OutputPath: {OutputPath}", backupDetails.BackupDBConfiguration.Path);
                }
            }
        }
    }
}
