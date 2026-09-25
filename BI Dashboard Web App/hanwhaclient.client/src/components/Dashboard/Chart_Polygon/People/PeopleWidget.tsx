import React, { useEffect, useRef, useState } from "react";
import {
  IWidgetPayload,
  LayoutItem,
  PVInOutData,
  CommonWidgetProps,
  IPInOutData,
  ISelectedDevicesHeatmap,
} from "../../../../interfaces/IChart";
import {
  PeopleAvgInOutDataService,
  fetchDevicesByFloorZoneDataService,
} from "../../../../services/dashboardService";
import { Box } from "@mui/material";
import {
  LocalLoader,
  People1_1,
  People2_1_Option1,
  People2_1_Option2,
  People2_1_Option3,
  CommonDialog,
} from "../../../index";
import { convertToUTC } from "../../../../utils/convertToUTC";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import apiUrls from "../../../../constants/apiUrls";
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

const PeopleWidget: React.FC<CommonWidgetProps> = ({
  item,
  floor,
  zones,
  selectedStartDate,
  selectedEndDate,
  setExportHandler,
  setIsDraggable,
  finalRulesValue,
  pdfMode,
}) => {
  const {
    width,
    height,
    size,
    expanded,
    displayName,
    deivceListforPVCount,
    chartName,
  } = item as LayoutItem;
  const customizedWidth =
    size === "3x1" ? width / 3 : size === "2x1" ? width / 2 : width;
  const [peopleInOutData, setPeopleInOutData] = useState<PVInOutData>();
  const [pInOutData, setPInOutData] = useState<IPInOutData[] | null>(null);
  const [selectedCamera, setSelectedCamera] = useState(
    "6812332a6d517f8bfff611bb",
  );
  const [loadingCount, setLoadingCount] = useState(0);
  const [openZoomDialog, setOpenZoomDialog] = useState(false);
  const [filetredDevices, setFilteredDevices] = useState<
    ISelectedDevicesHeatmap[]
  >([]);
  const selectedIntervalNameRef = useRef<string>("");
  const { IsDisplayLoader } = useSignalRContext();

  const AllCameraOpt = {
    deviceId: "6812332a6d517f8bfff611bb",
    channelNo: 0,
    cameraName: "All Camera",
    floorId: null,
    zoneId: null,
  };

  useExportHandler({
    apiEndpoint: `${apiUrls.PeopleInOutCountChart}/csv`,
    startDate: convertDateToISOLikeString(selectedStartDate),
    endDate: convertDateToISOLikeString(selectedEndDate),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  useEffect(() => {
    if (size != "1x1") {
      fetchDevicesByFloorZone();
    }
  }, [deivceListforPVCount, floor, zones]);

  useEffect(() => {
    const params = {
      floorIds: floor,
      zoneIds: zones,
      startDate: convertToUTC(selectedStartDate),
      endDate: convertToUTC(selectedEndDate),
      deviceId: selectedCamera,
    };

    fetchDataAndRender(params as unknown as IWidgetPayload);
  }, [floor, zones, selectedStartDate, selectedEndDate, size, expanded, finalRulesValue]);

  const fetchDevicesByFloorZone = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const param = {
        floorIds: floor,
        zoneIds: zones,
      };

      let response: any = await fetchDevicesByFloorZoneDataService(
        param as unknown as IWidgetPayload,
      );

      const filtered = response?.data?.filter(
        (device: ISelectedDevicesHeatmap) =>
          deivceListforPVCount?.some(
            (sel: { deviceId: string; channelNo: number }) =>
              sel.deviceId === device.deviceId &&
              sel.channelNo === device.channelNo,
          ),
      );

      setFilteredDevices([AllCameraOpt, ...(filtered || [])]);
    } catch (error) {
      console.error("Error fetching device by floor and zone:", error);
      throw error;
    } finally {
      setLoadingCount((c) => c - 1);
    }
  };

  const handleDeviceChange = async (event: any) => {
    const selectedDeviceId = event.target.value;
    setSelectedCamera(selectedDeviceId);
    const params = {
      floorIds: floor,
      zoneIds: zones,
      startDate: convertToUTC(selectedStartDate),
      endDate: convertToUTC(selectedEndDate),
      deviceId: selectedDeviceId,
    };

    fetchDataAndRender(params as unknown as IWidgetPayload);
  };

  const fetchDataAndRender = async (params: IWidgetPayload) => {
    setLoadingCount((c) => c + 1);
    try {
      const response: any = await PeopleAvgInOutDataService(params);

      if (response?.data && response?.data.length > 0) {
        setPInOutData(response?.data as IPInOutData[]);

        const totalIn = response?.data.reduce(
          (sum: any, item: { inCount: any }) => sum + (item.inCount || 0),
          0,
        );
        const totalOut = response?.data.reduce(
          (sum: any, item: { outCount: any }) => sum + (item.outCount || 0),
          0,
        );

        setPeopleInOutData({
          totalInCount: totalIn,
          totalOutCount: totalOut,
        });
        
      } else {
        setPInOutData([]);
        setPeopleInOutData({
          totalInCount: 0,
          totalOutCount: 0,
        });
      }
    } catch (error) {
      console.error("Error fetching People In out data:", error);
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
      <Box sx={{ display: "flex", height: height }}>
        <People1_1
          inOutValue={peopleInOutData}
          customizedWidth={customizedWidth}
          displayName={displayName}
          onZoomClick={handleZoomClick}
          openZoomDialog={openZoomDialog}
          setIsDraggable={setIsDraggable}
          finalRulesValue={finalRulesValue}
        />

        {size === "2x1" &&
          (expanded === "Option3" ? (
            <People2_1_Option3
              customizedWidth={customizedWidth}
              customizedHeight={height}
              selectedStartDate={new Date(selectedStartDate)}
              selectedEndDate={new Date(selectedEndDate)}
              pInOutData={pInOutData}
              filetredDevices={filetredDevices}
              OnDeviceChange={handleDeviceChange}
              selectedCamera={selectedCamera}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
              finalRulesValue={finalRulesValue}
            />
          ) : expanded === "Option2" ? (
            <People2_1_Option2
              customizedWidth={customizedWidth}
              customizedHeight={height}
              selectedStartDate={new Date(selectedStartDate)}
              selectedEndDate={new Date(selectedEndDate)}
              pInOutData={pInOutData}
              filetredDevices={filetredDevices}
              OnDeviceChange={handleDeviceChange}
              selectedCamera={selectedCamera}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
              finalRulesValue={finalRulesValue}
            />
          ) : (
            <People2_1_Option1
              customizedWidth={customizedWidth}
              customizedHeight={height}
              selectedStartDate={new Date(selectedStartDate)}
              selectedEndDate={new Date(selectedEndDate)}
              pInOutData={pInOutData}
              filetredDevices={filetredDevices}
              OnDeviceChange={handleDeviceChange}
              selectedCamera={selectedCamera}
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
  // useEffect(() => {
  //   if (!localLoading && !hasDispatchedRef.current) {
  //     hasDispatchedRef.current = true;
  //     onLoadComplete?.();
  //   }
  // }, [localLoading, onLoadComplete]);

  return (
    <>
      {loadingCount > 0 && !IsDisplayLoader ? (
        <LocalLoader width={width} height={height} size={50} color="warning" />
      ) : (
          <>
            {renderLayout()}
            {/* PDF export table: date-wise In/Out counts */}
            {pdfMode && pInOutData  && (
              <div style={{ marginTop: 8 }}>
                {/* Aggregate by date (YYYY-MM-DD) so summary shows one row per day */}
                {(() => {
                  const map: Record<string, { date: string; inCount: number; outCount: number }> = {};

                  pInOutData.forEach((d) => {
                    if (!d?.dateTime) return;

                    //  Force UTC (important)
                    const utcDate = new Date(d.dateTime + "Z");
                
                    // Convert to local time automatically
                    const year = utcDate.getFullYear();
                    const month = String(utcDate.getMonth() + 1).padStart(2, "0");
                    const day = String(utcDate.getDate()).padStart(2, "0");

                    const key = `${year}-${month}-${day}`;

                    if (!map[key]) {
                      map[key] = { date: key, inCount: 0, outCount: 0 };
                    }

                    map[key].inCount += d.inCount ?? 0;
                    map[key].outCount += d.outCount ?? 0;
                  });

                  const grouped = Object.values(map)
                    .filter((r) => r.inCount !== 0 || r.outCount !== 0)
                    .sort((a, b) => a.date.localeCompare(b.date));

                    /* APPLY PERCENTAGE RULE ONLY FOR SUMMARY TABLE */
                  const rulePercent = finalRulesValue || 0;

                  const adjustedGrouped = grouped.map((row) => {
                    const adjustedIn =
                      row.inCount + (row.inCount * rulePercent) / 100;

                    const adjustedOut =
                      row.outCount + (row.outCount * rulePercent) / 100;

                    return {
                      ...row,
                      inCount: adjustedIn,
                      outCount: adjustedOut,
                    };
                  });

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
                              backgroundColor: "#f0f0f0",
                              pageBreakInside: "avoid",
                              breakInside: "avoid",
                            }}
                          >
                            <th style={thStyle}> Date</th>
                            <th style={thStyle}>In</th>
                            <th style={thStyle}>Out</th>
                          </tr>
                        </thead>
                        <tbody>
                            {adjustedGrouped.length > 0 ? (
                        <>
                          {adjustedGrouped.map((row) => {
                            // row.date is YYYY-MM-DD (local date key). Create a local Date object safely:
                            const convertedDatetime = formatDateToConfiguredTimezone(row.date) as string;                          
                            const dateLabel = formatDateToCustomFormat(convertedDatetime,"DD/MM/YYYY")

                            return (
                              <tr 
                              key={row.date}
                              style={{
                                pageBreakInside: "avoid",
                                breakInside: "avoid",
                              }}
                              >
                                <td style={tdStyle}>{dateLabel}</td>
                                <td style={tdStyle}>{formatNumber(row.inCount)}</td>
                                <td style={tdStyle}>{formatNumber(row.outCount)}</td>
                              </tr>
                            );
                          })}
                          {/* Totals and Average rows */}
                          {(() => {
                            const date1 = new Date(selectedStartDate);
                            const date2 = new Date(selectedEndDate);
                            const diffDays = Math.max(
                              1,
                              Math.floor((date2.getTime() - date1.getTime()) / (1000 * 60 * 60 * 24)) + 1
                            );
                            const totalIn = adjustedGrouped.reduce((s, r) => s + (r.inCount || 0), 0);
                            const totalOut = adjustedGrouped.reduce((s, r) => s + (r.outCount || 0), 0);
                            const avgIn = adjustedGrouped.length > 0 ? (totalIn / diffDays) : 0;
                            const avgOut = adjustedGrouped.length > 0 ?(totalOut /diffDays) : 0;
                            return (
                              <>
                                <tr 
                                style={{ 
                                  fontWeight: 600, 
                                  backgroundColor: "#f5f5f5",
                                  pageBreakInside: "avoid",
                                  breakInside: "avoid",
                                  }}>
                                   <td style={tdStyle}>Total</td>
                                   <td style={tdStyle}>{formatNumber(totalIn)}</td>
                                   <td style={tdStyle}>{formatNumber(totalOut)}</td>
                                </tr>
                                <tr 
                                style={{ 
                                  fontWeight: 600,
                                  pageBreakInside: "avoid",
                                  breakInside: "avoid",
                                  }}>
                                  <td style={{ border: "1px solid #eee", padding: 6 }}>Average</td>
                                    <td style={tdStyle}>{formatNumber(avgIn)}</td>
                                    <td style={tdStyle}>{formatNumber(avgOut)}</td>
                                </tr>
                              </>
                            );
                          })()}
                          </>) : (
                            <tr>
                              <td
                                colSpan={3}
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

                      {/* Summary text */}
                      {adjustedGrouped.length > 0 && (() => {
                        const highest = adjustedGrouped.reduce((max, r) => (r.inCount > max.inCount ? r : max), adjustedGrouped[0]);
                        const lowest = adjustedGrouped.reduce((min, r) => (r.inCount < min.inCount ? r : min), adjustedGrouped[0]);
                        const highestLabel =
                          formatDateToCustomFormat((formatDateToConfiguredTimezone(highest.date) as string),"DD/MM/YYYY");

                        const lowestLabel =
                          formatDateToCustomFormat((formatDateToConfiguredTimezone(lowest.date) as string),"DD/MM/YYYY");

                        return (
                          <div style={{ marginTop: 8 }}>
                            <div style={{ fontWeight: 600, marginBottom: 6 }}>Summary</div>
                            <ul style={{ marginTop: 0, marginBottom: 0, paddingLeft: 18 }}>
                              <li  style={{ lineHeight: "18px"}}>
                                Day {highestLabel} has a highest people in count value ({formatNumber(highest.inCount)})
                              </li>
                              <li style={{ lineHeight: "18px"}}> 
                                Day {lowestLabel} has a lowest people in count value ({formatNumber(lowest.inCount)})
                              </li>
                            </ul>
                          </div>
                        );
                      })()}
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

export { PeopleWidget };
