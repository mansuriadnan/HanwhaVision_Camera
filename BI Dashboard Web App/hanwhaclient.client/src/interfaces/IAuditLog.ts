import { iOptionModelResponse } from "./IApiResponse";
import { IOptionModelResponse } from "./IOptionModelResponse";

export interface FinalAuditLogData {
  [key: string]: any;
}

export interface Collectiontype {
  id: number;
  title: string;
}

export interface AuditLogres {
  auditLogDetails: FinalAuditLogData[];
  totalCount: number;
}

// export interface referenceDatatype {
//   createdBy: iOptionModelResponse[];
//   updatedBy: iOptionModelResponse[];
// }

export interface referenceDatatype {
  [key: string]: iOptionModelResponse[];
}

export interface IAuditLogsResponse {
  id: number;
  operationType: number;
  collectionName: string;
  documentKey?: number;
  documentBeforeChange?: string;
  operationData?: string;  
  createdBy?:string;
  createdOn?:string;
}
export interface IAuditLogsRequest {
  pageNumber: number;
  pageSize: number;
  sortBy?: string | null;
  sortOrder?: number | null;       
  collectionName: string;
  id?: string | null;             
}
export interface IAuditLogsReferenceDatatype {
  createdBy?: IOptionModelResponse[];
  roleIds?: IOptionModelResponse[];
  deviceIds?: IOptionModelResponse[];
  countryIds?: IOptionModelResponse[];
  ownerIds?: IOptionModelResponse[];
  timezoneId?: IOptionModelResponse[];
  childSite?: IOptionModelResponse[];
  parentSite?: IOptionModelResponse[];
}