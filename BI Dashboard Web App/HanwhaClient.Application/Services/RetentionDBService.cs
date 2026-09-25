using HanwhaClient.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services
{
    public class RetentionDBService : IRetentionDBService
    {
        private readonly Dictionary<string, Func<int, Task>> _retentionProcessors;

        public RetentionDBService(
            IPeopleWidgetService peopleWidgetService,
            IVehicleService vehicleService,
            IForkliftCountService forkliftCountService,
            IWidgetService widgetService,
            IMultiLaneVehicleCountService multiLaneVehicleCountService,
            IExceptionLogService exceptionLogService,
            IShoppingCartCountService shoppingCartCountService,
            IUserNotificationService userNotificationService)
        {
            _retentionProcessors = new Dictionary<string, Func<int, Task>>(StringComparer.OrdinalIgnoreCase)
        {
            { "peopleCount", retentionPeriod => peopleWidgetService.ProcessPeopleRetentionData(retentionPeriod) },
            { "vehicleCount", retentionPeriod => vehicleService.ProcessVehicleRetentionData(retentionPeriod) },
            { "forkliftCount", retentionPeriod => forkliftCountService.ProcessForkliftRetentionData(retentionPeriod) },
            { "heatMap", retentionPeriod => widgetService.ProcessHetmapRetentionData(retentionPeriod) },
            { "multiLaneVehicleCount", retentionPeriod => multiLaneVehicleCountService.ProcessMultiLaneVehicleRetentionData(retentionPeriod) },
            { "queueManagement", retentionPeriod => widgetService.ProcessQueueManagementRetentionData(retentionPeriod) },
            { "deviceEvents", retentionPeriod => widgetService.ProcessDeviceEventRetentionData(retentionPeriod) },
            { "exceptionLog", retentionPeriod => exceptionLogService.ProcessExceptionLogsRetentionData(retentionPeriod) },
            { "shoppingCartCount", retentionPeriod => shoppingCartCountService.ProcessShoppingCartCountRetentionData(retentionPeriod) },
            { "userNotification", retentionPeriod => userNotificationService.ProcessUserNotificationRetentionData(retentionPeriod) }
        };
        }

        public async Task<bool> InsertOrDeleteRetentionData(int retentionPeriod)
        {
            foreach (var processor in _retentionProcessors.Values)
            {
                await processor(retentionPeriod);
            }

            return true;
        }
    }

}
