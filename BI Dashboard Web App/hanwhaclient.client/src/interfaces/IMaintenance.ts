import dayjs, { Dayjs } from "dayjs";

export interface IPlanProps {
  id?: string;
  planName: string;
  duration: number | null;
  startDate: Dayjs | null;
  endDate: Dayjs | null;
  deviceIds: string[];
  deviceNames?: string[];
  floorIds?: string[];
  zoneIds?: string[];
}

export interface planAddEditFormProps {
  onClose: () => void;
  planData?: IPlanProps;
  refreshData: () => void;
}

export interface IDeviceList {
  id: string;
  deviceName: string;
  floorId:string;
  zoneId:string;
}

export interface IGetAllPlanRequestProps {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string | null;
  sortOrder?: number;
  searchText?: string;
}

export interface deleteProps {
  id: string;
}

export interface scheduleAddFormProps {
  onClose: () => void;
  // planData?: IPlanProps;
  refreshData: () => void;
}

export interface addScheduleReq {
  dueDate: Dayjs | null;
  statusHistory: { notes: string }[];
  deviceId: string;
}

export interface scheduleFilter {
  floorIds: string[];
  zoneIds: string[];
  dueFilter: string;
  statusFilter: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string | null;
  sortOrder?: number;
}

export interface addScheduleform {
  startDate: Dayjs | null;
  notes: string;
  deviceId: string;
  floorId:string;
  zoneId:string;
}

export interface statusHistory {
  statusDatetime: string;
  notes: string;
  status: string;
}

export interface scheduleList {
  id: string;
  deviceId: string;
  deviceName: string;
  deviceType: string;
  model: string;
  location: string;
  serialNumber: string;
  planName: string;
  maintenanceDueDate: string;
  status: string;
  ipAddress: string;
  statusHistory: statusHistory[];
  beforeCameraImage: string;
  afterCameraImage: string;
}

export interface deviceImgReq {
  deviceId: string;
}

export interface updateStatusForm {
  statusDatetime: Dayjs | null;
  notes: string;
  status: string;
  maintenanceScheduleId: string;
}

export interface updateStatusProps {
  selectedSchedule: scheduleList | undefined;
  beforeImg: string;
  afterImg: string;
  refreshData: () => void;
  onClose: () => void;
}

export interface RMAAddEditFormProps {
  onClose: () => void;
  RMAData?: IRMAProps;
  refreshData: () => void;
}

export interface RMAFormProps {
  deviceId: string;
  rmaStatus: string;
  inProgressNotes: string;
  completedNotes: string;
  startDate: Dayjs | null;
  endDate: Dayjs | null;
  floorId:string;
  zoneId:string;
}

export interface IGetAllRMARequestProps {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string | null;
  sortOrder?: number;
  searchText?: string;
  floorIds: string[];
  zoneIds: string[];
  startDate: string;
  rmaStatus: string;
}

export interface IRMAProps {
  id?: string;
  deviceId: string;
  deviceName: string;
  deviceType: string;
  location: string;
  model: string;
  rmaStatus: string;
  serialNumber: string;
  inProgressNotes: string;
  completedNotes: string;
  startDate: Dayjs | null;
  endDate: Dayjs | null;
  floorId:string;
  zoneId:string;
}

export interface RMAFilterprops {
  floorIds: string[];
  zoneIds: string[];
  startDate: Dayjs | null;
  rmaStatus: string;
}

export interface getImgReqProps {
  imageFullPath: string;
}
