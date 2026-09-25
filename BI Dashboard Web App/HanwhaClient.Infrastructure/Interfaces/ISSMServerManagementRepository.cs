using HanwhaClient.Model.DbEntities;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface ISSMServerManagementRepository : IRepositoryBase<SsmSiteMapping>
    {
        Task<SsmSiteMapping?> GetByIpAndPortAsync(string ipAddress, string port, string? excludeId = null);
        Task<List<SsmSiteMapping>> GetByParentSiteIdsAsync(List<string> parentSiteIds);
    }
}
