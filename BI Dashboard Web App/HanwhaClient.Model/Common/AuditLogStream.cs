using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Common
{
    public class AuditLogStream<T> : BaseModel
    {
        protected readonly IMongoCollection<T> dbEntity;
        private IChangeStreamCursor<ChangeStreamDocument<T>> cursor;
        protected readonly IMongoCollection<AuditLog> dbAuditEntity;
        public async Task StartChangeStreamAsync()
        {
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<T>>()
                .Match(change => change.OperationType == ChangeStreamOperationType.Insert ||
                                 change.OperationType == ChangeStreamOperationType.Update ||
                                 change.OperationType == ChangeStreamOperationType.Delete);

            if (cursor == null)
            {
                try
                {
                    cursor = dbEntity.Watch(pipeline);

                    while (await cursor.MoveNextAsync())
                    {
                        foreach (var change in cursor.Current)
                        {
                            var auditLogData = new AuditLog
                            {
                                OperationType = change.OperationType.ToString(),
                                CollectionName = change.CollectionNamespace.CollectionName,
                                DocumentKey = change.DocumentKey,
                                FullDocument = change.FullDocument.ToBsonDocument<T>(),
                                UpdateDescription = change.UpdateDescription?.UpdatedFields,
                                RemovedFields = change.UpdateDescription?.RemovedFields,
                                CreatedOn = System.DateTime.UtcNow

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
