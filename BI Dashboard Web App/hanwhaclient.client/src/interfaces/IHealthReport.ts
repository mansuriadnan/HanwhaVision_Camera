// ─── ROUTING PARAMS TYPE ──────────────────────────────────────────────────────

export interface HealthReportRouteData {
  siteList: string[];   // or the exact type your project uses for siteList
  siteNames: string[];
  startDateUtc: string; // ISO
  endDateUtc: string;      // ISO
}

// ─── API REQUEST / RESPONSE TYPES ────────────────────────────────────────────

export interface ApiOfflineEvent {
  id: string;
  offlineTime: string;
  onlineTime: string;
  duration: number;
}

export interface ApiStopRecording {
  id: string;
  stopRecordingTime: string;
  startRecordingTime: string;
  duration: number;
}

export interface ApiDevice {
  id: string;
  ip: string;
  name: string;
  model: string;
  online: string;
  offlineIncidence: number;
  offlineDuration: string;
  rec: string;
  noRecIncidence: number;
  noRecDuration: string;
  status: string;
  offlineEvents: ApiOfflineEvent[];
  stopRecording: ApiStopRecording[];
}

export interface ApiCpuSpike {
  id: string;
  cpuSpikeStartDatetime: string;
  cpuNormalDatetime: string;
}

export interface ApiRamSpike {
  id: string;
  ramSpikeStartDatetime: string;
  ramNormalDatetime: string;
}

export interface ApiDiskSpike {
  id: string;
  drive: string;
  ipAddress: string;
  diskSpikeStartDatetime: string; // ISO date string
  diskNormalDatetime: string;     // ISO date string
}

export interface ApiServerOfflineOnline {
  id: string;
  onlineTime: string;
  offlineTime: string;
}

export interface ApiServer {
  id: string;
  name: string;
  port: number;
  status: string;   
  ipAddress : string;    
  siteId: string;
  siteName: string;
  uptimePercent: number;
  downTimeMin: number;
  cpuAvgPercent: number;
  ramAvgPercent: number;
  totalCameras: number;
  offlineCameras: number;
  cpuMaxUtilizationPercent: number;
  totalRamAvailable: number;
  ramMaxUtilizationPercent: number;
  totalDiskSpace: number;
  freeDiskSpace: number;
  diskUtilizationPercent: number;
  serverOfflineOnlineData: ApiServerOfflineOnline[];
  cpuSpikeData: ApiCpuSpike[];
  ramSpikeData: ApiRamSpike[];
  diskSpikeData: ApiDiskSpike[];
  deviceOffline: ApiDevice[];
}

export interface HealthReportApiResponse {
  data: ApiServer[];
}


export interface DerivedOverview {
  totalServer: number;
  totalCamera: number;
  avgServerUptime: string;
  avgCPU: string;
  avgRAM: string;
  sitesLabel: string;
}

export type ServerStatus = "Warning" | "Connected" | "Disconnected";