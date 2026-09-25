using HanwhaClient.Model.DbEntities;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Indexes
{
    public class MongoIndexInitializer
    {
        private readonly IMongoDatabase _database;

        public MongoIndexInitializer(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task CreateIndexesAsync()
        {
            var deviceEventsCollection = _database.GetCollection<DeviceEvents>("deviceEvents");
            var deviceEventsIndexes = new List<CreateIndexModel<DeviceEvents>>
            {
                new CreateIndexModel<DeviceEvents>(
                    Builders<DeviceEvents>.IndexKeys.Descending(x => x.CreatedOn)),
                 new CreateIndexModel<DeviceEvents>(
                    Builders<DeviceEvents>.IndexKeys.Text(x => x.EventName)),
                 new CreateIndexModel<DeviceEvents>(
                    Builders<DeviceEvents>.IndexKeys.Ascending(x => x.DeviceId))
            };
            await deviceEventsCollection.Indexes.CreateManyAsync(deviceEventsIndexes);


            var forkliftCountCollection = _database.GetCollection<ForkliftCount>("forkliftCount");
            var forkliftCountIndexes = new List<CreateIndexModel<ForkliftCount>>
            {
                new CreateIndexModel<ForkliftCount>(
                    Builders<ForkliftCount>.IndexKeys.Descending(x => x.CreatedOn))
            };

            await forkliftCountCollection.Indexes.CreateManyAsync(forkliftCountIndexes);


            var heatMapCollection = _database.GetCollection<HeatMap>("heatMap");
            var heatMapIndexes = new List<CreateIndexModel<HeatMap>>
            {
                new CreateIndexModel<HeatMap>(
                    Builders<HeatMap>.IndexKeys.Descending(x => x.CreatedOn))
            };
            await heatMapCollection.Indexes.CreateManyAsync(heatMapIndexes);


            var multiLaneVehicleCountCollection = _database.GetCollection<MultiLaneVehicleCount>("multiLaneVehicleCount");
            var multiLaneVehicleCountIndexes = new List<CreateIndexModel<MultiLaneVehicleCount>>
            {
                new CreateIndexModel<MultiLaneVehicleCount>(
                    Builders<MultiLaneVehicleCount>.IndexKeys.Descending(x => x.CreatedOn)),
                new CreateIndexModel<MultiLaneVehicleCount>(
                    Builders<MultiLaneVehicleCount>.IndexKeys.Ascending(x => x.DeviceId))
            };
            await multiLaneVehicleCountCollection.Indexes.CreateManyAsync(multiLaneVehicleCountIndexes);


            var peopleCountCollection = _database.GetCollection<PeopleCount>("peopleCount");
            var peopleCountIndexes = new List<CreateIndexModel<PeopleCount>>
            {
                new CreateIndexModel<PeopleCount>(
                    Builders<PeopleCount>.IndexKeys.Descending(x => x.CreatedOn)),
                new CreateIndexModel<PeopleCount>(
                    Builders<PeopleCount>.IndexKeys.Ascending(x => x.DeviceId))
            };
            await peopleCountCollection.Indexes.CreateManyAsync(peopleCountIndexes);


            var queueManagementCollection = _database.GetCollection<QueueManagement>("queueManagement");
            var queueManagementIndexes = new List<CreateIndexModel<QueueManagement>>
            {
                new CreateIndexModel<QueueManagement>(
                    Builders<QueueManagement>.IndexKeys.Descending(x => x.CreatedOn)),
                 new CreateIndexModel<QueueManagement>(
                    Builders<QueueManagement>.IndexKeys.Text(x => x.EventName)),
                 new CreateIndexModel<QueueManagement>(
                    Builders<QueueManagement>.IndexKeys.Ascending(x => x.DeviceId))
            };
            await queueManagementCollection.Indexes.CreateManyAsync(queueManagementIndexes);


            var shoppingCartCountCollection = _database.GetCollection<ShoppingCartCount>("shoppingCartCount");
            var shoppingCartCountIndexes = new List<CreateIndexModel<ShoppingCartCount>>
            {
                new CreateIndexModel<ShoppingCartCount>(
                    Builders<ShoppingCartCount>.IndexKeys.Descending(x => x.CreatedOn)),
                new CreateIndexModel<ShoppingCartCount>(
                    Builders<ShoppingCartCount>.IndexKeys.Ascending(x => x.DeviceId))
            };
            await shoppingCartCountCollection.Indexes.CreateManyAsync(shoppingCartCountIndexes);


            var userMasterCollection = _database.GetCollection<UserMaster>("userMaster");
            var userMasterIndexes = new List<CreateIndexModel<UserMaster>>
            {
                new CreateIndexModel<UserMaster>(
                    Builders<UserMaster>.IndexKeys.Text(x => x.Username)),
            };
            await userMasterCollection.Indexes.CreateManyAsync(userMasterIndexes);


            var userNotificationCollection = _database.GetCollection<UserNotification>("userNotification");
            var userNotificationIndexes = new List<CreateIndexModel<UserNotification>>
            {
                new CreateIndexModel<UserNotification>(
                    Builders<UserNotification>.IndexKeys.Descending(x => x.CreatedOn))
            };
            await userNotificationCollection.Indexes.CreateManyAsync(userNotificationIndexes);


            var vehicleCountCollection = _database.GetCollection<VehicleCount>("vehicleCount");
            var vehicleCountIndexes = new List<CreateIndexModel<VehicleCount>>
            {
                new CreateIndexModel<VehicleCount>(
                    Builders<VehicleCount>.IndexKeys.Descending(x => x.CreatedOn)),
                new CreateIndexModel<VehicleCount>(
                    Builders<VehicleCount>.IndexKeys.Ascending(x => x.DeviceId))
            };
            await vehicleCountCollection.Indexes.CreateManyAsync(vehicleCountIndexes);


            var zoneCameraCollection = _database.GetCollection<ZoneCamera>("zoneCamera");
            var zoneCameraIndexes = new List<CreateIndexModel<ZoneCamera>>
            {
                new CreateIndexModel<ZoneCamera>(
                    Builders<ZoneCamera>.IndexKeys.Descending(x => x.CreatedOn))
            };
            await zoneCameraCollection.Indexes.CreateManyAsync(zoneCameraIndexes);


            var roleScreenMappingCollection = _database.GetCollection<RoleScreenMapping>("roleScreenMapping");
            var roleScreenMappingIndexes = new List<CreateIndexModel<RoleScreenMapping>>
            {
                new CreateIndexModel<RoleScreenMapping>(
                    Builders<RoleScreenMapping>.IndexKeys.Ascending(x => x.RoleId))
            };
            await roleScreenMappingCollection.Indexes.CreateManyAsync(roleScreenMappingIndexes);

            var auditLogsCollection = _database.GetCollection<AuditLog>("auditLog");
            var auditLogsIndexes = new List<CreateIndexModel<AuditLog>>
            {
                new CreateIndexModel<AuditLog>(
                    Builders<AuditLog>.IndexKeys
                        .Ascending(x => x.CollectionName)
                        .Ascending("DocumentKey._id")
                        .Ascending(x => x.IsDeleted)
                        .Descending(x => x.CreatedOn)
                )
            };
            await auditLogsCollection.Indexes.CreateManyAsync(auditLogsIndexes);


            var SsmOfflinedeviceCollection = _database.GetCollection<SsmOfflinedevice>("ssmOfflinedevice");
            var SsmOfflinedeviceIndexes = new List<CreateIndexModel<SsmOfflinedevice>>
            {
                new CreateIndexModel<SsmOfflinedevice>(
                    Builders<SsmOfflinedevice>.IndexKeys
                        .Ascending(x => x.DeviceId)
                        .Ascending(x => x.OnlineTime)),

                new CreateIndexModel<SsmOfflinedevice>(
                    Builders<SsmOfflinedevice>.IndexKeys
                        .Ascending(x => x.DeviceId)
                        .Ascending(x => x.ServerId)
                        .Ascending(x => x.IsDeleted)
                        .Descending(x => x.CreatedOn)),

                new CreateIndexModel<SsmOfflinedevice>(
                    Builders<SsmOfflinedevice>.IndexKeys
                        .Ascending(x => x.DeviceId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.OfflineTime)
                        .Ascending(x => x.OnlineTime)),

                new CreateIndexModel<SsmOfflinedevice>(
                    Builders<SsmOfflinedevice>.IndexKeys
                        .Ascending(x => x.ServerId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.OfflineTime)
                        .Ascending(x => x.OnlineTime)),

                new CreateIndexModel<SsmOfflinedevice>(
                    Builders<SsmOfflinedevice>.IndexKeys
                        .Ascending(x => x.ServerId)
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.DeviceId)
                        .Descending(x => x.OfflineTime)
                        .Ascending(x => x.OnlineTime)
                )
            };
            await SsmOfflinedeviceCollection.Indexes.CreateManyAsync(SsmOfflinedeviceIndexes);

            var ssmDeviceDetailsCollection = _database.GetCollection<SsmDeviceDetails>("ssmDeviceDetails");
            var ssmDeviceDetailsIndexes = new List<CreateIndexModel<SsmDeviceDetails>>
            {
                new CreateIndexModel<SsmDeviceDetails>(
                    Builders<SsmDeviceDetails>.IndexKeys
                        .Ascending(x => x.ServerId)
                        .Ascending(x => x.IsDeleted)),

                new CreateIndexModel<SsmDeviceDetails>(
                    Builders<SsmDeviceDetails>.IndexKeys
                        .Ascending(x => x.ServerId)
                        .Ascending(x => x.IsDeleted)
                        .Descending(x => x.CreatedOn)),

                new CreateIndexModel<SsmDeviceDetails>(
                    Builders<SsmDeviceDetails>.IndexKeys
                        .Ascending(x => x.ServerId)
                        .Ascending(x => x.IpAddress))
            };
            await ssmDeviceDetailsCollection.Indexes.CreateManyAsync(ssmDeviceDetailsIndexes);

            var exceptionLogCollection = _database.GetCollection<ExceptionLog>("exceptionLog");

            var exceptionLogIndexes = new List<CreateIndexModel<ExceptionLog>>
            {
                new CreateIndexModel<ExceptionLog>(
                    Builders<ExceptionLog>.IndexKeys
                        .Ascending(x => x.IsDeleted)
                        .Ascending(x => x.IsSuccess)
                        .Descending(x => x.LoggedAt)),
            
                // Index for ExceptionMessage regex/text search
                new CreateIndexModel<ExceptionLog>(
                    Builders<ExceptionLog>.IndexKeys
                        .Ascending(x => x.ExceptionMessage)),
            
                // Index for ExceptionType regex/text search
                new CreateIndexModel<ExceptionLog>(
                    Builders<ExceptionLog>.IndexKeys
                        .Ascending(x => x.ExceptionType)),
            
                // Index for RequestPath regex/text search
                new CreateIndexModel<ExceptionLog>(
                    Builders<ExceptionLog>.IndexKeys
                        .Ascending(x => x.RequestPath)),
            
                // Index for StackTrace regex/text search
                // Remove this if StackTrace is very large and rarely searched.
                new CreateIndexModel<ExceptionLog>(
                    Builders<ExceptionLog>.IndexKeys
                        .Ascending(x => x.StackTrace))
            };

            await exceptionLogCollection.Indexes.CreateManyAsync(exceptionLogIndexes);

        }
    }
}
