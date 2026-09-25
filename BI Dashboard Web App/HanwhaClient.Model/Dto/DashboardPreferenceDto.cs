using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.DeviceApiResponse;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;

namespace HanwhaClient.Model.Dto
{
    public class CameraCountResponse
    {
        public int TotalCameraCount { get; set; }
        public int OnlineCameraCount { get; set; }
        public int OflineCameraCount { get; set; }

    }

    public class CameraSeriesCountResponse
    {
        public string SeriesName { get; set; }
        public int TotalCount { get; set; }
    }
    public class CameraFeaturesCountResponse
    {
        public string? FeaturesName { get; set; }
        public int TotalCount { get; set; }
    }

    public class CameraDisconnectedTrackerResponse
    {
        public int HoursCount { get; set; }
        public int DayCount { get; set; }
        public int WeekCount { get; set; }
        public int MonthCount { get; set; }
    }

    public class CameraDisconnectedTrackerAnalsisResponse
    {
        public string DeviceId { get; set; }

        public DateTime OfflineTime { get; set; }

        public DateTime? OnlineTime { get; set; }

    }

    public class CameraDisconnectedTrackerCSV
    {
        public DateTime? DateTime { get; set; }
        public int Offline { get; set; }
        public int Online { get; set; }

    }
    public class MapCameraListByFeatures
    {
        public string DeviceId { get; set; }
        public int ChannelNo { get; set; }
        public string CameraName { get; set; }
        public string DeviceType { get; set; }
        public string? ZoneId { get; set; }
        public string? FloorId { get; set; }
    }

    public class DeviceByFloorsAndZonesRequest
    {
        public IEnumerable<string> FloorIds { get; set; }
        public IEnumerable<string>? ZoneIds { get; set; }
    }

    public class WidgetRequest
    {
        public IEnumerable<string> FloorIds { get; set; }
        public IEnumerable<string>? ZoneIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IEnumerable<string>? userRoles { get; set; }
        public int AverageIntervalMinute { get; set; } = 60;
        public int IntervalMinute { get; set; } = 10;
        public List<string>? WidgetNames { get; set; }
        public List<WidgetInfo>? WidgetTitleNames { get; set; }

        public string? UserId { get; set; }
        public string? DeviceId { get; set; } = "";
        public string? FromSummary { get; set; } = "";
        public string? WidgetName { get; set; }
        public CalculatorExportRule[]? calculatorExportRule { get; set; }
    }

    public class CalculatorExportRule
    {
        public string? Category { get; set; }
        public int Value { get; set; }
        public string? Unit { get; set; }
        public string? Operation { get; set; }
    }

    public class WidgetInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }


    public class MapWidgetRequest : WidgetRequest
    {
        public string? DeviceId { get; set; }
        public int? Channel { get; set; }
    }

    public class HeatmapWidgetResponse
    {
        public int ResolutionHeight { get; set; }
        public int ResolutionWidth { get; set; }
        public List<int> HeatMapData { get; set; }
        public string HeatmapImage { get; set; }
    }

    public class WidgetHeatmapRequest
    {
        public string DeviceId { get; set; }
        public int ChannelNo { get; set; }
        public string HeatmapType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class WidgetRequestForChart : WidgetRequest
    {
        public string? InOutType { get; set; } = "In";
        public int IntervalMinutes { get; set; } = 60;
        public int AddMinutes { get; set; } = 10;

        public string? WidgetName { get; set; } = "";
    }

    public class CameraCapacityUtilizationByZones
    {
        public string ZoneName { get; set; }
        public int MaxCapacity { get; set; } = 0;
        public double Utilization { get; set; } = 0;
        public double Percentage { get; set; } = 0;
        public int? PeopleDefaultOccupancy { get; set; } = 0;
        public int? VehicleDefaultOccupancy { get; set; } = 0;
    }

    public class CameraCapacityUtilizationAnalysisByZones
    {
        public string ZoneName { get; set; }
        public IEnumerable<CountAnalysisData> UtilizationData { get; set; }
    }

    public class CapacityUtilization
    {
        public double Utilization { get; set; }
        public double Percentage { get; set; }
        public double TotalCapacity { get; set; }
        public UtilizationMostLeastDay UtilizationMostLeastDay { get; set; }
        public int? PeopleDefaultOccupancy { get; set; } = 0;
        public int? VehicleDefaultOccupancy { get; set; } = 0;
    }

    public class UtilizationMostLeastDay
    {
        public double MostDayUtilization { get; set; }
        public DateTime MostDayUtilizationDay { get; set; }
        public double LeastDayUtilization { get; set; }
        public DateTime LeastDayUtilizationDay { get; set; }
    }


    public class CameraCapacityUtilizationByDevice
    {
        public string DeviceId { get; set; }
        public double UtilizationCount { get; set; }
        public DateTime? Date { get; set; }
    }

    public class PeopleVehicleCountSummary
    {
        public string DeviceId { get; set; }
        public DateTime? Date { get; set; }
        public int TotalInCount { get; set; }
        public int TotalOutCount { get; set; }
        public string HourRange { get; set; }
        public DateTime? CreatedOnWithTimezone { get; set; } // New field
    }


    public class PeopleVehicleCountAvgSummary
    {
        public DateTime? Date { get; set; }
        public int TotalInMinAvgCount { get; set; }
        public int TotalOutMinAvgCount { get; set; }
        public int TotalInMaxAvgCount { get; set; }
        public int TotalOutMaxAvgCount { get; set; }
        public string HourRange { get; set; }
    }


    public class TimeLineChartPeopleVehicleCount
    {
        public string? Time { get; set; }
        public string? DateTime { get; set; }
        public string? Date { get; set; }
        public int Count { get; set; }
        public string? Hour { get; set; }
        public int Value { get; set; }

    }

    public class PeopleVehicleInOutAvgChart
    {
        public DateTime? DateTime { get; set; }
        public string? DateTimeCsv { get; set; }
        public int InCount { get; set; }
        public int OutCount { get; set; }
        public string Hour { get; set; }
    }

    public class AvgPeopleInOutAvgChart
    {
        public DateTime? DateTime { get; set; }
        public string? DateTimeCsv { get; set; }
        public double InCount { get; set; }
        public double OutCount { get; set; }
    }

    public class PeopleVehicleInOutAvgChartDevicewise: PeopleVehicleInOutAvgChart
    {
        public string DeviceId { get; set; }
        public int ChannelNo { get; set; }
    }

    public class NewVsTotalVisitorChart
    {
        public DateTime? DateTime { get; set; }
        public string? DateTimeCsv { get; set; }
        public int NewInCount { get; set; }
        public int TotalCount { get; set; }
        public string Hour { get; set; }
        public int InValue { get; set; }
        public int OutValue { get; set; }
    }

    public class PVInOutAvgChart
    {
        public string DateTime { get; set; }
        public int InCount { get; set; }
        public int OutCount { get; set; }
        public string Hour { get; set; }
        public int InValue { get; set; }
        public int OutValue { get; set; }
    }


    public class PeopleVehicleInOutChartResponse
    {
        public List<TimeLineChartPeopleVehicleCount> Data { get; set; } = new();
    }


    public class PeopleCountRawDto
    {
        public DateTime? CreatedOn { get; set; }
        public IEnumerable<Line> Lines { get; set; }
        public IEnumerable<VehicleLine> VehicleLine { get; set; }
        public int InCount { get; set; }
        public int OutCount { get; set; }
        public string HourRange { get; set; }
    }

    public class InOutCountAverageWidgetDto
    {
        public int MinInCount { get; set; }
        public DateTime? MinInDate { get; set; }

        public int MaxInCount { get; set; }
        public DateTime? MaxInDate { get; set; }

        public int MinOutCount { get; set; }
        public DateTime? MinOutDate { get; set; }

        public int MaxOutCount { get; set; }
        public DateTime? MaxOutDate { get; set; }

        public double AverageInCount { get; set; }
        public double AverageOutCount { get; set; }
    }

    public class InOutPeopleCountAverageWidgetDto : InOutCountAverageWidgetDto
    {
        public int TotalInPeople { get; set; }
    }

    public class InOutVehicleCountAverageWidgetDto : InOutCountAverageWidgetDto
    {
        public int TotalInVehicle { get; set; }
    }
    public class VehicleByTypeCountWidgetDto
    {
        public long TruckInCount { get; set; }
        public long MotorCycleInCount { get; set; }
        public long BusInCount { get; set; }
        public long BicycleInCount { get; set; }
        public long CarInCount { get; set; }

        public long TruckOutCount { get; set; }
        public long MotorCycleOutCount { get; set; }
        public long BusOutCount { get; set; }
        public long BicycleOutCount { get; set; }
        public long CarOutCount { get; set; }
        public long TotalInVehicleCount { get; set; }
        public long TotalOutVehicleCount { get; set; }
    }

    public class VehicleByTypeChartWidgetDto : VehicleByTypeCountWidgetDto
    {
        public string? Date { get; set; }
        public DateTime? DateTime { get; set; }
        public string? Hour { get; set; }
    }

    public class VehicleByTypeChartSummary : VehicleByTypeCountWidgetDto
    {
        public DateTime? Date { get; set; }
        public string? HourRange { get; set; }
        public string? DeviceId { get; set; }
    }

    public class VehicleByTypeChartResponse
    {
        public double TruckInCount { get; set; }
        public double MotorCycleInCount { get; set; }
        public double BusInCount { get; set; }
        public double BicycleInCount { get; set; }
        public double CarInCount { get; set; }
        public double TruckOutCount { get; set; }
        public double MotorCycleOutCount { get; set; }
        public double BusOutCount { get; set; }
        public double BicycleOutCount { get; set; }
        public double CarOutCount { get; set; }
        public DateTime? DateTime { get; set; }
    }


    public class NewVsTotalVisitorExcel
    {
        public long NewVisitorsCount { get; set; }
        public long TotalVisitorsCount { get; set; }
        public DateTime? DateTime { get; set; }
    }

    public class NewVsTotalVisitorCountWidget
    {
        public long NewVisitorsCount { get; set; }
        public long TotalVisitorsCount { get; set; }
    }
    public class PeopleVehicleInOutTotal
    {
        public int TotalInCount { get; set; }
        public int TotalOutCount { get; set; }
    }
    public class VehicleByTypeCountWidgetData
    {
        public string? DeviceId { get; set; }
        public IEnumerable<VehicleCountData> VehicleCounts { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
    public class NewVsTotalVisitorCountWidgetData
    {
        public string? DeviceId { get; set; }
        public IEnumerable<Line> Lines { get; set; }
        public int? ChannelNo { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class PeopleCountByZones
    {
        public string? ZoneName { get; set; }
        public int PeopleInCount { get; set; }
        public int PeopleOutCount { get; set; }
    }

    public class PeopleCountByDevice
    {
        public string DeviceId { get; set; }
        public int PeopleInCount { get; set; }
        public int PeopelOutCount { get; set; }
    }

    public class EventQueueAnalysis
    {
        public DateTime DateTime { get; set; }
        public string? DeviceId { get; set; }
        public double QueueCount { get; set; }
    }

    public class MaskDetectionAnalysis
    {
        public DateTime DateTime { get; set; }
        public int MaskCount { get; set; }
        public int WithoutMaskCount { get; set; }
    }

    public class VehicleParking
    {
        public string ZoneName { get; set; }
        public int TotalParkingOccupancy { get; set; }
        public int ParkingCount { get; set; }
    }

    public class DeviceExceptionResponse
    {
        public List<DeviceException> PeopleCountDevices { get; set; }
        public List<DeviceException> VehicleCountDevices { get; set; }
        public List<DeviceException> ShoppingCartCountDevices { get; set; }
        public List<DeviceException> ForkliftCountDevices { get; set; }
        public List<DeviceException> HeatmapDevices { get; set; }
        public List<DeviceException> MultilaneVehicleCountDevices { get; set; }
        public List<DeviceException> DeviceEvents { get; set; }
    }
    public class StoppedVehicleByTypeData
    {
        public DateTime DateTime { get; set; }
        public int Car { get; set; }
        public int Bus { get; set; }
        public int Truck { get; set; }
        public int Motorcycle { get; set; }
        public int Bicycle { get; set; }
    }

    public class CountAnalysisData
    {
        public DateTime DateTime { get; set; }
        public int Count { get; set; }
    }

    public class ChartAvgInOut
    {
        public DateTime? DateTime { get; set; }
        public int NewVisitor { get; set; }
        public int TotalVisitor { get; set; }
    }

    public class ChartPoint
    {
        public string Date { get; set; }
        //public string X { get; set; }
        public int Y { get; set; }
    }

    public class DeviceData
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
    }

    public class VehicleTurningMovementResponse
    {
        public DateTime? DateTime { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public int Straight { get; set; }
    }

    public class WidgetRequestDevice
    {
        public IEnumerable<string>? userRoles { get; set; }
        public IEnumerable<string>? FloorIds { get; set; }
        public IEnumerable<string>? ZoneIds { get; set; }
    }

    public class CsvResponseModel
    {
        public string DateTime { get; set; }
        public int Value { get; set; }
    }

    public class CsvInOutResponseModel
    {
        public DateTime? DateTime { get; set; }
        public double InCount { get; set; }
        public double OutCount { get; set; }
    }

    public class CsvRmaMaintenanceResponseModel
    {
        public DateTime? DateTime { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
    }

    public class CameraStreamRequest
    {
        public string CameraUrl { get; set; }
    }
}