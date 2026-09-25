import React, { useState } from 'react';
import { Box, Typography } from '@mui/material';
import { IPowerSupplyDetail } from '../../../interfaces/IManageiDRAC';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { useThemeContext } from '../../../context/ThemeContext';
import { useTranslation } from 'react-i18next';
import { getHealthClass } from '../../../utils/healthStyle';
import NoDataFound from './NoDataFound';

interface PowerSupplyTabProps {
  PowerSupply?: IPowerSupplyDetail;
}
  const PowerSupplyTab: React.FC<PowerSupplyTabProps> = ({
    PowerSupply,
  }) => {
  const [activeSubTab, setActiveSubTab] =
    useState<'details'>('details');

  const { theme, themeColor } = useThemeContext();
    const { t } = useTranslation  ();
  
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
   
  
    const PowerSupplyColumns : GridColDef[] = [
        { field: "Psu", headerName: t("iDRAC_PowerSupply.Grid.PSU"), flex: 1,
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
                  ? "/images/iDRAC/power-grid-icon.svg"
                  : "/images/dark-theme/iDRAC/power-grid-icon.svg"
              }
              alt="power-supply"
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
        { field: "Capacity", 
          headerName: t("iDRAC_PowerSupply.Grid.Capacity"), 
          flex: 1,
          renderCell: (params) =>{
            return(
              <span>{`${params.value}W`}</span>
            )
          }
        
        },
        { field: "Output", 
          headerName: t("iDRAC_PowerSupply.Grid.Output"), 
          flex: 1,
          renderCell: (params) =>{
            return(
              <span>{`${params.value}W`}</span>
            )
          }
        },
        { field: "InputVoltage", 
          headerName: t("iDRAC_PowerSupply.Grid.Input_Voltage"), 
          flex: 1,
          renderCell: (params) =>{
            return(
              <span>{`${params.value}V`}</span>
            )
          }
        },
        { field: "CurrentVoltage",
          headerName: t("iDRAC_PowerSupply.Grid.Current"), 
          flex: 1,
          renderCell: (params) =>{
          return(
            <span>{`${params.value}A`}</span>
          )
        } 
        },
        {
          field: "Health",
          headerName: t("iDRAC_PowerSupply.Grid.Health"),
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

  return (
    PowerSupply ?
    <Box className="memory-tab-wrapper">
      {/* ===== Top Summary ===== */}
      <Box className="memory-summary-card">
        <Typography variant='h3' className="memory-title">
          {t("iDRAC_PowerSupply.Power_Supply")}
        </Typography>

        <Box className="memory-divider" />

        <Box className="memory-stats-row">
          {[
            {
              label: `${t("iDRAC_PowerSupply.PSU_Count")}:`,
              value: PowerSupply.PowerSupplyList.length,
              badge: true,
            },
            {
              label: `${t("iDRAC_PowerSupply.Total_Capacity")}:`,
              value: `${PowerSupply.TotalCapacity}W`,
              icon: theme === "light"
                ? "/images/iDRAC/power-supply.svg"
                : "/images/dark-theme/iDRAC/power-supply.svg"
            },
            {
              label: `${t("iDRAC_PowerSupply.Current_Usage")}:`,
              value: `${PowerSupply.CurrentUsage}W`,
              icon: theme === "light"
              ? "/images/iDRAC/power-supply.svg"
              : "/images/dark-theme/iDRAC/power-supply.svg"
            },
            {
              label: `${t("iDRAC_PowerSupply.Redundancy")}:`,
              value: PowerSupply.Redundancy,
              badge: true,
            },
            {
              label: `${t("iDRAC_PowerSupply.Health")}:`,
              value: PowerSupply.Health,
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

              {item.badge ? (
                <Box className="memory-badge">
                  {item.value}
                </Box>
              ) : (
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
              )}
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
          {t("iDRAC_PowerSupply.Details_List")}
        </button>
      </Box>

      {/* ===== Content ===== */}
      <Box className="memory-content-wrapper">
       
        {activeSubTab === 'details' && (
          <Box >
            <DataGrid
              rows={PowerSupply.PowerSupplyList || []}
              columns={PowerSupplyColumns}
              getRowId={(row) => row.Psu}
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
   
    :
     <NoDataFound />
  );
};

export default PowerSupplyTab;