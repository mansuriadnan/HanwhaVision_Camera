using HanwhaAdminApi.Application.Interfaces;
using HanwhaAdminApi.Infrastructure.Interfaces;
using HanwhaAdminApi.Infrastructure.Repository;
using HanwhaAdminApi.Model.Common;
using HanwhaAdminApi.Model.Common.ReferenceData;
using HanwhaAdminApi.Model.DbEntities;
using HanwhaAdminApi.Model.Dto;
using HanwhaAdminApi.Model.Dto.Role;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace HanwhaAdminApi.Application.Services
{
    public class RoleScreenMappingService : IRoleScreenMappingService
    {
        private readonly IRoleScreenMappingRepository _roleScreenMappingRepository;
        private readonly IScreenMasterRepository _screenMasterRepository;
        private readonly IRolePermissionHistoryRepository _rolePermissionHistoryRepository;
        private readonly IRegionMasterRepository _regionMasterRepository;
        private readonly IRoleRepository _roleRepository;

        public RoleScreenMappingService(IRoleScreenMappingRepository _roleScreenMappingrepository,
            IScreenMasterRepository screenMasterRepository,
            IRolePermissionHistoryRepository rolePermissionHistoryRepository,
            IRegionMasterRepository regionMasterRepository,
            IRoleRepository roleRepository)
        {
            this._roleScreenMappingRepository = _roleScreenMappingrepository;
            this._screenMasterRepository = screenMasterRepository;
            this._rolePermissionHistoryRepository = rolePermissionHistoryRepository;
            _regionMasterRepository = regionMasterRepository;
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<RoleScreenMapping>> GetRoleScreenMappings()
        {
            var data = await _roleScreenMappingRepository.GetAllAsync();
            return data;
        }

        public async Task<(PermissionResponseDto data, Dictionary<string, object> referenceData)> GetRoleScreenMappingsByRoleIdAsync(string roleId)
        {
            var permissionResult = new PermissionResponseDto();
            // Validate roleId if necessary (e.g., ensure it's not null or empty)
            if (string.IsNullOrEmpty(roleId))
                throw new ArgumentException("Role_Id cannot be null or empty.");

            var screens = await _screenMasterRepository.GetAllAsync();
            permissionResult.ScreenPermission = screens.OrderBy(screen => screen.SequenceNo).ToList();

            Dictionary<string, object> referenceData = new();
            var options = new List<OptionModel<string, bool>>();
            var mappings = await _roleScreenMappingRepository.GetRoleScreenMappingAsync(roleId);
            if (mappings != null)
            {
                options = mappings.ScreenMappings.Select(x => new OptionModel<string, bool>(x.ScreenId, x.AccessAllowed)).ToList();
            }
            referenceData.Add("screensMapping", options);

            var roleScreenMappings = await _roleScreenMappingRepository.GetRoleScreenMappingAsync(roleId);
            var regionData = await _regionMasterRepository.GetAllAsync();

            var regionResult = new List<RegionPermissionResponse>();

            foreach (var region in regionData)
            {
                var accessAllowed = roleScreenMappings?.RegionPermissions?
                    .Any(p => p?.RegionIds != null && p.RegionIds.Contains(region.Id)) == true;

                regionResult.Add(new RegionPermissionResponse
                {
                    Id = region.Id,
                    RegionName = region.Name,
                    AccessAllowed = accessAllowed
                });
            }

            permissionResult.RegionPermission = regionResult;

            return (permissionResult, referenceData);
        }

        public async Task<string> AddUpdateRoleScreenMapping(List<RoleScreenMappingRequestDto> roleScreenMappings, string userId)
        {
            var roleId = roleScreenMappings.FirstOrDefault()?.roleId;
            if (string.IsNullOrEmpty(roleId)) return null;

            var mappingData = roleScreenMappings.Select(x => new ScreenMapping { ScreenId = x.Id, AccessAllowed = x.AccessAllowed });

            var updateCount = await _roleScreenMappingRepository.SaveRoleScreenMappingAsync(roleId, mappingData, userId);

            if (updateCount > 0)
            {
                await InsertRolePermissionHistory(roleId, "Update", mappingData, userId);
                return roleId;
            }

            var newRecordId = await InsertNewRoleScreenMapping(roleId, mappingData, userId);

            return newRecordId ?? roleScreenMappings.FirstOrDefault()?.Id;
        }

        // Extracted helper method for inserting new RoleScreenMapping
        private async Task<string> InsertNewRoleScreenMapping(string roleId, IEnumerable<ScreenMapping> mappingData, string userId)
        {
            var newRecord = new RoleScreenMapping
            {
                RoleId = roleId,
                ScreenMappings = mappingData,
                CreatedOn = DateTime.Now,
                CreatedBy = userId,
                UpdatedOn = DateTime.Now,
                UpdatedBy = userId,
            };

            var newRecordId = await _roleScreenMappingRepository.InsertAsync(newRecord);

            if (!string.IsNullOrEmpty(newRecordId))
            {
                await InsertRolePermissionHistory(roleId, "Insert", mappingData, userId);
            }

            return newRecordId;
        }

        // Extracted helper method for inserting RolePermissionHistory
        private async Task InsertRolePermissionHistory(string roleId, string action, IEnumerable<ScreenMapping> mappingData, string userId)
        {
            var historyRecord = new RolePermissionHistory
            {
                RoleId = roleId,
                Action = action,
                ScreenMappings = mappingData,
                CreatedOn = DateTime.Now,
                CreatedBy = userId,
                UpdatedOn = DateTime.Now,
                UpdatedBy = userId,
            };

            await _rolePermissionHistoryRepository.InsertAsync(historyRecord);
        }

        public async Task<string> SaveRegionScreenMapping(SaveRegionRequest roleScreenMappingsData, string userId)
        {
            if (roleScreenMappingsData is null)
                return null;

            if (string.IsNullOrWhiteSpace(roleScreenMappingsData.RoleId))
                return null;

            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var updateCount = await _roleScreenMappingRepository
                .UpdateRegionScreenMappingAsync(roleScreenMappingsData, userId);

            return updateCount > 0
                ? string.Empty
                : AppMessageConstants.NotFound;
        }

        public async Task<IEnumerable<CustomerRegionResDto>> GetPermissionRegionAsync(List<string> userRoles)
        {
            var projection = Builders<RegionMaster>.Projection
                .Include("_id")
                .Include("name");

            var regionData = await _regionMasterRepository.GetAllAsync(projection);

            var permittedRegionIds = new HashSet<string>();

            foreach (var roleName in userRoles)
            {
                var roleId = await _roleRepository.GetRoleIdByRoleName(roleName);
                if (string.IsNullOrEmpty(roleId))
                {
                    continue;
                }

                var roleScreenMappings = await _roleScreenMappingRepository
                    .GetRoleScreenMappingAsync(roleId);

                if (roleScreenMappings?.RegionPermissions == null)
                    continue;

                foreach (var permission in roleScreenMappings.RegionPermissions)
                {
                    if (!string.IsNullOrEmpty(permission.RegionIds))
                    {
                        permittedRegionIds.Add(permission.RegionIds);
                    }

                }
            }

            var result = regionData
                .Where(r => permittedRegionIds.Contains(r.Id))
                .Select(r => new CustomerRegionResDto
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToList();

            return result;
        }
    }
}
