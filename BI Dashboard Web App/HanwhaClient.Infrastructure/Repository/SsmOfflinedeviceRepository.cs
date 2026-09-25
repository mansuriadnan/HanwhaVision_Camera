using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Servers;

namespace HanwhaClient.Infrastructure.Repository
{
    public class SsmOfflinedeviceRepository : RepositoryBase<SsmOfflinedevice>, ISsmOfflinedeviceRepository
    {
        public SsmOfflinedeviceRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.SsmOfflinedevice)
        {
        }

        public async Task<bool> UpdateSsmDeviceStatusAsync(string serverId, string deviceId, bool isOnline)
        {
            var filter = Builders<SsmOfflinedevice>.Filter.And(
                Builders<SsmOfflinedevice>.Filter.Eq(x => x.DeviceId, deviceId),
                Builders<SsmOfflinedevice>.Filter.Eq(x => x.OnlineTime, null)
            );

            var existing = await dbEntity.Find(filter).FirstOrDefaultAsync();

            if (!isOnline)
            {
                if (existing != null)
                {
                    // Already offline do nothing
                    return false;
                }

                var newEntry = new SsmOfflinedevice
                {
                    ServerId = serverId,
                    DeviceId = deviceId,
                    OfflineTime = DateTime.UtcNow,
                    OnlineTime = null,
                    CreatedOn = DateTime.UtcNow
                };

                var result = await InsertAsync(newEntry);
                return !string.IsNullOrEmpty(result);
            }
            else
            {
                //Device ONLINE

                if (existing == null)
                {
                    // No offline record nothing to update
                    return false;
                }

                var update = Builders<SsmOfflinedevice>.Update
                    .Set(x => x.OnlineTime, DateTime.UtcNow)
                    .Set(x => x.UpdatedOn, DateTime.UtcNow);

                var result = await UpdateFieldsAsync(existing.Id, update);
                return result;
            }
        }

        public async Task<SsmOfflinedevice> GetSsmOfflinedevice(string serverId, string deviceId)
        {
            var filter = Builders<SsmOfflinedevice>.Filter.And(
                Builders<SsmOfflinedevice>.Filter.Eq(x => x.DeviceId, deviceId),
                Builders<SsmOfflinedevice>.Filter.Eq(x => x.ServerId, serverId),
                Builders<SsmOfflinedevice>.Filter.Eq(x => x.IsDeleted, false)
            );

            var data = await dbEntity.Find(filter).SortByDescending(x => x.CreatedOn).FirstOrDefaultAsync();
            return data;
        }

        public async Task<List<SsmDeviceAvailabilityResponse>> GetSsmOfflineDeviceHistoryByDateAsync(string deviceId, DateTime startOfDayUTC, DateTime endOfDayUTC)
        {
            var filter = Builders<SsmOfflinedevice>.Filter.And(
                Builders<SsmOfflinedevice>.Filter.Eq(x => x.DeviceId, deviceId),
                Builders<SsmOfflinedevice>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SsmOfflinedevice>.Filter.Lte(x => x.OfflineTime, endOfDayUTC),
                Builders<SsmOfflinedevice>.Filter.Or(
                    Builders<SsmOfflinedevice>.Filter.Eq(x => x.OnlineTime, null),
                    Builders<SsmOfflinedevice>.Filter.Gte(x => x.OnlineTime, startOfDayUTC)
                )
            );

            var data = await dbEntity
                .Find(filter)
                .Project(x => new SsmDeviceAvailabilityResponse
                {
                    Id = x.Id,
                    OfflineTime = x.OfflineTime,
                    OnlineTime = x.OnlineTime
                })
                .SortBy(x => x.OfflineTime)
                .ToListAsync();

            return data;
        }

        public async Task<List<SsmOfflinedevice>> GetOfflineDeviceHistoryByServerIdsAsync(List<string> serverIds, DateTime startOfDayUTC, DateTime endOfDayUTC)
        {
            var filter = Builders<SsmOfflinedevice>.Filter.And(
                Builders<SsmOfflinedevice>.Filter.In(x => x.ServerId, serverIds),
                Builders<SsmOfflinedevice>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SsmOfflinedevice>.Filter.Lte(x => x.OfflineTime, endOfDayUTC),
                Builders<SsmOfflinedevice>.Filter.Or(
                    Builders<SsmOfflinedevice>.Filter.Eq(x => x.OnlineTime, null),
                    Builders<SsmOfflinedevice>.Filter.Gte(x => x.OnlineTime, startOfDayUTC)
                )
            );

            var data = await dbEntity.Find(filter).SortBy(x => x.OfflineTime).ToListAsync();
            return data;
        }

        public async Task<Dictionary<string, List<SsmOfflinedevice>>> GetOfflineStatusAsync(string serverId,List<string> deviceIds,DateTime startOfDayUTC,DateTime endOfDayUTC)
        {
            var filter = Builders<SsmOfflinedevice>.Filter.And(
                Builders<SsmOfflinedevice>.Filter.Eq(x => x.ServerId, serverId),
                Builders<SsmOfflinedevice>.Filter.In(x => x.DeviceId, deviceIds),
                Builders<SsmOfflinedevice>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SsmOfflinedevice>.Filter.Lte(x => x.OfflineTime, endOfDayUTC),
                Builders<SsmOfflinedevice>.Filter.Or(
                    Builders<SsmOfflinedevice>.Filter.Eq(x => x.OnlineTime, null),
                    Builders<SsmOfflinedevice>.Filter.Gte(x => x.OnlineTime, startOfDayUTC)
                )
            );

            var data = await dbEntity.Find(filter)
                .SortByDescending(x => x.OfflineTime)
                .ToListAsync();

            return data
                .GroupBy(x => x.DeviceId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
