using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public class SsmOfflineServerRepository : RepositoryBase<SsmOfflineServer>, ISsmOfflineServerRepository
    {
        public SsmOfflineServerRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.SsmOfflineServer)
        {
            
        }

        public async Task<SsmOfflineServer> GetSsmOfflineServer(string serverId)
        {
            var filter = Builders<SsmOfflineServer>.Filter.And(
                Builders<SsmOfflineServer>.Filter.Eq(x => x.ServerId, serverId),
                Builders<SsmOfflineServer>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SsmOfflineServer>.Filter.Eq(x => x.OnlineTime, null)
            );

            var data = await dbEntity.Find(filter).SortByDescending(x => x.CreatedOn).FirstOrDefaultAsync();
            return data;
        }

        public async Task<bool> UpdateSsmServerStatusAsync(string serverId, bool isOnline)
        {
            var filter = Builders<SsmOfflineServer>.Filter.And(
                Builders<SsmOfflineServer>.Filter.Eq(x => x.ServerId, serverId),
                Builders<SsmOfflineServer>.Filter.Eq(x => x.OnlineTime, null) );

            var existing = await dbEntity.Find(filter).FirstOrDefaultAsync();

            if (!isOnline)
            {
                

                if (existing != null)
                {
                    // Already offline  avoid duplicate entry
                    return false;
                }

                var newEntry = new SsmOfflineServer
                {
                    ServerId = serverId,
                    OfflineTime = DateTime.UtcNow,
                    OnlineTime = null,
                    CreatedOn = DateTime.UtcNow
                };

                var result = await InsertAsync(newEntry);
                return !string.IsNullOrEmpty(result);
            }
            else
            {
                if (existing == null)
                {
                    // No active offline record  nothing to update
                    return false;
                }

                var update = Builders<SsmOfflineServer>.Update
                    .Set(x => x.OnlineTime, DateTime.UtcNow)
                    .Set(x => x.UpdatedOn, DateTime.UtcNow);

                var result = await UpdateFieldsAsync(existing.Id, update);
                return result;
            }
        }

        public async Task<List<SsmServerAvailabilityResponse>> GetOfflineServerHistoryByDateAsync(string serverId, DateTime startOfDayUTC, DateTime endOfDayUTC)
        {
            var filter = Builders<SsmOfflineServer>.Filter.And(
                Builders<SsmOfflineServer>.Filter.Eq(x => x.ServerId, serverId),
                Builders<SsmOfflineServer>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SsmOfflineServer>.Filter.Lte(x => x.OfflineTime, endOfDayUTC),
                Builders<SsmOfflineServer>.Filter.Or(
                    Builders<SsmOfflineServer>.Filter.Eq(x => x.OnlineTime, null),
                    Builders<SsmOfflineServer>.Filter.Gte(x => x.OnlineTime, startOfDayUTC)
                )
            );

            var projection = Builders<SsmOfflineServer>.Projection
                .Include(x => x.Id)
                .Include(x => x.OfflineTime)
                .Include(x => x.OnlineTime);

            var data = await dbEntity
                .Find(filter)
                .Project(x => new SsmServerAvailabilityResponse
                {
                    Id = x.Id,
                    OfflineTime = x.OfflineTime,
                    OnlineTime = x.OnlineTime
                })
                .SortBy(x => x.OfflineTime)
                .ToListAsync();

            return data;
        }

        public async Task<List<SsmOfflineServer>> GetOfflineServerHistoryByServerIdsAsync(List<string> serverIds, DateTime startOfDayUTC, DateTime endOfDayUTC)
        {
            var filter = Builders<SsmOfflineServer>.Filter.And(
                Builders<SsmOfflineServer>.Filter.In(x => x.ServerId, serverIds),
                Builders<SsmOfflineServer>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SsmOfflineServer>.Filter.Lte(x => x.OfflineTime, endOfDayUTC),
                Builders<SsmOfflineServer>.Filter.Or(
                    Builders<SsmOfflineServer>.Filter.Eq(x => x.OnlineTime, null),
                    Builders<SsmOfflineServer>.Filter.Gte(x => x.OnlineTime, startOfDayUTC)
                )
            );

            var data = await dbEntity.Find(filter).SortBy(x => x.OfflineTime).ToListAsync();
            return data;
        }
    }
}
