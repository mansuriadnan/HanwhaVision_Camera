using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.DbEntities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface ISsmDeviceStoppedRecordingRepository : IRepositoryBase<SsmDeviceStoppedRecording>
    {
        Task<List<SsmDeviceStoppedRecording>> GetStoppedRecordingHistoryByServerIdsAsync(List<string> serverIds, DateTime startOfDayUTC, DateTime endOfDayUTC);
    }
}
