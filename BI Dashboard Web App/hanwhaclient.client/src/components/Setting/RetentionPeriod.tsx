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
  saveRetentionSettings,
  saveSmtpSettings,
} from "../../services/settingService";
import { IFTPSettings, IRetentionSettings, ISmtpSettings } from "../../interfaces/ISettings";
import { HasPermission } from "../../utils/screenAccessUtils";
import { useTranslation } from "react-i18next";

type RetentionFormValues = {
  enable: boolean,
  retentionPeriod: number,
};

type Props = {
  RetentionSettings?: RetentionFormValues;
};

export const RetentionPeriod = ({ RetentionSettings }: Props) => {
  const [initialValues, setInitialValues] = useState<RetentionFormValues | null>(
    null
  );
  const {
    handleSubmit,
    control,
    reset,
    watch,
    setValue,
    formState: { errors },
  } = useForm<RetentionFormValues>({
    defaultValues: {
      enable: false,
      retentionPeriod: 0,
    },
  });

  const useisEnabledwatch = watch("enable");
  const { t } = useTranslation();

  useEffect(() => {
    if (RetentionSettings) {
      reset({
        enable: RetentionSettings?.enable ?? false,
        retentionPeriod: RetentionSettings?.retentionPeriod ?? 0,

      });
    }
  }, [RetentionSettings, reset]);

  const onSubmit = async (data: RetentionFormValues) => {
    try {
      const finalRetentionPeriod = data.enable ? Number(data.retentionPeriod) : 0;

      const retentionPayload: IRetentionSettings = {
        Enable: data.enable,
        RetentionPeriod: finalRetentionPeriod,

      };
      const response: any = await saveRetentionSettings(retentionPayload);
      if (response?.isSuccess) {
        setInitialValues(data);
        reset(data);
      }
    } catch (err: any) {
      console.error("Failed to update Retention details. Please try again.", err);
    }
  };

  return (
    <Paper elevation={1} className='smtp-setup-wrapper'>
      <Typography variant="h4">
        {t("General_Settings.Retention_Period_Setting.Retention_Period")}
      </Typography>

      <Box onSubmit={handleSubmit(onSubmit)} component="form" noValidate>
        <Grid container spacing={2}>
          <Grid item xs={12} md={2}>
            <Controller
              name="enable"
              control={control}
              render={({ field }) => (
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={field.value}
                      onChange={() => setValue("enable", !field.value)}
                    />
                  }
                  label= {t("General_Settings.Retention_Period_Setting.Enabled")}
                />
              )}
            />
          </Grid>
          <Grid item xs={12} md={4}>
            <CustomTextField
              control={control}
              name="retentionPeriod"
              label={<span>{t("General_Settings.Retention_Period_Setting.Period")} <span className="star-error">*</span></span>}
              type="number"
              rules={{
                required: t("General_Settings.Retention_Period_Setting.Validation.Period_Required"),
                validate: (value: string) => {
                  const period = parseInt(value, 10);
                  if (isNaN(period) || period < 1 || period >= 99) {
                    return t("General_Settings.Retention_Period_Setting.Validation.Period_Strong_Validation");
                  }
                  return true;
                },
              }}
              required
              placeholder={t("General_Settings.Retention_Period_Setting.Period_Plceholder")}
              disabled={useisEnabledwatch ? false : true}
            />
          </Grid>

        </Grid>
        {HasPermission(LABELS.RetentionPeriod) && (
          <CustomButton className="common-btn-design">{t("Save_btn")}</CustomButton>
        )}
      </Box>
    </Paper>
  );
};
