// import { IPolygonSubmissionPayload } from "../components/Floor/AddFloorPlan";
import {
  IFloorPlan,
  IBindSite,
  GetFloorPlanResponseDto,
  IZoneList,
  DeviceList,
  IZoneDetails,
  IFloorZoneNamesRequest,
  IPolygonSubmissionPayload,
  IdeleteProps,
} from "../interfaces/IFloorAndZone";
import apiUrls from "../constants/apiUrls";
import {
  apiGetService,
  apiGetServiceForWidget,
  apiGetServiceFullResponse,
} from "../utils/apiGetService";
import {
  apiPostService,
  apiPostServiceWithoutToast,
} from "../utils/apiPostService";
import { ApiResponse } from "../interfaces/IApiResponse";

export const GetAllSiteService = () =>
  apiGetService<IBindSite[]>({
    url: apiUrls.GetAllSite,
  });

// export const AddFloorPanService = () =>
//   apiPost<PolygonSubmissionPayload, PolygonSubmissionPayload>(
//     apiUrls.AddCamera,
//     payload
//   );

export const AddFloorPanService = (
  addFloorPlanRequestDto: Omit<IPolygonSubmissionPayload, "id">
) =>
  apiPostService<IPolygonSubmissionPayload>({
    url: apiUrls.AddFloorPlan,
    data: addFloorPlanRequestDto,
  });

export const GetAllFloorPanService = () =>
  apiGetService<IFloorPlan[]>({
    url: apiUrls.GetAllFloorPlan,
  });

export const GetAllZoneByFloorPanIdService = (id: string) =>
  apiGetService<GetFloorPlanResponseDto>({
    url: apiUrls.GetAllZoneByFloorId,
    id: id,
  });

export const DeleteZoneService = (data: IdeleteProps) =>
  apiPostService({
    url: apiUrls.DeleteZoneById,
    data: data,
  });

// Added By Hasti

export const GetAllFloorService = () =>
  apiGetService<IFloorPlan[]>({
    url: apiUrls.GetFloorList,
  });

export const DeleteFloorService = (data: IdeleteProps) =>
  apiPostService({
    url: apiUrls.DeleteFloor,
    data: data,
  });

export const AddFloorService = (FloorData: IFloorPlan) =>
  apiPostService<IFloorPlan>({
    url: apiUrls.AddFloor,
    data: FloorData,
  });

export const GetAllZoneByFloorIdService = (id: string,IncludeMultiServer: boolean = false) =>
  apiGetService<IZoneList[]>({
    url: `${apiUrls.GetAllZoneByFloorId}/${id}?IncludeMultiServer=${IncludeMultiServer}`,
  });

export const AddZoneService = (ZoneData: IZoneList) =>
  apiPostService<IZoneList>({
    url: apiUrls.AddUpdateZone,
    data: ZoneData,
  });

export const GetAllDevicewithoutZoneService = () =>
  apiGetService<DeviceList[]>({
    url: apiUrls.GetAllDevicewithoutZone,
  });

export const uploadFloorPlanImageService = (formData: FormData, id: string) =>
  apiPostService<FormData>({
    url: apiUrls.AddFloorPlanImage + `/${id}`,
    data: formData,
    isFormData: true,
  });

export const GetFloorPlanImageService = (id: string) =>
  apiGetService<string | "">({
    url: apiUrls.GetFloorPlanImage,
    id: id,
  });

export const GetFloorPlanImageServicelocalloader = (id: string) =>
  apiGetServiceForWidget<string | "">({
    url: apiUrls.GetFloorPlanImage,
    id: id,
  });

export const AddZonePlanDetailService = (ZoneDetails: IZoneDetails) =>
  apiPostService<IZoneDetails>({
    url: apiUrls.AddZonePlanDetail,
    data: ZoneDetails,
  });

export const DeleteMappedCameraService = (data: IdeleteProps) =>
  apiPostService({
    url: apiUrls.DeleteMappedCamera,
    data: data,
  });

export const FloorZonesNameByIds = (payload: IFloorZoneNamesRequest) =>
  apiPostServiceWithoutToast<IFloorZoneNamesRequest>({
    url: apiUrls.FetchFloorZoneNames,
    data: payload,
  });
