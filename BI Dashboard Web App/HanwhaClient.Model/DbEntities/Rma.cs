using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.DbEntities
{
    public class Rma : BaseModel
    {
        [BsonRequired]
        [BsonElement("deviceId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string DeviceId { get; set; } = null!;

        [BsonRequired]
        [BsonElement("rmaStatus")]
        [BsonRepresentation(BsonType.String)]
        public string RMAStatus { get; set; }
   
        [BsonElement("inProgressNotes")]
        [BsonRepresentation(BsonType.String)]
        public string? InProgressNotes { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("completedNotes")]
        public string? CompletedNotes { get; set; }

        [BsonElement("startDate")]
        public DateTime StartDate { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("endDate")]
        public DateTime? EndDate { get; set; }

        [BsonElement("floorId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? FloorId { get; set; } = null;

        [BsonElement("zoneId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ZoneId { get; set; } = null;
    }

}
