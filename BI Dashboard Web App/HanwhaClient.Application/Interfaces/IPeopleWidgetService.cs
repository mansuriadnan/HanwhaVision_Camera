using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.PeopleWidget;

namespace HanwhaClient.Application.Interfaces
{
    public interface IPeopleWidgetService
    {
        Task<IEnumerable<GenderWisePeopleCounting>> GenderWisePeopleCounting(WidgetRequest widgetRequest);
        Task<(IEnumerable<CameraCapacityUtilizationByZones>, UtilizationMostLeastDay)> PeopleCameraCapacityUtilizationByZoneAsync(WidgetRequest widgetRequest);
        Task<CapacityUtilization> PeopleCapacityUtilizationAsync(WidgetRequest widgetRequest);
        Task<IEnumerable<CameraCapacityUtilizationAnalysisByZones>> PeopleCameraCapacityUtilizationAnalysisByZones(WidgetRequest widgetRequest);
        Task<NewVsTotalVisitorCountWidget> NewVsTotalVisitorCountAsync(WidgetRequest widgetRequest);
        Task<IEnumerable<PeopleCountByZones>> PeopleCountByZones(WidgetRequest widgetRequest);
        Task<IEnumerable<GenderWisePeopleAnalysisCount>> GenderWisePeopleCountAnalysisData(WidgetRequest widgetRequest);
        Task<IEnumerable<AgeWisePeopleAnalysisCount>> AgeWisePeopleCountAnalysisData(WidgetRequest widgetRequest);
        //Task<List<PeopleVehicleInOutAvgChart>> PeopleInOutCountAnalysisAsync(WidgetRequest widgetRequest);
        Task<PeopleVehicleInOutTotal> PeopleIOnOutTotalV2Async(WidgetRequest widgetRequest);
        Task<IEnumerable<PeopleVehicleInOutAvgChart>> PeopleInOutCountAnalysisV2Async(WidgetRequest widgetRequest, bool isCumulative = false);
        Task<List<ChartAvgInOut>> NewVsTotalVisitorChartAsync(WidgetRequest widgetRequest);
        Task<int> ProcessPeopleRetentionData(int retentionPeriod);
    }
}
