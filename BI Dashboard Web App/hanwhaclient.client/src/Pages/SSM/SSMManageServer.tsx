import React, { useEffect, useState } from 'react'
import { Box, Container, Drawer, IconButton, InputAdornment, TextField, Tooltip, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { CommonDialog, CustomButton } from '../../components';
import { HasPermission } from '../../utils/screenAccessUtils';
import { LABELS } from '../../utils/constants';
import { DataGrid, GridCloseIcon, GridColDef } from '@mui/x-data-grid';
import { ISSMServersList } from '../../interfaces/IManageServer';
import SSMServerAddEditForm from './SSMServerAddEditForm';
import { IReferenceData } from '../../interfaces/IManageiDRAC';
import { ICommonId } from '../../interfaces/ILookup';
import { useThemeContext } from '../../context/ThemeContext';
import { useTimeFormatContext } from '../../context/TimeFormatContext';
import { DeleteSsmService, GetAllSsmService } from '../../services/SSMService';
import { formatDateToConfiguredTimezone } from '../../utils/formatDateToConfiguredTimezone';
import { formatDate } from '../../utils/dateUtils';
import { useNavigate } from 'react-router-dom';


const SSMManageServer = () => {
  const [openAddServer, setOpenAddServer] = useState<boolean>(false);
  const [selectedServer, setSelectedServer] = useState<ISSMServersList | undefined>();
  const [ssmServers, setSsmServers] = useState<ISSMServersList[]>([]);
  const [referenceData, setReferenceData] = useState<IReferenceData>();
  const [searchTerm, setSearchTerm] = useState<string>("");
  const { t } = useTranslation();
  const [openEditSsm, setOpenEditSsm] = useState<boolean>(false);
  // const [selectedSsm, setSelectedSsm] = useState<ISSMServersList | undefined>(
  //   undefined
  // );
  const [openDeleteConfirm, setOpenDeleteConfirm] = useState(false);
  const [serverToBeDelete, setServerToBeDelete] = useState<ICommonId | null>(null);
  const [paginationModel, setPaginationModel] = useState({
    pageSize: 10,
    page: 0,
  });
  const { theme, themeColor } = useThemeContext();
  const { timeFormat } = useTimeFormatContext();
  const navigate = useNavigate();

  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
        ? `${themeColor}/dark-theme/`
        : `${themeColor}/`;
  
  const backgroundStyle = {
    backgroundImage: ` url('/images/lines.png'), linear-gradient(287.68deg, #FE6500 -0.05%, #FF8A00 57.77%)`,
    backgroundSize: 'cover',
    backgroundRepeat: 'no-repeat',
    backgroundPosition: 'center',
  };

  useEffect(() => {
    fetchInitialData();
  }, []);


  const fetchInitialData = async () => {
    try {
      const ssmData: any = await GetAllSsmService();
      setSsmServers(ssmData.data as ISSMServersList[]);
      setReferenceData(ssmData.referenceData);
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };


  const handleAddServer = () => {
    setOpenAddServer(true);
    setSelectedServer(undefined);
  };

  const handleCloseAddDrawer = () => {
    setOpenAddServer(false);
    setSelectedServer(undefined);
  };

  const handleEdit = (server: ISSMServersList) => {
    setSelectedServer(server);
    setOpenEditSsm(true);
  };


  const handleDelete = (serverID: string) => {
    setServerToBeDelete({ id: serverID });
    setOpenDeleteConfirm(true);
  };

   const handleCloseEditDrawer = () => {
    setOpenEditSsm(false);
    setSelectedServer(undefined);
  };

  const finalDeleteServer = async (id: string) => {
    try {
      const deleteData: any = await DeleteSsmService({ id });
      if (deleteData.isSuccess) {
        setOpenDeleteConfirm(false);
        fetchInitialData();
      }
    } catch (err: any) { }
  };
  const handleCloseConfirm = () => {
    setOpenDeleteConfirm(false);
  };
  const getLabel = (list: any[] = [], value: string | null) => {
    return list.find((item) => item.value === value)?.label || "";
  };

  const filteredServer =
    searchTerm.trim().length >= 2
      ? ssmServers.filter((server) => {
        const search = searchTerm.toLowerCase();

        const parentName = getLabel(referenceData?.parentSite, server.parentSiteId);
        const childName = getLabel(referenceData?.childSite, server.childSiteId);

        return (
          server.ipAddress?.toLowerCase().includes(search) ||
          server.port?.toString().toLowerCase().includes(search) ||
          server.username?.toLowerCase().includes(search) ||
          parentName.toLowerCase().includes(search) ||
          childName.toLowerCase().includes(search)
        );
      })
      : ssmServers;

  const CustomNoRowsOverlay = () => (
    <Box className="no-data-douns"
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

  const ViewAuditLogs = async (collectionId: string) => {
    navigate("/audit", { state: { id: collectionId, collectionName: "ssmSiteMapping" } });
  }

  const columns: GridColDef[] = [
    { field: "ipAddress", headerName: t("Manage_SSM.Grid_Column.IP_Address"), flex: 1 },
    { field: "port", headerName: t("Manage_SSM.Grid_Column.Port"), flex: 1 },
    {
      field: "parentSiteId", headerName: t("Manage_SSM.Grid_Column.Parent_Site"), flex: 1,
      renderCell: (params) => {
        const parentID = params.row.parentSiteId as string;
        const filteredData = referenceData?.parentSite?.find(
          (item) => item.value === parentID
        );
        return <Box>{filteredData && <div>{filteredData.label}</div>}</Box>;
      },
    },
    {
      field: "childSiteId", headerName: t("Manage_SSM.Grid_Column.Child_Site"), flex: 1,
      renderCell: (params) => {
        const parentID = params.row.childSiteId as string;
        const filteredData = referenceData?.childSite?.find(
          (item) => item.value === parentID
        );
        return <Box>{filteredData && <div>{filteredData.label}</div>}</Box>;
      },
    },
    { field: "username", headerName: t("Manage_User.Manage_User_Grid_column.Username"), flex: 1 },

    {
      field: "createdBy",
      headerName: t("Manage_User.Manage_User_Grid_column.Created_By"),
      flex: 1,
      renderCell: (params) => {
        const createdByID = params.row.createdBy as string;
        const filteredData = referenceData?.createdBy?.find(
          (item) => item.value === createdByID
        );
        return <Box>{filteredData && <div>{filteredData.label}</div>}</Box>;
      },
    },
    {
      field: "createdOn",
      headerName: t("Manage_User.Manage_User_Grid_column.Created_On"),
      flex: 1,
      renderCell: (params) => {
        const convertedDateTime = formatDateToConfiguredTimezone(params.value);
        return <span>{formatDate(convertedDateTime, timeFormat)}</span>;
      },
    },
    {
      field: "updatedBy",
      headerName: t("Manage_User.Manage_User_Grid_column.Updated_By"),
      flex: 1,
      renderCell: (params) => {
        const updatedById = params.row.updatedBy as string;
        const filteredData = referenceData?.updatedBy?.find(
          (item) => item.value === updatedById
        );
        return <Box>{filteredData && <div>{filteredData.label}</div>}</Box>;
      },
    },
    {
      field: "updatedOn",
      headerName: t("Manage_User.Manage_User_Grid_column.Updated_On"),
      flex: 1,
      renderCell: (params) => {
        const convertedDateTime = formatDateToConfiguredTimezone(params.value);
        return <span>{formatDate(convertedDateTime, timeFormat)}</span>;
      },
    },
    {
      field: "actions",
      headerName: t("Manage_User.Manage_User_Grid_column.Actions"),
      flex: 1,
      sortable: false,
      renderCell: (params) => (
        <Box>
          {HasPermission(LABELS.AddOrUpdateSsmServers) && (
            <Tooltip title={t("My_Report.Edit")}>
              <IconButton onClick={() => handleEdit(params.row)}>
                <img
                  src={"/images/user-action-edit.svg"}
                  alt="Edit Icon"
                  width={20}
                  height={20}
                />
              </IconButton>
            </Tooltip>
          )}
          {HasPermission(LABELS.DeleteSsmServers) && (
            <Tooltip title={t("My_Report.Delete")}>
              <IconButton onClick={() => handleDelete(params.id as string)}>
                <img
                  src={"/images/user-action-delete.svg"}
                  alt="Delete Icon"
                  width={20}
                  height={20}
                />
              </IconButton>
            </Tooltip>
          )
          }
          {(HasPermission(LABELS.AuditLogMaster) &&
            HasPermission(LABELS.ViewAuditLogSSMServer)) && (
              <Tooltip title="View Audit logs">

                <IconButton onClick={() => ViewAuditLogs(params.id as string)}>
                  <img
                    src={"/images/audit_history.png"}
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

  const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(event.target.value);
  };

 

   return (
    <>
      <Container sx={{}} maxWidth={false}>
        <div className="top-orange-head" style={backgroundStyle} >
          <Box className="top-orange-head-left">
            <Typography variant="h4">
              {t("Manage_SSM.Manage_Server")}
            </Typography>
            <Typography>
              {t("Manage_SSM.description")}
            </Typography>
          </Box>

          {HasPermission(LABELS.AddOrUpdateSsmServers) && (
            <CustomButton
              size="small"
              variant="outlined"
              customStyles={{
                background: "#FFFFFF",
                color: "#090909",
                borderRadius: 8,
              }}
              onClick={handleAddServer}
            >
              <img src={"/images/adddevice.svg"} alt="Add Server" />{t("Manage_SSM.Add_Server")}
            </CustomButton>
          )}
        </div>

        <Box
          className="top-list-bar"
        >


          <h5>{t("Manage_SSM.List_Of_Server")}</h5>
           <div className="top-listing-items">

             <TextField
               variant="outlined"
               placeholder={t("Manage_SSM.Search_Placeholder")}
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
              
        </Box>

         <DataGrid
           rows={filteredServer}
           columns={columns}
           getRowId={(row) => row.id}
           pagination
           paginationModel={paginationModel}
           onPaginationModelChange={setPaginationModel}
           pageSizeOptions={[10, 20, 50, 100]}
           className="user-screen"
           slots={{
             noRowsOverlay: CustomNoRowsOverlay,
           }}
           hideFooter={ssmServers.length === 0}
           localeText={localeText}
           disableRowSelectionOnClick
         />

        <Drawer
          anchor={"right"}
          open={openAddServer}
          onClose={() => {
            handleCloseAddDrawer();
          }}
          ModalProps={{
            onClose: (event, reason) => {
              if (reason !== 'backdropClick') {
                // onClose(); // only call your close function on non-backdrop events
                handleCloseAddDrawer();
              }
            }
          }}
          className="cmn-pop"
        >
          <Box className="cmn-pop-head">
            <Typography variant="h6">{t("Manage_SSM.Add_Server")}</Typography>
            
            <IconButton onClick={handleCloseAddDrawer}>
              <GridCloseIcon />
            </IconButton>
          </Box>
          <SSMServerAddEditForm
            onClose={handleCloseAddDrawer}
            refreshData={fetchInitialData}
          />
        </Drawer>

         <Drawer
          anchor={"right"}
          open={openEditSsm}
          onClose={() => {
            handleCloseEditDrawer();
          }}
          ModalProps={{
            onClose: (event, reason) => {
              if (reason !== 'backdropClick') {
                // onClose(); // only call your close function on non-backdrop events
                handleCloseEditDrawer();
              }
            }
          }}
          className="cmn-pop"
        >
          <Box className="cmn-pop-head">
            <Typography variant="h6">{t("Manage_SSM.Edit_Server")}</Typography>
            <IconButton onClick={handleCloseEditDrawer}>
              <GridCloseIcon />
            </IconButton>
          </Box>
          <SSMServerAddEditForm
            onClose={handleCloseEditDrawer}
            refreshData={fetchInitialData}
            server={selectedServer}
          />
        </Drawer>

         <CommonDialog
           open={openDeleteConfirm}
           title={t("Common_DELETE_Confirmation_Dialog.Title")}
           content={t("Common_DELETE_Confirmation_Dialog.Content")}
           customClass="cmn-confirm-delete-icon"
           onConfirm={() => serverToBeDelete && finalDeleteServer(serverToBeDelete.id)}
           onCancel={handleCloseConfirm}
           confirmText={t("Common_DELETE_Confirmation_Dialog.Delete")}
           cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
           type="delete"
           titleClass={true}
           showCloseIcon={true}
         />
      </Container>

    
    </>
  );
}

export default SSMManageServer