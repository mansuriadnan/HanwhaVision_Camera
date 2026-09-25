using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{
    public class VehicleCurrentParkingCount : BaseModel
    {
        [BsonElement("deviceId")]
        public string DeviceId { get; set; }

        [BsonElement("channel")]
        public int Channel { get; set; }

        [BsonElement("parkingCount")]
        public int ParkingCount { get; set; }

        [BsonElement("lineIndex")]
        public int LineIndex { get; set; }
    }
}
