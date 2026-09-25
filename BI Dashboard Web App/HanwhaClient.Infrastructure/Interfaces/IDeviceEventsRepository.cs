using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using SharpCompress.Archives;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IDeviceEventsRepository : IRepositoryBase<DeviceEvents>, IRetentionRepository<DeviceEvents>
    {
        Task<(IEnumerable<DeviceEventsLogsResponse> deviceDetails, int eventCount)> GetDeviceEventsLogsAsync(DeviceEventsLogsRequest request);
        Task<(IEnumerable<DeviceEvents> deviceDetails, int eventCount)> GetDeviceEventsLogsAsync1(DeviceEventsLogsRequest request);
        Task<bool> UpdateDeviceEventsStatusAsync(string id, string userId);
        Task<IEnumerable<EventQueueAnalysis>> PedestrianQueueAnalysisDataAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute, bool isArchived);
        Task<IEnumerable<EventQueueAnalysis>> ProxomityDetectionAnalysisDataAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute, bool isArchived);
        Task<IEnumerable<StoppedVehicleByTypeData>> StoppedVehicleByTypeAnalysisDataAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute, bool isArchived);
        Task<IEnumerable<EventQueueAnalysis>> VehicleSpeedViolationAnalysisDataAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute, bool isArchived);
        Task<IEnumerable<EventQueueAnalysis>> TrafficJamAnalysisDataAsync(string deviceId, DateTime startdate, DateTime enddate, int channel,int intervalMinute, bool isArchived);
        Task<IEnumerable<EventQueueAnalysis>> SlipFallQueueAnalysisDataAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute, bool isArchived);
        Task<IEnumerable<MaskDetectionAnalysis>> MaskDetectionAnalysisDataAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute,bool isArchived);
        Task<IEnumerable<EventQueueAnalysis>> WrongWayQueueAnalysisDataAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute, bool isArchived);
        Task<IEnumerable<EventQueueAnalysis>> BlockedExitAnalysisDataAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute, bool isArchived);
        Task<IEnumerable<EventQueueAnalysis>> VehicleUTurnAnalysisDataAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute, bool isArchived);
        Task<IEnumerable<EventQueueAnalysis>> SpeedDetectionByVehicleAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int IntervalMinute, bool isArchived);
        Task<IEnumerable<EventQueueAnalysis>> ForkliftSpeedDetectionAnalysisAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int IntervalMinute, bool isArchived);
    }
}
