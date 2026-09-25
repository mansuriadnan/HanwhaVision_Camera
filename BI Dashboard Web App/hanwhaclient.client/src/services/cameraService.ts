import apiUrls from "../constants/apiUrls";
import {
  apiPostService,
  apiPostServiceWithoutToast,
  downloadFile,
} from "../utils/apiPostService";
import {
  ICamera,
  IDeviceCredentials,
  IEventlogsRequest,
  IGetAllDeviceRequest,
  IUpdateDeviceMaintenanceStstus
} from "../interfaces/ICamera";
import { IDeviceID, Ipayload } from "../interfaces/Inotifications";
import { apiGetService } from "../utils/apiGetService";

export const GetAllCameraListService = (request: IGetAllDeviceRequest) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetAllDeviceList,
    data: request,
  });

export const UpdateCameraService = (
  camera: Omit<ICamera, "createdDateTime" | "lastUpdatedDateTime">
) =>
  apiPostService<Omit<ICamera, "createdDateTime" | "lastUpdatedDateTime">>({
    url: apiUrls.ManageAddDevice,
    data: camera,
  });

export const AddCameraService = (
  camera: Omit<ICamera, "createdDateTime" | "lastUpdatedDateTime">
) =>
  apiPostService<Omit<ICamera, "createdDateTime" | "lastUpdatedDateTime">>({
    url: apiUrls.ManageAddDevice,
    data: camera,
  });

export const DeleteCameraService = (ids: string[]) =>
  apiPostService({
    url: apiUrls.DeleteDevices,
    data: ids,
  });

export const getChannelDataService = (data: IDeviceCredentials) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetAllChannels,
    data: data,
  });
export const GetAllEventLogsListService = (data: IEventlogsRequest) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetAllEventLogsList,
    data: data,
  });

export const UpdateEventLogsStatusService = (data: Ipayload) =>
  apiPostService({
    url: apiUrls.UpdateEventLogsStatus,
    data: data,
  });

export const UploadBulkDevice = (formData: FormData) =>
  apiPostService<FormData>({
    url: apiUrls.UploadBulkDevice, // This should point to your single combined API
    data: formData,
    isFormData: true,
  });

export const SampleExcelDownloadService = () =>
  downloadFile({
    url: apiUrls.DownloadSampleFile,
    data: "",
    method: "GET",
    responseType: "blob",
  });

export const ExportDeviceCSVService = (data: any) =>
  downloadFile({
    url: apiUrls.ExportDeviceCSV,
    data: data.data,
    method: "POST",
    responseType: "text/csv",
  });

export const UpdateDeviceMaintenanceStatusService = (
  device: IUpdateDeviceMaintenanceStstus
) =>
  apiPostServiceWithoutToast<IUpdateDeviceMaintenanceStstus>({
    url: apiUrls.UpdateDeviceMaintenanceStatus,
    data: device,
  });

export const manuallyOpenGateBarrierService = (device:IDeviceID) =>
  apiPostService({
    url: apiUrls.ManuallyOpenGateBarrier,
    data: device
  });