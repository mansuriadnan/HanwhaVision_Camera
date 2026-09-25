import React, { useEffect, useState } from "react";
import { useForm, Controller } from "react-hook-form";
import {
  Box,
  Typography,
  Checkbox,
  FormControlLabel,
  Grid,
  Paper,
} from "@mui/material";
import { CustomTextField } from "../Reusable/CustomTextField";
import { LABELS } from "../../utils/constants";
import { CustomButton } from "../Reusable/CustomButton";
import { saveBackupDbSettings } from "../../services/settingService";
import { IBackupDbSettings } from "../../interfaces/ISettings";
import { HasPermission } from "../../utils/screenAccessUtils";
import { useTranslation } from "react-i18next";

type BackupDBFormValues = {
  // enable: boolean;
  path: string;
};

type Props = {
  BackupDBSettings?: BackupDBFormValues;
};

export const BackupDatabase = ({ BackupDBSettings }: Props) => {
  const [initialValues, setInitialValues] = useState<BackupDBFormValues | null>(
    null
  );
  const {
    handleSubmit,
    control,
    reset,
    watch,
    setValue,
    formState: { errors },
  } = useForm<BackupDBFormValues>({
    defaultValues: {
      //enable: false,
      path: "",
    },
  });

  const { t } = useTranslation();

  useEffect(() => {
    if (BackupDBSettings) {
      reset({
        // enable: BackupDBSettings?.enable ?? false,
        path: BackupDBSettings?.path ?? "",
      });
    }
  }, [BackupDBSettings, reset]);

  const onSubmit = async (data: BackupDBFormValues) => {
    try {
      const backupDbPayload: IBackupDbSettings = {
        // Enable: data.enable,
        Path: data.path,
      };
      const response: any = await saveBackupDbSettings(backupDbPayload);
      if (response?.isSuccess) {
        setInitialValues(data);
        reset(data);
      }
    } catch (err: any) {
      console.error(
        "Failed to update Retention details. Please try again.",
        err
      );
    }
  };

  return (
    <Paper elevation={1} className="smtp-setup-wrapper">
      <Typography variant="h4">
        {t(
          "General_Settings.Backup_Database_Setting.Scheduled_Database_Backup"
        )}
        {/* Database Backup */}
      </Typography>

      <Box onSubmit={handleSubmit(onSubmit)} component="form" noValidate>
        <Grid container spacing={2}>
            <Grid item xs={12} md={6} lg={4}>
              <CustomTextField
                control={control}
                name="path"
                rules={{
                  required: t(
                    "General_Settings.Backup_Database_Setting.BackupPath_Required"
                  ),
                }}
                required
                //  placeholder="Enter Backup database path"
                placeholder={t(
                  "General_Settings.Backup_Database_Setting.BackupPath_Placeholder"
                )}
                autoComplete="off"
              />
            </Grid>
        </Grid>
        {HasPermission(LABELS.ScheduledDatabaseBackup) && (
          <CustomButton className="common-btn-design">
            {t("Save_btn")}
          </CustomButton>
        )}
      </Box>
    </Paper>
  );
};
