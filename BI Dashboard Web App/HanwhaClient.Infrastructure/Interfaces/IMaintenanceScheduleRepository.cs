using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IMaintenanceScheduleRepository : IRepositoryBase<MaintenanceSchedule>
    {
        Task<(IEnumerable<MaintenanceSchedule> Data, int TotalCount)> GetAllMaintenanceSchedule(MaintenanceScheduleSerachModel model);
        Task<IEnumerable<MaintenanceSchedule>> GetDueMaintenanceScheduleDevices();
        Task<IEnumerable<MaintenanceSchedule>> GetMaintenanceScheduleForWidget(RMAWidgetRequest widgetRequest);
        Task<IEnumerable<MaintenanceSchedule>> GetCameraInMaintenance(CameraInMaintenanceSearchDto cameraInMaintenanceSearchDto);
        Task<bool> CheckMaintenanceDeviceExits(string deviceId);
        Task<IEnumerable<string>> GetmaintenanceScheduleByDeviceIds(IEnumerable<string> deviceIds);
        //Task<List<MaintenanceSchedule>> GetAllAsync();
        //Task<MaintenanceSchedule> UpsertAsync(MaintenanceSchedule schedule);
        //Task<MaintenanceSchedule> GetByDeviceAndPlanAsync(string deviceId, string maintenancePlanId);
    }
}
