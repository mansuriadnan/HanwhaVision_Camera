using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IHeatMapRepository : IRepositoryBase<HeatMap>, IRetentionRepository<HeatMap>
    {
        Task<IEnumerable<HeatMap>> HeatMapWidgetDataAsync(WidgetHeatmapRequest widgetRequest, bool isArchived);
        Task<IEnumerable<string>> GetCameraListByHeatmapTypeAsync(string heatmapType);
    }
}
