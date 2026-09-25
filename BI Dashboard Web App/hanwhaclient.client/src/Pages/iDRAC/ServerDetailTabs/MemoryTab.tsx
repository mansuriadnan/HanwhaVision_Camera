import React, { useEffect, useState } from 'react';
import { Box, Typography } from '@mui/material';
import CommonAreaChart from '../CommonAreaChart';
import { IChartData, IMemoryDetail, IserverData } from '../../../interfaces/IManageiDRAC';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { useThemeContext } from '../../../context/ThemeContext';
import { useTimeFormatContext } from '../../../context/TimeFormatContext';
import { useTranslation } from 'react-i18next';
import { getHealthClass } from '../../../utils/healthStyle';
import { fetchEventSource } from '@microsoft/fetch-event-source';
import { DateTime } from 'luxon';
import { formatDateToConfiguredTimezone } from '../../../utils/formatDateToConfiguredTimezone';
import apiUrls from '../../../constants/apiUrls';
import NoDataFound from './NoDataFound';
import { refreshAccessToken } from '../../../services/authService';
import { useAuth } from '../../../hooks/useAuth';

interface MemoryTabProps {
  memoryDetail?: IMemoryDetail;
  server: IserverData
}

const MemoryTab: React.FC<MemoryTabProps> = ({
  memoryDetail,
  server
}) => {

  const [activeSubTab, setActiveSubTab] =
    useState<'chart' | 'details'>('chart');  
  const [chartData, setChartData] = useState<IChartData[]>([]);
  const { theme, themeColor } = useThemeContext();
  const { t } = useTranslation();
  const { handleLogout } = useAuth();  
  
  useEffect(() => {
    
            let token =
                localStorage.getItem(
                    "accessToken"
                );
    
            const controller =
                new AbortController();
    
            const connect = async () => {
    
                await fetchEventSource(                    
                  `${apiUrls.StreamMEMUsage}?ServerID=${server.id}`,
                    {
                        method: "GET",
    
                        headers: {
                            Authorization:
                                `Bearer ${token}`,
                            Accept:
                                "text/event-stream"
                        },
    
                        signal:
                            controller.signal,
    
                        openWhenHidden: true,
    
                        async onopen(response) {
    
                            console.log(
                                "CONNECTED:",
                                response.status
                            );

                          if (response.status === 401) {
                            // ── same as apiGet 401 block: refresh token then retry ──────────
                            token = await refreshAccessToken();
                            if (!token) {
                              // refresh failed (refreshToken expired) → logout
                              handleLogout();
                              controller.abort();
                              return;
                            }

                            // reconnect with the new token (same as apiGet re-fetches)
                            controller.abort();
                            connect();
                            return;
                          }

                            if (!response.ok) {
                                throw new Error(
                                    "SSE CONNECTION FAILED"
                                );
                            }
                        },
    
                      onmessage(ev) {

                        const response = JSON.parse(ev.data);

                        const currentUtcTime =
                          DateTime.utc().toISO();

                        const convertedTime = formatDateToConfiguredTimezone(currentUtcTime!);

                        const formattedTime = DateTime
                          .fromFormat(convertedTime, "yyyy-LL-dd HH:mm:ss")
                          .toFormat("HH:mm:ss");

                        const newData = {
                          time: formattedTime,
                          value: response.Reading,
                          name: response.Name,
                        };

                        setChartData((prev: IChartData[]) => {

                          const updated = [...prev, newData];
                          if (updated.length > 60) updated.shift();
                          return updated;
                        });
                        
                      },
    
                        onclose() {
    
                            console.log(
                                "STREAM CLOSED"
                            );
                        },
    
                        onerror(error) {
    
                            console.error(
                                "SSE ERROR:",
                                error
                            );
    
                            throw error;
                        }
                    }
                );
            };
    
            connect();
    
            return () => {
    
                controller.abort();
            };
    
        }, []);

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

  const MemoryColumns: GridColDef[] = [
    {
      field: "RamName",
      headerName: t("IDRAC_Memory.Grid.RAM"),
      flex: 1,
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
                    ? "/images/iDRAC/memory-slot.svg"
                    : "/images/dark-theme/iDRAC/memory-slot.svg"
                }
              alt="Memory"
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
      field: "Size",
      headerName: t("IDRAC_Memory.Grid.Size"),
      flex: 1,
      renderCell: (params) => {
        const sizeInGB =
          params.value
            ? (Number(params.value) / 1024).toFixed(0)
            : "0";

        return <span>{sizeInGB} GB</span>;
      },
    },
    {
      field: "Type",
      headerName: t("IDRAC_Memory.Grid.Type"),
      flex: 1,
    },
    {
      field: "Speed",
      headerName: t("IDRAC_Memory.Grid.Speed"),
      flex: 1,
      renderCell: (params) => (
        <span>{`${params.value} MHz`}</span>
      ),
    },
    {
      field: "Manufacturer",
      headerName: t("IDRAC_Memory.Grid.Manufacturer"),
      flex: 1,
    },
    {
      field: "Health",
      headerName: t("IDRAC_Memory.Grid.Health"),
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
  ];

  const averageMemorySpeed =
    memoryDetail?.MemoryList?.length
      ? Math.round(
        memoryDetail.MemoryList.reduce(
          (sum, item) => sum + (Number(item.Speed) || 0),
          0
        ) / memoryDetail.MemoryList.length
      )
      : 0;
  return (
    memoryDetail ?
  (
    <Box className="memory-tab-wrapper">
      {/* ===== Top Summary ===== */}
      <Box className="memory-summary-card">
        <Typography variant='h3' className="memory-title">
          {t("IDRAC_Memory.MEMORY_Details")}
        </Typography>

        <Box className="memory-divider" />

        <Box className="memory-stats-row">
          {[
            {
              label: `${t("IDRAC_Memory.Total")}:`,
              value:`${memoryDetail?.TotalMemory} GB`,
              badge: true,
            },
            {
              label:  `${t("IDRAC_Memory.Used_Slots")}:`,
              value:`${memoryDetail?.UsedSlots} / ${memoryDetail?.TotalSlots}`,
              icon:  theme === "light"
                    ? "/images/iDRAC/memory-slot.svg"
                    : "/images/dark-theme/iDRAC/memory-slot.svg"
            }, 
            {
              label: `${t("IDRAC_Memory.Free_Slots")}:`,
              value: memoryDetail?.TotalSlots - memoryDetail?.UsedSlots,
              icon: theme === "light"
                    ? "/images/iDRAC/memory-slot.svg"
                    : "/images/dark-theme/iDRAC/memory-slot.svg"
            },
            {
              label: `${t("IDRAC_Memory.Speed")}:`,
              value: `${averageMemorySpeed} MHz`,
              icon: theme === "light"
                  ? "/images/iDRAC/speed.svg"
                  : "/images/dark-theme/iDRAC/speed.svg"
            },
            {
              label: `${t("IDRAC_Memory.Health")}:`,
              value: memoryDetail?.Health,
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
            activeSubTab === 'chart' ? 'active' : ''
          }`}
          onClick={() => setActiveSubTab('chart')}
        >
         {t("IDRAC_Memory.Chart")}
        </button>

        <button
          className={`memory-subtab-btn ${
            activeSubTab === 'details' ? 'active' : ''
          }`}
          onClick={() => setActiveSubTab('details')}
        >
         {t("IDRAC_Memory.Details_List")}
        </button>
      </Box>

      {/* ===== Content ===== */}
      <Box className="memory-content-wrapper">
        {activeSubTab === 'chart' && (
          <Box>
            
              <div className="common-chart-title">
                {chartData[chartData.length - 1]?.name}
            </div>

            <CommonAreaChart
              data={chartData}
              totalLabel={`${t("IDRAC_Memory.Total")}:`}
              totalValue={`${memoryDetail?.TotalMemory} GB`}
              usedLabel="Memory Utilization :"
              usedValue={`${chartData[chartData.length - 1]?.value ?? 0} %`}
            />
          </Box>
        )}

        {activeSubTab === 'details' && (
          <Box >
            <DataGrid
              rows={memoryDetail?.MemoryList || []}
              columns={MemoryColumns}
              getRowId={(row) => row.RamName}
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
   
  )
    : 
    <NoDataFound />    
  );
};

export default MemoryTab;