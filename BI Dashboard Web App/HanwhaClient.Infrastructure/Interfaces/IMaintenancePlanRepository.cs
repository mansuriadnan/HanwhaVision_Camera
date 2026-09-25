using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IMaintenancePlanRepository : IRepositoryBase<MaintenancePlan>
    {
        Task<(IEnumerable<MaintenancePlan> Data, int TotalCount)> GetAllMaintenancePlan(MaintenancePlanSerachModel model, List<string> filteredDeviceIds);
        Task<IEnumerable<MaintenancePlan>> GetMaintenancePlanData();
        Task<IEnumerable<string>> GetAllMaintenancePlanIdsAsync(IEnumerable<string> deviceIds);
        Task<bool> IsMaintenancePlanExistsAsync(string maintenancePlanName, string? maintenancePlanId);
        Task<long> DeleteMaintenancePlanDeviceIdsAsync(IEnumerable<string> deviceIds, IEnumerable<string> planIds, string userId);
    }
}
