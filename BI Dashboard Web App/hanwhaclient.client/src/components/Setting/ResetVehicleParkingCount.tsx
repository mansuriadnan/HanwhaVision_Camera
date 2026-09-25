import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { Box, Typography, Grid, Paper } from "@mui/material";
import { LABELS } from "../../utils/constants";
import { CustomButton } from "../Reusable/CustomButton";
import { ResetParkingCountService } from "../../services/settingService";
import { HasPermission } from "../../utils/screenAccessUtils";
import { useTranslation } from "react-i18next";
import { CustomTextField } from "../Reusable/CustomTextField";
import { IResetVehiclePayload, IResetVehicleSettings } from "../../interfaces/ISettings";


export const ResetVehicleParkingCount = () => {

  const { t } = useTranslation();

  const { handleSubmit, control, setValue } = useForm<IResetVehicleSettings>({
      defaultValues: {
        count: 0,
      },
    });

  const onSubmit = async (data: IResetVehicleSettings) => {
    try {
      const payload: IResetVehiclePayload = {
        CurrentCount : data.count
      }
      await ResetParkingCountService(payload);
    } catch (err: any) {
      console.error(
        "Failed to reset vehicle parking count. Please try again.",
        err,
      );
    }
  };

  return (
    <Paper elevation={1} className="smtp-setup-wrapper">
      <Typography variant="h4">
        {t("General_Settings.Reset_Vehicle.Reset_Vehicle_Parking_Count")}
      </Typography>

      <Box onSubmit={handleSubmit(onSubmit)} component="form" noValidate>
        <Grid container item xs={12} md={4} >
          <CustomTextField
            control={control}
            name="count"
            label={""}
            type="number"
            rules={{
              required: t("General_Settings.Reset_Vehicle.RequiredCount"),
            }}
            required
            placeholder={t(
              "General_Settings.Reset_Vehicle.count",
            )}
            onKeyDown={(e) => {
              // Allow control keys
              if (
                e.key === "Backspace" ||
                e.key === "Delete" ||
                e.key === "ArrowLeft" ||
                e.key === "ArrowRight" ||
                e.key === "Tab"
              ) {
                return;
              }

              // Block non-numeric keys
              if (!/^[0-9]$/.test(e.key)) {
                e.preventDefault();
              }
            }}
          />
        </Grid>
        {HasPermission(LABELS.View_and_Configure_Reset_Vehicle_Parking_Count) && (
          <CustomButton className="common-btn-design">
            {t("General_Settings.Reset_Vehicle.Reset")}
          </CustomButton>
        )}
      </Box>
    </Paper>
  );
};
