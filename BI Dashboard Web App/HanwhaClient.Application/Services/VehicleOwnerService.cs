using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;
using System;

namespace HanwhaClient.Application.Services
{
    public class VehicleOwnerService : IVehicleOwnerService
    {

        private readonly IMapper _mapper;
        private readonly IVehicleOwnerRepository _vehicleOwnerRepository;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IUsersRepository _userRepository;
        private readonly IANPRVehicleRepository _anprVehicleRepository;
        public VehicleOwnerService(IMapper mapper,
            IVehicleOwnerRepository vehicleOwnerRepository,
            IDeviceMasterRepository deviceMasterRepository,
            IUsersRepository userRepository,
            IANPRVehicleRepository anprVehicleRepository)
        {
            _mapper = mapper;
            this._vehicleOwnerRepository = vehicleOwnerRepository;
            this._deviceMasterRepository = deviceMasterRepository;
            this._userRepository = userRepository;
            this._anprVehicleRepository = anprVehicleRepository;
        }

        public async Task<(string Id, string ErrorMessage)> SaveVehicleOwnerAsync(VehicleOwnerRequest ownerRequest, string userId)
        {
            var vehicleOwner = _mapper.Map<VehicleOwner>(ownerRequest);
            if (ownerRequest.RegistrationType.ToLower() == "permanent")
            {
                var isExist = await _vehicleOwnerRepository.IsVehicleOwnerExistAsync(ownerRequest.Building, ownerRequest.BuildingUnit, ownerRequest.Id);
                if (isExist)
                    return ("", $"Owner {ownerRequest.Building}-{ownerRequest.BuildingUnit} already exist.");
            }
            if (ownerRequest.RegistrationType.ToLower() == "visitor" && ownerRequest.AllowedVehicle != 1)
            {
                return ("", $"A visitor is restricted from registering more than one vehicle at a time.");
            }

            if (string.IsNullOrEmpty(ownerRequest.Id))
            {
                vehicleOwner.CreatedOn = DateTime.UtcNow;
                vehicleOwner.UpdatedOn = DateTime.UtcNow;
                vehicleOwner.CreatedBy = userId;
                vehicleOwner.UpdatedBy = userId;
                var data = await _vehicleOwnerRepository.InsertAsync(vehicleOwner);
                return await Task.FromResult((data, ""));
            }
            else
            {
                var update = Builders<VehicleOwner>.Update
                .Set(c => c.RegistrationType, vehicleOwner.RegistrationType)
                .Set(c => c.OwnerName, vehicleOwner.OwnerName)
                .Set(c => c.Building, vehicleOwner.Building)
                .Set(c => c.BuildingUnit, vehicleOwner.BuildingUnit)
                .Set(c => c.Email, vehicleOwner.Email)
                .Set(c => c.ContactNumber, vehicleOwner.ContactNumber)
                .Set(c => c.AllowedVehicle, vehicleOwner.AllowedVehicle)
                .Set(c => c.OwnerValidTo, vehicleOwner.OwnerValidTo)
                .Set(c => c.AllowedFromTime, vehicleOwner.AllowedFromTime)
                .Set(c => c.AllowedToTime, vehicleOwner.AllowedToTime)
                .Set(c => c.AllowedGates, vehicleOwner.AllowedGates)
                .Set(c => c.EnabledAlarmForOverstay, vehicleOwner.EnabledAlarmForOverstay)
                .Set(c => c.EnabledAlarmFor24HStay, vehicleOwner.EnabledAlarmFor24HStay)
                .Set(c => c.UpdatedOn, DateTime.UtcNow)
                .Set(c => c.UpdatedBy, userId);
                await _vehicleOwnerRepository.UpdateFieldsAsync(vehicleOwner.Id, update);
                return await Task.FromResult((vehicleOwner.Id, ""));
            }
        }

        public async Task<AllVehicleOwnerListResponse> GetAllVehicleOwnerAsync(AllVehicleOwnerRequest request)
        {
            List<string> filterDeviceIds = new();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                filterDeviceIds = await _deviceMasterRepository.FindDeviceIdsByNameAsync(request.SearchText);

            }
            var owners = await _vehicleOwnerRepository.GetAllOwnerAsync(request, filterDeviceIds);

            var deviceIds = owners.ownerDetails
                .SelectMany(v => v.AllowedGates)
                .Distinct()
                .ToList();

            var userIds = owners.ownerDetails
                 .Select(o => o.CreatedBy)
                 .Concat(owners.ownerDetails.Select(o => o.UpdatedBy))
                 .Where(id => !string.IsNullOrEmpty(id))
                 .Distinct()
                 .ToList();

            ProjectionDefinition<DeviceMaster> projection = Builders<DeviceMaster>.Projection
            .Include("deviceName")
            .Include("_id");

            var Devices = await _deviceMasterRepository.GetManyAsync(deviceIds, projection);
            var deviceDict = Devices.ToDictionary(
                            o => o.Id,
                            o => o.DeviceName);

            ProjectionDefinition<UserMaster> projectionUser = Builders<UserMaster>.Projection
            .Include("Username")
            .Include("_id");

            var users = await _userRepository.GetManyAsync(userIds, projectionUser);
            var userDict = users.ToDictionary(
                            u => u.Id,
                            u => u.Username);
            AllVehicleOwnerListResponse vehicleOwner = new AllVehicleOwnerListResponse
            {
                allVehicleOwnerLists = owners.ownerDetails.Select(v => new AllVehicleOwnerList
                {
                    Id = v.Id,
                    RegistrationType = v.RegistrationType,
                    OwnerName = v.OwnerName,
                    Building = v.Building,
                    BuildingUnit = v.BuildingUnit,
                    Email = v.Email,
                    ContactNumber = v.ContactNumber,
                    OwnerValidTo = v.OwnerValidTo,
                    AllowedVehicle = v.AllowedVehicle,
                    AllowedFromTime = v.AllowedFromTime,
                    AllowedToTime = v.AllowedToTime,
                    AllowedGates = v.AllowedGates,
                    AllowedGateNames = v.AllowedGates
                           .Where(g => deviceDict.ContainsKey(g))
                           .Select(g => deviceDict[g])
                           .ToArray(),
                    EnabledAlarmForOverstay = v.EnabledAlarmForOverstay,
                    EnabledAlarmFor24HStay = v.EnabledAlarmFor24HStay
                }).ToList(),
                TotalCount = owners.totalCount
            };
            return vehicleOwner;
        }

        public async Task<bool> DeleteOwnerAsync(string id, string userId)
        {
            var vehicleIds = await _anprVehicleRepository.GetANPRVehicleIdsByOwner(id);
            if (vehicleIds.Any())
            {
                var vehicleData = await _anprVehicleRepository.SoftDeleteManyAsync(vehicleIds, userId);
                if(vehicleData < 0)
                {
                    return false;
                }
            }
            var data = await _vehicleOwnerRepository.SoftDeleteAsync(id, userId);
            return data;
        }
    }

}
