import React, { useState, useEffect } from "react";
import {
  Typography,
  Box,
  IconButton,
  Tooltip,
  Chip,
  Drawer,
  FormLabel,
} from "@mui/material";
import {
  DataGrid,
  GridCloseIcon,
  GridColDef,
  GridPaginationModel,
  GridSortModel,
} from "@mui/x-data-grid";
import { AddEditRMA } from "./AddEditRMA";
import {
  DeleteRMAService,
  ExportRMACSVService,
  GetRMAService,
} from "../../services/maintenanceService";
import { CustomButton } from "../../components/Reusable/CustomButton";
import { HasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { useThemeContext } from "../../context/ThemeContext";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { formatDate } from "../../utils/dateUtils";
import {
  CommonDialog,
  CustomMultiSelect,
  CustomSelect,
} from "../../components";
import { ICommonId } from "../../interfaces/ILookup";
import {
  IGetAllRMARequestProps,
  IRMAProps,
  RMAFilterprops,
} from "../../interfaces/IMaintenance";
import { Controller, useForm } from "react-hook-form";
import {
  GetAllFloorsListService,
  GetAllZonesByFloorIdService,
} from "../../services/dashboardService";
import { ILookup } from "../../interfaces/ILookup";
import { DatePicker, LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { convertToUTC } from "../../utils/convertToUTC";
import dayjs from "dayjs";
import { useTranslation } from "react-i18next";

const RMA: React.FC = () => {
  const [addRMADrawer, setAddRMADrawer] = useState(false);
  const [RMAList, setRMAList] = useState<IRMAProps[]>([]);
  const [TotalRecord, SetTotalRecord] = useState<number>(0);
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    pageSize: 10,
    page: 0,
  });
  const [sortModel, setSortModel] = useState<GridSortModel>([
    { field: "deviceName", sort: "desc" },
  ]);
  const [selectedRMA, setSelectedRMA] = useState<IRMAProps | undefined>(
    undefined
  );
  const [openDeleteConfirm, setOpenDeleteConfirm] = useState(false);
  const [RMAToBeDelete, setRMAToBeDelete] = useState<ICommonId | null>(null);
  const [floorList, setFloorList] = useState<ILookup[]>([]);const [zoneList, setZoneList] = useState<ILookup[]>([]);
  const { t } = useTranslation();

  const { theme, themeColor } = useThemeContext();
  const { timeFormat } = useTimeFormatContext();
  const backgroundStyle = {
    backgroundImage: ` url('/images/lines.png'), linear-gradient(287.68deg, #FE6500 -0.05%, #FF8A00 57.77%)`,
    backgroundSize: "cover",
    backgroundRepeat: "no-repeat",
    backgroundPosition: "center",
  };

  const { control, setValue, watch } = useForm<RMAFilterprops>({
    defaultValues: {
      floorIds: [],
      zoneIds: [],
      startDate: null,
      rmaStatus: "",
    },
  });

  const floorIds = watch("floorIds");
  const defaultFloorId = "000000000000000000000000";

  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
      ? `${themeColor}/dark-theme/`
      : `${themeColor}/`;

  const getChipStyle = (type: string) => {
    return type === "AIB"
      ? {
          bgcolor: "#FFF7D6 !important",
          color: "#FF8A01  !important",
          fontWeight: 600,
          height: "22px",
        }
      : {
          bgcolor: "#E0FFD6  !important",
          color: "#1E7A00  !important",
          fontWeight: 600,
          height: "22px",
        };
  };

  const columns: GridColDef[] = [
    {
      field: "deviceName",
      headerName: t("RMA.RMA_Grid_DeviceName"),
      width: 150,
      filterable: false,
    },
    {
      field: "ipAddress",
      headerName: t("RMA.RMA_Grid_IPAddress"),
      width: 180,
      filterable: false,
    },
    {
      field: "model",
      headerName: t("RMA.RMA_Grid_Model"),
      width: 150,
      filterable: false,
    },
    {
      field: "location",
      headerName: t("RMA.RMA_Grid_Location"),
      width: 150,
      filterable: false,
    },
    // {
    //   field: "serialNumber",
    //   headerName:  t("RMA.RMA_Grid_SerialNumber"),
    //   width: 180,
    //   filterable: false,
    // },
    {
      field: "deviceType",
      headerName: t("RMA.RMA_Grid_DeviceType"),
      width: 180,
      filterable: false,
      renderCell: (params) => {
        return <Chip label={params.value} sx={getChipStyle(params.value)} />;
      },
    },
    // {
    //   field: "inProgressNotes",
    //   headerName:  t("RMA.RMA_Grid_InProgressNotes"),
    //   width: 200,
    //   filterable: false,
    // },
    // {
    //   field: "completedNotes",
    //   headerName:  t("RMA.RMA_Grid_CompletedNotes"),
    //   width: 200,
    //   filterable: false,
    // },
    {
      field: "rmaStatus",
      headerName: t("RMA.RMA_Grid_RMAStatus"),
      width: 200,
      filterable: false,
      renderCell: (params) => {
        return (
          <span>
            {params.value === "InProgress" ? "In Progress" : params.value}
          </span>
        );
      },
    },

    {
      field: "startDate",
      headerName: "Start Date",
      width: 220,
      filterable: false,
      renderCell: (params) => {
        const convertedStartDate = formatDateToConfiguredTimezone(params.value);
        const formattedStartDate = formatDate(convertedStartDate, timeFormat);
        return <span>{formattedStartDate}</span>;
      },
    },
    {
      field: "endDate",
      headerName: "End Date",
      width: 220,
      filterable: false,
      renderCell: (params) => {
        const convertedEndDate = formatDateToConfiguredTimezone(params.value);
        const formattedEndDate = formatDate(convertedEndDate, timeFormat);
        return <span>{formattedEndDate}</span>;
      },
    },
    {
      field: "actions",
      headerName: t("RMA.RMA_Grid_Actions"),
      width: 100,
      sortable: false,
      filterable: false,
      renderCell: (params) => (
        <Box>
          {HasPermission(LABELS.AddOrUpdateRMA) && (
            <Tooltip title={t("RMA.RMA_Grid_Tooltip_EditPlan")}>
              <IconButton onClick={() => handleEdit(params.row)}>
                <img
                  src={
                    theme === "light"
                      ? "/images/edit.svg"
                      : "/images/dark-theme/edit.svg"
                  }
                  alt="Edit Plan"
                  width="20"
                  height="20"
                />
              </IconButton>
            </Tooltip>
          )}
          {HasPermission(LABELS.DeleteRMA) && (
            <Tooltip title={t("RMA.RMA_Grid_Tooltip_DeletePlan")}>
              <IconButton onClick={() => handleDelete(params.id as string)}>
                <img
                  src={"/images/user-action-delete.svg"}
                  alt="Delete Plan"
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

  const localeText = {
    // Column Menu
    columnMenuSortAsc: t("MUI_Grid_Filter_Paginition.columnMenuSortAsc"),
    columnMenuSortDesc: t("MUI_Grid_Filter_Paginition.columnMenuSortDesc"),
    columnMenuHideColumn: t("MUI_Grid_Filter_Paginition.columnMenuHideColumn"),
    columnMenuManageColumns: t(
      "MUI_Grid_Filter_Paginition.columnMenuManageColumns"
    ),

    // Pagination (TablePagination)
    MuiTablePagination: {
      labelRowsPerPage: t(
        "MUI_Grid_Filter_Paginition.footerPaginationRowsPerPage"
      ),
      labelDisplayedRows: ({
        from,
        to,
        count,
      }: {
        from: number;
        to: number;
        count: number;
      }) =>
        `${from}–${to} ${t(
          "MUI_Grid_Filter_Paginition.footerTotalVisibleRows",
          { total: count }
        )}`,
    },

    footerRowSelected: (count: number) =>
      t("MUI_Grid_Filter_Paginition.footerRowSelected", { count }),

    footerRowSelectedPlural: (count: number) =>
      t("MUI_Grid_Filter_Paginition.footerRowSelectedPlural", { count }),
  };

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
    fetchRMAData();
  }, [paginationModel, sortModel]);

  useEffect(() => {
    fetchFloorData();
  }, []);

  useEffect(() => {
    if (!floorIds || floorIds.length === 0) {
      setZoneList([]);
      return;
    }

    const lastSelectedId = floorIds[floorIds.length - 1];

    if (lastSelectedId === defaultFloorId) {
      if (floorIds.length !== 1) {
        setValue("floorIds", [defaultFloorId], {
          shouldValidate: true,
        });
      }
    } else {
      if (floorIds.includes(defaultFloorId)) {
        setValue(
          "floorIds",
          floorIds.filter((id) => id !== defaultFloorId),
          { shouldValidate: true }
        );
      }
    }
    fetchZoneData(floorIds);
  }, [floorIds]);

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

      const allZones: ILookup[] = (response?.data ?? []).flatMap((floor: any) =>
        Array.isArray(floor?.zones)
          ? floor.zones.map((zone: any) => ({
              id: zone.id,
              title: zone.zoneName,
            }))
          : []
      );

      setZoneList(allZones);
      setValue("zoneIds", []);
    } catch (err: any) {
      console.error("Error while fetching the zone data:", err?.message || err);
      setZoneList([]);
    }
  };

  const fetchRMAData = async () => {
    const sortBy = sortModel[0]?.field || "deviceName";
    const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;

    const startDate = watch("startDate");

    const start_Date = startDate
      ? convertToUTC(dayjs(startDate).format("YYYY-MM-DDTHH:mm:ss"))
      : "";

    try {
      let request: IGetAllRMARequestProps = {
        searchText: "",
        pageNumber: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        sortBy: sortBy,
        sortOrder: sortOrder,
        floorIds: watch("floorIds"),
        zoneIds: watch("zoneIds"),
        startDate: start_Date,
        rmaStatus: watch("rmaStatus"),
      };
      const RMAData: any = await GetRMAService(request);

      if (RMAData && RMAData?.isSuccess && RMAData?.data) {
        setRMAList(RMAData?.data?.items);
        SetTotalRecord(RMAData?.data?.totalCount);
      } else {
        setRMAList([]);
        SetTotalRecord(0);
      }
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };

  const handlePaginationModelChange = (
    newPaginationModel: GridPaginationModel
  ) => {
    // setPaginationModel(newPaginationModel);
    const isPageSizeChanged = newPaginationModel.pageSize !== paginationModel.pageSize;
    if (isPageSizeChanged) {
      setPaginationModel({
        ...newPaginationModel,
        page: 0,
      });
    } else {
      setPaginationModel(newPaginationModel);
    }
  };

  const handleSortModelChange = (newSortModel: GridSortModel) => {
    setSortModel(newSortModel);
  };

  const handleDelete = (planId: string) => {
    setRMAToBeDelete({ id: planId });
    setOpenDeleteConfirm(true);
  };

  const CustomNoRowsOverlay = () => (
    <Box className="no-data-douns">
      <Box sx={{ width: 200, justifyItems: "center", flex: 1 }}>
        <img
          src={`/images/${themeColorPath}noData.gif`}
          alt="Animated GIF"
          width="100"
          height="100"
        />
        <Typography sx={{ FontWeight: 600, fontSize: 24, color: "#090909" }}>
          {t("No_data_found")}
        </Typography>
      </Box>
    </Box>
  );

  const handleEdit = (item: IRMAProps) => {
    setSelectedRMA(item);
    setAddRMADrawer(true);
  };

  const handleDeleteRMA = async (id: string) => {
    const param = {
      id: id,
    };
    try {
      const deleteData: any = await DeleteRMAService(param);
      if (deleteData.isSuccess) {
        setOpenDeleteConfirm(false);
        fetchRMAData();
      }
    } catch (err: any) {}
  };

  const exportRMAData = async () => {
    const sortBy = sortModel[0]?.field || "maintenanceDueDate";
    const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
    const startDate = watch("startDate");

    const start_Date = startDate
      ? convertToUTC(dayjs(startDate).format("YYYY-MM-DDTHH:mm:ss"))
      : "";
    try {
      let request: IGetAllRMARequestProps = {
        searchText: "",
        pageNumber: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        sortBy: sortBy,
        sortOrder: sortOrder,
        floorIds: watch("floorIds"),
        zoneIds: watch("zoneIds"),
        startDate: start_Date,
        rmaStatus: watch("rmaStatus"),
      };
      await ExportRMACSVService({ data: request });
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };

  return (
    <>
      {/* <Container sx={{ mt: 4, width: "100%" }}> */}
      <div className="main-dashbourd-wrapper">
        <div className="top-orange-head" style={backgroundStyle}>
          <Box className="top-orange-head-left">
            <Typography variant="h4">{t("RMA.RMA_Header")}</Typography>
            <Typography>{t("RMA.RMA_Header_Desc")}</Typography>
          </Box>
          {HasPermission(LABELS.AddOrUpdateRMA) && (
            <CustomButton
              size="small"
              variant="outlined"
              onClick={() => setAddRMADrawer(true)}
            >
              <img src={"/images/adddevice.svg"} alt="Add Devices" />
              {t("RMA.RMA_Add_Button")}
            </CustomButton>
          )}
        </div>

        <Box className="top-orange-head-maintenance">
          <div className="maintenance-schedule-items">
            <CustomMultiSelect
              name="floorIds"
              control={control}
              label={t("RMA.RMA_Select_Floor")}
              options={floorList}
              placeholder={t("Maintenance_Schedule.Select_PH_Floor")}
            />
          </div>
          <div className="maintenance-schedule-items">
            <CustomMultiSelect
              name="zoneIds"
              control={control}
              label={t("RMA.RMA_Select_Zone")}
              options={zoneList}
              placeholder={t("Maintenance_Schedule.Select_PH_Zone")}
            />
          </div>
          <div className="maintenance-schedule-items">
            <LocalizationProvider dateAdapter={AdapterDayjs}>
              <Box>
                <FormLabel>
                  <span>{t("RMA.RMA_Filter_StartDate")}</span>
                </FormLabel>
                <Controller
                  name="startDate"
                  control={control}
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
          </div>
          <div className="maintenance-schedule-items">
            <CustomSelect
              name="rmaStatus"
              variant="filled"
              control={control}
              label={t("RMA.RMA_Filter_Status")}
              options={statusList}
              placeholder={t("RMA.RMA_Filter_PH_Status")}
            />
          </div>
          <div className="dashbourd-retail-details-export">
            <CustomButton variant="outlined" onClick={() => fetchRMAData()}>
              <img src="images/search.svg" alt="" />
            </CustomButton>
          </div>
          <div className="dashbourd-retail-details-export">
            <CustomButton variant="outlined" onClick={() => exportRMAData()}>
              <img src={"/images/csv.svg"} alt="export" />
            </CustomButton>
          </div>
        </Box>

        <DataGrid
          rows={RMAList}
          columns={columns}
          getRowId={(row) => row.id}
          paginationModel={paginationModel}
          onPaginationModelChange={handlePaginationModelChange}
          rowCount={RMAList.length === 0 ? 0 : TotalRecord}
          paginationMode="server"
          pageSizeOptions={[5, 10, 15, 20, 25]}
          sortingMode="server"
          sortModel={sortModel}
          onSortModelChange={handleSortModelChange}
          disableRowSelectionOnClick
          slots={{
            noRowsOverlay: CustomNoRowsOverlay,
          }}
          hideFooter={RMAList.length === 0}
          localeText={localeText}
          className="rma-data-grid"
        />
      </div>

      <Drawer
        anchor={"right"}
        open={addRMADrawer}
        onClose={() => {
          setAddRMADrawer(false);
        }}
        ModalProps={{
          onClose: (_, reason) => {
            if (reason !== "backdropClick") {
              setAddRMADrawer(false);
            }
          },
        }}
        className="cmn-pop"
      >
        <Box className="cmn-pop-head">
          <Typography variant="h6">{selectedRMA?.id ? t("RMA.RMA_Edit_Header") : t("RMA.RMA_Add_Button")}</Typography>
          <IconButton
            onClick={() => {
              setAddRMADrawer(false);
              setSelectedRMA(undefined);
            }}
          >
            <GridCloseIcon />
          </IconButton>
        </Box>

        <AddEditRMA
          onClose={() => {
            setAddRMADrawer(false);
          }}
          refreshData={() => {
            fetchRMAData();
          }}
          RMAData={selectedRMA}
        />
      </Drawer>

      <CommonDialog
        open={openDeleteConfirm}
        title={t("Common_DELETE_Confirmation_Dialog.Title")}
        content={t("Common_DELETE_Confirmation_Dialog.Content")}
        customClass="cmn-confirm-delete-icon"
        onConfirm={() => RMAToBeDelete && handleDeleteRMA(RMAToBeDelete.id)}
        onCancel={() => setOpenDeleteConfirm(false)}
        confirmText={t("Common_DELETE_Confirmation_Dialog.Delete")}
        cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
        type="delete"
        titleClass={true}
        showCloseIcon={true}
      />
    </>
  );
};

export default RMA;
