using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface IMaintenanceScheduleService
    {
        Task<string> AddUpdateMaintenanceScheduleAsync(MaintenanceScheduleDto scheduleDto, string userId);
        Task<PagedResult<MaintenanceScheduleSearchResponseDto>> GetAllAsync(MaintenanceScheduleSerachModel model);
        Task<StringBuilder> ExportMaintenanceScheduleCSV(MaintenanceScheduleSerachModel model);
        Task<string> UpdateMaintenanceScheduleStatusAsync(UpdateMaintenanceScheduleStatusDto dto, string userId);
        Task<string> GetLiveImageAsync(RequestCurrentImageDto dto);
        Task<(string imageBase64, string ErrorMessage)> GetBeforeAfterImageAsync(string imageFullpath);
    }
}
