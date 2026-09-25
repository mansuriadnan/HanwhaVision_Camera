using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface IIdracDashboardService
    {
        Task<(IEnumerable<IdracServerListDashboardResponse> data, Dictionary<string, object> referenceData)> GetIdracServerListAsync(string parentSiteId);
        Task<IdracEventLogsListResponse> GetIdracEventLogsAsync(IdracEventLogsListRequest request);
        Task<bool> UpdateIdracLedIndicatorAsync(IdracLedIndicatorRequest request);
        Task<bool> IdracPowerActionAsync(IdracPowerActionRequest request);
        Task<IdracSystemInformationResponse>GetSystemInformationAsync(IdracSystemInformationRequest request);
        Task<IdracSystemtLogsListResponse> GetIdracSystemLogsAsync(IdracSystemLogsListRequest request);
    }
}
