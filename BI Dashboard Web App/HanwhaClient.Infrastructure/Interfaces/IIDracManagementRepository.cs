using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IIDracManagementRepository : IRepositoryBase<IDracMaster>
    {
        Task<List<IdracServerListDashboardResponse>> GetServersByParentSiteIdAsync(string parentSiteId);
        Task<bool> AddAlarmAsync(string serverId, AlarmDetails alarm);
        Task<bool> UpdateAlarmSubscriptionIdAsync(string serverId, string alarmId, string subscriptionId);
        Task<List<AlarmEventDetails>> GetEventsByIpAddressAsync(string ipAddress);
        Task RemoveAlarmAsync(string serverId, string alarmId);
        Task UpdateIdracLedIndicatorAsync(string serverId, bool ledState);
        Task<(string? UserName, string? Password)> GetCredentialsByIpAddressAsync(string ipAddress);
        Task UpdatePowerStateAsync(string serverId, string powerState);
        Task ClearEventSubscriptionIdAsync(string serverId);
        Task<List<IDracMaster>> GetServersByIpAsync(string ipAddress);
        Task<bool> IsIpExistsAsync(string ipAddress, string? excludeId = null);
    }
}
