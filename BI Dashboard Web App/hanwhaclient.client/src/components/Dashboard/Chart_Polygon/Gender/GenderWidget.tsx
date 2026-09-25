import React, { useEffect, useRef, useState } from "react";
import {
  CommonWidgetProps,
  IWidgetPayload,
  IGenderDataWithTime,
  LayoutItem,
} from "../../../../interfaces/IChart";
import {
  Gender1_1,
  Gender2_1_Option1,
  Gender2_1_Option2,
  Gender3_1_Option1,
  CommonDialog,
  LocalLoader,
} from "../../../index";
import { fetchGenderWisePeopleCountDataWithTimeService } from "../../../../services/dashboardService";
import { Box } from "@mui/material";
import { convertToUTC } from "../../../../utils/convertToUTC";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import apiUrls from "../../../../constants/apiUrls";
import { useSignalRContext } from "../../../../context/SignalRContext";
import { formatDateToConfiguredTimezone } from "../../../../utils/formatDateToConfiguredTimezone";
import moment from "moment";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";
import { formatNumber } from "../../../../utils/formatNumber";
import { formatDateToCustomFormat } from "../../../../utils/dateUtils";

const thStyle = {
  border: "1px solid #ccc",
  padding: "0px 4px",
  textAlign: "left",
  lineHeight: "18px",
};

const tdStyle = {
  border: "1px solid #ccc",
  padding: "0px 4px",
  lineHeight: "18px",
};

const GenderWidget: React.FC<CommonWidgetProps> = ({
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
  const { width, height, size, expanded, displayName, chartName } =
    item as LayoutItem;
  const customizedWidth =
    size === "3x1" ? width / 3 : size === "2x1" ? width / 2 : width;
  const [openZoomDialog, setOpenZoomDialog] = useState(false);
  const [genderDataWithTime, setGenderDataWithTime] = useState<
    IGenderDataWithTime[] | null
  >([]);
  const [loadingCount, setLoadingCount] = useState(0);
  const selectedIntervalNameRef = useRef<string>("");
  const { IsDisplayLoader } = useSignalRContext();
  const [groupbyDateData, setGroupbyDateData] = useState<
    IGenderDataWithTime[] | null
  >([]);

  useExportHandler({
    apiEndpoint: `${apiUrls.GenderWisePeopleCountAnalysisCsv}/csv`,
    startDate: convertDateToISOLikeString(selectedStartDate),
    endDate: convertDateToISOLikeString(selectedEndDate),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  useEffect(() => {
    fetchGenderWisePeopleCountDataWithTime();
  }, [floor, zones, selectedStartDate, selectedEndDate, finalRulesValue]);

  const fetchGenderWisePeopleCountDataWithTime = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const data = {
        floorIds: floor,
        zoneIds: zones,
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const response: any = await fetchGenderWisePeopleCountDataWithTimeService(
        data as IWidgetPayload,
      );

      if (response?.data.length > 0) {
        setGenderDataWithTime(response?.data as IGenderDataWithTime[]);
        const formattedData = response?.data.map((d: IGenderDataWithTime) => ({
          ...d,
          date: new Date(formatDateToConfiguredTimezone(d.dateTime) as string),
        }));

        // Group by "yyyy-MM-dd" (ignoring time part)
        const groupedData = formattedData.reduce((acc: any, curr: any) => {
          const key = moment(curr.date).format("YYYY-MM-DD");

          if (!acc[key]) {
            acc[key] = {
              date: key,
              maleCount: curr.maleCount,
              femaleCount: curr.femaleCount,
              undefinedCount: curr.undefinedCount,
            };
          } else {
            acc[key].maleCount += curr.maleCount;
            acc[key].femaleCount += curr.femaleCount;
            acc[key].undefinedCount += curr.undefinedCount;
          }
          return acc;
        }, {});

        // Convert back to array
        const groupedArray: IGenderDataWithTime[] = Object.values(groupedData);
        setGroupbyDateData(groupedArray);
      } else {
        setGenderDataWithTime([]);
        setGroupbyDateData([]);
      }
    } catch (error) {
      console.error("Error fetching Gender data:", error);
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
        <Gender1_1
          customizedWidth={customizedWidth}
          displayName={displayName}
          onZoomClick={handleZoomClick}
          openZoomDialog={openZoomDialog}
          setIsDraggable={setIsDraggable}
          groupbyDateData={groupbyDateData}
          finalRulesValue={finalRulesValue}
        ></Gender1_1>

        {size === "2x1" &&
          (expanded === "Option3" ? null : expanded === "Option2" ? (
            <Gender2_1_Option2
              customizedWidth={customizedWidth}
              customizedHeight={height}
              genderDataWithTime={genderDataWithTime}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
              finalRulesValue={finalRulesValue}
            />
          ) : (
            <Gender2_1_Option1
              customizedWidth={customizedWidth}
              customizedHeight={height}
              groupbyDateData={groupbyDateData}
            />
          ))}

        {size === "3x1" &&
          (expanded === "Option1" ? (
            <Gender3_1_Option1
              customizedWidth={773}
              customizedHeight={height}
              groupbyDateData={groupbyDateData}
            />
          ) : null)}
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

  return (
    <>
      {loadingCount > 0 && !IsDisplayLoader ? (
        <LocalLoader width={width} height={height} size={50} color="warning" />
      ) : (
        <>
          {renderLayout()}
         
          {pdfMode && groupbyDateData &&  (
            <div style={{ marginTop: 8 }}>
              {(() => {
                const date1 = new Date(selectedStartDate);
                const date2 = new Date(selectedEndDate);
                const diffDays = Math.max(
                    1,
                    Math.floor((date2.getTime() - date1.getTime()) / (1000 * 60 * 60 * 24)) + 1
                  );
                const multiplier = 1 + (finalRulesValue || 0) / 100;

                const overall = {
                  Male: 0,
                  Female: 0,
                  Undefined: 0,
                };

                const sortedData = [...groupbyDateData].sort((a: any, b: any) =>
                  a.date.localeCompare(b.date),
                );

                  const allData = sortedData.map((day: any) => {
                    const male = (day.maleCount ?? 0) * multiplier;
                    const female = (day.femaleCount ?? 0) * multiplier;
                    const undefinedCount = (day.undefinedCount ?? 0) * multiplier;

                    return male + female + undefinedCount; // total per day
                  });
                  const filteredData = sortedData.filter((day: any) => {
                    const male = ((day.maleCount ?? 0) * multiplier);
                    const female = ((day.femaleCount ?? 0) * multiplier);
                    const undefinedCount = ((day.undefinedCount ?? 0) * multiplier);

                    return !(male === 0 && female === 0 && undefinedCount === 0);
                  });

                  const total = allData.reduce((s, v) => s + v, 0);

                  const average =
                    allData.length > 0 ? total / diffDays : 0;

                return (
                  <div style={{
                          padding: "0px 15px 30px 15px "                            
                          }}> 
                 
                    <table
                      className="report-table-sch"
                      style={{
                        width: "100%",
                        borderCollapse: "collapse",
                        tableLayout: "fixed",
                        fontSize: "12px",
                        pageBreakInside: "avoid",
                        breakInside: "avoid",
                      }}
                    >
                      <thead>
                        <tr
                          style={{
                            pageBreakInside: "avoid",
                            breakInside: "avoid",
                            backgroundColor: "#f0f0f0",
                          }}
                        >
                          <th style={thStyle}>Date</th>
                          <th style={thStyle}>Gender</th>
                          <th style={thStyle}>Count</th>
                        </tr>
                      </thead>

                      <tbody>
                        {filteredData.length > 0 ? (
                          <>
                            {filteredData.flatMap((day: any) => {
                              const male = (day.maleCount ?? 0) * multiplier;
                              const female = (day.femaleCount ?? 0) * multiplier;
                              const undefinedCount = (day.undefinedCount ?? 0) * multiplier;

                              // Update overall
                              overall.Male += male;
                              overall.Female += female;
                              overall.Undefined += undefinedCount;

                              const convertedDatetime = formatDateToConfiguredTimezone(day.date) as string;
                              const dateLabel = formatDateToCustomFormat(convertedDatetime,"DD/MM/YYYY");
                              return [
                                ["Male", male],
                                ["Female", female],
                                ["Undefined", undefinedCount],
                              ].map(([type, value]) => (
                                <tr
                                  key={`${day.date}-${type}`}
                                  style={{
                                    pageBreakInside: "avoid",
                                    breakInside: "avoid",
                                  }}
                                >
                                  <td style={tdStyle}>{dateLabel}</td>
                                  <td style={tdStyle}>{type}</td>
                                  <td style={tdStyle}>{formatNumber(value)}</td>
                                </tr>
                              ));
                            })}
                            {/* Total */}
                            <tr style={{ fontWeight: 600, backgroundColor: "#f5f5f5" }}>
                              <td style={tdStyle}>Total</td>
                              <td style={tdStyle}></td>
                              <td style={tdStyle}>{formatNumber(total)}</td>
                            </tr>

                            {/* Average */}
                            <tr style={{ fontWeight: 600 }}>
                              <td style={tdStyle}>Average</td>
                              <td style={tdStyle}></td>
                              <td style={tdStyle}>{formatNumber(average)}</td>
                            </tr>
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
                 
                    {/* Final Summary */}

                    {filteredData.length > 0 && (() => {
                      const entries = Object.entries(overall).map(
                        ([name, value]) => ({
                          name,
                          value,
                        }),
                      );

                      const highest = entries.reduce((a, b) =>
                        b.value > a.value ? b : a,
                      );
                      const lowest = entries.reduce((a, b) =>
                        b.value < a.value ? b : a,
                      );

                      return (
                        <div style={{ marginTop: 8 }}>
                          <div style={{ fontWeight: 600, marginBottom: 6 }}>
                            Summary
                          </div>
                          <ul
                            style={{
                              marginTop: 0,
                              marginBottom: 12,
                              paddingLeft: 18,
                            }}
                          >
                            <li style={{ lineHeight: "18px" }}>
                              {highest.name} has the highest count (
                              {formatNumber(highest.value)})
                            </li>
                            <li style={{ lineHeight: "18px" }}>
                              {lowest.name} has the lowest count ({formatNumber(lowest.value)}
                              )
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

export { GenderWidget };
