using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{
    public class SsmOfflinedevice : BaseModel
    {
        [BsonElement("serverId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ServerId { get; set; }

        [BsonElement("deviceId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string DeviceId { get; set; }

        [BsonElement("offlineTime")]
        public DateTime OfflineTime { get; set; }

        [BsonElement("onlineTime")]
        public DateTime? OnlineTime { get; set; }

        
    }
}
