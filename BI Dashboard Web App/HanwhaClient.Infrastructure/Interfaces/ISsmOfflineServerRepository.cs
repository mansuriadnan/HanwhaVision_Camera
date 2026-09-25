using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface ISsmOfflineServerRepository : IRepositoryBase<SsmOfflineServer>
    {
        Task<bool> UpdateSsmServerStatusAsync(string serverId, bool isOnline);
        Task<SsmOfflineServer> GetSsmOfflineServer(string serverId);
        Task<List<SsmServerAvailabilityResponse>> GetOfflineServerHistoryByDateAsync(string serverId, DateTime startOfDayUTC, DateTime endOfDayUTC);
        Task<List<SsmOfflineServer>> GetOfflineServerHistoryByServerIdsAsync(List<string> serverIds, DateTime startOfDayUTC, DateTime endOfDayUTC);
    }
}
