using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public class ShoppingCartCountRepository : RepositoryBase<ShoppingCartCount>, IShoppingCartCountRepository
    {
        public ShoppingCartCountRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.ShoppingCartCount, AppDBConstants.ShoppingCartCountArchive)
        {
        }

        public async Task<IEnumerable<EventQueueAnalysis>> ShoppingCartCountAnalysisData(string deviceId, DateTime startdate, DateTime enddate, int channel, int intervalMinute, bool isArchived)
        {
            var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "deviceId", new ObjectId(deviceId) },
                    { "createdOn", new BsonDocument {
                        { "$gte", startdate },
                        { "$lte", enddate }
                    }},
                    { "channelNo", channel }
                }),
                new BsonDocument("$unwind", "$lines"), // Important: Flatten Lines array
                new BsonDocument("$project", new BsonDocument
                {
                    { "inCount", "$lines.inCount" }, // Extract inCount
                    { "createdOn", 1 },
                    { "bucketTime", new BsonDocument("$add", new BsonArray
                        {
                            BsonValue.Create(startdate),
                            new BsonDocument("$multiply", new BsonArray
                            {
                                new BsonDocument("$floor", new BsonDocument("$divide", new BsonArray
                                {
                                    new BsonDocument("$subtract", new BsonArray
                                    {
                                        "$createdOn", BsonValue.Create(startdate)
                                    }),
                                    1000 * 60 * intervalMinute // 10-minute interval
                                })),
                                1000 * 60 * intervalMinute
                            })
                        })
                    }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument
                        {
                            { "bucketTime", "$bucketTime" },
                            { "lineIndex", "$lines.lineIndex" } // Group by interval and line
                        }
                    },
                    { "maxInCountPerLine", new BsonDocument("$max", "$inCount") }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$_id.bucketTime" }, // Group only by bucketTime now
                    { "MaxCount", new BsonDocument("$sum", "$maxInCountPerLine") } // Sum all max inCounts
                }),
                new BsonDocument("$sort", new BsonDocument("_id", 1))
            };

            var pipelineData = await dbEntity.Aggregate<BsonDocument>(pipeline).ToListAsync();
            await QueryDataFromLinkedServersByPipeline(pipeline, pipelineData); 
            if (isArchived) // check is archive is enable// by single tansevice
            {
                // is seach range is in archive ranage
                if (dbEntityArchive != null)
                {
                    //null check if required
                    var archiveData = await dbEntityArchive.Aggregate<BsonDocument>(pipeline).ToListAsync();
                    pipelineData.AddRange(archiveData);
                }
            }
            var result = pipelineData.Select(g => new EventQueueAnalysis
            {
                DateTime = g["_id"].ToUniversalTime(),
                QueueCount = g["MaxCount"].ToInt32(),
                DeviceId = deviceId,
            }).ToList();
            return result;
        }
    }
}
