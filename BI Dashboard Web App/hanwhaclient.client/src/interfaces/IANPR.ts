import dayjs, { Dayjs } from "dayjs";

export interface OwnerInputProps {
  id?: string;
  registrationType: string;
  ownerName: string;
  building: string;
  buildingUnit: string;
  email: string;
  contactNumber: string;
  allowedVehicle: number;
  allowedFromTime: Dayjs | null;
  allowedToTime: Dayjs | null;
  allowedGates: string[];
  enabledAlarmForOverstay: boolean;
  enabledAlarmFor24HStay: boolean;
  ownerValidTo: Dayjs | null;
}

export interface IGetAllOwnerRequestProps {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string | null;
  sortOrder?: number;
  searchText?: string;
}

export interface IOwnerList {
  id?: string;
  plate?: string;
  country?: string;
  state?: string;
  entryGate?: string;
  entryTime?: string | null;
  exitGate?: string;
  exitTime?: string | null;
  ownerName?: string;
  registrationType?: string;
  building?: string;
  buildingUnit?: string;
  contact?: string;
  email?: string;
  make?: string;
  model?: string;
  color?: string;
  message?: string;
  createdOn?: string | null;
  smallImage?:string;

  // existing fields (keep if still used elsewhere)
  allowedVehicle?: number;
  allowedFromTime?: Dayjs | null;
  allowedToTime?: Dayjs | null;
  allowedGates?: string[];
  allowedGateNames?: string[];
  enabledAlarmForOverstay?: boolean;
  enabledAlarmFor24HStay?: boolean;
}

export interface OwnerAddEditFormProps {
  onClose: () => void;
  ownerData?: IOwnerList;
  refreshData: () => void;
}

export interface IVehicleList {
  id: string;
  vehicleOwnerId?: string;
  country: string;
  countryId: string;
  countryName: string;
  state: string;
  series: string;
  vehicleNumber: number;
  plateCode: string;
  plateCategory: string;
  make: string;
  model: string;
  color: string;
  visitorValidFrom: Dayjs | null;
  visitorValidTo: Dayjs | null;
}

export interface IGetAllVehicleByOwnerRequestProps {
  searchText?: string;
  vehicleOwnerId: string;
}

export interface vehicleProps {
  selectedOwner?: IOwnerList;
}

export interface countryres {
  countryCode: string;
  countryName: string;
  hasState: boolean;
  id: string;
}

export interface deleteProps {
  id: string;
}

export interface LPRFilter {
  floorIds: string[];
  zoneIds: string[];
  deviceIds: string[];
  countryName: string;
  fromDate: Dayjs | null;
  toDate: Dayjs | null;
}

export interface IDeviceList {
  id: string;
  deviceName: string;
  floorId: string;
  zoneId: string;
}


export interface IGetAllLPRData {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string | null;
  sortOrder?: number;
  searchText?: string;
  floorIds: string[];
  zoneIds: string[];
  deviceIds: string[];
  countryName: string;
  fromDate: string | null;
  toDate: string | null;
}

export interface ILPRList {
  id: string;
  vehicleOwnerId?: string;
  country: string;
  countryId: string;
  countryName: string;
  state: string;
  series: string;
  vehicleNumber: number;
  plateCode: string;
  plateCategory: string;
  make: string;
  model: string;
  color: string;
  visitorValidFrom: Dayjs | null;
  visitorValidTo: Dayjs | null;
}

export interface ICountryOption {
  id: string;
  title: string;
  countryName: string;
}

export interface ImagePayload {
  imageName: string;
  size: string;
}