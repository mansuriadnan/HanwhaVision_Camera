using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.DbEntities
{
    public class VehicleParkingCount :BaseModel
    {
        [BsonElement("deviceId")]
        public string DeviceId { get; set; }

        [BsonElement("channel")]
        public int Channel { get; set; }

        [BsonElement("lineIndex")]
        public int LineIndex { get; set; }

        [BsonElement("parkingCount")]
        public int ParkingCount { get; set; }

    }
}
