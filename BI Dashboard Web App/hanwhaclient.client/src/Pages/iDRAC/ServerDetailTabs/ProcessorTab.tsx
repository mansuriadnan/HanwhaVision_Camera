import React, { useEffect, useState } from 'react';
import { Box, Typography } from '@mui/material';
import { IChartData, IProcessorDetail, IserverData } from '../../../interfaces/IManageiDRAC';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { useThemeContext } from '../../../context/ThemeContext';
import { useTimeFormatContext } from '../../../context/TimeFormatContext';
import { useTranslation } from 'react-i18next';
import { getHealthClass } from '../../../utils/healthStyle';
import { fetchEventSource } from '@microsoft/fetch-event-source';
import { DateTime } from 'luxon';
import { formatDateToConfiguredTimezone } from '../../../utils/formatDateToConfiguredTimezone';
import CommonAreaChart from '../CommonAreaChart';
import apiUrls from '../../../constants/apiUrls';
import NoDataFound from './NoDataFound';
import { refreshAccessToken } from '../../../services/authService';
import { useAuth } from '../../../hooks/useAuth';

interface ProcessorTabProps {
  processorDetail?: IProcessorDetail;
  server: IserverData
}

const ProcessorTab : React.FC<ProcessorTabProps> = ({
  processorDetail,
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
                      `${apiUrls.StreamCPUUsage}?ServerID=${server.id}`,
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
  
    const ProcessorColumns : GridColDef[] = [
        {
          field: "CpuName", headerName: t("iDRAC_CPU.Grid.CPU"), flex: 1,
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
                    ? "/images/iDRAC/cpu-grid-icon.svg"
                    : "/images/dark-theme/iDRAC/cpu-grid-icon.svg"
                }
                alt="cpu"
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
        { field: "Model", headerName: t("iDRAC_CPU.Grid.Model"), flex: 1 },
        { field: "Cores", headerName: t("iDRAC_CPU.Grid.Cores"), flex: 1 },
        { field: "Threads", headerName: t("iDRAC_CPU.Grid.Threads"), flex: 1 },
        {
          field: "MaxSpeed",
          headerName: t("iDRAC_CPU.Grid.Max_Speed"),
          flex: 1,
          renderCell: (params) => (
            <span>{`${params.value} MHz`}</span>
          ),
        },
        {
          field: "CurrentSpeed",
          headerName: t("iDRAC_CPU.Grid.Current_Speed"),
          flex: 1,
          renderCell: (params) => (
            <span>{`${params.value} MHz`}</span>
          ),
        },
        {
          field: "Health",
          headerName: t("iDRAC_CPU.Grid.Health"),
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
  const totalCores =
    processorDetail?.ProcessorList?.reduce(
      (sum, item) => sum + (Number(item.Cores) || 0),
      0
    ) || 0;

  const totalThreads =
    processorDetail?.ProcessorList?.reduce(
      (sum, item) => sum + (Number(item.Threads) || 0),
      0
    ) || 0;

  const averageCurrentSpeed =
    processorDetail?.ProcessorList?.length
      ? Math.round(
        processorDetail.ProcessorList.reduce(
          (sum, item) =>
            sum + (Number(item.CurrentSpeed) || 0),
          0
        ) / processorDetail.ProcessorList.length
      )
      : 0;

  return (
    processorDetail ?
    <Box className="memory-tab-wrapper">
      {/* ===== Top Summary ===== */}
      <Box className="memory-summary-card">
        <Typography  variant='h3' className="memory-title">
         {t("iDRAC_CPU.Processor_CPU")}
        </Typography>

        <Box className="memory-divider" />

        <Box className="memory-stats-row">
          {[
            {
              label: `${t("iDRAC_CPU.Count")}:`,
              value: processorDetail?.TotalCpu,
              badge: true,
            },
            {
              label: `${t("iDRAC_CPU.Total_Cores")}:`,
              value: totalCores,
              icon: theme === "light"
                  ? "/images/iDRAC/voice.svg"
                  : "/images/dark-theme/iDRAC/voice.svg"
            },
            {
              label: `${t("iDRAC_CPU.Total_Threads")}:`,
              value:  totalThreads,
              icon: theme === "light"
                  ? "/images/iDRAC/voice.svg"
                  : "/images/dark-theme/iDRAC/voice.svg"
            },
            {
              label: `${t("iDRAC_CPU.Avg_Speed")}:`,
              value: `${averageCurrentSpeed} GHz`,
              icon: theme === "light"
                  ? "/images/iDRAC/speed.svg"
                  : "/images/dark-theme/iDRAC/speed.svg"
            },
            {
              label: `${t("iDRAC_CPU.Health")}:`,
              value: processorDetail.Health,
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
          {t("iDRAC_CPU.Chart")}
        </button>

        <button
          className={`memory-subtab-btn ${
            activeSubTab === 'details' ? 'active' : ''
          }`}
          onClick={() => setActiveSubTab('details')}
        >
          {t("iDRAC_CPU.Details_List")}
        </button>
      </Box>

      {/* ===== Content ===== */}
      <Box className="memory-content-wrapper">
        {activeSubTab === 'chart' && (
          <Box >
            
              <div className="common-chart-title">
                {chartData[chartData.length - 1]?.name}
            </div>

            <CommonAreaChart
              data={chartData}
              totalLabel={`${t("iDRAC_CPU.System_Utilization")}:`}
              totalValue={`${chartData[chartData.length - 1]?.value ?? 0} %`}
            />
          </Box>
        )}

        {activeSubTab === 'details' && (
          <Box >
            <DataGrid
              rows={processorDetail?.ProcessorList || []}
              columns={ProcessorColumns}
              getRowId={(row) => row.CpuName}
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

export default ProcessorTab;