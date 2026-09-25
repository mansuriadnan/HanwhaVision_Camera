using AutoMapper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.Extensions.Localization;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services
{
    public class MaintanancePlanService : IMaintenancePlanService
    {
        private readonly IMaintenancePlanRepository _maintenancePlanRepository;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<AppMessages> _localizer;
        private readonly IFloorService _floorService;
        private readonly IZoneCameraRepository _zoneCameraRepository;
        private readonly IZoneService _zoneService;

        public MaintanancePlanService(IMaintenancePlanRepository repository, IDeviceMasterRepository deviceMasterRepository, IMapper mapper, IStringLocalizer<AppMessages> localizer, IFloorService floorService, IZoneCameraRepository zoneCameraRepository, IZoneService zoneService)
        {
            _maintenancePlanRepository = repository;
            _deviceMasterRepository = deviceMasterRepository;
            _mapper = mapper;
            _localizer = localizer;
            _floorService = floorService;
            _zoneCameraRepository = zoneCameraRepository;
            _zoneService = zoneService;
        }

        public async Task<PagedResult<MaintenancePlanDto>> GetMaintenancePlansAsync(MaintenancePlanSerachModel query)
        {
            //var plans = await _maintenancePlanRepository.GetAllAsync(); // Returns IQueryable
            List<string> filterDeviceIds = new();

            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                filterDeviceIds = await _deviceMasterRepository.FindDeviceIdsByNameAsync(query.SearchText);

            }

            var plansResult = await _maintenancePlanRepository.GetAllMaintenancePlan(query, filterDeviceIds);



            // 2) Gather all device ids present in the current page of plans
            filterDeviceIds = plansResult.Data
                .SelectMany(v => v.DeviceIds ?? Enumerable.Empty<ObjectId>())
                .Distinct()
                .Select(id => id.ToString())
                .ToList();

            // 3) Fetch device names for those ids
            ProjectionDefinition<DeviceMaster> projection = Builders<DeviceMaster>.Projection
                .Include(x => x.Id)
                .Include(x => x.DeviceName);

            var devices = await _deviceMasterRepository.GetManyAsync(filterDeviceIds, projection);

            // 4) Build a lookup: deviceId(string) -> deviceName
            var deviceNameById = devices
                .Where(d => d != null)
                .ToDictionary(d => d.Id.ToString(), d => d.DeviceName ?? string.Empty);

            // 5) Optional: filter by SearchText on device name
            var searchText = query?.SearchText?.Trim();
            IEnumerable<MaintenancePlan> plansToUse = plansResult.Data; // default

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // Find device ids whose names contain the searchText (case-insensitive)
                var matchingDeviceIds = devices
                    .Where(d => !string.IsNullOrWhiteSpace(d?.DeviceName) &&
                                d.DeviceName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.Id.ToString())
                    .ToHashSet();

                // Filter plans whose DeviceIds intersect with matching device ids
                var filteredPlans = plansResult.Data
                    .Where(p => p.DeviceIds != null &&
                                p.DeviceIds.Any(id => matchingDeviceIds.Contains(id.ToString())))
                    .ToList();

                // If we found any, prefer them; otherwise keep original plans
                if (filteredPlans.Count > 0)
                {
                    plansToUse = filteredPlans;
                }
            }

            // 6) Map to DTOs with device names populated
            var planDtos = plansToUse.Select(plan =>
            {
                var dto = _mapper.Map<MaintenancePlanDto>(plan);

                dto.DeviceNames = plan.DeviceIds?
                    .Select(id =>
                        deviceNameById.TryGetValue(id.ToString(), out var name) ? name : string.Empty)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList()
                    ?? new List<string>();

                return dto;
            }).ToList();

            // 7) Return paged result
            return new PagedResult<MaintenancePlanDto>
            {
                Items = planDtos,
                // If you want TotalCount to reflect filtered results when SearchText matched,
                // you could set it to planDtos.Count in that case. Otherwise keep original:
                TotalCount = plansResult.TotalCount// or: plansResult.TotalCount
            };
        }

        public async Task<(bool result, string errorMessage)> SaveMaintenancePlanAsync(MaintenancePlanDto planDto, string userId)
        {
            // Convert DTO to Model
            var plan = new MaintenancePlan
            {
                PlanName = planDto.PlanName,
                Duration = planDto.Duration,
                StartDate = planDto.StartDate,
                EndDate = planDto.EndDate,
                DeviceIds = planDto.DeviceIds?
                    .Where(id => ObjectId.TryParse(id, out _))
                    .Select(id => ObjectId.Parse(id))
                    .ToList() ?? new List<ObjectId>(),

                FloorIds = planDto.FloorIds?
                    .Where(id => ObjectId.TryParse(id, out _))
                    .Select(id => ObjectId.Parse(id))
                    .ToList() ?? new List<ObjectId>(),


                ZoneIds = planDto.ZoneIds?
                    .Where(id => ObjectId.TryParse(id, out _))
                    .Select(id => ObjectId.Parse(id))
                    .ToList() ?? new List<ObjectId>(),

                NextExecutionDate = planDto.NextExecutionDate,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = userId,
            };

            if (await _maintenancePlanRepository.IsMaintenancePlanExistsAsync(planDto.PlanName, planDto.Id))
            {
                return (false, _localizer[MessageKeys.MaintenancePlanExists]);
            }

            // Determine if this is Create or Update
            if (string.IsNullOrEmpty(planDto.Id))
            {
                // CREATE
                var maintenancePlanId = await _maintenancePlanRepository.InsertAsync(plan);
                return (!string.IsNullOrEmpty(maintenancePlanId) ? true : false, "");
            }
            else
            {
                // UPDATE
                var exists = await _maintenancePlanRepository.GetAsync(planDto.Id);
                if (exists == null)
                {
                    throw new KeyNotFoundException($"Maintenance plan with ID {planDto.Id} not found");
                }

                var deviceObjectIds = (planDto.DeviceIds ?? new List<string>())
                    .Select(s => ObjectId.TryParse(s, out var oid) ? oid : (ObjectId?)null)
                    .Where(oid => oid.HasValue)
                    .Select(oid => oid.Value)
                    .Distinct() // optional: avoid duplicates
                    .ToList();

                 var floorObjectIds = (planDto.FloorIds ?? new List<string>())
                    .Select(s => ObjectId.TryParse(s, out var oid) ? oid : (ObjectId?)null)
                    .Where(oid => oid.HasValue)
                    .Select(oid => oid.Value)
                    .Distinct() // optional: avoid duplicates
                    .ToList();

                  var ZoneObjectIds = (planDto.FloorIds ?? new List<string>())
                    .Select(s => ObjectId.TryParse(s, out var oid) ? oid : (ObjectId?)null)
                    .Where(oid => oid.HasValue)
                    .Select(oid => oid.Value)
                    .Distinct() // optional: avoid duplicates
                    .ToList();

                var isCompare = exists.StartDate.Date == planDto.StartDate.Date;


                // Build base update
                var updates = new List<UpdateDefinition<MaintenancePlan>>
                            {
                                Builders<MaintenancePlan>.Update.Set(c => c.PlanName, planDto.PlanName),
                                Builders<MaintenancePlan>.Update.Set(c => c.Duration, planDto.Duration),
                                Builders<MaintenancePlan>.Update.Set(c => c.EndDate, planDto.EndDate),
                                Builders<MaintenancePlan>.Update.Set(c => c.StartDate, planDto.StartDate),
                                Builders<MaintenancePlan>.Update.Set(c => c.UpdatedOn, DateTime.UtcNow),
                                Builders<MaintenancePlan>.Update.Set(c => c.DeviceIds, deviceObjectIds),
                                Builders<MaintenancePlan>.Update.Set(c => c.FloorIds, floorObjectIds),
                                Builders<MaintenancePlan>.Update.Set(c => c.ZoneIds, ZoneObjectIds),
                                Builders<MaintenancePlan>.Update.Set(c => c.UpdatedBy, userId),
                            };

                // Only update NextExecutionDate when StartDate changed
                if (!isCompare)
                {
                    // Option A: set to null
                    updates.Add(Builders<MaintenancePlan>.Update.Set(c => c.NextExecutionDate, (DateTime?)null));
                }

                var updateMaintanance = Builders<MaintenancePlan>.Update.Combine(updates);

                var result = await _maintenancePlanRepository.UpdateFieldsAsync(planDto.Id, updateMaintanance);
                return (result, "");
            }

            // Convert Model back to DTO
            //return MapToDto(savedPlan);
        }

        public async Task<bool> DeleteMaintenanceAsync(string id, string userId)
        {
            var data = await _maintenancePlanRepository.GetAsync(id);
            if (data != null)
            {
                var dataResult = await _maintenancePlanRepository.SoftDeleteAsync(id, userId);
                return dataResult;
            }
            return false;
        }

        public async Task<List<DeviceListByFloorZoneDto>> GetDeviceListByFloorZoneAsync()
        {
            var result = new List<DeviceListByFloorZoneDto>();

            IEnumerable<string> unMappeddeviceIds = Enumerable.Empty<string>();

            var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget(false);
            unMappeddeviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();


            // Get device details
            var unMappedDeviceData = unMappeddeviceIds.Any()
                ? await _deviceMasterRepository.GetAllDeviceByDeviceIds(unMappeddeviceIds)
                : new List<DeviceMaster>();


            foreach (var unMappedDevice in unMappedDeviceData)
            {

                result.Add(new DeviceListByFloorZoneDto
                {
                    DeviceId = unMappedDevice.Id,
                    DeviceName = unMappedDevice.DeviceName ?? "",
                    FloorId = "000000000000000000000000",
                    ZoneId = "000000000000000000000000"
                });

            }

            var floorData = await _floorService.GetAllFloorsAsync();
            if (floorData == null || !floorData.Any())
                return result;

            foreach (var floor in floorData)
            {
                var floorId = floor.Id.ToString();

                // 1️⃣ Get zones by floor
                var zones = await _zoneService.GetZoneByFloorId(floorId);
                if (zones == null || !zones.Any())
                    continue;

                var zoneIds = zones
                    .Select(z => z.ZoneId.ToString())
                    .Distinct()
                    .ToList();

                // 2️⃣ Get deviceId + zoneId mapping
                var deviceZoneId = await _zoneCameraRepository
                    .GetDeviceIdZoneIdbyFloorAndZoneAsync(
                        new List<string> { floorId },
                        zoneIds);

                deviceZoneId ??= new List<DeviceResDto>();
                IEnumerable<string> deviceIds = Enumerable.Empty<string>();

                deviceIds = deviceZoneId
                    .Where(x => !string.IsNullOrEmpty(x.DeviceId))
                    .Select(x => x.DeviceId!)
                    .Distinct()
                    .ToList();

                // Get device details
                var deviceData = deviceIds.Any()
                    ? await _deviceMasterRepository.GetAllDeviceByDeviceIds(deviceIds)
                    : new List<DeviceMaster>();

                //  Add devices with correct ZoneId
                foreach (var dz in deviceZoneId)
                {
                    var device = deviceData.FirstOrDefault(d => d.Id == dz.DeviceId);

                    if (result.Any(x =>
                            x.DeviceId == dz.DeviceId &&
                            x.ZoneId == dz.ZoneId &&
                            x.FloorId == floorId))
                        continue;

                    result.Add(new DeviceListByFloorZoneDto
                    {
                        DeviceId = dz.DeviceId,
                        DeviceName = device?.DeviceName ?? "",
                        FloorId = floorId,
                        ZoneId = dz.ZoneId
                    });
                }

            }
           
            return result;
        }

    }
}
