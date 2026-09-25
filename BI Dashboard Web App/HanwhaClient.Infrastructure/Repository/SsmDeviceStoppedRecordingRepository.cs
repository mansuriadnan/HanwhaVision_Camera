using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Repository
{
    public class SsmDeviceStoppedRecordingRepository : RepositoryBase<SsmDeviceStoppedRecording>, ISsmDeviceStoppedRecordingRepository
    {
        public SsmDeviceStoppedRecordingRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.SsmDeviceStoppedRecording)
        {
        }

        public async Task<List<SsmDeviceStoppedRecording>> GetStoppedRecordingHistoryByServerIdsAsync(List<string> serverIds, DateTime startOfDayUTC, DateTime endOfDayUTC)
        {
            var filter = Builders<SsmDeviceStoppedRecording>.Filter.And(
                Builders<SsmDeviceStoppedRecording>.Filter.In(x => x.ServerId, serverIds),
                Builders<SsmDeviceStoppedRecording>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SsmDeviceStoppedRecording>.Filter.Lte(x => x.StopRecordingTime, endOfDayUTC),
                Builders<SsmDeviceStoppedRecording>.Filter.Or(
                    Builders<SsmDeviceStoppedRecording>.Filter.Eq(x => x.StartRecordingTime, null),
                    Builders<SsmDeviceStoppedRecording>.Filter.Gte(x => x.StartRecordingTime, startOfDayUTC)
                )
            );

            var data = await dbEntity.Find(filter).SortBy(x => x.StopRecordingTime).ToListAsync();
            return data;
        }
    }
}
