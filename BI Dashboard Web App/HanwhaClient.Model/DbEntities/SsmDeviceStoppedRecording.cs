using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{
    public class SsmDeviceStoppedRecording : BaseModel
    {
        [BsonElement("serverId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ServerId { get; set; }

        [BsonElement("deviceId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string DeviceId { get; set; }

        [BsonElement("stopRecordingTime")]
        public DateTime StopRecordingTime { get; set; }

        [BsonElement("startRecordingTime")]
        public DateTime? StartRecordingTime { get; set; }

        
    }
}
