using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface ILicensePlateRecogRepository : IRepositoryBase<LicensePlateRecogDetails>
    {
        Task<int> CountVehiclesInByOwnerIdAsync(string vehicleOwnerId);
        Task<LicensePlateRecogDetails> GetLatestInRecordByVehicleAsync(string plate, string country, string state, string vehicleOwnerId);
        Task<IEnumerable<LicensePlateRecogDetails>> GetAllInRecordsByVehicleAsync(string plate, string country, string state);
        Task<IEnumerable<ANPRParkingResponse>> ANPRVehicleParkingCountAsync(IEnumerable<string> deviceIds, DateTime startTime, DateTime endTime);
        Task<(IEnumerable<LicensePlateRecogDetails> Data, int TotalCount)> GetAllLprDetailsAsync(IEnumerable<string>? deviceIds, IEnumerable<string>? ownerIds, AllLprRequest request);
        Task<IEnumerable<LicensePlateRecogDetails>> GetAllVehiclesCurrentlyInAsync();
        Task<IEnumerable<LicensePlateRecogDetails>> GetVehiclesCurrentlyInByOwnerIdsAsync(IEnumerable<string> vehicleOwnerIds);
        Task<IEnumerable<LicensePlateRecogDetails>> GetVehiclesInFor24HByOwnerIdsAsync(IEnumerable<string> vehicleOwnerIds, DateTime thresholdTime);
    }
}
