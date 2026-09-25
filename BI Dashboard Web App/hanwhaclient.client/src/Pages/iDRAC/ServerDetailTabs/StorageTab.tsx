import React, { useState } from 'react';
import { Box, Typography } from '@mui/material';
import { IDriveInfo, IStorageInfo } from '../../../interfaces/IManageiDRAC';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { getHealthClass } from '../../../utils/healthStyle';
import { useThemeContext } from '../../../context/ThemeContext';
import { useTranslation } from 'react-i18next';
import NoDataFound from './NoDataFound';

interface StorageTabProps {
  storageDetail?: IStorageInfo;
}

const StorageTab : React.FC<StorageTabProps> = ({
  storageDetail,
}) => {
  const [activeSubTab, setActiveSubTab] =
    useState<'details'>('details');
  
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
  
    const StorageColumns: GridColDef[] = [
      { field: "DiskName", headerName: t("IDRAC_Storage.Grid.Disk_Name"), flex: 1,
        renderCell: (params) => (
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              gap: 1,
              height: "100%",
            }}
          >
            <img
              src={
                theme === "light"
                  ? "/images/iDRAC/storage-grid-icon.svg"
                  : "/images/dark-theme/iDRAC/storage-grid-icon.svg"
              }
              alt="storage"
              className="memory-stat-icon"
            />
            {params.row.Health !== "OK" && (
              <img
                src="/images/iDRAC/Alert.gif"
                alt="Alert"
                style={{
                  width: 18,
                  height: 18,
                  objectFit: "contain",
                }}
              />
            )}

            <span>{params.value}</span>
          </Box>
        ),
       },
      {
        field: "Size", headerName: t("IDRAC_Storage.Grid.Size"), flex: 1,
        renderCell: (params) => {
          const sizeInTB =
            params.value
              ? (
                Number(params.value) /
                (1024 ** 4)
              ).toFixed(2)
              : "0";

          return (
            <span>
               {sizeInTB} TB
            </span>
          );
        },
      },
      { field: "Type", headerName: t("IDRAC_Storage.Grid.Type"), flex: 1 },
      { field: "Protocol", headerName: t("IDRAC_Storage.Grid.Protocol"), flex: 1 },
      { field: "Rpm", headerName: t("IDRAC_Storage.Grid.RPM"), flex: 1 },
      {
        field: "Health",
        headerName: t("iDRAC_Cooling.Grid.Health"),
        flex: 1,
        sortable: false,
        renderCell: (params) => {
                   return (
                     <Box
                       className={`status-chip ${getHealthClass(params.value)}`}
                       sx={{
                         width: 90,
                         height: 28,
                         px: 2,
                         borderRadius: "999px",
                         display: "flex",
                         alignItems: "center",
                         justifyContent: "center",
                         fontSize: "12px",
                         fontWeight: 600,
                         mt: 1,
                       }}
                     >
                       {params.value}
                     </Box>
                   );
                 },
      },
    ]


const totalCapacityTB =
  (
    (storageDetail?.Drives?.reduce(
      (total, drive) => total + drive.Size,
      0
    ) || 0) /
    (1024 ** 4)
  ).toFixed(2);

  const getOverallHealth = (
    drives: IDriveInfo[]
  ) => {
    if (
      drives.some(
        (drive) => drive.Health === "Critical"
      )
    ) {
      return "Critical";
    }

    if (
      drives.some(
        (drive) => drive.Health === "Warning"
      )
    ) {
      return "Warning";
    }

    return "OK";
  };

  return (
    storageDetail ? 
    (
    <Box className="memory-tab-wrapper">
      {/* ===== Top Summary ===== */}
      <Box className="memory-summary-card">
        <Typography variant='h3' className="memory-title">
         {t("IDRAC_Storage.Title")}
        </Typography>

        <Box className="memory-divider" />

        <Box className="memory-stats-row">
          {[
            {
              label: `${t("IDRAC_Storage.Used_Slots")}:`,
              value: storageDetail?.Drives?.length || 0,
              icon: theme === "light"
                  ? "/images/iDRAC/storage.svg"
                  : "/images/dark-theme/iDRAC/storage.svg"
            },
            {
              label: `${t("IDRAC_Storage.Total_Capacity")}:`,
              value: `${totalCapacityTB} TB`,
              icon: theme === "light"
                  ? "/images/iDRAC/capacity.svg"
                  : "/images/dark-theme/iDRAC/capacity.svg"
            },
            {
              label: `${t("IDRAC_Storage.Health")}:`,
              value: getOverallHealth(
                storageDetail?.Drives || []
              ),
              icon: theme === "light"
                  ? "/images/iDRAC/health.svg"
                  : "/images/dark-theme/iDRAC/health.svg"
            },
          ].map((item) => (
            <Box
              key={item.label}
              className="memory-stat-item"
            >
              <Typography className="memory-stat-label">
                {item.label}
              </Typography>

              <Box className="memory-stat-value-row">
                <img
                  src={item.icon}
                  alt={item.label}
                  className="memory-stat-icon"
                />

                <Typography className="memory-stat-value">
                  {item.value}
                </Typography>
              </Box>
            </Box>
          ))}
        </Box>
      </Box>

      {/* ===== Sub Tabs ===== */}
      <Box className="memory-subtabs">
       
        <button
          className={`memory-subtab-btn ${
            activeSubTab === 'details' ? 'active' : ''
          }`}
          onClick={() => setActiveSubTab('details')}
        >
           {t("IDRAC_Storage.Details_List")}
        </button>
      </Box>

      {/* ===== Content ===== */}
      <Box className="memory-content-wrapper">
        
        {activeSubTab === 'details' && (
          <Box className='testing'>
            <DataGrid
              rows={storageDetail?.Drives || []}
              columns={StorageColumns}
              getRowId={(row) => row.DiskName}
              disableRowSelectionOnClick
              className="user-screen"
              localeText={localeText}
              slots={{
                noRowsOverlay: CustomNoRowsOverlay,
              }}
              hideFooter
            />
          </Box>
        )}
      </Box>
    </Box>
    ) :
      <NoDataFound />    
  );
};

export default StorageTab;