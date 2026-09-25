using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IVehicleParkingCountRepository : IRepositoryBase<VehicleParkingCount>
    {
        Task<IEnumerable<VehicleParkingCount>> VehicleParkingAnalysisDataAsync(IEnumerable<ZoneCamera> zoneCamerasList, DateTime startdate, DateTime enddate);
        Task<VehicleParkingCount> GetLatestVehicleParkingCountAsync(string DeviceId, int ChannelNo, int LineIndex, DateTime startdate);
    }
}
