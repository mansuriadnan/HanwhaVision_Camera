using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IANPRVehicleRepository : IRepositoryBase<ANPRVehicle>
    {
        Task<bool> IsANPRVehicleExistAsync(ANPRVehicleRequest anprVehicleRequest, string ownerRegistrationType);
        Task<IEnumerable<ANPRVehicle>> GetAllANPRVehicleByOwner(AllANPRVehicleWithSearchRequest request);
        Task<IEnumerable<string>> GetANPRVehicleIdsByOwner(string ownerId);
        Task<VehicleByPlateNumberLPRDto> GetVehicleByPlateNumAsync(string plateCode, string country, string state);
    }
}
