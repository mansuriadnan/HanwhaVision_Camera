import { Box, Container, Drawer, IconButton, InputAdornment, TextField, Tooltip, Typography } from '@mui/material';
import React, { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next';
import { HasPermission } from '../../utils/screenAccessUtils';
import { CommonDialog, CustomButton } from '../../components';
import { LABELS } from '../../utils/constants';
import { DataGrid, GridCloseIcon, GridColDef } from '@mui/x-data-grid';
import { IAlarmData, IdeleteAlarmPayload, IiDRACDetail, IiDRACServer, IReferenceData } from '../../interfaces/IManageiDRAC';
import IDRACAddEditForm from './IDRACAddEditForm';
import { useThemeContext } from '../../context/ThemeContext';
import { formatDateToConfiguredTimezone } from '../../utils/formatDateToConfiguredTimezone';
import { formatDate } from '../../utils/dateUtils';
import { useTimeFormatContext } from '../../context/TimeFormatContext';
import { DeleteAlarmService, DeleteiDRACService, GetAlliDRACService } from '../../services/iDRACService';
import { ICommonId } from '../../interfaces/ILookup';
import IDRACAddAlarmForm from './IDRACAddAlarmForm';
import { useNavigate } from 'react-router-dom';

const ManageiDRAC = () => {
  const [iDRACServers, setiDRACServers] = useState<IiDRACServer[]>([]);
  const [referenceData, setReferenceData] = useState<IReferenceData>();
  const [searchTerm, setSearchTerm] = useState<string>("");
  const { t } = useTranslation();
  const [openAddiDRAC, setOpenAddiDRAC] = useState<boolean>(false);
  const [openEditiDRAC, setOpenEditiDRAC] = useState<boolean>(false);
  const [selectediDRAC, setSelectediDRAC] = useState<IiDRACServer | undefined>(
    undefined
  );
  const [openDeleteConfirm, setOpenDeleteConfirm] = useState(false);
  const [serverToBeDelete, setServerToBeDelete] = useState<ICommonId | null>(null);
  const [paginationModel, setPaginationModel] = useState({
    pageSize: 10,
    page: 0,
  });
  const [openAddAlarm, setopenAddAlarm] = useState<boolean>(false);
  const { theme, themeColor } = useThemeContext();
  const { timeFormat } = useTimeFormatContext();
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedAlarms, setSelectedAlarms] = useState<IAlarmData[]>([]);
  const [alarmToBeDelete, setAlarmToBeDelete] = useState<IdeleteAlarmPayload | null>(null);
  const [openDeleteAlarmConfirm, setOpenDeleteAlarmConfirm] = useState(false);
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
        const iDRACData: any = await GetAlliDRACService();
        setiDRACServers(iDRACData.data as IiDRACServer[]);
        setReferenceData(iDRACData.referenceData);
      } catch (err: any) {
        console.error("Error fetching initial data:", err);
      }
    };
  
    const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
      setSearchTerm(event.target.value);
    };

  const getLabel = (list: any[] = [], value: string | null) => {
    return list.find((item) => item.value === value)?.label || "";
  };
const filteredServer =
  searchTerm.trim().length >= 2
    ? iDRACServers.filter((server) => {
        const search = searchTerm.toLowerCase();

        const parentName = getLabel(referenceData?.parentSite, server.parentSiteId);
        const childName = getLabel(referenceData?.childSite, server.childSiteId);

        return (
          server.ipAddress?.toLowerCase().includes(search) ||
          server.port?.toString().toLowerCase().includes(search) ||
          server.userName?.toLowerCase().includes(search) ||
          parentName.toLowerCase().includes(search) ||
          childName.toLowerCase().includes(search)
        );
      })
    : iDRACServers;

 
  const ViewAuditLogs = async (collectionId: string) => {
    navigate("/audit", {
      state: { id: collectionId, collectionName: "iDRACMaster" },
    });
  };

  const columns: GridColDef[] = [
    { field: "ipAddress",headerName: t("IDRAC_Server.Grid.IP_Address"), width:150 },
    { field: "port", headerName: t("IDRAC_Server.Grid.Port"), width:100 },
    { field: "serverName", headerName: t("IDRAC_Server.Grid.Server_Name"),  width:150 },   
    { field: "parentSiteId",  headerName: t("IDRAC_Server.Grid.Parent_Site"),  width:150,
       renderCell: (params) => {
        const parentID = params.row.parentSiteId as string;
        const filteredData = referenceData?.parentSite?.find(
          (item) => item.value === parentID
        );
        return <Box>{filteredData && <div>{filteredData.label}</div>}</Box>;
      },
    },
    { field: "childSiteId", headerName: t("IDRAC_Server.Grid.Child_Site"), width:150,
       renderCell: (params) => {
        const parentID = params.row.childSiteId as string;
        const filteredData = referenceData?.childSite?.find(
          (item) => item.value === parentID
        );
        return <Box>{filteredData && <div>{filteredData.label}</div>}</Box>;
      },
    },
    { field: "userName", headerName: t("Manage_User.Manage_User_Grid_column.Username"),  width:150 },
    { field: "cpuLoad", headerName: t("IDRAC_Server.Grid.CPU_Load"),  width:100 },
    { field: "temperature", headerName: t("IDRAC_Server.Grid.Temperature"),  width:100 },
    { field: "memoryUsage", headerName: t("IDRAC_Server.Grid.Memory_Usage"),  width:100 },

    {
      field: "createdBy",
      headerName: t("Manage_User.Manage_User_Grid_column.Created_By"),
      width:150,
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
      width:150,
      renderCell: (params) => {
        const convertedDateTime = formatDateToConfiguredTimezone(params.value);
        return <span>{formatDate(convertedDateTime, timeFormat)}</span>;
      },
    },
    {
      field: "updatedBy",
      headerName: t("Manage_User.Manage_User_Grid_column.Updated_By"),
      width:150,
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
      width:150,
      renderCell: (params) => {
        const convertedDateTime = formatDateToConfiguredTimezone(params.value);
        return <span>{formatDate(convertedDateTime, timeFormat)}</span>;
      },
    },
    {
      field: "alarms",
      headerName: t("IDRAC_Server.Grid.List_Of_Alarm"),
      width: 170,
      renderCell: (params) => {       
        const alarms = params.row.alarms || [];
        const count = alarms.length;

        const alarmText =
          count > 4 ? "4+ Alarm" : `${count} Alarm`;
        
        return (
          <Box
            className="alarm-count-chip"
            onClick={() => {
              setSelectedAlarms(alarms);
              setSelectediDRAC(params.row); 
              setOpenDialog(true);
            }}
          >
            {alarmText}
          </Box>
        );
      },
    },
    
     ...(HasPermission(LABELS.AddOrEditAlarmEventIdracServers)
    ? [
        {
          field: "addAlarm",
          headerName: t("IDRAC_Server.Grid.Add_Alarm"),
          width: 150,
          sortable: false,
          renderCell: (params: any) => (
            <Box>
              <Tooltip title={t("IDRAC_Server.Grid.Add_Alarm")}>
                <IconButton onClick={() => addAlarm(params.row)}>
                  <img
                    src="/images/alarm-icon.svg"
                    alt="Alarm Icon"
                    width={20}
                    height={20}
                  />
                </IconButton>
              </Tooltip>
            </Box>
          ),
        },
      ]
    : []),
    {
      field: "actions",
      headerName: t("Manage_User.Manage_User_Grid_column.Actions"),
      width:200,
      sortable: false,
      renderCell: (params) => (
        <Box>
          {HasPermission(LABELS.AddOrUpdateIdracServers) && (
            <Tooltip title={"Edit"}>
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
          {HasPermission(LABELS.DeleteIdracServers) && (
          <Tooltip title={t("Manage_User.Delete_User")}>
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
          {HasPermission(LABELS.AuditLogMaster) &&
            HasPermission(LABELS.ViewAuditLogIdracServer) && (
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

  const alarmColumns: GridColDef[] = [
  {
    field: "name",
    headerName: t("IDRAC_Server.Alarm_Grid.Alarm_Name"),
    flex: 1,
  },
  {
    field: "event",
    headerName: t("IDRAC_Server.Alarm_Grid.IDRAC_Event"),
    flex: 1.5,
  },
  {
    field: "timeLimit",
    headerName: t("IDRAC_Server.Alarm_Grid.Time_Limit"),
    flex: 1,
    renderCell: (params) => `${params.row.timeLimit} seconds`,
  },
  {
    field: "actions",
    headerName: t("Manage_User.Manage_User_Grid_column.Actions"),
    flex: 1,
    sortable: false,
    renderCell: (params) => {
      return(
      <Box>
        {HasPermission(LABELS.DeleteAlarmEventIdracServers) && (
          <Tooltip title={t("IDRAC_Server.Alarm_Grid.Delete_Alarm")}>
            <IconButton onClick={() => handleDeleteAlarm(params.row.alarmId as string, selectediDRAC?.id as string)}>
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

      </Box>
    )},
  },
];

    const handleEdit = (server: IiDRACServer) => {    
      setSelectediDRAC(server);
      setOpenEditiDRAC(true);
    };

    const addAlarm = (server: IiDRACServer) => {
      setSelectediDRAC(server)
      setopenAddAlarm(true)
    }
    
  const handleDelete = (serverID: string) => {
    setServerToBeDelete({ id: serverID });
    setOpenDeleteConfirm(true);
  };

  const handleDeleteAlarm = (alarmId: string,serverID:string) => {
    setAlarmToBeDelete({ alarmEventId: alarmId,serverId : serverID });
    setOpenDeleteAlarmConfirm(true);
  };


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


  const handleAddServer = () => {
    setOpenAddiDRAC(true);
    setSelectediDRAC(undefined);
  };

  const handleCloseAddDrawer = () => {
    setOpenAddiDRAC(false);
    setSelectediDRAC(undefined);
  };

   const handleCloseEditDrawer = () => {
    setOpenEditiDRAC(false);
    setSelectediDRAC(undefined);
  };

  const handleCloseConfirm = () => {
    setOpenDeleteConfirm(false);
  };

  const handleCloseAlarmConfirm = () => {
  setOpenDeleteAlarmConfirm(false);
}

  const handleCloseAlarmDrawer = () => {
    setopenAddAlarm(false);
    setSelectediDRAC(undefined);
  };

  
  const finalDeleteServer = async (id: string) => {
    try {
      const deleteData : any = await DeleteiDRACService({id});
      if (deleteData.isSuccess) {
        setOpenDeleteConfirm(false);
        fetchInitialData();
      }
    } catch (err: any) { }
  };
  
  const AlarmDelete = async (data: IdeleteAlarmPayload) => {  
    try {
      const deleteData: any = await DeleteAlarmService(data);
      if (deleteData.isSuccess) {
        setOpenDeleteAlarmConfirm(false);
        setOpenDialog(false);
        setSelectediDRAC(undefined)
        fetchInitialData();
      }
    } catch (err: any) { }
  };
  


   return (
    <>
      <Container sx={{}} maxWidth={false}>
        <div className="top-orange-head" style={backgroundStyle} >
          <Box className="top-orange-head-left">
            <Typography variant="h4">
              {"Manage iDRAC"}
            </Typography>
            <Typography>
              {"Add, update & delete your iDRAC information here.."}
            </Typography>
          </Box>

          {HasPermission(LABELS.AddOrUpdateIdracServers) && (
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
              <img src={"/images/adddevice.svg"} alt="Add iDRAC" />{"Add iDRAC"}
            </CustomButton>
          )}
        </div>

        <Box
          className="top-list-bar"
        >


          <h5>{"List of iDRAC"}</h5>
          <div className="top-listing-items">

            <TextField
              variant="outlined"
              placeholder={"Search by IP Address, Port, ParentSite, ChildSite and Username..."}
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
          hideFooter={iDRACServers.length === 0}
          localeText={localeText}
          disableRowSelectionOnClick
        />

        <Drawer
          anchor={"right"}
          open={openAddiDRAC}
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
            <Typography variant="h6">{"Add iDRAC"}</Typography>
            
            <IconButton onClick={handleCloseAddDrawer}>
              <GridCloseIcon />
            </IconButton>
          </Box>
          <IDRACAddEditForm
            onClose={handleCloseAddDrawer}
            refreshData={fetchInitialData}
          />
        </Drawer>

        <Drawer
          anchor={"right"}
          open={openEditiDRAC}
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
            <Typography variant="h6">{"Edit iDRAC"}</Typography>
            <IconButton onClick={handleCloseEditDrawer}>
              <GridCloseIcon />
            </IconButton>
          </Box>
          <IDRACAddEditForm
            onClose={handleCloseEditDrawer}
            refreshData={fetchInitialData}
            server={selectediDRAC}
          />
        </Drawer>

         <Drawer
           anchor={"right"}
           open={openAddAlarm}
           onClose={() => {
             handleCloseAlarmDrawer();
           }}
           ModalProps={{
             onClose: (event, reason) => {
               if (reason !== 'backdropClick') {
                 // onClose(); // only call your close function on non-backdrop events
                 handleCloseAlarmDrawer();
               }
             }
           }}
           className="cmn-pop"
         >
           <Box className="cmn-pop-head">
             <Typography variant="h6">{"Add Alarm"}</Typography>
             <IconButton onClick={handleCloseAlarmDrawer}>
               <GridCloseIcon />
             </IconButton>
           </Box>
           <IDRACAddAlarmForm
             onClose={handleCloseAlarmDrawer}
             refreshData={fetchInitialData}
             server={selectediDRAC}
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

       <CommonDialog
          customClass="maintenace-plan-device-dialog-main"
          open={openDialog}
          title={"Alarm List"}
          onCancel={() => {
            setOpenDialog(false);
          }}
          showCloseIcon={true}
          cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
         content={
            <Box sx={{ height: "max-content", width: "100%" }}>
             <DataGrid
               rows={selectedAlarms}
               columns={alarmColumns}
               getRowId={(row) => row.alarmId}
               disableRowSelectionOnClick
               hideFooter
             />
           </Box>
         }
       />

       <CommonDialog
         open={openDeleteAlarmConfirm}
         title={t("Common_DELETE_Confirmation_Dialog.Title")}
         content={t("Common_DELETE_Confirmation_Dialog.Content")}
         customClass="cmn-confirm-delete-icon"
         onConfirm={() => alarmToBeDelete && AlarmDelete(alarmToBeDelete)}
         onCancel={handleCloseAlarmConfirm}
         confirmText={t("Common_DELETE_Confirmation_Dialog.Delete")}
         cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
         type="delete"
         titleClass={true}
         showCloseIcon={true}
       />
         
    </>
  );
}

export default ManageiDRAC