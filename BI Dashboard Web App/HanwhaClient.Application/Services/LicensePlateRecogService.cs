using AutoMapper;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Common.ReferenceData;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.DeviceApiResponse;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services
{
    public class LicensePlateRecogService : ILicensePlateRecogService
    {
        private readonly IMapper _mapper;
        private readonly ILicensePlateRecogRepository _licensePlateRecogRepository;
        private readonly IVehicleOwnerRepository _vehicleOwnerRepository;
        private readonly IANPRVehicleRepository _aNPRVehicleRepository;
        private readonly IANPRVehicleService _aNPRVehicleService;
        private readonly IDeviceMasterService _deviceMasterService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IZoneCameraRepository _zoneCameraRepository;
        private readonly IUsersService _usersService;
        private readonly IDateConvert _dateConvert;
        private readonly IDeviceApiService _deviceApiService;
        private readonly IStringLocalizer<AppMessages> _localizer;

        public LicensePlateRecogService(IMapper mapper,
            ILicensePlateRecogRepository licensePlateRecogRepository,
            IVehicleOwnerRepository vehicleOwnerRepository,
            IANPRVehicleRepository aNPRVehicleRepository,
            IANPRVehicleService aNPRVehicleService,
            IDeviceMasterService deviceMasterService,
            IHttpContextAccessor httpContextAccessor,
            IUserNotificationService userNotificationService,
            IDeviceMasterRepository deviceMasterRepository,
            IZoneCameraRepository zoneCameraRepository,
            IUsersService usersService,
            IDateConvert dateConvert,
            IDeviceApiService deviceApiService,
            IStringLocalizer<AppMessages> localizer)

        {
            _mapper = mapper;
            _licensePlateRecogRepository = licensePlateRecogRepository;
            _vehicleOwnerRepository = vehicleOwnerRepository;
            _aNPRVehicleRepository = aNPRVehicleRepository;
            _aNPRVehicleService = aNPRVehicleService;
            _deviceMasterService = deviceMasterService;
            _httpContextAccessor = httpContextAccessor;
            _userNotificationService = userNotificationService;
            _deviceMasterRepository = deviceMasterRepository;
            _zoneCameraRepository = zoneCameraRepository;
            _usersService = usersService;
            _dateConvert = dateConvert;
            _deviceApiService = deviceApiService;
            _localizer = localizer;
        }

        public async Task<(PagedResult<AllLprDetailsResponse> Data, Dictionary<string, object> ReferenceData)> GetAllLprDetailsAsync(AllLprRequest request)
        {
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();
            IEnumerable<string> ownerIds = Enumerable.Empty<string>();
            if (request.DeviceIds != null && request.DeviceIds.Any())
            {
                deviceIds = request.DeviceIds;
            }
            else
            {
                if ((request.FloorIds != null && request.FloorIds.Any()) || (request.ZoneIds != null && request.ZoneIds.Any()))
                {
                    if (request.FloorIds != null && request.FloorIds.FirstOrDefault() == "000000000000000000000000")
                    {
                        var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget(false);
                        deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
                    }
                    else
                    {
                        deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneAsync(request.FloorIds, request.ZoneIds);
                    }
                }
            }
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                ownerIds = await _vehicleOwnerRepository.FindOwnerIdsByNameTypeEmailAsync(request.SearchText);
            }
            // Get data from repository
            var (data, totalCount) = await _licensePlateRecogRepository.GetAllLprDetailsAsync(deviceIds, ownerIds, request);

            // Extract unique IDs for reference data
            var vehicleOwnerIds = data
                .Where(x => !string.IsNullOrEmpty(x.VehicleOwnerId))
                .Select(x => x.VehicleOwnerId)
                .Distinct()
                .ToList();

            var entryGateIds = data
                .Where(x => !string.IsNullOrEmpty(x.EntryGate))
                .Select(x => x.EntryGate)
                .Distinct()
                .ToList();

            var exitGateIds = data
                .Where(x => !string.IsNullOrEmpty(x.ExitGate))
                .Select(x => x.ExitGate)
                .Distinct()
                .ToList();

            var allGateIds = entryGateIds.Concat(exitGateIds).Distinct().ToList();

            // Fetch owner names
            ProjectionDefinition<VehicleOwner> ownerProjection = Builders<VehicleOwner>.Projection
                .Include("ownerName")
                .Include("_id");

            var owners = vehicleOwnerIds.Any()
                ? await _vehicleOwnerRepository.GetManyAsync(vehicleOwnerIds, ownerProjection)
                : Enumerable.Empty<VehicleOwner>();
            var ownerDict = owners.ToDictionary(
                o => o.Id,
                o => o.OwnerName);

            // Fetch gate names
            ProjectionDefinition<DeviceMaster> deviceProjection = Builders<DeviceMaster>.Projection
                .Include("deviceName")
                .Include("_id");

            var devices = allGateIds.Any()
                ? await _deviceMasterRepository.GetManyAsync(allGateIds, deviceProjection)
                : Enumerable.Empty<DeviceMaster>();
            var deviceDict = devices.ToDictionary(
                d => d.Id,
                d => d.DeviceName);

            // Fetch all vehicle owner details for mapping (we need RegistrationType, Building, etc.)
            var allVehicleOwners = vehicleOwnerIds.Any()
                ? await _vehicleOwnerRepository.GetManyAsync(vehicleOwnerIds)
                : Enumerable.Empty<VehicleOwner>();
            var vehicleOwnerDict = allVehicleOwners.ToDictionary(
                o => o.Id,
                o => o);

            // Map to response
            var responseItems = new List<AllLprDetailsResponse>();
            foreach (var lpr in data)
            {
                var vehicleOwner = lpr.VehicleOwnerId != null && vehicleOwnerDict.ContainsKey(lpr.VehicleOwnerId)
                    ? vehicleOwnerDict[lpr.VehicleOwnerId]
                    : null;
                var imageName = lpr.DynamicFields?.GetValue("plateimage", "").AsString;
                responseItems.Add(new AllLprDetailsResponse
                {
                    Id = lpr.Id,
                    Plate = lpr.DynamicFields?.GetValue("plate", "").AsString ?? "",
                    Country = lpr.DynamicFields?.GetValue("country", "").AsString ?? "",
                    State = lpr.DynamicFields?.GetValue("state", "").AsString ?? "",
                    Make = lpr.DynamicFields?.GetValue("make", "").AsString,
                    Model = lpr.DynamicFields?.GetValue("model", "").AsString,
                    Color = lpr.DynamicFields?.GetValue("color", "").AsString,
                    RegistrationType = vehicleOwner?.RegistrationType ?? "",
                    OwnerName = vehicleOwner != null && ownerDict.ContainsKey(vehicleOwner.Id)
                        ? ownerDict[vehicleOwner.Id]
                        : "",
                    Building = vehicleOwner?.Building,
                    buildingUnit = vehicleOwner?.BuildingUnit,
                    Email = vehicleOwner?.Email,
                    Contact = vehicleOwner?.ContactNumber,
                    EntryGate = lpr.EntryGate != null && deviceDict.ContainsKey(lpr.EntryGate)
                        ? deviceDict[lpr.EntryGate]
                        : null,
                    ExitGate = lpr.ExitGate != null && deviceDict.ContainsKey(lpr.ExitGate)
                        ? deviceDict[lpr.ExitGate]
                        : null,
                    EntryTime = lpr.EntryTime,
                    ExitTime = lpr.ExitTime,
                    Message = lpr.Message,
                    CreatedOn = lpr.CreatedOn,
                    SmallImage = !string.IsNullOrEmpty(imageName)? imageName.Replace("#small", ""): null
                });
            }

            // Create reference data
            var referenceData = new Dictionary<string, object>();

            if (vehicleOwnerIds.Any() && owners.Any())
            {
                referenceData.Add("ownerIds", owners.Select(x => new OptionModel<string, string>(x.Id, x.OwnerName)).ToList());
            }

            if (allGateIds.Any() && devices.Any())
            {
                referenceData.Add("deviceIds", devices.Select(x => new OptionModel<string, string>(x.Id, x.DeviceName)).ToList());
            }

            var pagedResult = new PagedResult<AllLprDetailsResponse>
            {
                Items = responseItems,
                TotalCount = totalCount
            };

            return (pagedResult, referenceData);
        }

        public async Task<(string Id, string ErrorMessage)> SaveLicensePlateRecogDetailsAsync(JsonElement request, string userId)
        {
            DateTime currentUtcTime = DateTime.UtcNow;
            var timeZone = await _usersService.GetTimeZone(userId);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);

            var remoteIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "";
            var json = request.GetRawText();
            var doc = BsonDocument.Parse(json);

            var requestModel = new LicensePlateRecogRequest
            {
                CreatedAt = currentUtcTime,
                DynamicFields = doc
            };
            var lpr = _mapper.Map<LicensePlateRecogDetails>(requestModel);

            lpr.CreatedOn = currentUtcTime;
            lpr.CreatedBy = userId;
            lpr.DynamicFields = requestModel.DynamicFields;
            var image = lpr.DynamicFields.GetValue("image", "").AsString;
            var plateimage = lpr.DynamicFields.GetValue("plateimage", "").AsString;
            var images = new List<ANPRImageUpload>();
            var imageName = $"{lpr.DynamicFields.GetValue("plate", "").AsString}_{lpr.DynamicFields.GetValue("country", "").AsString}_{lpr.DynamicFields.GetValue("state", "").AsString}";
            if (!string.IsNullOrWhiteSpace(image))
            {
                images.Add(new ANPRImageUpload
                {
                    ImageName = $"{imageName}#large",
                    ImageBase64 = image
                });
            }
            if (!string.IsNullOrWhiteSpace(plateimage))
            {
                images.Add(new ANPRImageUpload
                {
                    ImageName = $"{imageName}#small",
                    ImageBase64 = plateimage
                });
            }
            await _aNPRVehicleService.UploadANPRImages(images);

            if (images.Count > 0)
            {
                lpr.DynamicFields["image"] = images.FirstOrDefault(x => x.ImageName.EndsWith("large"))?.ImageName + ".png";
                lpr.DynamicFields["plateimage"] = images.FirstOrDefault(x => x.ImageName.EndsWith("small"))?.ImageName + ".png";
            }                

            var country = lpr.DynamicFields.GetValue("country", "").AsString;
            var state = lpr.DynamicFields.GetValue("state", "").AsString;
            var plate = lpr.DynamicFields.GetValue("plate", "").AsString;

            var deviceDetails = await _deviceMasterService.GetDeviceDetailByIpAddressAsync(remoteIp);

            // If device details not found, log and return error
            if (deviceDetails == null)
            {
                var message = string.Format(_localizer[MessageKeys.DeviceNotRegisteredForIP], remoteIp);
                //var message = $"Device not registered for IP {remoteIp}";
                await _userNotificationService.AddUserNotification(_localizer[MessageKeys.DeviceNotRegistered], message, null, null);
                return await Task.FromResult((string.Empty, message));
            }

            // Determine if this is Entry or Exit based on CameraDirection
            var isEntry = deviceDetails != null && string.Equals(deviceDetails.CameraDirection, "Entry", StringComparison.OrdinalIgnoreCase);
            var isExit = deviceDetails != null && string.Equals(deviceDetails.CameraDirection, "Exit", StringComparison.OrdinalIgnoreCase);

            string LprId;
            LicensePlateRecogDetails recordToUse = lpr;

            if (isEntry)
            {
                // Entry gate - Always insert new record
                LprId = await _licensePlateRecogRepository.InsertAsync(lpr);
            }
            else if (isExit)
            {
                // Exit gate - First try to find existing "in" record
                var allInRecords = await _licensePlateRecogRepository.GetAllInRecordsByVehicleAsync(plate, country, state);
                var latestInRecord = allInRecords.OrderByDescending(x => x.EntryTime).FirstOrDefault();

                if (latestInRecord != null)
                {
                    // Found existing "in" record - use it, no need to insert new record
                    LprId = latestInRecord.Id;
                    recordToUse = latestInRecord;
                }
                else
                {
                    // Not found - insert new record
                    LprId = await _licensePlateRecogRepository.InsertAsync(lpr);
                }
            }
            else
            {
                // Neither Entry nor Exit - insert new record as fallback
                LprId = await _licensePlateRecogRepository.InsertAsync(lpr);
            }

            var ANPRvehicleDetails = await _aNPRVehicleService.GetVehicleByPlateNumAsync(plate, country, state);

            // Check if vehicle is found
            if (ANPRvehicleDetails == null)
            {
                //var message = "Unauthorized Vehicle";
                var message = string.Format(_localizer[MessageKeys.ANPRVehicleNotFound], country, state, plate, deviceDetails.DeviceName);
                //  var message = $"{country} {state} {plate} is on gate {deviceDetails.DeviceName}";
                recordToUse.Message = message;

                // Update EntryGate with current device id when vehicle is not found in system
                _ = isEntry ? recordToUse.EntryGate = deviceDetails?.Id : recordToUse.ExitGate = deviceDetails?.Id;

                await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                await _userNotificationService.AddUserNotification(_localizer[MessageKeys.UnauthorizedVehicle], message, null, null);
                return await Task.FromResult((LprId, message));
            }

            // Fetch vehicle owner by OwnerId
            var vehicleOwner = await _vehicleOwnerRepository.GetSingleOwnerByOwnerId(ANPRvehicleDetails.OwnerId);

            // Check if owner exists
            if (vehicleOwner == null)
            {
                var message = string.Format(_localizer[MessageKeys.VehicleOwnerNotFound], country, state, plate);
                //var message = $"Vehicle Owner not found for {country} {state} {plate}";
                recordToUse.Message = message;
                // Update EntryGate with current device id when vehicle owner is not found in system
                _ = isEntry ? recordToUse.EntryGate = deviceDetails?.Id : recordToUse.ExitGate = deviceDetails?.Id;
                await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                await _userNotificationService.AddUserNotification(_localizer[MessageKeys.VehicleOwnerNotFoundNotification], message, null, null);
                return await Task.FromResult((LprId, message));
            }
            if (vehicleOwner.OwnerValidTo != null && currentUtcTime >= vehicleOwner.OwnerValidTo.Value.AddDays(1).AddSeconds(-10))
            {
                var message = string.Format(
                    _localizer[MessageKeys.OwnerValidityExpired],
                    vehicleOwner.OwnerName,
                    vehicleOwner.OwnerValidTo.Value.ToLocalTime().ToString("dd-MMM-yyyy"));

                recordToUse.Message = message;

                _ = isEntry
                    ? recordToUse.EntryGate = deviceDetails?.Id
                    : recordToUse.ExitGate = deviceDetails?.Id;

                await _licensePlateRecogRepository.UpdateAsync(recordToUse);

                await _userNotificationService.AddUserNotification(
                    _localizer[MessageKeys.OwnerValidityExpiredTitle],
                    message,
                    null,
                    null);

                return (LprId, message);
            }

            // Check if deviceId is in AllowedGates           
            if (vehicleOwner != null && deviceDetails != null && !string.IsNullOrEmpty(deviceDetails.Id))
            {
                if (vehicleOwner.AllowedGates != null && vehicleOwner.AllowedGates.Any() && !vehicleOwner.AllowedGates.Contains(deviceDetails.Id))
                {

                    var message = string.Format(_localizer[MessageKeys.DeviceNotAllowed], country, state, plate, deviceDetails.DeviceName);
                    // var message = $"{country} {state} {plate} is not allowed on gate {deviceDetails.DeviceName}";
                    recordToUse.Message = message;
                    // Update EntryGate with current device id when vehicle is not allowed on this gate
                    _ = isEntry ? recordToUse.EntryGate = deviceDetails?.Id : recordToUse.ExitGate = deviceDetails?.Id;
                    
                    await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                    await _userNotificationService.AddUserNotification(_localizer[MessageKeys.GateAccessDenied], message, null, null);
                    return await Task.FromResult((LprId, message));
                }
            }

            if (isEntry)
            {
                // Entry logic - Check if owner type is permanent
                if (vehicleOwner != null && string.Equals(vehicleOwner.RegistrationType, "permanent", StringComparison.OrdinalIgnoreCase))
                {
                    // Check if current time falls within allowed time window
                    if (vehicleOwner.AllowedFromTime.HasValue && vehicleOwner.AllowedToTime.HasValue)
                    {
                        var currentTime = currentUtcTime.TimeOfDay;
                        var fromTime = vehicleOwner.AllowedFromTime.Value.TimeOfDay;
                        var toTime = vehicleOwner.AllowedToTime.Value.TimeOfDay;

                        bool isWithinTimeWindow = IsTimeWithinWindow(currentTime, fromTime, toTime);

                        if (!isWithinTimeWindow)
                        {
                            var _fromTime = vehicleOwner.AllowedFromTime.Value.ToLocalTime().TimeOfDay;
                            var _toTime = vehicleOwner.AllowedToTime.Value.ToLocalTime().TimeOfDay;

                            var message = string.Format(_localizer[MessageKeys.VehicleNotAllowedTime], country, state, plate, $"{_fromTime:hh\\:mm}", $"{_toTime:hh\\:mm}");
                            //  var message = $"Vehicle is not allowed at this time. Allowed time: {fromTime:hh\\:mm} - {toTime:hh\\:mm}";
                            recordToUse.Message = message;

                            // Update EntryGate with current device id when system restricts entry by time
                            recordToUse.EntryGate = deviceDetails?.Id;

                            await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                            await _userNotificationService.AddUserNotification(_localizer[MessageKeys.TimeRestriction], message, null, null);
                            return await Task.FromResult((LprId, message));
                        }
                    }

                    // Check how many vehicles are currently in
                    var vehiclesInCount = await _licensePlateRecogRepository.CountVehiclesInByOwnerIdAsync(vehicleOwner.Id);

                    // Check if vehicle count limit is reached
                    if (vehiclesInCount >= vehicleOwner.AllowedVehicle)
                    {
                        var message = string.Format(_localizer[MessageKeys.VehicleCounLimit], country, state, plate, vehicleOwner.AllowedVehicle);
                        //var message = $"{country} {state} {plate} cannot enter. Maximum allowed vehicles ({vehicleOwner.AllowedVehicle}) already in.";
                        recordToUse.Message = message;

                        // Update EntryGate with current device id when vehicle count limit is reached
                        recordToUse.EntryGate = deviceDetails?.Id;
                        await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                        await _userNotificationService.AddUserNotification(_localizer[MessageKeys.VehicleLimitReached], message, null, null);
                        return await Task.FromResult((LprId, message));
                    }
                }
                // Entry logic - Check if owner type is visitor
                else if (vehicleOwner != null && string.Equals(vehicleOwner.RegistrationType, "visitor", StringComparison.OrdinalIgnoreCase))
                {
                    // Check if current time falls within visitor valid time window
                    if (ANPRvehicleDetails.VisitorValidFrom.HasValue && ANPRvehicleDetails.VisitorValidTo.HasValue)
                    {
                        if (currentUtcTime < ANPRvehicleDetails.VisitorValidFrom.Value || currentUtcTime > ANPRvehicleDetails.VisitorValidTo.Value)
                        {
                            //var message = $"{country} {state} {plate} is not allowed to enter. Visitor valid period: {ANPRvehicleDetails.VisitorValidFrom.Value:yyyy-MM-dd HH:mm} - {ANPRvehicleDetails.VisitorValidTo.Value:yyyy-MM-dd HH:mm}";
                            var message = string.Format(_localizer[MessageKeys.VisitorValidPeriod], country, state, plate, $"{ANPRvehicleDetails.VisitorValidFrom.Value.ToLocalTime():yyyy-MM-dd HH:mm}", $"{ANPRvehicleDetails.VisitorValidTo.Value.ToLocalTime():yyyy-MM-dd HH:mm}");
                            recordToUse.Message = message;

                            // Update EntryGate with current device id when visitor is restricted by time
                            recordToUse.EntryGate = deviceDetails?.Id;
                            
                            await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                            await _userNotificationService.AddUserNotification(_localizer[MessageKeys.VisitorTimeRestriction], message, null, null);
                            return await Task.FromResult((LprId, message));
                        }
                    }
                }

                // Exit all old "in" records with same country, state, plate
                var oldInRecords = await _licensePlateRecogRepository.GetAllInRecordsByVehicleAsync(plate, country, state);
                foreach (var oldRecord in oldInRecords)
                {
                    // Skip the current record we just inserted
                    if (oldRecord.Id != LprId)
                    {
                        //SystemAutoExited = System auto-exited: same vehicle entered again without a recorded exit.
                        var message = _localizer[MessageKeys.SystemAutoExited];
                        oldRecord.IsIn = false;
                        oldRecord.ExitGate = deviceDetails?.Id;
                        oldRecord.ExitTime = currentUtcTime;
                        oldRecord.Message = message;
                        await _licensePlateRecogRepository.UpdateAsync(oldRecord);
                    }
                }

                // All checks passed - mark new vehicle as in
                var apiResponse = await _deviceApiService.CallDeviceApi<ResetDeviceCountResponse>((deviceDetails.IsHttps ? "https://" : "http://") + deviceDetails.IpAddress + SunapiAPIConstant.OpenAnprGateBarrier, deviceDetails.UserName, deviceDetails.Password);
                if (apiResponse.Response == "Success")
                {
                    //VehicleEnteredSuccessfully =Vehicle entered successfully
                    var message = _localizer[MessageKeys.VehicleEnteredSuccessfully];
                    recordToUse.IsIn = true;
                    recordToUse.AnprVehicleId = ANPRvehicleDetails.AnprVehicleId;
                    recordToUse.VehicleOwnerId = vehicleOwner?.Id;
                    recordToUse.EntryTime = currentUtcTime;
                    recordToUse.EntryGate = deviceDetails?.Id;
                    recordToUse.Message = message;
                    await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                }
                else
                {
                    //FailToOpenGateBarrierDesc = Fail to open gate barrier for {0}, {deviceDetails.DeviceName}.
                    var message = string.Format(_localizer[MessageKeys.FailToOpenGateBarrierDesc], deviceDetails.DeviceName);
                    recordToUse.Message = message;
                    recordToUse.EntryGate = deviceDetails?.Id;
                    await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                    await _userNotificationService.AddUserNotification(_localizer[MessageKeys.FailToOpenGateBarrierTitle], message, null, null); //FailToOpenGateBarrierTitle = Fail to Open Gate Barrier
                    return await Task.FromResult((LprId, message));
                }

            }
            else if (isExit)
            {
                // Exit logic - Check if owner type is permanent and validate time window
                if (vehicleOwner != null && string.Equals(vehicleOwner.RegistrationType, "permanent", StringComparison.OrdinalIgnoreCase))
                {
                    // Check if current time falls within allowed time window
                    if (vehicleOwner.AllowedFromTime.HasValue && vehicleOwner.AllowedToTime.HasValue)
                    {
                        var currentTime = currentUtcTime.TimeOfDay;
                        var fromTime = vehicleOwner.AllowedFromTime.Value.TimeOfDay;
                        var toTime = vehicleOwner.AllowedToTime.Value.TimeOfDay;

                        bool isWithinTimeWindow = IsTimeWithinWindow(currentTime, fromTime, toTime);

                        if (!isWithinTimeWindow)
                        {
                            var _fromTime = vehicleOwner.AllowedFromTime.Value.ToLocalTime().TimeOfDay;
                            var _toTime = vehicleOwner.AllowedToTime.Value.ToLocalTime().TimeOfDay;
                            //PermanentTimeRestrictionDesc = {0} {1} {2} is not allowed to exit at this time. Allowed time: {3} - {4} , country,state,plate,fromTime,toTime
                            var message = string.Format(_localizer[MessageKeys.PermanentTimeRestrictionDesc], country, state, plate, $"{_fromTime:hh\\:mm}", $"{_toTime:hh\\:mm}");
                            recordToUse.Message = message;

                            // Update ExitGate with current device id when system restricts exit by time
                            recordToUse.ExitGate = deviceDetails?.Id;

                            await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                            await _userNotificationService.AddUserNotification(_localizer[MessageKeys.PermanentTimeRestrictionTitle], message, null, null); //PermanentTimeRestrictionTitle = Time Restriction
                            return await Task.FromResult((LprId, message));
                        }
                    }
                }
                // Exit logic - Check if owner type is visitor and validate time window
                else if (vehicleOwner != null && string.Equals(vehicleOwner.RegistrationType, "visitor", StringComparison.OrdinalIgnoreCase))
                {
                    // Check if current time falls within visitor valid time window
                    if (ANPRvehicleDetails.VisitorValidFrom.HasValue && ANPRvehicleDetails.VisitorValidTo.HasValue)
                    {
                        if (currentUtcTime < ANPRvehicleDetails.VisitorValidFrom.Value || currentUtcTime > ANPRvehicleDetails.VisitorValidTo.Value)
                        {
                            //VisitorTimeRestrictionDesc = {0} {1} {2} is not allowed to exit. Visitor valid period: {3} - {4} , country, state, plate,ANPRvehicleDetails.VisitorValidFrom.Value:yyyy-MM-dd HH:mm,ANPRvehicleDetails.VisitorValidTo.Value:yyyy-MM-dd HH:mm
                            var message = string.Format(_localizer[MessageKeys.VisitorTimeRestrictionDesc], country, state, plate, $"{ANPRvehicleDetails.VisitorValidFrom.Value.ToLocalTime():yyyy-MM-dd HH:mm}", $"{ANPRvehicleDetails.VisitorValidTo.Value.ToLocalTime():yyyy-MM-dd HH:mm}");
                            recordToUse.Message = message;

                            // Update ExitGate with current device id when visitor exit is restricted by time
                            recordToUse.ExitGate = deviceDetails?.Id;

                            await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                            await _userNotificationService.AddUserNotification(_localizer[MessageKeys.VisitorTimeRestrictionTitle], message, null, null); ////VisitorTimeRestrictionTitle = Visitor Time Restriction
                            return await Task.FromResult((LprId, message));
                        }
                    }
                }

                // Mark the record as out (we already have the record from earlier)
                var apiResponse = await _deviceApiService.CallDeviceApi<ResetDeviceCountResponse>((deviceDetails.IsHttps ? "https://" : "http://") + deviceDetails.IpAddress + SunapiAPIConstant.OpenAnprGateBarrier, deviceDetails.UserName, deviceDetails.Password);
                if (apiResponse.Response == "Success")
                {
                    //VehicleExitedSuccssfully = Vehicle exited successfully
                    var message = _localizer[MessageKeys.VehicleExitedSuccssfully];
                    recordToUse.IsIn = false;
                    recordToUse.ExitTime = currentUtcTime;
                    recordToUse.ExitGate = deviceDetails?.Id;
                    recordToUse.Message = message;
                    if (vehicleOwner != null)
                    {
                        recordToUse.VehicleOwnerId = vehicleOwner.Id;
                    }
                    await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                }
                else
                {
                    //FailToOpenGateBarrierDesc = Fail to open gate barrier for {0}, {deviceDetails.DeviceName}.
                    var message = string.Format(_localizer[MessageKeys.FailToOpenGateBarrierDesc], deviceDetails.DeviceName);
                    recordToUse.Message = message;
                    recordToUse.ExitGate = deviceDetails?.Id;
                    await _licensePlateRecogRepository.UpdateAsync(recordToUse);
                    await _userNotificationService.AddUserNotification(_localizer[MessageKeys.FailToOpenGateBarrierTitle], message, null, null);
                    return await Task.FromResult((LprId, message));
                }

            }

            return await Task.FromResult((LprId, ""));

        }

        public async Task<StringBuilder> GetLPRDtailsCSVAsync(AllLprRequest lprRequest, string userId)
        {
            StringBuilder sb = new StringBuilder();
            lprRequest.PageNumber = 1;
            lprRequest.PageSize = 100000;

            var timeZone = await _usersService.GetTimeZone(userId);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);

            var LPRresponse = await GetAllLprDetailsAsync(lprRequest);

            if (LPRresponse.Data.TotalCount == 0)
            {
                return sb;
            }

            //sb.AppendLine("Owner Name,Registration Type,Building,Building Unit,Contact Number,Email,Entry Gate,Entry Time,Exit Gate,Exit Time,Country,State,Plate,Make,Model,Color,Message,Created On");
            sb.AppendLine("Plate,Country,State,Entry Gate,Entry Time,Exit Gate,Exit Time,Owner Name,Registration Type,Building,Building Unit,Contact Number,Email,Make,Model,Color,Message,Created On");

            var result = LPRresponse.Data;

            foreach (var item in result.Items)
            {
                var createdOnDate = FormatDate(item.CreatedOn, offsetTimeStamp);
                var entryTime = FormatDate(item.EntryTime, offsetTimeStamp);
                var exitTime = FormatDate(item.ExitTime, offsetTimeStamp);

                sb.AppendLine(string.Join(",",
                    EscapeCsv(item.Plate),
                    EscapeCsv(item.Country),
                    EscapeCsv(item.State),
                    EscapeCsv(item.EntryGate),
                    entryTime,
                    EscapeCsv(item.ExitGate),
                    exitTime,
                    EscapeCsv(item.OwnerName),
                    EscapeCsv(item.RegistrationType),
                    EscapeCsv(item.Building),
                    EscapeCsv(item.buildingUnit),
                    EscapeCsv(item.Contact),
                    EscapeCsv(item.Email),
                    EscapeCsv(item.Make),
                    EscapeCsv(item.Model),
                    EscapeCsv(item.Color),
                    EscapeCsv(item.Message),
                    createdOnDate

                ));
            }

            return sb;
        }

        /// <summary>
        /// Checks if the current time falls within the allowed time window.
        /// Handles both normal windows (e.g., 09:00 - 17:00) and windows that span midnight (e.g., 22:30 - 08:00).
        /// </summary>
        /// <param name="currentTime">The current time of day</param>
        /// <param name="fromTime">The start time of the allowed window</param>
        /// <param name="toTime">The end time of the allowed window</param>
        /// <returns>True if current time is within the window, false otherwise</returns>
        private static bool IsTimeWithinWindow(TimeSpan currentTime, TimeSpan fromTime, TimeSpan toTime)
        {
            if (fromTime <= toTime)
            {
                // Normal case: window doesn't span midnight (e.g., 09:00 - 17:00)
                // Current time must be >= fromTime AND <= toTime
                return currentTime >= fromTime && currentTime <= toTime;
            }
            else
            {
                // Window spans midnight (e.g., 22:30 - 08:00)
                // Current time must be >= fromTime (e.g., >= 22:30) OR <= toTime (e.g., <= 08:00)
                return currentTime >= fromTime || currentTime <= toTime;
            }
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }


        // Prevent Excel/WPS auto-format
        public static string EscapeText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            value = value.Replace("\"", "\"\"");

            return $"=\"{value}\"";
        }

        public static string FormatDate(DateTime? date, TimeSpan offset)
        {
            if (date == null)
                return "";

            string formatted = date.Value
                .Add(offset)
                .ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

            return EscapeText(formatted);
        }

    }
}
