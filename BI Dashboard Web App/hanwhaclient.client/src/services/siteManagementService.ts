// src/services/roleService.ts
import apiUrls from "../constants/apiUrls";
import { ChildSite, ISite } from "../interfaces/IMultiSite";
import { ISiteList, ISitedeleteProps } from "../interfaces/ISiteList";
import { apiDeleteService } from "../utils/apiDeleteService";
import {
  apiGetService,
  apiGetServiceFullResponse,
} from "../utils/apiGetService";
import { apiPostService } from "../utils/apiPostService";

export const GetAllSiteService = () =>
  apiGetServiceFullResponse<ISite[]>({
    url: apiUrls.GetAllSite,
  });

export const CreateSiteService = (sites: Omit<ISiteList, "id">) =>
  apiPostService<ISiteList>({
    url: apiUrls.CreateSite,
    data: sites,
  });

export const UpdateSiteService = (sites: ISiteList) =>
  apiPostService<ISiteList>({
    url: apiUrls.UpdateSite,
    data: sites,
  });

export const DeleteSiteService = (id: string) =>
  apiDeleteService({
    url: apiUrls.SiteDelete,
    id: id,
  });

export const AddChildSiteService = (site: ISite) =>
  apiPostService<ISite>({
    url: apiUrls.AddChildSite,
    data: site,
  });

export const UpdateChildSiteService = (site: ISite) =>
  apiPostService<ISite>({
    url: apiUrls.AddChildSite,
    data: site,
  });

export const AddSubChildSiteService = (site: ChildSite) =>
  apiPostService<ChildSite>({
    url: apiUrls.AddSubChildSite,
    data: site,
  });

export const UpdateSubChildSiteService = (site: ChildSite) =>
  apiPostService<ChildSite>({
    url: apiUrls.AddSubChildSite,
    data: site,
  });

export const DeleteChildSiteService = (data: ISitedeleteProps) =>
  apiPostService({
    url: apiUrls.DeleteChildSite,
    data: data,
  });

export const DeleteSubChildSiteService = (data: ISitedeleteProps) =>
  apiPostService({
    url: apiUrls.DeleteSubChildSite,
    data: data,
  });

export const GetAllSiteForReportService = () =>
  apiGetServiceFullResponse<ISite[]>({
    url: apiUrls.GetAllSite,
  });
