using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using Quartz;

namespace HanwhaClient.Server.BackgroundTask
{
    public class LinkedServerPermissionSyncJob : IJob
    {
        private readonly ICacheService _cacheService;
        private readonly IViServerManagementRepository _serverManagementRepository;
        private readonly ILinkedServerPermissionSyncService _linkedServerPermissionSyncService;

        public LinkedServerPermissionSyncJob(
            ICacheService cacheService,
            IViServerManagementRepository serverManagementRepository,
            ILinkedServerPermissionSyncService linkedServerPermissionSyncService)
        {
            _cacheService = cacheService;
            _serverManagementRepository = serverManagementRepository;
            _linkedServerPermissionSyncService = linkedServerPermissionSyncService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var linkedServers = await _serverManagementRepository.GetAllAsync();

            if (linkedServers == null || !linkedServers.Any())
                return;

            var tasks = linkedServers.Select(server => _linkedServerPermissionSyncService.FetchAndCacheServerAsync(server));
            await Task.WhenAll(tasks);

            await _cacheService.RemoveAsync(CacheConstants.ServerManagement);
        }
    }
}
