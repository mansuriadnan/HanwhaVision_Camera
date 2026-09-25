using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Repository
{
    public class IdracSystemLogsRepository : RepositoryBase<IdracSystemLogs>, IIdracSystemLogsRepository
    {
        public IdracSystemLogsRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.IdracSystemLogs)
        {
        }
        public async Task<(List<IdracSystemLogs> logs, long totalCount)> GetIdracSystemLogsAsync(IdracSystemLogsListRequest request)
        {
            var filter =
                Builders<IdracSystemLogs>.Filter.Eq(
                    x => x.IdracServerId,
                    request.IdracServerId);

            var totalCount =
                await dbEntity.CountDocumentsAsync(filter);

            var data = await dbEntity
                .Find(filter)
                .SortByDescending(x => x.Datetime)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Limit(request.PageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<DateTime?> GetLatestLogTimestampAsync(string idracServerId)
        {
            var filter = MongoDB.Driver.Builders<IdracSystemLogs>.Filter.Eq(x => x.IdracServerId, idracServerId);

            var latestLogTimestamp = await dbEntity.Find(filter)
                .SortByDescending(x => x.Datetime)
                .Project(x => (DateTime?)x.Datetime)
                .FirstOrDefaultAsync();

            return latestLogTimestamp;
        }
    }
}
