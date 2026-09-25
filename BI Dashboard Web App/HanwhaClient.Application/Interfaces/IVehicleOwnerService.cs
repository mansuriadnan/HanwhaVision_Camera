using HanwhaClient.Model.Dto;

namespace HanwhaClient.Application.Interfaces
{
    public interface IVehicleOwnerService
    {
        Task<(string Id, string ErrorMessage)> SaveVehicleOwnerAsync(VehicleOwnerRequest ownerRequest, string userId);
        Task<AllVehicleOwnerListResponse> GetAllVehicleOwnerAsync(AllVehicleOwnerRequest request);
        Task<bool> DeleteOwnerAsync(string id, string userId);
    }
}
