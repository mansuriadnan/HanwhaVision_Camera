using HanwhaClient.Application.Interfaces;
using MongoDB.Driver.Linq;
using Quartz;
using System;
using System.IO;

namespace HanwhaClient.Server.BackgroundTask
{
    public class ANPRImageRetentionJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RetentionDBJob> _logger;

        public ANPRImageRetentionJob(IServiceProvider serviceProvider, ILogger<RetentionDBJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var clientService = scope.ServiceProvider.GetRequiredService<IClientSettingService>();
                var clientSettings = await clientService.GetClientSetting();

                try
                   {
                    if (clientSettings.ANPRImageConfiguration != null && clientSettings.ANPRImageConfiguration.RetentionPeriod != null && clientSettings.ANPRImageConfiguration.RetentionPeriod > 0)
                    {
                        if (!string.IsNullOrEmpty(clientSettings.ANPRImageConfiguration.ImagePath) && Directory.Exists(clientSettings.ANPRImageConfiguration.ImagePath))
                        {
                            DateTime targetDate = DateTime.Now.Date.AddMonths((int)-clientSettings.ANPRImageConfiguration.RetentionPeriod);

                            var largeImageFiles = Directory.GetFiles(Path.Combine(clientSettings.ANPRImageConfiguration.ImagePath, "Large"))
                                                      .Where(file =>
                                                          File.GetCreationTime(file) < targetDate);

                            foreach (var file in largeImageFiles)
                            {
                                File.Delete(file);
                            }

                            var smallImageFiles = Directory.GetFiles(Path.Combine(clientSettings.ANPRImageConfiguration.ImagePath, "Small"))
                                                      .Where(file =>
                                                          File.GetCreationTime(file) < targetDate);

                            foreach (var file in smallImageFiles)
                            {
                                File.Delete(file);
                            }
                        }
                        else
                        {
                            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "ANPRImages");
                            if (Directory.Exists(rootPath))
                            {
                                DateTime targetDate = DateTime.Now.Date.AddMonths((int)-clientSettings.ANPRImageConfiguration.RetentionPeriod);

                                var largeImageFiles = Directory.GetFiles(Path.Combine(rootPath, "Large"))
                                                          .Where(file =>
                                                              File.GetCreationTime(file) < targetDate);

                                foreach (var file in largeImageFiles)
                                {
                                    File.Delete(file);
                                }

                                var smallImageFiles = Directory.GetFiles(Path.Combine(rootPath, "Small"))
                                                          .Where(file =>
                                                              File.GetCreationTime(file) < targetDate);

                                foreach (var file in smallImageFiles)
                                {
                                    File.Delete(file);
                                }
                            }
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred while executing ANPR image retendion job for  ImagePath: {OutputPath}", clientSettings.ANPRImageConfiguration.ImagePath);
                }
            }
        }
    }
}
