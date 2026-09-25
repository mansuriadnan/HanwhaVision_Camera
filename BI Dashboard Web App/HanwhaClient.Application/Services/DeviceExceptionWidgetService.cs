using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Dto;
using System.Linq;
using System.Text;

namespace HanwhaClient.Application.Services
{
    public class DeviceExceptionWidgetService : IDeviceExceptionWidgetService
    {
        private readonly IDeviceExeceptionService _deviceExeceptionService;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IZoneCameraRepository _zoneCameraRepository;
        private readonly IUsersService _usersService;
        private readonly IDateConvert _dateConvert;
        public DeviceExceptionWidgetService(IDeviceExeceptionService deviceExeceptionService,
                                            IDeviceMasterRepository deviceMasterRepository,
                                            IZoneCameraRepository zoneCameraRepository,
                                            IUsersService usersService,
                                            IDateConvert dateConvert)
        {
            _deviceExeceptionService = deviceExeceptionService;
            _deviceMasterRepository = deviceMasterRepository;
            _zoneCameraRepository = zoneCameraRepository;
            _usersService = usersService;
            _dateConvert = dateConvert;
        }
        public async Task<DeviceExceptionResponse> DeviceExceptionDataAsync(WidgetRequest widgetRequest)
        {
            IEnumerable<string> deviceIds = Enumerable.Empty<string>();

            if (widgetRequest.FloorIds != null && widgetRequest.FloorIds.FirstOrDefault() == "000000000000000000000000")
            {
                var zoneCameraList = await _deviceMasterRepository.GetUnMappeddevicesforWidget();
                deviceIds = zoneCameraList.Select(f => f.DeviceId).Distinct();
            }
            else
            {
                deviceIds = await _zoneCameraRepository.GetDevicebyFloorAndZoneAsync(widgetRequest.FloorIds, widgetRequest.ZoneIds);
            }

            return new DeviceExceptionResponse
            {
                ForkliftCountDevices = _deviceExeceptionService.ForkliftCountDevices.Where(x => deviceIds.Contains(x.DeviceId)).ToList(),
                PeopleCountDevices = _deviceExeceptionService.PeopleCountDevices.Where(x => deviceIds.Contains(x.DeviceId)).ToList(),
                ShoppingCartCountDevices = _deviceExeceptionService.ShoppingCartCountDevices.Where(x => deviceIds.Contains(x.DeviceId)).ToList(),
                MultilaneVehicleCountDevices = _deviceExeceptionService.MultilaneVehicleCountDevices.Where(x => deviceIds.Contains(x.DeviceId)).ToList(),
                DeviceEvents = _deviceExeceptionService.DeviceEvents.Where(x => deviceIds.Contains(x.DeviceId)).ToList(),
                HeatmapDevices = _deviceExeceptionService.HeatmapDevices.Where(x => deviceIds.Contains(x.DeviceId)).ToList(),
                VehicleCountDevices = _deviceExeceptionService.VehicleCountDevices.Where(x => deviceIds.Contains(x.DeviceId)).ToList(),
            };
        }

        public async Task<StringBuilder> DeviceExceptionDataCSVAsync1(WidgetRequest widgetRequest)
        {
            var csvBuilder = new StringBuilder();

            var cameraCountByFeaturesData = await DeviceExceptionDataAsync(widgetRequest);

            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);

            var offsetStartDate = widgetRequest.StartDate.Add(offsetTimeStamp);
            var offsetEndDate = widgetRequest.EndDate.Add(offsetTimeStamp);

            csvBuilder.Append(await GetTitleForCsv(widgetRequest.WidgetName, offsetStartDate, offsetEndDate, widgetRequest.UserId));

            var header = new List<string> { "Feature Name", "Ip Address", "ChannelNo" };
            csvBuilder.AppendLine(string.Join(",", header));

            foreach (var item in cameraCountByFeaturesData.PeopleCountDevices)
            {
                var row = $"{item.Feature},{item.IpAddress},{item.ChannelNo}";
                csvBuilder.AppendLine(row);
            }
            return csvBuilder;
        }

        public async Task<StringBuilder> DeviceExceptionDataCSVAsync(WidgetRequest widgetRequest)
        {
            var builder = new StringBuilder();

            var response = await DeviceExceptionDataAsync(widgetRequest);

            var offsetTimeStamp = await GetOffset(widgetRequest.UserId);
            var offsetStartDate = widgetRequest.StartDate.Add(offsetTimeStamp);
            var offsetEndDate = widgetRequest.EndDate.Add(offsetTimeStamp);

            // Title (unchanged)
            builder.Append(await GetTitleForCsv(
                widgetRequest.WidgetName,
                offsetStartDate,
                offsetEndDate,
                widgetRequest.UserId
            ));

            // CSV Header (each value in a separate column)
            builder.AppendLine("Feature Name,Ip Address,ChannelNo");

            // Append all device lists
            AppendRows(builder, response.PeopleCountDevices);
            AppendRows(builder, response.VehicleCountDevices);
            AppendRows(builder, response.ForkliftCountDevices);
            AppendRows(builder, response.ShoppingCartCountDevices);
            AppendRows(builder, response.MultilaneVehicleCountDevices);

            return builder;
        }

        private void AppendRows<T>(StringBuilder builder, IEnumerable<T>? items)
        {
            if (items == null) return;

            foreach (dynamic item in items)
            {
                builder.AppendLine(
                    $"{EscapeCsv(item.Feature)},{EscapeCsv(item.IpAddress)},{EscapeCsv(item.ChannelNo)}"
                );
            }
        }

        private string EscapeCsv(object? value)
        {
            if (value == null)
                return string.Empty;

            var stringValue = value.ToString() ?? string.Empty;

            // Escape double quotes and wrap in quotes if needed
            if (stringValue.Contains(",") || stringValue.Contains("\"") || stringValue.Contains("\n"))
            {
                stringValue = stringValue.Replace("\"", "\"\"");
                return $"\"{stringValue}\"";
            }

            return stringValue;
        }

        private async Task<TimeSpan> GetOffset(string userId)
        {
            var timeZone = await _usersService.GetTimeZone(userId);
            var offsetTimeStamp = _dateConvert.GetTimeSpanByOffset(timeZone.UtcOffset);
            return offsetTimeStamp;
        }
        private async Task<string> GetTitleForCsv(string widgetName, DateTime startDate, DateTime endDate, string userId)
        {
            var csvBuilder = new StringBuilder();
            var userData = await _usersService.GetUserAsync(userId);

            csvBuilder.AppendLine($"Widget name: {widgetName}");
            csvBuilder.AppendLine($"\"Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}\"");
            csvBuilder.AppendLine($"Exported By: {userData.Firstname} {userData.Lastname}");
            csvBuilder.AppendLine($"\"Exported On: {DateTime.Now:MMM dd, yyyy hh:mm tt}\"");

            return csvBuilder.ToString();
        }
    }
}
