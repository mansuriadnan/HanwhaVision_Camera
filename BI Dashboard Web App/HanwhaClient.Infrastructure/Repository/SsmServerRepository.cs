using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.SSM;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Servers;

namespace HanwhaClient.Infrastructure.Repository
{
    public class SsmServerRepository : RepositoryBase<SsmServers>, ISsmServerRepository
    {
        private readonly ISsmOfflineServerRepository _ssmOfflineServerRepository;
        public SsmServerRepository(MongoDbConnectionService mongoDbConnectionService,
            ISsmOfflineServerRepository ssmOfflineServerRepository) : base(mongoDbConnectionService, AppDBConstants.SsmServer)
        {
            _ssmOfflineServerRepository = ssmOfflineServerRepository;
        }

        public async Task<(bool success, string id)> AddUpdateSsmServerDetails(SsmServerDto ssmServer, string siteId)
        {
            var filter = Builders<SsmServers>.Filter.And(
                Builders<SsmServers>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SsmServers>.Filter.Eq(x => x.IpAddress, ssmServer.Data.Address));

            var data = await dbEntity.Find(filter).FirstOrDefaultAsync();

            var serverDetails = new SsmServers
            {
                SsmSiteId = siteId,
                Name = ssmServer.Data.Name,
                IpAddress = ssmServer.Data.Address,
                Port = ssmServer.Data.Port,
                
                Status = Convert.ToInt16(ssmServer.Data.Status),
            };

            if (data != null)
            {

                serverDetails.UpdatedOn = DateTime.UtcNow;
                serverDetails.Id = data.Id;

                bool isOnlineServer = data.ServerStatus == "Connected" || data.ServerStatus == "Warning";
                bool isOnlineApi = serverDetails.Status != 0;

                if (isOnlineServer != isOnlineApi)
                {
                    if (isOnlineServer == true && isOnlineApi == false)
                    {
                        serverDetails.ServerStatus = "Disconnected";
                        var newEntry = new SsmOfflineServer
                        {
                            ServerId = serverDetails.Id,
                            OfflineTime = DateTime.UtcNow,
                            OnlineTime = null,
                            CreatedOn = DateTime.UtcNow
                        };

                        await _ssmOfflineServerRepository.InsertAsync(newEntry);
                        return (await UpdateAsync(serverDetails), serverDetails.Id);
                    }
                    else
                    {
                        serverDetails.ServerStatus = "Warning";
                        var offlineRecord = await _ssmOfflineServerRepository.GetSsmOfflineServer(data.Id);
                        if (offlineRecord != null)
                        {
                            var update = Builders<SsmOfflineServer>.Update
                            .Set(x => x.OnlineTime, DateTime.UtcNow)
                            .Set(x => x.UpdatedOn, DateTime.UtcNow);

                            var result = await _ssmOfflineServerRepository.UpdateFieldsAsync(offlineRecord.Id, update);
                        }
                        return (await UpdateAsync(serverDetails), serverDetails.Id);
                    }
                }
                return (false, serverDetails.Id);
            }
            else
            {
                serverDetails.CreatedOn = DateTime.UtcNow;
                serverDetails.ServerStatus = "Connected";
                var result = await InsertAsync(serverDetails);
                return (!string.IsNullOrEmpty(result), result);
            }
        }
        public async Task<List<SsmServers>> GetBySsmSiteIdsAsync(List<string> mappingIds)
        {
            var filter = Builders<SsmServers>.Filter.And(
                Builders<SsmServers>.Filter.In(x => x.SsmSiteId, mappingIds),
                Builders<SsmServers>.Filter.Ne(x => x.IsDeleted, true)
            );

            return await dbEntity.Find(filter).ToListAsync();
        }
        public async Task<SsmServers> GetBySsmServerByIpAsync(string ipAddress)
        {
            var filter = Builders<SsmServers>.Filter.And(
                Builders<SsmServers>.Filter.Eq(x => x.IpAddress, ipAddress),
                Builders<SsmServers>.Filter.Ne(x => x.IsDeleted, true)
            );

            return await dbEntity.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<bool> SoftDeleteSsmServerAsync(string ssmSiteid, string userId)
        {
            if (string.IsNullOrEmpty(ssmSiteid))
                return false;

            var filter = Builders<SsmServers>.Filter.And(
                        Builders<SsmServers>.Filter.Eq(x => x.SsmSiteId, ssmSiteid),
                        Builders<SsmServers>.Filter.Ne(x => x.IsDeleted, true)
                    );

            var update = Builders<SsmServers>.Update
                .Set(x => x.UpdatedOn, DateTime.UtcNow)
                .Set(x => x.UpdatedBy, userId)
                .Set(x => x.IsDeleted, true)
                .Set(x => x.DeletedOn, DateTime.UtcNow);

            var result = await dbEntity.UpdateManyAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}
