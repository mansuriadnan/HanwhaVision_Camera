// src/constants/apiUrls.ts
const API_BASE_URL = import.meta.env.VITE_REACT_APP_API_BASE_URL;

const AuthEndPoint: string = `${API_BASE_URL}Auth`;
const UsersEndPoint: string = `${API_BASE_URL}Users`;
const RoleEndPoint: string = `${API_BASE_URL}Role`;
const permissionEndPoint: string = `${API_BASE_URL}Permission`;
// const CameraEndPoint: string = `${API_BASE_URL}Camera`;
const DeviceEndPoint: string = `${API_BASE_URL}Device`;
const ZoneEndPoint: string = `${API_BASE_URL}Zone`;
const RoleScreenMappingEndPoint: string = `${API_BASE_URL}RoleScreenMapping`;
const SiteEndPoint: string = `${API_BASE_URL}Site`;
const EmailTemplatesEndPoint: string = `${API_BASE_URL}EmailTemplate`;
const ClientSettingEndPoint: string = `${API_BASE_URL}ClientSetting`;
const LicenseSettingEndpoint: string = `${API_BASE_URL}license`;
const DashboardEndPoint: string = `${API_BASE_URL}DashboardPreference`;
const AuditEndPoint: string = `${API_BASE_URL}AuditLog`;
// const ZoneCameraEndPoint: string = `${API_BASE_URL}ZoneCamera`;
const FloorEndPoint: string = `${API_BASE_URL}Floor`;
const WidgetEndPoint: string = `${API_BASE_URL}Widget`;
const PeopleWidgetEndPoint: string = `${API_BASE_URL}PeopleWidget`;
const MonitoringEndPoint: string = `${API_BASE_URL}Monitoring`;
const ReportEndPoint: string = `${API_BASE_URL}Report`;
const CameraStreamEndPoint: string = `${API_BASE_URL}CameraStream`;
const MongoBackupRestoreEndPoint: string = `${API_BASE_URL}MongoBackupRestore`;
const VideoEndPoint: string = `${API_BASE_URL}Video`;
const SSLCertificateEndPoint: string = `${API_BASE_URL}SSLCertificate`;
const ExceptionEndPoint: string = `${API_BASE_URL}Exception`;
const DeviceExceptionEndPoint: string = `${API_BASE_URL}DeviceException`;
const VehicleOwnerEndPoint: string = `${API_BASE_URL}VehicleOwner`;
const CountryEndPoint: string = `${API_BASE_URL}Country`;
const ANPRVehicleEndPoint: string = `${API_BASE_URL}ANPRVehicle`;
const MaintenancePlanEndPoint: string = `${API_BASE_URL}MaintenancePlan`;
const MaintenanceScheduleEndPoint: string = `${API_BASE_URL}MaintenanceSchedule`;
const RMAEndPoint: string = `${API_BASE_URL}RMA`;
const LPREndPoint: string = `${API_BASE_URL}LicensePlateRecog`;
const MultiServerManagementEndPoint: string = `${API_BASE_URL}MultiServerManagement`;
const SSMServerManagementEndPoint: string = `${API_BASE_URL}SSMServerManagement`;
const SSMDashboardEndPoint: string = `${API_BASE_URL}SSMDashboard`;
const SSMDeviceDetailsEndPoint: string = `${API_BASE_URL}SSMDeviceDetails`;
const iDRACManagementEndPoint: string = `${API_BASE_URL}IDracManagement`;
const iDRACDashboardEndPoint: string = `${API_BASE_URL}IdracDashboard`;
const ExposeApiUserEndPoint: string = `${API_BASE_URL}ExposeApiUser`;


const apiUrls: Record<string, string> = {
  UserLogin: `${AuthEndPoint}/login`,
  RefreshToken: `${AuthEndPoint}/refreshToken`,
  GetAllPermission: `${permissionEndPoint}/GetAllPermission`,
  AddUser: `${UsersEndPoint}`,
  getUserPermission: `${RoleEndPoint}/UserRolePermissions`,
  CreateUser: `${UsersEndPoint}/CreateUser`,
  ChangePassword: `${UsersEndPoint}/ResetPassword`,
  ForgotPassword: `${UsersEndPoint}/ForgotPassword`,
  ForgotResetPassword: `${UsersEndPoint}/ForgotResetPassword`,
  ValidateOtp: `${UsersEndPoint}/ValidateOtp`,
  GetAllRole: `${RoleEndPoint}/GetAllRoles`,
  AddUpdateRole: `${RoleEndPoint}/AddUpdateRole`,
  UpdateRole: `${RoleEndPoint}/UpdateRole`,
  DeleteRole: `${RoleEndPoint}/DeleteRole`,
  GetRoleScreen: `${RoleScreenMappingEndPoint}/RoleScreenMappings`,
  AddRolePermissionScreen: `${RoleScreenMappingEndPoint}/AddRolePermission`,
  ResetPassword: `${UsersEndPoint}/ResetPasswords`,
  // AddCamera: `${CameraEndPoint}/AddCamera`,
  // AddFloorPlan: `${CameraEndPoint}/AddFloorPlan`,
  // GetCameraData: `${CameraEndPoint}/GetCameraData`,
  // GetAllFloorPlan: `${CameraEndPoint}/GetAllFloorPlan`,
  // GetAllSite: `${SiteEndPoint}/GetAllSite`,
  // CreateSite: `${SiteEndPoint}/CreateSite`,
  // UpdateSite: `${SiteEndPoint}/UpdateSite`,
  // SiteDelete: `${SiteEndPoint}/SiteDelete`,
  GetEmailTemplates: `${EmailTemplatesEndPoint}`,
  UpdateEmailTemplate: `${EmailTemplatesEndPoint}`,
  SendEmailTemplate: `${EmailTemplatesEndPoint}/TestEmail`,
  ValidateLicenseEndpoint: `${LicenseSettingEndpoint}/ValidateLicense`,
  GetHardwareIdEndpoint: `${LicenseSettingEndpoint}/GetHardwareId`,
  // SaveDashboardDesign: `${DashboardEndPoint}/SaveDashboardDesign`,
  // GetDashboardDesign: `${DashboardEndPoint}/GetDashboardPreference`,
  UploadLicenseKey: `${LicenseSettingEndpoint}/uploadLicenseKey`,
  UploadLicense: `${LicenseSettingEndpoint}/uploadLicense`,
  UploadFullLicense: `${LicenseSettingEndpoint}/UploadFullLicense`,
  GetcurrentLicenseDetail: `${LicenseSettingEndpoint}/LicenseDetail`,
  GetLicensehistory: `${LicenseSettingEndpoint}/LicenseHistory`,
  GetAuditLog: `${AuditEndPoint}/GetAuditLog`,
  GetCollectionList: `${AuditEndPoint}/AuditLogCollectionName`,

  UserResetPassword: `${UsersEndPoint}/UserResetPassword`,
  SaveSmtpSetting: `${ClientSettingEndPoint}/Smpt`,
  UploadClientLogo: `${ClientSettingEndPoint}/uploadClientLogo`,
  SaveOperationalTimeZoneSetting: `${ClientSettingEndPoint}/ClientOperationalTiming`,
  GetClientSettings: `${ClientSettingEndPoint}/ClientSetting`,
  GetTimeZoneDropdownData: `${ClientSettingEndPoint}/ClientTimeZones`,
  // GetClientLogo: `${ClientSettingEndPoint}/Logo`,
  GetFloorZonedata: `${RoleScreenMappingEndPoint}/FloorRoleScreenMappings`,
  SaveFloorZonedata: `${RoleScreenMappingEndPoint}/FloorRolePermission`,
  GetRoleScreenMappingWidgetData: `${RoleScreenMappingEndPoint}/Widget`,
  SaveWidgetsData: `${RoleScreenMappingEndPoint}/Widget`,
  // GetAllFloorPlanList: `${ZoneCameraEndPoint}/GetAllFloorPlan`,
  saveGoogleMapApIKeySetting: `${ClientSettingEndPoint}/GoogleApiKey`,
  SaveFtpSetting: `${ClientSettingEndPoint}/FTPConfiguration`,
  SaveRetentionSetting: `${ClientSettingEndPoint}/RetentionDBConfiguration`,
  //SaveSSLCertificate: `${ClientSettingEndPoint}/SSLCertificate`,
  SaveSSLCertificate: `${SSLCertificateEndPoint}/SSLCertificateUpload`,
  StreamVideo: `${VideoEndPoint}/StreamVideo`,
  SaveReportSchedulerSetting: `${ClientSettingEndPoint}/ReportSchedule`,
  TurnReportSchedule: `${ClientSettingEndPoint}/TurnReportSchedule`,
  SaveBackupDBConfiguration: `${ClientSettingEndPoint}/BackupDBConfiguration`,
  FloorZones: `${FloorEndPoint}/FloorZones`,
  GetAppMainLogo: `${ClientSettingEndPoint}/ApplicationMainLogo`,
  SaveANPRConfiguration: `${ClientSettingEndPoint}/ANPRImageLocation`,
  ResetParkingCount: `${ClientSettingEndPoint}/ResetParkingCount`,

  // Zone
  AddUpdateZone: `${ZoneEndPoint}/AddUpdateZone`,
  GetAllZoneByFloorId: `${ZoneEndPoint}/GetAllZoneByFloorId`,
  DeleteZoneById: `${ZoneEndPoint}/DeleteZoneById`,
  AddZonePlanDetail: `${ZoneEndPoint}/ZonePlanDetail`,
  DeleteMappedCamera: `${ZoneEndPoint}/DeleteZoneMappedDevice`,

  // Floor
  GetFloorList: `${FloorEndPoint}`,
  AddFloor: `${FloorEndPoint}`,
  DeleteFloor: `${FloorEndPoint}/DeleteFloor`,
  AddFloorPlanImage: `${FloorEndPoint}/FloorPlanImage`,
  GetFloorPlanImage: `${FloorEndPoint}/FloorPlanImage`,
  FetchFloorZoneNames: `${FloorEndPoint}/FloorZonesNameByIds`,
  GetFloorByServerIds: `${FloorEndPoint}/GetFloorByServerIds`,

  // Devices
  ManageAddDevice: `${DeviceEndPoint}`,
  GetAllDeviceList: `${DeviceEndPoint}/GetAllDevices`,
  DeleteDevices: `${DeviceEndPoint}/DeleteDevices`,
  GetAllDevicewithoutZone: `${DeviceEndPoint}/GetDevicesWithoutZones`,
  GetAllChannels: `${DeviceEndPoint}/GetAllChannels`,
  MapCameraListByFeatures: `${DeviceEndPoint}/MapCameraListByFeatures`,
  UploadBulkDevice: `${DeviceEndPoint}/UploadBulkDevice`,
  DownloadSampleFile: `${DeviceEndPoint}/DownloadSampleFile`,
  GetDevicesAsync: `${DeviceEndPoint}/GetDevicesAsync`,
  ExportDeviceCSV: `${DeviceEndPoint}/ExportDeviceCSV`,
  UpdateDeviceMaintenanceStatus: `${DeviceEndPoint}/UpdateDeviceMaintenanceStatus`,
  ManuallyOpenGateBarrier: `${DeviceEndPoint}/ManuallyOpenGateBarrier`,

  // Manage Users
  GetAllUser: `${UsersEndPoint}/GetAllUser`,
  UpdateUser: `${UsersEndPoint}`,
  UserDelete: `${UsersEndPoint}/DeleteUser`,

  //Manage Multisite
  AddChildSite: `${SiteEndPoint}/AddOrUpdateChildSite`,
  GetAllSite: `${SiteEndPoint}`,
  AddSubChildSite: `${SiteEndPoint}/AddOrUpdateSubChildSite`,
  DeleteChildSite: `${SiteEndPoint}/DeleteChildSite`,
  DeleteSubChildSite: `${SiteEndPoint}/DeleteSubChildSite`,

  //Dashboard
  SaveDashboardName: `${DashboardEndPoint}`,
  GetDashboardDesign: `${DashboardEndPoint}`,
  GetFloorListByPermission: `${FloorEndPoint}/FloorsByPermission`,
  GetZonesListByFloorIdsPermission: `${FloorEndPoint}/FloorZoneByPermission`,
  DeleteDashboard: `${DashboardEndPoint}/DeleteDashboard`,
  GetAllActiveServerForDashboard : `${MultiServerManagementEndPoint}/GetAllActiveServerForDashboard`,

  //Notification
  GetNOtification: `${DashboardEndPoint}/UserNotification`,
  MarkReadUserNotification: `${DashboardEndPoint}/MarkReadUserNotification`,
  UserNotificationCount: `${DashboardEndPoint}/UserNotificationCount`,

  //Monitoring
  GetMonitoringDesign: `${MonitoringEndPoint}/GetMonitoring`,
  SaveMonitoringName: `${MonitoringEndPoint}/AddUpdateMonitoring`,
  AddUpdateGroup: `${MonitoringEndPoint}/AddUpdateGroup`,
  GetGroupAndItemData: `${MonitoringEndPoint}/GetMonitoringGroupAndItem`,
  AddUpdateGroupItem: `${MonitoringEndPoint}/AddUpdateGroupItem`,
  DeleteGroup: `${MonitoringEndPoint}/DeleteMonitoringGroup`,
  DeleteMonitoring: `${MonitoringEndPoint}/DeleteMonitoring`,
  DeleteGroupItem: `${MonitoringEndPoint}/DeleteMonitoringGroupItemSite`,

  //Widget
  CameraCountByFeatures: `${WidgetEndPoint}/CameraCountByFeatures`,
  TotalCameraCount: `${WidgetEndPoint}/TotalCameraCount`,
  // VehicleByTypeCount: `${WidgetEndPoint}/VehicleByTypeCount`,
  CameraCountByModel: `${WidgetEndPoint}/CameraCountByModel`,
  AveragePeopleCount: `${WidgetEndPoint}/AveragePeopleCount`,
  VehicleQueueAnalysis: `${WidgetEndPoint}/VehicleQueueAnalysis`,
  CumulativePeopleCountChart: `${WidgetEndPoint}/CumulativePeopleCountChart`,
  PeopleInOutCountChart: `${WidgetEndPoint}/PeopleInOutCountChart`,
  PeopleInOutChart: `${WidgetEndPoint}/PeopleInOutChart`,
  PedestrianAnalysis: `${WidgetEndPoint}/PedestrianAnalysis`,
  SlipFallAnalysis: `${WidgetEndPoint}/SlipFallAnalysis`,
  VehicleCapacityUtilization: `${WidgetEndPoint}/VehicleCapacityUtilization`,
  VehicleCameraCapacityUtilizationAnalysisByZones: `${WidgetEndPoint}/VehicleCameraCapacityUtilizationAnalysisByZones`,
  VehicleByTypeLineChartData: `${WidgetEndPoint}/VehicleByTypeLineChartData`,
  WrongWayAnalysis: `${WidgetEndPoint}/WrongWayAnalysis`,
  VehicleUTurnAnalysis: `${WidgetEndPoint}/VehicleUTurnAnalysis`,
  VehicleCameraCapacityUtilizationByZones: `${WidgetEndPoint}/VehicleCameraCapacityUtilizationByZones`,
  AverageVehicleCount: `${WidgetEndPoint}/AverageVehicleCount`,
  GetAllDeviceData: `${WidgetEndPoint}/GetAllDeviceData`,
  VehicleTurningMovementAnalysis: `${WidgetEndPoint}/VehicleTurningMovementAnalysis`,
  ShoppingCartQueueAnalysis: `${WidgetEndPoint}/ShoppingCartQueueAnalysis`,
  ForkliftQueueAnalysis: `${WidgetEndPoint}/ForkliftQueueAnalysis`,
  VehicleSpeedViolationAnalysis: `${WidgetEndPoint}/VehicleSpeedViolationAnalysis`,
  BlockedExitDetecion: `${WidgetEndPoint}/BlockedExitAnalysis`,
  VehicleInOutChart: `${WidgetEndPoint}/VehicleInOutChart`,
  // VehicleInOutTotal: `${WidgetEndPoint}/VehicleInOutTotal`,
  AverageVehicleCountChart: `${WidgetEndPoint}/AverageVehicleCountChart`,
  VehicleInOutCountChart: `${WidgetEndPoint}/VehicleInOutCountChart`,
  TrafficJamAnalysis: `${WidgetEndPoint}/TrafficJamAnalysis`,
  StoppedVehicleCountbyType: `${WidgetEndPoint}/StoppedVehicleByTypeAnalysis`,
  ForkliftCountAnalysis: `${WidgetEndPoint}/ForkliftCountAnalysis`,
  PeopleQueueAnalysis: `${WidgetEndPoint}/PeopleQueueAnalysis`,
  ProxomityDetectionAnalysis: `${WidgetEndPoint}/ProxomityDetectionAnalysis`,
  HeatMapAllDevice: `${DeviceEndPoint}/MapCameraListByFeatures`,
  DeviceByFloorZone: `${DeviceEndPoint}/GetDevicesByFloorAndZones`,
  HeatmapDataByDevice: `${WidgetEndPoint}/HeatMapWidgetData`,
  ForkliftSpeedDetectionAnalysis: `${WidgetEndPoint}/ForkliftSpeedDetectionAnalysis`,
  ShoppingCartCountAnalysis: `${WidgetEndPoint}/ShoppingCartCountAnalysis`,
  WidgetWiseHeatMapAllDevice: `${DeviceEndPoint}/CameraListHeatmap`,
  PeopleCapacityUtilizationforCSV: `${WidgetEndPoint}/PeopleCapacityUtilization/csv`,
  PeopleCameraCapacityUtilizationAnalysisByZonesforCSV: `${WidgetEndPoint}/PeopleCameraCapacityUtilizationAnalysisByZones/csv`,
  AveragePeopleCountChartforCSV: `${WidgetEndPoint}/AveragePeopleCountChart/csv`,
  NewVsTotalVisitorsChartforCSV: `${WidgetEndPoint}/NewVsTotalVisitorChart/csv`,
  CameraDisconnectedTrackerAnalysis: `${WidgetEndPoint}/CameraDisconnectedTrackerAnalysis`,
  MaskDetectionAnalysis: `${WidgetEndPoint}/MaskDetectionAnalysis`,
  VehicleParkingAnalysis: `${WidgetEndPoint}/VehicleParkingAnalysis`,
  VehicleParkingByZones: `${WidgetEndPoint}/VehicleParkingByZones`,
  ANPRVehicleParkingByZones: `${WidgetEndPoint}/ANPRVehicleParkingByZones`,
  ANPRVehicleParking: `${WidgetEndPoint}/ANPRVehicleParking`,

  // Map Widgets
  PeopleCountForMap: `${WidgetEndPoint}/PeopleCountingmap`,
  VehicleCountForMap: `${WidgetEndPoint}/VehicleCountForMap`,
  SlipandFallDetectionForMap: `${WidgetEndPoint}/SlipandFallDetectionForMap`,
  PedestrianDetectionForMap: `${WidgetEndPoint}/PedestrianDetectionForMap`,
  VehicleQueueManagementForMap: `${WidgetEndPoint}/VehicleQueueManagementForMap`,
  VehicleSpeedDetectionForMap: `${WidgetEndPoint}/VehicleSpeedDetectionForMap`,
  TrafficJamDetectionForMap: `${WidgetEndPoint}/TrafficJamDetectionForMap`,
  ShoppingCountForMap: `${WidgetEndPoint}/ShoppingCountForMap`,
  ForkliftCountForMap: `${WidgetEndPoint}/ForkliftCountForMap`,

  //Setting - General
  SaveTimeZoneSetting: `${ClientSettingEndPoint}/ClientOperationalTimeZone`,
  SaveOperationalTimeSetting: `${ClientSettingEndPoint}/ClientOperationalTiming`,
  BackupDatabase: `${MongoBackupRestoreEndPoint}`,
  RestoreDatabase: `${MongoBackupRestoreEndPoint}`,

  RestoreChunkDatabase: `${MongoBackupRestoreEndPoint}/chunk`,
  RestoreFinalDatabase: `${MongoBackupRestoreEndPoint}/finalize`,

  //User Profile
  GetUserProfile: `${UsersEndPoint}/UserProfile`,
  UploadProfileImage: `${UsersEndPoint}/UploadProfileImage`,
  SaveUserPreferences: `${UsersEndPoint}/UserPreferences`,

  // People Widget
  // GenderWisePeopleCounting: `${PeopleWidgetEndPoint}/GenderWisePeopleCounting`,
  PeopleCameraCapacityUtilizationByZones: `${PeopleWidgetEndPoint}/PeopleCameraCapacityUtilizationByZones`,
  PeopleCameraCapacityUtilizationAnalysisByZones: `${PeopleWidgetEndPoint}/PeopleCameraCapacityUtilizationAnalysisByZones`,
  PeopleCapacityUtilization: `${PeopleWidgetEndPoint}/PeopleCapacityUtilization`,
  // NewVsTotalVisitorCount: `${PeopleWidgetEndPoint}/NewVsTotalVisitorCount`,
  PeopleCountByZones: `${PeopleWidgetEndPoint}/PeopleCountByZones`,
  PeopleCountByZonesCsv: `${WidgetEndPoint}/PeopleCountByZones/csv`,
  GenderWisePeopleCountAnalysis: `${PeopleWidgetEndPoint}/GenderWisePeopleCountAnalysis`,
  AgeWisePeopleCountAnalysisCsv: `${WidgetEndPoint}/AgeWisePeopleCountAnalysis`,
  AveragePeopleCountChart: `${PeopleWidgetEndPoint}/AveragePeopleCountChart`,
  NewVsTotalVisitorsChart: `${PeopleWidgetEndPoint}/NewVsTotalVisitorChart`,
  // PeopleInOutTotal: `${PeopleWidgetEndPoint}/PeopleInOutTotal`,
  AgeWisePeopleCountAnalysis: `${PeopleWidgetEndPoint}/AgeWisePeopleCountAnalysis`,
  GenderWisePeopleCountAnalysisCsv: `${WidgetEndPoint}/GenderWisePeopleCountAnalysis`,

  //Event logs
  GetAllEventLogsList: `${DeviceEndPoint}/GetDeviceEventsLogs`,
  UpdateEventLogsStatus: `${DeviceEndPoint}/UpdateDeviceEventsStatus`,

  //Exception logs
  GetAllExceptionLogsList: `${ExceptionEndPoint}/GetExceptionLogs`,
  ExceptionEmail: `${ExceptionEndPoint}/SendExceptionAlertEmail`,
  //reports
  GetAllReports: `${ReportEndPoint}/GetAllReport`,
  AddEditReports: `${ReportEndPoint}/AddUpdateReport`,
  DeleteReport: `${ReportEndPoint}/DeleteReportById`,
  GenerateReportPDF: `${ReportEndPoint}/GenerateReportPDF`,
  GetReportDetailsbyId: `${ReportEndPoint}/GenerateReportById`,
  GetAllFloorBySiteId: `${ReportEndPoint}/GetAllFloorBySiteId`,
  GetAllZoneBySiteId: `${ReportEndPoint}/GetAllZoneBySiteId`,

  CameraStream: `${CameraStreamEndPoint}`,

  // Device Exception
  DeviceException: `${DeviceExceptionEndPoint}/DeviceException`,

  // ANPR Owner
  AddVehicleOwner: `${VehicleOwnerEndPoint}`,
  GetVehicleOwnerList: `${VehicleOwnerEndPoint}/GetVehicleOwnerList`,
  GetAllVehicleOwner: `${VehicleOwnerEndPoint}/GetAllVehicleOwner`,
  DeleteOwner: `${VehicleOwnerEndPoint}/DeleteOwner`,
  DownloadOwnerSampleFile: `${VehicleOwnerEndPoint}/DownloadSampleFile`,
  UploadBulkOwner: `${VehicleOwnerEndPoint}/UploadVehicleOwner`,

  // ANPR Vehicle
  AddVehicle: `${ANPRVehicleEndPoint}`,
  GetAllVehicleByOwner: `${ANPRVehicleEndPoint}/GetAllANPRVehicleByOwner`,
  DeleteANPRVehicle: `${ANPRVehicleEndPoint}/DeleteANPRVehicle`,
  DownloadVehicleSampleFile: `${ANPRVehicleEndPoint}/DownloadSampleFile`,
  UploadBulkVehicle: `${ANPRVehicleEndPoint}/UploadANPRVehicle`,

  //LPR
  GetAllLprDetails: `${LPREndPoint}/GetAllLprDetails`,
  ExportLPRDetailsCSV: `${LPREndPoint}/ExportLPRDetailsCSV`,
  ANPRImage : `${LPREndPoint}/anpr-image`,

  // Country
  GetAllCountryList: `${CountryEndPoint}`,

  // Maintenance Plan
  GetMaintenancePlan: `${MaintenancePlanEndPoint}/GetMaintenancePlan`,
  SaveMaintenancePlan: `${MaintenancePlanEndPoint}`,
  DeleteMaintenancePlan: `${MaintenancePlanEndPoint}/DeleteMaintenancePlan`,
  GetAllDeviceForMaintenancePlan: `${MaintenancePlanEndPoint}/GetAllDeviceForMaintenancePlan`,

  // Maintenance Schedule
  AddUpdateMaintenanceSchedule: `${MaintenanceScheduleEndPoint}/AddUpdateMaintenanceSchedule`,
  GetMaintenanceSchedule: `${MaintenanceScheduleEndPoint}/GetMaintenanceSchedule`,
  UpdateMaintenanceScheduleStatus: `${MaintenanceScheduleEndPoint}/UpdateMaintenanceScheduleStatus`,
  GetLiveImage: `${MaintenanceScheduleEndPoint}/GetLiveImage`,
  GetBeforeAfterImage: `${MaintenanceScheduleEndPoint}/GetBeforeAfterImage`,
  ExportMaintenanceScheduleCSV: `${MaintenanceScheduleEndPoint}/ExportMaintenanceScheduleCSV`,
  comapareImage: `${MaintenanceScheduleEndPoint}/CompareImages`,
  CameraMaintenanceStatusWidget: `${WidgetEndPoint}/CameraMaintenanceStatusWidget`,
  CameraInMaintenance: `${WidgetEndPoint}/CameraInMaintenance`,

  // RMA
  GetRMA: `${RMAEndPoint}/GetRMA`,
  SaveRMA: `${RMAEndPoint}`,
  DeleteRMA: `${RMAEndPoint}/DeleteRMA`,
  MaintenanceMode: `${RMAEndPoint}/CamerasInRMAWidget`,
  ExportCamerasInRMAWidgetCSV: `${RMAEndPoint}/ExportCamerasInRMAWidgetCSV`,
  ExportRMACSV: `${RMAEndPoint}/ExportRMACSV`,

  // Auditlogs
  GetAllAuditLogs: `${AuditEndPoint}/GetAuditLogs`,
  ExportAuditLogsCSV: `${AuditEndPoint}/ExportAuditLogsCSV`,

  //Multi server
  AddUpdateServer: `${MultiServerManagementEndPoint}/AddUpdateServer`,
  GetAllServer: `${MultiServerManagementEndPoint}/GetAllServer`,
  DeleteServer: `${MultiServerManagementEndPoint}/DeleteServer`,
  EnableServer: `${MultiServerManagementEndPoint}/EnableServer`,
  FetchDBString : `${MultiServerManagementEndPoint}/MongoDatabaseConnection`,

  //SSM Server management
  AddUpdateSSMServer: `${SSMServerManagementEndPoint}/AddUpdateServer`,
  SSMServerData : `${SSMServerManagementEndPoint}/GetAllSsmServers`,
  DeleteSSMServer : `${SSMServerManagementEndPoint}/DeleteSsmServer`,

  //SSM Dashboard

  SSMDashboardData: `${SSMDashboardEndPoint}/ssm-server-hierarchy`,
  SSMserverCPUUtilization : `${SSMDashboardEndPoint}/ssm-server-cpu-utilization`,
  SSMserverRAMUtilization : `${SSMDashboardEndPoint}/ssm-server-ram-utilization`,
  SSMServerDeviceDetail : `${SSMDashboardEndPoint}/ssm-server-device-details`,
  SSMserverAvailability : `${SSMDashboardEndPoint}/ssm-server-availability`,
  DeviceAvailability : `${SSMDashboardEndPoint}/ssm-device-availability`,
  ExportHealthReportData :`${SSMDashboardEndPoint}/ssm-server-health-report`,
  

  //Manage iDRAC
  AddUpdateiDRAC: `${iDRACManagementEndPoint}/AddUpdateIDracServer`,
  GetAlliDRACServer: `${iDRACManagementEndPoint}/GetAllIDracServer`,
  iDRACDelete: `${iDRACManagementEndPoint}/DeleteIDracServer`,
  addAlarm:  `${iDRACManagementEndPoint}/AddIdracEventAlarm`, 
  AlarmDelete :  `${iDRACManagementEndPoint}/DeleteIdracSubscribedEvent`,

  //iDRAC Dashboard
  iDRACServerList: `${iDRACDashboardEndPoint}/idrac-server-list`,
  fetchEventLogDetail: `${iDRACDashboardEndPoint}/idrac-event-logs`,
  iDRACLedIndicator: `${iDRACDashboardEndPoint}/idrac-led-indicator`,
  iDRACPowerAction:  `${iDRACDashboardEndPoint}/idrac-power-action`,
  iDRACSystemInfo : `${iDRACDashboardEndPoint}/idrac-system-information`,
  fetchSystemLogDetail : `${iDRACDashboardEndPoint}/idrac-system-logs`,
  StreamCooling: `${iDRACDashboardEndPoint}/StreamCooling`,
  StreamMEMUsage: `${iDRACDashboardEndPoint}/StreamMemoryUsage`,
  StreamEmbeddedNetworkUsage: `${iDRACDashboardEndPoint}/StreamEmbeddedNetworkUsage`,
  StreamIntegratedNetworkUsage: `${iDRACDashboardEndPoint}/StreamIntegratedNetworkUsage`,
  StreamCPUUsage: `${iDRACDashboardEndPoint}/StreamCPUUsage`,
  stream: `${iDRACDashboardEndPoint}/stream`,

  // ExposeApiUser
  GetExposeApiUsers: `${ExposeApiUserEndPoint}/get-all`,
  AddEditExposeApiUser: `${ExposeApiUserEndPoint}/add-edit`,
  DeleteExposeApiUser: `${ExposeApiUserEndPoint}/delete`,
};

export default apiUrls;
