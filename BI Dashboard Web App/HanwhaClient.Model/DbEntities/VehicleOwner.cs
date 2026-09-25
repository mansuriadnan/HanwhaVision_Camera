using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{
    public class VehicleOwner : BaseModel
    {
        [BsonElement("registrationType")]
        public string RegistrationType { get; set; }

        [BsonElement("ownerName")]
        public string OwnerName { get; set; }

        [BsonElement("building")]
        public string Building { get; set; }

        [BsonElement("buildingUnit")]
        public string BuildingUnit { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("contactNumber")]
        public string ContactNumber { get; set; }

        [BsonElement("allowedVehicle")]
        public int AllowedVehicle { get; set; }

        [BsonElement("ownerValidTo")]
        public DateTime? OwnerValidTo { get; set; }

        [BsonElement("allowedFromTime")]
        public DateTime? AllowedFromTime { get; set; }

        [BsonElement("allowedToTime")]
        public DateTime? AllowedToTime { get; set; }

        [BsonElement("allowedGates")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string[] AllowedGates { get; set; }

        [BsonElement("enabledAlarmForOverstay")]
        public bool EnabledAlarmForOverstay { get; set; }

        [BsonElement("enabledAlarmFor24HStay")]
        public bool EnabledAlarmFor24HStay { get; set; }
        
    }
}
