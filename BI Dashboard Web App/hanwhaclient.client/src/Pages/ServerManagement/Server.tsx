import React, { useState, useEffect } from "react";
import {
  Typography,
  TextField,
  Box,
  IconButton,
  Tooltip,
  InputAdornment,
  Drawer,
  Switch,
} from "@mui/material";
import {
  DataGrid,
  GridCloseIcon,
  GridColDef,
  GridPaginationModel,
  GridSortModel,
} from "@mui/x-data-grid";
import { AddEditServer } from "./AddEditServer";
import {
  DeleteServerService,
  GetServerListService,
  UpdateServerStatusService,
} from "../../services/settingService";
import { CustomButton } from "../../components/Reusable/CustomButton";
import { HasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { useThemeContext } from "../../context/ThemeContext";
import { CommonDialog, showToast } from "../../components";
import { ICommonId } from "../../interfaces/ILookup";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  IServerManagement,
  IUpdateServerStstus,
} from "../../interfaces/ISettings";
import { Trans } from "react-i18next";

const Server: React.FC = () => {
  const [openServerDrawer, setOpenServerDrawer] = useState(false);
  const [serverList, setServerList] = useState<IServerManagement[]>([]);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    pageSize: 10,
    page: 0,
  });
  const [sortModel, setSortModel] = useState<GridSortModel>([
    { field: "createdOn", sort: "desc" },
  ]);
  const [selectedServer, setSelectedServer] = useState<
    IServerManagement | undefined
  >(undefined);
  const [openDeleteConfirm, setOpenDeleteConfirm] = useState(false);
  const [serverToBeDelete, setServerToBeDelete] = useState<ICommonId | null>(
    null,
  );
  const [openStatusConfirm, setOpenStatusConfirm] = useState(false);
  const [pendingStatusChange, setPendingStatusChange] = useState<{
    id: string;
    isActive: boolean;
  } | null>(null);
  const { theme, themeColor } = useThemeContext();

  const backgroundStyle = {
    backgroundImage: ` url('/images/lines.png'), linear-gradient(287.68deg, #FE6500 -0.05%, #FF8A00 57.77%)`,
    backgroundSize: "cover",
    backgroundRepeat: "no-repeat",
    backgroundPosition: "center",
  };

  const navigate = useNavigate();
  const ViewAuditLogs = async (collectionId: string) => {
    navigate("/audit", {
      state: { id: collectionId, collectionName: "viMultiServerManagement" },
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

  useEffect(() => {
    fetchServerData();
  }, []);

  const columns: GridColDef[] = [
    {
      field: "serverName",
      headerName: t("ServerManagement.Server_Name"),
      filterable: false,
      width: 200,
    },
    {
      field: "databaseConnectionString",
      headerName: t("ServerManagement.Database_Connection_String"),
      filterable: false,
      width: 700,
    },
    {
      field: "hostingAddress",
      headerName: t("ServerManagement.Hosting_Address"),
      filterable: false,
      width: 400,
    },
    // {
    //   field: "username",
    //   headerName: "User Name",
    //   filterable: false,
    //    width: 200
    // },
    // {
    //   field: "password",
    //   headerName: "Password",
    //   filterable: false,
    //   width: 200
    // },
    {
      field: "isAvailable",
      headerName: t("ServerManagement.IsAvailable"),
      filterable: false,
      width: 200,
      renderCell: (params) => (
        <span
          style={{
            color: params.value ? "#2DB400" : "#FF0000",
          }}
        >
          {params.value ? "Online" : "Offline"}
        </span>
      ),
    },
  ...(HasPermission(LABELS.EnabledMultiServers)
    ? [
    {
      field: "isActive",
      headerName: t("ServerManagement.IsActive"),
      width: 180,
      sortable: false,
      filterable: false,
      renderCell: (params) => {
        return (
          <Switch
            checked={Boolean(params.row.isActive)}
            disabled={!params.row.isAvailable}
            onChange={(event) => {
              setPendingStatusChange({
                id: params.row.id,
                isActive: event.target.checked,
              });
              setOpenStatusConfirm(true);
            }}
            color="primary"
          />
        );
      },
    },
     ]
    : []),
    {
      field: "actions",
      headerName: t("ServerManagement.Actions"),
      width: 170,
      sortable: false,
      filterable: false,
      renderCell: (params) => (
        <Box>
          {HasPermission(LABELS.AddOrUpdateMultiServers) && (
            <Tooltip title={t("ServerManagement.Edit_Server")}>
              <IconButton onClick={() => handleEdit(params.row)}>
                <img
                  src={
                    theme === "light"
                      ? "/images/edit.svg"
                      : "/images/dark-theme/edit.svg"
                  }
                  alt="Edit Server"
                  width="20"
                  height="20"
                />
              </IconButton>
            </Tooltip>
          )}
          {HasPermission(LABELS.DeleteMultiServers) && (
            <Tooltip title={t("ServerManagement.Delete_Server")}>
              <IconButton onClick={() => handleDelete(params.id as string)}>
                <img
                  src={"/images/user-action-delete.svg"}
                  alt="Delete Server"
                  width={20}
                  height={20}
                />
              </IconButton>
            </Tooltip>
          )}
          {HasPermission(LABELS.AuditLogMaster) &&
            HasPermission(LABELS.ViewAuditLogMultiServers) && (
              <Tooltip title={t("ServerManagement.View_Audit_logs")}>
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

  const handleToggle = async (serverId: string, updatedValue: boolean) => {
    // optimistic UI update for DataGrid
    setServerList((prev) =>
      prev.map((row) =>
        row.id === serverId ? { ...row, isActive: updatedValue } : row,
      ),
    );

    const res = await updateServerStatus(serverId, updatedValue);

    if (res === false) {
      // rollback DataGrid
      setServerList((prev) =>
        prev.map((row) =>
          row.id === serverId ? { ...row, isActive: !updatedValue } : row,
        ),
      );
    }
  };

  const handleConfirmStatusChange = async () => {
    if (!pendingStatusChange) return;

    await handleToggle(
      pendingStatusChange.id,
      pendingStatusChange.isActive
    );

    setPendingStatusChange(null);
    setOpenStatusConfirm(false);
  };

  const handleCancelStatusChange = () => {
    setOpenStatusConfirm(false);
    setPendingStatusChange(null);
  };

  const updateServerStatus = async (
    serverId: string,
    updatedValue: boolean,
  ) => {
    try {
      const UpdatedData = {
        id: serverId,
        isActive: updatedValue,
      };
      const response = await UpdateServerStatusService(
        UpdatedData as IUpdateServerStstus,
      );

      if (typeof response === "string") {
        showToast(response, "error");
        return false;
      }

      if ((response?.data as any) === false) {
        showToast(response?.message, "error");
        return false;
      } else {
        showToast(response?.message, "success");
        return true;
      }
    } catch (err: any) {
      console.error("Error in update server status:", err);
      return false;
    }
  };
  const fetchServerData = async () => {
    try {
      const serverData: any = await GetServerListService();
      if (serverData && serverData.length > 0) {
        setServerList(serverData);
      } else {
        setServerList([]);
      }
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };

  const handlePaginationModelChange = (
    newPaginationModel: GridPaginationModel,
  ) => {
    const isPageSizeChanged =
      newPaginationModel.pageSize !== paginationModel.pageSize;
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

  const handleDelete = (serverId: string) => {
    setServerToBeDelete({ id: serverId });
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

  const handleEdit = (server: IServerManagement) => {
    setSelectedServer(server);
    setOpenServerDrawer(true);
  };

  const finalDeleteServer = async (id: string) => {
    const param = {
      id: id,
    };
    try {
      const deleteData: any = await DeleteServerService(param);
      if (deleteData.isSuccess) {
        setOpenDeleteConfirm(false);
        fetchServerData();
      }
    } catch (err: any) {}
  };

  const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    const searchValue = event.target.value;
    setSearchTerm(searchValue);
  };

  const filteredRows = React.useMemo(() => {
    if (!searchTerm) return serverList;
    if (searchTerm.length < 3) return serverList;
    const lowerSearch = searchTerm.toLowerCase();

    return serverList.filter(
      (row) =>
        row.serverName?.toLowerCase().includes(lowerSearch) ||
        row.databaseConnectionString?.toLowerCase().includes(lowerSearch) ||
        row.hostingAddress?.toLowerCase().includes(lowerSearch),
    );
  }, [serverList, searchTerm]);

  return (
    <>
      <div className="main-dashbourd-wrapper ">
        <div
          className="top-orange-head server-page-orange-head"
          style={backgroundStyle}
        >
          <Box className="top-orange-head-left ">
            <Typography variant="h4">
              {t("ServerManagement.ServerManagement_Title")}
            </Typography>
            <Typography>
              {t("ServerManagement.ServerManagement_SubTitle")}
            </Typography>
          </Box>

          {HasPermission(LABELS.AddOrUpdateMultiServers) && (
            <CustomButton
              size="small"
              variant="outlined"
              onClick={() => setOpenServerDrawer(true)}
            >
              <img src={"/images/adddevice.svg"} alt="Add Server" />
              {t("ServerManagement.Add_Server")}
            </CustomButton>
          )}
        </div>

        <div className="top-list-bar">
          <Typography variant="h5" gutterBottom>
            {t("ServerManagement.List_Of_Servers")}
          </Typography>

          <div className="top-listing-items">
            <TextField
              placeholder={t("ServerManagement.Search_Server_Placeholder")}
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
          rows={filteredRows}
          columns={columns}
          getRowId={(row) => row.id}
          paginationModel={paginationModel}
          onPaginationModelChange={handlePaginationModelChange}
          pageSizeOptions={[5, 10, 15, 20, 25]}
          sortModel={sortModel}
          onSortModelChange={handleSortModelChange}
          disableRowSelectionOnClick
          slots={{
            noRowsOverlay: CustomNoRowsOverlay,
          }}
          hideFooter={serverList.length === 0}
        />
      </div>

      <Drawer
        anchor={"right"}
        open={openServerDrawer}
        onClose={() => {
          setOpenServerDrawer(false);
        }}
        ModalProps={{
          onClose: (_, reason) => {
            if (reason !== "backdropClick") {
              setOpenServerDrawer(false);
            }
          },
        }}
        className="cmn-pop"
      >
        <Box className="cmn-pop-head">
          <Typography variant="h6">
            {selectedServer ? t("ServerManagement.Edit_Server") : t("ServerManagement.Add_Server")}
          </Typography>
          <IconButton
            onClick={() => {
              setOpenServerDrawer(false);
              setSelectedServer(undefined);
            }}
          >
            <GridCloseIcon />
          </IconButton>
        </Box>

        <AddEditServer
          onClose={() => {
            setOpenServerDrawer(false);
          }}
          refreshData={() => {
            fetchServerData();
            setSelectedServer(undefined);
          }}
          serverData={selectedServer}
        />
      </Drawer>

      <CommonDialog
        open={openDeleteConfirm}
        title={t("Common_DELETE_Confirmation_Dialog.Title")}
        customClass="cmn-confirm-delete-icon"
        content={t("Common_DELETE_Confirmation_Dialog.Content")}
        onConfirm={() =>
          serverToBeDelete && finalDeleteServer(serverToBeDelete.id)
        }
        onCancel={() => setOpenDeleteConfirm(false)}
        confirmText={t("Common_DELETE_Confirmation_Dialog.Delete")}
        cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
        type="delete"
        titleClass={true}
        showCloseIcon={true}
      />

      <CommonDialog
        open={openStatusConfirm}
        title={t("ServerManagement.Status_Confirmation_Title")}
        content={
          <Trans
            i18nKey={
              pendingStatusChange?.isActive
                ? "ServerManagement.Activate_Confirmation"
                : "ServerManagement.Deactivate_Confirmation"
            }
            components={{
              1: <strong />,
            }}
          />
        }
        customClass="server-confirmation-pop"
        onConfirm={handleConfirmStatusChange}
        onCancel={handleCancelStatusChange}
        confirmText={t("ServerManagement.Yes")}
        cancelText={t("ServerManagement.No")}
        // type="warning"
      />
    </>
  );
};

export default Server;
