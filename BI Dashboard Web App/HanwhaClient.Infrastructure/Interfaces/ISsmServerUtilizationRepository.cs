using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.DbEntities;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface ISsmServerUtilizationRepository : IRepositoryBase<SsmServerUtilization>
    {
        Task<List<SsmServerUtilization>> GetByServerIdsAsync(List<string> serverIds, DateTime startOfDayUTC, DateTime endOfDayUTC);
    }
}
