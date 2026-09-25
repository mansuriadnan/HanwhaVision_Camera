import { useState } from "react";
import { login } from "../services/loginService";
import { usePermissions } from "../context/PermissionsContext";
import {
  GetUserPermissions,
  GetUserProfileDetails,
} from "../services/userService";
import { useUser } from "../context/UserContext";
import { IProfileReferenceData, IUsers } from "../interfaces/IUser";
import { ValidateLicenseService } from "../services/licenseService";
import { convertFieldResponseIntoMuiTextFieldProps } from "@mui/x-date-pickers/internals";
import { useThemeContext } from "../context/ThemeContext";
import { GetCurrentLicenseDetailService } from "../services/settingService";

export const useAuth = () => {
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const { setPermissions, setWidgetPermissions } = usePermissions();
  const { setUser, setReferenceData } = useUser();
  
  const handleLogin = async (userName: string, password: string) => {
    setLoading(true);
    setError(null);

    try {
      const response = await login(userName, password);

      if (response.isSuccess && response.data) {
        localStorage.setItem("accessToken", response.data?.accessToken);
        localStorage.setItem("refreshToken", response.data?.refreshToken);

        const validateLicenseResponse = await ValidateLicenseService();
        // setIsLicenseValid(false); // true or false
        const validateLicenseRes: string = String(validateLicenseResponse);
        localStorage.setItem("isLicenseValid", validateLicenseRes);

        const LicenseResponse: any = await GetCurrentLicenseDetailService();
        if (LicenseResponse !== null) {
          localStorage.setItem("isANPR", LicenseResponse.isANPR);
          localStorage.setItem("isMaintenance", LicenseResponse.isMaintenance);
        }

        const PermissionData: any = await GetUserPermissions();
        let permissionsArray: Object[] = [];
        let widgetPermissionsArray: Object[] = [];
        // if (Array.isArray(PermissionData.screensPermission) && PermissionData.screensPermission.length > 0 ||
        //   Array.isArray(PermissionData.widgetsPermission) && PermissionData.widgetsPermission.length > 0) {
        //   permissionsArray = PermissionData.screensPermission;
        //   widgetPermissionsArray = PermissionData.widgetsPermission;
        //   setPermissions(PermissionData.screensPermission);
        //   setWidgetPermissions(PermissionData.widgetsPermission);
        // } else {
        //   setPermissions([]);
        //   setWidgetPermissions([]);
        // }
        // localStorage.setItem("permissions", JSON.stringify(permissionsArray));
        // localStorage.setItem("WidgetPermissions", JSON.stringify(widgetPermissionsArray));

        if (
          Array.isArray(PermissionData.screensPermission) &&
          PermissionData.screensPermission.length > 0
        ) {
          permissionsArray = PermissionData.screensPermission;
          setPermissions(permissionsArray);
          localStorage.setItem("permissions", JSON.stringify(permissionsArray));

          if (
            Array.isArray(PermissionData.widgetsPermission) &&
            PermissionData.widgetsPermission.length > 0
          ) {
            widgetPermissionsArray = PermissionData.widgetsPermission;
            setWidgetPermissions(widgetPermissionsArray);
            localStorage.setItem(
              "widgetPermissions",
              JSON.stringify(widgetPermissionsArray)
            );
          } else {
            setWidgetPermissions([]);
            localStorage.setItem("widgetPermissions", JSON.stringify([]));
          }
        } else {
          setPermissions([]);
          setWidgetPermissions([]);
          localStorage.setItem("permissions", JSON.stringify([]));
          localStorage.setItem("WidgetPermissions", JSON.stringify([]));
        }

        const userProfileDetails: any = await GetUserProfileDetails();
        if (userProfileDetails) {
          const userData = userProfileDetails.data as IUsers;
          const referenceData =
            userProfileDetails.referenceData as IProfileReferenceData;

          setUser(userData);
          setReferenceData(referenceData);

          localStorage.setItem("userProfile", JSON.stringify(userData));
          localStorage.setItem(
            "userProfileReferenceData",
            JSON.stringify(referenceData)
          );

          // const theme = userData?.userPreferences?.theme || "light"; // fallback to 'light' if undefined
          // localStorage.setItem("theme", theme);
          // setTheme(theme);

          // const themeColor = userData?.userPreferences?.themeColor || "default-theme"; // fallback to 'default' if undefined
          // localStorage.setItem("themeColor", themeColor);
          // setThemeColor(themeColor);

          // const language = userData?.userPreferences?.language || "en"
          //  localStorage.setItem("i18nextLng", language);
        }

        return { success: true, permissions: permissionsArray, user: userProfileDetails.data }; // Return permissions here
      } else {
        const errorMessage =
          response?.message || "An unknown error occurred. Please try again.";
        setError(errorMessage);
        return { success: false, errorMessage, permissions: [], user: {} };
      }
    } catch (error: any) {
      setLoading(false);
      const errorMessage =
        error?.message || "An unknown error occurred. Please try again.";
      setError(errorMessage);
      return { success: false, errorMessage, permissions: [] };
    }
  };

  const handleLogout = () => {
    const savedLang = localStorage.getItem("i18nextLng") || "en";
    const savedThemeColor = localStorage.getItem("themeColor") || "default-theme";
    localStorage.clear();
    sessionStorage.clear();

    // Restore language
    localStorage.setItem("i18nextLng", savedLang);
    localStorage.setItem("themeColor", savedThemeColor);
    document.body.classList.remove("light", "dark");
    document.body.classList.add("light");
  };

  return { handleLogin, handleLogout, loading, error };
  
};
