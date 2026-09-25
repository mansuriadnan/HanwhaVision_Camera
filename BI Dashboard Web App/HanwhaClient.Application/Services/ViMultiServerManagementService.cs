using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace HanwhaClient.Application.Services
{
    
    public class ViMultiServerManagementService : IViMultiServerManagementService
    {
        private readonly IViServerManagementRepository _serverManagementRepository;
        private readonly IReportService _reportService;
        private readonly IDeviceEventsMonitorJobService _deviceEventsMonitorJobService;
        private readonly MongoDbConnectionService _mongoDbConnectionService;
        private readonly ICacheService _cacheService;
        private readonly ILinkedServerPermissionSyncService _linkedServerPermissionSyncService;

        public ViMultiServerManagementService(IViServerManagementRepository serverManagementRepository, IReportService reportService,
            IDeviceEventsMonitorJobService deviceEventsMonitorJobService, MongoDbConnectionService mongoDbConnectionService, ICacheService cacheService,
            ILinkedServerPermissionSyncService linkedServerPermissionSyncService)
        {
            _serverManagementRepository = serverManagementRepository;
            _deviceEventsMonitorJobService = deviceEventsMonitorJobService;
            _reportService = reportService;
            _mongoDbConnectionService = mongoDbConnectionService;
            _cacheService = cacheService;
            _linkedServerPermissionSyncService = linkedServerPermissionSyncService;
        }


        public async Task<(bool isSuccess, string errorMessage)> AddUpdateServerDetails(ViMultiServerManagementDTO serverManagementRequest, string userId)
        {
            var apiBaseUrl = serverManagementRequest?.HostingAddress; 
            var username = serverManagementRequest.Username;
            var password = serverManagementRequest.Password;

            try
            {
                using var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
                var loginResponse = await _reportService.GetAuthToken(httpClient, username, password);
                if (!loginResponse.IsSuccess)
                {
                    return (false, "Authentication failed: The credentials provided for the server are invalid.");
                }
                
            }
            catch (Exception ex)
                 {
                return (false, "The provided server details are invalid. Please enter the correct details.");
            }

            await _cacheService.RemoveAsync(CacheConstants.ServerManagement);
            if (string.IsNullOrEmpty(serverManagementRequest.Id))
            {
                bool isExist = await _serverManagementRepository.CheckMultiServerExists(serverManagementRequest);
                if (isExist)
                {
                    return (false, "The server already exists with the provided details.");
                }

                var data = new ViMultiServerManagement
                {
                    ServerName = serverManagementRequest.ServerName,
                    DatabaseConnectionString = serverManagementRequest.DatabaseConnectionString,
                    HostingAddress = serverManagementRequest.HostingAddress,
                    Username = serverManagementRequest.Username,
                    Password = serverManagementRequest.Password,
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    IsActive = true,
                    IsAvailable = true
                };

                
                var result = await _serverManagementRepository.InsertAsync(data);
                var serverDetailsUpdate = await _serverManagementRepository.GetAsync(result);
                await _linkedServerPermissionSyncService.FetchAndCacheServerAsync(serverDetailsUpdate);
                await _mongoDbConnectionService.InitializeLinkedServers();
                return (!string.IsNullOrEmpty(result), "");
            }
            else
            {
                var update = Builders<ViMultiServerManagement>.Update
                .Set(c => c.Username, serverManagementRequest.Username)
                .Set(c => c.Password, serverManagementRequest.Password)
                .Set(c => c.DatabaseConnectionString, serverManagementRequest.DatabaseConnectionString)
                .Set(c => c.HostingAddress, serverManagementRequest.HostingAddress)
                .Set(c => c.ServerName, serverManagementRequest.ServerName)
                .Set(c => c.Username, serverManagementRequest.Username)
                .Set(c => c.UpdatedOn, DateTime.UtcNow)
                .Set(c => c.UpdatedBy, userId);
                var result = await _serverManagementRepository.UpdateFieldsAsync(serverManagementRequest.Id, update);
                var serverDetailsUpdate = await _serverManagementRepository.GetAsync(serverManagementRequest.Id);
                await _linkedServerPermissionSyncService.FetchAndCacheServerAsync(serverDetailsUpdate);
                await _mongoDbConnectionService.InitializeLinkedServers();
                return (result, "");
            }
        }

        public async Task<bool> DeleteServerManagement(DeleteServerManagement request, string userId)
        {
            await _cacheService.RemoveAsync(CacheConstants.ServerManagement);
            var result = await _serverManagementRepository.SoftDeleteAsync(request.Id, userId);
            return result;
        }

        public async Task<(bool isSuccess, string errorMessage)> EnableServerManagement(EnabledServerRequest request, string userId)
        {
            await _cacheService.RemoveAsync(CacheConstants.ServerManagement);
            var serverDetails = await _serverManagementRepository.GetAsync(request.Id);
            var apiBaseUrl = serverDetails.HostingAddress;
            var username = serverDetails.Username;
            var password = serverDetails.Password;

            using var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
            var loginResponse = await _reportService.GetAuthToken(httpClient, username, password);
            if (!loginResponse.IsSuccess)
            {
                return (false, "Authentication failed: The credentials provided for the server are invalid.");
            }
            httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", loginResponse.Data.AccessToken);

            var deviceRequest = new DeviceRequest();
            var deviceJson = JsonConvert.SerializeObject(deviceRequest);
            var deviceContent = new StringContent(deviceJson, Encoding.UTF8, "application/json");

            var deviceListData = await httpClient.PostAsync($"{apiBaseUrl}/api/Device/GetAllDevices", deviceContent);
            var deviceList = JsonConvert.DeserializeObject<StandardAPIResponse<DeviceResponse>>(await deviceListData.Content.ReadAsStringAsync());

            if (request.IsActive && deviceList != null && deviceList.Data != null && deviceList.Data.DeviceDetails != null)
            {
                foreach (var device in deviceList.Data.DeviceDetails)
                {
                    var deviceMaster = new DeviceMaster
                    {
                        Id = device.Id,
                        APIModel = device.ApiModel,
                        DeviceName = device.DeviceName,
                        DeviceType = device.DeviceType,
                        IpAddress = device.IpAddress,
                        IsOnline = device.IsOnline,
                        UserName = device.UserName,
                        Password = device.Password,
                        IsHttps = (bool)device.IsHttps
                    };
                    _deviceEventsMonitorJobService.StartTaskForDevice(deviceMaster);
                }
            }
            else if(request.IsActive == false && deviceList != null && deviceList.Data != null && deviceList.Data.DeviceDetails != null)
            {
                foreach (var device in deviceList.Data.DeviceDetails)
                {
                    _deviceEventsMonitorJobService.KillTaskForDevice(device.Id);
                }
            }


            var update = Builders<ViMultiServerManagement>.Update
                    .Set(c => c.IsActive, request.IsActive)
                    .Set(c => c.UpdatedOn, DateTime.UtcNow)
                    .Set(c => c.UpdatedBy, userId);
            var result = await _serverManagementRepository.UpdateFieldsAsync(request.Id, update);

            var serverDetailsUpdate = await _serverManagementRepository.GetAsync(request.Id);

            await _linkedServerPermissionSyncService.FetchAndCacheServerAsync(serverDetailsUpdate);

            await _mongoDbConnectionService.InitializeLinkedServers();
            return (result, "");
        }

        public async Task<IEnumerable<ViMultiServerManagement>> GetAllServerDetails()
        {
            var cachedServerMgt = await _cacheService.GetAsync<IEnumerable<ViMultiServerManagement>>(CacheConstants.ServerManagement);
            if (cachedServerMgt != null)
            {
                return cachedServerMgt;
            }
            var data = await _serverManagementRepository.GetAllAsync();
            await _cacheService.SetAsync(CacheConstants.ServerManagement, data);
            return data;
        }

        public async Task<IEnumerable<GetViMultiServerDashDto>> GetAllActiveServerForDashboardAsync()
        {
            var result = new List<GetViMultiServerDashDto>
                             {
                                 new GetViMultiServerDashDto
                                 {
                                     Id = "000000000000000000000000",
                                     ServerName = "My Server"
                                 }
                             };

            var cachedServerMgt = await _cacheService.GetAsync<IEnumerable<ViMultiServerManagement>>(CacheConstants.ServerManagement);

            if (cachedServerMgt != null)
            {
                result.AddRange(cachedServerMgt.Where(x=>x.IsAvailable && x.IsActive).Select(x => new GetViMultiServerDashDto
                {
                    Id = x.Id.ToString(),
                    ServerName = x.ServerName
                }));

                return result;
            }

            var data = await _serverManagementRepository.GetAllActiveServer();

            if (data != null)
            {
                result.AddRange(data.Select(x => new GetViMultiServerDashDto
                {
                    Id = x.Id.ToString(),
                    ServerName = x.ServerName
                }));
            }
            return result;
        }
    }
}
