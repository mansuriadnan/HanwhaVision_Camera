using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{
    public class OfflineDevices : BaseModel
    {
        [BsonElement("deviceId")]
        public string DeviceId { get; set; }

        [BsonElement("offlineTime")]
        public DateTime OfflineTime { get; set; }

        [BsonElement("onlineTime")]
        public DateTime? OnlineTime { get; set; }

        [BsonElement("status")]
        public int Status { get; set; }
    }
}
