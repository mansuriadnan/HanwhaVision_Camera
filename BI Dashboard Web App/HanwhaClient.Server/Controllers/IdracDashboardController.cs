using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize([ScreenNames.IdracMaster])]
    public class IdracDashboardController : ControllerBase
    {
        private readonly IIdracDashboardService _idracDashboardService;
        private IStringLocalizer<AppMessages> _localizer;
        private readonly IIdracDetailsRepository _idracDetailsRepository;
        private readonly IIdracClientService _idracClientService;
        private readonly IIDracManagementRepository _idracManagementRepository;
        private readonly IExceptionLogService _exceptionLogService;

        public IdracDashboardController(IIdracDashboardService idracDashboardService,
            IStringLocalizer<AppMessages> localizer,
            IIdracDetailsRepository idracDetailsRepository,
            IIdracClientService idracClientService,
            IIDracManagementRepository idracManagementRepository,
            IExceptionLogService exceptionLogService)
        {
            _idracDashboardService = idracDashboardService;
            _localizer = localizer;
            _idracDetailsRepository = idracDetailsRepository;
            _idracClientService = idracClientService;
            _idracManagementRepository = idracManagementRepository;
            _exceptionLogService = exceptionLogService;
        }
        
        [HttpGet("idrac-server-list")]
        [CustomAuthorize([ScreenNames.ViewIdracDashboard])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<IdracServerListDashboardResponse>>>> GetIdracServerList([FromQuery] string parentSiteId)
        {
            var result = await _idracDashboardService.GetIdracServerListAsync(parentSiteId);
            var response = StandardAPIResponse<IEnumerable<IdracServerListDashboardResponse>>.SuccessResponse(result.data, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK, result.referenceData);
            return response;
        }
        
        [HttpPost("idrac-event-logs")]
        [CustomAuthorize([ScreenNames.ViewListofEvents])]
        public async Task<ActionResult<StandardAPIResponse<IdracEventLogsListResponse>>>GetIdracEventLogs([FromBody] IdracEventLogsListRequest request)
        {
            var result = await _idracDashboardService.GetIdracEventLogsAsync(request);

            return StandardAPIResponse<IdracEventLogsListResponse>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
        }
        
        [HttpPost("idrac-led-indicator")]
        [CustomAuthorize([ScreenNames.IdracServerLedIndicator])]
        public async Task<ActionResult<StandardAPIResponse<bool>>>UpdateIdracLedIndicator([FromBody] IdracLedIndicatorRequest request)
        {
            var result = await _idracDashboardService.UpdateIdracLedIndicatorAsync(request);
            if (result)
            {
                return StandardAPIResponse<bool>.SuccessResponse(result, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.ErrorResponse(result, "", StatusCodes.Status400BadRequest);
        }
        
        [HttpPost("idrac-power-action")]
        [CustomAuthorize([ScreenNames.IdracServerPowerAction])]
        public async Task<ActionResult<StandardAPIResponse<bool>>>IdracPowerAction([FromBody] IdracPowerActionRequest request)
        {
            var result = await _idracDashboardService.IdracPowerActionAsync(request);
            if (result)
            {
                return StandardAPIResponse<bool>.SuccessResponse(result, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.ErrorResponse(result, "", StatusCodes.Status400BadRequest);
        }
        
        [HttpPost("idrac-system-information")]
        [CustomAuthorize([ScreenNames.ViewIdracDashboard])]
        public async Task<ActionResult<StandardAPIResponse<IdracSystemInformationResponse>>>GetIdracSystemInformation([FromBody] IdracSystemInformationRequest request)
        {
            var result = await _idracDashboardService.GetSystemInformationAsync(request);

            return StandardAPIResponse<IdracSystemInformationResponse>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
        }
        
        [HttpGet("stream")]
        [CustomAuthorize([ScreenNames.ViewIdracDashboard])]
        public async Task Stream(string ServerId, string SiteId)
        {
            try
            {
                Response.Headers.Append("Content-Type", "text/event-stream");
                Response.Headers.Append("Cache-Control", "no-cache");
                Response.Headers.Append("Connection", "keep-alive");

                var serverList = await _idracManagementRepository.GetServersByParentSiteIdAsync(SiteId);
                var currentServer = serverList.Where(x => x.Id == ServerId).FirstOrDefault();
                IEnumerable<string> serverIdLst = serverList.Select(x => x.Id);

                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                    IdracOverallResponse idracOverallResponse = new IdracOverallResponse();

                    var idracLight = await _idracClientService.GetIdracAsync<IdracLightIndigator>(IdracApiConstant.IdracMainComponent, currentServer.IPAddress);

                    if(idracLight != null)
                    {
                        idracOverallResponse.LightIndigator = idracLight.LocationIndicatorActive;
                    }

                    if (serverIdLst != null && serverIdLst.Count() > 0)
                    {
                        List<IdracStatusReponse> idracStatusReponses = await _idracDetailsRepository.GetIdracServerHealth(serverIdLst);
                        idracOverallResponse.ServerHealth = idracStatusReponses;
                    }
                    
                    var idracDetails = await _idracDetailsRepository.GetIdracDetails(currentServer.Id);
                    idracOverallResponse.IdracDetails = idracDetails;

                    var json = JsonSerializer.Serialize(idracOverallResponse);

                    await Response.WriteAsync($"data: {json}\n\n");

                    await Response.Body.FlushAsync();

                    await Task.Delay(60000);
                }
            }
            catch (Exception ex)
            {
                var exceptionLog = new ExceptionLog
                {
                    ExceptionMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    ExceptionType = ex.GetType().Name,
                    LoggedAt = DateTime.UtcNow,
                    UserId = User.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value,
                    ResponseTime = DateTime.Now,
                    RequestPath = HttpContext.Request?.Path,
                    IsSuccess = false
                };
                await _exceptionLogService.SaveExceptionLogAsync(exceptionLog);
            }
        }

        [HttpGet("StreamCooling")]
        [CustomAuthorize([ScreenNames.ViewIdracDashboard])]
        public async Task StreamCooling(string ServerID)
        {
            IDracMaster serverDetail = null;
            try
            {
                Response.Headers.Append("Content-Type", "text/event-stream");
                Response.Headers.Append("Cache-Control", "no-cache");
                Response.Headers.Append("Connection", "keep-alive");

                serverDetail = await _idracManagementRepository.GetAsync(ServerID);
                var login = await _idracClientService.IdracLoginAsync(serverDetail.IPAddress, serverDetail.UserName, serverDetail.Password);

                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                    var CoolingMain = await _idracClientService.GetIdracAsync<ThermalRoot>(IdracApiConstant.IdracCoolingFan, serverDetail.IPAddress);
                    if (CoolingMain != null)
                    {
                        var temperaturesTasks = CoolingMain.Temperatures.Select(Temperature =>
                        {
                            CoolingTemperatureDetail indTemperatureDetail = new CoolingTemperatureDetail();
                            if (Temperature != null)
                            {
                                indTemperatureDetail.Name = Temperature.Name;
                                indTemperatureDetail.PhysicalContext = Temperature.PhysicalContext;
                                indTemperatureDetail.Temperature = Temperature.ReadingCelsius;
                                indTemperatureDetail.health = Temperature.Status.Health;
                            }
                            return indTemperatureDetail;
                        });

                        var json = JsonSerializer.Serialize(temperaturesTasks);

                        await Response.WriteAsync($"data: {json}\n\n");

                        await Response.Body.FlushAsync();
                    }
                    await Task.Delay(2000);
                }

                await _idracClientService.IdracLogoutAsync(serverDetail.IPAddress);
            }
            catch (Exception ex)
            {
                try
                {
                    if (serverDetail != null)
                    {
                        await _idracClientService.IdracLogoutAsync(serverDetail.IPAddress);
                    }
                }
                catch (Exception)
                {
                    // Ignore logout exception to ensure original exception is logged
                }

                var exceptionLog = new ExceptionLog
                {
                    ExceptionMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    ExceptionType = ex.GetType().Name,
                    LoggedAt = DateTime.UtcNow,
                    UserId = User.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value,
                    ResponseTime = DateTime.Now,
                    RequestPath = HttpContext.Request?.Path,
                    IsSuccess = false
                };
                await _exceptionLogService.SaveExceptionLogAsync(exceptionLog);
            }
        }

        [HttpGet("StreamCPUUsage")]
        [CustomAuthorize([ScreenNames.ViewIdracDashboard])]
        public async Task StreamCPUUsage(string ServerID)
        {
            IDracMaster serverDetail = null;
            try
            {
                Response.Headers.Append("Content-Type", "text/event-stream");
                Response.Headers.Append("Cache-Control", "no-cache");
                Response.Headers.Append("Connection", "keep-alive");

                serverDetail = await _idracManagementRepository.GetAsync(ServerID);
                var login = await _idracClientService.IdracLoginAsync(serverDetail.IPAddress, serverDetail.UserName, serverDetail.Password);

                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                    var cpuUsage = await _idracClientService.GetIdracAsync<CPUUsageResponse>(IdracApiConstant.IdracSystemBoardCPUUsage, serverDetail.IPAddress);

                    if (cpuUsage != null)
                    {
                        var result = new
                        {
                            Name = cpuUsage.Name,
                            Reading = cpuUsage.Reading
                        };

                        var json = JsonSerializer.Serialize(result);

                        await Response.WriteAsync($"data: {json}\n\n");
                        await Response.Body.FlushAsync();
                    }

                    await Task.Delay(2000);
                }
                await _idracClientService.IdracLogoutAsync(serverDetail.IPAddress);
            }
            catch (Exception ex)
            {
                try
                {
                    if (serverDetail != null)
                    {
                        await _idracClientService.IdracLogoutAsync(serverDetail.IPAddress);
                    }
                }
                catch (Exception)
                {
                    // Ignore logout exception to ensure original exception is logged
                }

                var exceptionLog = new ExceptionLog
                {
                    ExceptionMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    ExceptionType = ex.GetType().Name,
                    LoggedAt = DateTime.UtcNow,
                    UserId = User.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value,
                    ResponseTime = DateTime.Now,
                    RequestPath = HttpContext.Request?.Path,
                    IsSuccess = false
                };
                await _exceptionLogService.SaveExceptionLogAsync(exceptionLog);
            }
        }

        [HttpGet("StreamMemoryUsage")]
        [CustomAuthorize([ScreenNames.ViewIdracDashboard])]
        public async Task StreamMemoryUsage(string ServerID)
        {
            IDracMaster serverDetail = null;
            try
            {
                Response.Headers.Append("Content-Type", "text/event-stream");
                Response.Headers.Append("Cache-Control", "no-cache");
                Response.Headers.Append("Connection", "keep-alive");
                
                serverDetail = await _idracManagementRepository.GetAsync(ServerID);
                var login = await _idracClientService.IdracLoginAsync(serverDetail.IPAddress, serverDetail.UserName, serverDetail.Password);
                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                    var memUsage = await _idracClientService.GetIdracAsync<MemoryChartResponse>(IdracApiConstant.IdracSystemBoardMEMUsage, serverDetail.IPAddress);

                    if (memUsage != null)
                    {
                        var result = new
                        {
                            Name = memUsage.Name,
                            Reading = memUsage.Reading
                        };

                        var json = JsonSerializer.Serialize(result);

                        await Response.WriteAsync($"data: {json}\n\n");
                        await Response.Body.FlushAsync();
                    }

                    await Task.Delay(2000);
                }
                await _idracClientService.IdracLogoutAsync(serverDetail.IPAddress);
            }
            catch (Exception ex)
            {
                try
                {
                    if (serverDetail != null)
                    {
                        await _idracClientService.IdracLogoutAsync(serverDetail.IPAddress);
                    }
                }
                catch (Exception)
                {
                    // Ignore logout exception to ensure original exception is logged
                }

                var exceptionLog = new ExceptionLog
                {
                    ExceptionMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    ExceptionType = ex.GetType().Name,
                    LoggedAt = DateTime.UtcNow,
                    UserId = User.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value,
                    ResponseTime = DateTime.Now,
                    RequestPath = HttpContext.Request?.Path,
                    IsSuccess = false
                };
                await _exceptionLogService.SaveExceptionLogAsync(exceptionLog);
            }
        }

        [HttpGet("StreamEmbeddedNetworkUsage")]
        [CustomAuthorize([ScreenNames.ViewIdracDashboard])]
        public async Task StreamEmbeddedNetworkUsage(string ServerID)
        {
            IDracMaster serverDetail = null;
            try
            {
                Response.Headers.Append("Content-Type", "text/event-stream");
                Response.Headers.Append("Cache-Control", "no-cache");
                Response.Headers.Append("Connection", "keep-alive");

                serverDetail = await _idracManagementRepository.GetAsync(ServerID);
                var login = await _idracClientService.IdracLoginAsync(serverDetail.IPAddress, serverDetail.UserName, serverDetail.Password);

                var networkAdapter = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(IdracApiConstant.IdracNetworkAdapter, serverDetail.IPAddress);
                var embeddedUrl = networkAdapter.Members.Where(x => x.OdataId.Contains("Embedded")).FirstOrDefault();
                var networkAdapterMain = await _idracClientService.GetIdracAsync<NetworkAdapterMain>(embeddedUrl.OdataId, serverDetail.IPAddress);
                var networkportLinks = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(networkAdapterMain.NetworkPorts.OdataId, serverDetail.IPAddress);


                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                    List<NetworkPortResponse> networkPortResponse = new List<NetworkPortResponse>();
                    foreach (var networkLink in networkportLinks.Members)
                    {
                        var networkPortRes = await _idracClientService.GetIdracAsync<NetworkPortResponse>(networkLink.OdataId, serverDetail.IPAddress);
                        networkPortResponse.Add(networkPortRes);
                    }
                    
                    if (networkPortResponse != null && networkPortResponse.Count > 0)
                    {
                        var result = networkPortResponse.Select( x => new
                        {
                            Name = networkAdapterMain.Manufacturer,
                            Reading = x.CurrentLinkSpeedMbps,
                            Link = x.LinkStatus
                        });

                        var json = JsonSerializer.Serialize(result);

                        await Response.WriteAsync($"data: {json}\n\n");
                        await Response.Body.FlushAsync();
                    }

                    await Task.Delay(2000);
                }
                await _idracClientService.IdracLogoutAsync(serverDetail.IPAddress);
            }
            catch (Exception ex)
            {
                try
                {
                    if (serverDetail != null)
                    {
                        await _idracClientService.IdracLogoutAsync(serverDetail.IPAddress);
                    }
                }
                catch (Exception)
                {
                    // Ignore logout exception to ensure original exception is logged
                }

                var exceptionLog = new ExceptionLog
                {
                    ExceptionMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    ExceptionType = ex.GetType().Name,
                    LoggedAt = DateTime.UtcNow,
                    UserId = User.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value,
                    ResponseTime = DateTime.Now,
                    RequestPath = HttpContext.Request?.Path,
                    IsSuccess = false
                };
                await _exceptionLogService.SaveExceptionLogAsync(exceptionLog);
            }
        }

        [HttpGet("StreamIntegratedNetworkUsage")]
        [CustomAuthorize([ScreenNames.ViewIdracDashboard])]
        public async Task StreamIntegratedNetworkUsage(string ServerID)
        {
            IDracMaster serverDetail = null;
            try
            {
                Response.Headers.Append("Content-Type", "text/event-stream");
                Response.Headers.Append("Cache-Control", "no-cache");
                Response.Headers.Append("Connection", "keep-alive");

                serverDetail = await _idracManagementRepository.GetAsync(ServerID);
                var login = await _idracClientService.IdracLoginAsync(serverDetail.IPAddress, serverDetail.UserName, serverDetail.Password);

                var networkAdapter = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(IdracApiConstant.IdracNetworkAdapter, serverDetail.IPAddress);
                if (networkAdapter == null || networkAdapter.Members == null) return;

                var integratedUrl = networkAdapter.Members.FirstOrDefault(x => x.OdataId.Contains("Integrated"));
                if (integratedUrl == null) return;

                var networkAdapterMain = await _idracClientService.GetIdracAsync<NetworkAdapterMain>(integratedUrl.OdataId, serverDetail.IPAddress);
                if (networkAdapterMain == null || networkAdapterMain.NetworkPorts == null) return;

                var networkportLinks = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(networkAdapterMain.NetworkPorts.OdataId, serverDetail.IPAddress);
                if (networkportLinks == null || networkportLinks.Members == null) return;

                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                    var tasks = networkportLinks.Members.Select(networkLink => 
                        _idracClientService.GetIdracAsync<NetworkPortResponse>(networkLink.OdataId, serverDetail.IPAddress));
                    
                    var networkPortResponses = await Task.WhenAll(tasks);

                    var validResponses = networkPortResponses.Where(x => x != null).ToList();

                    if (validResponses.Any())
                    {
                        var result = validResponses.Select(x => new
                        {
                            Name = networkAdapterMain.Manufacturer,
                            Reading = x.CurrentLinkSpeedMbps,
                            Link = x.LinkStatus
                        });

                        var json = JsonSerializer.Serialize(result);

                        await Response.WriteAsync($"data: {json}\n\n");
                        await Response.Body.FlushAsync(); 
                    }

                    await Task.Delay(2000);
                }

                await _idracClientService.IdracLogoutAsync(serverDetail.IPAddress);
            }
            catch (Exception ex)
            {
                try
                {
                    if (serverDetail != null)
                    {
                        await _idracClientService.IdracLogoutAsync(serverDetail.IPAddress);
                    }
                }
                catch (Exception)
                {
                    // Ignore logout exception to ensure original exception is logged
                }

                var exceptionLog = new ExceptionLog
                {
                    ExceptionMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    ExceptionType = ex.GetType().Name,
                    LoggedAt = DateTime.UtcNow,
                    UserId = User.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value,
                    ResponseTime = DateTime.Now,
                    RequestPath = HttpContext.Request?.Path,
                    IsSuccess = false
                };
                await _exceptionLogService.SaveExceptionLogAsync(exceptionLog);
            }
        }

        [HttpGet("streamChart")]
        [CustomAuthorize([ScreenNames.ViewIdracDashboard])]
        public async Task StreamChart()
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            while (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                var payload = new
                {
                    value = Random.Shared.Next(0, 100),
                };

                var json = JsonSerializer.Serialize(payload);

                await Response.WriteAsync($"data: {json}\n\n");
                await Response.Body.FlushAsync();

                await Task.Delay(1000);
            }
        }

        [HttpPost("idrac-system-logs")]
        [CustomAuthorize([ScreenNames.ViewSystemLogsIdracServers])]
        public async Task<ActionResult<StandardAPIResponse<IdracSystemtLogsListResponse>>> GetIdracSystemLogs([FromBody] IdracSystemLogsListRequest request)
        {
            var result = await _idracDashboardService.GetIdracSystemLogsAsync(request);

            return StandardAPIResponse<IdracSystemtLogsListResponse>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
        }
    }
}
