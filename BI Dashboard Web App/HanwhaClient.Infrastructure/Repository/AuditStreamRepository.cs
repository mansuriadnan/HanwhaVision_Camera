using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public interface IAuditStreamRepository<T> where T : class
    {
        /// <summary>
        /// Record data for auditing
        /// </summary>
        /// <returns></returns>
        Task StartChangeStreamAsync(string collectionName);
    }

    public class AuditStreamRepository<T> : IAuditStreamRepository<T> where T : BaseModel
    {
        protected IMongoCollection<T> dbEntity;
        protected IMongoCollection<AuditLog> dbAuditEntity;
        protected readonly IMongoDatabase mongoDatabase;
        private IChangeStreamCursor<ChangeStreamDocument<T>> cursor;
        
        public AuditStreamRepository(MongoDbConnectionService mongoDbConnectionService)
        {
            mongoDatabase = mongoDbConnectionService.Database;
        }

        public async Task StartChangeStreamAsync(string collectionName)
        {
            // Enable pre-images for old document retrieval
            var command = new BsonDocument
            {
                { "collMod", collectionName },
                { "changeStreamPreAndPostImages", new BsonDocument { { "enabled", true } } }
            };
            await mongoDatabase.RunCommandAsync<BsonDocument>(command);


            dbEntity = mongoDatabase?.GetCollection<T>(collectionName);
            dbAuditEntity = mongoDatabase?.GetCollection<AuditLog>("auditLog");

            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<T>>()
                .Match(change => change.OperationType == ChangeStreamOperationType.Insert ||
                                 change.OperationType == ChangeStreamOperationType.Update ||
                                 change.OperationType == ChangeStreamOperationType.Delete);

            if (cursor == null)
            {
                try
                {
                    cursor = dbEntity.Watch(
                        pipeline,
                        new ChangeStreamOptions
                        {
                            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
                            FullDocumentBeforeChange = ChangeStreamFullDocumentBeforeChangeOption.WhenAvailable
                        });

                    while (await cursor.MoveNextAsync())
                    {
                        foreach (var change in cursor.Current)
                        {
                            var operationType = change.OperationType;
                            string? createdBy = string.Empty;
                            BsonDocument documentBeforeChangeData = null;
                            if (operationType == ChangeStreamOperationType.Update && change.FullDocumentBeforeChange != null)
                            {
                                var updateFieldNames = change.UpdateDescription?.UpdatedFields.Names;

                                if(updateFieldNames.Count() == 1 && updateFieldNames.FirstOrDefault() == "updatedOn")
                                {
                                    continue;
                                }

                                documentBeforeChangeData = updateFieldNames == null
                                    ? null
                                    : new BsonDocument(
                                        change.FullDocumentBeforeChange.ToBsonDocument().Elements
                                            .Where(e => updateFieldNames.Contains(e.Name))
                                      );

                                createdBy = change?.FullDocument?.UpdatedBy;
                            }
                            else if (operationType == ChangeStreamOperationType.Insert)
                            {
                                createdBy = change?.FullDocument?.CreatedBy;
                            }
                            else if (operationType == ChangeStreamOperationType.Delete)
                            {
                                createdBy = change?.FullDocument?.UpdatedBy;
                            }
                            var auditLogData = new AuditLog
                            {
                                OperationType = operationType.ToString(),
                                CollectionName = change.CollectionNamespace.CollectionName,
                                DocumentKey = change.DocumentKey,
                                FullDocument = operationType != ChangeStreamOperationType.Update ? change.FullDocument?.ToBsonDocument<T>() : null,
                                DocumentBeforeChange = documentBeforeChangeData,
                                UpdateDescription = change.UpdateDescription?.UpdatedFields,
                                RemovedFields = change.UpdateDescription?.RemovedFields,
                                CreatedOn = System.DateTime.UtcNow,
                                CreatedBy = createdBy
                            };
                            await dbAuditEntity.InsertOneAsync(auditLogData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    throw;
                }
            }
            cursor.Dispose();
        }

    }


}
