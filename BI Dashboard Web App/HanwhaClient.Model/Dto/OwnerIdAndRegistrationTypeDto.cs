using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class OwnerIdAndRegistrationTypeDto
    {
        public string? OwnerId { get; set; }
        public string? RegistrationType { get; set; }
    }
    public class VehicleByPlateNumberLPRDto
    {
        public string? AnprVehicleId { get; set; }
        public string? OwnerId { get; set; }
        public DateTime? VisitorValidFrom { get; set; }
        public DateTime? VisitorValidTo { get; set; }
    }

    public class ANPRParkingResponse
    {
        public string? Id { get; set; }
        public string? ANPRVehicleId { get; set; }
        public string? DeviceId { get; set; }
        public DateTime? EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
    }
    public class CsvANPRParkingResponseModel
    {
        public DateTime? DateTime { get; set; }
        public int Available { get; set; }
        public int Occupied { get; set; }
    }
}
