using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Repository
{
    public class SiteRepository : RepositoryBase<SiteMaster>, ISiteRepository
    {
        public SiteRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.SiteMaster)
        {
        }
        public async Task<List<SiteMaster>> GetByFilterAsync(FilterDefinition<SiteMaster> filter,ProjectionDefinition<SiteMaster> projection)
        {
            return await dbEntity
                .Find(filter)
                .Project<SiteMaster>(projection)
                .ToListAsync();
        }
    }
}
