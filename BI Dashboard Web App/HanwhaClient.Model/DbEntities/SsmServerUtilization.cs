using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{
    public class SsmServerUtilization : BaseModel
    {
        [BsonElement("serverId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ServerId { get; set; }

        [BsonElement("totalProcessorUsage")]
        public decimal TotalProcessorUsage { get; set; }

        [BsonElement("totalMemoryUsage")]
        public decimal TotalMemoryUsage { get; set; }

        [BsonElement("cpuSystemUsage")]
        public decimal CpuSystemUsage { get; set; }

        [BsonElement("memorySystemUsage")]
        public decimal MemorySystemUsage { get; set; }

        [BsonElement("cpuMediaUsage")]
        public decimal CpuMediaUsage { get; set; }

        [BsonElement("memoryMediaUsage")]
        public decimal MemoryMediaUsage { get; set; }

        [BsonElement("disks")]
        public List<DiskDetail> Disks { get; set; } = new List<DiskDetail>();

        [BsonElement("diskTotalSize")]
        public long DiskTotalSize { get; set; }

        [BsonElement("diskFreeSize")]
        public long DiskFreeSize { get; set; }

        [BsonElement("diskFreePercentage")]
        public decimal DiskFreePercentage { get; set; }

        [BsonElement("totalCameraCount")]
        public int TotalCameraCount { get; set; }

        [BsonElement("failureCameraCount")]
        public int FailureCameraCount { get; set; }

    }

    public class DiskDetail
    {
        [BsonElement("drive")]
        public string Drive { get; set; }

        [BsonElement("free")]
        public long Free { get; set; }

        [BsonElement("total")]
        public long Total { get; set; }
    }
}
