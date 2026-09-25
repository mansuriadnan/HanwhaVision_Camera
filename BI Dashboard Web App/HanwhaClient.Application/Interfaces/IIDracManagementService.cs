using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface IIDracManagementService
    {
        public Task<(bool isSuccess, string ErrorMessage)> AddUpdateServerDetails(IDracServerManagementRequest request, string userId);
        Task<(IEnumerable<IDracMaster> data, Dictionary<string, object> referenceData)> GetAllIDracServerAsync();
        Task<bool> DeleteIDracServerManagement(DeleteIDracServerRequest request, string userId);
        Task<(bool success, string errorMessage)> AddAlarmDetailsAsync(IDracEventAlarmRequest request);
        Task<bool> IdracEventAlarmReceiverAsync(SubscribedAlertEventPayload request);
        Task<bool> DeleteIdracSubscribedEventAsync(string alarmId, string serverId);
        Task<JsonElement> GetIdracSubscribedEventsAsync();
    }
}
