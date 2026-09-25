import React, { useState, useEffect } from "react";
import {
  Typography,
  TextField,
  Box,
  IconButton,
  Tooltip,
  Chip,
  InputAdornment,
  Drawer,
} from "@mui/material";
import {
  DataGrid,
  GridCloseIcon,
  GridColDef,
  GridPaginationModel,
  GridSortModel,
} from "@mui/x-data-grid";
import { AddEditPlan } from "./AddEditPlan";
import {
  GetMaintenancePlanService,
  DeletePlanService,
} from "../../services/maintenanceService";
import { CustomButton } from "../../components/Reusable/CustomButton";
import { HasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { useThemeContext } from "../../context/ThemeContext";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { formatDate } from "../../utils/dateUtils";
import { CommonDialog } from "../../components";
import { ICommonId } from "../../interfaces/ILookup";
import {
  IGetAllPlanRequestProps,
  IPlanProps,
} from "../../interfaces/IMaintenance";
import { useTranslation } from "react-i18next";

const MaintenancePlan: React.FC = () => {
  const [addPlanDrawer, setAddPlanDrawer] = useState(false);
  const [editPlanDrawer, setEditPlanDrawer] = useState(false);
  const [planList, setPlanList] = useState<IPlanProps[]>([]);
  const [TotalRecord, SetTotalRecord] = useState<number>(0);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    pageSize: 10,
    page: 0,
  });
  const [sortModel, setSortModel] = useState<GridSortModel>([
    { field: "createdOn", sort: "desc" },
  ]);
  const [selectedPlan, setSelectedPlan] = useState<IPlanProps | undefined>(
    undefined,
  );
  const [openDeleteConfirm, setOpenDeleteConfirm] = useState(false);
  const [planToBeDelete, setPlanToBeDelete] = useState<ICommonId | null>(null);
  const { theme, themeColor } = useThemeContext();
  const { timeFormat } = useTimeFormatContext();
  const { t } = useTranslation();
  const backgroundStyle = {
    backgroundImage: ` url('/images/lines.png'), linear-gradient(287.68deg, #FE6500 -0.05%, #FF8A00 57.77%)`,
    backgroundSize: "cover",
    backgroundRepeat: "no-repeat",
    backgroundPosition: "center",
  };

  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
        ? `${themeColor}/dark-theme/`
        : `${themeColor}/`;

  const columns: GridColDef[] = [
    {
      field: "planName",
      headerName: t("Maintanace_Plan.Maintanace_Plan_Grid.Plan_Name"),
      width: 250,
      filterable: false,
    },
    {
      field: "duration",
      headerName: t("Maintanace_Plan.Maintanace_Plan_Grid.Duration"),
      width: 200,
      filterable: false,
    },
    {
      field: "deviceNames",
      headerName: t("Maintanace_Plan.Maintanace_Plan_Grid.Device"),
      width: 450,
      renderCell: (params) => {
        const devices = (params.value as string[]) || [];

        const containerRef = React.useRef<HTMLDivElement>(null);
        const [visibleCount, setVisibleCount] = React.useState(0);
        const [openDialog, setOpenDialog] = React.useState(false);
        const [searchText, setSearchText] = React.useState("");

        const CHIP_MAX_WIDTH = 140;
        const MORE_CHIP_WIDTH = 80;

        React.useEffect(() => {
          if (!containerRef.current) return;

          const observer = new ResizeObserver(([entry]) => {
            const width = entry.contentRect.width;

            if (width < CHIP_MAX_WIDTH + MORE_CHIP_WIDTH) {
              setVisibleCount(0);
              return;
            }

            const availableWidth = width - MORE_CHIP_WIDTH;
            const maxChips = Math.floor(availableWidth / CHIP_MAX_WIDTH);

            setVisibleCount(Math.min(maxChips, devices.length));
          });

          observer.observe(containerRef.current);
          return () => observer.disconnect();
        }, [devices.length]);

        const visibleDevices =
          visibleCount > 0 ? devices.slice(0, visibleCount) : [];

        const remainingDevices = devices.slice(visibleDevices.length);
        const remainingCount = remainingDevices.length;

        const deviceRows = React.useMemo(() => {
          return devices
            .slice(visibleDevices.length)          // recompute remaining inline
            .map((device, index) => ({
              id: index + 1,
              srNo: index + 1,
              deviceName: device,
            }))
            .filter((row) =>
              row.deviceName.toLowerCase().includes(searchText.toLowerCase()),
            );
        }, [devices, visibleCount, searchText]);

        const deviceColumns = [
          {
            field: "srNo",
            headerName: t("Maintanace_Plan.Serial_No"),
            width: 100,
            sortable: false,
          },
          {
            field: "deviceName",
            headerName: t(
              "Manage_Device.Manage_Device_Grid_column.Device_name",
            ),
            flex: 1,
            sortable: false,
            renderCell: (params: any) => (
              <Box
                sx={{
                  whiteSpace: "nowrap",
                  overflow: "hidden",
                  textOverflow: "ellipsis",
                  width: "100%",
                }}
                title={params.value}
              >
                {params.value}
              </Box>
            ),
          },
        ];

        return (
          <>
            <Box
              ref={containerRef}
              sx={{
                display: "flex",
                alignItems: "center",
                gap: 0.5,
                height: "100%",
                maxWidth: "100%",
                overflow: "hidden",
                whiteSpace: "nowrap",
              }}
              className="role-chip"
            >
              {/* Visible chips */}
              {visibleDevices.map((item) => (
                <Chip
                  key={item}
                  label={
                    <Box
                      sx={{
                        maxWidth: CHIP_MAX_WIDTH,
                        overflow: "hidden",
                        textOverflow: "ellipsis",
                        whiteSpace: "nowrap",
                      }}
                      title={item}
                    >
                      {item}
                    </Box>
                  }
                  size="small"
                  variant="outlined"
                  sx={{
                    maxWidth: CHIP_MAX_WIDTH,
                    color: "#F4731F",
                    borderColor: "#F4731F",
                    flexShrink: 0,
                  }}
                  className="role-chip"
                />
              ))}

              {/* +MORE */}
              {remainingCount > 0 && (
                <>
                  <Chip
                    label={t("Maintanace_Plan.More_Count", {
                      count: remainingCount,
                    })}
                    size="small"
                    onClick={(e) => setOpenDialog(true)}
                    sx={{
                      backgroundColor: "#F4731F",
                      color: "#fff",
                      flexShrink: 0,
                      cursor: "pointer",
                    }}
                    className="role-chip-more"
                  />
                </>
              )}
            </Box>

            <CommonDialog
              customClass="maintenace-plan-device-dialog-main"
              open={openDialog}
              title={t("Maintanace_Plan.Device_Dialog_header")}
              onCancel={() => {
                setOpenDialog(false);
                setSearchText("");
              }}
              showCloseIcon={true}
              cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
              confirmText=""
              content={
                <Box>
                  <TextField
                    fullWidth
                    size="small"
                    placeholder={t("Maintanace_Plan.Search_Devices")}
                    value={searchText}
                    onChange={(e) => setSearchText(e.target.value)}
                    sx={{ mb: 1 }}
                  />

                  <Box sx={{ height: "max-content", width: "100%" }}>
                    <DataGrid
                      rows={deviceRows}
                      columns={deviceColumns}
                      hideFooter
                      disableColumnMenu
                      disableRowSelectionOnClick
                      rowHeight={42}
                      slots={{
                        noRowsOverlay: () => (
                          <Box
                            sx={{
                              height: "100%",
                              display: "flex",
                              alignItems: "center",
                              justifyContent: "center",
                              color: "text.secondary",
                              fontSize: 14,
                            }}
                          >
                            No devices found
                          </Box>
                        ),
                      }}
                      sx={{
                        border: "none",
                        "& .MuiDataGrid-columnHeaders": {
                          fontWeight: 600,
                        },
                      }}
                    />
                  </Box>
                </Box>
              }
            />
          </>
        );
      },
    },

    {
      field: "startDate",
      headerName: t("Maintanace_Plan.Maintanace_Plan_Grid.Start_Date"),
      width: 220,
      filterable: false,
      renderCell: (params) => {
        const convertedStartDate = formatDateToConfiguredTimezone(params.value);
        const formattedStartDate = formatDate(convertedStartDate, timeFormat);
        const startDateOnly = formattedStartDate.slice(
          0,
          formattedStartDate.indexOf(" "),
        );
        return <span>{startDateOnly}</span>;
      },
    },
    {
      field: "endDate",
      headerName: t("Maintanace_Plan.Maintanace_Plan_Grid.End_Date"),
      width: 220,
      filterable: false,
      renderCell: (params) => {
        const convertedEndDate = formatDateToConfiguredTimezone(params.value);
        const formattedEndDate = formatDate(convertedEndDate, timeFormat);
        const endDateOnly = formattedEndDate.slice(
          0,
          formattedEndDate.indexOf(" "),
        );
        return <span>{endDateOnly}</span>;
      },
    },
    {
      field: "actions",
      headerName: t("Maintanace_Plan.Maintanace_Plan_Grid.Actions"),
      width: 100,
      sortable: false,
      filterable: false,
      renderCell: (params) => (
        <Box>
          {HasPermission(LABELS.AddOrUpdateMaintenancePlan) && (
            <Tooltip
              title={t(
                "Maintanace_Plan.Maintanace_Plan_Grid.Edit_Plan_Tooltip",
              )}
            >
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
          {HasPermission(LABELS.DeleteMaintenancePlan) && (
            <Tooltip
              title={t(
                "Maintanace_Plan.Maintanace_Plan_Grid.Delete_Plan_Tooltip",
              )}
            >
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

  useEffect(() => {
    const delayDebounce = setTimeout(() => {
      if (searchTerm.length >= 3 || searchTerm.length === 0) {
        fetchPlanData();
      }
    }, 500);

    return () => clearTimeout(delayDebounce);
  }, [paginationModel, sortModel, searchTerm]);

  const fetchPlanData = async () => {
    const sortBy = sortModel[0]?.field || "createdOn";
    const sortOrder =
      sortModel[0]?.sort && sortModel[0]?.sort === "asc" ? 1 : -1;
    try {
      let request: IGetAllPlanRequestProps = {
        searchText: searchTerm.length > 0 ? searchTerm : "",
        pageNumber: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        sortBy: sortBy,
        sortOrder: sortOrder,
      };
      const planData: any = await GetMaintenancePlanService(request);

      if (planData && planData?.isSuccess && planData?.data) {
        setPlanList(planData?.data?.items);
        SetTotalRecord(planData?.data?.totalCount);
      } else {
        setPlanList([]);
        SetTotalRecord(0);
      }
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
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

  const handleSortModelChange = (newSortModel: GridSortModel) => {
    setSortModel(newSortModel);
    // setSortModel(newSortModel.length ? newSortModel : [
    //   { field: "createdOn", sort: "desc" },
    // ]);
  };

  const handleDelete = (planId: string) => {
    setPlanToBeDelete({ id: planId });
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
        `${from}–${to} ${t("MUI_Grid_Filter_Paginition.footerTotalVisibleRows", { total: count })}`,
    },

    footerRowSelected: (count: number) =>
      t("MUI_Grid_Filter_Paginition.footerRowSelected", { count }),

    footerRowSelectedPlural: (count: number) =>
      t("MUI_Grid_Filter_Paginition.footerRowSelectedPlural", { count }),
  };
  const handleEdit = (plan: IPlanProps) => {
    setSelectedPlan(plan);
    setEditPlanDrawer(true);
  };

  const handleDeletePlan = async (id: string) => {
    const param = {
      id: id,
    };
    try {
      const deleteData: any = await DeletePlanService(param);
      if (deleteData.isSuccess) {
        setOpenDeleteConfirm(false);
        fetchPlanData();
      }
    } catch (err: any) {}
  };

  const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    const searchValue = event.target.value;
    setSearchTerm(searchValue);
  };

  return (
    <>
      {/* <Container sx={{ mt: 4, width: "100%" }}> */}
      <div className="main-dashbourd-wrapper">
        <div className="top-orange-head" style={backgroundStyle}>
          <Box className="top-orange-head-left">
            <Typography variant="h4">
              {t("Maintanace_Plan.Maintenance_Plan_Header")}
            </Typography>
            <Typography>
              {t("Maintanace_Plan.Maintenance_Plan_Description")}
            </Typography>
          </Box>
          {HasPermission(LABELS.AddOrUpdateMaintenancePlan) && (
            <CustomButton
              size="small"
              variant="outlined"
              onClick={() => setAddPlanDrawer(true)}
            >
              <img src={"/images/adddevice.svg"} alt="Add Devices" />
              {t("Maintanace_Plan.Add_Plan")}
            </CustomButton>
          )}
        </div>

        <div className="top-list-bar">
          <Typography variant="h5" gutterBottom>
            {t("Maintanace_Plan.List_Of_Plans")}
          </Typography>

          <div className="top-listing-items">
            <TextField
              placeholder={t("Maintanace_Plan.Search_Placeholder")}
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
            />
          </div>
        </div>

        <DataGrid
          rows={planList}
          columns={columns}
          getRowId={(row) => row.id}
          paginationModel={paginationModel}
          onPaginationModelChange={handlePaginationModelChange}
          rowCount={planList.length === 0 ? 0 : TotalRecord}
          paginationMode="server"
          pageSizeOptions={[5, 10, 15, 20, 25]}
          sortingMode="server"
          sortModel={sortModel}
          onSortModelChange={handleSortModelChange}
          disableRowSelectionOnClick
          slots={{
            noRowsOverlay: CustomNoRowsOverlay,
          }}
          hideFooter={planList.length === 0}
          localeText={localeText}
        />
      </div>

      <Drawer
        anchor={"right"}
        open={addPlanDrawer}
        onClose={() => {
          setAddPlanDrawer(false);
        }}
        ModalProps={{
          onClose: (_, reason) => {
            if (reason !== "backdropClick") {
              setAddPlanDrawer(false);
            }
          },
        }}
        className="cmn-pop"
      >
        <Box className="cmn-pop-head">
          <Typography variant="h6">{t("Maintanace_Plan.Add_Plan")}</Typography>
          <IconButton
            onClick={() => {
              setAddPlanDrawer(false);
              setSelectedPlan(undefined);
            }}
          >
            <GridCloseIcon />
          </IconButton>
        </Box>

        <AddEditPlan
          onClose={() => {
            setAddPlanDrawer(false);
          }}
          refreshData={() => {
            fetchPlanData();
          }}
        />
      </Drawer>

      <Drawer
        anchor={"right"}
        open={editPlanDrawer}
        onClose={() => {
          setEditPlanDrawer(false);
        }}
        ModalProps={{
          onClose: (_, reason) => {
            if (reason !== "backdropClick") {
              setEditPlanDrawer(false);
            }
          },
        }}
        className="cmn-pop"
      >
        <Box className="cmn-pop-head">
          <Typography variant="h6">{t("Maintanace_Plan.Edit_Plan")}</Typography>
          <IconButton
            onClick={() => {
              setEditPlanDrawer(false);
              setSelectedPlan(undefined);
            }}
          >
            <GridCloseIcon />
          </IconButton>
        </Box>

        <AddEditPlan
          onClose={() => {
            setEditPlanDrawer(false);
          }}
          refreshData={() => {
            fetchPlanData();
          }}
          planData={selectedPlan}
        />
      </Drawer>

      <CommonDialog
        open={openDeleteConfirm}
        title="Delete Confirmation!"
        customClass="cmn-confirm-delete-icon"
        content="Are you sure you want to continue?"
        onConfirm={() => planToBeDelete && handleDeletePlan(planToBeDelete.id)}
        onCancel={() => setOpenDeleteConfirm(false)}
        confirmText="Delete"
        cancelText="Cancel"
        type="delete"
        titleClass={true}
        showCloseIcon={true}
      />
    </>
  );
};

export default MaintenancePlan;
