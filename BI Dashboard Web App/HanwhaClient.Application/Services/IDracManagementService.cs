using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.SignalR;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Common.ReferenceData;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Web.Administration;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services
{
    public class IDracManagementService : IIDracManagementService
    {
        public readonly IIDracManagementRepository _iDracManagementRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IUsersRepository _userRepository;
        private readonly IIdracClientService _idracClientService;
        private readonly IIdracEventLogsRepository _idracEventLogsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ILogger<IDracManagementService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IStringLocalizer<AppMessages> _localizer;

        public IDracManagementService(IIDracManagementRepository iDracManagementRepository,
            ISiteRepository siteRepository,
            IUsersRepository userRepository,
            IIdracClientService idracClientService,
            IIdracEventLogsRepository idracEventLogsRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<IDracManagementService> logger,
            IServiceProvider serviceProvider,
            IHubContext<NotificationHub> hubContext,
            IStringLocalizer<AppMessages> localizer)
        {
            _iDracManagementRepository = iDracManagementRepository;
            _siteRepository = siteRepository;
            _userRepository = userRepository;
            _idracClientService = idracClientService;
            _idracEventLogsRepository = idracEventLogsRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _localizer = localizer;
        }
        public async Task<(bool isSuccess, string ErrorMessage)> AddUpdateServerDetails(IDracServerManagementRequest request, string userId)
        {
            var login = await _idracClientService.IdracLoginAsync(request.IPAddress, request.UserName, request.Password);
            if (!login)
            {
                return (false, "Unable to login iDRAC server");
            }
            await _idracClientService.IdracLogoutAsync(request.IPAddress);
            bool ipExists = await _iDracManagementRepository.IsIpExistsAsync(request.IPAddress, request.Id);
            if (ipExists)
            {
                return (false, "Server already exists with the same IP address");
            }

            if (string.IsNullOrEmpty(request.Id))
            {
                var data = new IDracMaster
                {
                    IPAddress = request.IPAddress,
                    Port = request.Port,
                    ParentSiteId = request.ParentSiteId,
                    ChildSiteId = request.ChildSiteId,
                    ServerName = request.ServerName,
                    UserName = request.UserName,
                    Password = request.Password,
                    CpuLoad = request.CpuLoad,
                    Temperature = request.Temperature,
                    MemoryUsage = request.MemoryUsage,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow
                };

                var result = await _iDracManagementRepository.InsertAsync(data);
                return (!string.IsNullOrEmpty(result), "");
            }
            else
            {
                var update = Builders<IDracMaster>.Update
                .Set(c => c.IPAddress, request.IPAddress)
                .Set(c => c.Port, request.Port)
                .Set(c => c.ParentSiteId, request.ParentSiteId)
                .Set(c => c.ChildSiteId, request.ChildSiteId)
                .Set(c => c.ServerName, request.ServerName)
                .Set(c => c.UserName, request.UserName)
                .Set(c => c.Password, request.Password)
                .Set(c => c.CpuLoad, request.CpuLoad)
                .Set(c => c.MemoryUsage, request.MemoryUsage)
                .Set(c => c.Temperature, request.Temperature)
                .Set(c => c.UpdatedOn, DateTime.UtcNow)
                .Set(c => c.UpdatedBy, userId);
                var result = await _iDracManagementRepository.UpdateFieldsAsync(request.Id, update);
                return (result, "");
            }
        }
        public async Task<(IEnumerable<IDracMaster> data, Dictionary<string, object> referenceData)> GetAllIDracServerAsync()
        {
            var servers = await _iDracManagementRepository.GetAllAsync();

            Dictionary<string, object> referenceData = new();

            var parentSiteIds = servers.Where(x => x.ParentSiteId != null).Select(x => x.ParentSiteId).Distinct();
            var parentSiteReferenceData = await GetIDracMasterParentSiteReferenceDataAsync(parentSiteIds);
            referenceData.Add("parentSite", parentSiteReferenceData);

            var childSiteIds = servers.Where(x => x.ParentSiteId != null).Select(x => x.ChildSiteId).Distinct();
            var childSiteReferenceData = await GetChildSiteReferenceDataAsync(childSiteIds);
            referenceData.Add("childSite", childSiteReferenceData);

            var CreatedByIds = servers.Select(x => x.CreatedBy).Distinct().ToList();
            var createByReferenceData = await GetIDracMasterReferenceDataAsync(CreatedByIds);
            referenceData.Add("createdBy", createByReferenceData);

            var UpdatedByIds = servers.Select(x => x.UpdatedBy).Distinct().ToList();
            var UpdateByReferenceData = await GetIDracMasterReferenceDataAsync(UpdatedByIds);
            referenceData.Add("updatedBy", UpdateByReferenceData);


            return await Task.FromResult((servers, referenceData));
        }
        public async Task<List<OptionModel<string, string>>> GetIDracMasterReferenceDataAsync(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<UserMaster> projection = Builders<UserMaster>.Projection
            .Include("Username")
            .Include("_id");
            var users = await _userRepository.GetManyAsync(ids, projection);
            options = users.Select(x => new OptionModel<string, string>(x.Id, x.Username)).ToList();
            return options;
        }
        public async Task<List<OptionModel<string, string>>> GetIDracMasterParentSiteReferenceDataAsync(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<SiteMaster> projection = Builders<SiteMaster>.Projection
            .Include("siteName")
            .Include("_id");
            var sites = await _siteRepository.GetManyAsync(ids, projection);
            options = sites.Select(x => new OptionModel<string, string>(x.Id, x.SiteName)).ToList();
            return options;
        }
        public async Task<List<OptionModel<string, string>>> GetChildSiteReferenceDataAsync(IEnumerable<string> ids)
        {
            var objectIds = ids.Select(id => (id)).ToList();

            var filter = Builders<SiteMaster>.Filter.ElemMatch(
                x => x.ChildSites,
                c => objectIds.Contains(c.Id)
            );

            var projection = Builders<SiteMaster>.Projection.Include("childSites");

            var sites = await _siteRepository.GetByFilterAsync(filter, projection);

            return sites
                .SelectMany(x => x.ChildSites ?? new List<ChildSite>())
                .Where(c => objectIds.Contains(c.Id))
                .Select(c => new OptionModel<string, string>(
                    c.Id.ToString(),
                    c.SiteName
                ))
                .DistinctBy(x => x.label)
                .ToList();
        }
        public async Task<bool> DeleteIDracServerManagement(DeleteIDracServerRequest request, string userId)
        {
            var result = await _iDracManagementRepository.SoftDeleteAsync(request.Id, userId);
            return result;
        }
        public async Task<(bool success, string errorMessage)> AddAlarmDetailsAsync(IDracEventAlarmRequest request)//(string Id, string ErrorMessage)
        {
            var login = await _idracClientService.IdracLoginAsync(request.IpAddress, request.UserName, request.Password);
            if (!login)
            {
                return (false, _localizer[MessageKeys.RecordNotInserted]);
            }
            var httpRequest = _httpContextAccessor.HttpContext?.Request;
            if (httpRequest == null)
            {
                await _idracClientService.IdracLogoutAsync(request.IpAddress);
                return (false, _localizer[MessageKeys.RecordNotInserted]);
            }
            // Validate HTTPS and localhost
            var isHttps = httpRequest.Scheme.Equals(
                "https",
                StringComparison.OrdinalIgnoreCase);

            var isLocalhost =
                httpRequest.Host.Host.Equals(
                    "localhost",
                    StringComparison.OrdinalIgnoreCase) ||
                httpRequest.Host.Host.StartsWith("127.") ||
                httpRequest.Host.Host.Equals("::1");

            if (!isHttps || isLocalhost)
            {
                await _idracClientService.IdracLogoutAsync(request.IpAddress);
                return (
                    false,
                    "To receive notifications from iDRAC, the Vision Insight application URL must use HTTPS and contain a valid IP address or domain name."
                );
            }
            var alarm = new AlarmDetails
            {
                AlarmId = Guid.NewGuid().ToString(),
                Name = request.Name,
                Event = request.Event,
                TimeLimit = request.TimeLimit
            };

            // Get server details first
            var server = await _iDracManagementRepository.GetAsync(request.ServerId);

            if (server == null)
            {
                await _idracClientService.IdracLogoutAsync(request.IpAddress);
                return (false, _localizer[MessageKeys.RecordNotInserted]);
            }

            // Check existing alarm event
            var existingAlarm = server.Alarms?
                .FirstOrDefault(x =>
                    x.Event.Equals(
                        request.Event,
                        StringComparison.OrdinalIgnoreCase));

            if (existingAlarm != null)
            {
                await _idracClientService.IdracLogoutAsync(request.IpAddress);
                return (false, _localizer[MessageKeys.EventAlreadyExist]);
            }

            var added = await _iDracManagementRepository.AddAlarmAsync(
                request.ServerId,
                alarm);

            if (!added)
            {
                await _idracClientService.IdracLogoutAsync(request.IpAddress);
                return (false, _localizer[MessageKeys.RecordNotInserted]);
            }
                

            if (!string.IsNullOrWhiteSpace(server.EventSubscriptionId))
            {
                await _idracClientService.IdracLogoutAsync(request.IpAddress);
                return (true, "");
            }

            // Call iDRAC APIs only once 
            var body = new
            {
                ServiceEnabled = true
            };

            var eventEnableApi1 = await _idracClientService.PatchIdracAsync<IdracEnableEventServiceResponse>(
                IdracApiConstant.IdracAlertEventServiceEnable1,
                body,
                request.IpAddress);

            var body2 = new
            {
                Attributes = new Dictionary<string, string>
                {
                    { "IPMILan.1.AlertEnable", "Enabled" }
                }
            };
            var eventEnableApi2 = await _idracClientService.PatchIdracAsync<IdracEnableEventServiceResponse>(
               IdracApiConstant.IdracAlertEventServiceEnable2,
               body2,
               request.IpAddress);

            var callbackUrl = $"{httpRequest.Scheme}://{httpRequest.Host}{IdracApiConstant.IdracAlertEventServiceReceiver}";
            var username = "idracadmin";
            var password = "Admin@12";

            var authValue = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{username}:{password}")
            );
            var subscriptionRequestBody = new IdracSubscriptionRequest
            {
                Destination = callbackUrl,
                Protocol = "Redfish",
                EventTypes = new List<string>
                {
                    "Alert"
                },
                Context = "Actual running .net server",
                HttpHeaders =
                [
                    new Dictionary<string, string>
                    {
                        {
                            "Authorization",
                            $"Basic {authValue}"
                        }
                    },
                    new Dictionary<string, string>
                    {
                        {
                            "Content-Type",
                            "application/json"
                        }
                    }
                ],
            };
            var response = await _idracClientService.PostIdracAsync<AlertEventSubscriptionResponse>(
               IdracApiConstant.IdracAlertEventServiceSubscriptions,
               subscriptionRequestBody,
               request.IpAddress);

            await _iDracManagementRepository.UpdateAlarmSubscriptionIdAsync(
                request.ServerId,
                alarm.AlarmId,
                response.Id);

            await _idracClientService.IdracLogoutAsync(request.IpAddress);
            return (true, "");
        }
        public async Task<bool> IdracEventAlarmReceiverAsync(SubscribedAlertEventPayload request)
        {
            if (request?.Events == null || !request.Events.Any())
                return false;

            var remoteIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "";
            var configuredEvents = await _iDracManagementRepository.GetEventsByIpAddressAsync(remoteIp);

            if (configuredEvents == null || !configuredEvents.Any())
                return false;

            // Filter only matching events
            var matchedEvents = request.Events
                 .Select(x =>
                 {
                     var matchedConfig = configuredEvents
                         .FirstOrDefault(e =>
                             !string.IsNullOrWhiteSpace(x.Message) &&
                             x.Message.Contains(
                                 e.Event,
                                 StringComparison.OrdinalIgnoreCase));

                     return new
                     {
                         IncomingEvent = x,
                         Config = matchedConfig
                     };
                 })
                 .Where(x => x.Config != null)
                 .ToList();

            if (!matchedEvents.Any())
                return false;
            // Prepare DB entities
            var entities = matchedEvents.Select(x => new IdracEventLogs
            {
                SourceIP = remoteIp,
                EventSubscriptionId = request.Id,
                EventName = request.Name,
                EventId = x.IncomingEvent.EventId,
                EventTimestamp = x.IncomingEvent.EventTimestamp,
                EventType = x.IncomingEvent.EventType,
                Message = x.IncomingEvent.Message,
                Severity = x.IncomingEvent.Severity,
                CreatedOn = DateTime.UtcNow
            }).ToList();

            var notificationData = entities.Select(x => x.Message);

            var jsonMessage = JsonConvert.SerializeObject(
                matchedEvents.Select(x => new
                {
                    EventId = x.IncomingEvent.EventId,
                    SourceIP = remoteIp,
                    Message = x.IncomingEvent.Message,
                    TimeLimit = x.Config.TimeLimit,
                    Event = x.Config.Event,
                    Severity = x.IncomingEvent.Severity
                })
            );


            await _hubContext.Clients.Group("iDRAC_Group").SendAsync("idracEventNotification", jsonMessage);
            
            await _idracEventLogsRepository.InsertManyAsync(entities);

            return true;

        }
        public async Task<bool> DeleteIdracSubscribedEventAsync(string alarmId, string serverId)
        {
            // Get server + alarm from DB
            var server = await _iDracManagementRepository.GetAsync(serverId);

            if (server == null)
                return false;

            // Find alarm
            var alarm = server.Alarms.FirstOrDefault(x => x.AlarmId == alarmId);

            if (alarm == null)
                return false;

            await _iDracManagementRepository.RemoveAlarmAsync(
               server.Id,
               alarmId);

            // Get updated server data
            var updatedServer =
                await _iDracManagementRepository.GetAsync(serverId);

            if (updatedServer == null)
                return false;

            // If alarms still exist then do nothing
            if (updatedServer.Alarms.Any())
            {
                return true;
            }


            var login = await _idracClientService.IdracLoginAsync(server.IPAddress, server.UserName, server.Password);
            if (!login)
            {
                return false;
            }

            // Last alarm deleted
            // Remove iDRAC subscription
            if (!string.IsNullOrWhiteSpace(updatedServer.EventSubscriptionId))
            {
                var deleteUrl =
                    $"/redfish/v1/EventService/Subscriptions/{updatedServer.EventSubscriptionId}";

                var response =
                    await _idracClientService.DeleteIdracAsync(
                        deleteUrl,
                        server.IPAddress);

                if (!response)
                    return false;
            }

            // Clear subscription id
            await _iDracManagementRepository.ClearEventSubscriptionIdAsync(
                serverId);
            await _idracClientService.IdracLogoutAsync(server.IPAddress);
            return true;

        }
        public async Task<JsonElement> GetIdracSubscribedEventsAsync()
        {
            
            var login = await _idracClientService.IdracLoginAsync("192.168.0.60", "root", "VWXYRVF3HHXF");
            if (!login)
            {
                return default;
                //log error
            }
            var response =
            await _idracClientService.GetIdracAsync<JsonElement>(
                "/redfish/v1/EventService/Subscriptions/",
                "192.168.0.60");

            return response;


        }
    }
}
