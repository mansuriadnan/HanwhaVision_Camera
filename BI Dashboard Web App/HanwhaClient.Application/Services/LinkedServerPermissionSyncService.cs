using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Utilities;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;

namespace HanwhaClient.Application.Services
{
    public class LinkedServerPermissionSyncService : ILinkedServerPermissionSyncService
    {
        private readonly ICacheService _cacheService;
        private readonly IViServerManagementRepository _serverManagementRepository;

        public LinkedServerPermissionSyncService(
            ICacheService cacheService,
            IViServerManagementRepository serverManagementRepository)
        {
            _cacheService = cacheService;
            _serverManagementRepository = serverManagementRepository;
        }

        public async Task FetchAndCacheServerAsync(ViMultiServerManagement linkedServer)
        {
            var cacheKey = $"FloorZonePermissions_{linkedServer.Id}";

            var linkedServerCache = new LinkedServerCacheEntry();

            try
            {
                using var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(linkedServer.HostingAddress),
                    Timeout = TimeSpan.FromSeconds(8)
                };

                var response = await AutheticationToken.GetFloorZoneLinkedServer(
                    httpClient,
                    linkedServer.Username,
                    linkedServer.Password,
                    CancellationToken.None
                );

                if (response?.Data?.DataAccessPermissions != null)
                {
                    linkedServerCache.IsAvailable = true;
                    linkedServerCache.IsActive = linkedServer.IsActive;
                    linkedServerCache.Permissions = response.Data.DataAccessPermissions.ToList();
                }
                else
                {
                    linkedServerCache.IsAvailable = false;
                    //linkedServerCache.IsActive = false;
                }
            }
            catch (TaskCanceledException)
            {
                linkedServerCache.IsAvailable = false;
                //linkedServerCache.IsActive = false;
            }
            catch (HttpRequestException)
            {
                linkedServerCache.IsAvailable = false;
                //linkedServerCache.IsActive = false;
            }
            catch (Exception ex)
            {
                linkedServerCache.IsAvailable = false;
                //linkedServerCache.IsActive = false;
            }
            finally
            {
                var update = Builders<ViMultiServerManagement>.Update
                .Set(c => c.IsAvailable, linkedServerCache.IsAvailable);
                //.Set(c => c.IsActive, linkedServerCache.IsActive);

                var result = await _serverManagementRepository.UpdateFieldsAsync(linkedServer.Id, update);

                // ✅ Always write — whether success or failure
                await _cacheService.SetAsync(cacheKey, linkedServerCache, TimeSpan.FromMinutes(6));
            }
        }
    }
}
