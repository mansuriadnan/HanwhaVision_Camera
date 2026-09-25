using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace HanwhaClient.Model.DbEntities
{
    public class ANPRVehicle : BaseModel
    {
        [BsonElement("vehicleOwnerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string VehicleOwnerId { get; set; }

        [BsonElement("country")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Country { get; set; }

        [BsonElement("state")]
        public string State { get; set; }

        [BsonElement("series")]
        public string Series { get; set; }

        [BsonElement("vehicleNumber")]
        public int VehicleNumber { get; set; }

        [BsonElement("plateCode")]
        public string PlateCode { get; set; }

        [BsonElement("plateCategory")]
        public string PlateCategory { get; set; }

        [BsonElement("make")]
        public string Make { get; set; }

        [BsonElement("model")]
        public string Model { get; set; }

        [BsonElement("color")]
        public string Color { get; set; }

        [BsonElement("visitorValidFrom")]
        public DateTime? VisitorValidFrom { get; set; }

        [BsonElement("visitorValidTo")]
        public DateTime? VisitorValidTo { get; set; }
    }
}
