import apiUrls from "../constants/apiUrls";
import { IEmailData, IExceptionlog } from "../interfaces/IExceptionlogs";
import { apiPostService, apiPostServiceWithoutToast } from "../utils/apiPostService";

export const GetAllExceptionLogsListService = (data: IExceptionlog) =>
  apiPostServiceWithoutToast({
    url: apiUrls.GetAllExceptionLogsList,
    data: data
  });

export const sendEmailService = (emailData: IEmailData) =>
  apiPostService<IEmailData>({
    url: apiUrls.ExceptionEmail,
    data: emailData,
  });