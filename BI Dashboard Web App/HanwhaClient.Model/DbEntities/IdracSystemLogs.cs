using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{
    public class IdracSystemLogs : BaseModel
    {
        [BsonElement("idracServerId")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string IdracServerId { get; set; }

        [BsonElement("logId")]
        public string LogId { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("datetime")]
        public DateTime Datetime { get; set; }

        [BsonElement("severity")]
        public string Severity { get; set; }
    }
}
