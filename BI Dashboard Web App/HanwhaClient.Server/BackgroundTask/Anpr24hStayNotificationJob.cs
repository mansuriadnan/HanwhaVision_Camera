using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using Quartz;

namespace HanwhaClient.Server.BackgroundTask
{
    public class Anpr24hStayNotificationJob : IJob
    {
        private readonly ILicensePlateRecogRepository _licensePlateRecogRepository;
        private readonly IVehicleOwnerRepository _vehicleOwnerRepository;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IFileLogger _fileLogger;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Anpr24hStayNotificationJob(
            ILicensePlateRecogRepository licensePlateRecogRepository,
            IVehicleOwnerRepository vehicleOwnerRepository,
            IUserNotificationService userNotificationService,
            IFileLogger fileLogger,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            _licensePlateRecogRepository = licensePlateRecogRepository;
            _vehicleOwnerRepository = vehicleOwnerRepository;
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
                // Calculate threshold time: 24 hours ago
                DateTime thresholdTime = currentUtcTime.AddHours(-24);
                
                _fileLogger.Log("ANPR 24H Stay Notification job execution starts");
                Console.WriteLine($"ANPR 24H Stay Notification job executed at: {DateTime.Now}");

                // Step 1: Get vehicle owner IDs where 24H stay alarm is enabled (optimization)
                var applicableOwnerIds = await _vehicleOwnerRepository.GetVehicleOwnerIdsWith24HStayEnabledAsync();

                if (applicableOwnerIds == null || !applicableOwnerIds.Any())
                {
                    _fileLogger.Log("No vehicle owners with 24H stay alarm enabled");
                    Console.WriteLine($"ANPR 24H Stay Notification job completed: No owners with 24H stay enabled");
                    return;
                }

                // Step 2: Get only vehicles that have been in for 24+ hours and notification hasn't been sent
                // This query is optimized to only fetch vehicles that need processing
                var vehiclesIn24H = await _licensePlateRecogRepository.GetVehiclesInFor24HByOwnerIdsAsync(applicableOwnerIds, thresholdTime);

                if (vehiclesIn24H == null || !vehiclesIn24H.Any())
                {
                    _fileLogger.Log("No vehicles have been in for 24+ hours for owners with 24H stay alarm enabled");
                    Console.WriteLine($"ANPR 24H Stay Notification job completed: No vehicles in for 24+ hours");
                    return;
                }

                // Step 3: Get all applicable vehicle owners in one query (batch optimization)
                var vehicleOwners = await _vehicleOwnerRepository.GetManyAsync(applicableOwnerIds);
                var vehicleOwnerDict = vehicleOwners.ToDictionary(vo => vo.Id, vo => vo);

                // Process each vehicle individually
                var vehiclesToProcess = vehiclesIn24H
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

                        // Double-check: Verify vehicle has been in for 24+ hours
                        if (!vehicleEntry.EntryTime.HasValue)
                        {
                            continue;
                        }

                        var timeInParking = currentUtcTime - vehicleEntry.EntryTime.Value;
                        
                        if (timeInParking.TotalHours >= 24)
                        {
                            // Check if notification was already sent for this vehicle entry
                            if (!vehicleEntry.Overstay24hNotificationSent)
                            {
                                var plate = vehicleEntry.DynamicFields?.GetValue("plate", "").AsString ?? "Unknown";
                                var country = vehicleEntry.DynamicFields?.GetValue("country", "").AsString ?? "Unknown";
                                var state = vehicleEntry.DynamicFields?.GetValue("state", "").AsString ?? "Unknown";
                                
                                var hoursInParking = Math.Floor(timeInParking.TotalHours);
                                var minutesInParking = Math.Floor((timeInParking.TotalHours - hoursInParking) * 60);
                                
                                var message = $"{country} {state} {plate} (Owner: {vehicleOwner.OwnerName}) has been in the parking for {hoursInParking} hours and {minutesInParking} minutes. Entry time: {vehicleEntry.EntryTime.Value:yyyy-MM-dd HH:mm}";
                                
                                // Send notification only if not already sent
                                await _userNotificationService.AddUserNotification(
                                    "Vehicle 24H Stay Alert",
                                    message,
                                    null,
                                    null
                                );
                                
                                // Mark notification as sent
                                vehicleEntry.Overstay24hNotificationSent = true;
                                
                                // Update the vehicle entry to persist the notification flag
                                await _licensePlateRecogRepository.UpdateAsync(vehicleEntry);
                                
                                _fileLogger.Log($"24H stay notification sent: {message}");
                            }
                            else
                            {
                                // Notification already sent for this vehicle entry
                                _fileLogger.Log($"24H stay notification already sent for vehicle entry {vehicleEntry.Id}, skipping");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing vehicle entry {vehicleEntry.Id} for 24H stay notification");
                        // Continue to next vehicle if there's an error
                        continue;
                    }
                }

                Console.WriteLine($"ANPR 24H Stay Notification job completed at: {DateTime.Now}");
                _fileLogger.Log("ANPR 24H Stay Notification job execution ends");
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
                    exceptionLog2.RequestPath = "ANPR 24H Stay Notification job";
                    exceptionLog2.ResponseTime = DateTime.Now;
                    exceptionLog2.IsSuccess = false;
                    await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                }
            }
        }
    }
}
