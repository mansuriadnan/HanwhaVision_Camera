import React, { useEffect, useRef, useState } from "react";
import {
  CommonWidgetProps,
  ISVbyVehicleData,
  IWidgetPayload,
  LayoutItem,
} from "../../../../interfaces/IChart";
import { Box } from "@mui/material";
import {
  CountingForForklift1_1,
  CountingForForklift2_1Option1,
  CountingForForklift2_1Option2,
  CountingForForklift2_1Option3,
  CommonDialog,
  LocalLoader,
} from "../../../index";
import {
  fetchForkliftDataService,
  // fetchVehicleQueueAnalysisDataService,
} from "../../../../services/dashboardService";
import { convertToUTC } from "../../../../utils/convertToUTC";
import apiUrls from "../../../../constants/apiUrls";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import { useSignalRContext } from "../../../../context/SignalRContext";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";
import { formatNumber } from "../../../../utils/formatNumber";
import { formatDateToConfiguredTimezone } from "../../../../utils/formatDateToConfiguredTimezone";
import { formatDateToCustomFormat } from "../../../../utils/dateUtils";

const thStyle = {
  border: "1px solid #ccc",
  padding: "0px 4px",
  textAlign: "left",
  lineHeight: "18px"
};

const tdStyle = {
  border: "1px solid #ccc",
  padding: "0px 4px",
  lineHeight: "18px"
};


const CountingForForkliftWidget: React.FC<CommonWidgetProps> = ({
  item,
  floor,
  zones,
  selectedStartDate,
  selectedEndDate,
  setExportHandler,
  setIsDraggable,
  pdfMode,
  finalRulesValue
}) => {
  const { width, height, size, expanded, displayName, chartName } =
    item as LayoutItem;
  const [ForkliftData, setForkliftData] = useState<ISVbyVehicleData[]>();
  const [openZoomDialog, setOpenZoomDialog] = useState(false);
  const customizedWidth =
    size === "3x1" ? width / 3 : size === "2x1" ? width / 2 : width;
  const [loadingCount, setLoadingCount] = useState(0);
  const selectedIntervalNameRef = useRef<string>("");
  const { IsDisplayLoader } = useSignalRContext();

  useExportHandler({
    apiEndpoint: `${apiUrls.ForkliftCountAnalysis}/csv`,
    startDate: convertDateToISOLikeString(selectedStartDate),
    endDate: convertDateToISOLikeString(selectedEndDate),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  useEffect(() => {
    fetchForkliftData();
  }, [floor, zones, selectedStartDate, selectedEndDate]);

  const fetchForkliftData = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const data = {
        floorIds: floor,
        zoneIds: zones,
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const response: any = await fetchForkliftDataService(
        data as unknown as IWidgetPayload
      );
      // const response: any = await fetchVehicleQueueAnalysisDataService(
      //   data as IWidgetPayload
      // );
      
      if (response?.data.length > 0) {
        setForkliftData(response?.data as ISVbyVehicleData[]);
      } else {
        setForkliftData([]);
      }
    } catch (error) {
      console.error("Error fetching Forklift count data:", error);
      throw error;
    } finally {
      setLoadingCount((c) => c - 1);
    }
  };

  const handleZoomClick = () => {
    setOpenZoomDialog(true);
  };

  const handleCloseZoom = () => {
    setOpenZoomDialog(false);
  };

  const renderLayout = () => {
    return (
      <Box sx={{ height: height, display: "flex" }}>
        <CountingForForklift1_1
          customizedWidth={customizedWidth}
          customizedHeight={height}
          ForkliftData={ForkliftData}
          displayName={displayName}
          onZoomClick={handleZoomClick}
          openZoomDialog={openZoomDialog}
          setIsDraggable={setIsDraggable}
          finalRulesValue={finalRulesValue}
        />

        {size === "2x1" &&
          (expanded === "Option3" ? (
            <CountingForForklift2_1Option3
              customizedWidth={customizedWidth}
              customizedHeight={height}
              ForkliftData={ForkliftData}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
              finalRulesValue={finalRulesValue}
            />
          ) : expanded === "Option2" ? (
            <CountingForForklift2_1Option2
              customizedWidth={customizedWidth}
              customizedHeight={height}
              ForkliftData={ForkliftData}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
              finalRulesValue={finalRulesValue}
            />
          ) : (
            <CountingForForklift2_1Option1
              customizedWidth={customizedWidth}
              customizedHeight={height}
              ForkliftData={ForkliftData}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
              finalRulesValue={finalRulesValue}
            />
          ))}
        <Box className="widget-label-bottom">
          <span>{chartName}</span>
          <img
            src="/images/question_circle_icon_widget_bottom_title.svg"
            alt="info Icon"
            className="widget-label-icon-bottom"
          />
        </Box>
      </Box>
    );
  };

  // const hasDispatchedRef = useRef(false);
  //   useEffect(() => {
  //     if (!localLoading && !hasDispatchedRef.current) {
  //       hasDispatchedRef.current = true;
  //       onLoadComplete?.();
  //     }
  //   }, [localLoading, onLoadComplete]);

  return (
    <>
      {loadingCount > 0 && !IsDisplayLoader ? (
        <LocalLoader width={width} height={height} size={50} color="warning" />
      ) : (
        <>
          {renderLayout()}

            {pdfMode && ForkliftData &&  (
              <div style={{ marginTop: 8 }}>
                {(() => {
                  const multiplier = 1 + (finalRulesValue || 0) / 100;

                  const map: Record<string, { date: string; queueCount: number }> = {};

                  ForkliftData.forEach((d) => {
                    if (!d?.dateTime) return;

                    const utcDate = new Date(d.dateTime);

                    const year = utcDate.getFullYear();
                    const month = String(utcDate.getMonth() + 1).padStart(2, "0");
                    const day = String(utcDate.getDate()).padStart(2, "0");

                    const key = `${year}-${month}-${day}`;

                    if (!map[key]) {
                      map[key] = { date: key, queueCount: 0 };
                    }

                    //  Apply percentage 
                    const adjustedValue =(d.queueCount ?? 0) * multiplier;

                    map[key].queueCount += adjustedValue;
                  });

                  const grouped = Object.values(map)
                    .map((r) => ({
                      ...r,
                      queueCount: r.queueCount
                    }))
                    .filter((r) => r.queueCount !== 0)
                    .sort((a, b) => a.date.localeCompare(b.date));

                 
                  const total = grouped.reduce((s, r) => s + r.queueCount, 0);

                  const date1 = new Date(selectedStartDate);
                  const date2 = new Date(selectedEndDate);
                  const diffDays = Math.max(
                    1,
                    Math.floor((date2.getTime() - date1.getTime()) / (1000 * 60 * 60 * 24)) + 1
                  );
                  const average =
                    grouped.length > 0 ? (total / diffDays) : 0;

                  const highest =  grouped.length > 0 ? grouped.reduce((max, r) =>
                    r.queueCount > max.queueCount ? r : max
                  ) : null;

                  const lowest = grouped.length > 0 ? grouped.reduce((min, r) =>
                    r.queueCount < min.queueCount ? r : min
                  ) : null;

                  const highestLabel =
                    highest && formatDateToCustomFormat(formatDateToConfiguredTimezone(highest.date),"DD/MM/YYYY")
                      ? formatDateToCustomFormat((formatDateToConfiguredTimezone(highest.date) as string),"DD/MM/YYYY")
                      : "-";

                  const lowestLabel =
                    lowest && formatDateToCustomFormat(formatDateToConfiguredTimezone(lowest.date),"DD/MM/YYYY")
                      ? formatDateToCustomFormat((formatDateToConfiguredTimezone(lowest.date) as string),"DD/MM/YYYY")
                      : "-";
                  return (
                    <div style={{
                          padding: "0px 15px 30px 15px "                            
                          }}>                       
                      <table
                       className="report-table-sch"
                         style={{
                            width: "100%",
                            borderCollapse: "collapse",
                            borderSpacing: 0,
                            tableLayout: "fixed",
                            fontSize: "12px",
                            lineHeight: "14px",
                            pageBreakInside: "avoid",
                            breakInside: "avoid",
                          }}
                      >
                        <thead>
                          <tr
                            style={{
                              pageBreakInside: "avoid",
                              breakInside: "avoid",
                              backgroundColor: "#f0f0f0" 
                            }}
                          >
                            <th style={thStyle}>
                              Date
                            </th>
                            <th style={thStyle}>
                              Forklift Count
                            </th>
                          </tr>
                        </thead>
                        <tbody>
                             {grouped.length > 0 ? (
                            <>
                              {grouped.map((row) => {
                                const convertedDatetime = formatDateToConfiguredTimezone(row.date) as string;
                                const dateLabel = formatDateToCustomFormat(convertedDatetime,"DD/MM/YYYY");


                                return (
                                  <tr key={row.date}
                                    style={{
                                      pageBreakInside: "avoid",
                                      breakInside: "avoid",
                                    }}
                                  >
                                    <td style={tdStyle}>
                                      {dateLabel}
                                    </td>
                                    <td style={tdStyle}>
                                      {formatNumber(row.queueCount)}
                                    </td>
                                  </tr>
                                );
                              })}

                              <tr
                                style={{
                                  fontWeight: 600,
                                  backgroundColor: "#f5f5f5",
                                  pageBreakInside: "avoid",
                                  breakInside: "avoid",
                                }}>
                                <td style={tdStyle}>
                                  Total
                                </td>
                                <td style={tdStyle} >
                                  {formatNumber(total)}
                                </td>
                              </tr>

                              <tr
                                style={{
                                  fontWeight: 600,
                                  pageBreakInside: "avoid",
                                  breakInside: "avoid",
                                }}>
                                <td style={tdStyle}>
                                  Average
                                </td>
                                <td style={tdStyle}>
                                  {formatNumber(average)}
                                </td>
                              </tr>
                            </>) : (
                          <tr>
                            <td
                              colSpan={2}
                              style={{
                                textAlign: "center",
                                border: "1px solid #ccc",
                                padding: "0px 4px",
                                lineHeight: "18px",
                              }}
                            >
                              No data available
                            </td>
                          </tr>
                        )}
                        </tbody>
                      </table>

                      {/* Summary */}
                       {grouped.length > 0 && (
                      <div style={{ marginTop: 8 }}>
                        <div style={{ fontWeight: 600, marginBottom: 6 }}>
                          Summary
                        </div>
                        <ul style={{ marginTop: 0, marginBottom: 0, paddingLeft: 18 }}>
                          <li  style={{ lineHeight: "18px"}}>
                            Day {highestLabel} has the highest forklift count ({formatNumber(highest.queueCount)})
                          </li>
                          <li  style={{ lineHeight: "18px"}}>
                            Day {lowestLabel} has the lowest forklift count ({formatNumber(lowest.queueCount)})
                          </li>
                        </ul>
                      </div>
                       )}
                    </div>
                  );
                })()}
              </div>
            )}

          <CommonDialog
            open={openZoomDialog}
            title={"Expanded View"}
            onCancel={handleCloseZoom}
            maxWidth={"lg"}
            customClass={"widget_popup cmn-pop-design-parent"}
            content={renderLayout()}
            isWidget={true}
          />
        </>
      )}
    </>
  );
};

export { CountingForForkliftWidget };
