import React, {
  useState,
  useEffect,
  useRef,
  useCallback,
  useMemo,
} from "react";
import {
  Typography,
  Container,
  TextField,
  Box,
  IconButton,
  Tooltip,
  FormControl,
  Grid,
  Card,
  CardContent,
  Chip,
  InputAdornment,
  ToggleButtonGroup,
  ToggleButton,
  TablePagination,
  Drawer,
  Checkbox,
  Button,
  ButtonGroup,
  Switch,
  Menu,
  MenuItem,
  ListItemText,
} from "@mui/material";
import {
  DataGrid,
  GridCloseIcon,
  GridColDef,
  GridPaginationModel,
  GridSortModel,
} from "@mui/x-data-grid";
import { DeviceAddEdit } from "../Camera/DeviceAddEdit";
import {
  ICamera,
  IGetAllDeviceRequest,
  IUpdateDeviceMaintenanceStstus,
} from "../../interfaces/ICamera";
import {
  DeleteCameraService,
  ExportDeviceCSVService,
  GetAllCameraListService,
  manuallyOpenGateBarrierService,
  SampleExcelDownloadService,
  UpdateDeviceMaintenanceStatusService,
} from "../../services/cameraService";
import { CustomButton } from "../../components/Reusable/CustomButton";
import { CommonDialog } from "../../components/Reusable/CommonDialog";

import Divider from "@mui/material/Divider";
import { HasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { useThemeContext } from "../../context/ThemeContext";
import { BulkUploadDevice } from "./BulkUploadDevice";
import { useTranslation } from "react-i18next";
import VerticalAlignBottomIcon from "@mui/icons-material/VerticalAlignBottom";
import AddIcon from "@mui/icons-material/Add";
import IosShareIcon from "@mui/icons-material/IosShare";
import { useNavigate } from "react-router-dom";
import { MoreVert } from "@mui/icons-material";
import { showToast } from "../../components";
import { error } from "console";

const DeviceManagement: React.FC = () => {
  const [cameraGridData, setCameraGridData] = useState<ICamera[]>([]);
  const [cameraListData, setCameraListData] = useState<ICamera[]>([]);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [openDeleteConfirm, setOpenDeleteConfirm] = useState(false);
  const [openAddCamera, setOpenAddCamera] = useState<boolean>(false);
  const [openEditCamera, setOpenEditCamera] = useState<boolean>(false);
  const [selectedCamera, setSelectedCamera] = useState<ICamera | undefined>(
    undefined,
  );
  const [TotalRecord, SetTotalRecord] = useState<number>(0);
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    pageSize: 10,
    page: 0,
  });
  const [gridPaginationModel, setGridPaginationModel] =
    useState<GridPaginationModel>({
      pageSize: 8,
      page: 0,
    });
  // const [gridpage, setGridPage] = useState(1);
  // const [loading, setLoading] = useState(false);
  // const [hasMore, setHasMore] = useState(true);
  const [displayView, setDisaplayView] = useState<string>("listview");
  // const observer = useRef<IntersectionObserver | null>(null);
  const [selectedDevices, setSelectedDevices] = useState<string[]>([]);
  const [maintenanceStatus, SetMaintenanceStatus] = useState(false);
  const [menuAnchor, setMenuAnchor] = useState<{
    anchorEl: HTMLElement | null;
    item: any | null;
  }>({
    anchorEl: null,
    item: null,
  });
  const open = Boolean(menuAnchor.anchorEl);
  const [sortModel, setSortModel] = useState<GridSortModel>([
    { field: "deviceName", sort: "desc" }, // default sorting
  ]);

  const [isOpenBulkUploadDeviceDrawer, setIsOpenBulkUploadDeviceDrawer] =
    useState(false);
  const navigate = useNavigate();

  const ViewDeviceLogs = async (collectionId: string) => {
    navigate("/audit", {
      state: { id: collectionId, collectionName: "deviceMaster" },
    });
  };
  const handleCloseDrawer = () => {
    setIsOpenBulkUploadDeviceDrawer(false);
  };

  
  const handleClose = () => {
    setMenuAnchor({ anchorEl: null, item: null });
  };
  const isANPR = localStorage.getItem("isANPR");

  const { theme, themeColor } = useThemeContext();
  const { t } = useTranslation();

  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
        ? `${themeColor}/dark-theme/`
        : `${themeColor}/`;

  // useEffect(() => {
  //   if (displayView === "gridview") {
  //     fetchGridCameraData();
  //   } else {
  //     fetchListCameraData();
  //   }
  // }, [displayView, paginationModel, gridPaginationModel, sortModel]);

  const localeText = {
    // Column Menu
    columnMenuSortAsc: t("MUI_Grid_Filter_Paginition.columnMenuSortAsc"),
    columnMenuSortDesc: t("MUI_Grid_Filter_Paginition.columnMenuSortDesc"),
    columnMenuHideColumn: t("MUI_Grid_Filter_Paginition.columnMenuHideColumn"),
    columnMenuManageColumns: t(
      "MUI_Grid_Filter_Paginition.columnMenuManageColumns",
    ),

    // Pagination (TablePagination)
    MuiTablePagination: {
      labelRowsPerPage: t(
        "MUI_Grid_Filter_Paginition.footerPaginationRowsPerPage",
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
          { total: count },
        )}`,
    },

    footerRowSelected: (count: number) =>
      t("MUI_Grid_Filter_Paginition.footerRowSelected", { count }),

    footerRowSelectedPlural: (count: number) =>
      t("MUI_Grid_Filter_Paginition.footerRowSelectedPlural", { count }),
  };

  useEffect(() => {
    const delayDebounce = setTimeout(() => {
      if (displayView === "gridview") {
        fetchGridCameraData();
      } else {
        if (searchTerm.length >= 3 || searchTerm.length === 0) {
          fetchListCameraData();
        }
      }
    }, 500); // Optional debounce to limit rapid-fire calls

    return () => clearTimeout(delayDebounce);
  }, [
    displayView,
    paginationModel,
    gridPaginationModel,
    sortModel,
    searchTerm,
  ]);

  // useEffect(() => {
  //   const delayDebounce = setTimeout(() => {
  //     if (searchTerm.length >= 3) {
  //       fetchListCameraData();

  //     }
  //     resetpagination();
  //   }, 1000);

  //   return () => clearTimeout(delayDebounce);
  // }, [searchTerm])

  const fetchGridCameraData = async () => {
    // if (loading || !hasMore) return;
    // setLoading(true);
    try {
      let request: IGetAllDeviceRequest = {
        searchText: searchTerm.length > 0 ? searchTerm : "",
        pageNo: gridPaginationModel.page + 1,
        pageSize: gridPaginationModel.pageSize,
        deviceIds: null,
        sortBy: "deviceName",
        sortOrder: -1,
      };
      if (searchTerm.length === 0 || searchTerm.length >= 3) {
        // suburl = `?searchText=${searchTerm.trim()}&PageSize=${gridPaginationModel.pageSize
        //   }&PageNo=${gridPaginationModel.page + 1}`;

        const cameraData: any = await GetAllCameraListService(request);

        setCameraGridData(cameraData.data.deviceDetails as ICamera[]);
        SetTotalRecord(cameraData.data.totalCount);
      }

      // suburl = `?gridPaginationModel.=${10}&PageNo=${gridpage+1}`;

      // setCameraGridData((prevCameras) => {
      //   const updatedCameras = [...prevCameras, ...cameraData.cameraDetails];

      //   if (updatedCameras.length >= cameraData.totalCount) {
      //     setHasMore(false);
      //   }

      //   return updatedCameras;
      // });

      // if (cameraData.cameraDetails.length > 0) {
      //   setGridPage((prev) => prev + 1);
      // } else {
      //   setHasMore(false);
      // }
    } catch (err: any) {
      console.error("Error fetching camera data:", err);
    } finally {
      // setLoading(false);
    }
  };

  const fetchListCameraData = async (isReload = false) => {
    try {
      const sortBy = sortModel[0]?.field || "deviceName";
      const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
      let deviceIds: string[];
      if (displayView === "gridview") {
        // need to add condition as reload is common for both gridview and listview
        deviceIds = cameraGridData.map((device) => device.id!);
      } else {
        deviceIds = cameraListData.map((device) => device.id!);
      }
      let request: IGetAllDeviceRequest = {
        searchText: searchTerm.length > 0 ? searchTerm : "",
        pageNo: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        deviceIds: isReload ? deviceIds : null,
        sortBy: sortBy,
        sortOrder: sortOrder,
      };
      if (searchTerm.length === 0 || searchTerm.length >= 3) {
        // suburl = `?searchText=${searchTerm.trim()}&PageSize=${paginationModel.pageSize
        //   }&PageNo=${paginationModel.page + 1}`;

        const cameraData: any = await GetAllCameraListService(request);
        if (cameraData.data?.deviceDetails.length > 0) {
          setCameraListData(cameraData.data.deviceDetails as ICamera[]);
          SetTotalRecord(cameraData.data.totalCount);
        } else {
          setCameraListData([]);
          SetTotalRecord(0);
        }
      }
    } catch (err: any) {
      console.error("Error fetching camera data:", err);
    }
  };

  const updateMaintenanceMode = async (
    deviceId: string,
    updatedValue: boolean,
  ) => {
    try {
      const UpdatedData = {
        deviceId: deviceId,
        isMaintenance: updatedValue,
      };
     const response =  await UpdateDeviceMaintenanceStatusService(
        UpdatedData as IUpdateDeviceMaintenanceStstus,
      );
     
      if (typeof response === "string") {
        showToast(response, "error");
        return false;
      }

      if (response?.data as any === false) {
        showToast(response?.message, "error")
        return false
      } else {
        showToast(response?.message, "success")
        return true
      }

    } catch (err: any) {
      console.error("Error in update Maintenance Mode:", err);
      return false;
    }
  };

  // const lastElementRef = useCallback(
  //   (node: HTMLDivElement | null) => {
  //     if (loading) return;
  //     if (observer.current) observer.current.disconnect();

  //     observer.current = new IntersectionObserver((entries) => {
  //       if (entries[0].isIntersecting && hasMore && !loading) {
  //         console.log("Fetching more data...");
  //         fetchGridCameraData();
  //       }
  //     });

  //     if (node) observer.current.observe(node);
  //   },
  //   [loading, hasMore]
  // );

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

  const refreshCameraData = async () => {
    await Promise.all([fetchListCameraData(), fetchGridCameraData()]);
  };

  const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    const searchValue = event.target.value;
    setSearchTerm(searchValue);
  };

  const handleCloseConfirm = () => {
    setOpenDeleteConfirm(false);
  };

  const finalDeleteDevices = async () => {
    if (selectedDevices.length > 0) {
      try {
        const deleteData = await DeleteCameraService(selectedDevices);
        // handleResponse(deleteData);
        if (deleteData !== undefined) {
          setOpenDeleteConfirm(false);
        }
        refreshCameraData();
      } catch (err: any) {}
    }
  };

  const handleAddCamera = () => {
    setOpenAddCamera(true);
  };

  const handleCloseAddModal = () => {
    setOpenAddCamera(false);
  };

  const handleCloseEditModal = () => {
    setOpenEditCamera(false);
  };

  const handleEdit = (camera: ICamera) => {
    handleClose();
    setSelectedCamera(camera);
    setOpenEditCamera(true);
  };

  const handleDelete = () => {
    setOpenDeleteConfirm(true);
  };

  const columns: GridColDef[] = [
    {
      field: "deviceName",
      headerName: t("Manage_Device.Manage_Device_Grid_column.Device_name"),
      width: 200,
      filterable: false,
      renderCell: (params) => {
        const { deviceType, isOnline } = params.row;
        const imageSrc =
          deviceType === "AIB"
            ? theme === "light"
              ? isOnline
                ? "/images/AI_Box.gif"
                : "/images/offline_AI_Box.svg"
              : isOnline
                ? "/images/dark-theme/AI_Box.gif"
                : "/images/dark-theme/offline_AI_Box.svg"
            : deviceType === "MLenses"
              ? theme === "light"
                ? isOnline
                  ? "/images/M_Lenses.gif"
                  : "/images/offline_M_Lenses.svg"
                : isOnline
                  ? "/images/dark-theme/M_Lenses.gif"
                  : "/images/dark-theme/offline_M_Lenses.svg"
            : deviceType === "ANPR"
              ? theme === "light"
                ? isOnline
                  ? "/images/ANPR_icon_online.gif"
                  : "/images/ANPR_icon_offline.gif"
                : isOnline
                  ? "/images/ANPR_icon_online.gif"
                  : "/images/ANPR_icon_offline.gif"
              : theme === "light"
                ? isOnline
                  ? "/images/activecamera.gif"
                  : "/images/offlineCamera.svg"
                : isOnline
                  ? "/images/dark-theme/activecamera.gif"
                  : "/images/dark-theme/offlineCamera.svg";

        return (
          <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
            <img src={imageSrc} alt={deviceType} width="34" height="34" />
            <span>{params.value}</span>
          </div>
        );
      },
    },
    {
      field: "model",
      headerName: t("Manage_Device.Manage_Device_Grid_column.Model"),
      width: 200,
      filterable: false,
    },
    {
      field: "ipAddress",
      headerName: t("Manage_Device.Manage_Device_Grid_column.IP_Address"),
      width: 200,
      filterable: false,
      renderCell: (params) => {
        const protocol = params.row.isHttps ? "https" : "http";
        return (
          <a
            // href={`http://${params.value}`}
            href={`${protocol}://${params.value}`}
            target="_blank"
            rel="noopener noreferrer"
            style={{
              color: "#1168D0",
              textDecoration: "underline",
              fontWeight: 600,
            }}
          >
            {params.value}
          </a>
        );
      },
    },
    {
      field: "location",
      headerName: t("Manage_Device.Manage_Device_Grid_column.Location"),
      width: 200,
      filterable: false,
    },
    // {
    //   field: "cameraType",
    //   headerName: "cameraType",
    //   width: 250,
    //   renderCell: (params) => {
    //     return (
    //       <Box>
    //         {(params.value || []).map((cameratype: string, index: number) => (
    //           <Chip
    //             key={index}
    //             label={cameratype === "people_count" ? "People Count" : "Vehicle Count"}
    //             size="small"
    //             color="warning"
    //             variant="outlined"
    //             style={{ margin: 2 }}
    //           />
    //         ))}
    //       </Box>
    //     );
    //   }

    // },
    {
      field: "zoneNames",
      headerName: t("Manage_Device.Manage_Device_Grid_column.Zones"),
      width: 200,
      filterable: false,
    },
    {
      field: "serialNumber",
      headerName: t("Manage_Device.Manage_Device_Grid_column.Serial_number"),
      width: 200,
      filterable: false,
    },

    // {
    //   field: "direction",
    //   headerName: "direction",
    //   width: 150,
    //   renderCell: (params) => {
    //     return (
    //       <Box>
    //         {(params.value || []).map((direction: string, index: number) => (
    //           <Chip
    //             key={index}
    //             label={direction === "in" ? "In" : "Out"}
    //             size="small"
    //             color={direction === "in" ? "success" : "error"}
    //             variant="outlined"
    //             style={{ margin: 2 }}
    //           />
    //         ))}
    //       </Box>
    //     );
    //   }
    // },
    // {
    //   field: "apiType",
    //   headerName: "apiType",
    //   width: 150,
    //   renderCell: (params) => {
    //     return (
    //       <Box>
    //         {params.value === "wise_api" ? "Wise Api" : "Sun Api"}
    //       </Box>
    //     );
    //   }

    // },
    {
      field: "macAddress",
      headerName: t("Manage_Device.Manage_Device_Grid_column.macAddress"),
      width: 220,
      filterable: false,
    },
    {
      field: "deviceType",
      headerName: t("Manage_Device.Manage_Device_Grid_column.DeviceType"),
      width: 150,
      filterable: false,
      renderCell: (params) => {
        return <Chip label={params.value} sx={getChipStyle(params.value)} />;
      },
    },
    {
      field: "isMaintenance",
      headerName: t("Manage_Device.Manage_Device_Grid_column.Maintenance_Mode"),
      width: 180,
      sortable: false,
      filterable: false,
      renderCell: (params) => {
        if (params.row.deviceType !== "ANPR") {
          return null; 
        }
        return (
          <Switch
            checked={Boolean(params.row.isMaintenance)}
            onChange={(event) =>
              handleToggle(params.row.id, event.target.checked)
            }
            color="primary"
          />
        );
      },
    },
    {
      field: "actions",
      headerName: t("Manage_Device.Manage_Device_Grid_column.Actions"),
      width: 180,
      sortable: false,
      filterable: false,
      renderCell: (params) => (
        <Box>
          {HasPermission(LABELS.Can_Add_Or_Update_Device) && (
            <Tooltip title={t("Manage_Device.Edit_camera")}>
              <IconButton onClick={() => handleEdit(params.row)}>
                <img
                  src={
                    theme === "light"
                      ? "/images/edit.svg"
                      : "/images/dark-theme/edit.svg"
                  }
                  alt="Edit Camera"
                  width="20"
                  height="20"
                />
              </IconButton>
            </Tooltip>
          )}
          {(HasPermission(LABELS.ManuallyOpenGate) && isANPR === "true" && params.row.deviceType === "ANPR") && (
            <Tooltip title={t("Manage_Device.Open_Gate")}>
              <IconButton
                onClick={() => manuallyOpenGateBarrier(params?.row?.id)}
              >
                <img
                  src={
                    theme === "light"
                      ? "/images/stop.svg"
                      : "/images/dark-theme/stop.svg"
                  }
                  alt="Open Gate"
                  width="20"
                  height="20"
                />
              </IconButton>
            </Tooltip>
          )}
          {HasPermission(LABELS.AuditLogMaster) &&
            HasPermission(LABELS.ViewAuditLogsDeviceMaster) && (
              <Tooltip title={t("ANPR_Screen.Owner.View_Audit_logs")}>
                <IconButton onClick={() => ViewDeviceLogs(params.id as string)}>
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

const handleToggle = async (deviceId: string, updatedValue: boolean) => {
  // optimistic UI update for DataGrid
  setCameraListData((prev) =>
    prev.map((row) =>
      row.id === deviceId ? { ...row, isMaintenance: updatedValue } : row
    )
  );

  // optimistic UI update for Grid view
  setCameraGridData((prev) =>
    prev.map((row) =>
      row.id === deviceId ? { ...row, isMaintenance: updatedValue } : row
    )
  );

  const res = await updateMaintenanceMode(deviceId, updatedValue);

  if (res === false) {
    // rollback DataGrid
    setCameraListData((prev) =>
      prev.map((row) =>
        row.id === deviceId ? { ...row, isMaintenance: !updatedValue } : row
      )
    );

    // rollback Grid view
    setCameraGridData((prev) =>
      prev.map((row) =>
        row.id === deviceId ? { ...row, isMaintenance: !updatedValue } : row
      )
    );
  }
};

  const manuallyOpenGateBarrier = async (deviceId: string) => {
    if (deviceId != null && deviceId != "") {
      let data = {
        deviceId: deviceId.trim()
      }

      try {
        await manuallyOpenGateBarrierService(data);
      } catch (err: any) {
        console.error(
          "Error while manually open gate barrier",
          err?.message || err,
        );
      }
    }
  };

  const handlePaginationModelChange = (
    newPaginationModel: GridPaginationModel,
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

  const resetpagination = () => {
    setPaginationModel({
      ...paginationModel,
      page: 0,
    });
  };

  const changeDisaplayView = (
    event: React.MouseEvent<HTMLElement>,
    nextView: string,
  ) => {
    setDisaplayView(nextView);
    setSelectedDevices([]);
  };

  const handleCheckboxChange = (deviceId: string) => {
    setSelectedDevices((prev) =>
      prev.includes(deviceId)
        ? prev.filter((id) => id !== deviceId)
        : [...prev, deviceId],
    );
  };

  const backgroundStyle = {
    backgroundImage: ` url('/images/lines.png'), linear-gradient(287.68deg, #FE6500 -0.05%, #FF8A00 57.77%)`,
    backgroundSize: "cover",
    backgroundRepeat: "no-repeat",
    backgroundPosition: "center",
  };
  const handleSortModelChange = (newSortModel: GridSortModel) => {
    setSortModel(newSortModel);
  };

  const CameraGrid = () => {
    return (
      <>
        <div className="grid-view-table">
          <Grid className="table-grid-main">
            {cameraGridData.length > 0 ? (
              cameraGridData.map((camera, index) => {
                const { deviceType, isOnline } = camera;

                const imageSrc =
                  deviceType === "AIB"
                    ? theme === "light"
                      ? isOnline
                        ? "/images/AI_Box.gif"
                        : "/images/offline_AI_Box.svg"
                      : isOnline
                        ? "/images/dark-theme/AI_Box.gif"
                        : "/images/dark-theme/offline_AI_Box.svg"
                    : deviceType === "MLenses"
                      ? theme === "light"
                        ? isOnline
                          ? "/images/M_Lenses.gif"
                          : "/images/offline_M_Lenses.svg"
                        : isOnline
                          ? "/images/dark-theme/M_Lenses.gif"
                          : "/images/dark-theme/offline_M_Lenses.svg"
                      : deviceType === "ANPR"
                        ? theme === "light"
                          ? isOnline
                            ? "/images/ANPR_icon_online.gif"
                            : "/images/ANPR_icon_offline.gif"
                          : isOnline
                            ? "/images/ANPR_icon_online.gif"
                            : "/images/ANPR_icon_offline.gif"
                      : theme === "light"
                        ? isOnline
                          ? "/images/activecamera.gif"
                          : "/images/offlineCamera.svg"
                        : isOnline
                          ? "/images/dark-theme/activecamera.gif"
                          : "/images/dark-theme/offlineCamera.svg";

                return (
                  <Grid item className="table-grid-items">
                    <Card
                      className="table-grid-card"
                      style={{ height: "100%" }}
                    >
                      <Grid className="table-grid-head">
                        <img
                          src={imageSrc}
                          alt={deviceType}
                          width="34"
                          height="34"
                        />

                        <Box className="grid-avtar-detail">
                          <Typography>
                            {t(
                              "Manage_Device.Manage_Device_Grid_column.Device_name",
                            )}
                          </Typography>
                          <Typography>{camera.deviceName}</Typography>
                        </Box>

                        <Box className="table-grid-details">
                          <Checkbox
                            checked={selectedDevices.includes(
                              camera.id as string,
                            )}
                            onChange={() =>
                              handleCheckboxChange(camera.id as string)
                            }
                          />
                          {/* {HasPermission(LABELS.Can_Add_Or_Update_Device) && (
                          <Tooltip title={t("Manage_Device.Edit_camera")}>
                            <IconButton onClick={() => handleEdit(camera)}>
                              <img
                                src={
                                  theme === "light"
                                    ? "/images/edit.svg"
                                    : "/images/dark-theme/edit.svg"
                                }
                                alt="Edit Devices"
                              />
                            </IconButton>
                          </Tooltip>
                        )} */}
                          <IconButton
                            size="small"
                            sx={{ color: "black" }}
                            onClick={(e) => {
                              e.stopPropagation();
                              setMenuAnchor({
                                anchorEl: e.currentTarget,
                                item: camera,
                              });
                            }}
                          >
                            <MoreVert />
                          </IconButton>
                        </Box>
                      </Grid>
                      <CardContent>
                        <Grid className="table-grid-items-list">
                          <Grid item className="table-grid-items-half">
                            <Typography
                              variant="body2"
                              className="table-grid-items-label"
                            >
                              {t(
                                "Manage_Device.Manage_Device_Grid_column.Model",
                              )}
                            </Typography>
                            <Typography>{camera.model}</Typography>
                          </Grid>
                          <Grid item className="table-grid-items-half">
                            <Typography
                              variant="body2"
                              className="table-grid-items-label"
                            >
                              {t(
                                "Manage_Device.Manage_Device_Grid_column.Location",
                              )}
                            </Typography>
                            <Typography>{camera.location}</Typography>
                          </Grid>
                          <Grid item className="table-grid-items-half">
                            <Typography
                              variant="body2"
                              className="table-grid-items-label"
                            >
                              {t(
                                "Manage_Device.Manage_Device_Grid_column.macAddress",
                              )}
                            </Typography>
                            <Typography>{camera.macAddress}</Typography>
                          </Grid>
                          <Grid item className="table-grid-items-half">
                            <Typography
                              variant="body2"
                              className="table-grid-items-label"
                            >
                              {t(
                                "Manage_Device.Manage_Device_Grid_column.Serial_number",
                              )}
                            </Typography>
                            <Typography>{camera.serialNumber}</Typography>
                          </Grid>
                          <Grid item className="table-grid-items-half">
                            <Typography
                              variant="body2"
                              className="table-grid-items-label"
                            >
                              {t(
                                "Manage_Device.Manage_Device_Grid_column.Zones",
                              )}
                            </Typography>
                            <Typography>{camera.zoneNames}</Typography>
                          </Grid>
                          <Grid item className="table-grid-items-full">
                            <Typography
                              variant="body2"
                              className="table-grid-items-label"
                            >
                              {t(
                                "Manage_Device.Manage_Device_Grid_column.DeviceType",
                              )}
                            </Typography>
                            <Chip
                              label={camera.deviceType}
                              sx={getChipStyle(camera.deviceType)}
                            />
                          </Grid>
                          <Grid item className="table-grid-items-full">
                            <Typography
                              variant="body2"
                              className="table-grid-items-label"
                            >
                              {t(
                                "Manage_Device.Manage_Device_Grid_column.IP_Address",
                              )}
                            </Typography>
                            <Tooltip title={`http://${camera.ipAddress}`}>
                              <Typography
                                component="a"
                                href={`http://${camera.ipAddress}`}
                                target="_blank"
                                rel="noopener noreferrer"
                                sx={{
                                  color: "#1976d2",
                                  textDecoration: "underline",
                                  whiteSpace: "nowrap",
                                  overflow: "hidden",
                                  textOverflow: "ellipsis",
                                  display: "inline-block",
                                  maxWidth: "150px", // adjust width to fit your layout
                                  fontWeight: "600",
                                }}
                              >
                                {camera.ipAddress.length > 20
                                  ? `${camera.ipAddress.slice(0, 17)}...`
                                  : camera.ipAddress}
                              </Typography>
                            </Tooltip>
                          </Grid>
                        </Grid>
                      </CardContent>
                      {menuAnchor?.item?.id === camera.id && (
                        <Menu
                          // anchorEl={anchorEl}
                          anchorEl={menuAnchor.anchorEl}
                          open={open}
                          onClose={handleClose}
                          PaperProps={{
                            elevation: 3,
                            sx: {
                              borderRadius: 2,
                              pl: 1,
                              pr: 1,
                            },
                          }}
                          className="edit-delete-pop dit-delete-pop-grid"
                        >
                          {HasPermission(LABELS.UpdateMaintenanceDevice) &&
                            isANPR === "true" && camera.deviceType === "ANPR" && (
                              <MenuItem className="switch-grid">
                                <Switch
                                  checked={Boolean(camera.isMaintenance)}
                                  onChange={(event) =>
                                    handleToggle(
                                      camera.id!,
                                      event.target.checked,
                                    )
                                  }
                                  color="primary"
                                />
                                <ListItemText
                                  primary={t("sidebar.Maintenance")}
                                />
                              </MenuItem>
                            )}

                          {HasPermission(LABELS.Can_Add_Or_Update_Device) && (
                            <MenuItem onClick={() => handleEdit(camera)}>
                              <IconButton>
                                <img
                                  src={
                                    theme === "light"
                                      ? "/images/edit.svg"
                                      : "/images/dark-theme/edit.svg"
                                  }
                                  alt="Edit Devices"
                                />
                              </IconButton>
                              <ListItemText
                                primary={t("Common_Edit_Delte_Menu.Edit")}
                              />
                            </MenuItem>
                          )}

                          {HasPermission(LABELS.ManuallyOpenGate) &&
                            isANPR === "true" && camera.deviceType === "ANPR" && (
                              <MenuItem
                                onClick={() =>
                                  manuallyOpenGateBarrier(camera?.id as string)
                                }
                              >
                                <IconButton>
                                  <img
                                    src={
                                      theme === "light"
                                        ? "/images/stop.svg"
                                        : "/images/stop.svg" //"/images/dark-theme/edit.svg"
                                    }
                                    alt="Open Gate"
                                    width="20"
                                    height="20"
                                  />
                                </IconButton>
                                <ListItemText
                                  primary={t("Manage_Device.Open_Gate")}
                                />
                              </MenuItem>
                            )}
                          {HasPermission(LABELS.AuditLogMaster) &&
                            HasPermission(LABELS.ViewAuditLogsDeviceMaster) && (
                              <MenuItem
                                onClick={() =>
                                  ViewDeviceLogs(camera.id as string)
                                }
                              >
                                <IconButton>
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
                                <ListItemText
                                  primary={t("Audit_Log.Audit_Log_Header")}
                                />
                              </MenuItem>
                            )}
                        </Menu>
                      )}
                    </Card>
                  </Grid>
                );
              })
            ) : (
              <Box className="no-data-douns">
                <Box sx={{ width: 200, justifyItems: "center", flex: 1 }}>
                  <img
                    src={`/images/${themeColorPath}noData.gif`}
                    alt="Animated GIF"
                    width="100"
                    height="100"
                  />
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
            )}
          </Grid>
        </div>

        {cameraGridData.length > 0 && (
          <TablePagination
            component="div"
            count={TotalRecord}
            page={gridPaginationModel.page}
            rowsPerPage={gridPaginationModel.pageSize}
            onPageChange={(event, newPage) =>
              setGridPaginationModel((prev) => ({ ...prev, page: newPage }))
            }
            onRowsPerPageChange={(event) =>
              setGridPaginationModel({
                pageSize: parseInt(event.target.value, 10),
                page: 0,
              })
            }
            rowsPerPageOptions={[4, 8, 12]}
          />
        )}
      </>
    );
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
        {/* <Typography
            sx={{ FontWeight: 400, fontSize: 12, color: "#212121" }}
          >
           No data available to display Users. Please add a new user by clicking the <strong>Add New User</strong> button.
          </Typography> */}
      </Box>
    </Box>
  );

  const handleSampleExcelDownload = async () => {
    try {
      await SampleExcelDownloadService();
    } catch (err: any) {}
  };

  const handleDeviceCSVDownload = async () => {
    const sortBy = sortModel[0]?.field || "deviceName";
    const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
    let deviceIds: string[];
    try {
      if (displayView === "gridview") {
        // need to add condition as reload is common for both gridview and listview
        deviceIds = cameraGridData.map((device) => device.id!);
      } else {
        deviceIds = cameraListData.map((device) => device.id!);
      }
      let request: IGetAllDeviceRequest = {
        searchText: searchTerm.length > 0 ? searchTerm : "",
        pageNo: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        deviceIds: deviceIds,
        sortBy: sortBy,
        sortOrder: sortOrder,
      };
      await ExportDeviceCSVService({ data: request });
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };

  return (
    <>
      {/* <Container sx={{ mt: 4, width: "100%" }}> */}
      <div className="main-dashbourd-wrapper">
        <div
          className="top-orange-head manage-orange-head"
          style={backgroundStyle}
        >
          <Box className="top-orange-head-left">
            <Typography variant="h4">
              {t("Manage_Device.Manage_Device_header")}
            </Typography>
            <Typography>
              {t("Manage_Device.Manage_Device_Description")}
            </Typography>
          </Box>
          <Button onClick={handleDeviceCSVDownload}>
            <img src="images/csv.svg" alt="" />&nbsp;
            {t("LPR_Screen.CSV")}
          </Button>
          {HasPermission(LABELS.Can_Add_Or_Update_Device) && (
            <ButtonGroup
              size="large"
              aria-label="Large button group"
              variant="outlined"
            >
              <Button onClick={() => setIsOpenBulkUploadDeviceDrawer(true)}>
                <AddIcon style={{ color: "#090909b5" }} />
                {t("Manage_Device.Upload_Bulk_Device_btn")}
              </Button>
              <Button onClick={handleSampleExcelDownload}>
                <VerticalAlignBottomIcon style={{ color: "#090909b5" }} />
                {t("Manage_Device.Sample_File")}
              </Button>
            </ButtonGroup>
          )}
          {HasPermission(LABELS.Can_Add_Or_Update_Device) && (
            <CustomButton
              size="small"
              variant="outlined"
              onClick={handleAddCamera}
              // onClick={handleAddDevices}
            >
              <img src={"/images/adddevice.svg"} alt="Add Devices" />
              {t("Manage_Device.Add_Devices_btn")}
            </CustomButton>
          )}
        </div>

        <div className="top-list-bar">
          <Typography variant="h5" gutterBottom>
            {t("Manage_Device.List_Of_Devices")}
          </Typography>

          <div className="top-listing-items">
            <TextField
              placeholder={t("Manage_Device.Device_Search_Placeholder_text")}
              variant="outlined"
              // size="small"
              value={searchTerm}
              onChange={handleSearch}
              sx={{ borderRadius: 8 }}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <img
                      src={"/images/search.svg"}
                      alt="Search"
                      style={{ cursor: "pointer" }}
                    />
                  </InputAdornment>
                ),
              }}
              className="top-list-margin"
            />

            {cameraListData && cameraListData.length > 0 && (
              <CustomButton
                variant="outlined"
                onClick={() => fetchListCameraData(true)}
              >
                <img src={"/images/reload.svg"} alt="Reload Devices" />
                {t("Manage_Device.Reload_btn")}
              </CustomButton>
            )}

            {HasPermission(LABELS.Can_Delete_Device) && (
              <CustomButton
                variant="outlined"
                size="small"
                onClick={() => handleDelete()}
                disabled={selectedDevices.length > 0 ? false : true}
              >
                {selectedDevices.length > 0 ? (
                  <img src={"/images/remove.svg"} alt="Remove Devices" />
                ) : (
                  <img
                    src={"/images/remove_disabled.svg"}
                    alt="Remove Devices disabled"
                  />
                )}
                {t("Manage_Device.Remove_btn")}
              </CustomButton>
            )}

            <ToggleButtonGroup
              exclusive
              value={displayView}
              onChange={changeDisaplayView}
              className="list-and-grid top-list-margin"
            >
              <ToggleButton
                className="list-button"
                value="listview"
                aria-label="listview"
              >
                <img src={"/images/ListView.svg"} alt="listview" />{" "}
                {t("Manage_Device.List_view")}
              </ToggleButton>
              <ToggleButton
                className="grid-buttons"
                value="gridview"
                aria-label="gridview"
              >
                <img src={"/images/grid-orange.svg"} alt="gridview" />
                {t("Manage_Device.Grid_view")}
              </ToggleButton>
            </ToggleButtonGroup>
          </div>
        </div>

        {displayView === "gridview" ? (
          CameraGrid()
        ) : (
          <DataGrid
            rows={cameraListData}
            columns={columns}
            getRowId={(row) => row.id}
            paginationModel={paginationModel}
            onPaginationModelChange={handlePaginationModelChange}
            rowCount={cameraListData.length === 0 ? 0 : TotalRecord}
            paginationMode="server"
            pageSizeOptions={[5, 10, 15, 20, 25]}
            checkboxSelection
            onRowSelectionModelChange={(ids) => {
              setSelectedDevices(ids as string[]);
            }}
            sortingMode="server"
            sortModel={sortModel}
            onSortModelChange={handleSortModelChange}
            disableRowSelectionOnClick
            slots={{
              noRowsOverlay: CustomNoRowsOverlay,
            }}
            hideFooter={cameraListData.length === 0}
            localeText={localeText}
            columnVisibilityModel={{
              isMaintenance: HasPermission(LABELS.UpdateMaintenanceDevice) && isANPR === "true"  ,
            }}
          />
        )}
      </div>

      {/* <CommonDialog
        open={openAddCamera}
        title={"Add New Camera"}
        content={
          <DeviceAddEdit
            onClose={handleCloseAddModal}
            refreshData={refreshCameraData}
          />
        }
        onCancel={handleCloseAddModal}
      //cancelText="Cancel"
      /> */}

      <Drawer
        anchor={"right"}
        open={openAddCamera}
        onClose={() => {
          //  reset();
          setOpenAddCamera(false);
        }}
        ModalProps={{
          onClose: (event, reason) => {
            if (reason !== "backdropClick") {
              setOpenAddCamera(false);
            }
          },
        }}
        className="cmn-pop"
      >
        <Box className="cmn-pop-head">
          {/* Title on Left */}
          <Typography variant="h6">{t("Manage_Device.Add_Device")}</Typography>

          {/* Close Icon on Right */}
          <IconButton onClick={() => setOpenAddCamera(false)}>
            <GridCloseIcon />
          </IconButton>
        </Box>

        <DeviceAddEdit
          onClose={handleCloseAddModal}
          refreshData={refreshCameraData}
        />
      </Drawer>

      {/* <CommonDialog
        open={openEditCamera}
        title={"Edit Camera"}
        content={
          <DeviceAddEdit
            cameras={selectedCamera}
            onClose={handleCloseEditModal}
            refreshData={refreshCameraData}
          />
        }
        onCancel={handleCloseEditModal}
      //cancelText="Cancel"
      /> */}

      <Drawer
        anchor={"right"}
        open={openEditCamera}
        onClose={() => {
          //  reset();
          setOpenEditCamera(false);
        }}
        PaperProps={{
          sx: {
            borderRadius: "20px 0 0 20px",
          },
        }}
        ModalProps={{
          onClose: (event, reason) => {
            if (reason !== "backdropClick") {
              setOpenEditCamera(false);
            }
          },
        }}
        className="cmn-pop"
      >
        <Box className="cmn-pop-head">
          {/* Title on Left */}
          <Typography variant="h6">{t("Manage_Device.Edit_camera")}</Typography>

          {/* Close Icon on Right */}
          <IconButton onClick={() => setOpenEditCamera(false)}>
            <GridCloseIcon />
          </IconButton>
        </Box>

        <DeviceAddEdit
          cameras={selectedCamera}
          onClose={handleCloseEditModal}
          refreshData={refreshCameraData}
        />
      </Drawer>

      <Drawer
        anchor="right"
        open={isOpenBulkUploadDeviceDrawer}
        onClose={handleCloseDrawer}
        className="cmn-pop"
        ModalProps={{
          onClose: (event, reason) => {
            if (reason !== "backdropClick") {
              setIsOpenBulkUploadDeviceDrawer(false);
            }
          },
        }}
      >
        <Box className="cmn-pop-head">
          <Typography variant="h6">
            {t("Manage_Device.Upload_Bulk_Device_Drawer.Upload_Device")}{" "}
          </Typography>
          <IconButton onClick={handleCloseDrawer}>
            <GridCloseIcon />
          </IconButton>
        </Box>
        <BulkUploadDevice
          onClose={handleCloseDrawer}
          refreshData={refreshCameraData}
        />
      </Drawer>

      <CommonDialog
        open={openDeleteConfirm}
        title={t("Common_DELETE_Confirmation_Dialog.Title")}
        content={
          <div>
            <p>{t("Common_DELETE_Confirmation_Dialog.Content")}</p>

            <p className="deleted-colors-text">
              {t(
                "Common_DELETE_Confirmation_Dialog.Delete_Device_Content.Line1_Part1",
              )}{" "}
              <span style={{ color: "#f57c00" }}>
                {t(
                  "Common_DELETE_Confirmation_Dialog.Delete_Device_Content.Line1_Highlight",
                )}
              </span>{" "}
              {t(
                "Common_DELETE_Confirmation_Dialog.Delete_Device_Content.Line1_Part2",
              )}{" "}
              <span style={{ color: "#f57c00" }}>
                {t(
                  "Common_DELETE_Confirmation_Dialog.Delete_Device_Content.Line1_Highlight2",
                )}
              </span>{" "}
              {t(
                "Common_DELETE_Confirmation_Dialog.Delete_Device_Content.Line1_Part3",
              )}
            </p>
          </div>
        }
        onConfirm={() => selectedDevices.length > 0 && finalDeleteDevices()}
        type="delete"
        onCancel={handleCloseConfirm}
        confirmText={t("Common_DELETE_Confirmation_Dialog.Delete")}
        cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
        titleClass={true}
        customClass="cmn-confirm-delete-icon cmn-confirm-delete-icon-new"
      />
    </>
  );
};

export default DeviceManagement;
