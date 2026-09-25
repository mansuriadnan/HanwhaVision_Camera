import { useEffect, useState } from "react";
import {
  Container,
  CssBaseline,
  Box,
  Typography,
  Grid2,
  Grid,
} from "@mui/material";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { ValidateLicenseService } from "../../services/licenseService";
import { useLicense } from "../../context/LicenseContext";
import { SubmitHandler, useForm } from "react-hook-form";
import { useThemeContext } from "../../context/ThemeContext";
import { GetAppMainLogo, GetClientSettingsData } from "../../services/settingService";
import { useSettingsContext } from "../../context/SettingContext";
import { LOCAL_LOADER_THEME_COLORS, REGEX } from "../../utils/constants";
import { CommonDialog } from "../../components/Reusable/CommonDialog";
import { CustomButton } from "../../components/Reusable/CustomButton";
import { CustomTextField } from "../../components/Reusable/CustomTextField";
import { showToast } from "../../components/Reusable/Toast";
import { useTranslation } from "react-i18next";
import ResetPassword from "../../components/Users/ResetPassword";

interface LoginFormInputs {
  userName: string;
  password: string;
}

const LoginPage = () => {
  const navigate = useNavigate();
  const { handleLogin, error, handleLogout } = useAuth();
  //const { setIsLicenseValid } = useLicense();
  const { saveSettings } = useSettingsContext();
  const [isForgotPasswordDialogOpen, setForgotPasswordDialogOpen] =
    useState<boolean>(false);
  const [isPermissionModelOpen, setIsPermissionModelOpen] =
    useState<boolean>(false);
  const { control, handleSubmit } = useForm<LoginFormInputs>({
    defaultValues: {
      userName: "",
      password: "",
    },
  });
  const location = useLocation();
  const { setTheme, setThemeColor } = useThemeContext();
  const { t } = useTranslation();
   const [isViadminResetPasswordOpen, setViadminIsResetPasswordOpen] =
    useState<boolean>(false);
  const [clientLogo, setClientLogo] = useState<string>("");

useEffect(() => {  
  //fetchAppMainLogo();
  if (location.pathname === "/login" || location.pathname === "/") {
    
    const savedLang = localStorage.getItem("i18nextLng") || "en";
    const savedThemeColor = localStorage.getItem("themeColor") || "default-theme";
    localStorage.clear();   // remove only theme

    // Restore language
    localStorage.setItem("i18nextLng", savedLang);
    localStorage.setItem("themeColor", savedThemeColor);
    document.body.classList.remove("dark"); // force remove if needed
    document.body.classList.add("light");
    setTheme("light")

  }
}, []);


  const login: SubmitHandler<LoginFormInputs> = async (data) => {
    try {
      const { success, errorMessage, permissions, user } = await handleLogin(
        data.userName,
        data.password
      );
      if (success) {
        if (user?.isPasswordReset == false) {
          //navigate("/reset-password");
          setViadminIsResetPasswordOpen(true)
          return
        }
   
        fetchClientSettings();
        const validateLicenseResponse = await ValidateLicenseService();
        if (Array.isArray(permissions) && permissions.length > 0) {
          // setIsLicenseValid(validateLicenseResponse);
          //  navigate("/general-settings")
          navigate("/welcome");
          const theme = user?.userPreferences?.theme || "light"; // fallback to 'light' if undefined
          localStorage.setItem("theme", theme);
          setTheme(theme);

          const themeColor = user?.userPreferences?.themeColor || "default-theme"; // fallback to 'default' if undefined
          localStorage.setItem("themeColor", themeColor);
          const loaderColor = LOCAL_LOADER_THEME_COLORS[themeColor];
          if (loaderColor) {
            localStorage.setItem("loaderColor", loaderColor);
          }
          setThemeColor(themeColor);

          const language = user?.userPreferences?.language || "en"
          localStorage.setItem("i18nextLng", language);

        } else {
          setIsPermissionModelOpen(true);
        }
      } else {
        showToast(errorMessage || "An unknown error occurred.", "error");
      }
    } catch (error) {}
  };

  const fetchAppMainLogo = async () => {      
    try {
      const response: any = await GetAppMainLogo();
      if (response != undefined) {
        setClientLogo(response);
      }
    } catch (err) {
      console.error("Error fetching client settings", err);
    }
  };

  const fetchClientSettings = async () => {
  
    try {
      const response: any = await GetClientSettingsData();
      if (response != undefined) {
        saveSettings(response);
      }
    } catch (err) {
      console.error("Error fetching client settings", err);
    }
  };
  

  const handleClosePermissionModal = () => {
    handleLogout();
    setIsPermissionModelOpen(false);
  };

  const forgotPasswordContent = (
    <>
      <h1>Forget password is restricted</h1>
      <h4>Kindly coordinate with your administrator to reset the password.</h4>
    </>
  );
  const handleCloseViadminResetPasswordDialog = () => {
    setViadminIsResetPasswordOpen(false);
  };

  return (
    <Box className="login-main">
      <Box className="container">
        <Grid className="login-wrapper">
          {/* left side */}
          <Grid className="login-left">
            <img src={"images/vision_insight_logo.svg"} alt="Logo" />
            <Typography variant="h4" fontWeight={600} sx={{ marginTop: 10 }}>
              {t("Login.Login_Line1")}
            </Typography>
            <Typography variant="body1">{t("Login.Login_Line2")}</Typography>
            <Typography variant="body1">{t("Login.Login_Line3")}</Typography>
          </Grid>

          {/* Right Side - Login Form */}

          <Grid className="login-right">
            <CssBaseline />
            <Box className="login-form">
              {/* <Avatar sx={{ m: 1, bgcolor: "primary.light" }}>
                  <LockOutlined />
                </Avatar> */}
              <Typography variant="h5"> {t("Login.Login_Header")}</Typography>
              <Typography
                variant="body2"
                sx={{ marginTop: 1, marginBottom: "20px" }}
              >
                {t("Login.Login_Form")}
              </Typography>
              <form onSubmit={handleSubmit(login)}>
                <CustomTextField
                  name="userName"
                  label={t("Login.Form_UserName")}
                  type="text"
                  control={control}
                  rules={{
                    required: t("Login.Login_Email_Require"),
                    validate: (value: any) => {
                      if (
                        REGEX.UserName_Regex.test(value) ||
                        REGEX.Email_Regex.test(value)
                      ) {
                        return true;
                      }
                      return t("Login.Login_Email_validation");
                    },
                  }}
                  fullWidth
                  autoFocus
                  // required
                  customStyles={{
                    marginBottom: "16px", // Add spacing
                  }}
                  placeholder={t("Login.Login_Email_Placeholder")}
                />

                <CustomTextField
                  name="password"
                  label={t("Login.Login_Password")}
                  control={control}
                  rules={{
                    required: t("Login.Login_Pass_Require"),
                  }}
                  type="password"
                  fullWidth
                  // required
                  customStyles={{
                    marginBottom: "16px",
                  }}
                  placeholder={t("Login.Login_Pass_Placeholder")}
                />

                <CustomButton
                  type="submit"
                  fullWidth
                  customStyles={{ mt: 3, mb: 2 }}
                  className="common-btn-design"
                >
                  {t("Login.Login_Header")}
                </CustomButton>
                <Grid2 container justifyContent={"center"}>
                  <Box>
                    <Link
                      to="#"
                      onClick={() => setForgotPasswordDialogOpen(true)}
                      style={{ cursor: "pointer", textDecoration: "none" }}
                    >
                      {t("Login.Forgot_Password")}
                    </Link>
                  </Box>
                </Grid2>
              </form>
            </Box>
          </Grid>

          <CommonDialog
            open={isForgotPasswordDialogOpen}
            title={""}
            content={
              <div>
                <h2> {t("Login.Forgot_Password_lable1")}</h2>
                <p>{t("Login.Forgot_Password_lable2")}</p>
              </div>
            }
            confirmText={t("Login.Okay")}
            onCancel={() => setForgotPasswordDialogOpen(false)}
            onConfirm={() => setForgotPasswordDialogOpen(false)}
            type="contactAdministrator"
            customClass="forgot-pass"
            titleClass={true}
          />
        </Grid>
      </Box>
      <CommonDialog
        open={isPermissionModelOpen}
        title=""
        content="You have not been assigned any permissions to access the application. Please contact your administrator for more information."
        onConfirm={handleClosePermissionModal}
        confirmText="Okay"
        customClass="common-dialog-with-icon"
        titleClass={true}
      />
    
      <CommonDialog
            open={isViadminResetPasswordOpen}
            title= {t("Reset_Password_Screen.Reset_Password")}
            content={<ResetPassword onClose={handleCloseViadminResetPasswordDialog} />}
            onCancel={handleCloseViadminResetPasswordDialog}
            fullWidth={true}
            customClass="preferences-pop-main reset-pwd-only"
            maxWidth="xs"
          />
    </Box>
  );
};
export default LoginPage;
