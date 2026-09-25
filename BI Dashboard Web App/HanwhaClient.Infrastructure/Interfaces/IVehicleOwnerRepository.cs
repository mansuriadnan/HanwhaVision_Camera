using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IVehicleOwnerRepository : IRepositoryBase<VehicleOwner>
    {
        Task<bool> IsVehicleOwnerExistAsync(string building, string buildingUnit, string vehicleOwnerId = null);
        Task<(IEnumerable<VehicleOwner> ownerDetails, int totalCount)> GetAllOwnerAsync(AllVehicleOwnerRequest request, List<string> filteredDeviceIds);
        Task<List<string>> FindOwnerIdsByNameAsync(string ownerName);
        Task<IEnumerable<OwnerIdAndRegistrationTypeDto>> GetOwnerIdAndRegistrationType(string building, string buildingUnit);
        Task<IEnumerable<VehicleOwner>> GetOwnerByOwnerId(string ownerId);
        public Task<List<string>> FindOwnerIdsByNameTypeEmailAsync(string searchText);
        Task<VehicleOwner> GetSingleOwnerByOwnerId(string ownerId);
        Task<List<string>> GetVehicleOwnerIdsWithOverstayEnabledAsync();
        Task<List<string>> GetVehicleOwnerIdsWith24HStayEnabledAsync();
    }
}
