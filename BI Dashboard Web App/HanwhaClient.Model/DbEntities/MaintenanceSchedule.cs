using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.DbEntities
{
    public class MaintenanceSchedule : BaseModel
    {

        [BsonElement("deviceId")]
        [BsonRequired]
        [BsonRepresentation(BsonType.ObjectId)]
        public string DeviceId { get; set; }

        [BsonElement("maintenancePlanId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? MaintenancePlanId { get; set; }

        [BsonElement("statusHistory")]
        [BsonRequired]
        public List<StatusHistoryItem> StatusHistory { get; set; } = new List<StatusHistoryItem>();

        [BsonElement("latestStatus")]
        [BsonRequired]
        public string LatestStatus { get; set; }

        [BsonElement("dueDate")]
        public DateTime? DueDate { get; set; }

        [BsonElement("beforeCameraImage")]
        public string? BeforeCameraImage { get; set; }

        [BsonElement("afterCameraImage")]
        public string? AfterCameraImage { get; set; }

        [BsonElement("imageMatchingPercentage")]
        public double? ImageMatchingPercentage { get; set; }

        [BsonElement("isScheduleManually")]
        [BsonRequired]
        public bool IsScheduleManually { get; set; }

        [BsonElement("floorId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? FloorId { get; set; } = null;

        [BsonElement("zoneId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ZoneId { get; set; } = null;
    }

    public class StatusHistoryItem
    {
        [BsonElement("status")]
        public string? Status { get; set; }

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("statusDatetime")]
        public DateTime? StatusDatetime { get; set; }
    }
}
