using HanwhaAdminApi.Model.DbEntities;
using HanwhaAdminApi.Model.Dto;

namespace HanwhaAdminApi.Infrastructure.Interfaces
{
    public interface IRoleScreenMappingRepository : IRepositoryBase<RoleScreenMapping>
    {
        Task<RoleScreenMapping> GetRoleScreenMappingAsync(string roleId);
        Task<long> SaveRoleScreenMappingAsync(string roleId, IEnumerable<ScreenMapping> obj, string userId);
        Task<long> DeleteRoleScreenMappingByRoleIdAsync(string roleId, string userId);
        Task<long> UpdateRegionScreenMappingAsync(SaveRegionRequest saveRegionRequest, string userId);
        Task<long> DeleteRegionPermissionByRoleIdAsync(string roleId, string regionId, string userId);
    }
}
