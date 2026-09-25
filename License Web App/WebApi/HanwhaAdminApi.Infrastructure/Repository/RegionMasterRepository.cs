using HanwhaAdminApi.Infrastructure.Connection;
using HanwhaAdminApi.Infrastructure.Interfaces;
using HanwhaAdminApi.Model.Common;
using HanwhaAdminApi.Model.DbEntities;
using MongoDB.Driver;

namespace HanwhaAdminApi.Infrastructure.Repository
{
    public class RegionMasterRepository : RepositoryBase<RegionMaster>, IRegionMasterRepository
    {
        public RegionMasterRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.RegionMaster)
        {

        }


        public async Task<bool> IsRegionNameExistAsync(string regionName, string regionId)
        {
            var filter = Builders<RegionMaster>.Filter.And(
                // Same name (with collation for case-insensitive)
                Builders<RegionMaster>.Filter.Eq(x => x.Name, regionName),
                // Not deleted
                Builders<RegionMaster>.Filter.Eq(x => x.IsDeleted, false),
                // Exclude the current document by Id
                Builders<RegionMaster>.Filter.Ne(x => x.Id, regionId)
            );

            var options = new FindOptions
            {
                // Case-insensitive; Secondary ignores case and diacritics.
                // If you want case-insensitive but diacritic-sensitive, you can add `caseLevel: true`.
                Collation = new Collation(locale: "en", strength: CollationStrength.Secondary /*, caseLevel: true */)
            };

            return await dbEntity.Find(filter, options).AnyAsync();
        }

    }
}
