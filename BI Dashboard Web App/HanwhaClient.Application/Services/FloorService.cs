using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Utilities;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Auth;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace HanwhaClient.Application.Services
{
    public class FloorService : IFloorService
    {
        private readonly IFloorRepository _floorRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IZoneRepository _zoneRepository;
        private readonly IPermissionService _permissionService;
        private readonly IZoneCameraRepository _zoneCameraRepository;
        private readonly IZoneService _zoneService;
        private readonly IRoleService _roleService;
        private readonly IRoleScreenMappingRepository _roleScreenMappingRepository;
        private readonly ICacheService _cacheService;
        private readonly IViServerManagementRepository _serverManagementRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRoleRepository _roleRepository;
        private readonly ILinkedServerPermissionSyncService _linkedServerPermissionSyncService;

        public FloorService(IFloorRepository floorRepository, IUsersRepository usersRepository,
             IZoneRepository zoneRepository,
            IPermissionService permissionService,
            IZoneCameraRepository zoneCameraRepository,
            IZoneService zoneService,
            IRoleService roleService,
            IRoleScreenMappingRepository roleScreenMappingRepository,
            ICacheService cacheService, IViServerManagementRepository serverManagementRepository, ILinkedServerPermissionSyncService linkedServerPermissionSyncService)
        {
            _floorRepository = floorRepository;
            _usersRepository = usersRepository;
            _zoneRepository = zoneRepository;
            _permissionService = permissionService;
            _zoneCameraRepository = zoneCameraRepository;
            _zoneService = zoneService;
            _roleService = roleService;
            _roleScreenMappingRepository = roleScreenMappingRepository;
            _cacheService = cacheService;
            _serverManagementRepository = serverManagementRepository;
            _linkedServerPermissionSyncService = linkedServerPermissionSyncService;
        }

        public async Task<(string data, string errorMessage)> AddUpdateFloorAsync(AddFloorRequest floorRequest, string UserId)
        {
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
            var isDuplicateName = await _floorRepository.CheckFloorExistbyName(floorRequest.FloorPlanName, floorRequest.Id);
            if (isDuplicateName)
            {
                return ("", AppMessageConstants.DuplicateFloor);
            }
            if (string.IsNullOrEmpty(floorRequest.Id))
            {
                FloorPlanMaster floorPlanMaster = new FloorPlanMaster
                {
                    FloorPlanName = floorRequest.FloorPlanName,
                    CreatedBy = UserId,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = UserId,
                    UpdatedOn = DateTime.UtcNow
                };
                var floorId = await _floorRepository.InsertAsync(floorPlanMaster);
                // ⬇️ Add FloorId to RoleScreenMapping if not already present
                if (!string.IsNullOrEmpty(roleId) && !string.IsNullOrEmpty(floorId))
                {

                    var filter = Builders<RoleScreenMapping>.Filter.Eq(r => r.RoleId, roleId);

                    var floorDataAccessPermission = new FloorDataAccessPermission
                    {
                        FloorId = floorId,
                        ZoneIds = new List<string>()
                    };

                    var addfloorDataAccess = Builders<RoleScreenMapping>.Update.Push(c => c.DataAccessPermissions, floorDataAccessPermission);
                    await _roleScreenMappingRepository.UpdateManyFieldsAsync(filter, addfloorDataAccess);
                }
                await _cacheService.RemoveAsync(CacheConstants.Floors);
                return (floorId, "");
            }
            else
            {
                var update = Builders<FloorPlanMaster>.Update
                    .Set(c => c.FloorPlanName, floorRequest.FloorPlanName)
                    .Set(c => c.UpdatedBy, UserId)
                    .Set(c => c.UpdatedOn, DateTime.UtcNow);

                await _floorRepository.UpdateFieldsAsync(floorRequest.Id, update);
                await _cacheService.RemoveAsync(CacheConstants.Floors);
                return (floorRequest.Id, "");
            }
        }

        public async Task<IEnumerable<GetFloorDto>> GetAllFloorsAsync()
        {
            var floors = await _cacheService.GetAsync<IEnumerable<GetFloorDto>>(CacheConstants.Floors);
            if (floors != null)
            {
                return floors;
            }
            ProjectionDefinition<FloorPlanMaster> projection = Builders<FloorPlanMaster>.Projection
            .Include("floorPlanName")
            .Include("_id");
            var floorData = await _floorRepository.GetAllAsync(projection);
            var result = floorData.Select(item => new GetFloorDto
            {
                Id = item.Id,
                FloorPlanName = item.FloorPlanName
            });
            await _cacheService.SetAsync(CacheConstants.Floors, result);
            return result;
        }

        public async Task<bool> DeleteFloorAsync(string id, string userId)
        {
            var zones = await _zoneRepository.GetZonesByFloorIdAsync(id);
            if (zones != null && zones.Count() > 0)
            {
                foreach (var zone in zones)
                {
                    await _zoneService.DeleteZoneAsync(zone.Id, userId);
                }
            }
            var result = await _floorRepository.SoftDeleteAsync(id, userId);
            await _cacheService.RemoveAsync(CacheConstants.Floors);
            await _cacheService.RemoveAsync(CacheConstants.ClientSettings);
            return result;
        }

        public async Task<bool> UploadFloorPlanImageAsync(string floorId, IFormFile file, string userId)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            var base64Image = Convert.ToBase64String(fileBytes);
            var mimeType = file.ContentType;
            var base64ImageWithMime = $"data:{mimeType};base64,{base64Image}";
            var update = Builders<FloorPlanMaster>.Update
                .Set(c => c.FloorImage, base64ImageWithMime)
                .Set(c => c.UpdatedBy, userId)
                .Set(c => c.UpdatedOn, DateTime.UtcNow);
            var data = await _floorRepository.UpdateFieldsAsync(floorId, update);
            return data;
        }

        public async Task<string> GetFloorPlanImageAsync(string floorId)
        {
            var floorData = await _floorRepository.GetFromLinkedServerAsync(floorId);
            return floorData?.FloorImage;
        }

        public async Task<IEnumerable<FloorZoneByPermissionDto>> GetFloorZoneByPermissionAsync(IEnumerable<string> userRoles, IEnumerable<string> floorIds)
        {
            IEnumerable<FloorDataAccessPermission> floorZonePermissions = _permissionService.GetFloorZonePermissionByRoles(userRoles);

            var linkedServerFloorId = await GetLinkedFloorsZoneByPermissionAsync();

            if (linkedServerFloorId != null && linkedServerFloorId.Any())
            {
                floorZonePermissions = floorZonePermissions.Union(linkedServerFloorId);
            }

            IEnumerable<string> zoneIds = floorZonePermissions.Where(y => floorIds.Any(z => z == y.FloorId)).SelectMany(x => x.ZoneIds).Distinct();

            ProjectionDefinition<FloorPlanMaster> projection = Builders<FloorPlanMaster>.Projection
            .Include("floorPlanName")
            .Include("_id");
            var floorData = await _floorRepository.GetManyFromLinkedServerAsync(floorIds, projection);

            ProjectionDefinition<ZoneMaster> zoneProjection = Builders<ZoneMaster>.Projection
            .Include("zoneName")
            .Include("_id");
            var zoneData = await _zoneRepository.GetManyFromLinkedServerAsync(zoneIds, zoneProjection);

            var result = floorData.Select(item => new FloorZoneByPermissionDto
            {
                Id = item.Id,
                FloorPlanName = item.FloorPlanName,
                Zones = zoneData.Where(x => floorZonePermissions.Any(y => y.FloorId == item.Id && y.ZoneIds.Contains(x.Id))).Select(z => new ZoneByPermissionDto
                {
                    Id = z.Id,
                    ZoneName = z.ZoneName
                })
            });

            return result;
        }

        public async Task<IEnumerable<GetFloorDto>> GetFloorsByPermissionAsync(IEnumerable<string> userRoles, bool IncludeMultiServer)
        {
            IEnumerable<FloorDataAccessPermission> floorZonePermissions = _permissionService.GetFloorZonePermissionByRoles(userRoles);

            if (IncludeMultiServer)
            {
                var linkedServerFloorId = await GetLinkedFloorsZoneByPermissionAsync();

                if (linkedServerFloorId != null && linkedServerFloorId.Any())
                {
                    floorZonePermissions = floorZonePermissions.Union(linkedServerFloorId);
                }
                else
                {
                    var linkedServers = await _serverManagementRepository.GetAllAsync();

                    if (linkedServers != null && linkedServers.Count() > 0)
                    {
                        var tasks = linkedServers.Select(server => _linkedServerPermissionSyncService.FetchAndCacheServerAsync(server));
                        await Task.WhenAll(tasks);

                        await _cacheService.RemoveAsync(CacheConstants.ServerManagement);

                        var linkedServerFloorIdNew = await GetLinkedFloorsZoneByPermissionAsync();

                        if (linkedServerFloorIdNew != null && linkedServerFloorIdNew.Any())
                        {
                            floorZonePermissions = floorZonePermissions.Union(linkedServerFloorIdNew);
                        }
                    }
                }
            }

            IEnumerable<string> floorIds = floorZonePermissions.Select(x => x.FloorId).Distinct();

            ProjectionDefinition<FloorPlanMaster> projection = Builders<FloorPlanMaster>.Projection
            .Include("floorPlanName")
            .Include("_id");

            IEnumerable<FloorPlanMaster> floorData;

            if (IncludeMultiServer)
            {
                floorData = await _floorRepository.GetManyFromLinkedServerAsync(floorIds, projection);
            }
            else
            {
                floorData = await _floorRepository.GetManyAsync(floorIds, projection);
            }

            var result = floorData.Select(item => new GetFloorDto
            {
                Id = item.Id,
                FloorPlanName = item.FloorPlanName
            });

            result = result.Append(new GetFloorDto
            {
                Id = ObjectId.Parse("000000000000000000000000").ToString(),
                FloorPlanName = "Default Floor"
            });
            return result;
        }


        public async Task<IEnumerable<FloorDataAccessPermission>> GetLinkedFloorsZoneByPermissionAsync1()
        {
            var result = new List<FloorDataAccessPermission>();

            var linkedServerData = await _cacheService.GetAsync<IEnumerable<ViMultiServerManagement>>(CacheConstants.ServerManagement)
                                  ?? await _serverManagementRepository.GetAllAsync();

            if (linkedServerData != null && linkedServerData.Any())
            {
                foreach (var linkedServer in linkedServerData)
                {
                    CancellationToken cancellationToken = default;
                    var cacheKey = $"FloorZonePermissions_{linkedServer.Id}";

                    // ✅ Try cache per server
                    var cachedPermissions = await _cacheService
                        .GetAsync<IEnumerable<FloorDataAccessPermission>>(cacheKey);

                    if (cachedPermissions != null && cachedPermissions.Any())
                    {
                        result.AddRange(cachedPermissions);
                        continue;
                    }

                    using var httpClient = new HttpClient
                    {
                        BaseAddress = new Uri(linkedServer.HostingAddress)
                    };

                    var response = await AutheticationToken.GetFloorZoneLinkedServer(
                        httpClient,
                        linkedServer.Username,
                        linkedServer.Password,
                        cancellationToken
                    );

                    if (response?.Data == null)
                    {
                        // ❗ Skip instead of breaking everything
                        continue;
                    }

                    var permissions = response.Data.DataAccessPermissions?.ToList();

                    if (permissions != null && permissions.Any())
                    {
                        result.AddRange(permissions);

                        // ✅ Cache per server for 15 minutes
                        await _cacheService.SetAsync(
                            cacheKey,
                            permissions,
                            TimeSpan.FromMinutes(15)
                        );
                    }
                }
            }

            return result.Distinct();
        }

        /// <summary>
        /// Get the linked sever floor and zones ids
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<FloorDataAccessPermission>> GetLinkedFloorsZoneByPermissionAsync()
        {
            var result = new List<FloorDataAccessPermission>();

            var linkedServerData = await _cacheService.GetAsync<IEnumerable<ViMultiServerManagement>>(CacheConstants.ServerManagement)
                                   ?? await _serverManagementRepository.GetAllAsync();

            if (linkedServerData == null || !linkedServerData.Any())
                return result;

            foreach (var linkedServer in linkedServerData)
            {
                var cacheKey = $"FloorZonePermissions_{linkedServer.Id}";

                LinkedServerCacheEntry entry = await _cacheService.GetAsync<LinkedServerCacheEntry>(cacheKey);

                if (entry == null)
                {
                    continue;
                }

                if (!entry.IsActive)
                {
                    continue;
                }

                if (!entry.IsAvailable)
                {
                    continue;
                }

                if (entry.Permissions.Any())
                    result.AddRange(entry.Permissions);
            }

            return result.Distinct();
        }

        public async Task<GetFloorDto> GetFloorByFloorIdAsync(string floorId)
        {
            var floorEntity = await _floorRepository.GetAsync(floorId);

            if (floorEntity == null)
            {
                return null; // Or throw an exception depending on your error handling strategy
            }

            var responseDto = new GetFloorDto
            {
                Id = floorEntity.Id,
                FloorPlanName = floorEntity.FloorPlanName
            };

            return responseDto;
        }

        public async Task<IEnumerable<FloorZoneByPermissionDto>> GetFloorZonesAsync()
        {
            ProjectionDefinition<FloorPlanMaster> projection = Builders<FloorPlanMaster>.Projection
            .Include("floorPlanName")
            .Include("_id");
            var floors = await _floorRepository.GetAllAsync(projection);
            var zones = await _zoneRepository.GetZonesByMultipleFloorIdZoneIdAsync(floors.Select(x => x.Id).ToList(), null);
            var result = floors.Select(item => new FloorZoneByPermissionDto
            {
                Id = item.Id,
                FloorPlanName = item.FloorPlanName,
                Zones = zones.Where(z => z.FloorId == item.Id).Select(z => new ZoneByPermissionDto
                {
                    Id = z.Id,
                    ZoneName = z.ZoneName
                })
            });

            return result;


        }

        public async Task<FloorZonesNameByIdResponse> GetFloorZonesByIdsAsync(FloorZonesNameByIdRequest request)
        {
            var floors = await _floorRepository.GetManyAsync(request.FloorIds);
            var zones = await _zoneRepository.GetManyAsync(request.ZoneIds);
            var result = new FloorZonesNameByIdResponse
            {
                FloorNames = floors.Select(x => x.FloorPlanName),
                ZoneNames = zones.Select(x => x.ZoneName)

            };
            return result;

        }

        public async Task<IEnumerable<GetFloorDto>> GetFloorByServerIdsAsync(
     IEnumerable<string> userRoles,
     IEnumerable<string> serverIds)
        {
            IEnumerable<FloorPlanMaster> floorData = Enumerable.Empty<FloorPlanMaster>();

            ProjectionDefinition<FloorPlanMaster> projection =
                Builders<FloorPlanMaster>.Projection
                    .Include("floorPlanName")
                    .Include("_id");

            if (serverIds.Contains("000000000000000000000000") && serverIds.Count() == 1)
            {
                floorData = await _floorRepository.GetAllAsync(projection);
            }
            else
            {
                IEnumerable<FloorDataAccessPermission> floorZonePermissions =
                    _permissionService.GetFloorZonePermissionByRoles(userRoles);

                if (serverIds.Contains("000000000000000000000000"))
                {
                    floorData = await _floorRepository.GetAllAsync(projection);
                }
                else
                {
                    floorZonePermissions = Enumerable.Empty<FloorDataAccessPermission>();
                }

                var linkedServerFloorId =
                    await GetLinkedFloorsZoneByServerIdsAsync(serverIds);

                if (linkedServerFloorId != null && linkedServerFloorId.Any())
                {
                    floorZonePermissions = floorZonePermissions.Union(linkedServerFloorId);
                }

                var floorIds = floorZonePermissions
                    .Select(x => x.FloorId)
                    .Distinct();

                var floorDataNew =
                    await _floorRepository.GetManyFromLinkedServerAsync(floorIds, projection);

                // Add floorDataNew to floorData
                floorData = floorData
                            .Concat(floorDataNew)
                            .GroupBy(x => x.Id)
                            .Select(x => x.First());

            }

            var result = floorData.Select(item => new GetFloorDto
            {
                Id = item.Id,
                FloorPlanName = item.FloorPlanName
            }).Distinct();

            result = result.Append(new GetFloorDto
            {
                Id = ObjectId.Parse("000000000000000000000000").ToString(),
                FloorPlanName = "Default Floor"
            });
            return result;
        }

        public async Task<IEnumerable<FloorDataAccessPermission>> GetLinkedFloorsZoneByServerIdsAsync(IEnumerable<string> serverIds)
        {
            var result = new List<FloorDataAccessPermission>();

            if (serverIds == null || !serverIds.Any())
                return result;

            foreach (var serverId in serverIds)
            {
                var cacheKey = $"FloorZonePermissions_{serverId}";

                LinkedServerCacheEntry entry = await _cacheService.GetAsync<LinkedServerCacheEntry>(cacheKey);

                if (entry == null)
                {
                    continue;
                }

                if (!entry.IsActive)
                {
                    continue;
                }

                if (!entry.IsAvailable)
                {
                    continue;
                }

                if (entry.Permissions.Any())
                    result.AddRange(entry.Permissions);
            }

            return result.Distinct();
        }


    }
}