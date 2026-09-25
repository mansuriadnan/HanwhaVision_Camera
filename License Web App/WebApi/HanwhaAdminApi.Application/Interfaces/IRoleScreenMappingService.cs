using HanwhaAdminApi.Model.DbEntities;
using HanwhaAdminApi.Model.Dto;
using HanwhaAdminApi.Model.Dto.Role;

namespace HanwhaAdminApi.Application.Interfaces
{
    public interface IRoleScreenMappingService
    {
        Task<IEnumerable<RoleScreenMapping>> GetRoleScreenMappings();
        Task<(PermissionResponseDto data, Dictionary<string, object> referenceData)> GetRoleScreenMappingsByRoleIdAsync(string roleId);
        Task<string> AddUpdateRoleScreenMapping(List<RoleScreenMappingRequestDto> obj, string userId);
        Task<string> SaveRegionScreenMapping(SaveRegionRequest roleScreenMappingsData, string userId);
        Task<IEnumerable<CustomerRegionResDto>> GetPermissionRegionAsync(List<string> userRoles);

    }
}
