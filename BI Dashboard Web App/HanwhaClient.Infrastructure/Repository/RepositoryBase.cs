using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public abstract class RepositoryBase<T>
    : IRepositoryBase<T>, IRetentionRepository<T>, IDisposable
    where T : BaseModel
    {
        protected readonly IMongoCollection<T> dbEntity;
        protected readonly IMongoCollection<T> dbEntityArchive;
        protected readonly IMongoCollection<AuditLog> dbAuditEntity;
        private IChangeStreamCursor<ChangeStreamDocument<T>> cursor;
        private string collectionName;
        MongoDbConnectionService mongoDbConnectionService;

        public RepositoryBase(MongoDbConnectionService mongoDbConnectionService, string collectionName, string archiveCollectionName = "")
        {
            dbEntity = mongoDbConnectionService.Database?.GetCollection<T>(collectionName);
            if (!string.IsNullOrEmpty(archiveCollectionName))
            {
                dbEntityArchive = mongoDbConnectionService.Database?.GetCollection<T>(archiveCollectionName);
            }
            //if (ChangeStreamLog)
            //{
            //    dbAuditEntity = mongoDbConnectionService.Database?.GetCollection<AuditLog>("auditLog");
            //    StartChangeStreamAsync();
            //}
            this.collectionName = collectionName;
            this.mongoDbConnectionService = mongoDbConnectionService;
        }


        public async Task<IEnumerable<T>> GetAllFromLinkedServerAsync(ProjectionDefinition<T> projection = null, bool includeDeleted = false)
        {
            FilterDefinition<T> filter;
            if (includeDeleted)
            {
                filter = Builders<T>.Filter.Empty;
            }
            else
            {
                filter = Builders<T>.Filter.Ne(x => x.IsDeleted, true);
            }
            var query = dbEntity.Find(filter);
            if (projection != null)
                query = query.Project<T>(projection);
            var data = query.ToList();

            await QueryDataFromLinkedServers(filter, data);
            return data;
        }

        public async Task<IEnumerable<T>> GetManyFromLinkedServerAsync(IEnumerable<string> ids, ProjectionDefinition<T> projection = null)
        {
            var filter = Builders<T>.Filter.And(
                Builders<T>.Filter.In(x => x.Id, ids),
                Builders<T>.Filter.Eq(x => x.IsDeleted, false));
            var query = dbEntity.Find(filter);
            if (projection != null)
                query = query.Project<T>(projection);
            var data = await query.ToListAsync();
            await QueryDataFromLinkedServers(filter, data);
            return data;
        }


        public async Task<T?> GetFromLinkedServerAsync(string id)
        {
            var filter = Builders<T>.Filter.And(
                Builders<T>.Filter.Eq(x => x.Id, id),
                Builders<T>.Filter.Or(
                    Builders<T>.Filter.Eq(x => x.IsDeleted, false),
                    Builders<T>.Filter.Eq(x => x.IsDeleted, (bool?)null)
                )
            );

            // Try primary database first
            var data = await dbEntity
                .Find(filter)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (data != null)
                return data;

            // Fallback to linked servers
            return await GetQueryDataFromLinkedServers(filter)
                .ConfigureAwait(false);
        }

        public async Task<T?> GetQueryDataFromLinkedServers(FilterDefinition<T> filter)
        {
            var tasks = mongoDbConnectionService.LinkedDatabase
                .Where(db => db != null)
                .Select(db => db!.GetCollection<T>(collectionName).Find(filter).FirstOrDefaultAsync());

            var results = await Task.WhenAll(tasks);

            return results.FirstOrDefault(r => r != null);
        }


        //public async Task QueryDataFromLinkedServers(FilterDefinition<T> filter, List<T> data)
        //{
        //    await mongoDbConnectionService.InitializeLinkedServers();
        //    foreach (var database2 in mongoDbConnectionService.LinkedDatabase)
        //    {
        //        var linkedDbEntity = database2?.GetCollection<T>(collectionName);
        //        var tdata = await linkedDbEntity.Find(filter).ToListAsync();
        //        data.AddRange(tdata);
        //    }
        //}

        public async Task QueryDataFromLinkedServers(FilterDefinition<T> filter, List<T> data)
        {
            var tasks = mongoDbConnectionService.LinkedDatabase
                .Where(db => db != null)
                .Select(db => db!.GetCollection<T>(collectionName).Find(filter).ToListAsync());

            var results = await Task.WhenAll(tasks);

            data.AddRange(results.SelectMany(r => r));
        }

        //public async Task QueryDataFromLinkedServersByPipeline(List<BsonDocument> pipeline, List<BsonDocument> data)
        //{
        //    await mongoDbConnectionService.InitializeLinkedServers();
        //    foreach (var database2 in mongoDbConnectionService.LinkedDatabase)
        //    {
        //        var linkedDbEntity = database2?.GetCollection<T>(collectionName);
        //        var tdata = await dbEntity.Aggregate<BsonDocument>(pipeline).ToListAsync();
        //        data.AddRange(tdata);
        //    }
        //}

        public async Task QueryDataFromLinkedServersByPipeline(List<BsonDocument> pipeline, List<BsonDocument> data)
        {
            var tasks = mongoDbConnectionService.LinkedDatabase
                .Where(db => db != null)
                .Select(db => db!.GetCollection<T>(collectionName).Aggregate<BsonDocument>(pipeline).ToListAsync());

            var results = await Task.WhenAll(tasks);

            data.AddRange(results.SelectMany(r => r));
        }

        //public async Task QueryDataFromLinkedServersByPipeline(List<BsonDocument> pipeline, List<T> data)
        //{
        //    await mongoDbConnectionService.InitializeLinkedServers();
        //    foreach (var database2 in mongoDbConnectionService.LinkedDatabase)
        //    {
        //        var linkedDbEntity = database2?.GetCollection<T>(collectionName);
        //        var tdata = await dbEntity.Aggregate<T>(pipeline).ToListAsync();
        //        data.AddRange(tdata);
        //    }
        //}
        public async Task QueryDataFromLinkedServersByPipeline(List<BsonDocument> pipeline, List<T> data)
        {

            var tasks = mongoDbConnectionService.LinkedDatabase
                .Where(db => db != null)
                .Select(db => db!.GetCollection<T>(collectionName).Aggregate<T>(pipeline).ToListAsync());

            var results = await Task.WhenAll(tasks);

            data.AddRange(results.SelectMany(r => r));
        }

        //public async Task<IEnumerable<T>> GetAllAsync(ProjectionDefinition<T> projection = null, bool includeDeleted = false)
        //{
        //    FilterDefinition<T> filter;
        //    if (includeDeleted)
        //    {
        //        filter = Builders<T>.Filter.Empty;
        //    }
        //    else
        //    {
        //        filter = Builders<T>.Filter.Ne(x => x.IsDeleted, true);
        //    }
        //    var query = dbEntity.Find(filter);
        //    if (projection != null)
        //        query = query.Project<T>(projection);
        //    return query.ToList();
        //}

        public async Task<IEnumerable<T>> GetAllAsync(
         ProjectionDefinition<T> projection = null,
         bool includeDeleted = false,
         CancellationToken cancellationToken = default)
        {
            FilterDefinition<T> filter = includeDeleted
                ? Builders<T>.Filter.Empty
                : Builders<T>.Filter.Ne(x => x.IsDeleted, true);

            var query = dbEntity.Find(filter);

            if (projection != null)
                query = query.Project<T>(projection);

            return await query.ToListAsync(cancellationToken); // ✅ ASYNC + TOKEN
        }

        public async Task<T> GetAsync(string id)
        {
            var filter = Builders<T>.Filter.And(
              Builders<T>.Filter.Eq(x => x.Id, id),
             Builders<T>.Filter.Or(
             Builders<T>.Filter.Eq(x => x.IsDeleted, false),
             Builders<T>.Filter.Eq(x => x.IsDeleted, (bool?)null)));

            return await dbEntity.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetManyAsync(IEnumerable<string> ids, ProjectionDefinition<T> projection = null)
        {
            var filter = Builders<T>.Filter.And(
                Builders<T>.Filter.In(x => x.Id, ids),
                Builders<T>.Filter.Eq(x => x.IsDeleted, false));
            var query = dbEntity.Find(filter);
            if (projection != null)
                query = query.Project<T>(projection);
            var data = await query.ToListAsync();
            return data;
        }

        public async Task<string> InsertAsync(T entity)
        {
            await dbEntity.InsertOneAsync(entity);
            return entity.Id;
        }

        public async Task<bool> InsertManyAsync(IEnumerable<T> entities)
        {
            await dbEntity.InsertManyAsync(entities);
            return true;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
            var result = await dbEntity.ReplaceOneAsync(filter, entity);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateFieldsAsync(string id, UpdateDefinition<T> updateDefinition, UpdateOptions updateOptions = null)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            var result = await dbEntity.UpdateOneAsync(filter, updateDefinition, updateOptions);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateManyFieldsAsync(FilterDefinition<T> filter, UpdateDefinition<T> updateDefinition)
        {
            var result = await dbEntity.UpdateManyAsync(filter, updateDefinition);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Soft deletes a document by setting its `IsDeleted` flag to true.
        /// </summary>
        public async Task<bool> SoftDeleteAsync(string id, string userId)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            var filter = Builders<T>.Filter.Eq(x => x.Id, id);

            var update = Builders<T>.Update
                .Set(x => x.UpdatedOn, DateTime.UtcNow)
                .Set(x => x.UpdatedBy, userId)
                .Set(x => x.IsDeleted, true)
                .Set(x => x.DeletedOn, DateTime.UtcNow);

            var result = await dbEntity.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Soft deletes multiple documents by setting their `IsDeleted` flag to true.
        /// </summary>
        public async Task<long> SoftDeleteManyAsync(IEnumerable<string> ids, string userId)
        {
            if (ids == null || !ids.Any())
                return 0;

            var filter = Builders<T>.Filter.In(x => x.Id, ids);

            var update = Builders<T>.Update
                .Set(x => x.UpdatedOn, DateTime.UtcNow)
                .Set(x => x.UpdatedBy, userId)
                .Set(x => x.IsDeleted, true)
                .Set(x => x.DeletedOn, DateTime.UtcNow);

            var result = await dbEntity.UpdateManyAsync(filter, update);
            return result.ModifiedCount;
        }

        public async Task<long> DeleteManyAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
                return 0;

            var filter = Builders<T>.Filter.In(x => x.Id, ids);


            var result = await dbEntity.DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public async Task<IEnumerable<T>> GetRetentionPeriodData(int retentionPeriod, int batchSize)
        {
            DateTime cutoffDate = DateTime.Now.AddMonths(-retentionPeriod);

            var filter = Builders<T>.Filter.Lt(x => x.CreatedOn, cutoffDate);

            return await dbEntity
                .Find(filter)
                .SortBy(x => x.CreatedOn)
                .Limit(batchSize)
                .ToListAsync();
        }

        public void Dispose()
        {
            if (cursor != null)
                cursor.Dispose();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            var filter = Builders<T>.Filter.Eq(x => x.Id, id);

            var result = await dbEntity.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }
    }
}
