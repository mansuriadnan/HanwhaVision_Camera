using HanwhaClient.Model.Dto;
using System.Text;

namespace HanwhaClient.Application.Interfaces
{
    public interface IANPRVehicleService
    {
        Task<(string Id, string ErrorMessage)> SaveANPRVehicleAsync(ANPRVehicleRequest ownerRequest, string userId);
        Task<IEnumerable<ANPRVehicleList>> GetAllANPRVehicleByOwnerAsync(AllANPRVehicleRequest request);
        Task<bool> DeleteANPRVehiclesAsync(string id, string userId);
        Task<IEnumerable<OwnerIdAndRegistrationTypeDto>> GetOwnerIdAndRegistrationTypeAsync(string building, string buildingUnit);
        Task<IEnumerable<string>> CountryIdsByNameAsync(string countryName);
        Task<bool> UploadANPRImages(IEnumerable<ANPRImageUpload> aNPRImageUploads);
        Task<VehicleByPlateNumberLPRDto> GetVehicleByPlateNumAsync(string vehicleNumber, string Country, string state);
        Task<IEnumerable<ANPRParkingResponse>> ANPRVehicleParkingCountAsync(WidgetRequest widgetRequest);
        Task<IEnumerable<VehicleParking>> ANPRVehicleParkingCountByZonesAsync(WidgetRequest widgetRequest);
        Task<StringBuilder> ANPRVehicleParkingCountCsvAsync(WidgetRequest widgetRequest);
    }
}
