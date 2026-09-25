import apiUrls from "../constants/apiUrls";
import { IdeleteProps } from "../interfaces/IChart";
import { HealthReportRequest, IavailabilityPayload, IdeviceAvailabilityPayload, ISSMCPUPayload, ISSMDevicePayload, ISSMPayload, ISSMServers } from "../interfaces/IManageServer";
import { apiGetServiceFullResponse } from "../utils/apiGetService";
import { apiPostService, apiPostServiceForWidgets } from "../utils/apiPostService";


export const AddSSMServerService = (server:ISSMServers) =>
  apiPostService<ISSMServers>({
    url: apiUrls.AddUpdateSSMServer,
    data: server,
  });



export const fetchSSMSiteDataService = (
  payload: ISSMPayload
) =>
  apiPostServiceForWidgets({
    url: apiUrls.SSMDashboardData,
    data: payload,
  });


  export const GetSSMDeviceService = (
  payload: ISSMDevicePayload
) =>
  apiPostServiceForWidgets({
    url: apiUrls.SSMServerDeviceDetail,
    data: payload,
  });


export const GetCPUUtilizationService = (
  payload: ISSMCPUPayload
) =>
  apiPostServiceForWidgets({
    url: apiUrls.SSMserverCPUUtilization,
    data: payload,
  });
  
  export const GetRAMUtilizationService = (
  payload: ISSMCPUPayload
) =>
  apiPostServiceForWidgets({
    url: apiUrls.SSMserverRAMUtilization,
    data: payload,
  });

  export const serverAvailabilityService = (
  payload: IavailabilityPayload
) =>
  apiPostServiceForWidgets({
    url: apiUrls.SSMserverAvailability,
    data: payload,
  });

export const deviceAvailabilityService = (
  payload: IdeviceAvailabilityPayload
) =>
  apiPostServiceForWidgets({
    url: apiUrls.DeviceAvailability,
    data: payload,
  });

export const GetAllSsmService = () =>
  apiGetServiceFullResponse<ISSMServers[]>({
    url: apiUrls.SSMServerData,
  });

export const DeleteSsmService = (data: IdeleteProps) =>
  apiPostService({
    url: apiUrls.DeleteSSMServer,
    data: data,
})

export const exportHealthReportService = (
  payload: HealthReportRequest
) =>
  apiPostServiceForWidgets({
    url: apiUrls.ExportHealthReportData,
    data: payload,
  });