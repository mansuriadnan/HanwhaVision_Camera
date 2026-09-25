using AutoMapper;
using HanwhaAdminApi.Application.Interfaces;
using HanwhaAdminApi.Core.Interfaces;
using HanwhaAdminApi.Infrastructure.Interfaces;
using HanwhaAdminApi.Infrastructure.Repository;
using HanwhaAdminApi.Model.Common;
using HanwhaAdminApi.Model.Common.ReferenceData;
using HanwhaAdminApi.Model.DbEntities;
using HanwhaAdminApi.Model.Dto;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaAdminApi.Application.Services
{
    public class RegionMasterService : IRegionMasterService
    {
        private readonly IRegionMasterRepository _regionMasterRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IMapper _mapper;
        private readonly IRoleService _roleService;
        private readonly IRoleScreenMappingRepository _roleScreenMappingRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICustomerMasterRepository _customerMasterRepository;

        public RegionMasterService(IRegionMasterRepository regionMasterRepository, IMapper mapper, IUsersRepository usersRepository, IRoleService roleService, IRoleScreenMappingRepository roleScreenMappingRepository, IRoleRepository roleRepository, ICustomerMasterRepository customerMasterRepository)
        {
            _regionMasterRepository = regionMasterRepository;
            _mapper = mapper;
            _usersRepository = usersRepository;
            _roleService = roleService;
            _roleScreenMappingRepository = roleScreenMappingRepository;
            _roleRepository = roleRepository;
            _customerMasterRepository = customerMasterRepository;
        }

        public async Task<(string Id, string ErrorMessage)> SaveRegionAsync(RequestRegionMasterDto regionMasterDto, string userId)
        {
            var isExist = await _regionMasterRepository.IsRegionNameExistAsync(regionMasterDto.Name, regionMasterDto.Id);
            if (isExist)
                return ("", $"Region {regionMasterDto.Name} already exist.");

            var roleResult = await _roleService.GetRolePermissionAsync();
            string roleId = "";
            if (roleResult != null)
            {
                var roleExits = roleResult.Where(x => x.Name.ToLower() == "super admin").FirstOrDefault();
                if (roleExits != null)
                {
                    roleId = roleExits.Id;
                }
            }

            if (string.IsNullOrEmpty(regionMasterDto.Id))
            {
                var regionId = await _regionMasterRepository.InsertAsync(new RegionMaster
                {
                    Id = null,
                    Name = regionMasterDto.Name,
                    CreatedOn = DateTime.Now,
                    CreatedBy = userId,
                    UpdatedOn = DateTime.Now,
                    UpdatedBy = userId,

                });

                if (!string.IsNullOrEmpty(roleId) && !string.IsNullOrEmpty(regionId))
                {

                    var filter = Builders<RoleScreenMapping>.Filter.Eq(r => r.RoleId, roleId);

                    var regionDataPermission = new RegionPermission
                    {
                        RegionIds = regionId,
                    };

                    var addfloorDataAccess = Builders<RoleScreenMapping>.Update.Push(c => c.RegionPermissions, regionDataPermission);
                    await _roleScreenMappingRepository.UpdateManyFieldsAsync(filter, addfloorDataAccess);
                }

                return await Task.FromResult((regionId, string.Empty));
            }
            else
            {
                var update = Builders<RegionMaster>.Update
                 .Set(c => c.Name, regionMasterDto.Name)
                 .Set(c => c.UpdatedOn, DateTime.Now)
                 .Set(c => c.UpdatedBy, userId);
                await _regionMasterRepository.UpdateFieldsAsync(regionMasterDto.Id, update);
                return await Task.FromResult((regionMasterDto.Id, string.Empty));
            }

        }

        public async Task<(IEnumerable<RegionMasterResponseDto> data, Dictionary<string, object> referenceData)> GetAllRegionAsync()
        {
            var regionData = await _regionMasterRepository.GetAllAsync();

            var data = _mapper.Map<IEnumerable<RegionMasterResponseDto>>(regionData);

            Dictionary<string, object> referenceData = new();

            if (regionData != null && regionData.Count() > 0)
            {
                var CreatedByIds = regionData.Select(x => x.CreatedBy).Distinct().ToList();
                var createByReferenceData = await GetUserMasterReferenceDataAsync(CreatedByIds);
                referenceData.Add("createdBy", createByReferenceData);

                var UpdatedByIds = regionData.Select(x => x.UpdatedBy).Distinct().ToList();
                var UpdateByReferenceData = await GetUserMasterReferenceDataAsync(UpdatedByIds);
                referenceData.Add("updatedBy", UpdateByReferenceData);
            }

            return await Task.FromResult((data, referenceData));
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> DeleteRegionAsync(string regionId,string userId)
        {
            // 1. Validate user
            var user = await _usersRepository.GetUserByUserIdAsync(userId);
            if (user == null)
                return (false, AppMessageConstants.NotFound);

            // 2. Check if region is assigned to any customer
            var customers = await _customerMasterRepository.GetCustomerByRegionId(regionId);
            var assignedCustomer = customers?.FirstOrDefault();

            if (assignedCustomer != null)
            {
                return (
                    false,
                    $"Region already assigned to {assignedCustomer.CustomerName} customer."
                );
            }

            // 3. Check if region exists
            var region = await _regionMasterRepository.GetAsync(regionId);
            if (region == null)
                return (false, AppMessageConstants.NotFound);

            // 4. Soft delete region
            await _regionMasterRepository.SoftDeleteAsync(regionId, userId);

            // 5. Remove region permissions from role mappings
            foreach (var roleId in user.RoleIds ?? Enumerable.Empty<string>())
            {
                await _roleScreenMappingRepository
                    .DeleteRegionPermissionByRoleIdAsync(roleId, regionId, userId);
            }

            return (true, string.Empty);
        }


        public async Task<List<OptionModel<string, string>>> GetUserMasterReferenceDataAsync(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<UserMaster> projection = Builders<UserMaster>.Projection
            .Include("Firstname")
            .Include("Lastname")
            .Include("_id");
            var users = await _usersRepository.GetManyAsync(ids, projection);
            options = users.Select(x => new OptionModel<string, string>(x.Id, x.Firstname + " " + x.Lastname)).ToList();
            return options;
        }

        public async Task<List<OptionModel<string, string>>> GetRegionNameReferenceDataAsync(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<RegionMaster> projection = Builders<RegionMaster>.Projection
            .Include("_id")
            .Include("name");
            var users = await _regionMasterRepository.GetManyAsync(ids, projection);
            options = users.Select(x => new OptionModel<string, string>(x.Id, x.Name)).ToList();
            return options;
        }
    }
}
