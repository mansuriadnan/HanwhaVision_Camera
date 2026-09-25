import React, { useEffect, useState } from "react";
import { Box, Button, ButtonGroup, Drawer, FormLabel, IconButton, Tooltip, Typography } from "@mui/material";
import {
  IVehicleList,
  IGetAllVehicleByOwnerRequestProps,
  vehicleProps,
  countryres,
} from "../../interfaces/IANPR";
import { CustomTextField } from "../../components/Reusable/CustomTextField";
import { CustomButton } from "../../components/Reusable/CustomButton";
import {
  DeleteANPRVehicleService,
  GetAllCountryListService,
  GetAllVehicleByOwnerService,
  SaveVehicleService,
  VehicleSampleExcelDownloadService,
} from "../../services/anprService";
import { REGEX } from "../../utils/constants";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import { CommonDialog, CustomSelect } from "../../components";
import { LocalizationProvider, DateTimePicker } from "@mui/x-date-pickers";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs from "dayjs";
import { convertToUTC } from "../../utils/convertToUTC";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { DataGrid, GridCloseIcon, GridColDef } from "@mui/x-data-grid";
import { formatDate } from "../../utils/dateUtils";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { useThemeContext } from "../../context/ThemeContext";
import { HasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { useNavigate } from "react-router-dom";
import AddIcon from "@mui/icons-material/Add";
import VerticalAlignBottomIcon from "@mui/icons-material/VerticalAlignBottom";
import { useTranslation } from "react-i18next";
import { BulkUploadOwner } from "./BulkUploadOwner";
import { BulkUploadVehicle } from "./BulkUploadVehicle";

const Vehicle: React.FC<vehicleProps> = ({ selectedOwner }) => {
  const [vehicleData, setVehicleData] = useState<IVehicleList[]>([]);
  const [countryList, setCountryList] = useState([]);
  const [selectedVehicleId, setSelectedVehicleId] = useState<string>("");
  const [openDeleteConfirm, setOpenDeleteConfirm] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);
  const plateCategoryList = [
    {
      title: "Private/Civil",
      id: "private/civil",
    },
  ];

  const { timeFormat } = useTimeFormatContext();
  const { theme, themeColor } = useThemeContext();
  const navigate = useNavigate();
  const { t } = useTranslation();

  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
        ? `${themeColor}/dark-theme/`
        : `${themeColor}/`;

  const ViewAuditLogs = async (collectionId: string) => {
    navigate("/audit", { state: { id: collectionId, collectionName: "ANPRvehicle" } });
  }

  const {
    control,
    setValue,
    handleSubmit,
    reset,
    getValues,
    // formState: { errors },
    formState: { isDirty },
  } = useForm<IVehicleList>({
    defaultValues: {
      countryId: "",
      countryName: "",
      state: "",
      series: "",
      vehicleNumber: 0,
      plateCode: "",
      plateCategory: "",
      make: "",
      model: "",
      color: "",
      visitorValidFrom: dayjs(),
      visitorValidTo: dayjs(),
    },
  });

  useEffect(() => {
    getAllCountryListData();
    featchAllVehicleByOwner();
  }, []);

  const getAllCountryListData = async () => {
    try {
      const countryData: any = await GetAllCountryListService();
      if (countryData && countryData?.length && countryData?.length > 0) {
        const tempcountryList = countryData.map((item: countryres) => ({
          title: `${item.countryCode} - ${item.countryName}`,
          id: item.id,
        }));
        setCountryList(tempcountryList);
      } else {
        setCountryList([]);
      }
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };

  const featchAllVehicleByOwner = async () => {
    try {
      let request: IGetAllVehicleByOwnerRequestProps = {
        searchText: "",
        vehicleOwnerId: selectedOwner?.id ?? "",
      };
      const vehicleData: any = await GetAllVehicleByOwnerService(request);
      if (vehicleData && vehicleData.isSuccess) {
        setVehicleData(vehicleData?.data);
      } else {
        setVehicleData([]);
      }
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };

  const addVehicle: SubmitHandler<IVehicleList> = async (data) => {

    const validFrom = data.visitorValidFrom && dayjs(data.visitorValidFrom).isValid()
      ? convertToUTC(data.visitorValidFrom.format("YYYY-MM-DDTHH:mm:ss"))
      : null;
    const validTo = data.visitorValidTo && dayjs(data.visitorValidTo).isValid()
      ? convertToUTC(data.visitorValidTo.format("YYYY-MM-DDTHH:mm:ss"))
      : null;

    const vehicleData = {
      country: data.countryId,
      state: data.state,
      series: data.series,
      vehicleNumber: data.vehicleNumber,
      plateCode: data.plateCode,
      plateCategory: data.plateCategory,
      make: data.make,
      model: data.model,
      color: data.color,
      visitorValidFrom: selectedOwner?.registrationType === "permanent" ? null : validFrom,
      visitorValidTo: selectedOwner?.registrationType === "permanent" ? null : validTo,
      vehicleOwnerId: selectedOwner?.id,
      ...(isEditMode && selectedVehicleId && { id: selectedVehicleId }),
    };

    var result: any = await SaveVehicleService(vehicleData as IVehicleList);
    if (result && result.isSuccess) {
      reset();
      featchAllVehicleByOwner();
      setIsEditMode(false);
    }
  };

  const columns: GridColDef[] = [
    {
      field: "countryName",
      headerName: t("ANPR_Screen.Vehicle.Country"),
      width: 200,
      filterable: false,
    },
    {
      field: "state",
      headerName: t("ANPR_Screen.Vehicle.State"),
      width: 150,
      filterable: false,
    },
    // {
    //   field: "series",
    //   headerName: "Series",
    //   // width: 200,
    //   filterable: false,
    // },
    // {
    //   field: "vehicleNumber",
    //   headerName: "Vehicle Number",
    //   // width: 200,
    //   filterable: false,
    // },
    {
      field: "plateCode",
      headerName: t("ANPR_Screen.Vehicle.Plate_Code"),
      width: 170,
      filterable: false,
    },
    {
      field: "plateCategory",
      headerName: t("ANPR_Screen.Vehicle.Plate_Category"),
      width: 170,
      filterable: false,
    },
    {
      field: "make",
      headerName: t("ANPR_Screen.Vehicle.Make"),
      width: 150,
      filterable: false,
    },
    {
      field: "model",
      headerName: t("ANPR_Screen.Vehicle.Model"),
      width: 150,
      filterable: false,
    },
    {
      field: "color",
      headerName: t("ANPR_Screen.Vehicle.Color"),
      width: 150,
      filterable: false,
    },
    {
      field: "visitorValidFrom",
      headerName: t("ANPR_Screen.Vehicle.Visitor_Valid_From"),
      width: 200,
      renderCell: (params) => {
        const from = formatDateToConfiguredTimezone(
          params.row.visitorValidFrom
        );
        const fromFormatted = formatDate(from, timeFormat);
        return <Box>{fromFormatted}</Box>;
      },
    },
    {
      field: "visitorValidTo",
      headerName: t("ANPR_Screen.Vehicle.Visitor_Valid_To"),
      width: 200,
      renderCell: (params) => {
        const from = formatDateToConfiguredTimezone(params.row.visitorValidTo);
        const fromFormatted = formatDate(from, timeFormat);
        return <Box>{fromFormatted}</Box>;
      },
    },

    {
      field: "actions",
      headerName: t("ANPR_Screen.Vehicle.Actions"),
      width: 150,
      sortable: false,
      filterable: false,
      renderCell: (params) => (
        <Box>
          {HasPermission(LABELS.Add_or_Update_Vehicle_Details) && (
            <Tooltip title={t("ANPR_Screen.Vehicle.Edit_Vehicle")}>
              <IconButton onClick={() => handleEdit(params.row)}>
                <img
                  src={
                    theme === "light"
                      ? "/images/edit.svg"
                      : "/images/dark-theme/edit.svg"
                  }
                  alt="Edit Vehicle"
                  width="20"
                  height="20"
                />
              </IconButton>
            </Tooltip>
          )}
          {HasPermission(LABELS.Delete_an_Existing_Vehicle) && (
            <Tooltip title={t("ANPR_Screen.Vehicle.Delete_Vehicle")}>
              <IconButton onClick={() => handleDelete(params.id as string)}>
                <img
                  src={"/images/user-action-delete.svg"}
                  alt="Delete Vehicle"
                  width={20}
                  height={20}
                />
              </IconButton>
            </Tooltip>
          )}
          {(HasPermission(LABELS.AuditLogMaster) &&
            HasPermission(LABELS.ViewAuditLogsANPRVehicle)) && (
              <Tooltip title={t("ANPR_Screen.Vehicle.View_Audit_logs")}>
                <IconButton onClick={() => ViewAuditLogs(params.id as string)}>
                  <img
                    src={
                      theme === "light"
                        ? "/images/audit_history.png"
                        : "/images/dark-theme/audit_history.png"
                    }
                    alt="History Icon"
                    width={20}
                    height={20}
                  />
                </IconButton>
              </Tooltip>
            )}
        </Box>
      ),
    },
  ];

  const CustomNoRowsOverlay = () => (
    <Box className="no-data-douns"
      sx={{
        height: "calc(100vh - 658px)"
      }}
    >
      <Box sx={{ width: 200, justifyItems: "center", flex: 1, }}>
        <img src={`/images/${themeColorPath}noData.gif`} alt="Animated GIF" width="100" height="100" />
        <Typography
          sx={{ FontWeight: 600, fontSize: 24, color: "#090909" }}
        >
          {t("No_data_found")}
        </Typography>
        {/* <Typography
            sx={{ FontWeight: 400, fontSize: 12, color: "#212121" }}
          >
           No data available to display Users. Please add a new user by clicking the <strong>Add New User</strong> button.
          </Typography> */}
      </Box>
    </Box>
  );

  const handleEdit = (vehicle: IVehicleList) => {
    setIsEditMode(true);
    setSelectedVehicleId(vehicle.id);
    setValue("countryId", vehicle?.countryId);
    setValue("state", vehicle?.state || "");
    setValue("series", vehicle?.series || "");
    setValue("vehicleNumber", vehicle?.vehicleNumber || 0);
    setValue("plateCode", vehicle?.plateCode || "");
    setValue("plateCategory", vehicle?.plateCategory || "");
    setValue("make", vehicle?.make || "");
    setValue("model", vehicle?.model || "");
    setValue("color", vehicle?.color || "");
    setValue(
      "visitorValidFrom",
      dayjs(formatDateToConfiguredTimezone(vehicle?.visitorValidFrom as any))
    );
    setValue(
      "visitorValidTo",
      dayjs(formatDateToConfiguredTimezone(vehicle?.visitorValidTo as any))
    );
  };

  const handleDelete = (vehicleid: string) => {
    setSelectedVehicleId(vehicleid);
    setOpenDeleteConfirm(true);
  };

  const finalDeleteVehicle = async (id: string) => {
    const param = {
      id: id,
    };
    try {
      const deleteData: any = await DeleteANPRVehicleService(param);
      if (deleteData.isSuccess) {
        setOpenDeleteConfirm(false);
        featchAllVehicleByOwner();
      }
    } catch (err: any) { }
  };



  return (
    <div className="cmn-pop-form vehicle-pop-main">
      <div className="cmn-pop-form-wrapper">
        {HasPermission(LABELS.Add_or_Update_Vehicle_Details) && (
          <Box
            component="form"
            onSubmit={handleSubmit(addVehicle)}
            noValidate
            sx={{ width: "100%" }}
          >
            <div className="cmn-pop-form-inner">
              <Box display="flex" gap={2} className="no-margin-here">
                <CustomSelect
                  name="countryId"
                  variant="filled"
                  control={control}
                  label={
                    <span style={{ marginTop: 0 }}>
                      {t("ANPR_Screen.Vehicle.Country")}<span className="star-error">*</span>
                    </span>
                  }
                  options={countryList}
                  placeholder={t("ANPR_Screen.Vehicle.Country_placeholder")}
                  rules={{
                    required: t("ANPR_Screen.Vehicle.Country_required")
                  }}
                />

                <CustomTextField
                  name="state"
                  label={
                    <span>
                      {t("ANPR_Screen.Vehicle.State")}<span className="star-error">*</span>
                    </span>
                  }
                  control={control}
                  rules={{
                    required: t("ANPR_Screen.Vehicle.State_required"),
                    pattern: {
                      value: REGEX.State_Regex,
                      message: t("ANPR_Screen.Vehicle.State_Validation"),
                    },
                  }}
                  placeholder={t("ANPR_Screen.Vehicle.State_placeholder")}
                  required
                  fullWidth
                />
              </Box>
              <Box display="flex" gap={2} className="no-margin-here">
                {/* <CustomTextField
                  name="series"
                  label={
                    <span>
                      Series <span className="star-error">*</span>
                    </span>
                  }
                  control={control}
                  rules={{
                    required: "Series is required.",
                    pattern: {
                      value: REGEX.Floor_Zone_Name_Regex,
                      message: "Enter a valid building name",
                    },
                  }}
                  placeholder="Enter series"
                  required
                  fullWidth
                />  */}

                {/* <CustomTextField
                  name="vehicleNumber"
                  label={
                    <span>
                      Vehicle Number <span className="star-error">*</span>
                    </span>
                  }
                  control={control}
                  rules={{
                    required: "Vehicle number is required.",
                    // pattern: {
                    //   value: REGEX.Email_Regex,
                    //   message: "Enter a valid email address",
                    // },
                  }}
                  placeholder="Enter vehicle number"
                  required
                  fullWidth
                  type="number"
                /> */}

                <CustomTextField
                  name="plateCode"
                  label={
                    <span>
                      {t("ANPR_Screen.Vehicle.Plate_Code")}<span className="star-error">*</span>
                    </span>
                  }
                  control={control}
                  rules={{
                    required: t("ANPR_Screen.Vehicle.Plate_code_required"),
                    pattern: {
                      value: REGEX.NumberPlate_Regex,
                      message: t("ANPR_Screen.Vehicle.plateCode_Validation"),
                    },
                  }}
                  placeholder={t("ANPR_Screen.Vehicle.Plate_code_placeholder")}
                  required
                  fullWidth
                />
                <CustomSelect
                  name="plateCategory"
                  variant="filled"
                  control={control}
                  label={
                    <span>
                      {t("ANPR_Screen.Vehicle.Plate_Category")}<span className="star-error">*</span>
                    </span>
                  }
                  options={plateCategoryList}
                  placeholder={t("ANPR_Screen.Vehicle.Plate_category_placeholder")}
                  rules={{
                    required: t("ANPR_Screen.Vehicle.Plate_category_required"),
                  }}
                />
              </Box>

              <Box display="flex" gap={2}>
                <CustomTextField
                  name="make"
                  label={
                    <span>
                      {t("ANPR_Screen.Vehicle.Make")}<span className="star-error">*</span>
                    </span>
                  }
                  control={control}
                  rules={{
                    required: t("ANPR_Screen.Vehicle.Make_required"),
                    pattern: {
                      value: REGEX.Name_Regex,
                      message: t("ANPR_Screen.Vehicle.Make_Validation"),
                    },
                  }}
                  placeholder={t("ANPR_Screen.Vehicle.Make_placeholder")}
                  required
                  fullWidth
                />

                <CustomTextField
                  name="model"
                  label={
                    <span>
                      {t("ANPR_Screen.Vehicle.Model")}<span className="star-error">*</span>
                    </span>
                  }
                  control={control}
                  rules={{
                    required: t("ANPR_Screen.Vehicle.Model_required"),
                    pattern: {
                      value: REGEX.Model_Regex,
                      message: t("ANPR_Screen.Vehicle.Model_Validation"),
                    },
                  }}
                  placeholder={t("ANPR_Screen.Vehicle.Model_placeholder")}
                  required
                  fullWidth
                />

                <CustomTextField
                  name="color"
                  label={
                    <span>
                      {t("ANPR_Screen.Vehicle.Color")}<span className="star-error">*</span>
                    </span>
                  }
                  control={control}
                  rules={{
                    required: t("ANPR_Screen.Vehicle.Color_required"),
                    pattern: {
                      value: REGEX.Color_Regex,
                      message: t("ANPR_Screen.Vehicle.color_Validation"),
                    },
                  }}
                  placeholder={t("ANPR_Screen.Vehicle.Color_placeholder")}
                  required
                  fullWidth
                />

                {/* <CustomSelect
                name="color"
                variant="filled"
                control={control}
                label={
                  <span>
                    Color<span className="star-error">*</span>
                  </span>
                }
                options={countryList}
                placeholder="Select color"
              /> */}
              </Box>

              <Box display="flex" gap={2} className="vehicle-pop-buttons">
                {selectedOwner &&
                  selectedOwner.registrationType != "permanent" ? (
                  <LocalizationProvider dateAdapter={AdapterDayjs}>
                    <Box className="label-full">
                      <FormLabel><span>{t("ANPR_Screen.Vehicle.Visitor_Valid_From")}</span></FormLabel>
                      <Controller
                        name="visitorValidFrom"
                        control={control}
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

                    <Box className="label-full">
                      <FormLabel><span>{t("ANPR_Screen.Vehicle.Visitor_Valid_To")}</span></FormLabel>
                      <Controller
                        name="visitorValidTo"
                        control={control}
                        rules={{
                          validate: (value) => {
                            const from = getValues("visitorValidFrom");
                            if (!from || !value) return true;
                            return dayjs(value).isAfter(dayjs(from))
                              || t("ANPR_Screen.Vehicle.Visitor_Valid_To_Validation");
                          },
                        }}
                        render={({ field, fieldState }) => (
                          <DateTimePicker
                            value={field.value}
                            format={timeFormat === "24h"
                              ? "DD-MM-YYYY HH:mm"
                              : "DD-MM-YYYY hh:mm A"}
                            ampm={timeFormat !== "24h"}
                            onChange={field.onChange}
                            minDateTime={getValues("visitorValidFrom")}
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
                ) : null}

                <CustomButton
                  className="common-btn-design"
                  // fullWidth
                  customStyles={{ mt: 2 }}
                  disabled={isEditMode && !isDirty}
                >
                  {isEditMode ? t("Update") : t("Add_btn")}
                </CustomButton>
                <CustomButton
                  className="common-btn-design"
                  type="button"
                  // fullWidth
                  customStyles={{ mt: 2 }}
                  onClick={() => {
                    reset();
                    setSelectedVehicleId("");
                    setIsEditMode(false);
                  }}
                >
                  {t("ANPR_Screen.Vehicle.Clear")}
                </CustomButton>

              </Box>
            </div>
          </Box>
        )}
        {HasPermission(LABELS.View_List_of_Vehicles) && (
          <Box>
            <DataGrid
              rows={vehicleData}
              columns={columns}
              getRowId={(row) => row.id}
              disableRowSelectionOnClick
              hideFooter
              hideFooterPagination
              hideFooterSelectedRowCount
              autoHeight={false}
              style={{ height: 310 }}
              slots={{
                noRowsOverlay: CustomNoRowsOverlay,
              }}
            />
          </Box>
        )}
      </div>
      <CommonDialog
        open={openDeleteConfirm}
        title={t("Common_DELETE_Confirmation_Dialog.Title")}
        customClass="cmn-confirm-delete-icon "
        content={t("Common_DELETE_Confirmation_Dialog.Content")}
        onConfirm={() =>
          selectedVehicleId && finalDeleteVehicle(selectedVehicleId)
        }
        onCancel={() => setOpenDeleteConfirm(false)}
        confirmText={t("Common_DELETE_Confirmation_Dialog.Delete")}
        cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
        type="delete"
        titleClass={true}
        showCloseIcon={true}

      />
    </div>
  );
};

export { Vehicle };
