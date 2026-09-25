export interface IEmailTemplate {
  emailTemplateName: string;
  emailTemplateTitle: string;
  emailTemplateDescription?: string;
  emailTemplateHtml: string;
  createdOn: string | null;
  createdBy: string | null;
  updatedOn: string | null;
  updatedBy: string | null;
  isDeleted?: boolean;
  id: string;
}

export interface ISendEmailDto {
  id: string;
}

export interface IOperationalTimeZone {
  startTime: string;
  endTime: string;
  timeZone: string;
}

export interface ITimeZoneDropDownDto {
  id: string;
  name: string;
  timeZoneName: string;
  timeZoneAbbr: string;
  utcOffset: string;
}

export interface ISmtpSettings {
  host: string;
  port: number;
  enableSsl: boolean;
  username: string;
  password: string;
  fromEmail: string;
}

export interface IGeneralSettings {
  logo: string;
  smtpSettings: ISmtpSettings;
  timeZone: string;
  operationalTiming: {
    startTime: string;
    endTime: string;
  };
  googleApiKey: string;
  ftpConfiguration: {
    ftpHost: string;
    ftpUser: string;
    ftpPassword: string;
  };
}

export interface IFTPSettings {
  host: string;
  port: number;
  username: string;
  password: string;
}

export interface IRetentionSettings {
  Enable: boolean;
  RetentionPeriod: number;
}

export interface IReportSchedulerSettings {
  emails: string[];
  widgets: string[];
  startDate: string;
  startTime: string;
  reportFormat: string;
  sendInterval: string;
}
export interface IOnOffReportScheduler {
  isReportSchedule: boolean;
}

export interface IBackupDbSettings {
  //Enable: boolean;
  Path: string;
}

export interface IANPRSettings {
  imagePath: string;
  retentionPeriod:number | null | string;
}

export interface IServerManagement {
  id?: string;
  serverName: string;
  databaseConnectionString: string;
  hostingAddress: string;
  username: string;
  password: string;
  isActive?: boolean;
  isAvailable?: boolean;
}

export interface ServerAddEditFormProps {
  onClose: () => void;
  serverData?: IServerManagement;
  refreshData: () => void;
}

export interface deleteProps {
  id: string;
}
export interface IUpdateServerStstus {
  id: string,
  isActive: boolean
}

export interface IServer {
  id: string;
  hostingAddress: string;
  serverName: string;
  username: string;
  password: string;
  createdBy: string;
  createdOn: string;
  databaseConnectionString: string;
  deletedOn: string;
  isActive: boolean;
  isDeleted: boolean;
  updatedBy: string;
  updatedOn: string;
  isAvailable?: boolean;
}

export interface IResetVehicleSettings {
  count:number | null | string;
}

export interface IResetVehiclePayload {
  CurrentCount :number | null | string;
}

export interface IFetchDBString {
  hostingAddress: string;
  username: string;
  password: string;
}