import React, { useEffect, useState } from 'react';
import { Box, Typography } from '@mui/material';
import { IChartData, IEmbeddedNetworkCardInfo, IIntegratedNetworkCardInfo, IserverData } from '../../../interfaces/IManageiDRAC';
import { fetchEventSource } from '@microsoft/fetch-event-source';
import { DateTime } from 'luxon';
import { formatDateToConfiguredTimezone } from '../../../utils/formatDateToConfiguredTimezone';
import CommonAreaChart from '../CommonAreaChart';
import { useTranslation } from 'react-i18next';
import apiUrls from '../../../constants/apiUrls';
import NoDataFound from './NoDataFound';
import { useThemeContext } from '../../../context/ThemeContext';
import { refreshAccessToken } from '../../../services/authService';
import { useAuth } from '../../../hooks/useAuth';

interface NetworkTabProps {
  EmbeddedNetworkDetail?: IEmbeddedNetworkCardInfo;
  IntegratedNetworkDetail?: IIntegratedNetworkCardInfo;
  server: IserverData
}


const NetworkDevicesTab: React.FC<NetworkTabProps> = ({
  EmbeddedNetworkDetail,
  IntegratedNetworkDetail,
  server
}) => {
  const { t } = useTranslation();
  const [activeSubTab, setActiveSubTab] =
    useState<'chart' | 'details'>('chart');
  const [embeddedChartData, setEmbeddedChartData] =
    useState<Record<string, IChartData[]>>({});

  const [integratedChartData, setIntegratedChartData] =
    useState<Record<string, IChartData[]>>({});
  const { theme, themeColor } = useThemeContext();
  const { handleLogout } = useAuth();

    const sections = [
    { title: 'EMBEDDED CARD',   cards: EmbeddedNetworkDetail?.NetworkCardList   ?? [] },
    { title: 'INTEGRATED CARD', cards: IntegratedNetworkDetail?.NetworkCardList ?? [] },
  ];
  useEffect(() => {

    let token =
      localStorage.getItem("accessToken");

    const embeddedController =
      new AbortController();

    const integratedController =
      new AbortController();

    const connectEmbedded = async () => {

      await fetchEventSource(        
        `${apiUrls.StreamEmbeddedNetworkUsage}?ServerID=${server.id}`,
        {
          method: "GET",

          headers: {
            Authorization: `Bearer ${token}`,
            Accept: "text/event-stream"
          },

          signal:
            embeddedController.signal,

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
                embeddedController.abort();
                return;
              }

              // reconnect with the new token (same as apiGet re-fetches)
              embeddedController.abort();
              connectEmbedded();
              return;
            }

            if (!response.ok) {
              throw new Error(
                "SSE CONNECTION FAILED"
              );
            }
          },              

          onmessage(ev) {

            const response =
              JSON.parse(ev.data);

            const currentUtcTime =
              DateTime.utc().toISO();

            const convertedTime = formatDateToConfiguredTimezone(currentUtcTime!);

            const formattedTime = DateTime
              .fromFormat(convertedTime, "yyyy-LL-dd HH:mm:ss")
              .toFormat("HH:mm:ss");

            setEmbeddedChartData((prev) => {

              const updated = { ...prev };

              response.forEach((item: any, index: number) => {

                // Create unique key
                const uniqueKey = `${item.Name}-${index}`;

                if (!updated[uniqueKey]) {
                  updated[uniqueKey] = [];
                }

                updated[uniqueKey] = [
                  ...updated[uniqueKey],
                  {
                    time: formattedTime,
                    value: item.Reading,
                    name: item.Name,
                    link: item.Link
                  }
                ];

                if (updated[uniqueKey].length > 60) {
                  updated[uniqueKey].shift();
                }
              });

              return updated;
            });
          }
        }
      );
    };

    const connectIntegrated = async () => {

      await fetchEventSource(        
        `${apiUrls.StreamIntegratedNetworkUsage}?ServerID=${server.id}`,
        {
          method: "GET",

          headers: {
            Authorization: `Bearer ${token}`,
            Accept: "text/event-stream"
          },

          signal:
            integratedController.signal,

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
                integratedController.abort();
                return;
              }

              // reconnect with the new token (same as apiGet re-fetches)
              integratedController.abort();
              connectIntegrated();
              return;
            }

            if (!response.ok) {
              throw new Error(
                "SSE CONNECTION FAILED"
              );
            }
          }, 

          onmessage(ev) {

            const response =
              JSON.parse(ev.data);

            const currentUtcTime =
              DateTime.utc().toISO();

            const convertedTime = formatDateToConfiguredTimezone(currentUtcTime!);

            const formattedTime = DateTime
              .fromFormat(convertedTime, "yyyy-LL-dd HH:mm:ss")
              .toFormat("HH:mm:ss");

            setIntegratedChartData((prev) => {

              const updated = { ...prev };

              response.forEach((item: any, index: number) => {

                const uniqueKey = `${item.Name}-${index}`;

                if (!updated[uniqueKey]) {
                  updated[uniqueKey] = [];
                }

                updated[uniqueKey] = [
                  ...updated[uniqueKey],
                  {
                    time: formattedTime,
                    value: item.Reading,
                    name: item.Name,
                    link: item.Link
                  }
                ];

                if (updated[uniqueKey].length > 60) {
                  updated[uniqueKey].shift();
                }
              });

              return updated;
            });
          }
        }
      );
    };

    connectEmbedded();
    connectIntegrated();

    return () => {

      embeddedController.abort();
      integratedController.abort();
    };

  }, []);

return (
  EmbeddedNetworkDetail && IntegratedNetworkDetail ?
    <Box className="memory-tab-wrapper">
      {/* ===== Top Summary ===== */}
      <Box className="memory-summary-card">
        <Typography variant='h3' className="memory-title">
          {t("iDRAC_Network.Embedded_Card")}
        </Typography>

        <Box className="memory-divider" />

        <Box className="memory-stats-row">
          {[
            {
              label: `${t("iDRAC_Network.Manufacturer")}:`,
              value: EmbeddedNetworkDetail?.Manufacturer,
              badge: true,
            },
            {
              label: `${t("iDRAC_Network.Model")}:`,
              value: EmbeddedNetworkDetail?.Model,
              badge: true,
            },
            {
              label: `${t("iDRAC_Network.Firmware_Package_Version")}:`,
              value: EmbeddedNetworkDetail?.FirmwarePackageVersion,
              badge: true,
            },           
            {
              label: `${t("iDRAC_Network.Health")}:`,
              value: EmbeddedNetworkDetail?.Health,
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

        <Typography variant='h3' className="memory-title" mb={2} mt={2}>
          {t("iDRAC_Network.Integrated_Card")}
        </Typography>

        <Box className="memory-stats-row">
          {[
               {
              label: `${t("iDRAC_Network.Manufacturer")}:`,
              value: IntegratedNetworkDetail?.Manufacturer,
              badge: true,
            },
            {
              label: `${t("iDRAC_Network.Model")}:`,
              value: IntegratedNetworkDetail?.Model,
              badge: true,
            },
            {
              label: `${t("iDRAC_Network.Firmware_Package_Version")}:`,
              value: IntegratedNetworkDetail?.FirmwarePackageVersion,
              badge: true,
            },           
            {
              label: `${t("iDRAC_Network.Health")}:`,
              value: IntegratedNetworkDetail?.Health,
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
          {t("iDRAC_Network.Chart")}
        </button>

        <button
          className={`memory-subtab-btn ${
            activeSubTab === 'details' ? 'active' : ''
          }`}
          onClick={() => setActiveSubTab('details')}
        >
          {t("iDRAC_Network.Details_List")}
        </button>
      </Box>

      {/* ===== Content ===== */}
      <Box className="memory-content-wrapper">
        {activeSubTab === 'chart' && (
          <>
            {Object.keys(embeddedChartData).length === 0 && Object.keys(integratedChartData).length === 0 ? (
              <NoDataFound />
            ) : (

              <Box>

                {/* ===== EMBEDDED ===== */}
                {Object.keys(embeddedChartData).length > 0 && (
                  <Typography
                    sx={{
                      fontSize: 20,
                      fontWeight: 700,
                      mb: 2
                    }}
                    variant='h4'
                  >
                    {t("iDRAC_Network.Embedded_Card")}
                  </Typography>
                )}
                <Box className="idrac-chart"
                // sx={{
                //   display: "grid",
                //   gridTemplateColumns:
                //     "repeat(auto-fit,minmax(500px,1fr))",
                //   gap: 3,
                //   mb: 4
                // }}
                >
                  {Object.entries(embeddedChartData).map(([key, data]) => (

                    <Box
                      key={key}
                      className="memory-chart-card"
                    >
                      <Typography
                        sx={{
                          fontWeight: 600,
                          mb: 1,
                        }}
                      >
                        {data[0]?.name}
                      </Typography>

                      <CommonAreaChart
                        data={data}
                        totalLabel={`${t("iDRAC_Network.Speed")}:`}
                        totalValue={
                          `${data[data.length - 1]
                            ?.value?.toString() || "0"} Mbps`
                        }
                        usedLabel={`${t("iDRAC_Network.Link")} :`}
                        usedValue={`${data[data.length - 1]
                          ?.link}`}
                      />
                    </Box>
                  ))}
                </Box>

                {/* ===== INTEGRATED ===== */}
                {Object.keys(integratedChartData).length > 0 && (
                  <Typography
                    variant='h4'
                    sx={{
                      fontSize: 20,
                      fontWeight: 700,
                      mb: 2
                    }}
                  >
                    {t("iDRAC_Network.Integrated_Card")}
                  </Typography>
                )}

                <Box className="idrac-chart"
                // s x={{
                //   display: "grid",
                //   gridTemplateColumns:
                //     "repeat(auto-fit,minmax(500px,1fr))",
                //   gap: 3,
                // }}
                >
                    {Object.entries(
                      integratedChartData
                    ).map(([key, data]) => (

                    <Box
                      key={key}
                      className="memory-chart-card"
                    >
                      <Typography
                        sx={{
                          fontWeight: 600,
                          mb: 1,
                        }}
                      >
                        {data[0]?.name}
                      </Typography>

                      <CommonAreaChart
                        data={data}
                        totalLabel={`${t("iDRAC_Network.Speed")}:`}
                        totalValue={
                          `${data[data.length - 1]
                            ?.value?.toString() || "0"} Mbps`
                        }
                        usedLabel={`${t("iDRAC_Network.Link")} :`}
                        usedValue={`${data[data.length - 1]
                          ?.link}`}
                      />
                    </Box>
                  ))}
                </Box>

              </Box>
            )}
          </>
        )}

        {activeSubTab === 'details' && (
          <Box className="embedded-card-main">
            {sections.map(({ title, cards }) => (
              <Box key={title} mb={3}>
             
                <Typography sx={{ fontWeight: 700, fontSize: 18, mb: 1.5 }}>
                  {title}
                </Typography>
       
                <Box className="embedded-card">
                  {cards.map((card, i) => (
                    <Box
                      key={i}
                      sx={{
                        bgcolor: '#f7f7f7',
                        borderRadius: 2,
                        p: 2
                      }}
                    >

                      {/* CARD HEADER */}
                      <Box
                        sx={{
                          display: "flex",
                          alignItems: "center",
                          gap: 1,
                          mb: 2,
                        }}
                      >
                      
                        
                        <Typography sx={{ fontSize: 16, fontWeight: 600, whiteSpace: 'nowrap' }}>
                            {t("iDRAC_Network.Product_Name")}
                          </Typography>
                          <Typography sx={{ fontSize: 16, fontStyle: 'italic' }}>
                            {card.ProductName}
                          </Typography>
                            {card.health !== "OK" && (
                          <img
                            src="/images/iDRAC/Alert.gif"
                            alt="Alert"
                            style={{
                              width: 22,
                              height: 22,
                              objectFit: "contain",
                            }}
                          />
                        )}
                      </Box>


                      {[
                       // { label: t("iDRAC_Network.Product_Name"), value: card.ProductName },
                        { label: t("iDRAC_Network.Protocol"), value: card.Protocol },
                        { label: t("iDRAC_Network.Vendor_Name"), value: card.VendorName },
                        { label: t("iDRAC_Network.Active_Link_Technology"), value: card.ActiveLinkTechnology },
                        { label: t("iDRAC_Network.Associated_Network_Address_MAC"), value: card.AssociatedNetworkAddress },
                        { label: t("iDRAC_Network.Link_Status"), value: card.LinkStatus },
                        { label: t("iDRAC_Network.Current_Link_Speed"), value: `${card.CurrentLinkSpeed} Mbps` },
                        { label: t("iDRAC_Network.Physical_Port_Number"), value: card.PhysicalPortNumber },
                        { label: t("iDRAC_Network.Status_Health"), value: card.health },
                      ].map(({ label, value }) => (
                        <Box key={label} sx={{ display: 'flex', gap: 1, mb: 1 }}>
                          <Typography sx={{ fontSize: 16, fontWeight: 600, whiteSpace: 'nowrap' }}>
                            {label}:
                          </Typography>
                          <Typography sx={{ fontSize: 16, fontStyle: 'italic' }}>
                            {value}
                          </Typography>
                        </Box>
                      ))}

                    </Box>
                  ))}
                </Box>

              </Box>
            ))}
          </Box>
        )}
      </Box>
    </Box>
    :
     <NoDataFound />
  );
};

export default NetworkDevicesTab;