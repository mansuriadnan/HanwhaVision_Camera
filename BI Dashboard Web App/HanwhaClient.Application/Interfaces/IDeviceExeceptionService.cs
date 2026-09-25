using HanwhaClient.Model.Dto;

namespace HanwhaClient.Application.Interfaces
{
    public interface IDeviceExeceptionService
    {
        List<DeviceException> PeopleCountDevices { get; set; }
        List<DeviceException> VehicleCountDevices { get; set; }
        List<DeviceException> ShoppingCartCountDevices { get; set; }
        List<DeviceException> ForkliftCountDevices { get; set; }
        List<DeviceException> HeatmapDevices { get; set; }
        List<DeviceException> MultilaneVehicleCountDevices { get; set; }
        List<DeviceException> DeviceEvents { get; set; }
    }
}
