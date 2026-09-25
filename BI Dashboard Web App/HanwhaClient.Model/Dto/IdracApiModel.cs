using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace HanwhaClient.Model.Dto
{

    public class IdracLogin
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }

    public class IdracMainComponent
    {
        public OdataObject Memory { get; set; }

        public MemorySummary MemorySummary { get; set; }

        public SystemOem Oem { get; set; }
        public OdataObject Processors { get; set; }
        public ProcessorSummary ProcessorSummary { get; set; }
        public Status Status { get; set; }
    }

    public class IdracLightIndigator
    {
        public bool LocationIndicatorActive { get; set; }
    }

    public class SystemOem
    {
        public DellSystemWrapper Dell { get; set; }
    }

    public class DellSystemWrapper
    {
        [JsonPropertyName("@odata.type")]
        public string OdataType { get; set; }

        public DellSystem DellSystem { get; set; }
    }

    public class DellSystem
    {
        public string BIOSReleaseDate { get; set; }

        public string BaseBoardChassisSlot { get; set; }

        public string BatteryRollupStatus { get; set; }

        public string BladeGeometry { get; set; }

        public string CMCIP { get; set; }

        public string CMCRollupStatus { get; set; }

        public string CPURollupStatus { get; set; }

        public string ChassisModel { get; set; }

        public string ChassisName { get; set; }

        public string ChassisServiceTag { get; set; }

        public int ChassisSystemHeightUnit { get; set; }

        public string CoolingRollupStatus { get; set; }

        public string CurrentRollupStatus { get; set; }

        public int EstimatedExhaustTemperatureCelsius { get; set; }

        public int EstimatedSystemAirflowCFM { get; set; }

        public string ExpressServiceCode { get; set; }

        public string FanRollupStatus { get; set; }

        public string Id { get; set; }

        public string IntrusionRollupStatus { get; set; }

        public string IsOEMBranded { get; set; }

        public DateTime LastSystemInventoryTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string LicensingRollupStatus { get; set; }

        public string ManagedSystemSize { get; set; }

        public int MaxCPUSockets { get; set; }

        public int MaxDIMMSlots { get; set; }

        public int MaxPCIeSlots { get; set; }

        public int MaxSystemMemoryMiB { get; set; }

        public string MemoryOperationMode { get; set; }

        public string Name { get; set; }

        public string NodeID { get; set; }

        public string PSRollupStatus { get; set; }

        public string PlatformGUID { get; set; }

        public int PopulatedDIMMSlots { get; set; }

        public int PopulatedPCIeSlots { get; set; }

        public string PowerCapEnabledState { get; set; }

        public string SDCardRollupStatus { get; set; }

        public string SELRollupStatus { get; set; }

        public string ServerAllocationWatts { get; set; }

        public string StorageRollupStatus { get; set; }

        public string SysMemErrorMethodology { get; set; }

        public string SysMemFailOverState { get; set; }

        public string SysMemLocation { get; set; }

        public string SysMemPrimaryStatus { get; set; }

        public string SystemGeneration { get; set; }

        public string SystemHealthRollupStatus { get; set; }

        public int SystemID { get; set; }

        public string SystemRevision { get; set; }

        public string TempRollupStatus { get; set; }

        public string TempStatisticsRollupStatus { get; set; }

        public string UUID { get; set; }

        public string VoltRollupStatus { get; set; }

        public string smbiosGUID { get; set; }

        [JsonPropertyName("@odata.context")]
        public string OdataContext { get; set; }

        [JsonPropertyName("@odata.type")]
        public string OdataType { get; set; }

        [JsonPropertyName("@odata.id")]
        public string OdataId { get; set; }
    }

    public class OdataObject
    {
        [JsonPropertyName("@odata.id")]
        public string OdataId { get; set; }
    }

    public class MemorySummary
    {
        public string MemoryMirroring { get; set; }

        public Status Status { get; set; }

        public int TotalSystemMemoryGiB { get; set; }
    }

    public class Status
    {
        public string Health { get; set; }

        public string HealthRollup { get; set; }

        public string State { get; set; }
    }

    public class MemoryCollectionResponse
    {
        public List<OdataMember> Members { get; set; }

        [JsonPropertyName("Members@odata.count")]
        public int MembersCount { get; set; }

    }

    public class OdataMember
    {
        [JsonPropertyName("@odata.id")]
        public string OdataId { get; set; }
    }


    public class NetworkAdapterMain
    {
        [JsonPropertyName("Manufacturer")]
        public string Manufacturer { get; set; }

        [JsonPropertyName("Model")]
        public string? Model { get; set; }

        [JsonPropertyName("NetworkDeviceFunctions")]
        public OdataMember NetworkDeviceFunctions { get; set; }

        [JsonPropertyName("NetworkPorts")]
        public OdataMember NetworkPorts { get; set; }

        [JsonPropertyName("Status")]
        public Status Status { get; set; }
    }

    #region 
    public class IdracEnableEventServiceResponse
    {
        [JsonPropertyName("@Message.ExtendedInfo")]
        public List<EventServiceExtendedInfo> MessageExtendedInfo { get; set; } = new();
    }

    public class EventServiceExtendedInfo
    {

        [JsonPropertyName("Severity")]
        public string? Severity { get; set; }
    }
    #endregion
    public class AlertEventSubscriptionResponse
    {
        [JsonPropertyName("Id")]
        public string? Id { get; set; }
    }

    #region 
    // subscription API call payload
    //public class SubscribedAlertEventPayload
    //{       
    //    [JsonPropertyName("sourceIP")]
    //    public string? SourceIP { get; set; }

    //    [JsonPropertyName("payload")]
    //    public AlertPayload? Payload { get; set; }
    //}

    //public class AlertPayload
    //{
    //    [JsonPropertyName("Events")]
    //    public List<AlertEvent>? Events { get; set; }

    //    [JsonPropertyName("Id")]
    //    public string? Id { get; set; }

    //    [JsonPropertyName("Name")]
    //    public string? Name { get; set; }
    //}

    //public class AlertEvent
    //{
    //    [JsonPropertyName("EventId")]
    //    public string? EventId { get; set; }

    //    [JsonPropertyName("EventTimestamp")]
    //    public DateTimeOffset? EventTimestamp { get; set; }

    //    [JsonPropertyName("EventType")]
    //    public string? EventType { get; set; }
                
    //    [JsonPropertyName("Message")]
    //    public string? Message { get; set; }

    //    [JsonPropertyName("Severity")]
    //    public string? Severity { get; set; }
    //}

    public class SubscribedAlertEventPayload
    {
        [JsonPropertyName("@odata.context")]
        public string? ODataContext { get; set; }

        [JsonPropertyName("@odata.id")]
        public string? ODataId { get; set; }

        [JsonPropertyName("@odata.type")]
        public string? ODataType { get; set; }

        [JsonPropertyName("Context")]
        public string? Context { get; set; }

        [JsonPropertyName("Events")]
        public List<AlertEvent>? Events { get; set; }

        [JsonPropertyName("Id")]
        public string? Id { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Oem")]
        public OemData? Oem { get; set; }
    }

    public class AlertEvent
    {
        [JsonPropertyName("Context")]
        public string? Context { get; set; }

        [JsonPropertyName("EventId")]
        public string? EventId { get; set; }

        [JsonPropertyName("EventTimestamp")]
        public DateTimeOffset? EventTimestamp { get; set; }

        [JsonPropertyName("EventType")]
        public string? EventType { get; set; }

        [JsonPropertyName("MemberId")]
        public string? MemberId { get; set; }

        [JsonPropertyName("Message")]
        public string? Message { get; set; }

        [JsonPropertyName("MessageArgs")]
        public List<string>? MessageArgs { get; set; }

        [JsonPropertyName("MessageId")]
        public string? MessageId { get; set; }

        [JsonPropertyName("MessageSeverity")]
        public string? MessageSeverity { get; set; }

        [JsonPropertyName("Severity")]
        public string? Severity { get; set; }
    }

    public class OemData
    {
        [JsonPropertyName("Dell")]
        public DellData? Dell { get; set; }
    }

    public class DellData
    {
        [JsonPropertyName("@odata.type")]
        public string? ODataType { get; set; }

        [JsonPropertyName("ServerHostname")]
        public string? ServerHostname { get; set; }
    }
    #endregion



    public class  MemoryDetailResponse
    {
        public int CapacityMiB { get; set; }

        public string Id { get; set; }

        public string Manufacturer { get; set; }

        public string MemoryDeviceType { get; set; }

        public string Name { get; set; }

        public int OperatingSpeedMhz { get; set; }

        public Status Status { get; set; }

    }

    public class ProcessorSummary
    {
        public int Count { get; set; }

        public int CoreCount { get; set; }

        public int LogicalProcessorCount { get; set; }

        public string Model { get; set; }

        public Status Status { get; set; }

        [JsonPropertyName("Status@Redfish.Deprecated")]
        public string StatusDeprecated { get; set; }

        public bool ThreadingEnabled { get; set; }
    }

    public class ProcessorDetailResponse
    {
        public string Id { get; set; }

        public string Manufacturer { get; set; }

        public int MaxSpeedMHz { get; set; }

        public string Model { get; set; }

        public string Name { get; set; }

        public int OperatingSpeedMHz { get; set; }

        public string ProcessorType { get; set; }

        public string Socket { get; set; }

        public Status Status { get; set; }

        public int TotalCores { get; set; }

        public int TotalEnabledCores { get; set; }

        public int TotalThreads { get; set; }
    }

    public class ThermalRoot
    {
        public List<Fan> Fans { get; set; }

        public List<Temperature> Temperatures { get; set; }
    }

    public class Fan
    {
        public string FanName { get; set; }

        public string Name { get; set; }

        public string PhysicalContext { get; set; }

        public int Reading { get; set; }

        public string ReadingUnits { get; set; }
        public Status Status { get; set; }
    }

    public class PowerSubsystemRoot
    {
        public OdataMember PowerSupplies { get; set; }

        public Status Status { get; set; }

        public double CapacityWatts { get; set; }
    }

    public class PowerSupplyRoot
    {
        public OdataObject Metrics { get; set; }

        public string Name { get; set; }

        public double PowerCapacityWatts { get; set; }

        public Status Status { get; set; }
    }

    public class PowerSupplyMetricsRoot
    {
        public MetricReading InputVoltage { get; set; }

        public MetricReading InputCurrentAmps { get; set; }

        public MetricReading InputPowerWatts { get; set; }

        public MetricReading OutputPowerWatts { get; set; }

    }

    public class MetricReading
    {
        public string DataSourceUri { get; set; }

        public double Reading { get; set; }
    }



    public class Temperature
    {
        public string Name { get; set; }

        public string PhysicalContext { get; set; }

        public int ReadingCelsius { get; set; }

        public Status Status { get; set; }
    }


    public class NetworkFunctionResponse
    {
        [JsonPropertyName("Oem")]
        public Oem1 Oem { get; set; }

        [JsonPropertyName("Status")]
        public Status Status { get; set; }
    }

    public class Oem1
    {
        [JsonPropertyName("Dell")]
        public DellOem Dell { get; set; }
    }

    public class DellOem
    {
        [JsonPropertyName("DellNIC")]
        public DellNIC DellNIC { get; set; }
    }

    public class DellNIC
    {
        [JsonPropertyName("ProductName")]
        public string ProductName { get; set; }

        [JsonPropertyName("Protocol")]
        public string Protocol { get; set; }

        [JsonPropertyName("VendorName")]
        public string VendorName { get; set; }
    }

    public class NetworkPortResponse
    {

        [JsonPropertyName("ActiveLinkTechnology")]
        public string ActiveLinkTechnology { get; set; }

        [JsonPropertyName("AssociatedNetworkAddresses")]
        public List<string> AssociatedNetworkAddresses { get; set; }

        [JsonPropertyName("CurrentLinkSpeedMbps")]
        public int CurrentLinkSpeedMbps { get; set; }

        [JsonPropertyName("LinkStatus")]
        public string LinkStatus { get; set; }

        [JsonPropertyName("PhysicalPortNumber")]
        public string PhysicalPortNumber { get; set; }

    }

    #region
    public class RedfishSystemResponse
    {
        public string? PowerState { get; set; }
        public string? Model { get; set; }
        public string? BiosVersion { get; set; }
        public string? HostName { get; set; }
        public TrustedModule[]? TrustedModules { get; set; }
        public Oem? Oem { get; set; }
    }

    public class TrustedModule
    {
        public string? FirmwareVersion { get; set; }
    }

    public class Oem
    {
        public Dell? Dell { get; set; }
    }

    public class Dell
    {
        public DellSystemWrapper1? DellSystem { get; set; }
    }

    public class DellSystemWrapper1
    {
        public string? ChassisServiceTag { get; set; }
    }

    

    public class RedfishAttributesResponse
    {
        public Dictionary<string, object>? Attributes { get; set; }
    }

    

    public class BiosResponse
    {
        public BiosAttributes? Attributes { get; set; }
    }

    public class BiosAttributes
    {
        public string? SystemBiosVersion { get; set; }
        public string? SystemModelName { get; set; }
    }
    public class LicenseCollectionResponse
    {
        public List<LicenseMember>? Members { get; set; }
    }

    public class LicenseMember
    {
        [JsonPropertyName("@odata.id")]
        public string? OdataId { get; set; }
    }
    // ==========================================
    // LICENSE DETAIL RESPONSE MODEL
    // ==========================================

    public class LicenseDetailResponse
    {
        public string? Description { get; set; }
    }

    #endregion

    #region Storage APIs
    public class StorageCollectionResponse
    {
        public List<OdataMember> Members { get; set; }
    }

    public class StorageDetailResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Status Status { get; set; }
        public List<OdataMember> Drives { get; set; }
    }

    public class DriveDetailResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long CapacityBytes { get; set; }
        public string Manufacturer { get; set; }
        public string MediaType { get; set; }
        public string Model { get; set; }
        public double NegotiatedSpeedGbs { get; set; }
        public Status Status { get; set; }
        public string Protocol { get; set; }
        //public int RotationSpeedRPM { get; set; }
        //public OemDrive Oem { get; set; }
    }


    //public class OemDrive
    //{
    //    public DellDrive Dell { get; set; }
    //}

    //public class DellDrive
    //{
    //    public DellPhysicalDisk DellPhysicalDisk { get; set; }
    //}

    public class DellPhysicalDisk
    {
        public string RaidStatus { get; set; }
        public long FreeSizeInBytes { get; set; }
    }
    #endregion

    public class CPUUsageResponse
    {
        public string Name { get; set; }
        public double Reading { get; set; }
    }

    public class MemoryChartResponse
    {
        public string Name { get; set; }
        public double Reading { get; set; }
    }


    public class IdracLogResponse
    {
        public List<IdracLogEntry> Members { get; set; }
        public string Name { get; set; }
    }

    public class IdracLogEntry
    {
        public string Id { get; set; }

        public DateTime Created { get; set; }

        public string GeneratorId { get; set; }

        public string Message { get; set; }

        public string MessageId { get; set; }

        public string Name { get; set; }

        public string Severity { get; set; }
    }
    public class IdracEventSubscriptionResponse
    {        
        [JsonPropertyName("Members")]
        public List<IdracSubscriptionMember> Members { get; set; }
    }

    public class IdracSubscriptionMember
    {
        [JsonPropertyName("@odata.id")]
        public string ODataId { get; set; }
    }
}
