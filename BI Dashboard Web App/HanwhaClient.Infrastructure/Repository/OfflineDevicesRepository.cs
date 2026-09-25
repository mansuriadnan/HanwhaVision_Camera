using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public class OfflineDevicesRepository : RepositoryBase<OfflineDevices>, IOfflineDevicesRepository
    {
        public OfflineDevicesRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.OfflineDevices)
        {

        }

        

        public async Task<bool> UpdateOfflineDeviceStatus(string deviceId)
        {
            var update = Builders<OfflineDevices>.Update
                        .Set(x => x.OnlineTime, DateTime.UtcNow)
                        .Set(x => x.Status, 2);

            var filter = Builders<OfflineDevices>.Filter.Where(x => x.DeviceId == deviceId && x.Status == 1);
            var result = await dbEntity.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<OfflineDevices>> GetUnSyncOfflineDevices()
        {
            DateTime localMidnightTimeInUtc = DateTime.Now.Date.ToUniversalTime();
            var filter = Builders<OfflineDevices>.Filter.And(
                Builders<OfflineDevices>.Filter.Eq(x => x.Status, 2),
                Builders<OfflineDevices>.Filter.Eq(x => x.IsDeleted, false),
                Builders<OfflineDevices>.Filter.Gte(x => x.OnlineTime, localMidnightTimeInUtc));
            var result = await dbEntity
                .Find(filter)
                .SortBy(x => x.OnlineTime)
                .Limit(10)
                .ToListAsync();
            return result;
        }

        public async Task<List<OfflineDevices>> CameraDisconnectedTrackerAsync(RMAWidgetRequest widgetRequest)
        {
            var filter = Builders<OfflineDevices>.Filter.And(
                Builders<OfflineDevices>.Filter.Gte(x => x.OfflineTime, widgetRequest.StartDate),
                Builders<OfflineDevices>.Filter.Lte(x => x.OfflineTime, widgetRequest.EndDate),
                Builders<OfflineDevices>.Filter.Eq(x => x.IsDeleted, false),
                Builders<OfflineDevices>.Filter.In(x => x.DeviceId, widgetRequest.DeviceIds)
                );
            var result = await dbEntity.Find(filter).ToListAsync();
            await QueryDataFromLinkedServers(filter, result);
            return result;
        }
    }
}
