using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public class MaintenanceScheduleRepository : RepositoryBase<MaintenanceSchedule>, IMaintenanceScheduleRepository
    {
        public MaintenanceScheduleRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.MaintenanceSchedule)
        {
        }


        public async Task<(IEnumerable<MaintenanceSchedule> Data, int TotalCount)> GetAllMaintenanceSchedule(MaintenanceScheduleSerachModel model)
        {
            // ---- Validate / normalize paging ----
            var pageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
            var pageSize = model.PageSize <= 0 ? 10 : model.PageSize;
            var skip = (pageNumber - 1) * pageSize;

            // ---- Whitelist & map sort fields (prevent injection) ----
            // Make sure these match your Mongo field names exactly (likely camelCase)

            if (model.SortBy == "maintenanceDueDate")
            {
                model.SortBy = "dueDate";
            }
            if (model.SortBy == "status")
            {
                model.SortBy = "latestStatus";
            }
            var sortMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                           {
                               { "dueDate",       "dueDate" },
                               { "latestStatus",  "latestStatus" },
                               { "createdOn",     "createdOn" }, // use camelCase to match Mongo
                               { "updatedOn",     "updatedOn" },
                               { "status",        "status" }
                           };

            // If SortBy not allowed → default to createdOn
            var sortByField = "createdOn";
            if (!string.IsNullOrWhiteSpace(model.SortBy) && sortMap.TryGetValue(model.SortBy, out var mapped))
            {
                sortByField = mapped;
            }

            // Only accept 1 (asc) or -1 (desc); default to -1 (desc)
            var sortOrder = (model.SortOrder == 1 || model.SortOrder == -1) ? model.SortOrder.Value : -1;

            // ---- Build MATCH filter based on LatestStatus and DueDate ----
            var match = new BsonDocument();
            var andClauses = new BsonArray();

            // LatestStatus (single)
            if (!string.IsNullOrWhiteSpace(model.StatusFilter))
            {
                andClauses.Add(new BsonDocument("latestStatus", model.StatusFilter));
            }


            if (model.DeviceIds != null && model.DeviceIds.Any())
            {
                IEnumerable<ObjectId> deviceObjectIds;

                // If you already have ObjectIds
                if (model.DeviceIds is IEnumerable<ObjectId> idsObj)
                {
                    deviceObjectIds = idsObj;
                }
                // If they are strings, try to parse them to ObjectId
                else if (model.DeviceIds is IEnumerable<string> idsStr)
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

                // Only add the filter if we have valid ObjectIds
                if (deviceObjectIds.Any())
                {
                    andClauses.Add(
                        new BsonDocument("deviceId",
                            new BsonDocument("$in", new BsonArray(deviceObjectIds)))
                    );
                }
            }


            // DueDate range
            //DateTime? targetDueDate = null;
            //var startDate = new DateTime();
            //if (!string.IsNullOrWhiteSpace(model.DueFilter) && model.DueFilter != "-1")
            //{
            //    if (int.TryParse(model.DueFilter, out int dueDays))
            //    {
            //        startDate = model.DueFilter == "0" ? DateTime.UtcNow.Date : DateTime.UtcNow.Date.AddDays(1);
            //        targetDueDate = model.DueFilter == "0" ? startDate.AddDays(dueDays + 1) : startDate.AddDays(dueDays);
            //    }
            //}


            //if (targetDueDate.HasValue)
            //{
            //    var startOfDay = startDate;
            //    var endOfDay = targetDueDate;

            //    andClauses.Add(new BsonDocument("dueDate",
            //        new BsonDocument
            //        {
            //{ "$gte", startOfDay },
            //{ "$lte", endOfDay }
            //        }));
            //}

            // DueDate filter
            if (!string.IsNullOrWhiteSpace(model.DueFilter) &&
                model.DueFilter != "-1" &&
                int.TryParse(model.DueFilter, out int dueDays))
            {
                var today = DateTime.UtcNow.Date;

                BsonDocument dueDateFilter;

                // 0 => all past due + today
                if (dueDays == 0)
                {
                    var endOfToday = today.AddDays(1).AddTicks(-1);

                    dueDateFilter = new BsonDocument
                    {
                        { "$lte", endOfToday }
                    };
                }
                else
                {
                    // 1 => today + tomorrow
                    // 7 => next 7 days including today

                    var startDate = today.AddDays(1); // tomorrow start

                    var endDate = today
                        .AddDays(dueDays + 1)
                        .AddTicks(-1);

                    dueDateFilter = new BsonDocument
                    {
                        { "$gte", startDate },
                        { "$lte", endDate }
                    };
                }

                andClauses.Add(
                    new BsonDocument("dueDate", dueDateFilter)
                );
            }


            if (andClauses.Count > 0)
            {
                match = new BsonDocument("$match", new BsonDocument("$and", andClauses));
            }

            // ---- Build pipeline ----
            var pipeline = new List<BsonDocument>();

            pipeline.Add(new BsonDocument("$match", new BsonDocument("isDeleted", false)));

            // Add $match only if filters exist
            if (andClauses.Count > 0)
            {
                pipeline.Add(match);
            }

            //pipeline.Add(new BsonDocument("$match",
            //    new BsonDocument("dueDate", targetDueDate.Value)));


            // FACET with data (paged) + totalCount (via $count)
            var facet = new BsonDocument("$facet", new BsonDocument
                        {
                            {
                                "data",
                                new BsonArray
                                {
                                    new BsonDocument("$sort", new BsonDocument(sortByField, sortOrder)),
                                    new BsonDocument("$skip", skip),
                                    new BsonDocument("$limit", pageSize)
                                    // ❌ No $project — we want ALL columns
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


            // Collation makes sorting case-insensitive and locale-aware (for string sorts)
            var options = new AggregateOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary)
            };

            // Execute aggregation
            var result = await dbEntity.Aggregate<BsonDocument>(pipeline, options).FirstOrDefaultAsync();

            // ---- Parse facet result safely ----
            var data = new List<MaintenanceSchedule>();
            int totalCount = 0;

            if (result != null)
            {
                var dataArray = result.GetValue("data", new BsonArray()).AsBsonArray;
                var countArray = result.GetValue("totalCount", new BsonArray()).AsBsonArray;

                data = dataArray
                    .Select(d => BsonSerializer.Deserialize<MaintenanceSchedule>(d.AsBsonDocument))
                    .ToList();

                totalCount = countArray.Any() ? countArray[0]["count"].ToInt32() : 0;
            }

            return (data, totalCount);

        }

        public async Task<IEnumerable<MaintenanceSchedule>> GetDueMaintenanceScheduleDevices()
        {
            DateTime localDate = DateTime.Now.Date.AddDays(1);


            var startDate = localDate.ToUniversalTime();
            var endDate = startDate.AddDays(1).AddTicks(-1);

            var filter = Builders<MaintenanceSchedule>.Filter.And(
                Builders<MaintenanceSchedule>.Filter.Gte(x => x.DueDate, startDate),
                Builders<MaintenanceSchedule>.Filter.Lt(x => x.DueDate, endDate),
                Builders<MaintenanceSchedule>.Filter.Eq(x => x.LatestStatus, "Not Started"),
                Builders<MaintenanceSchedule>.Filter.Eq(x => x.IsDeleted, false)
            );

            var data = await dbEntity.Find(filter).ToListAsync();
            return data;
        }

        public async Task<IEnumerable<MaintenanceSchedule>> GetMaintenanceScheduleForWidget(RMAWidgetRequest widgetRequest)
        {
            var filter = Builders<MaintenanceSchedule>.Filter.And(
                Builders<MaintenanceSchedule>.Filter.In(x => x.DeviceId, widgetRequest.DeviceIds),
                Builders<MaintenanceSchedule>.Filter.Eq(x => x.IsDeleted, false),
                Builders<MaintenanceSchedule>.Filter.ElemMatch(
                    x => x.StatusHistory,
                    sh => sh.StatusDatetime >= widgetRequest.StartDate &&
                    sh.StatusDatetime <= widgetRequest.EndDate
                )
            );

            var data = await dbEntity
                .Find(filter)
                .ToListAsync();

            await QueryDataFromLinkedServers(filter, data);
            //Remove status from StatusHistory which are not in range
            foreach (var schedule in data)
            {
                schedule.StatusHistory = schedule.StatusHistory
                    .Where(sh =>
                        sh.StatusDatetime.HasValue &&
                        sh.StatusDatetime.Value >= widgetRequest.StartDate &&
                        sh.StatusDatetime.Value <= widgetRequest.EndDate
                    )
                    .ToList();
            }
            return data;
        }

        public async Task<IEnumerable<MaintenanceSchedule>> GetCameraInMaintenance(CameraInMaintenanceSearchDto obj)
        {
            var validStatuses = new[] { "In Progress", "Rework" };

            // Date-only range
            var startDate = obj.StartDate;
            var endDateExclusive = obj.EndDate;

            // 1) Optional device filter
            FilterDefinition<MaintenanceSchedule> deviceFilter = FilterDefinition<MaintenanceSchedule>.Empty;
            if (obj.DeviceId != null && obj.DeviceId.Any())
            {
                deviceFilter = Builders<MaintenanceSchedule>.Filter.In(x => x.DeviceId, obj.DeviceId);
            }

            // 2) Match documents that have at least one valid history entry
            var historyMatchFilter = Builders<MaintenanceSchedule>.Filter.ElemMatch(
                ms => ms.StatusHistory,
                sh =>
                    validStatuses.Contains(sh.Status) &&
                    sh.StatusDatetime >= startDate &&
                    sh.StatusDatetime <= endDateExclusive
            );

            var matchFilter = Builders<MaintenanceSchedule>.Filter.And(
                deviceFilter,
                historyMatchFilter
            );

            var docs = await dbEntity.Find(matchFilter).ToListAsync();
            await QueryDataFromLinkedServers(matchFilter, docs);

            // 3) Aggregation to RETURN ONLY MATCHED HISTORY
            var result = new List<MaintenanceSchedule>();
            try
            {
                result = docs.Select(ms => new MaintenanceSchedule
                {
                    Id = ms.Id,
                    DeviceId = ms.DeviceId,
                    LatestStatus = ms.LatestStatus,
                    IsScheduleManually = ms.IsScheduleManually,
                    StatusHistory = ms.StatusHistory?
                        .Where(sh =>
                            validStatuses.Contains(sh.Status) &&
                            sh.StatusDatetime >= startDate &&
                            sh.StatusDatetime <= endDateExclusive)
                        .ToList()
                }).ToList();

            }
            catch (Exception ex)
            {
                var ex1 = ex.Message;
                return result;
            }

            return result;
        }
        public async Task<bool> CheckMaintenanceDeviceExits(string deviceId)
        {
            var filter = Builders<MaintenanceSchedule>.Filter.And(
                Builders<MaintenanceSchedule>.Filter.Eq(x => x.DeviceId, deviceId),
                Builders<MaintenanceSchedule>.Filter.Ne(x => x.LatestStatus, "Done"),
                Builders<MaintenanceSchedule>.Filter.Eq(x => x.IsDeleted, false)
            );

            var data = await dbEntity.Find(filter).AnyAsync();
            return data;
        }

        public async Task<IEnumerable<string>> GetmaintenanceScheduleByDeviceIds(IEnumerable<string> deviceIds)
        {
            if (deviceIds == null || !deviceIds.Any())
                return Enumerable.Empty<string>();

            var builder = Builders<MaintenanceSchedule>.Filter;

            var filter =
                builder.Eq(x => x.IsDeleted, false) &
                builder.In(x => x.DeviceId, deviceIds);

            var planIds = await dbEntity
                .Find(filter)
                .Project(p => p.Id)
                .ToListAsync();

            return planIds.Select(id => id.ToString());
        }

    }
}
