using HanwhaClient.Model.Dto;
using System.Text;

namespace HanwhaClient.Application.Interfaces
{
    public interface IDeviceExceptionWidgetService
    {
        Task<DeviceExceptionResponse> DeviceExceptionDataAsync(WidgetRequest widgetRequest);
        Task<StringBuilder> DeviceExceptionDataCSVAsync(WidgetRequest widgetRequest);
    }
}
