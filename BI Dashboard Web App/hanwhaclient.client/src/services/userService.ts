import apiUrls from "../constants/apiUrls";
import {
  User,
  IChangePassword,
  IForgotPassword,
  IForgotPasswordReset,
  IUserResetPassword,
  IOtpVerification,
  IPasswordReset,
  IdeleteProps
} from "../interfaces/IUser";
import { apiPostService, apiPostServiceWithoutToast } from "../utils/apiPostService";
import {  
  apiGetServiceFullResponse,
} from "../utils/apiGetService";
import { IUsers } from "../interfaces/IManageUsers";

export const createUser = (user: User) =>
  apiPostService<User>({
    url: apiUrls.CreateUser,
    data: user,
  });

export const ChangePasswordService = (user: IChangePassword) =>
  apiPostService<IChangePassword>({
    url: apiUrls.ChangePassword,
    data: user,
  });

export const ForgotPasswordService = (user: IForgotPassword) =>
  apiPostService<IForgotPassword>({
    url: apiUrls.ForgotPassword,
    data: user,
  });

export const OtpVerificationService = (otpVerification: IOtpVerification) =>
  apiPostService<IOtpVerification>({
    url: apiUrls.ValidateOtp,
    data: otpVerification,
  });

export const ForgotPasswordResetService = (user: IForgotPasswordReset) =>
  apiPostService<IForgotPasswordReset>({
    url: apiUrls.ForgotResetPassword,
    data: user,
  });

export const GetAllUsersService = () =>
  apiGetServiceFullResponse<IUsers[]>({
    url: apiUrls.GetAllUser,
  });

export const UpdateUserService = (
  users: Omit<IUsers, "createdDateTime" | "lastUpdatedDateTime">
) =>
  apiPostService<Omit<IUsers, "createdDateTime" | "lastUpdatedDateTime">>({
    url: apiUrls.UpdateUser,
    data: users,
  });

export const AddUserService = (
  users: Omit<IUsers, "createdDateTime" | "lastUpdatedDateTime">
) =>
  apiPostService<Omit<IUsers, "createdDateTime" | "lastUpdatedDateTime">>({
    url: apiUrls.UpdateUser,
    data: users,
  });

export const DeleteUserService = (data: IdeleteProps) =>
  apiPostService({
    url: apiUrls.UserDelete,
    data: data,
  });

export const GetUserPermissions = () =>
  apiGetServiceFullResponse({
    url: apiUrls.getUserPermission,
  });

export const UserResetPassword = (payload: IUserResetPassword) =>
  apiPostService<IUserResetPassword>({
    url: apiUrls.UserResetPassword,
    data: payload,
  });

export const GetUserProfileDetails = () =>
  apiGetServiceFullResponse({
    url: apiUrls.GetUserProfile,
  });

export const uploadUserProfileImageService = (formData: FormData) =>
  apiPostService<FormData>({
    url: apiUrls.UploadProfileImage,
    data: formData,
    isFormData: true,
  });
export const SaveUserPreferences = (payload: any) =>
  apiPostService({
    url: apiUrls.SaveUserPreferences,
    data: payload,
  });
export const ResetPasswordService = (passwordreset: IPasswordReset) =>
  apiPostServiceWithoutToast({
    url: apiUrls.ResetPassword,
    data: passwordreset,
  });