
export interface IiDRACDetail {
  id?: string;
  IPAddress: string;
  port: string;
  userName: string;
  password: string;
  parentSiteId: string;
  childSiteId : string;
}

export interface IiDRACServer {
  serverName: string;
  ipAddress: string;
  port: string;
  parentSiteId: string;
  childSiteId: string;
  userName: string;
  password: string;
  id: string;
  cpuLoad : number;
  temperature: number;
  memoryUsage:number;
  alarms : IAlarmData[];
  createdOn: string;   
  createdBy: string;
  updatedOn: string;   
  updatedBy: string;
}

export interface IReferenceData {
  parentSite: IReferenceOption[];
  childSite: IReferenceOption[];
  createdBy: IReferenceOption[];
  updatedBy: IReferenceOption[];
}

export interface IReferenceOption {
  value: string;
  label: string;
  readOnly: boolean;
  isDisabled: boolean;
  isDefault: boolean;
  parentValue: string | null;
  type: string;
  order: number | null;
  hoverText: string;
  additionalProperty: string;
}

export interface IAlarmData {
  id: string;
  name: string;
  event: string;
  timeLimit: number;
  alarmSubscriptionId: string | null;
}

export interface IalarmPayload {
  name: string,
  event: string,
  timeLimit: number | null,
  userName: string,
  password: string,
  serverId: string,
  ipAddress: string
}

export interface IserverPayload { 
  parentSiteId : string
}

export interface IserverData {
  id: string;
  serverName : string;
  parentSiteId: string;
  childSiteId: string | null;
  ipAddress: string;
  port: string;
}

export interface IdeleteAlarmPayload { 
  alarmEventId: string;
  serverId: string
}

export interface IIdracServerEventPayload {
  pageNumber: number;
  pageSize: number;
  sortBy: string;
  sortOrder: number;
  ipAddress: string;
}

export interface IIdracServerSystemPayload {
  pageNumber: number;
  pageSize: number;
  sortBy: string;
  sortOrder: number;
  idracServerId: string;
}

export interface IEventLogDetail {
  id: string;
  description: string;
  eventTimestamp: string;
  severity: string;
}
export interface ISystemLogDetail {
  id: string;
  description: string;
  eventTimestamp: string;
  severity: string;
}

export interface IIdracServerEventResponse {
  eventLogsDetails: IEventLogDetail[];
  totalCount: number;
}

export interface ILEDPayload {
  serverId: string;
  ipAddress: string;
  ledState: boolean;
}

export interface IPowerPayload {
  serverId: string;
  ipAddress: string;
  resetType: string;
}

export interface IIdracSystemInfoPayload{
  ipAddress : string;
}

export interface ISystemInformation {
  powerState: string;
  model: string;
  hostName: string;
  operatingSystem: string;
  operatingSystemVersion: string;
  serviceTag: string;
  biosVersion: string;
  idracFirmwareVersion: string;
  ipAddress: string;
  idracMacAddress: string;
  license: string;
}

export interface IMemoryDetail {
  TotalMemory: number;
  UsedSlots: number;
  TotalSlots: number;
  TotalSpeed: number;
  Health: string;
  MemoryList: IMemoryInfo[];
}

export interface IMemoryInfo {
  RamName: string;
  Size: number;
  Type: string;
  Speed: number;
  Manufacturer: string;
  Health: string;
}

export interface IProcessorDetail {
  TotalCpu: number;
  UsedCpu: number;
  TotalCores: number;
  TotalThreads: number;
  SvgSpeed: number;
  Health: string;
  ProcessorList: IProcessorInfo[];
}

export interface IProcessorInfo {
  CpuName: string;
  Model: string;
  Cores: number;
  Threads: number;
  MaxSpeed: number;
  CurrentSpeed: number;
  Health: string;
}

export interface ICoolingFan {
  FanName: string;
  FanSpeed: number;
  Health: string;
}

export interface IPowerSupplyDetail {
  TotalCapacity: number;
  CurrentUsage: number;
  Redundancy: string;
  Health: string;
  PowerSupplyList: IPowerSupplyInfo[];
}

export interface IPowerSupplyInfo {
  Psu: string;
  Capacity: number;
  Output: number;
  InputVoltage: number;
  CurrentVoltage: number;
  Health: string;
}

export interface ITemperature {
  Name: string;
  PhysicalContext: string;
  Temperature: number;
  health: string;
}
export interface INetworkCard {
    ProductName: string;
    Protocol: string;
    VendorName: string;
    ActiveLinkTechnology: string;
    AssociatedNetworkAddress: string;
    LinkStatus: string;
    CurrentLinkSpeed: number;
    PhysicalPortNumber: string;
    health: string;
}
export interface IEmbeddedNetworkCardInfo {
    Manufacturer: string;
    Model: string;
    FirmwarePackageVersion: string;
    Health: string;
    NetworkCardList: INetworkCard[];
}

export interface IIntegratedNetworkCardInfo {
    Manufacturer: string;
    Model: string;
    FirmwarePackageVersion: string;
    Health: string;
    NetworkCardList: INetworkCard[];
}
export interface IStorageController {
    Id: string | null;
    Name: string;
    Health: string;
}

export interface IDriveInfo {
  DiskName: string;
  Size: number;
  Type: string;
  Protocol: string;
  Rpm: number;
  Health: string;
}

export interface IStorageInfo {
    Drives: IDriveInfo[];
}
export interface IserverHealth {
  ServerId : string;
  Health : string;
}
export interface IdracDashboardModel {
    IdracServerId: string;
    Memory: IMemoryDetail;
    Processor: IProcessorDetail;
    Cooling: ICoolingFan[];
    Temperature: ITemperature[];
    PowerSupply: IPowerSupplyDetail;
    EmbeddedNetworkCard: IEmbeddedNetworkCardInfo;
    IntegratedNetworkCard: IIntegratedNetworkCardInfo;
    Storage: IStorageInfo;
    Id: string;
    CreatedOn: string | null;
    CreatedBy: string | null;
    UpdatedOn: string | null;
    UpdatedBy: string | null;
    IsDeleted: boolean;
    DeletedOn: string | null;
}

export interface IChartData {
  time: string;
  value: number;
  name: string;
  link?: string;
}