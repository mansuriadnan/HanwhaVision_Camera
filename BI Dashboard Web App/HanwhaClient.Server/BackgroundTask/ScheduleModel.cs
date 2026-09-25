using HanwhaClient.Server.BackgroundTask;

namespace HanwhaClient.BackgroundTask
{
    public class ScheduledJob
    {
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public string CronSchedule { get; set; }
        public int Priority { get; set; } = 5;
        public Type ScheduleType { get; set; }
    }

    public static class JobScheduler
    {
        public static List<ScheduledJob> GetScheduledJobs()
        {
            return new List<ScheduledJob>
            {
                new ScheduledJob
                {
                    JobName = "ExampleJob1",
                    JobGroup = "GroupName",
                    CronSchedule = "0/10 * * * * ?", // every 15 seconds
                    ScheduleType = typeof(ExampleJob)
                },
                new ScheduledJob
                {
                    JobName = "ExampleJob2",
                    JobGroup = "GroupName",
                    CronSchedule = "0/15 * * * * ?", // every 30 seconds
                    ScheduleType = typeof(ExampleJob)
                },
                new ScheduledJob
                {
                    JobName = "ExampleJob22",
                    JobGroup = "GroupName22",
                    CronSchedule = "0 2 18 * * ? *", // specific time, every day 06:02 PM
                    ScheduleType = typeof(ExampleJob2)
                },
                new ScheduledJob
                {
                    JobName = "PeopleCountJob",
                    JobGroup = "DeviceGroup",
                    CronSchedule = "0 0/2 * * * ?", // every 15 seconds
                    ScheduleType = typeof(PeopleCountJob)
                },
                new ScheduledJob
                {
                    JobName = "VehicleCountJob",
                    JobGroup = "VehicleGroup",
                    CronSchedule = "0 0/2 * * * ?", // every 15 seconds
                    ScheduleType = typeof(VehicleCountJob)
                },
                new ScheduledJob
                {
                    JobName = "DeviceStatus",
                    JobGroup = "DeviceStatus",
                    CronSchedule = "0 0/2 * * * ?", // every 15 seconds
                    ScheduleType = typeof(CheckDeviceStatusJob)
                },
                new ScheduledJob
                {
                    JobName = "ShoppingCartCount",
                    JobGroup = "ShoppingCartCount",
                    CronSchedule = "0 0/2 * * * ?", // every 15 seconds
                    ScheduleType = typeof(ShoppingCartCountJob)
                },
                new ScheduledJob
                {
                    JobName = "ForkliftCount",
                    JobGroup = "ForkliftCount",
                    CronSchedule = "0 0/2 * * * ?", // every 15 seconds
                    ScheduleType = typeof(ForkliftCountJob)
                },
                new ScheduledJob
                {
                    JobName = "MultiLaneVehicleCount",
                    JobGroup = "MultiLaneVehicleCount",
                    CronSchedule = "0 0/2 * * * ?", // every 15 seconds
                    ScheduleType = typeof(MultiLaneVehicleCountJob)
                },
                new ScheduledJob
                {
                    JobName = "CheckZoneOccupancy",
                    JobGroup = "CheckZoneOccupancy",
                    CronSchedule = "0 0/1 * * * ?", // every 15 seconds
                    ScheduleType = typeof(CheckZoneOccupancy)
                },
                new ScheduledJob
                {
                    JobName = "HeatMap",
                    JobGroup = "HeatMap",
                    CronSchedule = "0 0 0/4 * * ?", // every hours
                    ScheduleType = typeof(HeatMapJob)
                },
                new ScheduledJob
                {
                    JobName = "OperationalTiming",
                    JobGroup = "OperationalTiming",
                    CronSchedule = "0 0/2 * * * ?", // every 2 miniute
                    ScheduleType = typeof(ManageOperationalTimingJob)
                },
                new ScheduledJob
                {
                    JobName = "ReportScheduler",
                    JobGroup = "ReportScheduler",
                    CronSchedule = "0 0/5 * * * ?", // every 5 miniute
                    ScheduleType = typeof(ReportSchedulerJob)
                },
                new ScheduledJob
                {
                    JobName = "BackupDBScheduler",
                    JobGroup = "BackupDBScheduler",
                    //CronSchedule = "0 0/2 * * * ?", // every 2 miniute
                    CronSchedule = "0 0 0 * * ?", // daily at 12:00 AM.
                    ScheduleType = typeof(BackupDBJob)
                },
                new ScheduledJob
                {
                    JobName = "RetentionDBScheduler",
                    JobGroup = "RetentionDBScheduler",
                    //CronSchedule = "0 0/5 * * * ?", // every 2 miniute
                    CronSchedule = "0 0 0 * * ?", // daily at 12:00 AM.
                    ScheduleType = typeof(RetentionDBJob)
                },
                new ScheduledJob
                {
                    JobName = "ANPRImageRetentionScheduler",
                    JobGroup = "ANPRImageRetentionScheduler",
                    CronSchedule = "0 0/5 * * * ?", // every 2 miniute
                    //CronSchedule = "0 0 0 * * ?", // daily at 12:00 AM.
                    ScheduleType = typeof(ANPRImageRetentionJob)
                },
                 new ScheduledJob
                {
                    JobName = "MaintenanceSchedule",
                    JobGroup = "MaintenanceSchedule",
                    //CronSchedule = "0 0/2 * * * ?", // every 2 miniute
                    CronSchedule = "0 0 0 * * ?", // daily at 12:00 AM.
                    ScheduleType = typeof(MaintenanceScheduleJob)
                },
                new ScheduledJob
                {
                    JobName = "ReviveDeviceOfflineData",
                    JobGroup = "ReviveDeviceOfflineData",
                    CronSchedule = "0 0/2 * * * ?", // every 5 miniute
                    ScheduleType = typeof(ReviveDeviceOfflineDataJob)
                },
                new ScheduledJob
                {
                    JobName = "DueMaintenanceNotificationJob",
                    JobGroup = "DueMaintenanceNotificationJob",
                    CronSchedule = "0 30 12 * * ?", // every 5 miniute
                    ScheduleType = typeof(DueMaintenanceNotificationJob)
                },
                new ScheduledJob
                {
                    JobName = "VehicleParkingCountJob",
                    JobGroup = "VehicleParkingCountJob",
                    CronSchedule = "0 0 0 * * ?", // daily at 12:00 AM.
                    //CronSchedule = "0 0/2 * * * ?", // every 2 Miniute
                    ScheduleType = typeof(VehicleParkingCountJob)
                },
                new ScheduledJob
                {
                    JobName = "AnprOverstayNotification",
                    JobGroup = "AnprOverstayNotification",
                    CronSchedule = "0 0/2 * * * ?", // every 15 seconds
                    ScheduleType = typeof(AnprOverstayNotificationJob)
                },
                new ScheduledJob
                {
                    JobName = "Anpr24hStayNotificationJob",
                    JobGroup = "Anpr24hStayNotificationJob",
                    CronSchedule = "0 0/2 * * * ?", // every 15 seconds
                    ScheduleType = typeof(Anpr24hStayNotificationJob)
                },
                new ScheduledJob
                {
                    JobName = "SsmServerJob",
                    JobGroup = "SsmServerJob",
                    CronSchedule = "0 0/2 * * * ?", // every 15 seconds
                    ScheduleType = typeof(SsmServerJob)
                },
                new ScheduledJob
                {
                    JobName = "LinkedServerPermissionSyncJob",
                    JobGroup = "LinkedServerPermissionSyncJob",
                    CronSchedule = "0 0/5 * * * ?", // every 5 minutes
                    ScheduleType = typeof(LinkedServerPermissionSyncJob)
                },
                new ScheduledJob
                {
                    JobName = "IdracSystemLogs",
                    JobGroup = "IdracSystemLogs",
                    CronSchedule = "0 0/5 * * * ?", // every 5 minutes
                    ScheduleType = typeof(IdracSystemLogsJob)
                },
                new ScheduledJob
                {
                    JobName = "IdracDetailsJob",
                    JobGroup = "IdracDetailsJob",
                    CronSchedule = "0/30 * * * * ?",
                    ScheduleType = typeof(IdracDetailsJob)
                }
                // Add more jobs as needed
            };
        }
    }  
} 
