import React, { useEffect, useState } from "react";
import { Box, FormLabel, TextField } from "@mui/material";
import { CustomTextField } from "../../components/Reusable/CustomTextField";
import { CustomButton } from "../../components/Reusable/CustomButton";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import { LocalizationProvider, DatePicker } from "@mui/x-date-pickers";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs, { Dayjs } from "dayjs";
import { convertToUTC } from "../../utils/convertToUTC";
import {
  planAddEditFormProps,
  IDeviceList,
  addScheduleform,
  addScheduleReq,
} from "../../interfaces/IMaintenance";
import { useTranslation } from "react-i18next";
import Autocomplete from "@mui/material/Autocomplete";
import {
  SaveMaintenanceScheduleService,
  GetAllDeviceForMaintenancePlanService,
} from "../../services/maintenanceService";
import { ILookup } from "../../interfaces/ILookup";
import {
  GetAllFloorsListService,
  GetAllZonesByFloorIdService,
} from "../../services/dashboardService";
import { CustomSelect } from "../../components";

const AddSchedule: React.FC<planAddEditFormProps> = ({
  onClose,
  refreshData,
}) => {
  const { t } = useTranslation();
  const [deviceList, setDeviceList] = useState<IDeviceList[]>([]);
  const [allDevices, setAllDevices] = useState<IDeviceList[]>([]);
  const [floorList, setFloorList] = useState<ILookup[]>([]);
  const [zoneList, setZoneList] = useState<ILookup[]>([]);

  const { control, handleSubmit, watch, getValues, setValue } = useForm<addScheduleform>({
    defaultValues: {
      startDate: dayjs(),
      notes: "",
      deviceId: "",
      floorId: "",
      zoneId: "",
    },
  });

  const floorId = watch("floorId") ?? "";
  const zoneId = watch("zoneId") ?? "";

  useEffect(() => {
    fetchDeviceList();
    fetchFloorData();
  }, []);

  useEffect(() => {
    fetchZoneData(floorId);
    setValue("zoneId", ""); 
    setValue("deviceId", "");
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

    const selected = getValues("deviceId");
    if (selected && !filtered.some((d) => d.id === selected)) {
      setValue("deviceId", "");
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

  const addSchedule: SubmitHandler<addScheduleform> = async (data) => {
    const mergeDateWithCurrentTime = (date: string | Date | Dayjs) =>
      dayjs(date)
        .set("hour", dayjs().hour())
        .set("minute", dayjs().minute())
        .set("second", dayjs().second());

    const start_Date = data.startDate
      ? convertToUTC(
          mergeDateWithCurrentTime(data.startDate).format(
            "YYYY-MM-DDTHH:mm:ss",
          ),
        )
      : null;

    const ScheduleData = {
      dueDate: start_Date,
      statusHistory: [{ notes: data.notes }],
      deviceId: data.deviceId,
      floorId:data.floorId,
      zoneId:data.zoneId
    };

    var result: any = await SaveMaintenanceScheduleService(
      ScheduleData as addScheduleReq,
    );

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
          onSubmit={handleSubmit(addSchedule)}
          noValidate
          sx={{ width: "100%" }}
        >
          <div className="cmn-pop-form-inner">
              <CustomSelect
                name="floorId"
                label={t("Maintanace_Plan.Maintanace_Plan_Grid.Floor")}
                variant="filled"
                control={control}
                options={floorList}
                placeholder={t("Maintanace_Plan.Maintanace_Plan_Grid.Select_Floor")}
              />
              <CustomSelect
                name="zoneId"
                variant="filled"
                control={control}
                options={zoneList}
                label={t("Maintanace_Plan.Maintanace_Plan_Grid.Zone")}
                placeholder={t("Maintanace_Plan.Maintanace_Plan_Grid.Select_Zone")}
              />
              <FormLabel>
                <span>
                  {t("Maintenance_Schedule.Add_Schedule_Drawer.Device")}
                  <span className="star-error">*</span>
                </span>
              </FormLabel>
              <Controller
                name="deviceId"
                control={control}
                rules={{
                  required: t(
                    "Maintenance_Schedule.Add_Schedule_Drawer.Device_Required",
                  ),
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
                        placeholder={t(
                          "Maintenance_Schedule.Add_Schedule_Drawer.Device_Placeholder",
                        )}
                        error={!!fieldState.error}
                        helperText={fieldState.error?.message}
                      />
                    )}
                  />
                )}
              />
            <LocalizationProvider dateAdapter={AdapterDayjs}>
              <Box className="star-end-date-plan">
                <FormLabel>
                  <span>
                    {/* {t("General_Settings.Report_Scheduler.Select_Date")}{" "} */}
                    {t("Maintenance_Schedule.Add_Schedule_Drawer.Due_Date")}
                    <span className="star-error">*</span>
                  </span>
                </FormLabel>

                <Controller
                  name="startDate"
                  control={control}
                  rules={{
                    required: t(
                      "Maintenance_Schedule.Add_Schedule_Drawer.Due_date_Required",
                    ),
                    validate: (value) => {
                      if (value.isBefore(dayjs(), "day")) {
                        return t(
                          "Maintenance_Schedule.Add_Schedule_Drawer.Due_date_Strong_Validation",
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
                  "Maintenance_Schedule.Add_Schedule_Drawer.Notes_Required",
                ),
              }}
              placeholder={t(
                "Maintenance_Schedule.Add_Schedule_Drawer.Notes_Placeholder",
              )}
              required
              fullWidth
            />
            <CustomButton
              className="common-btn-design"
              fullWidth
              customStyles={{ mt: 2 }}
            >
              {t("Add_btn")}
            </CustomButton>
          </div>
        </Box>
      </div>
    </div>
  );
};

export { AddSchedule };
