using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.DbEntities
{
    public class IdracEventLogs : BaseModel
    {

        [BsonElement("sourceIP")]
        public string? SourceIP { get; set; }

        [BsonElement("eventSubscriptionId")]
        public string? EventSubscriptionId { get; set; }

        [BsonElement("eventName")]
        public string? EventName { get; set; }

        [BsonElement("eventId")]
        public string? EventId { get; set; }

        [BsonElement("eventTimestamp")]
        public DateTimeOffset? EventTimestamp { get; set; }

        [BsonElement("eventType")]
        public string? EventType { get; set; }

        [BsonElement("message")]
        public string? Message { get; set; }

        [BsonElement("severity")]
        public string? Severity { get; set; }
    }
}
