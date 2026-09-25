using DocumentFormat.OpenXml.Spreadsheet;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.DbEntities;
using MongoDB.Driver;
using Quartz;

namespace HanwhaClient.Server.BackgroundTask
{
    [DisallowConcurrentExecution]
    public class RetentionDBJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RetentionDBJob> _logger;

        public RetentionDBJob(IServiceProvider serviceProvider, ILogger<RetentionDBJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
          

            using (var scope = _serviceProvider.CreateScope())
            {
                var retentionDBService = scope.ServiceProvider.GetRequiredService<IRetentionDBService>();

                var deviceClientService = scope.ServiceProvider.GetRequiredService<IClientSettingService>();
                var clientSettingRepository = scope.ServiceProvider.GetRequiredService<IClientSettingRepository>();

                var retentionDetails = await deviceClientService.GetClientSetting();
                try
                {

                    if (retentionDetails.RetentionDBConfiguration != null)
                    {
                        if (retentionDetails.RetentionDBConfiguration.Enable)
                        {

                            // Call your existing backup method. We pass targetPath but your implementation can ignore it if not needed.
                            var IsSuccess = await retentionDBService.InsertOrDeleteRetentionData(retentionDetails.RetentionDBConfiguration.RetentionPeriod);

                            if (IsSuccess)
                            {
                                retentionDetails.RetentionDBConfiguration.RetentionTil = DateTime.Now.AddMonths(-retentionDetails.RetentionDBConfiguration.RetentionPeriod);
                                var update = Builders<ClientSettings>.Update
                                            .Set(c => c.RetentionDBConfiguration, retentionDetails.RetentionDBConfiguration)
                                            .Set(c => c.UpdatedOn, DateTime.UtcNow);
                                var data = await clientSettingRepository.UpdateFieldsAsync(retentionDetails.Id, update);
                                _logger.LogInformation("Backup succeeded. OutputPath: {OutputPath}", IsSuccess);
                            }
                            else
                            {
                                _logger.LogError("Backup failed: {Error}", IsSuccess);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred while executing backup job for  OutputPath: {OutputPath}", retentionDetails.BackupDBConfiguration.Path);
                }
            }
        }
    }
}
