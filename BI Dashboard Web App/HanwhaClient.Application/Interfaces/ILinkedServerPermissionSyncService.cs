using HanwhaClient.Model.DbEntities;

namespace HanwhaClient.Application.Interfaces
{
    public interface ILinkedServerPermissionSyncService
    {
        Task FetchAndCacheServerAsync(ViMultiServerManagement linkedServer);
    }
}
