import apiUrls from "../constants/apiUrls";
import {
  apiPostService,
  apiPostServiceWithoutToast,
  downloadFile,
} from "../utils/apiPostService";
import {
  IGetAllPlanRequestProps,
  IPlanProps,
  deleteProps,
  addScheduleReq,
  scheduleFilter,
  deviceImgReq,
  updateStatusForm,
  RMAFormProps,
  IGetAllRMARequestProps,
  getImgReqProps,
} from "../interfaces/IMaintenance";
import { apiGetService } from "../utils/apiGetService";

export const SaveMaintenancePlanService = (Data: IPlanProps) =>
  apiPostService<IPlanProps>({
    url: apiUrls.SaveMaintenancePlan,
    data: Data,
  });

export const GetMaintenancePlanService = (request: IGetAllPlanRequestProps) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetMaintenancePlan,
    data: request,
  });

export const DeletePlanService = (params: deleteProps) =>
  apiPostService<deleteProps>({
    url: apiUrls.DeleteMaintenancePlan,
    data: params,
  });

export const SaveMaintenanceScheduleService = (Data: addScheduleReq) =>
  apiPostService<addScheduleReq>({
    url: apiUrls.AddUpdateMaintenanceSchedule,
    data: Data,
  });

export const GetMaintenanceScheduleService = (request: scheduleFilter) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetMaintenanceSchedule,
    data: request,
  });

export const GetDeviceLiveImageService = (request: deviceImgReq) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetLiveImage,
    data: request,
  });

export const UpdateMaintenanceScheduleStatusService = (
  Data: updateStatusForm
) =>
  apiPostService<updateStatusForm>({
    url: apiUrls.UpdateMaintenanceScheduleStatus,
    data: Data,
  });

export const SaveRMAService = (Data: RMAFormProps) =>
  apiPostService<RMAFormProps>({
    url: apiUrls.SaveRMA,
    data: Data,
  });

export const GetRMAService = (request: IGetAllRMARequestProps) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetRMA,
    data: request,
  });

export const DeleteRMAService = (params: deleteProps) =>
  apiPostService<deleteProps>({
    url: apiUrls.DeleteRMA,
    data: params,
  });

export const GetBeforeAfterImageService = (request: getImgReqProps) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetBeforeAfterImage,
    data: request,
  });

export const ExportMaintenanceScheduleCSVService = (data: any) =>
  downloadFile({
    url: apiUrls.ExportMaintenanceScheduleCSV,
    data: data.data,
    method: "POST",
    responseType: "text/csv",
  });

export const GetDevicesAsyncService = (suburl: string) =>
  apiGetService({
    url: apiUrls.GetDevicesAsync + suburl,
  });

export const uploadMaintananceFileComapreService = (formData: FormData) =>
  apiPostServiceWithoutToast<FormData>({
    url: apiUrls.comapareImage,
    data: formData,
    isFormData: true,
  });

export const ExportRMACSVService = (data: any) =>
  downloadFile({
    url: apiUrls.ExportRMACSV,
    data: data.data,
    method: "POST",
    responseType: "text/csv",
  });

export const GetAllDeviceForMaintenancePlanService = () =>
  apiGetService({
    url: apiUrls.GetAllDeviceForMaintenancePlan,
  });