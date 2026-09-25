export interface ISSMServers {
  ipAddress: string;
  port: string;
  userName: string;
  password: string;
  parentSiteId: string;
  childSiteId : string;
}
export interface ISSMServersList {
  ipAddress: string;
  port: string;
  parentSiteId: string;
  childSiteId: string;
  username: string;
  password: string;
  id: string;
  createdOn: string;   
  createdBy: string;
  updatedOn: string;   
  updatedBy: string;
}

export interface ISSMPayload {
  parentSiteIds: string[];
  date: string;
}


export interface ApiServer {
  id: string;
  name: string;
  ipAddress: string;
  port: number;
  status: string;
  totalProcessorUsage: string;
  totalMemoryUsage: string;
  diskFreePercentage: string;
  diskTotalSize: string;
  diskFreeSize: string;
  disks: ApiDisk[]; 
  totalCameraCount: number;
  failureCameraCount: number;
}

export interface ApiDisk {
  drive: string;   // "C" | "D" | "E" etc.
  free: number;    // MB
  total: number;   // MB
}

export interface ApiSubSite {
  childSiteId: string;
  childSiteName : string;
  servers: ApiServer[];
}

export interface ApiSiteData {
  parentSiteId: string;
  parentSiteName: string;
  parentServers: ApiServer[];
  subSites: ApiSubSite[];
}

export interface MappedDrive {
  name: string;
  totalCapacity: number;
  used: number;
}

export interface MappedServer {
  id:string;
  name: string;
  ip: string;
  cameras: number;
  online: number;
  offline: number;
  cpu: number;
  ram: number;
  totalStorage: number;
  drive: MappedDrive[];
  status: string;
}

export interface MappedSubSite {
  siteName: string;
  totalCamera: number;
  totalSSM: number;
  failureCamera: number;
  failureSSM: number;
  servers: MappedServer[];
}

export interface MappedSite {
  siteID: string;
  siteName: string;
  totalCamera: number;
  totalSSM: number;
  failureCamera: number;
  failureSSM: number;
  servers: MappedServer[];
  subsite: MappedSubSite[];
}

export interface SiteSummary {
  totalCamera: number;
  totalSSM: number;
  failureCamera: number;
  failureSSM: number;
}

export interface ISSMDevicePayload {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string | null;
  sortOrder?: number;
  searchText?: string;
  serverId : string;
  dateFilter : string | null;
}

export interface ISSMDeviceDetail {
  id: string;
  name: string;
  cameraModel: string;
  ipAddress: string;
  location: string;
  cameraStatus: "Online" | "Offline" | string; // keep flexible if more statuses come
  recordingStatus: string | null;
}

export interface ISSMCPUPayload {
  serverId: string;
  date: string |null;
}

export interface IavailabilityPayload {
  serverId: string;
  searchDate: string | null;
}

export interface IdeviceAvailabilityPayload {
  deviceId: string;
  searchDate: string | null;
}

export interface ChartDataItem {
  createdOn: string;
  totalUsage: string;
}

export interface CpuUsageResponse {
  chartData: ChartDataItem[];
  cpuSystemUsage: number;
  cpuMediaUsage: number;
}

export interface RamUsageResponse {
  chartData: ChartDataItem[];
  memorySystemUsage: number;
  memoryMediaUsage: number;
}
export interface SsmServerAvailabilityResponse {
  id: string;
  offlineTime: string;       
  onlineTime?: string | null;
}


export interface HealthReportRequest {
  ssmSiteIds: string[];
  startDateUtc: string;
  endDateUtc: string;
}
