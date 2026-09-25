import React, { useState, useEffect } from "react";
import {
  Typography,
  Box,
  IconButton,
  Tooltip,
  Chip,
  Drawer,
} from "@mui/material";
import {
  DataGrid,
  GridCloseIcon,
  GridColDef,
  GridPaginationModel,
  GridSortModel,
} from "@mui/x-data-grid";
import { AddSchedule } from "./AddSchedule";
import { UpdateMaintenanceStatus } from "./UpdateMaintenanceStatus";
import {
  GetMaintenanceScheduleService,
  GetDeviceLiveImageService,
  GetBeforeAfterImageService,
  ExportMaintenanceScheduleCSVService,
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
import { ILookup } from "../../interfaces/ILookup";
import { scheduleFilter, scheduleList } from "../../interfaces/IMaintenance";
import {
  GetAllFloorsListService,
  GetAllZonesByFloorIdService,
} from "../../services/dashboardService";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";

const MaintenanceSchedule: React.FC = () => {
  const [floorList, setFloorList] = useState<ILookup[]>([]);
  const [zoneList, setZoneList] = useState<ILookup[]>([]);
  const [addScheduleDrawer, setAddScheduleDrawer] = useState(false);
  const [scheduleList, setScheduleList] = useState<scheduleList[]>([]);
  const [TotalRecord, SetTotalRecord] = useState<number>(0);
  const [selectedSchedule, setSelectedSchedule] = useState<scheduleList>();
  const [openSettingDrawer, setOpenSettingDrawer] = useState(false);
  const [beforeImg, setBeforeImg] = useState("");
  const [afterImg, setAfterImg] = useState("");
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    pageSize: 10,
    page: 0,
  });
  const [sortModel, setSortModel] = useState<GridSortModel>([
    { field: "maintenanceDueDate", sort: "desc" },
  ]);
  const { theme, themeColor } = useThemeContext();
  const { timeFormat } = useTimeFormatContext();
  const backgroundStyle = {
    backgroundImage: ` url('/images/lines.png'), linear-gradient(287.68deg, #FE6500 -0.05%, #FF8A00 57.77%)`,
    backgroundSize: "cover",
    backgroundRepeat: "no-repeat",
    backgroundPosition: "center",
  };
  const { control, setValue, watch } = useForm<scheduleFilter>({
    defaultValues: {
      floorIds: [],
      zoneIds: [],
      dueFilter: "",
      statusFilter: "",
    },
  });
  const floorIds = watch("floorIds");
  const defaultFloorId = "000000000000000000000000";
  const { t } = useTranslation();

  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
      ? `${themeColor}/dark-theme/`
      : `${themeColor}/`;

  const dueFilterList = [
    {
      title: "None",
      id: "-1",
    },
    {
      title: "Due",
      id: "0",
    },
    {
      title: "Due in 1 day",
      id: "1",
    },
    {
      title: "Due in 1 week",
      id: "7",
    },
    {
      title: "Due in 15 days",
      id: "15",
    },
    {
      title: "Due in 1 month",
      id: "30",
    },
  ];

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
      headerName: t("Maintenance_Schedule.Column_Grid.Device_Name"),
      width: 180,
      filterable: false,
    },
    {
      field: "model",
      headerName: t("Maintenance_Schedule.Column_Grid.Model"),
      width: 180,
      filterable: false,
    },
    {
      field: "location",
      headerName: t("Maintenance_Schedule.Column_Grid.Location"),
      width: 150,
      filterable: false,
    },
    // {
    //   field: "serialNumber",
    //   headerName: t("Maintenance_Schedule.Column_Grid.Serial_Number"),
    //   width: 180,
    //   filterable: false,
    // },
    {
      field: "deviceType",
      headerName: t("Maintenance_Schedule.Column_Grid.Device_Type"),
      width: 180,
      filterable: false,
      renderCell: (params) => {
        return <Chip label={params.value} sx={getChipStyle(params.value)} />;
      },
    },
    {
      field: "ipAddress",
      headerName: t("Maintenance_Schedule.Column_Grid.Ip_Address"),
      width: 180,
      filterable: false,
      renderCell: (params) => {
        return <Chip label={params.value} sx={getChipStyle(params.value)} />;
      },
    },
    {
      field: "maintenanceDueDate",
      headerName: t("Maintenance_Schedule.Column_Grid.Maintenance_Due_Date"),
      width: 180,
      filterable: false,
      renderCell: (params) => {
        const convertedStartDate = formatDateToConfiguredTimezone(params.value);
        const formattedStartDate = formatDate(convertedStartDate, timeFormat);
        const startDateOnly = formattedStartDate.slice(
          0,
          formattedStartDate.indexOf(" ")
        );
        return <span>{startDateOnly}</span>;
      },
    },
    {
      field: "planName",
      headerName: t("Maintenance_Schedule.Column_Grid.Plan_Name"),
      width: 180,
      filterable: false,
    },
    {
      field: "status",
      headerName: t("Maintenance_Schedule.Column_Grid.status"),
      width: 150,
      filterable: false,
    },
    {
      field: "actions",
      headerName: t("Maintenance_Schedule.Column_Grid.Actions"),
      width: 150,
      sortable: false,
      filterable: false,
      renderCell: (params) => (
        <Box>
          {HasPermission(LABELS.AddOrUpdateMaintenanceSchedule) && (
          <Tooltip title={t("Maintenance_Schedule.Column_Grid.Setting_Tooltip")}>
            <IconButton onClick={() => handleSetting(params.row)}>
              <img
                src={"/images/user-action-setting.svg"}
                alt="Setting"
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

  useEffect(() => {
    fetchFloorData();
  }, []);

  useEffect(() => {
    fetchScheduleData();
  }, [paginationModel, sortModel]);

  useEffect(() => {
    if (!floorIds || floorIds.length === 0) 
    {
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

  const fetchScheduleData = async () => {
    const sortBy = sortModel[0]?.field || "maintenanceDueDate";
    const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
    try {
      let request: scheduleFilter = {
        floorIds: watch("floorIds"),
        zoneIds: watch("zoneIds"),
        dueFilter: watch("dueFilter"),
        statusFilter: watch("statusFilter"),
        pageNumber: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        sortBy: sortBy,
        sortOrder: sortOrder,
      };
      const scheduleData: any = await GetMaintenanceScheduleService(request);

      if (scheduleData && scheduleData.isSuccess && scheduleData?.data) {
        setScheduleList(scheduleData?.data?.items);
        SetTotalRecord(scheduleData?.data?.totalCount);
      } else {
        setScheduleList([]);
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

  const handleSetting = async (selectedItem: scheduleList) => {
    setSelectedSchedule(selectedItem);

    let before = "";
    let after = "";

    try {
      switch (selectedItem.status) {
        case "Not Started":
          before = await getLiveImage(selectedItem.deviceId as string);
          break;

        case "In Progress":
        case "Rework":
          if (selectedItem.beforeCameraImage) {
            before = await getImageFromPath(selectedItem.beforeCameraImage);
          }
          after = await getLiveImage(selectedItem.deviceId as string);
          break;

        case "Done":
          if (selectedItem.beforeCameraImage) {
            before = await getImageFromPath(selectedItem.beforeCameraImage);
          }
          if (selectedItem.afterCameraImage) {
            after = await getImageFromPath(selectedItem.afterCameraImage);
          }
          break;
      }

      setBeforeImg(before);
      setAfterImg(after);
      setOpenSettingDrawer(true);
    } catch (error) {
      console.error("Error loading images:", error);
    }
  };

  const getLiveImage = async (deviceId: string) => {
    const res: any = await GetDeviceLiveImageService({ deviceId });
    return res?.isSuccess ? res?.data : "";
  };

  const getImageFromPath = async (path: string) => {
    const res: any = await GetBeforeAfterImageService({ imageFullPath: path });
    return res?.isSuccess ? res?.data : "";
  };

  const exportMaintenanceSchedule = async () => {
    const sortBy = sortModel[0]?.field || "maintenanceDueDate";
    const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
    try {
      let request: scheduleFilter = {
        floorIds: watch("floorIds"),
        zoneIds: watch("zoneIds"),
        dueFilter: watch("dueFilter"),
        statusFilter: watch("statusFilter"),
        pageNumber: 0,
        pageSize: 0,
        sortBy: sortBy,
        sortOrder: sortOrder,
      };
      await ExportMaintenanceScheduleCSVService({data:request});
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };

  const localeText = {
    // Column Menu
    columnMenuSortAsc: t("MUI_Grid_Filter_Paginition.columnMenuSortAsc"),
    columnMenuSortDesc: t("MUI_Grid_Filter_Paginition.columnMenuSortDesc"),
    columnMenuHideColumn: t("MUI_Grid_Filter_Paginition.columnMenuHideColumn"),
    columnMenuManageColumns: t("MUI_Grid_Filter_Paginition.columnMenuManageColumns"),

    // Pagination (TablePagination)
    MuiTablePagination: {
      labelRowsPerPage: t("MUI_Grid_Filter_Paginition.footerPaginationRowsPerPage"),
      labelDisplayedRows: ({ from, to, count }: {
        from: number;
        to: number;
        count: number;
      }) =>
        `${from}–${to} ${t("MUI_Grid_Filter_Paginition.footerTotalVisibleRows", { total: count })}`,
    },

    footerRowSelected: (count: number) =>
      t("MUI_Grid_Filter_Paginition.footerRowSelected", { count }),

    footerRowSelectedPlural: (count: number) =>
      t("MUI_Grid_Filter_Paginition.footerRowSelectedPlural", { count }),
  };

  return (
    <>
      {/* <Container sx={{ mt: 4, width: "100%" }}> */}
      <div className="main-dashbourd-wrapper">
        <div className="top-orange-head" style={backgroundStyle}>
          <Box className="top-orange-head-left">
            <Typography variant="h4">{t("Maintenance_Schedule.Maintenance_Schedule_Header")}</Typography>
            <Typography>{t("Maintenance_Schedule.Maintenance_Schedule_Description")}</Typography>
          </Box>
          {HasPermission(LABELS.AddOrUpdateMaintenanceSchedule) && (
            <CustomButton
              size="small"
              variant="outlined"
              onClick={() => setAddScheduleDrawer(true)}
            >
              <img src={"/images/adddevice.svg"} alt="Add Devices" />
              {t("Maintenance_Schedule.Schedule_Maintenance")}
            </CustomButton>
          )}
        </div>

        <div className="top-orange-head-maintenance" >
          <div className='maintenance-schedule-items'>
            <CustomMultiSelect
              name="floorIds"
              control={control}
              label= {t("Maintenance_Schedule.Select_Floor")}
              options={floorList}
              placeholder= {t("Maintenance_Schedule.Select_PH_Floor")}
              // rules={{ required: "At least one role must be selected" }}
              // required
            />
          </div>
          <div className='maintenance-schedule-items'>
            <CustomMultiSelect
              name="zoneIds"
              control={control}
              label={t("Maintenance_Schedule.Select_Zone")}
              options={zoneList}
              placeholder={t("Maintenance_Schedule.Select_PH_Zone")}
              // rules={{ required: "At least one role must be selected" }}
              // required
            />
          </div>
          <div className='maintenance-schedule-items'>
            <CustomSelect
              name="dueFilter"
              variant="filled"
              control={control}
              label={t("Maintenance_Schedule.Select_due")}
              options={dueFilterList}
              placeholder={t("Maintenance_Schedule.Select_due")}
            />
          </div>
          <div className='maintenance-schedule-items'>
            <CustomSelect
              name="statusFilter"
              variant="filled"
              control={control}
              label={t("RMA.RMA_Filter_Status")}
              options={statusList}
              placeholder={t("Maintenance_Schedule.Select_PH_status")}
            />
          </div>
          <div className='dashbourd-retail-details-export maintenance-schedule-items-only'>
            <CustomButton
              variant="outlined"
              onClick={() => fetchScheduleData()}
            >
              <img src="images/search.svg" alt="" />
            </CustomButton>
          </div>
          <div className='dashbourd-retail-details-export maintenance-schedule-items-only'>
            <CustomButton
              variant="outlined"
              onClick={() => exportMaintenanceSchedule()}
            >
              <img src={"/images/export.svg"} alt="export" />
            </CustomButton>
          </div>
        </div>

        <DataGrid
          rows={scheduleList}
          columns={columns}
          getRowId={(row) => row.id}
          paginationModel={paginationModel}
          onPaginationModelChange={handlePaginationModelChange}
          rowCount={scheduleList.length === 0 ? 0 : TotalRecord}
          paginationMode="server"
          pageSizeOptions={[5, 10, 15, 20, 25]}
          sortingMode="server"
          sortModel={sortModel}
          onSortModelChange={handleSortModelChange}
          disableRowSelectionOnClick
          slots={{
            noRowsOverlay: CustomNoRowsOverlay,
          }}
          hideFooter={scheduleList.length === 0}
          localeText={localeText}
          className="maintanace-schedule-grid"
        />
      </div>

      <Drawer
        anchor={"right"}
        open={addScheduleDrawer}
        onClose={() => {
          setAddScheduleDrawer(false);
        }}
        ModalProps={{
          onClose: (_, reason) => {
            if (reason !== "backdropClick") {
              setAddScheduleDrawer(false);
            }
          },
        }}
        className="cmn-pop"
      >
        <Box className="cmn-pop-head">
          <Typography variant="h6">{t("Maintenance_Schedule.Add_Schedule_Drawer.Add_Schedule")}</Typography>
          <IconButton
            onClick={() => {
              setAddScheduleDrawer(false);
            }}
          >
            <GridCloseIcon />
          </IconButton>
        </Box>

        <AddSchedule
          onClose={() => {
            setAddScheduleDrawer(false);
          }}
          refreshData={() => {
            fetchScheduleData();
          }}
        />
      </Drawer>

      <CommonDialog
        open={openSettingDrawer}
        title={t("Maintenance_Schedule.Setting_Dialog.scheduledMaintenance", {
          id: selectedSchedule?.ipAddress ?? ""
        })}

        // customClass="cmn-confirm-delete-icon"
        content={
          <UpdateMaintenanceStatus
            selectedSchedule={selectedSchedule}
            beforeImg={beforeImg}
            afterImg={afterImg}
            refreshData={() => {
              setOpenSettingDrawer(false);
              fetchScheduleData();
            }}
            onClose={() => {
              setOpenSettingDrawer(false);
            }}
          />
        }
        onCancel={() => setOpenSettingDrawer(false)}
        titleClass={true}
        showCloseIcon={true}
        maxWidth={"lg"}
        // fullWidth={true}
      />
    </>
  );
};

export default MaintenanceSchedule;
