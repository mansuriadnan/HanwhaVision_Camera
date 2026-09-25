using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.SSM;


namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface ISsmServerRepository : IRepositoryBase<SsmServers>
    {
        Task<(bool success, string id)> AddUpdateSsmServerDetails(SsmServerDto ssmServer, string siteId);
        Task<List<SsmServers>> GetBySsmSiteIdsAsync(List<string> mappingIds);
        Task<SsmServers> GetBySsmServerByIpAsync(string ipAddress);
        Task<bool> SoftDeleteSsmServerAsync(string ssmSiteid, string userId);
    }
}
