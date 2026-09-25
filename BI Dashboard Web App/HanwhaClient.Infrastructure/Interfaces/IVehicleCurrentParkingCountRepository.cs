using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IVehicleCurrentParkingCountRepository : IRepositoryBase<VehicleCurrentParkingCount>
    {
        Task<VehicleCurrentParkingCount> GetCurrentParkingCountId(string DeviceId, int ChannelNo, int LineIndex);
        Task<bool> ResetParkingCount(ParkingCountResetRequest parkingCountResetRequest);
        
    }
}
