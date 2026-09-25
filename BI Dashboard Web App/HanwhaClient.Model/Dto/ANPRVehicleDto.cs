using HanwhaClient.Model.Common;

namespace HanwhaClient.Model.Dto
{
    public class ANPRVehicleRequest
    {
        public string? Id { get; set; }
        public string VehicleOwnerId { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Series { get; set; }
        public int VehicleNumber { get; set; }
        public string PlateCode { get; set; }
        public string PlateCategory { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public DateTime? VisitorValidFrom { get; set; }
        public DateTime? VisitorValidTo { get; set; }
    }
    public class ANPRVehicleList
    {
        public string? Id { get; set; }
        public string? VehicleOwnerType { get; set; }
        public string? VehicleOwnerName { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string State { get; set; }
        public string Series { get; set; }
        public int VehicleNumber { get; set; }
        public string PlateCode { get; set; }
        public string PlateCategory { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public DateTime? VisitorValidFrom { get; set; }
        public DateTime? VisitorValidTo { get; set; }
    }

    public class ANPRImageUpload
    {
        public string ImageName { get; set; }
        public string ImageBase64 { get; set; }
    }

    public class ANPRVehicleListResponse
    {
        public long TotalCount { get; set; }
        public List<ANPRVehicleList> ANPRVehicleList { get; set; }
    }

    public class OwnerInfo
    {
        public string OwnerName { get; set; }
        public string RegistrationType { get; set; }
    }
    public class AllANPRVehicleRequest
    {
        public string? SearchText { get; set; }
        public string VehicleOwnerId { get; set; }
    }
    public class AllANPRVehicleWithSearchRequest : AllANPRVehicleRequest
    {
        public List<string>? VehicleOwnerIds { get; set; }
        public List<string>? CountryIds { get; set; }
    }
    public class ANPRVehicleDeleteRequest
    {
        public string Id { get; set; }
    }
}
