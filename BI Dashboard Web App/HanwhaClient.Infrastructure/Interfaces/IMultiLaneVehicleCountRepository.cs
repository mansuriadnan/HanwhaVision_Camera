using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IMultiLaneVehicleCountRepository : IRepositoryBase<MultiLaneVehicleCount>, IRetentionRepository<MultiLaneVehicleCount>
    {
        Task<IEnumerable<VehicleTurningMovementResponse>> VehicleTurningMovementAnalysisData(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute, bool isArchivedData);
    }

}
