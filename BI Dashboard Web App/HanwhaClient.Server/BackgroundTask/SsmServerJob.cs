using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.SSM;
using Quartz;

namespace HanwhaClient.Server.BackgroundTask
{
    public class SsmServerJob : IJob
    {
        private readonly ISsmClientService _ssmClientService;
        private readonly ISsmServerRepository _ssmServerRepository;
        private readonly ISsmServerUtilizationRepository _ssmServerUtilizationRepository;
        private readonly ISSMDeviceDetailsRepository _ssmDeviceDetailsRepository;
        private readonly ISsmOfflinedeviceRepository _ssmOfflinedeviceRepository;
        private readonly ISsmOfflineServerRepository _ssmOfflineServerRepository;
        private readonly ISSMServerManagementRepository _SSMServerManagementRepository;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;


        public SsmServerJob(ISsmClientService ssmClientService,
            ISsmServerRepository ssmServerRepository,
            ISsmServerUtilizationRepository ssmServerUtilizationRepository,
            ISSMDeviceDetailsRepository ssmDeviceDetailsRepository,
            ISsmOfflinedeviceRepository ssmOfflinedeviceRepository,
            ISsmOfflineServerRepository ssmOfflineServerRepository,
            ISSMServerManagementRepository sSMServerManagementRepository,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            _ssmClientService = ssmClientService;
            _ssmServerRepository = ssmServerRepository;
            _ssmServerUtilizationRepository = ssmServerUtilizationRepository;
            _ssmDeviceDetailsRepository = ssmDeviceDetailsRepository;
            _ssmOfflinedeviceRepository = ssmOfflinedeviceRepository;
            _ssmOfflineServerRepository = ssmOfflineServerRepository;
            _SSMServerManagementRepository = sSMServerManagementRepository;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try { 
            var siteServers = await _SSMServerManagementRepository.GetAllAsync();

            foreach (var siteServer in siteServers)
            {
                string baseUrl = (siteServer.IsHttps ? "https://" : "http://") + siteServer.IPAddress + ":" + siteServer.Port;
                string username = siteServer.Username;
                string password = siteServer.Password;

                bool ok = false;
                try
                {
                    ok = await _ssmClientService.LoginAsync(baseUrl, username, password);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"LoginAsync threw an exception for site {baseUrl}: {ex.Message}");
                    
                    var exceptionLog2 = new ExceptionLog();
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLogService>();
                        exceptionLog2.ExceptionMessage = $"- Login failed at site {baseUrl} due to exception: {ex.Message}";
                        exceptionLog2.StackTrace = ex.StackTrace;
                        exceptionLog2.ExceptionType = ex.GetType().Name;
                        exceptionLog2.LoggedAt = DateTime.Now;
                        exceptionLog2.RequestPath = "VI SSM Monitoring job login exception";
                        exceptionLog2.ResponseTime = DateTime.Now;
                        exceptionLog2.IsSuccess = false;
                        await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                    }

                    await MarkSsmServersAsOffline(siteServer.Id, siteServer.IPAddress);
                    continue;
                }

                if (!ok)
                {
                    var exceptionLog2 = new ExceptionLog();
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLogService>();
                        exceptionLog2.ExceptionMessage = "- Login is getting failed at site :- " + baseUrl;
                        exceptionLog2.StackTrace = "SsmServerjob.cs job site server login";
                        exceptionLog2.ExceptionType = "login failed";
                        exceptionLog2.LoggedAt = DateTime.Now;
                        exceptionLog2.RequestPath = "VI SSM Monitoring job login failed";
                        exceptionLog2.ResponseTime = DateTime.Now;
                        exceptionLog2.IsSuccess = false;
                        await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                    }
                    continue;
                }
                else
                {
                    
                    var servers = await _ssmClientService.GetAsync<List<SsmServerSummary>>("/v3/metrics/servers");

                    foreach (var server in servers ?? Enumerable.Empty<SsmServerSummary>())
                    {
                        try
                        {
                            var serverDetails = await _ssmClientService.GetAsync<SsmServerDto>("/v3/metrics/servers/" + server.Guid);

                            if (serverDetails == null || serverDetails.Data == null)
                            {
                                continue;
                            }

                            serverDetails.Data.TotalCameraCount = server.Child?.Total ?? 0;
                            serverDetails.Data.FailureCameraCount = server.Child?.Failure ?? 0;

                            var result = await _ssmServerRepository.AddUpdateSsmServerDetails(serverDetails, siteServer.Id);
                            //if (result.success)
                            {
                                var disks = new List<DiskDetail>();
                                if (serverDetails.Data.ExtensionData != null)
                                {
                                    var diskDict = new Dictionary<string, DiskDetail>(StringComparer.OrdinalIgnoreCase);
                                    foreach (var kvp in serverDetails.Data.ExtensionData)
                                    {
                                        string key = kvp.Key;
                                        if (key.EndsWith("DiskFreeSize", StringComparison.OrdinalIgnoreCase))
                                        {
                                            string drive = key.Substring(0, key.Length - "DiskFreeSize".Length).ToUpper();
                                            if (!diskDict.ContainsKey(drive)) diskDict[drive] = new DiskDetail { Drive = drive };
                                            if (kvp.Value.ValueKind == System.Text.Json.JsonValueKind.String && long.TryParse(kvp.Value.GetString(), out long free))
                                            {
                                                diskDict[drive].Free = free;
                                            }
                                            else if (kvp.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
                                            {
                                                diskDict[drive].Free = kvp.Value.GetInt64();
                                            }
                                        }
                                        else if (key.EndsWith("DiskTotalSize", StringComparison.OrdinalIgnoreCase))
                                        {
                                            string drive = key.Substring(0, key.Length - "DiskTotalSize".Length).ToUpper();
                                            if (!diskDict.ContainsKey(drive)) diskDict[drive] = new DiskDetail { Drive = drive };
                                            if (kvp.Value.ValueKind == System.Text.Json.JsonValueKind.String && long.TryParse(kvp.Value.GetString(), out long total))
                                            {
                                                diskDict[drive].Total = total;
                                            }
                                            else if (kvp.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
                                            {
                                                diskDict[drive].Total = kvp.Value.GetInt64();
                                            }
                                        }
                                    }
                                    disks.AddRange(diskDict.Values);
                                }

                                var serverUtilization = new SsmServerUtilization
                                {
                                    ServerId = result.id,
                                    CreatedOn = DateTime.UtcNow,
                                    TotalProcessorUsage = Convert.ToDecimal(serverDetails.Data.TotalProcessorUsage),
                                    TotalMemoryUsage = Convert.ToDecimal(serverDetails.Data.TotalMemoryUsage),
                                    CpuSystemUsage = Convert.ToDecimal(serverDetails.Data.ServerProcessorUsage),
                                    MemorySystemUsage = Convert.ToDecimal(serverDetails.Data.ServerMemoryUsage),
                                    CpuMediaUsage = Convert.ToDecimal(serverDetails.Data.GatewayProcessorUsage),
                                    MemoryMediaUsage = Convert.ToDecimal(serverDetails.Data.GatewayMemoryUsage),
                                    Disks = disks,
                                    DiskTotalSize = Convert.ToInt64(serverDetails.Data.DiskTotalSize),
                                    DiskFreeSize = Convert.ToInt64(serverDetails.Data.DiskFreeSize),
                                    DiskFreePercentage = Convert.ToDecimal(serverDetails.Data.DiskFreePercentage),
                                    TotalCameraCount = serverDetails.Data.TotalCameraCount,
                                    FailureCameraCount = serverDetails.Data.FailureCameraCount,
                                };
                                await _ssmServerUtilizationRepository.InsertAsync(serverUtilization);

                                var ssmDevices = await _ssmClientService.GetAsync<List<SsmCameraDto>>("/v3/metrics/servers/" + server.Guid + "/channels");

                                if (ssmDevices != null && ssmDevices.Count > 0)
                                {
                                    var deviceResult = await _ssmDeviceDetailsRepository.AddUpdateSsmDeviceDetails(ssmDevices, serverId: result.id);
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
                                exceptionLog2.ExceptionMessage = ex.Message + ":---: " + server.Name;
                                exceptionLog2.StackTrace = ex.StackTrace;
                                exceptionLog2.ExceptionType = ex.GetType().Name;
                                exceptionLog2.LoggedAt = DateTime.Now;
                                exceptionLog2.RequestPath = "SSM Server";
                                exceptionLog2.ResponseTime = DateTime.Now;
                                exceptionLog2.IsSuccess = false;
                                await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                            }
                            continue;
                        }
                    }

                    await _ssmClientService.LogoutAsync("/v2/session");
                }
            }
            return;
            }
            catch (Exception ex) { 
            
            }

        }

        private async Task MarkSsmServersAsOffline(string siteId, string ipAddress)
        {
            try
            {
                var ssmServer = await _ssmServerRepository.GetBySsmServerByIpAsync(ipAddress);
                if (ssmServer != null)
                {
                    var dto = new SsmServerDto
                    {
                        Data = new SsmServerData
                        {
                            Name = ssmServer.Name,
                            Address = ssmServer.IpAddress,
                            Port = ssmServer.Port,
                            Status = "0"
                        }
                    };
                    await _ssmServerRepository.AddUpdateSsmServerDetails(dto, siteId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking SSM servers as offline for site {siteId}");
            }
        }
    }
}
