using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{
    public class SsmDeviceDetails : BaseModel
    {
        [BsonElement("serverId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ServerId { get; set; }

        [BsonElement("ipAddress")]
        public string IpAddress { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("status")]
        public int Status { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }

        [BsonElement("cameraStatus")]
        public string CameraStatus { get; set; }

        [BsonElement("recordingStatus")]
        public string RecordingStatus { get; set; }
    }
}
