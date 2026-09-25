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
  Button,
  ButtonGroup,
} from "@mui/material";
import {
  DataGrid,
  GridCloseIcon,
  GridColDef,
  GridPaginationModel,
  GridSortModel,
} from "@mui/x-data-grid";
import { AddEditOwner } from "./AddEditOwner";
import { Vehicle } from "./Vehicle";
import { IGetAllOwnerRequestProps, IOwnerList } from "../../interfaces/IANPR";
import {
  DeleteOwnerService,
  GetAllVehicleOwnerService,
  OwnerSampleExcelDownloadService,
  VehicleSampleExcelDownloadService,
} from "../../services/anprService";
import { CustomButton } from "../../components/Reusable/CustomButton";
import { HasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { useThemeContext } from "../../context/ThemeContext";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { formatDate } from "../../utils/dateUtils";
import { CommonDialog } from "../../components";
import { ICommonId } from "../../interfaces/ILookup";
import { useNavigate } from "react-router-dom";
import AddIcon from "@mui/icons-material/Add";
import VerticalAlignBottomIcon from "@mui/icons-material/VerticalAlignBottom";
import { useTranslation } from "react-i18next";
import { BulkUploadOwner } from "./BulkUploadOwner";
import { BulkUploadVehicle } from "./BulkUploadVehicle";

const OwnersPage: React.FC = () => {
  const [addOwnerDrawer, setAddOwnerDrawer] = useState(false);
  const [ownerList, setOwnerList] = useState<IOwnerList[]>([]);
  const [TotalRecord, SetTotalRecord] = useState<number>(0);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    pageSize: 10,
    page: 0,
  });
  const [sortModel, setSortModel] = useState<GridSortModel>([
    { field: "createdOn", sort: "desc" },
  ]);
  const [selectedOwner, setSelectedOwner] = useState<IOwnerList | undefined>(
    undefined,
  );
  const [openVehicleModel, setOpenVehicleModel] = useState(false);
  const [openDeleteConfirm, setOpenDeleteConfirm] = useState(false);
  const [ownerToBeDelete, setOwnerToBeDelete] = useState<ICommonId | null>(
    null,
  );
  const { theme, themeColor } = useThemeContext();
  const { timeFormat } = useTimeFormatContext();
  const [isOpenBulkUploadOwnerDrawer, setIsOpenBulkUploadOwnerDrawer] =
    useState(false);
  const [isOpenBulkUploadVehicleDrawer, setIsOpenBulkUploadVehicleDrawer] =
    useState(false);
  const backgroundStyle = {
    backgroundImage: ` url('/images/lines.png'), linear-gradient(287.68deg, #FE6500 -0.05%, #FF8A00 57.77%)`,
    backgroundSize: "cover",
    backgroundRepeat: "no-repeat",
    backgroundPosition: "center",
  };

  const navigate = useNavigate();
  const ViewAuditLogs = async (collectionId: string) => {
    navigate("/audit", {
      state: { id: collectionId, collectionName: "vehicleOwner" },
    });
  };
  const { t } = useTranslation();

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
      field: "ownerName",
      headerName: t("ANPR_Screen.Owner.Owner_Name"),
      filterable: false,
      width: 200
    },
    {
      field: "registrationType",
      headerName: t("ANPR_Screen.Owner.Registration_Type"),
      filterable: false,
      width: 200,
      renderCell: (params) => {
        return <Box style={{ textTransform: "capitalize" }}>{params.value}</Box>;
      },
    },
    {
      field: "allowedVehicle",
      headerName: t("ANPR_Screen.Owner.Allowed_Vehicle"),
      filterable: false,
      width: 200
    },
    {
      field: "building",
      headerName: t("ANPR_Screen.Owner.Building"),
      filterable: false,
       width: 200
    },
    {
      field: "buildingUnit",
      headerName: t("ANPR_Screen.Owner.Building_Unit"),
      filterable: false,
      width: 200
    },
    {
      field: "contactNumber",
      //width:100,
      headerName: t("ANPR_Screen.Owner.Contact_Number"),
      filterable: false,
    },
    {
      field: "email",
      width:200,
      headerName: t("ANPR_Screen.Owner.Email"),
      filterable: false,
    },
    // {
    //   field: "allowedGateNames",
    //   headerName: t("ANPR_Screen.Owner.Allowed_Gates"),
    //   width: 200,
    //   renderCell: (params) => {
    //     return (
    //       <Box>
    //         {(params.value as string[]).map((item) => {
    //           return item ? (
    //             <Chip
    //               key={item}
    //               label={item}
    //               size="small"
    //               // color="warning"
    //               sx={{ color: "#F4731F", borderColor: "#F4731F" }}
    //               variant="outlined"
    //               style={{ margin: 2 }}
    //               className="role-chip"
    //             />
    //           ) : null;
    //         })}
    //       </Box>
    //     );
    //   },
    // },
    {
      field: "allowedGateNames",
      headerName: t("ANPR_Screen.Owner.Allowed_Gates"),
      width: 250,
      sortable:false,
      renderCell: (params) => {
        const gates = (params.value as string[]) || [];

        const containerRef = React.useRef<HTMLDivElement>(null);
        const [visibleCount, setVisibleCount] = React.useState(0);
        const [openDialog, setOpenDialog] = React.useState(false);
        const [searchText, setSearchText] = React.useState("");

        const CHIP_MAX_WIDTH = 120;
        const MORE_CHIP_WIDTH = 70;

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

            setVisibleCount(Math.min(maxChips, gates.length));
          });

          observer.observe(containerRef.current);
          return () => observer.disconnect();
        }, [gates.length]);

        const visibleGates = visibleCount > 0 ? gates.slice(0, visibleCount) : [];
        const remainingGates = gates.slice(visibleGates.length);
        const remainingCount = remainingGates.length;

        const gateRows = React.useMemo(() => {
          return remainingGates
            .map((gate, index) => ({
              id: index + 1,
              srNo: index + 1,
              gateName: gate,
            }))
            .filter((row) =>
              row.gateName.toLowerCase().includes(searchText.toLowerCase()),
            );
        }, [remainingGates, searchText]);

        const gateColumns = [
          {
            field: "srNo",
            headerName: t("Maintanace_Plan.Serial_No"),
            width: 80,
            sortable: false,
          },
          {
            field: "gateName",
            headerName: t("ANPR_Screen.Owner.Allowed_Gates"),
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
              {/* Visible Chips */}
              {visibleGates.map((item) => (
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
                  sx={{
                    maxWidth: CHIP_MAX_WIDTH,
                    color: "#F4731F",
                    borderColor: "#F4731F",
                    flexShrink: 0,
                  }}
                  size="small"
                  variant="outlined"
                  className="role-chip"
                />
              ))}

              {/* +MORE */}
              {remainingCount > 0 && (
                <Chip
                  label={`+${remainingCount} More`}
                  size="small"
                  onClick={() => setOpenDialog(true)}
                  sx={{
                    backgroundColor: "#F4731F",
                    color: "#fff",
                    flexShrink: 0,
                    cursor: "pointer",
                  }}
                  className="role-chip-more"
                />
              )}
            </Box>

            {/* Dialog */}
            <CommonDialog
              open={openDialog}
              title={t("ANPR_Screen.Owner.Allowed_Gates")}
              onCancel={() => {
                setOpenDialog(false);
                setSearchText("");
              }}
              showCloseIcon
              cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
              confirmText=""
              content={
                <Box>
                  <TextField
                    fullWidth
                    size="small"
                    placeholder={t("ANPR_Screen.Owner.Search_Gates")}
                    value={searchText}
                    onChange={(e) => setSearchText(e.target.value)}
                    sx={{ mb: 1 }}
                  />

                  <Box sx={{ height: "max-content", width: "100%" }}>
                    <DataGrid
                      rows={gateRows}
                      columns={gateColumns}
                      hideFooter
                      disableColumnMenu
                      disableRowSelectionOnClick
                      rowHeight={42}
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
      field: "enabledAlarmFor24HStay",
      headerName: t("ANPR_Screen.Owner.Alarm_For_24H_Stay"),
      width: 100,
      renderCell: (params) => {
        return <Box>{params.value === true ? "YES" : "NO"}</Box>;
      },
    },
    {
      field: "enabledAlarmForOverstay",
      headerName: t("ANPR_Screen.Owner.Alarm_For_Over_Stay"),
      width: 100,
      renderCell: (params) => {
        return <Box>{params.value === true ? "YES" : "NO"}</Box>;
      },
    },

    {
      field: "allowedFromTime",
      headerName: t("ANPR_Screen.Owner.Allowed_Time"),
      width: 250,
      filterable: false,
      sortable: false,
      renderCell: (params) => {
        const from = formatDateToConfiguredTimezone(params.row.allowedFromTime);
        const to = formatDateToConfiguredTimezone(params.row.allowedToTime);

        const fromFormatted = formatDate(from, timeFormat);
        const toFormatted = formatDate(to, timeFormat);

        const fromTimeOnly = fromFormatted.slice(
          fromFormatted.indexOf(" ") + 1,
        );
        const toTimeOnly = toFormatted.slice(toFormatted.indexOf(" ") + 1);

        return (
          <Box>
            {fromTimeOnly} - {toTimeOnly}
          </Box>
        );
      },
    },

    {
      field: "ownerValidTo",
      headerName: t("ANPR_Screen.Owner.ValidTo"),
      width: 220,
      filterable: false,
      renderCell: (params) => {
        const convertedValidTo = formatDateToConfiguredTimezone(params.value);
        const formattedValidTo = formatDate(convertedValidTo, timeFormat);
        if (!formattedValidTo || formattedValidTo === "N/A") {
          return <span>N/A</span>;
        }
        const validToDateOnly = formattedValidTo.split(" ")[0];
        return <span>{validToDateOnly}</span>;
      },
    },

    {
      field: "actions",
      headerName: t("ANPR_Screen.Owner.Actions"),
      width: 170,
      sortable: false,
      filterable: false,
      renderCell: (params) => (
        <Box>
          {HasPermission(LABELS.Add_or_Update_Owner_Details) && (
            <Tooltip title={t("ANPR_Screen.Owner.Edit_Owner")}>
              <IconButton onClick={() => handleEdit(params.row)}>
                <img
                  src={
                    theme === "light"
                      ? "/images/edit.svg"
                      : "/images/dark-theme/edit.svg"
                  }
                  alt="Edit Owner"
                  width="20"
                  height="20"
                />
              </IconButton>
            </Tooltip>
          )}
          {HasPermission(LABELS.Delete_an_Existing_Vehicle_Owner) && (
            <Tooltip title={t("ANPR_Screen.Owner.Delete_Owner")}>
              <IconButton onClick={() => handleDelete(params.id as string)}>
                <img
                  src={"/images/user-action-delete.svg"}
                  alt="Delete Owner"
                  width={20}
                  height={20}
                />
              </IconButton>
            </Tooltip>
          )}
          {(HasPermission(LABELS.View_List_of_Vehicles) ||
            HasPermission(LABELS.Add_or_Update_Vehicle_Details)) && (
            <Tooltip title={t("ANPR_Screen.Owner.Add_Vehicle")}>
              <IconButton
                onClick={() => {
                  setOpenVehicleModel(true);
                  setSelectedOwner(params.row);
                }}
              >
                <img
                  src={
                      theme === "light"
                        ? "/images/user-action-vehicle.svg"
                        : "/images/dark-theme/user-action-vehicle.svg"
                    }
                  alt="Vehicle"
                  width={20}
                  height={20}
                />
              </IconButton>
            </Tooltip>
          )}
          {HasPermission(LABELS.AuditLogMaster) &&
            HasPermission(LABELS.ViewAuditLogsvehicleOwner) && (
              <Tooltip title={t("ANPR_Screen.Owner.View_Audit_logs")}>
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

  useEffect(() => {
    const delayDebounce = setTimeout(() => {
      if (searchTerm.length >= 3 || searchTerm.length === 0) {
        fetchVehicleOwnerData();
      }
    }, 500);

    return () => clearTimeout(delayDebounce);
  }, [paginationModel, sortModel, searchTerm]);

  const fetchVehicleOwnerData = async () => {
    const sortBy = sortModel[0]?.field || "createdOn";
    const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
    try {
      let request: IGetAllOwnerRequestProps = {
        searchText: searchTerm.length > 0 ? searchTerm : "",
        pageNumber: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        sortBy: sortBy,
        sortOrder: sortOrder,
      };
      const vehicleOwnerData: any = await GetAllVehicleOwnerService(request);
      if (vehicleOwnerData && vehicleOwnerData.isSuccess) {
        setOwnerList(vehicleOwnerData?.data?.allVehicleOwnerLists);
        SetTotalRecord(vehicleOwnerData.data.totalCount);
      } else {
        setOwnerList([]);
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
  };

  const handleDelete = (ownerid: string) => {
    setOwnerToBeDelete({ id: ownerid });
    setOpenDeleteConfirm(true);
  };

  const handleSampleExcelDownload = async () => {
    console.log("sample file download");
    try {
      await OwnerSampleExcelDownloadService();
    } catch (err: any) {}
  };

  const handleVehicleSampleExcelDownload = async () => {
    console.log("sample file download");
    try {
      await VehicleSampleExcelDownloadService();
    } catch (err: any) {}
  };

  const handleVehicleuploadCloseDrawer = () => {
    setIsOpenBulkUploadVehicleDrawer(false);
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

  const handleEdit = (owner: IOwnerList) => {
    setSelectedOwner(owner);
    setAddOwnerDrawer(true);
  };

  const handleCloseVehicleModal = () => {
    setOpenVehicleModel(false);
    setSelectedOwner(undefined);
  };

  const finalDeleteOwner = async (id: string) => {
    const param = {
      id: id,
    };
    try {
      const deleteData: any = await DeleteOwnerService(param);
      if (deleteData.isSuccess) {
        setOpenDeleteConfirm(false);
        fetchVehicleOwnerData();
      }
    } catch (err: any) {}
  };

  const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    const searchValue = event.target.value;
    setSearchTerm(searchValue);
  };

  const handleCloseDrawer = () => {
    setIsOpenBulkUploadOwnerDrawer(false);
  };

  return (
    <>
      {/* <Container sx={{ mt: 4, width: "100%" }}> */}
      <div className="main-dashbourd-wrapper ">
        <div className="top-orange-head owners-page-orange-head" style={backgroundStyle}>
          <Box className="top-orange-head-left ">
            <Typography variant="h4">
              {t("ANPR_Screen.Owner.Vehicle_Owners_title")}
            </Typography>
            <Typography>
              {t("ANPR_Screen.Owner.Vehicle_Owners_description")}
            </Typography>
          </Box>
          {HasPermission(LABELS.Add_or_Update_Owner_Details) && (
            <ButtonGroup
              size="large"
              aria-label="Large button group"
              variant="outlined"
              className="bulk-group-btn"
            >
              <Button onClick={() => setIsOpenBulkUploadOwnerDrawer(true)}>
                <AddIcon style={{ color: "#090909b5" }} />
                {t("ANPR_Screen.Owner.Owner_Bulk_Upload")}
              </Button>
              <Button onClick={handleSampleExcelDownload}>
                <VerticalAlignBottomIcon style={{ color: "#090909b5" }} />
                {t("ANPR_Screen.Owner.Owner_Sample_file")}
              </Button>
            </ButtonGroup>
          )}

          {HasPermission(LABELS.Add_or_Update_Vehicle_Details) && (
            <ButtonGroup
              size="large"
              aria-label="Large button group"
              variant="outlined"
              className="bulk-group-btn-right"
            // className="bulk-group-btn"
            >
              <Button onClick={() => setIsOpenBulkUploadVehicleDrawer(true)}>
                <AddIcon style={{ color: "#090909b5" }} />
                {t("ANPR_Screen.Owner.Vehicle_Bulk_Upload")}
              </Button>
              <Button onClick={handleVehicleSampleExcelDownload}>
                <VerticalAlignBottomIcon style={{ color: "#090909b5" }} />
                {t("ANPR_Screen.Owner.Vehicle_Sample_File")}
              </Button>
            </ButtonGroup>
          )}

          {HasPermission(LABELS.Add_or_Update_Owner_Details) && (
            <CustomButton
              size="small"
              variant="outlined"
              onClick={() => setAddOwnerDrawer(true)}
            >
              <img src={"/images/adddevice.svg"} alt="Add Devices" />
              {t("ANPR_Screen.Owner.Add_Owner")}
            </CustomButton>
          )}
        </div>

        <div className="top-list-bar">
          <Typography variant="h5" gutterBottom>
            {t("ANPR_Screen.Owner.List_Of_Owners")}
          </Typography>

          <div className="top-listing-items">
            <TextField
              placeholder={t("ANPR_Screen.Owner.Search_Placeholder")}
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
          rows={ownerList}
          columns={columns}
          getRowId={(row) => row.id}
          paginationModel={paginationModel}
          onPaginationModelChange={handlePaginationModelChange}
          rowCount={ownerList.length === 0 ? 0 : TotalRecord}
          paginationMode="server"
          pageSizeOptions={[5, 10, 15, 20, 25]}
          sortingMode="server"
          sortModel={sortModel}
          onSortModelChange={handleSortModelChange}
          disableRowSelectionOnClick
          slots={{
            noRowsOverlay: CustomNoRowsOverlay,
          }}
          hideFooter={ownerList.length === 0}
        />
      </div>

      <Drawer
        anchor={"right"}
        open={addOwnerDrawer}
        onClose={() => {
          setAddOwnerDrawer(false);
        }}
        ModalProps={{
          onClose: (_, reason) => {
            if (reason !== "backdropClick") {
              setAddOwnerDrawer(false);
            }
          },
        }}
        className="cmn-pop"
      >
        <Box className="cmn-pop-head">
          <Typography variant="h6">
            {selectedOwner ? t("ANPR_Screen.Owner.Update_Owner") : t("ANPR_Screen.Owner.Add_Owner")}
          </Typography>
          <IconButton
            onClick={() => {
              setAddOwnerDrawer(false);
              setSelectedOwner(undefined);
            }}
          >
            <GridCloseIcon />
          </IconButton>
        </Box>

        <AddEditOwner
          onClose={() => {
            setAddOwnerDrawer(false);
          }}
          refreshData={() => {
            fetchVehicleOwnerData();
            setSelectedOwner(undefined);
          }}
          ownerData={selectedOwner}
        />
      </Drawer>

      <CommonDialog
        open={openVehicleModel}
        // title={`Vehicle for ${selectedOwner?.ownerName}`}
        title={t("ANPR_Screen.Owner.Vehicle_for", { name: selectedOwner?.ownerName })}
        content={<Vehicle selectedOwner={selectedOwner} />}
        onCancel={handleCloseVehicleModal}
        maxWidth={false}
        fullWidth={true}
        customClass={"license-detail-content-pop"}
      />

      <CommonDialog
        open={openDeleteConfirm}
        title={t("Common_DELETE_Confirmation_Dialog.Title")}
        customClass="cmn-confirm-delete-icon"
        content={t("Common_DELETE_Confirmation_Dialog.Content")}
        onConfirm={() =>
          ownerToBeDelete && finalDeleteOwner(ownerToBeDelete.id)
        }
        onCancel={() => setOpenDeleteConfirm(false)}
        confirmText={t("Common_DELETE_Confirmation_Dialog.Delete")}
        cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
        type="delete"
        titleClass={true}
        showCloseIcon={true}
      />

      <Drawer
        anchor="right"
        open={isOpenBulkUploadOwnerDrawer}
        onClose={handleCloseDrawer}
        className="cmn-pop"
        ModalProps={{
          onClose: (event, reason) => {
            if (reason !== "backdropClick") {
              handleCloseDrawer();
            }
          },
        }}
      >
        <Box className="cmn-pop-head">
          <Typography variant="h6">{t("ANPR_Screen.Owner.Upload_Owner")}</Typography>
          <IconButton onClick={handleCloseDrawer}>
            <GridCloseIcon />
          </IconButton>
        </Box>
        <BulkUploadOwner
          onClose={handleCloseDrawer}
          refreshData={() => {
            fetchVehicleOwnerData();
            setSelectedOwner(undefined);
          }}
        />
      </Drawer>

      <Drawer
        anchor="right"
        open={isOpenBulkUploadVehicleDrawer}
        onClose={handleCloseDrawer}
        className="cmn-pop"
        ModalProps={{
          onClose: (event, reason) => {
            if (reason !== "backdropClick") {
              handleCloseDrawer();
            }
          },
        }}
        sx={{
          zIndex: 9000,
        }}
      >
        <Box className="cmn-pop-head">
          <Typography variant="h6">{t("ANPR_Screen.Owner.Upload_Vehicle")}</Typography>
          <IconButton onClick={handleVehicleuploadCloseDrawer}>
            <GridCloseIcon />
          </IconButton>
        </Box>
        <BulkUploadVehicle
          onClose={handleVehicleuploadCloseDrawer}
          refreshData={() => {
            fetchVehicleOwnerData();
          }}
        />
      </Drawer>
    </>
  );
};

export default OwnersPage;
