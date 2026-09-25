import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { Box, Typography, Grid, Paper } from "@mui/material";
import { CustomTextField } from "../Reusable/CustomTextField";
import { LABELS } from "../../utils/constants";
import { CustomButton } from "../Reusable/CustomButton";
import { SaveANPRConfigurationService } from "../../services/settingService";
import { IANPRSettings } from "../../interfaces/ISettings";
import { HasPermission } from "../../utils/screenAccessUtils";
import { useTranslation } from "react-i18next";

type Props = {
  ANPRConfiguration?: IANPRSettings;
};

export const ANPRConfiguration = ({ ANPRConfiguration }: Props) => {
  const { handleSubmit, control, setValue } = useForm<IANPRSettings>({
    defaultValues: {
      imagePath: "",
      retentionPeriod: 0,
    },
  });

  const { t } = useTranslation();

  useEffect(() => {
    if (ANPRConfiguration) {
      setValue("imagePath", ANPRConfiguration?.imagePath || "");
      setValue("retentionPeriod", ANPRConfiguration?.retentionPeriod);
    }
  }, [ANPRConfiguration]);

  const onSubmit = async (data: IANPRSettings) => {
    try {
      const anprConfigPayload: IANPRSettings = {
        imagePath: data.imagePath,
        retentionPeriod:
          data.retentionPeriod === "" ? null : data.retentionPeriod,
      };

      await SaveANPRConfigurationService(anprConfigPayload);
    } catch (err: any) {
      console.error(
        "Failed to update ANPR configuration details. Please try again.",
        err,
      );
    }
  };

  return (
    <Paper elevation={1} className="smtp-setup-wrapper">
      <Typography variant="h4">
        {t("General_Settings.ANPR_Setting.ANPR_Configuration")}
      </Typography>

      <Box onSubmit={handleSubmit(onSubmit)} component="form" noValidate>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6} lg={4}>
            <CustomTextField
              control={control}
              label={t("General_Settings.ANPR_Setting.Image_Path")}
              name="imagePath"
              placeholder={t(
                "General_Settings.ANPR_Setting.Image_Path_Placeholder",
              )}
              autoComplete="off"
            />
          </Grid>
          <Grid item xs={12} md={4}>
            <CustomTextField
              control={control}
              name="retentionPeriod"
              label={
                <span>
                  {t("General_Settings.Retention_Period_Setting.Period")}{" "}
                </span>
              }
              type="number"
              rules={{
                validate: (value: string) => {
                  if (!value) return true;
                  const period = parseInt(value, 10);
                  if (isNaN(period) || period < 1 || period >= 99) {
                    return t(
                      "General_Settings.Retention_Period_Setting.Validation.Period_Strong_Validation",
                    );
                  }
                  return true;
                },
              }}
              required
              placeholder={t(
                "General_Settings.Retention_Period_Setting.Period_Plceholder",
              )}
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
