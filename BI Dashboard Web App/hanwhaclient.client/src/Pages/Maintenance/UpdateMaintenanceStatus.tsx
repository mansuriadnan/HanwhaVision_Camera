import React, { useState } from "react";
import { Box, FormLabel, Grid, Tooltip } from "@mui/material";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import {
  updateStatusForm,
  updateStatusProps,
} from "../../interfaces/IMaintenance";
import { convertToUTC } from "../../utils/convertToUTC";
import dayjs from "dayjs";
import {
  UpdateMaintenanceScheduleStatusService,
  uploadMaintananceFileComapreService,
} from "../../services/maintenanceService";
import { CustomButton, CustomSelect, CustomTextField } from "../../components";
import { DateTimePicker, LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { formatDate } from "../../utils/dateUtils";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { IMaintenanceSchImgCompareParameter } from "../../interfaces/IChart";
import { useTranslation } from "react-i18next";
import InfoOutlinedIcon from "@mui/icons-material/InfoOutlined";

const UpdateMaintenanceStatus: React.FC<updateStatusProps> = ({
  selectedSchedule,
  beforeImg,
  afterImg,
  refreshData,
  onClose,
}) => {
  const { timeFormat } = useTimeFormatContext();
  const { control, handleSubmit, reset } = useForm<updateStatusForm>({
    defaultValues: {
      statusDatetime: dayjs(),
      notes: "",
      status: selectedSchedule?.status,
      maintenanceScheduleId: "",
    },
  });
  const [overallSimilarity, setOverallSimilarity] = useState<string>("");
  const [imgCompareParamenter, setImgCompareParameter] =
    useState<IMaintenanceSchImgCompareParameter>();
  const { t } = useTranslation();

  const statusList = [
    {
      title: "None",
      id: "",
    },
    {
      title: "Not Started",
      id: "Not Started",
    },
    {
      title: "In Progress",
      id: "In Progress",
    },
    {
      title: "Done",
      id: "Done",
    },
    {
      title: "Rework",
      id: "Rework",
    },
  ];

  const columns: GridColDef[] = [
    {
      field: "status",
      headerName: t("Maintenance_Schedule.Setting_Dialog.Grid_Column.Status"),
      width: 180,
      filterable: false,
    },
    {
      field: "notes",
      headerName: t("Maintenance_Schedule.Setting_Dialog.Grid_Column.notes"),
      width: 180,
      filterable: false,
    },

    {
      field: "statusDatetime",
      headerName: t(
        "Maintenance_Schedule.Setting_Dialog.Grid_Column.Status_Datetime"
      ),
      width: 180,
      filterable: false,
      renderCell: (params) => {
        const convertedStartDate = formatDateToConfiguredTimezone(params.value);
        const formattedStartDate = formatDate(convertedStartDate, timeFormat);
        return <span>{formattedStartDate}</span>;
      },
    },
  ];

  const updateScheduleStatus: SubmitHandler<updateStatusForm> = async (
    data
  ) => {
    const status_Date = data.statusDatetime
      ? convertToUTC(dayjs(data.statusDatetime).format("YYYY-MM-DDTHH:mm:ss"))
      : null;

    const update_ScheduleData = {
      statusDatetime: status_Date,
      notes: data.notes,
      status: data.status,
      maintenanceScheduleId:
        selectedSchedule && selectedSchedule?.id ? selectedSchedule.id : "",
    };

    var result: any = await UpdateMaintenanceScheduleStatusService(
      update_ScheduleData as updateStatusForm
    );

    if (result && result.isSuccess) {
      reset();
      refreshData();
    }
  };

  const base64ToFile = (base64: string, fileName: string): File => {
    // Split metadata and data
    const arr = base64.split(",");

    if (arr.length < 2) {
      throw new Error("Invalid base64 string");
    }

    // Extract MIME type
    const mimeMatch = arr[0].match(/:(.*?);/);
    const mime = mimeMatch ? mimeMatch[1] : "application/octet-stream";

    // Decode base64 data
    const bstr = atob(arr[1]);
    const n = bstr.length;
    const u8arr = new Uint8Array(n);

    for (let i = 0; i < n; i++) {
      u8arr[i] = bstr.charCodeAt(i);
    }

    return new File([u8arr], fileName, { type: mime });
  };

  const handleCompare = async () => {
    const beforeImage = base64ToFile(beforeImg, "beforeImg");
    const afterImage = base64ToFile(afterImg, "beforeImg");
    const formData = new FormData();
    formData.append("img1", beforeImage);
    formData.append("img2", afterImage);
    const response: any = await uploadMaintananceFileComapreService(formData);
    if (!response.isSuccess) {
      throw new Error(`Chunk upload failed: ${response.statusText}`);
    }
    const result: any = await response.data;
    setImgCompareParameter(result);
  };

  return (
    <Box
      // className="scheduled-maintenance-pop "
      className={`scheduled-maintenance-pop ${
        selectedSchedule &&
        ["In Progress", "Done", "Rework"].includes(selectedSchedule.status)
          ? ""
          : "afterimage"
      }`}
      style={{
        display:
          selectedSchedule &&
          ["In Progress", "Done", "Rework"].includes(selectedSchedule.status)
            ? ""
            : "flex",
      }}
    >
      <Box className="after-befor-box-wrapper">
        <Box className="after-maintenance">
          <p>{t("Maintenance_Schedule.Setting_Dialog.Before")}</p>
          <img src={`${beforeImg}`} alt="beforeImg" />
        </Box>
        {/* COMPARE SECTION */}
        <Box
          className="compare-section"
          sx={{
            border:
              imgCompareParamenter?.overallSimilarity !== undefined &&
              imgCompareParamenter?.overallSimilarity !== null
                ? "1px solid"
                : "none",
          }}
        >
          {imgCompareParamenter?.overallSimilarity !== undefined &&
            imgCompareParamenter?.overallSimilarity !== null && (
              <>
                <h2 className="percentage">
                  {imgCompareParamenter?.overallSimilarity}
                </h2>
                <p className="matched-text">
                  {t("Maintenance_Schedule.Setting_Dialog.Matched")}
                </p>
              </>
            )}

          {beforeImg != "" && afterImg != "" && (
            <CustomButton
              type="button"
              onClick={() => handleCompare()}
              customStyles={{ mt: 1 }}
              className="common-btn-design"
            >
              {t("Maintenance_Schedule.Setting_Dialog.Compare")}
            </CustomButton>
          )}
          {imgCompareParamenter != null && (
            <>
              <span className="compareImg">
                {t("Maintenance_Schedule.Setting_Dialog.brightness")}
                <Tooltip
                  title={t("Maintenance_Schedule.Tooltip.brightness_tooltip")}
                >
                  <InfoOutlinedIcon />
                </Tooltip>
                : {imgCompareParamenter.brightnessDifference}
              </span>
              <span className="compareImg">
                {t("Maintenance_Schedule.Setting_Dialog.contrast")}
                <Tooltip
                  title={t("Maintenance_Schedule.Tooltip.contrast_tooltip")}
                >
                  <InfoOutlinedIcon />
                </Tooltip>
                : {imgCompareParamenter.contrastDifference}
              </span>
              <span className="compareImg">
                {t("Maintenance_Schedule.Setting_Dialog.edgeOverlap")}
                <Tooltip
                  title={t("Maintenance_Schedule.Tooltip.edgeOverlap_tooltip")}
                >
                  <InfoOutlinedIcon />
                </Tooltip>
                : {imgCompareParamenter.edgeOverlap}
              </span>
              <span className="compareImg">
                {t("Maintenance_Schedule.Setting_Dialog.featureDisplacement")}
                <Tooltip
                  title={t(
                    "Maintenance_Schedule.Tooltip.featureDisplacement_tooltip"
                  )}
                >
                  <InfoOutlinedIcon />
                </Tooltip>
                : {imgCompareParamenter.featureDisplacement}
              </span>
              <span className="compareImg">
                {t("Maintenance_Schedule.Setting_Dialog.homographyShift")}
                <Tooltip
                  title={t(
                    "Maintenance_Schedule.Tooltip.homographyShift_tooltip"
                  )}
                >
                  <InfoOutlinedIcon />
                </Tooltip>
                : {imgCompareParamenter.homographyShift}
              </span>
              <span className="compareImg">
                {t("Maintenance_Schedule.Setting_Dialog.horizonTilt")}
                <Tooltip
                  title={t("Maintenance_Schedule.Tooltip.horizonTilt_tooltip")}
                >
                  <InfoOutlinedIcon />
                </Tooltip>
                : {imgCompareParamenter.horizonTiltDifference}
              </span>
              <span className="compareImg">
                {t("Maintenance_Schedule.Setting_Dialog.structuralSimilarity")}
                <Tooltip
                  title={t(
                    "Maintenance_Schedule.Tooltip.structuralSimilarity_tooltip"
                  )}
                >
                  <InfoOutlinedIcon />
                </Tooltip>
                : {imgCompareParamenter.structuralSimilarity}
              </span>
            </>
          )}
        </Box>
        {selectedSchedule &&
          ["In Progress", "Done", "Rework"].includes(
            selectedSchedule.status
          ) && (
            <Box className="before-maintenance">
              <p>{t("Maintenance_Schedule.Setting_Dialog.After")}</p>
              <img src={`${afterImg}`} alt="afterImg" />
            </Box>
          )}
      </Box>

      <Box
        component="form"
        onSubmit={handleSubmit(updateScheduleStatus)}
        noValidate
      >
        <div className="cmn-pop-form-inner">
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <CustomSelect
                name="status"
                variant="filled"
                control={control}
                label={
                  <span>
                    {t(
                      "Maintenance_Schedule.Setting_Dialog.Maintenance_Status"
                    )}
                    <span className="star-error">*</span>
                  </span>
                }
                rules={{
                  required: t(
                    "Maintenance_Schedule.Setting_Dialog.Maintenance_status_required"
                  ),
                }}
                options={statusList}
                placeholder={t("Maintenance_Schedule.Select_status")}
                enableOnly={
                  selectedSchedule && selectedSchedule.status === "Not Started"
                    ? ["In Progress"]
                    : ["Done", "Rework"]
                }
              />
            </Grid>
            <Grid item xs={12} md={6} className="custom-label-for-maintenance">
              <LocalizationProvider dateAdapter={AdapterDayjs}>
                <FormLabel
                  sx={{
                    mt: 6,
                    mb: 10,
                    fontWeight: 500,
                    fontSize: "14px",
                  }}
                >
                  {t("Maintenance_Schedule.Setting_Dialog.Start_Date")}
                  <span className="star-error">*</span>
                </FormLabel>
                <Controller
                  name="statusDatetime"
                  control={control}
                  render={({ field, fieldState }) => (
                    <DateTimePicker
                      value={field.value}
                      format={timeFormat === "24h"
                        ? "DD-MM-YYYY HH:mm"
                        : "DD-MM-YYYY hh:mm A"}
                      ampm={timeFormat !== "24h"}
                      onChange={field.onChange}
                      disableFuture
                      slotProps={{
                        textField: {
                          error: !!fieldState.error,
                          helperText: fieldState.error?.message,
                          fullWidth: true,
                        },
                      }}
                    />
                  )}
                  rules={{
                    validate: (value) => {
                      if (!value) {
                        return t("Maintenance_Schedule.Setting_Dialog.Start_Date_Required");
                      }
                      if (value.isAfter(dayjs())) {
                        return t(
                          "Maintenance_Schedule.Setting_Dialog.Start_Date_Strong_Validation",
                        );
                      }
                      return true;
                    },
                  }}
                />
              </LocalizationProvider>
            </Grid>
          </Grid>

          <CustomTextField
            name="notes"
            label={
              <span>
                {t("Maintenance_Schedule.Add_Schedule_Drawer.Notes")}{" "}
                <span className="star-error">*</span>
              </span>
            }
            control={control}
            rules={{
              required: t(
                "Maintenance_Schedule.Add_Schedule_Drawer.Notes_Required"
              ),
            }}
            placeholder={t(
              "Maintenance_Schedule.Add_Schedule_Drawer.Notes_Placeholder"
            )}
            required
            fullWidth
          />

          <Box className="maintenance-btn-wrapper">
            <CustomButton
              className="common-btn-design"
              fullWidth
              customStyles={{ mt: 2, mr: 2 }}
            >
              {t("Save_btn")}
            </CustomButton>
            <CustomButton
              type="button"
              className="common-btn-design common-btn-design-transparent"
              fullWidth
              customStyles={{ mt: 2 }}
              onClick={() => {
                reset();
                onClose();
              }}
            >
              {t("Common_DELETE_Confirmation_Dialog.Cancel")}
            </CustomButton>
          </Box>
        </div>
      </Box>

      <DataGrid
        rows={selectedSchedule?.statusHistory ?? []}
        columns={columns}
        // getRowId={(row) => row.id}
        getRowId={(row) => row.statusDatetime}
        disableRowSelectionOnClick
        hideFooter
        hideFooterPagination
        hideFooterSelectedRowCount
        autoHeight={false}
      />
    </Box>
  );
};

export { UpdateMaintenanceStatus };
