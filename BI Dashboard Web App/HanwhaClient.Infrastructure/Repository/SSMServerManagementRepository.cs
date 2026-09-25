using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public class SSMServerManagementRepository : RepositoryBase<SsmSiteMapping>, ISSMServerManagementRepository
    {
        public SSMServerManagementRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.SsmSiteMapping)
        {
        }
        public async Task<SsmSiteMapping?> GetByIpAndPortAsync(string ipAddress, string port, string? excludeId = null)
        {           
            var filter = Builders<SsmSiteMapping>.Filter.And(
                Builders<SsmSiteMapping>.Filter.Eq(x => x.IPAddress, ipAddress),
                Builders<SsmSiteMapping>.Filter.Eq(x => x.Port, port),
                Builders<SsmSiteMapping>.Filter.Eq(x => x.IsDeleted, false)
            );

            // Ignore same record during edit
            if (!string.IsNullOrEmpty(excludeId))
            {
                filter &= Builders<SsmSiteMapping>.Filter.Ne(
                    x => x.Id,
                    excludeId);
            }

            return await dbEntity
                .Find(filter)
                .FirstOrDefaultAsync();
        }
        public async Task<List<SsmSiteMapping>> GetByParentSiteIdsAsync(List<string> parentSiteIds)
        {
            var filter = Builders<SsmSiteMapping>.Filter.And(
                Builders<SsmSiteMapping>.Filter.In(x => x.ParentSiteId, parentSiteIds),
                Builders<SsmSiteMapping>.Filter.Eq(x => x.IsDeleted, false)
            );

            return await dbEntity.Find(filter).ToListAsync();
        }
    }
}
