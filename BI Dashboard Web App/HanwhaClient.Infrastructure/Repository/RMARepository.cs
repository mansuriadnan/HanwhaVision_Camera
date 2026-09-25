using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace HanwhaClient.Infrastructure.Repository
{
    public class RMARepository : RepositoryBase<Rma>, IRMARepository
    {
        public RMARepository(MongoDbConnectionService mongoDbConnectionService)
            : base(mongoDbConnectionService, AppDBConstants.RMA)
        {
        }

        public async Task<(IEnumerable<Rma> Data, int TotalCount)> GetAllRMA(RMASerachModel model)
        {
            // ---- Validate / normalize paging ----
            var pageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
            var pageSize = model.PageSize <= 0 ? 10 : model.PageSize;
            var skip = (pageNumber - 1) * pageSize;

            // ---- Whitelist & map sort fields (prevent injection) ----
            // Map C# property names (PascalCase) to Mongo field names (camelCase)
            var sortMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "RMAStatus",  "rmaStatus" },
                            { "CompletedNotes",  "completedNotes" },
                            { "InProgressNotes",  "inProgressNotes" },
                            { "CreatedOn", "createdOn" },
                            { "UpdatedOn", "updatedOn" },
                            { "StartDate", "startDate" },
                            { "EndDate",  "endDate" }
                        };

            // If model.SortBy is null/empty or not allowed, default to createdOn
            var sortByField = "createdOn";
            if (!string.IsNullOrWhiteSpace(model.SortBy) && sortMap.TryGetValue(model.SortBy, out var mapped))
            {
                sortByField = mapped;
            }

            // Only accept 1 (asc) or -1 (desc); default to -1 (desc)
            var sortOrder = (model.SortOrder == 1 || model.SortOrder == -1) ? model.SortOrder.Value : -1;

            // ---- Build pipeline ----
            var pipeline = new List<BsonDocument>();

            pipeline.Add(new BsonDocument("$match", new BsonDocument("isDeleted", false)));

            if (model.DeviceIds != null && model.DeviceIds.Any())
            {
                IEnumerable<ObjectId> deviceObjectIds;

                if (model.DeviceIds is IEnumerable<string> idsStr)
                {
                    var parsed = new List<ObjectId>();
                    foreach (var id in idsStr)
                    {
                        if (ObjectId.TryParse(id, out var oid))
                            parsed.Add(oid);
                    }
                    deviceObjectIds = parsed;
                }
                else
                {
                    deviceObjectIds = Enumerable.Empty<ObjectId>();
                }


                // deviceId filter (only if we have valid ObjectIds)
                if (deviceObjectIds.Any())
                {
                    pipeline.Add(
                        new BsonDocument("$match",
                            new BsonDocument("deviceId",
                                new BsonDocument("$in", new BsonArray(deviceObjectIds))))
                    );
                }
            }

            //if (DateTimeOffset.TryParse(model.StartDate, out var dto))
            //{
            //    var startUtc = dto.UtcDateTime.Date; // floor to UTC calendar day
            //    var endExclusive = startUtc.AddDays(1);

            //    // Match a full day
            //    pipeline.Add(new BsonDocument("$match",
            //        new BsonDocument("startDate",
            //            new BsonDocument
            //            {
            //    { "$gte", startUtc },
            //    { "$lt", endExclusive }
            //            })));
            //}
            if (!string.IsNullOrEmpty(model.StartDate))
            {
                var dateFilter = DateTime.Parse(
                    model.StartDate,
                    null,
                    System.Globalization.DateTimeStyles.AdjustToUniversal
                );

                var toDateFilter = dateFilter.AddDays(1).AddTicks(-1);               

                var rangeMatchStage = new BsonDocument("$match",
                    new BsonDocument("$and", new BsonArray
                    {
                        // fromDate <= searchTo
                        new BsonDocument("startDate", new BsonDocument("$lte", toDateFilter)),

                        // toDate >= searchFrom OR toDate is null/missing
                        new BsonDocument("$or", new BsonArray
                        {
                            new BsonDocument("endDate", new BsonDocument("$gte", dateFilter)),
                            new BsonDocument("endDate", BsonNull.Value),
                            new BsonDocument("endDate", new BsonDocument("$exists", false))
                        })
                    })
                );

                pipeline.Add(rangeMatchStage);


                //pipeline.Add(dateMatchStage);
            }



            if (!string.IsNullOrWhiteSpace(model.RMAStatus))
            {
                var searchTerm = model.RMAStatus.Trim();

                var matchStage = new BsonDocument("$match",
                    new BsonDocument("rmaStatus",
                        new BsonDocument
                        {
                { "$regex", Regex.Escape(searchTerm) },
                { "$options", "i" } // case-insensitive
                        }));

                pipeline.Add(matchStage);
            }

            // FACET with data (paged) + totalCount (via $count)
            var facet = new BsonDocument("$facet", new BsonDocument
            {
                {
                    "data",
                    new BsonArray
                    {
                        new BsonDocument("$sort", new BsonDocument(sortByField, sortOrder)),
                        new BsonDocument("$skip", skip),
                        new BsonDocument("$limit", pageSize),
                        new BsonDocument("$project", new BsonDocument
                        {
                            { "_id", 1 },
                            { "rmaStatus", 1 },
                            { "deviceId", 1 },
                            { "inProgressNotes", 1 },
                            { "completedNotes", 1 },
                            { "floorId", 1 },
                            { "zoneId", 1 },
                            { "createdOn", 1 },
                            { "updatedOn", 1 },
                            { "startDate", 1 },
                            { "endDate", 1 }
                        })
                    }
                },
                {
                    "totalCount",
                    new BsonArray
                    {
                        new BsonDocument("$count", "count")
                    }
                }
            });

            pipeline.Add(facet);

            // Collation makes sorting case-insensitive and locale-aware
            var options = new AggregateOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary)
            };

            // Execute aggregation (dbEntity is your collection)
            try
            {
                var result = await dbEntity.Aggregate<BsonDocument>(pipeline, options).FirstOrDefaultAsync();

                // ---- Parse facet result safely ----
                var data = new List<Rma>();
                int totalCount = 0;

                if (result != null)
                {
                    var dataArray = result.GetValue("data", new BsonArray()).AsBsonArray;
                    var countArray = result.GetValue("totalCount", new BsonArray()).AsBsonArray;

                    data = dataArray
                        .Select(d => BsonSerializer.Deserialize<Rma>(d.AsBsonDocument))
                        .ToList();

                    totalCount = countArray.Any() ? countArray[0]["count"].ToInt32() : 0;
                }

                return (data, totalCount);
            }

            catch (Exception ex)
            {
                var exMsg = ex.Message;
            }
            return (new List<Rma>(), 0);
        }

        public async Task<IEnumerable<string>> GetRMAByDeviceIds(IEnumerable<string> deviceIds)
        {
            if (deviceIds == null || !deviceIds.Any())
                return Enumerable.Empty<string>();

            var builder = Builders<Rma>.Filter;

            var filter =
                builder.Eq(x => x.IsDeleted, false) &
                builder.In(x => x.DeviceId, deviceIds);

            var rmaIds = await dbEntity
                .Find(filter)
                .Project(p => p.Id)
                .ToListAsync();

            return rmaIds.Select(id => id.ToString());
        }

        public async Task<IEnumerable<RMAWidgetResponse>> GetRmaMaintenanceWidgetData(RMAWidgetRequest model)
        {
            var filters = new List<FilterDefinition<Rma>>();

            filters.Add(Builders<Rma>.Filter.Eq(x => x.IsDeleted, false));
            if (model.DeviceIds != null && model.DeviceIds.Any())
            {
                filters.Add(Builders<Rma>.Filter.In(x => x.DeviceId, model.DeviceIds));
            }
            if (model.StartDate != null && model.EndDate != null)
            {
                filters.Add(
                    Builders<Rma>.Filter.And(
                        Builders<Rma>.Filter.Lte(x => x.StartDate, model.EndDate),
                        Builders<Rma>.Filter.Or(
                            Builders<Rma>.Filter.Gte(x => x.EndDate, model.StartDate),
                            Builders<Rma>.Filter.Eq(x => x.EndDate, null)
                        )
                    )
                );
            }

            var filter = Builders<Rma>.Filter.And(filters);

            var docs = await dbEntity.Find(filter).ToListAsync();
            await QueryDataFromLinkedServers(filter, docs);

            return docs.Select(x => new RMAWidgetResponse
            {
                DeviceId = x.DeviceId,
                Id = x.Id,
                EndDate = x.EndDate,
                RMAStatus = x.RMAStatus,
                StartDate = x.StartDate
            }); 
            
            
        }
    }
}