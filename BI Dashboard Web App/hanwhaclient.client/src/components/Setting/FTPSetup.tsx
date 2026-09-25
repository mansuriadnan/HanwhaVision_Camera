import React, { useEffect, useState } from "react";
import { useForm, Controller } from "react-hook-form";
import {
  Box,
  Button,
  Typography,
  Checkbox,
  FormControlLabel,
  Grid,
  Paper,
} from "@mui/material";
import { CustomTextField } from "../Reusable/CustomTextField";
import { COMMON_CONSTANTS, LABELS, REGEX } from "../../utils/constants";
import { CustomButton } from "../Reusable/CustomButton";
import {
    saveFTPSettings,
  saveSmtpSettings,
} from "../../services/settingService";
import { IFTPSettings, ISmtpSettings } from "../../interfaces/ISettings";
import { HasPermission } from "../../utils/screenAccessUtils";
import { useTranslation } from "react-i18next";

type FTPFormValues = {
  host: string;
  port: string;
  username: string;
  password: string;
};

type Props = {
  FTPSettings?: FTPFormValues;
};

export const FTPSetup = ({ FTPSettings }: Props) => {
  const { t } = useTranslation();
  const [initialValues, setInitialValues] = useState<FTPFormValues | null>(
    null
  );
  const {
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<FTPFormValues>({
    defaultValues: {
      host: "",
      port: "",
      username: "",
      password: ""
    },
  });

  useEffect(() => {
    if (FTPSettings) {
      reset({
        host: FTPSettings?.host ?? "",
        port: FTPSettings?.port?.toString() ?? "",
        username: FTPSettings?.username ?? "",
        password: FTPSettings?.password ?? "",
      });
    }
  }, [FTPSettings, reset]);


  const onSubmit = async (data: FTPFormValues) => {
    try {
      const ftpPayload: IFTPSettings = {
        host: data.host,
        port: Number(data.port),
        username: data.username,
        password: data.password,
      };
      const response: any = await saveFTPSettings(ftpPayload);
      if (response?.isSuccess) {
        setInitialValues(data);
        reset(data);
      }
    } catch (err: any) {
      console.error("Failed to update FTP details. Please try again.", err);
    }
  };

  return (
    <Paper elevation={1} className='smtp-setup-wrapper'>
      <Typography variant="h4">
       {t("General_Settings.FTP_Setting.FTP_Setup")}
      </Typography>

      <Box onSubmit={handleSubmit(onSubmit)} component="form" noValidate>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <CustomTextField
              control={control}
              name="host"
              label={<span>{t("General_Settings.SMTP_Setup.Host")} <span className="star-error">*</span></span>}
              rules={{
                required: t("General_Settings.SMTP_Setup.Validation.Host_Required"),
                maxLength: {
                  value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                  message: t("General_Settings.SMTP_Setup.Validation.Host_Max_Validation", {max : COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                },
                pattern: {
                  value: REGEX.HostOrIP_Regex,
                  message: t("General_Settings.FTP_Setting.Validation.Host_Strong_Validation"),
                },
              }}
              required
              placeholder={t("General_Settings.SMTP_Setup.Host_Placeholder")}
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <CustomTextField
              control={control}
              name="port"
              label={<span>{t("General_Settings.SMTP_Setup.Port")} <span className="star-error">*</span></span>}
              type="number"
              rules={{
                required: t("General_Settings.SMTP_Setup.Validation.Port_Required"),
                validate: (value: string) => {
                  const port = parseInt(value, 10);
                  if (isNaN(port) || port < 1 || port > 65535) {
                    return t("General_Settings.SMTP_Setup.Validation.Port_Strong_Validation");
                  }
                  return true;
                },
              }}
              required
              placeholder={t("General_Settings.SMTP_Setup.Post_Placeholder")}
            />
          </Grid>         
          <Grid item xs={12} md={6}>
            <CustomTextField
              control={control}
              name="username"
              label={<span>{ t("Manage_User.Add_Edit_User_Drawer.Username")} <span className="star-error">*</span></span>}
              rules={{
                required: t("Manage_User.Validation.Username_Required"),
                pattern: {
                  value: REGEX.UserName_Regex,
                  message: "Enter a valid username",
                },
                maxLength: {
                  value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                  message: t("Manage_User.Validation.Username_Strong_Validation", { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                },
              }}
              required
              placeholder={ t("Manage_User.Add_Edit_User_Drawer.Username_Placeholder")}
              autoComplete="off"
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <CustomTextField
              control={control}
              name="password"
              label={<span>{t("Manage_User.Add_Edit_User_Drawer.Password")} <span className="star-error">*</span></span>}
              type="password"
              rules={{
                required: t("Manage_User.Validation.Password_Required"),
                // pattern: {
                //   value: REGEX.Password_Regex,
                //   message:
                //     "Password must be at least 8 characters long and include one uppercase letter, one lowercase letter, one number, and one special character (excluding *)",
                // },
                // maxLength: {
                //   value: COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH,
                //   message: `Password cannot exceed ${COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH} characters`,
                // },
              }}
              required
              placeholder={ t("Manage_User.Add_Edit_User_Drawer.Password_Placeholder")}
              autoComplete="new-password" 
            />
          </Grid>          
        </Grid>
        {HasPermission(LABELS.View_and_Configure_FTP_Setup_Details) && (
          <CustomButton className="common-btn-design">{t("Save_btn")}</CustomButton>
        )}
      </Box>
    </Paper>
  );
};
