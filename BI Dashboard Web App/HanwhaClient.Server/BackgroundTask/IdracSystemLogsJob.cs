using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Quartz;
using Microsoft.Extensions.DependencyInjection;

namespace HanwhaClient.Server.BackgroundTask
{
    public class IdracSystemLogsJob : IJob
    {
        private readonly IIdracClientService _idracClientService;
        private readonly IIDracManagementRepository _iIDracManagementRepository;
        private readonly IIdracSystemLogsRepository _idracSystemLogsRepository;
        private readonly IServiceProvider _serviceProvider;

        public IdracSystemLogsJob(
            IIdracClientService idracClientService, 
            IIDracManagementRepository iDracManagementRepository,
            IIdracSystemLogsRepository idracSystemLogsRepository,
            IServiceProvider serviceProvider) 
        {
            _idracClientService = idracClientService;
            _iIDracManagementRepository = iDracManagementRepository;
            _idracSystemLogsRepository = idracSystemLogsRepository;
            _serviceProvider = serviceProvider;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var idracList = await _iIDracManagementRepository.GetAllAsync();
            var tasks = idracList.Select(async idrac =>
            {
                try
                {
                    var login = await _idracClientService.IdracLoginAsync(idrac.IPAddress, idrac.UserName, idrac.Password);
                    if (login)
                    {
                        IdracDetails idracDetails = new IdracDetails();

                            var latestTimestamp = await _idracSystemLogsRepository.GetLatestLogTimestampAsync(idrac.Id);

                            int skip = 0;
                            int top = 100;
                            bool fetchMore = true;

                            while (fetchMore)
                            {
                                string apiUrl = IdracApiConstant.IdracSystemLogs.Replace("skip=0", $"skip={skip}");
                                var idracMainModel = await _idracClientService.GetIdracAsync<IdracLogResponse>(apiUrl, idrac.IPAddress);

                                if (idracMainModel != null && idracMainModel.Members != null && idracMainModel.Members.Count > 0)
                                {
                                    var newLogs = idracMainModel.Members
                                        .Where(m => !latestTimestamp.HasValue || m.Created > latestTimestamp.Value)
                                        .Select(m => new IdracSystemLogs
                                        {
                                            IdracServerId = idrac.Id,
                                            LogId = m.Id,
                                            Description = m.Message,
                                            Datetime = m.Created,
                                            Severity = m.Severity,
                                            CreatedOn = DateTime.UtcNow,
                                            IsDeleted = false
                                        })
                                        .ToList();

                                    if (newLogs.Any())
                                    {
                                        await _idracSystemLogsRepository.InsertManyAsync(newLogs);
                                    }

                                    // If we found 'top' records to insert, there might be more new records on the next page
                                    if (newLogs.Count >= top)
                                    {
                                        skip += top;
                                    }
                                    else
                                    {
                                        fetchMore = false;
                                    }
                                }
                                else
                                {
                                    fetchMore = false;
                                }
                                
                                // if latestTimestamp is null or empty, break the loop
                                if(latestTimestamp == null || !latestTimestamp.HasValue)
                                {
                                    fetchMore = false;
                                }
                            }
                    }
                    else
                    {
                        // Error log
                    }
                    await _idracClientService.IdracLogoutAsync(idrac.IPAddress);
                }
                catch (Exception ex)
                {
                    await _idracClientService.IdracLogoutAsync(idrac.IPAddress);
                    var exceptionLog2 = new ExceptionLog();
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLogService>();
                        exceptionLog2.ExceptionMessage = ex.Message;
                        exceptionLog2.StackTrace = ex.StackTrace;
                        exceptionLog2.ExceptionType = ex.GetType().Name;
                        exceptionLog2.LoggedAt = DateTime.Now;
                        exceptionLog2.RequestPath = "";
                        exceptionLog2.ResponseTime = DateTime.Now;
                        exceptionLog2.IsSuccess = false;
                        await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                    }
                    // Log exception for this specific server processing so the job can continue with the rest
                    Console.WriteLine($"Error processing IDRAC {idrac.IPAddress}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
