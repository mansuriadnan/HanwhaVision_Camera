import React, { useEffect, useState } from "react";
import { Box, FormControlLabel, Checkbox, FormLabel } from "@mui/material";
import { OwnerInputProps, OwnerAddEditFormProps } from "../../interfaces/IANPR";
import { CustomTextField } from "../../components/Reusable/CustomTextField";
import { CustomButton } from "../../components/Reusable/CustomButton";
import { SaveVehicleOwnerService } from "../../services/anprService";
import { COMMON_CONSTANTS, REGEX } from "../../utils/constants";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import { CustomMultiSelect, CustomSelect } from "../../components";
import { DatePicker, LocalizationProvider, TimePicker } from "@mui/x-date-pickers";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs from "dayjs";
import { convertToUTC } from "../../utils/convertToUTC";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { GetDevicesAsyncService } from "../../services/maintenanceService";
import { IDeviceList } from "../../interfaces/IMaintenance";
import { ILookup } from "../../interfaces/ILookup";
import { useTranslation } from "react-i18next";
import { useTimeFormatContext } from "../../context/TimeFormatContext";

const AddEditOwner: React.FC<OwnerAddEditFormProps> = ({
  onClose,
  ownerData,
  refreshData,
}) => {
  const [deviceList, setDeviceList] = useState<ILookup[]>([]);
  const registrationTypeArray = [
    {
      title: "Permanent",
      id: "permanent",
    },
    {
      title: "Visitor",
      id: "visitor",
    },
    {
      title: "Tenant",
      id: "tenant",
    },
  ];

  const { t } = useTranslation();
  const { timeFormat } = useTimeFormatContext();

  const isEditMode = ownerData !== null && ownerData !== undefined;

  const {
    control,
    setValue,
    handleSubmit,
    watch,
    // formState: { errors },
    formState: { isDirty },
  } = useForm<OwnerInputProps>({
    defaultValues: {
      registrationType: "",
      ownerName: "",
      building: "",
      buildingUnit: "",
      email: "",
      contactNumber: "",
      allowedVehicle: 1,
      allowedFromTime: dayjs().startOf("day"),
      allowedToTime: dayjs().endOf("day"),
      allowedGates: [],
      enabledAlarmForOverstay: false,
      enabledAlarmFor24HStay: false,
      ownerValidTo:null,
    },
  });

  const registrationType = watch("registrationType");

  useEffect(() => {
    fetchDeviceList();
  }, []);

  const fetchDeviceList = async () => {
    try {
      let suburl: string;
      suburl = `?deviceType=ANPR`;
      const deviceData: any = await GetDevicesAsyncService(suburl);
      if (deviceData && deviceData?.length && deviceData?.length > 0) {
        const tempList = deviceData?.map((item: IDeviceList) => ({
          title: item.deviceName,
          id: item.id,
        }));

        setDeviceList(tempList);
      } else {
        setDeviceList([]);
      }
    } catch (err: any) {
      console.error("Error fetching device data:", err);
    } finally {
      // setLoading(false);
    }
  };

  useEffect(() => {
    if (isEditMode && ownerData) {
      setValue("registrationType", ownerData?.registrationType);
      setValue(
        "allowedGates",
        Array.isArray(ownerData?.allowedGates) ? ownerData?.allowedGates : [],
      );
      setValue("ownerName", ownerData?.ownerName || "");
      setValue("building", ownerData?.building || "");
      setValue("buildingUnit", ownerData?.buildingUnit || "");
      setValue("email", ownerData?.email || "");
      setValue("contactNumber", ownerData?.contactNumber || "");
      setValue("allowedVehicle", ownerData?.allowedVehicle || 1);
      setValue(
        "enabledAlarmForOverstay",
        ownerData?.enabledAlarmForOverstay || false,
      );
      setValue(
        "enabledAlarmFor24HStay",
        ownerData?.enabledAlarmFor24HStay || false,
      );
      setValue(
        "allowedFromTime",
        dayjs(
          formatDateToConfiguredTimezone(ownerData?.allowedFromTime as any),
        ),
      );
      setValue(
        "allowedToTime",
        dayjs(formatDateToConfiguredTimezone(ownerData?.allowedToTime as any)),
      );
      setValue(
        "ownerValidTo",
        dayjs(formatDateToConfiguredTimezone(ownerData?.ownerValidTo as any)),
      );
    }
  }, [isEditMode, ownerData]);

  const addVehicleOwner: SubmitHandler<OwnerInputProps> = async (data) => {
    let currentDate = dayjs().format("YYYY-MM-DD");

    if (isEditMode && ownerData?.allowedFromTime) {
      // Extract only the date part (YYYY-MM-DD) from the existing record
      currentDate = dayjs(ownerData.allowedFromTime).format("YYYY-MM-DD");
    }

    const formattedStartTime =
      data.allowedFromTime && dayjs(data.allowedFromTime).isValid()
        ? dayjs(`${currentDate} ${data.allowedFromTime.format("HH:mm")}`)
        : null;

    const formattedEndTime =
      data.allowedToTime && dayjs(data.allowedToTime).isValid()
        ? dayjs(`${currentDate} ${data.allowedToTime.format("HH:mm")}`)
        : null;

    const startTime = formattedStartTime
      ? convertToUTC(formattedStartTime.format("YYYY-MM-DDTHH:mm:ss"))
      : null;
    const endTime = formattedEndTime
      ? convertToUTC(formattedEndTime.format("YYYY-MM-DDTHH:mm:ss"))
      : null;

    const owner_ValidTo = data.ownerValidTo
      ? convertToUTC(dayjs(data.ownerValidTo).format("YYYY-MM-DDTHH:mm:ss"))
      : null;

    const vehicleOwnerData = {
      registrationType: data.registrationType,
      ownerName: data.ownerName,
      building: data.building,
      buildingUnit: data.buildingUnit,
      email: data.email,
      contactNumber: data.contactNumber,
      allowedVehicle: data.allowedVehicle,
      allowedFromTime: data.registrationType === "visitor" ? null : startTime,
      allowedToTime: data.registrationType === "visitor" ? null : endTime,
      allowedGates: data.allowedGates,
      enabledAlarmForOverstay: data.enabledAlarmForOverstay,
      enabledAlarmFor24HStay: data.enabledAlarmFor24HStay,
      ownerValidTo:owner_ValidTo,
      ...(isEditMode && ownerData?.id && { id: ownerData.id }),
    };

    var result: any = await SaveVehicleOwnerService(
      vehicleOwnerData as OwnerInputProps,
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
          onSubmit={handleSubmit(addVehicleOwner)}
          noValidate
          sx={{ width: "100%" }}
        >
          <div className="cmn-pop-form-inner">
            <CustomSelect
              name="registrationType"
              variant="filled"
              control={control}
              label={
                <span>
                  {t("ANPR_Screen.Owner.Registration_Type")}
                  <span className="star-error">*</span>
                </span>
              }
              rules={{
                required: t("ANPR_Screen.Owner.Registration_type_required"),
              }}
              options={registrationTypeArray}
              placeholder={t("ANPR_Screen.Owner.Registration_type_placeholder")}
              disabled={isEditMode}
            />
            <CustomTextField
              name="ownerName"
              label={
                <span>
                   {t("ANPR_Screen.Owner.Owner_Name")}<span className="star-error">*</span>
                </span>
              }
              control={control}
              rules={{
                required: t("ANPR_Screen.Owner.Owner_name_required"),
                pattern: {
                  value: REGEX.Name_Regex,
                  message: t("ANPR_Screen.Owner.Owner_Validation"),
                },
                // maxLength: {
                //   value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                //   message: t("ANPR_Screen.Owner.Owner_Strong_Validation", { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                // },
              }}
              placeholder={t("ANPR_Screen.Owner.Owner_name_placeholder")}
              required
              fullWidth
            />
            <Box display="flex" gap={2}>
              <CustomTextField
                name="building"
                label={
                  <span>
                    {t("ANPR_Screen.Owner.Building")} <span className="star-error">*</span>
                  </span>
                }
                control={control}
                rules={{
                  required: t("ANPR_Screen.Owner.Building_required"),
                  pattern: {
                    value: REGEX.Bulding_Regex,
                    message: t("ANPR_Screen.Owner.Building_name_validation"),
                  },
                }}
                placeholder={t("ANPR_Screen.Owner.Building_placeholder")}
                required
                fullWidth
              />

              <CustomTextField
                name="buildingUnit"
                label={
                  <span>
                    {t("ANPR_Screen.Owner.Building_Unit")} <span className="star-error">*</span>
                  </span>
                }
                control={control}
                rules={{
                  required: t("ANPR_Screen.Owner.Building_unit_required"),
                  pattern: {
                    value: REGEX.Bulding_Regex,
                    message: t("ANPR_Screen.Owner.Building_unit_validation"),
                  },
                }}
                placeholder={t("ANPR_Screen.Owner.Building_unit_placeholder")}
                required
                fullWidth
              />
            </Box>
            <CustomTextField
              name="email"
              label={
                <span>
                  {t("ANPR_Screen.Owner.Email")} <span className="star-error">*</span>
                </span>
              }
              control={control}
              rules={{
                required: t("ANPR_Screen.Owner.Email_required"),
                pattern: {
                  value: REGEX.Email_Regex,
                  message: t("ANPR_Screen.Owner.Email_validation"),
                },
              }}
              placeholder= {t("ANPR_Screen.Owner.Email_placeholder")}
              required
              fullWidth
            />

            {/* <CustomTextField
              name="contactNumber"
              label={
                <span>
                  {t("ANPR_Screen.Owner.Contact_Number")} <span className="star-error">*</span>
                </span>
              }
              control={control}
              rules={{
                required: t("ANPR_Screen.Owner.Contact_number_required"),
                pattern: {
                  value: REGEX.Mobile_Regex,
                  message: t("ANPR_Screen.Owner.Contact_number_validation"),
                },
              }}
              placeholder={t("ANPR_Screen.Owner.Contact_number_placeholder")}
              required
              fullWidth
              type="number"
              onKeyDown={(e) => {
                if (['e', 'E', '.', '-'].includes(e.key)) {
                  e.preventDefault();
                }
              }}
            /> */}

            <CustomTextField
              name="contactNumber"
              label={
                <span>
                  {t("ANPR_Screen.Owner.Contact_Number")}{" "}
                  <span className="star-error">*</span>
                </span>
              }
              control={control}
              rules={{
                required: t("ANPR_Screen.Owner.Contact_number_required"),
                pattern: {
                  value: REGEX.Mobile_Regex_All_Country, 
                  message: t("ANPR_Screen.Owner.Contact_number_validation"),
                },
              }}
              placeholder={t("ANPR_Screen.Owner.Contact_number_placeholder")}
              required
              fullWidth
              type="text"  //IMPORTANT (not number) - Q221259 to solve this ticket
              inputProps={{  maxLength: 16 }}
              // Allow only digits + "+" (only at start)
              onKeyDown={(e) => {
                const value = e.target.value;
                // Allow control shortcuts (copy, paste, select all, etc.)
                if (e.ctrlKey || e.metaKey) {
                  return;
                }
                // Allow control keys
                if (
                  ["Backspace", "Tab", "ArrowLeft", "ArrowRight", "Delete", "Home", "End"].includes(e.key)
                ) {
                  return;
                }
                // Allow "+" only at first position
                if (e.key === "+") {
                  if (value.length === 0 && !value.includes("+")) {
                    return;
                  } else {
                    e.preventDefault();
                    return;
                  }
                }
                // Allow digits only
                if (!/[0-9]/.test(e.key)) {
                  e.preventDefault();
                }
              }}

              //Clean pasted / typed input
              onInput={(e) => {
                let value = e.target.value;

                // Keep "+" only at start, remove other non-digits
                value = value.replace(/(?!^\+)\D/g, "");

                // Ensure only one "+"
                if (value.indexOf("+") > 0) {
                  value = value.replace(/\+/g, "");
                }

                e.target.value = value;
              }}
            />

            <CustomTextField
              name="allowedVehicle"
              label={
                <span>
                  {t("ANPR_Screen.Owner.Allowed_Vehicle")} <span className="star-error">*</span>
                </span>
              }
              control={control}
              rules={{
                required: t("ANPR_Screen.Owner.Vehicle_count_required"),
                pattern: {
                  value: REGEX.DigitsOnly_Regex,
                  message: t("ANPR_Screen.Owner.Vehicle_count_validation"),
                },
              }}
              placeholder={t("ANPR_Screen.Owner.Vehicle_count_placeholder")}
              defaultValue={"1"}
              required
              fullWidth
              type="number"
              disabled={registrationType === "visitor"}
              onKeyDown={(e) => {
                if (['e', 'E', '+', '-'].includes(e.key)) {
                  e.preventDefault();
                }
              }}
            />

            {registrationType !== "visitor" && (
              <Box display="flex" gap={2}>
                <LocalizationProvider dateAdapter={AdapterDayjs}>
                  <Box>
                    <FormLabel><span>{t("ANPR_Screen.Owner.Allowed_From_Time")}</span></FormLabel>
                    <Controller
                      name="allowedFromTime"
                      control={control}
                      render={({ field, fieldState }) => (
                        <TimePicker
                        ampm={timeFormat === "24h" ? false : true} 
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

                  <Box>
                     <FormLabel><span>{t("ANPR_Screen.Owner.Allowed_To_Time")}</span></FormLabel>
                    <Controller
                      name="allowedToTime"
                      control={control}
                      render={({ field, fieldState }) => (
                        <TimePicker
                        ampm={timeFormat === "24h" ? false : true} 
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
              </Box>
            )}

            <CustomMultiSelect
              name="allowedGates"
              control={control}
              label={t("ANPR_Screen.Owner.Gates")}
              options={deviceList}
              placeholder={t("ANPR_Screen.Owner.Select_Gates")}
            />

            <Box display="flex" gap={2}>
              <Box
                display="flex"
                alignItems="center"
                gap={2}
                className="enable-checked"
              >
                <Controller
                  name="enabledAlarmForOverstay"
                  control={control}
                  render={({ field }) => (
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={field.value}
                          onChange={() => field.onChange(!field.value)}
                        />
                      }
                      label={t("ANPR_Screen.Owner.Enable_Alarm_For_Overstay")}
                    />
                  )}
                />
              </Box>
              <Box
                display="flex"
                alignItems="center"
                gap={2}
                className="enable-checked"
              >
                <Controller
                  name="enabledAlarmFor24HStay"
                  control={control}
                  render={({ field }) => (
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={field.value}
                          onChange={() => field.onChange(!field.value)}
                        />
                      }
                      label={t("ANPR_Screen.Owner.Enable_Alarm_For_24H_Stay")}
                    />
                  )}
                />
              </Box>
            </Box>

            <LocalizationProvider dateAdapter={AdapterDayjs}>
              <Box className="star-end-date-plan">
                <FormLabel>
                  <span>
                    {t("ANPR_Screen.Owner.ValidTo")}
                    <span className="star-error">*</span>
                  </span>
                </FormLabel>

                <Controller
                  name="ownerValidTo"
                  control={control}
                  rules={{
                    validate: (value) => {
                      if (!value)
                        return t(
                          "ANPR_Screen.Owner.ValidTo_Requiered",
                        );
                      if (value.isBefore(dayjs(), "day")) {
                        return t(
                          "ANPR_Screen.Owner.ValidToDate_Strong_Validation",
                        );
                      }
                      return true;
                    },
                  }}
                  render={({ field, fieldState }) => (
                    <DatePicker
                      value={field.value}
                      onChange={(date) => field.onChange(date)}
                      disablePast
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

export { AddEditOwner };
