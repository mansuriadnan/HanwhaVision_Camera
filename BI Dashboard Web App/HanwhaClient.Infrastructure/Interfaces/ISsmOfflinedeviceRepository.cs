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
    public interface ISsmOfflinedeviceRepository : IRepositoryBase<SsmOfflinedevice>
    {
        Task<bool> UpdateSsmDeviceStatusAsync(string serverId, string deviceId, bool isOnline);
        Task<SsmOfflinedevice> GetSsmOfflinedevice(string serverId, string deviceId);
        Task<List<SsmDeviceAvailabilityResponse>> GetSsmOfflineDeviceHistoryByDateAsync(string deviceId, DateTime startOfDayUTC, DateTime endOfDayUTC);
        Task<List<SsmOfflinedevice>> GetOfflineDeviceHistoryByServerIdsAsync(List<string> serverIds, DateTime startOfDayUTC, DateTime endOfDayUTC);
        Task<Dictionary<string, List<SsmOfflinedevice>>> GetOfflineStatusAsync(string serverId, List<string> deviceIds, DateTime startOfDayUTC, DateTime endOfDayUTC);
    }
}
