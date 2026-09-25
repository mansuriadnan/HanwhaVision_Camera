import React, { useEffect, useState } from "react";
import { Box, Checkbox, FormLabel, TextField } from "@mui/material";
import { CustomTextField } from "../../components/Reusable/CustomTextField";
import { CustomButton } from "../../components/Reusable/CustomButton";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import { LocalizationProvider, DatePicker } from "@mui/x-date-pickers";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs from "dayjs";
import { convertToUTC } from "../../utils/convertToUTC";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import {
  IPlanProps,
  planAddEditFormProps,
  IDeviceList,
} from "../../interfaces/IMaintenance";
import { useTranslation } from "react-i18next";
import Autocomplete from "@mui/material/Autocomplete";
import CheckBoxOutlineBlankIcon from "@mui/icons-material/CheckBoxOutlineBlank";
import CheckBoxIcon from "@mui/icons-material/CheckBox";
import {
  SaveMaintenancePlanService,
  GetAllDeviceForMaintenancePlanService,
} from "../../services/maintenanceService";
import { COMMON_CONSTANTS } from "../../utils/constants";
import { ILookup } from "../../interfaces/ILookup";
import {
  GetAllFloorsListService,
  GetAllZonesByFloorIdService,
} from "../../services/dashboardService";
import { CustomMultiSelect } from "../../components";

const AddEditPlan: React.FC<planAddEditFormProps> = ({
  onClose,
  planData,
  refreshData,
}) => {
  const isEditMode = planData !== null && planData !== undefined;
  const { t } = useTranslation();
  const [floorList, setFloorList] = useState<ILookup[]>([]);
  const [zoneList, setZoneList] = useState<ILookup[]>([]);
  const [allDevices, setAllDevices] = useState<IDeviceList[]>([]);
  const [deviceList, setDeviceList] = useState<IDeviceList[]>([]);

  const SELECT_ALL_OPTION = {
    id: "ALL",
    deviceName: "Select All",
  };

  const {
    control,
    setValue,
    getValues,
    reset,
    handleSubmit,
    watch,
    // formState: { errors },
    formState: { isDirty },
  } = useForm<IPlanProps>({
    defaultValues: {
      planName: "",
      duration: null,
      startDate: null,
      endDate: null,
      deviceIds: [],
      floorIds: [],
      zoneIds: [],
    },
  });

  const floorIds = watch("floorIds") ?? [];
  const zoneIds = watch("zoneIds") ?? [];
  const deviceIds = watch("deviceIds") ?? [];

  useEffect(() => {
    fetchDeviceList();
    fetchFloorData();
  }, []);

  useEffect(() => {
    fetchZoneData(floorIds as string[]);
  }, [floorIds]);

  useEffect(() => {
    let filtered = [...allDevices];

    if (floorIds?.length > 0) {
      filtered = filtered.filter((device) =>
        floorIds.includes(device?.floorId),
      );
    }

    if (zoneIds?.length > 0) {
      filtered = filtered.filter((device) => zoneIds.includes(device?.zoneId));
    }

    setDeviceList(filtered);

    // Clear selected devices if not valid anymore
    if (!isEditMode) {
      const validDeviceIds = filtered.map((d) => d.id);
      const selected = getValues("deviceIds") || [];

      setValue(
        "deviceIds",
        selected.filter((id) => validDeviceIds.includes(id)),
      );
    }
  }, [floorIds, zoneIds, allDevices]);

  useEffect(() => {
    if (isEditMode && planData) {
      setValue("planName", planData?.planName);
      setValue("duration", planData?.duration || 0);
      setValue(
        "deviceIds",
        Array.isArray(planData?.deviceIds) ? planData?.deviceIds : [],
      );
      setValue(
        "startDate",
        dayjs(formatDateToConfiguredTimezone(planData?.startDate as any)),
      );
      setValue(
        "endDate",
        dayjs(formatDateToConfiguredTimezone(planData?.endDate as any)),
      );
      setValue(
        "floorIds",
        Array.isArray(planData?.floorIds) ? planData?.floorIds : [],
      );
      setValue(
        "zoneIds",
        Array.isArray(planData?.zoneIds) ? planData?.zoneIds : [],
      );
    }
  }, [isEditMode, planData]);

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

  const fetchZoneData = async (floorIds: string[]) => {
    if (!Array.isArray(floorIds) || floorIds.length === 0) {
      setZoneList([]);
      return;
    }

    try {
      const response: any = await GetAllZonesByFloorIdService(floorIds);

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
      if (!isEditMode) {
        setValue("zoneIds", []);
      }
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

  const addPlan: SubmitHandler<IPlanProps> = async (data) => {
    const start_Date = data.startDate
      ? convertToUTC(dayjs(data.startDate).format("YYYY-MM-DDTHH:mm:ss"))
      : null;

    const end_Date = data.endDate
      ? convertToUTC(dayjs(data.endDate).format("YYYY-MM-DDTHH:mm:ss"))
      : null;

    const plan_Data = {
      planName: data.planName,
      duration: data.duration,
      startDate: start_Date,
      endDate: end_Date,
      deviceIds: data.deviceIds,
      floorIds: data.floorIds,
      zoneIds: data.zoneIds,
      ...(isEditMode && planData?.id && { id: planData.id }),
    };

    var result: any = await SaveMaintenancePlanService(plan_Data as IPlanProps);

    if (result && result.isSuccess) {
      onClose();
      refreshData();
      reset();
    }
  };

  return (
    <div className="cmn-pop-form">
      <div className="cmn-pop-form-wrapper">
        <Box
          component="form"
          onSubmit={handleSubmit(addPlan)}
          noValidate
          sx={{ width: "100%" }}
        >
          <div className="cmn-pop-form-inner">
            <CustomTextField
              name="planName"
              label={
                <span>
                  {t("Maintanace_Plan.Maintanace_Plan_Grid.Plan_Name")}{" "}
                  <span className="star-error">*</span>
                </span>
              }
              control={control}
              rules={{
                required: t(
                  "Maintanace_Plan.Maintanance_Plan_Drawer.Plan_Name_Required",
                ),
                maxLength: {
                  value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                  message: t(
                    "Maintanace_Plan.Maintanance_Plan_Drawer.Plan_Name_Max_Length_validation",
                    { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH },
                  ),
                },
              }}
              placeholder={t(
                "Maintanace_Plan.Maintanance_Plan_Drawer.Plan_Name_Placeholder",
              )}
              required
              fullWidth
            />

            <CustomTextField
              name="duration"
              label={
                <span>
                  {t(
                    "Maintanace_Plan.Maintanance_Plan_Drawer.Duration_In_Month",
                  )}{" "}
                  <span className="star-error">*</span>
                </span>
              }
              control={control}
              rules={{
                required: t(
                  "Maintanace_Plan.Maintanance_Plan_Drawer.Duration_Required",
                ),
                maxLength: {
                  value: COMMON_CONSTANTS.MAX_DIGIT_LENGTH,
                  message: t(
                    "Maintanace_Plan.Maintanance_Plan_Drawer.Duration_Max_Length_validation",
                    { max: COMMON_CONSTANTS.MAX_DIGIT_LENGTH },
                  ),
                },
                pattern: {
                  value: /^[0-9]+$/,
                  message: t(
                    "Maintanace_Plan.Maintanance_Plan_Drawer.Duration_Strong_Validation",
                  ),
                },
              }}
              placeholder={t(
                "Maintanace_Plan.Maintanance_Plan_Drawer.Duration_Placeholder",
              )}
              required
              fullWidth
              type="text"
              inputProps={{ inputMode: "numeric" }}
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

            <LocalizationProvider dateAdapter={AdapterDayjs}>
              <Box className="star-end-date-plan">
                <FormLabel>
                  <span>
                    {/* {t("General_Settings.Report_Scheduler.Select_Date")}{" "} */}
                    {t("Maintanace_Plan.Maintanace_Plan_Grid.Start_Date")}
                    <span className="star-error">*</span>
                  </span>
                </FormLabel>

                <Controller
                  name="startDate"
                  control={control}
                  rules={{
                    // required: "Please select start date",
                    validate: (value) => {
                      if (!value)
                        return t(
                          "General_Settings.Report_Scheduler.Validation.Date_Requiered",
                        );
                      if (value.isBefore(dayjs(), "day")) {
                        return t(
                          "General_Settings.Report_Scheduler.Validation.Date_Strong_Validation",
                        );
                      }
                      return true;
                    },
                  }}
                  render={({ field, fieldState }) => (
                    <DatePicker
                      value={field.value}
                      onChange={(date) => field.onChange(date)}
                      slotProps={{
                        textField: {
                          error: !!fieldState.error,
                          helperText: fieldState.error?.message,
                        },
                      }}
                      format="DD-MM-YYYY"
                    />
                  )}
                />
              </Box>
              <Box className="star-end-date-plan">
                <FormLabel>
                  <span>
                    {/* {t("General_Settings.Report_Scheduler.Select_Date")}{" "} */}
                    {t("Maintanace_Plan.Maintanace_Plan_Grid.End_Date")}
                    <span className="star-error">*</span>
                  </span>
                </FormLabel>

                <Controller
                  name="endDate"
                  control={control}
                  rules={{
                    // required: "Please select start date",
                    validate: (value) => {
                      if (!value)
                        return t(
                          "Maintanace_Plan.Maintanance_Plan_Drawer.EndDate_Requiered",
                        );

                      const startDate = getValues("startDate");

                      if (startDate && value.isBefore(startDate, "day")) {
                        return t(
                          "Maintanace_Plan.Maintanance_Plan_Drawer.EndDate_Less_Than_StartDate",
                        );
                      }

                      if (value.isBefore(dayjs(), "day")) {
                        return t(
                          "Maintanace_Plan.Maintanance_Plan_Drawer.EndDate_Strong_Validation",
                        );
                      }
                      return true;
                    },
                  }}
                  render={({ field, fieldState }) => (
                    <DatePicker
                      value={field.value}
                      onChange={(date) => field.onChange(date)}
                      slotProps={{
                        textField: {
                          error: !!fieldState.error,
                          helperText: fieldState.error?.message,
                        },
                      }}
                      format="DD-MM-YYYY"
                    />
                  )}
                />
              </Box>
            </LocalizationProvider>

            <CustomMultiSelect
              name="floorIds"
              variant="filled"
              control={control}
              label={t("Maintanace_Plan.Maintanace_Plan_Grid.Floor")}
              options={floorList}
              placeholder={t("Maintanace_Plan.Maintanace_Plan_Grid.Select_Floor")}
            />
            <CustomMultiSelect
              name="zoneIds"
              variant="filled"
              control={control}
              label={t("Maintanace_Plan.Maintanace_Plan_Grid.Zone")}
              options={zoneList}
              placeholder={t("Maintanace_Plan.Maintanace_Plan_Grid.Select_Zone")}
            />
            <FormLabel>
              <span>
                {t("Maintanace_Plan.Maintanace_Plan_Grid.Device")}
                <span className="star-error">*</span>
              </span>
            </FormLabel>
            <Controller
              name="deviceIds"
              control={control}
              rules={{
                required: t(
                  "Maintanace_Plan.Maintanance_Plan_Drawer.Device_Required",
                ),
              }}
              render={({ field, fieldState }) => {
                const allDeviceIds = deviceList.map((d) => d.id);
                const isAllSelected =
                  allDeviceIds.length > 0 &&
                  field.value?.length === allDeviceIds.length;

                const options = [SELECT_ALL_OPTION, ...deviceList];

                const selectedOptions = isAllSelected
                  ? options
                  : options.filter((opt) =>
                      opt.id === "ALL" ? false : field.value?.includes(opt.id),
                    );

                return (
                  <Autocomplete
                    multiple
                    disableCloseOnSelect
                    options={options}
                    // options={deviceList}
                    value={selectedOptions}
                    // value={deviceList.filter((d) =>
                    //   field.value?.includes(d.id)
                    // )}
                    onChange={(_, selectedOptions, reason, details) => {
                      const clickedOption = details?.option;

                      if (clickedOption?.id === "ALL") {
                        field.onChange(isAllSelected ? [] : allDeviceIds);
                        return;
                      }

                      field.onChange(
                        selectedOptions
                          .filter((opt) => opt.id !== "ALL")
                          .map((opt) => opt.id),
                      );
                    }}
                    // onChange={(_, selectedOptions) => {
                    //   field.onChange(selectedOptions.map((opt) => opt.id));
                    // }}
                    getOptionLabel={(option) => option.deviceName || ""}
                    // style={{ width: 500 }}
                    isOptionEqualToValue={(option, value) =>
                      option.id === value.id
                    }
                    renderInput={(params) => (
                      <TextField
                        {...params}
                        placeholder={t(
                          "Maintanace_Plan.Maintanance_Plan_Drawer.Device_Placeholder",
                        )}
                        // placeholder={
                        //   field.value && field.value.length > 0
                        //     ? ""
                        //     : t(
                        //         "Maintanace_Plan.Maintanance_Plan_Drawer.Device_Placeholder"
                        //       )
                        // }
                        error={!!fieldState.error}
                        // helperText={fieldState.error?.message}
                        helperText={
                          <>
                            {fieldState.error?.message && (
                              <div>{fieldState.error.message}</div>
                            )}
                            {deviceIds.length > 0 && (
                              <div className="device-color">
                                <strong>{deviceIds.length}</strong> devices are
                                selected for this maintenance plan.
                              </div>
                            )}
                          </>
                        }
                      />
                    )}
                    renderOption={(props, option, { selected }) => {
                      const { key, ...optionProps } = props;
                      const checked =
                        option.id === "ALL" ? isAllSelected : selected;
                      return (
                        <li key={key} {...optionProps}>
                          <Checkbox
                            icon={<CheckBoxOutlineBlankIcon fontSize="small" />}
                            checkedIcon={<CheckBoxIcon fontSize="small" />}
                            style={{ marginRight: 8 }}
                            // checked={selected}
                            checked={checked}
                          />
                          {option.deviceName}
                        </li>
                      );
                    }}
                  />
                );
              }}
            />

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

export { AddEditPlan };
