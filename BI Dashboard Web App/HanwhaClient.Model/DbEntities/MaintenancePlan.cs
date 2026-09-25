using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.DbEntities
{
    public class MaintenancePlan : BaseModel
    {
        [BsonElement("planName")]
        [BsonRequired]
        public string PlanName { get; set; } = string.Empty;

        [BsonElement("duration")]
        public int Duration { get; set; }

        [BsonElement("startDate")]
        public DateTime StartDate { get; set; }

        [BsonElement("endDate")]
        public DateTime? EndDate { get; set; }

        [BsonElement("deviceIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<ObjectId> DeviceIds { get; set; } = new List<ObjectId>();
        
        [BsonElement("floorIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<ObjectId>? FloorIds { get; set; } = new List<ObjectId>();
        
        [BsonElement("zoneIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<ObjectId>? ZoneIds { get; set; } = new List<ObjectId>();

        [BsonElement("nextExecutionDate")]
        public DateTime? NextExecutionDate { get; set; }
    }
}
