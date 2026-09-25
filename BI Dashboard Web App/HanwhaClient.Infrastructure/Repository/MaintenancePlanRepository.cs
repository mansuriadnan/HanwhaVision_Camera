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
    public class MaintenancePlanRepository : RepositoryBase<MaintenancePlan>, IMaintenancePlanRepository
    {
        public MaintenancePlanRepository(MongoDbConnectionService mongoDbConnectionService)
            : base(mongoDbConnectionService, AppDBConstants.MaintenancePlan)
        {
        }

        public async Task<IEnumerable<string>> GetAllMaintenancePlanIdsAsync(IEnumerable<string> deviceIds)
        {
            // Safely convert string IDs to ObjectId
            var objectIds = deviceIds
                .Select(s =>
                {
                    // TryParse with an out var; return nullable to keep pipeline pure
                    if (ObjectId.TryParse(s, out var oid))
                        return (ObjectId?)oid;
                    return null;
                })
                .Where(oid => oid.HasValue)
                .Select(oid => oid!.Value)
                .ToList();

            if (objectIds.Count == 0)
                return Enumerable.Empty<string>();

            var builder = Builders<MaintenancePlan>.Filter;

            // NOTE: If you want active plans, change IsDeleted to false
            var filter = builder.Eq(x => x.IsDeleted, false) &
                         builder.AnyIn(x => x.DeviceIds, objectIds);

            var planIds = await dbEntity
                .Find(filter)
                .Project(p => p.Id) // Project ObjectId
                .ToListAsync();

            return planIds.Select(id => id.ToString());
        }
        public async Task<(IEnumerable<MaintenancePlan> Data, int TotalCount)> GetAllMaintenancePlan(MaintenancePlanSerachModel model, List<string> filteredDeviceIds)
        {
            // ---- Validate / normalize paging ----
            var pageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
            var pageSize = model.PageSize <= 0 ? 10 : model.PageSize;
            var skip = (pageNumber - 1) * pageSize;

            // ---- Whitelist & map sort fields (prevent injection) ----
            // Map C# property names (PascalCase) to Mongo field names (camelCase)
            var sortMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "PlanName",  "planName" },
                            { "CreatedOn", "createdOn" },
                            { "UpdatedOn", "updatedOn" },
                            { "Status",    "status" }
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

            //pipeline.Add(new BsonDocument("$match", new BsonDocument("isDeleted", false)));
            //// Optional MATCH (case-insensitive regex on planName)
            //if (!string.IsNullOrWhiteSpace(model.SearchText))
            //{
            //    var term = model.SearchText.Trim();
            //    // Escape regex special chars to avoid unintended patterns
            //    var escaped = Regex.Escape(term);

            //    pipeline.Add(new BsonDocument("$match",
            //        new BsonDocument("planName",
            //            new BsonDocument("$regex", escaped)
            //                .Add("$options", "i")))); // case-insensitive
            //}
            var orConditions = new BsonArray();
            var matchFilter = new BsonDocument
            {
                { "isDeleted", false }
            };

            // planName OR condition
            if (!string.IsNullOrWhiteSpace(model.SearchText))
            {
                var escaped = Regex.Escape(model.SearchText.Trim());
                orConditions.Add(
                    new BsonDocument("planName",
                        new BsonDocument("$regex", escaped)
                            .Add("$options", "i"))
                );
            }

            // deviceIds OR condition
            if (filteredDeviceIds?.Any() == true)
            {
                var deviceObjectIds = filteredDeviceIds
                    .Where(id => ObjectId.TryParse(id, out _))
                    .Select(ObjectId.Parse)
                    .ToList();

                if (deviceObjectIds.Any())
                {
                    orConditions.Add(
                        new BsonDocument("deviceIds",
                            new BsonDocument("$in", new BsonArray(deviceObjectIds)))
                    );
                }
            }

            // add OR only if at least one condition exists
            if (orConditions.Count > 0)
            {
                matchFilter.Add("$or", orConditions);
            }

            pipeline.Add(new BsonDocument("$match", matchFilter));


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
                            { "planName", 1 },
                            { "status", 1 },
                            { "deviceIds", 1 },
                            { "floorIds", 1 },
                            { "zoneIds", 1 },
                            { "createdOn", 1 },
                            { "updatedOn", 1 },
                            { "notes", 1 },
                            { "startDate", 1 },
                            { "duration", 1 },
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
            var result = await dbEntity.Aggregate<BsonDocument>(pipeline, options).FirstOrDefaultAsync();

            // ---- Parse facet result safely ----
            var data = new List<MaintenancePlan>();
            int totalCount = 0;

            if (result != null)
            {
                var dataArray = result.GetValue("data", new BsonArray()).AsBsonArray;
                var countArray = result.GetValue("totalCount", new BsonArray()).AsBsonArray;

                data = dataArray
                    .Select(d => BsonSerializer.Deserialize<MaintenancePlan>(d.AsBsonDocument))
                    .ToList();

                totalCount = countArray.Any() ? countArray[0]["count"].ToInt32() : 0;
            }

            return (data, totalCount);
        }

        public async Task<IEnumerable<MaintenancePlan>> GetMaintenancePlanData()
        {
            var builder = Builders<MaintenancePlan>.Filter;
            DateTime now = DateTime.UtcNow;

            var filter =
                builder.Ne(x => x.IsDeleted, true) &
                builder.Lte(x => x.StartDate, now) &
                builder.Gte(x => x.EndDate, now);

            var data = await dbEntity.Find(filter).ToListAsync();
            return data;
        }

        public async Task<bool> IsMaintenancePlanExistsAsync(string maintenancePlanName, string? maintenancePlanId)
        {
            var filters = new List<FilterDefinition<MaintenancePlan>>();
            filters.Add(Builders<MaintenancePlan>.Filter.Regex("planName", new BsonRegularExpression($"^{Regex.Escape(maintenancePlanName)}$", "i")));
            filters.Add(Builders<MaintenancePlan>.Filter.Eq(x => x.IsDeleted, false));

            if (!string.IsNullOrEmpty(maintenancePlanId))
            {
                filters.Add(Builders<MaintenancePlan>.Filter.Ne(x => x.Id, maintenancePlanId));
            }
            var filter = Builders<MaintenancePlan>.Filter.And(filters);
            var data = await dbEntity.Find(filter).AnyAsync();
            return data;
        }
        public async Task<long> DeleteMaintenancePlanDeviceIdsAsync(IEnumerable<string> deviceIds, IEnumerable<string> planIds, string userId)
        {
            if (planIds == null || !planIds.Any())
                return 0;
            var deviceObjectIds = new List<ObjectId>();
            foreach (var id in deviceIds)
            {
                if (ObjectId.TryParse(id, out var objectId))
                    deviceObjectIds.Add(objectId);
            }
            var filter = Builders<MaintenancePlan>.Filter.And(
                    Builders<MaintenancePlan>.Filter.In(x => x.Id, planIds),
                    Builders<MaintenancePlan>.Filter.Eq(x => x.IsDeleted, false)
                    );

            var update = Builders<MaintenancePlan>.Update
                .PullAll("deviceIds", deviceObjectIds)   
                .Set(x => x.UpdatedOn, DateTime.UtcNow)
                .Set(x => x.UpdatedBy, userId);

            var result = await dbEntity.UpdateManyAsync(filter, update);

            var softDeleteFilter = Builders<MaintenancePlan>.Filter.And(
                Builders<MaintenancePlan>.Filter.In(x => x.Id, planIds),
                Builders<MaintenancePlan>.Filter.Size("deviceIds", 0),
                Builders<MaintenancePlan>.Filter.Eq(x => x.IsDeleted, false)
                );
            
            var softDeleteUpdate = Builders<MaintenancePlan>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.UpdatedOn, DateTime.UtcNow)
                .Set(x => x.UpdatedBy, userId);

            await dbEntity.UpdateManyAsync(softDeleteFilter, softDeleteUpdate);
            return result.ModifiedCount;
        }
    }
}