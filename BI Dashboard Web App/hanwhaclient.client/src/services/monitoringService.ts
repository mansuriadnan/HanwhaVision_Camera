import apiUrls from "../constants/apiUrls";
import { IMonitoringNamePayload } from "../interfaces/IChart";
import {
  AddUpdateGroupItemPayload,
  IGroupData,
  IdeleteMonitoring,
} from "../interfaces/IMonitoring";
import { apiGetService } from "../utils/apiGetService";
import {
  apiPostService,
  apiPostServiceWithDataAndMessage,
} from "../utils/apiPostService";

export const GetMonitoringDesign = () =>
  apiGetService({
    url: apiUrls.GetMonitoringDesign,
  });

export const SaveMonitoringName = (payload: IMonitoringNamePayload) =>
  apiPostService({
    url: apiUrls.SaveMonitoringName,
    data: payload,
  });

export const AddMonitoringGroupService = (groupData: IGroupData) =>
  apiPostService<IGroupData>({
    url: apiUrls.AddUpdateGroup,
    data: groupData,
  });

export const GetGroupAndItemDataService = (monitoringId: string) =>
  apiGetService({
    url: `${apiUrls.GetGroupAndItemData}?monitoringId=${encodeURIComponent(
      monitoringId
    )}`,
  });

export const AddUpdateGroupItemService = (
  groupItemData: AddUpdateGroupItemPayload
) =>
  apiPostServiceWithDataAndMessage({
    url: apiUrls.AddUpdateGroupItem,
    data: groupItemData,
  });

export const DeleteGroupService = (data: IdeleteMonitoring) =>
  apiPostService({
    url: apiUrls.DeleteGroup,
    data: data,
  });

export const DeleteItemService = (data: IdeleteMonitoring) =>
  apiPostService({
    url: apiUrls.DeleteGroupItem,
    data: data,
  });

export const DeleteMonitoringService = (data: IdeleteMonitoring) =>
  apiPostService({
    url: apiUrls.DeleteMonitoring,
    data: data,
  });
