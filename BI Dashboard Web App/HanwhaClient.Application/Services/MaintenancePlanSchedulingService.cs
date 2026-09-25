using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.DbEntities;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services
{
    public class MaintenancePlanSchedulingService : IMaintenancePlanSchedulingService
    {
        private readonly IMaintenancePlanRepository _maintenancePlanRepository;
        private readonly IMaintenanceScheduleRepository _scheduleRepository;
        private readonly ILogger<MaintenancePlanSchedulingService> _logger;

        public MaintenancePlanSchedulingService(
            IMaintenancePlanRepository planRepository,
            IMaintenanceScheduleRepository scheduleRepository,
            ILogger<MaintenancePlanSchedulingService> logger)
        {
            _maintenancePlanRepository = planRepository;
            _scheduleRepository = scheduleRepository;
            _logger = logger;
        }

        public async Task ProcessMaintenancePlansAsync()
       {
            try
            {
                _logger.LogInformation("Starting maintenance plan processing at utc {Time}", DateTime.UtcNow);

                var plans = await _maintenancePlanRepository.GetMaintenancePlanData();
                // Adnan do not process plan which is expired, filter in DB make sure you check utc/local time

                var schedulesToInsert = new List<MaintenanceSchedule>();

                foreach (var plan in plans)
                {
                    try
                    {
                        await ProcessSinglePlanAsync(plan, schedulesToInsert);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing plan {PlanId}", plan.Id);
                    }
                }

                // Use InsertManyAsync for bulk insert of all schedules
                if (schedulesToInsert.Count > 0)
                {
                    await _scheduleRepository.InsertManyAsync(schedulesToInsert);
                    _logger.LogInformation("Inserted {Count} new maintenance schedules", schedulesToInsert.Count);
                }

                _logger.LogInformation("Completed maintenance plan processing at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessMaintenancePlansAsync");
                throw;
            }
        }


        private async Task ProcessSinglePlanAsync(
            MaintenancePlan plan,
            List<MaintenanceSchedule> schedulesToInsert)
        {
            // Calculate StartDate + Duration (in months) => candidate due date for the next execution
            DateTime calculatedDate; //= plan.StartDate.AddMonths(plan.Duration);

            // Choose the window start and end based on NextExecutionDate's presence
            DateTime windowStartUtcDate = DateTime.UtcNow.Date;
            DateTime windowEndUtcDate = windowStartUtcDate.AddMonths(1);

            if (plan.NextExecutionDate == null)
            {
                // Case 1: First run — compare against today .. next 30 days
                //windowStartUtcDate = DateTime.UtcNow.Date;
                //windowEndUtcDate = windowStartUtcDate.AddMonths(1);
                calculatedDate = plan.StartDate.AddMonths(plan.Duration);
            }
            else
            {
                // Case 2: Subsequent runs — compare against NextExecutionDate.Date .. NextExecutionDate.Date + 30 days
                calculatedDate = plan.NextExecutionDate.Value;
                //windowStartUtcDate = plan.NextExecutionDate.Value.Date;
                //windowEndUtcDate = windowStartUtcDate.AddMonths(1);
            }

            // Check if calculatedDate falls within the inclusive window
            if (calculatedDate.Date >= windowStartUtcDate && calculatedDate.Date <= windowEndUtcDate)
            {
                // ✅ Update nextExecutionDate to the calculatedDate (audit fields optional)
                var updatePlan = Builders<MaintenancePlan>.Update
                    .Set(p => p.NextExecutionDate, calculatedDate.AddMonths(plan.Duration))
                    .Set(p => p.UpdatedOn, DateTime.UtcNow); // optional, if part of BaseModel

                await _maintenancePlanRepository.UpdateFieldsAsync(plan.Id, updatePlan);

                // Create schedules for each device in the plan
                if (plan.DeviceIds != null && plan.DeviceIds.Count > 0)
                {
                    foreach (var deviceId in plan.DeviceIds)
                    {
                        var newSchedule = new MaintenanceSchedule
                        {
                            DeviceId = deviceId.ToString(),
                            MaintenancePlanId = plan.Id,
                            StatusHistory = new List<StatusHistoryItem>
                    {
                        new StatusHistoryItem
                        {
                            Status = "Not Started",
                            Notes = "Automatically scheduled by system",
                            StatusDatetime = DateTime.UtcNow
                        }
                    },
                            LatestStatus = "Not Started",
                            DueDate = calculatedDate,   // schedule due on the calculatedDate
                            IsScheduleManually = false
                        };

                        schedulesToInsert.Add(newSchedule);

                        _logger.LogInformation(
                            "Prepared schedule for plan {PlanId}, device {DeviceId}. Window [{Start:d}..{End:d}], due {Due:d}",
                            plan.Id, deviceId, windowStartUtcDate, windowEndUtcDate, calculatedDate.Date);
                    }
                }
            }
            else
            {
                _logger.LogInformation(
                    "Skipped plan {PlanId}: calculatedDate {CalculatedDate:d} not within window [{Start:d}..{End:d}]",
                    plan.Id, calculatedDate.Date, windowStartUtcDate, windowEndUtcDate);
            }
        }


        private void ProcessSinglePlan(MaintenancePlan plan, List<MaintenanceSchedule> schedulesToInsert)
        {
            // Define the window: today (inclusive) to next 30 days (inclusive)
            // Use DateTime.Today if your DueDate is stored in local time; otherwise use UtcNow.Date consistently.
            var today = DateTime.Today;
            var next30Days = today.AddDays(30);

            // Calculate StartDate + Duration (in months)
            var calculatedDate = plan.StartDate.AddMonths(plan.Duration);

            // Compare only the DATE portion, inclusive
            if (calculatedDate.Date >= today && calculatedDate.Date <= next30Days)
            {
                // Create schedule for each device in the plan
                if (plan.DeviceIds != null && plan.DeviceIds.Count > 0)
                {
                    foreach (var deviceId in plan.DeviceIds)
                    {
                        var newSchedule = new MaintenanceSchedule
                        {
                            DeviceId = deviceId.ToString(),
                            MaintenancePlanId = plan.Id,
                            StatusHistory = new List<StatusHistoryItem>
                    {
                        new StatusHistoryItem
                        {
                            Status = "Not Started",
                            Notes = "Automatically scheduled by system",
                            StatusDatetime = DateTime.UtcNow
                        }
                    },
                            LatestStatus = "Not Started",
                            DueDate = calculatedDate,      // this is the scheduled due date
                            IsScheduleManually = false
                        };

                        schedulesToInsert.Add(newSchedule);

                        _logger.LogInformation(
                            "Prepared new schedule for plan {PlanId}, device {DeviceId} with due date {DueDate}",
                            plan.Id, deviceId, calculatedDate);
                    }
                    // ✅ Update nextExecutionDate on the plan
                    var updatePlan = Builders<MaintenancePlan>.Update
                        .Set(p => p.NextExecutionDate, calculatedDate)
                        .Set(p => p.UpdatedOn, DateTime.UtcNow); // optional, if present on BaseModel

                    _maintenancePlanRepository.UpdateFieldsAsync(plan.Id, updatePlan);
                }


            }
            else
            {
                _logger.LogInformation(
                    "Skipped plan {PlanId}: calculatedDate {CalculatedDate:d} is outside the window {Start:d}..{End:d}",
                    plan.Id, calculatedDate.Date, today, next30Days);
            }
        }

    }
}
