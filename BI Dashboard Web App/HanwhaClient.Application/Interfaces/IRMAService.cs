using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface IRMAService
    {
        Task<bool> SaveRMAsync(RmaRequestDto planDto, string userId);
        Task<PagedResult<RMAResponseDto>> GetRMAAsync(RMASerachModel model);
        Task<bool> DeleteRMAAsync(string id, string userId);
        Task<IEnumerable<RMAWidgetResponse>> GetRmaMaintenanceCount(WidgetRequest widgetRequest);
        Task<StringBuilder> DownloadRmaMaintenanceCSV(WidgetRequest widgetRequest);
        Task<StringBuilder> ExportRMACSV(RMASerachModel model,string userId);
    }
}
