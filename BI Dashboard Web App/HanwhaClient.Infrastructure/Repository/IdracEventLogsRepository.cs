using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public class IdracEventLogsRepository : RepositoryBase<IdracEventLogs>, IIdracEventLogsRepository
    {
        public IdracEventLogsRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.IdracEventLogs)
        {
            
        }
        public async Task<(List<IdracEventLogs> logs, long totalCount)>GetIdracEventLogsAsync(IdracEventLogsListRequest request)
        {
            var filter =
                Builders<IdracEventLogs>.Filter.Eq(
                    x => x.SourceIP,
                    request.IpAddress);

            var totalCount =
                await dbEntity.CountDocumentsAsync(filter);

            var data = await dbEntity
                .Find(filter)
                .SortByDescending(x => x.EventTimestamp)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Limit(request.PageSize)
                .ToListAsync();

            return (data, totalCount);
        }
    }
}
