// SSMDeviceScreen.tsx — new file

import { Box, Chip, IconButton, InputAdornment, TextField, Typography } from "@mui/material";
import { DataGrid, GridColDef, GridPaginationModel, GridSortModel } from "@mui/x-data-grid";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useThemeContext } from "../../context/ThemeContext";
import { GetSSMDeviceService } from "../../services/SSMService";
import { ISSMDeviceDetail, ISSMDevicePayload } from "../../interfaces/IManageServer";
import SSMDeviceChartDialog from "./SSMDeviceChartDialog";

interface SSMDeviceScreenProps {
  server: { id: string; name: string };
  selectedDate: string | null;
  onBack: () => void;
}

const SSMDeviceScreen: React.FC<SSMDeviceScreenProps> = ({
  server,
  selectedDate,
  onBack,
}) => {
   const [SSMDeviceList, setSSMDeviceList] = useState<ISSMDeviceDetail[]>([]);
    const [TotalRecord, SetTotalRecord] = useState<number>(0);
    const [searchTerm, setSearchTerm] = useState<string>("");
    const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
      pageSize: 10,
      page: 0,
    });
    const [sortModel, setSortModel] = useState<GridSortModel>([
      { field: "createdOn", sort: "desc" },
    ]);
    const { theme, themeColor } = useThemeContext();
    const { t } = useTranslation();
    const [openDialog, setOpenDialog] = useState<boolean>(false); 
    const [selectedDevice, setSelectedDevice] = useState<string>("");
     const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
        ? `${themeColor}/dark-theme/`
        : `${themeColor}/`;

      
      useEffect(() => {
        const delayDebounce = setTimeout(() => {
          if (searchTerm.length >= 3 || searchTerm.length === 0) {
            fetchSSMDeviceData();
          }
        }, 500);
    
        return () => clearTimeout(delayDebounce);
      }, [paginationModel, sortModel, searchTerm]);
    
      const fetchSSMDeviceData = async () => {
        const sortBy = sortModel[0]?.field || "createdOn";
        const sortOrder =
          sortModel[0]?.sort && sortModel[0]?.sort === "asc" ? 1 : -1;
        try {
          let request :ISSMDevicePayload = {
            searchText: searchTerm.length > 0 ? searchTerm : "",
            pageNumber: paginationModel.page + 1,
            pageSize: paginationModel.pageSize,
            sortBy: sortBy,
            sortOrder: sortOrder,
            serverId : server.id,
            dateFilter : selectedDate
          };
          const ssmDeviceData: any = await GetSSMDeviceService(request);          
    
          if (ssmDeviceData && ssmDeviceData?.isSuccess && ssmDeviceData?.data) {
            setSSMDeviceList(ssmDeviceData?.data?.ssmDeviceDetails);
            SetTotalRecord(ssmDeviceData?.data?.totalCount);
          } else {
            setSSMDeviceList([]);
            SetTotalRecord(0);
          }
        } catch (err: any) {
          console.error("Error fetching initial data:", err);
        }
      };

    const getCameraStatusStyle = (status: string) => {
      switch (status?.toLowerCase()) {
        case "connected":
          return {
            color: "#2e7d32",
            backgroundColor: "#c8e6c9",
          };

        case "disconnected":
          return {
            color: "#d32f2f",
            backgroundColor: "#ffcdd2",
          };

        case "warning":
          return {
            color: "#ed6c02",
            backgroundColor: "#ffe0b2",
          };

        default:
          return {
            color: "#555",
            backgroundColor: "#e0e0e0",
          };
      }
    };
    
    const columns: GridColDef[] = [
      { field: "name", headerName: t("Manage_Device.Manage_Device_Grid_column.Device_name"), flex: 1 },
      { field: "cameraModel", headerName: t("Manage_Device.Manage_Device_Grid_column.Model"), flex: 1 },
      { field: "ipAddress", headerName: t("Manage_Device.Manage_Device_Grid_column.IP_Address"), flex: 1 },
      { field: "location", headerName:  t("Manage_Device.Manage_Device_Grid_column.Location"), flex: 1 },    
      { field: "recordingStatus", headerName: t("SSM_Device_screen.Recording_Status"), flex: 1 },  
        { field: "cameraStatus", headerName: t("SSM_Device_screen.Camera_Status"), flex: 1,
        renderCell: (params) => {
          return (
            <>
              <Chip label={params.value} sx={getCameraStatusStyle(params.value)} />
              <IconButton
                onClick={(e) => {
                 setSelectedDevice(params.row.id)
                 setOpenDialog(true)
                }}
                
              >
                <img src="/images/SSM/chart-icon.svg" width={30} />
              </IconButton>
              
            </>);
            },},
 
  ];

    const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
      const searchValue = event.target.value;
      setSearchTerm(searchValue);
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
        console.log("newSortModel",newSortModel)
        // setSortModel(newSortModel.length ? newSortModel : [
        //   { field: "createdOn", sort: "desc" },
        // ]);
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
  
  return (
    <Box>
      {/* Back Button */}
      <Box
        sx={{
          display: "flex",
          alignItems: "center",
          gap: 1,
          mb: 2,
          cursor: "pointer",
          width: "fit-content",
        }}
        onClick={onBack}
      >
        {/* <img src="/images/back-arrow.svg" width={20} alt="back" /> */}
        <Typography className="back-to">
          {t("SSM_Device_screen.Back_to_Dashboard")}
        </Typography>
      </Box>
   
      <div className="top-list-bar">

        <Typography variant="h5" fontWeight={700} mb={3}>
          {t("SSM_Device_screen.Devices", {
            serverName: server.name,
          })}
        </Typography>

        <div className="top-listing-items">
          <TextField
            placeholder={t("SSM_Device_screen.Seach_placeholder")}
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
        rows={SSMDeviceList}
        columns={columns}
        getRowId={(row) => row.id}
        paginationModel={paginationModel}
        onPaginationModelChange={handlePaginationModelChange}
        rowCount={SSMDeviceList.length === 0 ? 0 : TotalRecord}
        paginationMode="server"
        pageSizeOptions={[5, 10, 15, 20, 25]}
        sortingMode="server"
        sortModel={sortModel}
        onSortModelChange={handleSortModelChange}
        disableRowSelectionOnClick
        slots={{
          noRowsOverlay: CustomNoRowsOverlay,
        }}
        hideFooter={SSMDeviceList.length === 0}
        localeText={localeText}
      />
      <SSMDeviceChartDialog
        open={openDialog}
        onClose={() => setOpenDialog(false)}
        device={selectedDevice}
        selectedDate={selectedDate}
    />
    </Box>
  );
};

export default SSMDeviceScreen;