import apiUrls from "../constants/apiUrls";
import {
  apiPostService,
  apiPostServiceWithoutToast,
  downloadFile,
} from "../utils/apiPostService";
import {
  IGetAllOwnerRequestProps,
  OwnerInputProps,
  IGetAllVehicleByOwnerRequestProps,
  IVehicleList,
  deleteProps,
  IGetAllLPRData,
  ImagePayload,
} from "../interfaces/IANPR";
import { apiGetService } from "../utils/apiGetService";

export const SaveVehicleOwnerService = (DesignData: OwnerInputProps) =>
  apiPostService<OwnerInputProps>({
    url: apiUrls.AddVehicleOwner,
    data: DesignData,
  });

export const GetVehicleOwnerListService = () =>
  apiGetService({
    url: apiUrls.GetVehicleOwnerList,
  });

export const GetAllVehicleOwnerService = (request: IGetAllOwnerRequestProps) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetAllVehicleOwner,
    data: request,
  });

export const GetAllCountryListService = () =>
  apiGetService({
    url: apiUrls.GetAllCountryList,
  });

export const GetAllVehicleByOwnerService = (
  request: IGetAllVehicleByOwnerRequestProps,
) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetAllVehicleByOwner,
    data: request,
  });

export const SaveVehicleService = (DesignData: IVehicleList) =>
  apiPostService<IVehicleList>({
    url: apiUrls.AddVehicle,
    data: DesignData,
  });

export const DeleteOwnerService = (params: deleteProps) =>
  apiPostService<deleteProps>({
    url: apiUrls.DeleteOwner,
    data: params,
  });

export const DeleteANPRVehicleService = (params: deleteProps) =>
  apiPostService<deleteProps>({
    url: apiUrls.DeleteANPRVehicle,
    data: params,
  });

export const OwnerSampleExcelDownloadService = () =>
  downloadFile({
    url: apiUrls.DownloadOwnerSampleFile,
    data: "",
    method: "GET",
    responseType: "blob",
  });

export const UploadBulkOwner = (formData: FormData) =>
  apiPostService<FormData>({
    url: apiUrls.UploadBulkOwner, // This should point to your single combined API
    data: formData,
    isFormData: true,
  });

export const VehicleSampleExcelDownloadService = () =>
  downloadFile({
    url: apiUrls.DownloadVehicleSampleFile,
    data: "",
    method: "GET",
    responseType: "blob",
  });

export const UploadBulkVehicle = (formData: FormData) =>
  apiPostService<FormData>({
    url: apiUrls.UploadBulkVehicle, // This should point to your single combined API
    data: formData,
    isFormData: true,
  });

export const GetAllLprDetailsService = (request: IGetAllLPRData) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetAllLprDetails,
    data: request,
  });

export const ExportLPRCSVService = (data: any) =>
  downloadFile({
    url: apiUrls.ExportLPRDetailsCSV,
    data: data.data,
    method: "POST",
    responseType: "text/csv",
  });

export const GetLPRImageService = async (data: ImagePayload) => {
  const token = localStorage.getItem("accessToken");

  const response = await fetch(apiUrls.ANPRImage, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: token ? `Bearer ${token}` : "",
    },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    throw new Error("Failed to load image");
  }

  return await response.blob();
};
