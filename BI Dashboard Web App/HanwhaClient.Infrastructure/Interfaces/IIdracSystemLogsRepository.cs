using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IIdracSystemLogsRepository : IRepositoryBase<IdracSystemLogs>
    {
        Task<(List<IdracSystemLogs> logs, long totalCount)> GetIdracSystemLogsAsync(IdracSystemLogsListRequest request);
        Task<DateTime?> GetLatestLogTimestampAsync(string idracServerId);
    }
}
