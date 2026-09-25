import React, { useEffect, useState } from "react";
import { Box, FormLabel, TextField } from "@mui/material";
import { CustomTextField } from "../../components/Reusable/CustomTextField";
import { CustomButton } from "../../components/Reusable/CustomButton";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import { LocalizationProvider, DateTimePicker } from "@mui/x-date-pickers";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs from "dayjs";
import { convertToUTC } from "../../utils/convertToUTC";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import {
  RMAFormProps,
  RMAAddEditFormProps,
  IDeviceList,
} from "../../interfaces/IMaintenance";
import { useTranslation } from "react-i18next";
import Autocomplete from "@mui/material/Autocomplete";
import {
  SaveRMAService,
  GetAllDeviceForMaintenancePlanService,
} from "../../services/maintenanceService";
import { CustomSelect } from "../../components";
import { ILookup } from "../../interfaces/ILookup";
import {
  GetAllFloorsListService,
  GetAllZonesByFloorIdService,
} from "../../services/dashboardService";
import { useTimeFormatContext } from "../../context/TimeFormatContext";

const AddEditRMA: React.FC<RMAAddEditFormProps> = ({
  onClose,
  RMAData,
  refreshData,
}) => {
  const isEditMode = RMAData !== null && RMAData !== undefined;
  const { t } = useTranslation();
  const [deviceList, setDeviceList] = useState<IDeviceList[]>([]);
  const [allDevices, setAllDevices] = useState<IDeviceList[]>([]);
  const [floorList, setFloorList] = useState<ILookup[]>([]);
  const [zoneList, setZoneList] = useState<ILookup[]>([]);
  const { timeFormat } = useTimeFormatContext();

  const {
    control,
    setValue,
    handleSubmit,
    watch,
    getValues,
    formState: { isDirty },
    // formState: { errors },
  } = useForm<RMAFormProps>({
    defaultValues: {
      deviceId: "",
      rmaStatus: "InProgress",
      inProgressNotes: "",
      completedNotes: "",
      startDate: dayjs(),
      endDate: dayjs(),
      floorId: "",
      zoneId: "",
    },
  });

  const RMAStatus = watch("rmaStatus");
  const floorId = watch("floorId") ?? "";
  const zoneId = watch("zoneId") ?? "";

  const statusList = [
    {
      title: "In Progress",
      id: "InProgress",
    },
    {
      title: "Completed",
      id: "Completed",
    },
  ];

  useEffect(() => {
    if (isEditMode && RMAData) {
      setValue("deviceId", RMAData?.deviceId);
      setValue("rmaStatus", RMAData?.rmaStatus);
      setValue(
        "startDate",
        dayjs(formatDateToConfiguredTimezone(RMAData?.startDate as any)),
      );
      setValue(
        "endDate",
        dayjs(formatDateToConfiguredTimezone(RMAData?.endDate as any)),
      );
      setValue("inProgressNotes", RMAData?.inProgressNotes);
      setValue("completedNotes", RMAData?.completedNotes);
      setValue("floorId", RMAData?.floorId);
      setValue("zoneId", RMAData?.zoneId);
    }
  }, [isEditMode, RMAData]);

  useEffect(() => {
    fetchDeviceList();
    fetchFloorData();
  }, []);

  useEffect(() => {
    fetchZoneData(floorId);
    if (!isEditMode) {
      setValue("zoneId", "");
      setValue("deviceId", "");
    }
  }, [floorId]);

  useEffect(() => {
    let filtered = [...allDevices];

    if (floorId) {
      filtered = filtered.filter((d) => d.floorId === floorId);
    }

    if (zoneId) {
      filtered = filtered.filter((d) => d.zoneId === zoneId);
    }

    setDeviceList(filtered);

    if (!isEditMode) {
      const selected = getValues("deviceId");
      if (selected && !filtered.some((d) => d.id === selected)) {
        setValue("deviceId", "");
      }
    }
  }, [floorId, zoneId, allDevices]);

  const fetchFloorData = async () => {
    try {
      const response = await GetAllFloorsListService();
      const floorData = response?.map((item) => ({
        title: item.floorPlanName,
        id: item.id,
      }));
      setFloorList(floorData as ILookup[]);
    } catch (err: any) {
      console.error("Error while fetching the floor data");
    }
  };

  const fetchZoneData = async (floorId: string) => {
    if (!floorId) {
      setZoneList([]);
      return;
    }

    try {
      const response: any = await GetAllZonesByFloorIdService([floorId]);
      const allZones: ILookup[] = (response?.data ?? []).flatMap(
        (floor: any) =>
          Array.isArray(floor?.zones)
            ? floor.zones.map((zone: any) => ({
                id: zone.id,
                title: zone.zoneName,
              }))
            : [],
      );
      setZoneList(allZones);
    } catch (err: any) {
      console.error("Error while fetching the zone data:", err?.message || err);
      setZoneList([]);
    }
  };

  const fetchDeviceList = async () => {
    try {
      const deviceData: any = await GetAllDeviceForMaintenancePlanService();

      if (Array.isArray(deviceData)) {
        const formatted = deviceData.map((item: any) => ({
          id: item.deviceId,
          deviceName: item.deviceName,
          floorId: item.floorId,
          zoneId: item.zoneId,
        }));

        setAllDevices(formatted);
        setDeviceList(formatted);
      } else {
        setAllDevices([]);
        setDeviceList([]);
      }
    } catch (err) {
      console.error("Error fetching device data:", err);
    }
  };

  const addRMA: SubmitHandler<RMAFormProps> = async (data) => {
    const start_Date = data.startDate
      ? convertToUTC(dayjs(data.startDate).format("YYYY-MM-DDTHH:mm:ss"))
      : null;

    const end_Date =
      data.rmaStatus === "Completed" && data.endDate?.isValid()
        ? convertToUTC(dayjs(data.endDate).format("YYYY-MM-DDTHH:mm:ss"))
        : null;

    const RMA_Data = {
      deviceId: data.deviceId,
      rmaStatus: data.rmaStatus,
      startDate: start_Date,
      endDate: end_Date,
      inProgressNotes: data.inProgressNotes,
      completedNotes: data.completedNotes,
      floorId: data.floorId,
      zoneId: data.zoneId,
      ...(isEditMode && RMAData?.id && { id: RMAData.id }),
    };

    var result: any = await SaveRMAService(RMA_Data as RMAFormProps);

    if (result && result.isSuccess) {
      onClose();
      refreshData();
    }
  };

  return (
    <div className="cmn-pop-form">
      <div className="cmn-pop-form-wrapper">
        <Box
          component="form"
          onSubmit={handleSubmit(addRMA)}
          noValidate
          sx={{ width: "100%" }}
        >
          <div className="cmn-pop-form-inner">
            <CustomSelect
              name="floorId"
              variant="filled"
              control={control}
              options={floorList}
              label={t("Maintanace_Plan.Maintanace_Plan_Grid.Floor")}
              placeholder={t(
                "Maintanace_Plan.Maintanace_Plan_Grid.Select_Floor",
              )}
            />
            <CustomSelect
              name="zoneId"
              variant="filled"
              control={control}
              options={zoneList}
              label={t("Maintanace_Plan.Maintanace_Plan_Grid.Zone")}
              placeholder={t(
                "Maintanace_Plan.Maintanace_Plan_Grid.Select_Zone",
              )}
            />
            <FormLabel>
              <span>
                {t("RMA.RMA_Form_Device")}
                <span className="star-error">*</span>
              </span>
            </FormLabel>
            <Controller
              name="deviceId"
              control={control}
              rules={{
                required: t("RMA.RMA_Form_Validation_Deviceisrequired"),
              }}
              render={({ field, fieldState }) => (
                <Autocomplete
                  options={deviceList}
                  value={deviceList.find((d) => d.id === field.value) || null}
                  onChange={(_, selectedOption) => {
                    field.onChange(selectedOption?.id || null);
                  }}
                  getOptionLabel={(option) => option.deviceName || ""}
                  isOptionEqualToValue={(option, value) =>
                    option.id === value.id
                  }
                  // style={{ width: 500 }}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      placeholder={t("RMA.RMA_Form_PH_SelectDevice")}
                      error={!!fieldState.error}
                      helperText={fieldState.error?.message}
                    />
                  )}
                />
              )}
            />
            <CustomSelect
              name="rmaStatus"
              variant="filled"
              control={control}
              label={t("RMA.RMA_Form_Maintenancestatus")}
              options={statusList}
              placeholder={t("RMA.RMA_Form_PH_Device")}
              enableOnly={
                isEditMode ? ["Completed", "InProgress"] : ["InProgress"]
              }
            />
            <LocalizationProvider dateAdapter={AdapterDayjs}>
              <Box className="star-end-date-plan">
                <FormLabel>
                  <span>
                    {t("RMA.RMA_Grid_StartDate")}
                    <span className="star-error">*</span>
                  </span>
                </FormLabel>
                <Controller
                  name="startDate"
                  control={control}
                  rules={{
                    required: t("RMA.RMA_Form_Validation_StartDateTsRequired"),
                  }}
                  render={({ field, fieldState }) => (
                    <DateTimePicker
                      value={field.value}
                      format={timeFormat === "24h"
                        ? "DD-MM-YYYY HH:mm"
                        : "DD-MM-YYYY hh:mm A"}
                      ampm={timeFormat !== "24h"}
                      onChange={field.onChange}
                      slotProps={{
                        textField: {
                          error: !!fieldState.error,
                          helperText: fieldState.error?.message,
                        },
                      }}
                    />
                  )}
                />
              </Box>
              <Box className="star-end-date-plan">
                <FormLabel>
                  <span>
                    {t("RMA.RMA_Grid_EndDate")}
                    <span className="star-error">
                      {RMAStatus == "Completed" ? "*" : ""}
                    </span>
                  </span>
                </FormLabel>
                <Controller
                  name="endDate"
                  control={control}
                  rules={{
                    // required: "End Date is required.",
                    validate: (value) => {
                      if (
                        !value ||
                        (!dayjs(value).isValid() && RMAStatus == "Completed")
                      ) {
                        return t(
                          "Maintanace_Plan.Maintanance_Plan_Drawer.EndDate_Requiered",
                        );
                      }
                      const startDate = getValues("startDate");
                      if (RMAStatus !== "Completed") return true;
                      if (!value || !startDate) return true;
                      if (!dayjs(value).isAfter(dayjs(startDate))) {
                        return t(
                          "RMA.RMA_Form_Validation_EndDateMustBeGreaterThanStartDate",
                        );
                      }
                      return true;
                    },
                  }}
                  render={({ field, fieldState }) => (
                    <DateTimePicker
                      disabled={RMAStatus !== "Completed"}
                      format={timeFormat === "24h"
                        ? "DD-MM-YYYY HH:mm"
                        : "DD-MM-YYYY hh:mm A"}
                      ampm={timeFormat !== "24h"}
                      value={field.value}
                      onChange={field.onChange}
                      slotProps={{
                        textField: {
                          error: !!fieldState.error,
                          helperText: fieldState.error?.message,
                        },
                      }}
                    />
                  )}
                />
              </Box>
            </LocalizationProvider>

            {RMAStatus === "InProgress" && (
              <CustomTextField
                name="inProgressNotes"
                label={
                  <span>
                    {t("RMA.RMA_Form_InProgressNotes")}{" "}
                    <span className="star-error">*</span>
                  </span>
                }
                control={control}
                rules={{
                  required: t(
                    "RMA.RMA_Form_Validation_InProgressNotesIsRequired",
                  ),
                }}
                placeholder={t("RMA.RMA_Form_PH_EnterInProgressnotes")}
                required
                fullWidth
              />
            )}

            {RMAStatus === "Completed" && (
              <CustomTextField
                name="completedNotes"
                label={
                  <span>
                    {t("RMA.RMA_Form_CompletedNotes")}{" "}
                    <span className="star-error">*</span>
                  </span>
                }
                control={control}
                rules={{
                  required: t(
                    "RMA.RMA_Form_Validation_CompletedNotesIsRequired",
                  ),
                }}
                placeholder={t("RMA.RMA_Form_PH_CompletedNotes")}
                required
                fullWidth
              />
            )}

            <CustomButton
              className="common-btn-design"
              fullWidth
              customStyles={{ mt: 2 }}
              disabled={isEditMode && !isDirty}
            >
              {isEditMode ? t("Update") : t("Add_btn")}
            </CustomButton>
          </div>
        </Box>
      </div>
    </div>
  );
};

export { AddEditRMA };
