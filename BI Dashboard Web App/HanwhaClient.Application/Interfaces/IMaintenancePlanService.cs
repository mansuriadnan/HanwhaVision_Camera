using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface IMaintenancePlanService
    {
        Task<(bool result, string errorMessage)> SaveMaintenancePlanAsync(MaintenancePlanDto planDto, string userId);
       // Task<IEnumerable<MaintenancePlanDto>> GetAllMaintenancePlansAsync();
        Task<PagedResult<MaintenancePlanDto>> GetMaintenancePlansAsync(MaintenancePlanSerachModel model);
        Task<bool> DeleteMaintenanceAsync(string id, string userId);
        Task<List<DeviceListByFloorZoneDto>> GetDeviceListByFloorZoneAsync();
    }
}
