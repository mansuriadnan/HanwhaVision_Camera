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
    public class SsmServerUtilizationRepository : RepositoryBase<SsmServerUtilization>, ISsmServerUtilizationRepository
    {
        public SsmServerUtilizationRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.SsmServerUtilization)
        {
        }
        public async Task<List<SsmServerUtilization>> GetByServerIdsAsync(List<string> serverIds, DateTime startOfDayUTC, DateTime endOfDayUTC)
        {
            var filter = Builders<SsmServerUtilization>.Filter.In(x => x.ServerId, serverIds);

            var dateFilter = Builders<SsmServerUtilization>.Filter.And(
                Builders<SsmServerUtilization>.Filter.Gte(x => x.CreatedOn, startOfDayUTC),
                Builders<SsmServerUtilization>.Filter.Lt(x => x.CreatedOn, endOfDayUTC)
            );

            filter = Builders<SsmServerUtilization>.Filter.And(filter, dateFilter);           

            return await dbEntity.Find(filter).ToListAsync();
        }
    }
}
