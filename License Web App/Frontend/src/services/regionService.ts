import apiUrls from "../constants/apiUrls";
import { ApiResponse } from "../interfaces/IApiResponse";
import { IRegion, IRegionDD } from "../interfaces/ICreateRegion";
import { apiDeleteService } from "../utils/apiDeleteService";
import { apiGetServiceFullResponse } from "../utils/apiGetService";
import { apiPostService } from "../utils/apiPostService";

export const AddUpdateRegionService = (
  region: Omit<IRegion, "createdDateTime" | "lastUpdatedDateTime">
) =>
  apiPostService<Omit<IRegion, "createdDateTime" | "lastUpdatedDateTime">>({
    url: apiUrls.CreateRegion,
    data: region,
  });

export const GetAllRegionService = () =>
  apiGetServiceFullResponse({
    url: apiUrls.GetAllRegion,
  });

export const DeleteRegionService = (id: string) =>
  apiDeleteService({
    url:apiUrls.DeleteRegion,
    id:id
  });

export const GetAllRegionByPermissionService = () =>
  apiGetServiceFullResponse<ApiResponse<IRegionDD[]>>({
    url: apiUrls.GetRegionByPermission,
  });