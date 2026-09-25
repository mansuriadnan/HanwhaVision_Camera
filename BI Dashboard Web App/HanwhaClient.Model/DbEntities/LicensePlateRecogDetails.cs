using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.DbEntities
{
    public class LicensePlateRecogDetails : BaseModel
    {
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("isIn")]
        public bool IsIn { get; set; }

        [BsonElement("anprVehicleId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? AnprVehicleId { get; set; }

        [BsonElement("vehicleOwnerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? VehicleOwnerId { get; set; }

        [BsonElement("entryGate")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? EntryGate { get; set; }

        [BsonElement("entryTime")]
        public DateTime? EntryTime { get; set; }

        [BsonElement("exitTime")]
        public DateTime? ExitTime { get; set; }

        [BsonElement("exitGate")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ExitGate { get; set; }

        [BsonElement("message")]
        public string? Message { get; set; }

        [BsonElement("overstayNotificationSent")]
        public bool OverstayNotificationSent { get; set; }

        [BsonElement("overstay24hNotificationSent")]
        public bool Overstay24hNotificationSent { get; set; }

        [BsonExtraElements]
        public BsonDocument DynamicFields { get; set; } = new BsonDocument();
    }
}
