import apiUrls from "../constants/apiUrls";
import {
  apiGetService,
  apiGetServiceFullResponse,
} from "../utils/apiGetService";
import { AuditLogres, IAuditLogsRequest } from "../interfaces/IAuditLog";
import { ApiResponse } from "../interfaces/IApiResponse";
import { apiPostServiceWithoutToast, downloadFile } from "../utils/apiPostService";

export const GetAuditLog = (suburl: string) =>
  apiGetServiceFullResponse<ApiResponse<AuditLogres>>({
    url: apiUrls.GetAuditLog + suburl,
  });

export const GetCollectionList = () =>
  apiGetService<string[]>({
    url: apiUrls.GetCollectionList,
  });
export const GetAuditLogs = (data: IAuditLogsRequest) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetAllAuditLogs,
    data: data,
  });

export const ExportAuditLogsCSVService = (data: any) =>
  downloadFile({
    url: apiUrls.ExportAuditLogsCSV,
    data: data.data,
    method: "POST",
    responseType: "text/csv",
  });