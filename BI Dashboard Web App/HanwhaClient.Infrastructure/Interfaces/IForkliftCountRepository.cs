using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IForkliftCountRepository : IRepositoryBase<ForkliftCount>,IRetentionRepository<ForkliftCount>
    {
        Task<IEnumerable<EventQueueAnalysis>> ForkliftCountAnalysisDataAsync(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute, bool IsArchived);
        //Task<IEnumerable<ForkliftCount>> GetRetentionPeriodData(int retentionPeriod, int batchSize);
    }
}
