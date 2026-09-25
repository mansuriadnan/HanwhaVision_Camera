using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Bson;
using Quartz;

namespace HanwhaClient.Server.BackgroundTask
{
    public class AnprOverstayNotificationJob : IJob
    {
        private readonly ILicensePlateRecogRepository _licensePlateRecogRepository;
        private readonly IVehicleOwnerRepository _vehicleOwnerRepository;
        private readonly IANPRVehicleService _anprVehicleService;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IFileLogger _fileLogger;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        public AnprOverstayNotificationJob(
            ILicensePlateRecogRepository licensePlateRecogRepository,
            IVehicleOwnerRepository vehicleOwnerRepository,
            IANPRVehicleService anprVehicleService,
            IUserNotificationService userNotificationService,
            IFileLogger fileLogger,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            _licensePlateRecogRepository = licensePlateRecogRepository;
            _vehicleOwnerRepository = vehicleOwnerRepository;
            _anprVehicleService = anprVehicleService;
            _userNotificationService = userNotificationService;
            _fileLogger = fileLogger;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                DateTime currentUtcTime = DateTime.UtcNow;
                _fileLogger.Log("ANPR Overstay Notification job execution starts");
                Console.WriteLine($"ANPR Overstay Notification job executed at: {DateTime.Now}");

                // Step 1: Get vehicle owner IDs where overstay alarm is enabled (optimization)
                var applicableOwnerIds = await _vehicleOwnerRepository.GetVehicleOwnerIdsWithOverstayEnabledAsync();

                if (applicableOwnerIds == null || !applicableOwnerIds.Any())
                {
                    _fileLogger.Log("No vehicle owners with overstay alarm enabled");
                    Console.WriteLine($"ANPR Overstay Notification job completed: No owners with overstay enabled");
                    return;
                }

                // Step 2: Get only vehicles currently in that belong to owners with overstay enabled
                var vehiclesIn = await _licensePlateRecogRepository.GetVehiclesCurrentlyInByOwnerIdsAsync(applicableOwnerIds);

                if (vehiclesIn == null || !vehiclesIn.Any())
                {
                    _fileLogger.Log("No vehicles currently in the parking for owners with overstay enabled");
                    Console.WriteLine($"ANPR Overstay Notification job completed: No vehicles in parking");
                    return;
                }

                // Step 3: Get all applicable vehicle owners in one query (batch optimization)
                var vehicleOwners = await _vehicleOwnerRepository.GetManyAsync(applicableOwnerIds);
                var vehicleOwnerDict = vehicleOwners.ToDictionary(vo => vo.Id, vo => vo);

                // Process each vehicle individually
                var vehiclesToProcess = vehiclesIn
                    .Where(v => !string.IsNullOrEmpty(v.VehicleOwnerId) && v.EntryTime.HasValue)
                    .ToList();

                foreach (var vehicleEntry in vehiclesToProcess)
                {
                    try
                    {
                        var vehicleOwnerId = vehicleEntry.VehicleOwnerId;
                        
                        // Get vehicle owner from dictionary (already loaded in batch)
                        if (!vehicleOwnerDict.TryGetValue(vehicleOwnerId, out var vehicleOwner) || vehicleOwner == null)
                        {
                            continue;
                        }

                        bool isOverstay = false;
                        string overstayMessage = "";

                        // Check if permanent vehicle owner
                        if (vehicleOwner.RegistrationType?.ToLower() == "permanent")
                        {
                            if (vehicleOwner.AllowedFromTime.HasValue && vehicleOwner.AllowedToTime.HasValue)
                            {
                                var currentTime = currentUtcTime.TimeOfDay;
                                var allowedFrom = vehicleOwner.AllowedFromTime.Value.TimeOfDay;
                                var allowedTo = vehicleOwner.AllowedToTime.Value.TimeOfDay;
                                var entryTime = vehicleEntry.EntryTime.Value.TimeOfDay;

                                var plate = vehicleEntry.DynamicFields?.GetValue("plate", "").AsString ?? "Unknown";
                                var country = vehicleEntry.DynamicFields?.GetValue("country", "").AsString ?? "Unknown";
                                var state = vehicleEntry.DynamicFields?.GetValue("state", "").AsString ?? "Unknown";

                                // Check if current time is outside allowed time range
                                if (allowedFrom <= allowedTo)
                                {
                                    // Normal case: e.g., 09:00 - 17:00
                                    // Overstay if: vehicle entered during allowed time but current time is after allowed time
                                    // OR vehicle entered before allowed time and is still there
                                    // OR current time is before allowed time starts
                                    if (currentTime > allowedTo)
                                    {
                                        // Vehicle is still in after allowed time ended
                                        isOverstay = true;
                                        overstayMessage = $"{country} {state} {plate} (Owner: {vehicleOwner.OwnerName}) is overstaying. Allowed time: {allowedFrom:hh\\:mm} - {allowedTo:hh\\:mm}, Entry time: {entryTime:hh\\:mm}, Current time: {currentTime:hh\\:mm}";
                                    }
                                    else if (currentTime < allowedFrom)
                                    {
                                        // Vehicle entered before allowed time and is still there
                                        isOverstay = true;
                                        overstayMessage = $"{country} {state} {plate} (Owner: {vehicleOwner.OwnerName}) is overstaying. Allowed time: {allowedFrom:hh\\:mm} - {allowedTo:hh\\:mm}, Entry time: {entryTime:hh\\:mm}, Current time: {currentTime:hh\\:mm}";
                                    }
                                }
                                else
                                {
                                    // Overnight case: e.g., 22:00 - 06:00
                                    // Overstay if current time is between allowedTo and allowedFrom (outside the allowed window)
                                    if (currentTime > allowedTo && currentTime < allowedFrom)
                                    {
                                        isOverstay = true;
                                        overstayMessage = $"{country} {state} {plate} (Owner: {vehicleOwner.OwnerName}) is overstaying. Allowed time: {allowedFrom:hh\\:mm} - {allowedTo:hh\\:mm}, Entry time: {entryTime:hh\\:mm}, Current time: {currentTime:hh\\:mm}";
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Check if visitor vehicle owner
                            // Get plate information from the vehicle entry
                            var plate = vehicleEntry.DynamicFields?.GetValue("plate", "").AsString;
                            var country = vehicleEntry.DynamicFields?.GetValue("country", "").AsString;
                            var state = vehicleEntry.DynamicFields?.GetValue("state", "").AsString;

                            if (!string.IsNullOrEmpty(plate) && !string.IsNullOrEmpty(country) && !string.IsNullOrEmpty(state))
                            {
                                try
                                {
                                    var anprVehicle = await _anprVehicleService.GetVehicleByPlateNumAsync(plate, country, state);

                                    if (anprVehicle != null && anprVehicle.VisitorValidFrom.HasValue && anprVehicle.VisitorValidTo.HasValue)
                                    {
                                        // Check if current time is outside visitor valid period
                                        if (currentUtcTime < anprVehicle.VisitorValidFrom.Value || currentUtcTime > anprVehicle.VisitorValidTo.Value)
                                        {
                                            isOverstay = true;
                                            overstayMessage = $"{country} {state} {plate} (Owner: {vehicleOwner.OwnerName}) is overstaying. Visitor valid period: {anprVehicle.VisitorValidFrom.Value:yyyy-MM-dd HH:mm} - {anprVehicle.VisitorValidTo.Value:yyyy-MM-dd HH:mm}, Current time: {currentUtcTime:yyyy-MM-dd HH:mm}";
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, $"Error getting ANPR vehicle for plate {plate}, country {country}, state {state}");
                                    // Continue to next vehicle if we can't get vehicle details
                                    continue;
                                }
                            }
                        }

                        if (isOverstay && !string.IsNullOrEmpty(overstayMessage))
                        {
                            // Check if notification was already sent for this vehicle entry
                            if (!vehicleEntry.OverstayNotificationSent)
                            {
                                // Send notification only if not already sent
                                await _userNotificationService.AddUserNotification(
                                    "Vehicle Overstay Alert",
                                    overstayMessage,
                                    null,
                                    null
                                );
                                
                                // Mark notification as sent
                                vehicleEntry.OverstayNotificationSent = true;
                                
                                // Update the vehicle entry to persist the notification flag
                                await _licensePlateRecogRepository.UpdateAsync(vehicleEntry);
                                
                                _fileLogger.Log($"Overstay notification sent: {overstayMessage}");
                            }
                            else
                            {
                                // Notification already sent for this vehicle entry
                                _fileLogger.Log($"Overstay notification already sent for vehicle entry {vehicleEntry.Id}, skipping");
                            }
                        }
                        else if (!isOverstay)
                        {
                            // Vehicle is no longer overstaying, clear the notification flag
                            // This allows notification to be sent again if vehicle overstays in the future
                            if (vehicleEntry.OverstayNotificationSent)
                            {
                                vehicleEntry.OverstayNotificationSent = false;
                                await _licensePlateRecogRepository.UpdateAsync(vehicleEntry);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing vehicle entry {vehicleEntry.Id} for overstay notification");
                        // Continue to next vehicle if there's an error
                        continue;
                    }
                }

                Console.WriteLine($"ANPR Overstay Notification job completed at: {DateTime.Now}");
                _fileLogger.Log("ANPR Overstay Notification job execution ends");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                var exceptionLog2 = new ExceptionLog();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLogService>();
                    exceptionLog2.ExceptionMessage = ex.Message;
                    exceptionLog2.StackTrace = ex.StackTrace;
                    exceptionLog2.ExceptionType = ex.GetType().Name;
                    exceptionLog2.LoggedAt = DateTime.Now;
                    exceptionLog2.RequestPath = "ANPR Overstay Notification job";
                    exceptionLog2.ResponseTime = DateTime.Now;
                    exceptionLog2.IsSuccess = false;
                    await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                }
            }
        }
    }
}
