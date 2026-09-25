using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IOfflineDevicesRepository : IRepositoryBase<OfflineDevices>
    {
        public Task<bool> UpdateOfflineDeviceStatus(string deviceId);
        public Task<List<OfflineDevices>> GetUnSyncOfflineDevices();
        public Task<List<OfflineDevices>> CameraDisconnectedTrackerAsync(RMAWidgetRequest widgetRequest);
    }
}
