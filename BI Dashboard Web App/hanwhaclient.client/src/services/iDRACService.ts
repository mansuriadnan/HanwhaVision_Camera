import apiUrls from "../constants/apiUrls";
import { IdeleteProps } from "../interfaces/IChart";
import { IalarmPayload, IdeleteAlarmPayload, IiDRACDetail, IIdracServerEventPayload, IIdracServerSystemPayload, IIdracSystemInfoPayload, ILEDPayload, IPowerPayload, IserverPayload } from "../interfaces/IManageiDRAC";
import { apiGetServiceFullResponse } from "../utils/apiGetService";
import { apiPostService, apiPostServiceForWidgets } from "../utils/apiPostService";


export const AddiDRACService = (server:IiDRACDetail) =>
  apiPostService<IiDRACDetail>({
    url: apiUrls.AddUpdateiDRAC,
    data: server,
  });
  

export const GetAlliDRACService = () =>
  apiGetServiceFullResponse<IiDRACDetail[]>({
    url: apiUrls.GetAlliDRACServer,
  });

export const DeleteiDRACService = (data: IdeleteProps) =>
  apiPostService({
    url: apiUrls.iDRACDelete,
    data: data,
  });

  export const AddAlarmService = (data: IalarmPayload) =>
  apiPostService({
    url: apiUrls.addAlarm,
    data: data,
  });


  export const fetchiDRACServerListService = (data: IserverPayload) =>
  apiGetServiceFullResponse({
    url:`${apiUrls.iDRACServerList}?parentSiteId=${data.parentSiteId}`,
  });
    
  export const DeleteAlarmService = (data: IdeleteAlarmPayload) =>
  apiPostService({
    url: apiUrls.AlarmDelete,
    data: data,
  });

export const fetchIdracServerEventData = (data: IIdracServerEventPayload) =>
  apiPostServiceForWidgets({
    url: apiUrls.fetchEventLogDetail,
    data: data,
  });

export const fetchIdracSytemData = (data: IIdracServerSystemPayload) =>
  apiPostServiceForWidgets({
    url: apiUrls.fetchSystemLogDetail,
    data: data,
  });
  
export const toggleServerLEDService = (data: ILEDPayload) =>
  apiPostService({
    url: apiUrls.iDRACLedIndicator,
    data: data,
  });

export const powerActionservice = (data: IPowerPayload) =>
  apiPostService({
    url: apiUrls.iDRACPowerAction,
    data: data,
  });
  
  export const fetchSystemInfo = (data: IIdracSystemInfoPayload) =>
  apiPostService({
    url: apiUrls.iDRACSystemInfo,
    data: data,
  });