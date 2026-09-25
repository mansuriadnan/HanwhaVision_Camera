using HanwhaClient.Model.Dto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{

    public class IdracDetails : BaseModel
    {
        [BsonElement("idracServerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdracServerId { get; set; }

        [BsonElement("health")]
        public string Health { get; set; }

        [BsonElement("memory")]
        public IdracMemory? Memory { get; set; }

        [BsonElement("Processor")]
        public IdracProcessor? Processor { get; set; }

        [BsonElement("cooling")]
        public List<CoolingDetail> Cooling { get; set; }

        [BsonElement("temperature")]
        public List<CoolingTemperatureDetail> Temperature { get; set; }

        [BsonElement("powerSupply")]
        public PowerSupply PowerSupply { get; set; }

        [BsonElement("embeddedNetworkCard")]
        public NetworkCard EmbeddedNetworkCard { get; set; }
        
        [BsonElement("integratedNetworkCard")]
        public NetworkCard IntegratedNetworkCard { get; set; }

        [BsonElement("storage")]
        public IdracStorage? Storage { get; set; }

    }

    public class IdracMemory
    {

        [BsonElement("totalMemory")]
        public decimal TotalMemory { get; set; }

        [BsonElement("usedSlots")]
        public int UsedSlots { get; set; }

        [BsonElement("totalSlots")]
        public int TotalSlots { get; set; }

        [BsonElement("totalSpeed")]
        public int TotalSpeed { get; set; }

        [BsonElement("health")]
        public string Health { get; set; }

        [BsonElement("memoryList")]
        public List<IdracMemoryDetails>? MemoryList { get; set; }

    }

    public class IdracMemoryDetails
    {
        [BsonElement("ramName")]
        public string RamName { get; set; }

        [BsonElement("size")]
        public long Size { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("speed")]
        public int Speed { get; set; }

        [BsonElement("manufacturer")]
        public string Manufacturer { get; set; }

        [BsonElement("health")]
        public string Health { get; set; }
    }

    public class IdracProcessor
    {
        [BsonElement("totalCpu")]
        public int TotalCpu { get; set; }

        [BsonElement("usedCpu")]
        public int UsedCpu { get; set; }

        [BsonElement("totalCores")]
        public int TotalCores { get; set; }

        [BsonElement("totalThreads")]
        public int TotalThreads { get; set; }

        [BsonElement("avgSpeed")]
        public decimal SvgSpeed { get; set; }

        [BsonElement("health")]
        public string Health { get; set; }

        [BsonElement("processorList")]
        public List<IdracProcessorDetails> ProcessorList { get; set; }
    }

    public class IdracProcessorDetails
    {
        [BsonElement("cpuName")]
        public string CpuName { get; set; }

        [BsonElement("model")]
        public string Model { get; set; }

        [BsonElement("cores")]
        public int Cores { get; set; }

        [BsonElement("threads")]
        public int Threads { get; set; }

        [BsonElement("maxSpeed")]
        public int MaxSpeed { get; set; }

        [BsonElement("currentSpeed")]
        public int CurrentSpeed { get; set; }

        [BsonElement("health")]
        public string Health { get; set; }
    }

    public class CoolingDetail
    {
        [BsonElement("fanName")]
        public string FanName { get; set; }

        [BsonElement("fanSpeed")]
        public int FanSpeed { get; set; }

        [BsonElement("health")]
        public string Health { get; set; }

    }

    public class CoolingTemperatureDetail
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("physicalContext")]
        public string PhysicalContext { get; set; }

        [BsonElement("temperature")]
        public int Temperature { get; set; }

        [BsonElement("health")]
        public string health { get; set; }
    }

    public class PowerSupply
    {
        [BsonElement("totalCapacity")]
        public double TotalCapacity { get; set; }

        [BsonElement("currentUsage")]
        public int CurrentUsage { get; set; }

        [BsonElement("redundancy")]
        public string Redundancy { get; set; }

        [BsonElement("health")]
        public string Health { get; set; }

        [BsonElement("powerSupplyList")]
        public List<PowerSupplyDetail> PowerSupplyList { get; set; }
    }

    public class PowerSupplyDetail
    {
        [BsonElement("psu")]
        public string Psu { get; set; }

        [BsonElement("capacity")]
        public double Capacity { get; set; }

        [BsonElement("output")]
        public double Output { get; set; }

        [BsonElement("inputVoltage")]
        public double InputVoltage { get; set; }

        [BsonElement("currentVoltage")]
        public double CurrentVoltage { get; set; }
         
        [BsonElement("health")]
        public string Health { get; set; }

    }

    public class NetworkCard
    {
        [BsonElement("manufacturer")]
        public string Manufacturer { get; set; }

        [BsonElement("model")]
        public string Model { get; set; }

        [BsonElement("firmwarePackageVersion")]
        public string FirmwarePackageVersion { get; set; }

        [BsonElement("health")]
        public string Health { get; set; }

        [BsonElement("networkCardList")]
        public List<NetworkCardDetails> NetworkCardList { get; set; }
    }

    public class NetworkCardDetails
    {
        [BsonElement("productName")]
        public string ProductName { get; set; }

        [BsonElement("protocol")]
        public string Protocol { get; set; }

        [BsonElement("vendorName")]
        public string VendorName { get; set; }

        [BsonElement("activeLinkTechnology")]
        public string ActiveLinkTechnology { get; set; }

        [BsonElement("associatedNetworkAddress")]
        public string AssociatedNetworkAddress { get; set; }

        [BsonElement("linkStatus")]
        public string LinkStatus { get; set; }

        [BsonElement("currentLinkSpeed")]
        public int CurrentLinkSpeed { get; set; }

        [BsonElement("physicalPortNumber")]
        public string PhysicalPortNumber { get; set; }

        [BsonElement("health")]
        public string health { get; set; }
    }

    public class IdracStorage
    {

        [BsonElement("drives")]
        public List<IdracDriveDetails> Drives { get; set; } = new List<IdracDriveDetails>();
    }

    public class IdracDriveDetails
    {
        [BsonElement("diskName")]
        public string DiskName { get; set; }

        [BsonElement("size")]
        public long Size { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("protocol")]
        public string Protocol { get; set; }

        [BsonElement("rpm")]
        public int Rpm { get; set; }

        [BsonElement("health")]
        public string Health { get; set; }
    }
}
