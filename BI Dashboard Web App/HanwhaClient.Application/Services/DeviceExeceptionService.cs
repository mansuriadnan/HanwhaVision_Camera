using HanwhaClient.Application.Interfaces;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Application.Services
{
    public class DeviceExeceptionService : IDeviceExeceptionService
    {
        public List<DeviceException> PeopleCountDevices { get; set; } = new List<DeviceException>();
        public List<DeviceException> VehicleCountDevices { get; set; } = new List<DeviceException>();
        public List<DeviceException> ShoppingCartCountDevices { get; set; } = new List<DeviceException>();
        public List<DeviceException> ForkliftCountDevices { get; set; } = new List<DeviceException>();
        public List<DeviceException> HeatmapDevices { get; set; } = new List<DeviceException>();
        public List<DeviceException> MultilaneVehicleCountDevices { get; set; } = new List<DeviceException>();
        public List<DeviceException> DeviceEvents { get; set; } = new List<DeviceException>();

    }
}
