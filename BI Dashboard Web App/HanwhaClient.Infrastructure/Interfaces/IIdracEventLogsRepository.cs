using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IIdracEventLogsRepository : IRepositoryBase<IdracEventLogs>
    {
        Task<(List<IdracEventLogs> logs, long totalCount)> GetIdracEventLogsAsync(IdracEventLogsListRequest request);
    }
}
