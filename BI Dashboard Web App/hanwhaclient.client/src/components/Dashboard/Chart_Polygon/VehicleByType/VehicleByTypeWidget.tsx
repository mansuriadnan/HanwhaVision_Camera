import { useEffect, useRef, useState } from "react";
import { Box } from "@mui/material";
import {
  IVehicleByTypeCountData,
  IWidgetPayload,
  LayoutItem,
  CommonWidgetProps,
} from "../../../../interfaces/IChart";
import { fetchVehicleByTypeLineChartDataService } from "../../../../services/dashboardService";
import {
  VehicleByType1_1,
  VehicleByType2_1_Option1,
  VehicleByType2_1_Option2,
  VehicleByType2_1_Option3,
  CommonDialog,
  LocalLoader,
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

const VehicleByTypeWidget: React.FC<CommonWidgetProps> = ({
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
  const customizedWidth =
    size === "3x1" ? width / 3 : size === "2x1" ? width / 2 : width;
  const [vehicleByTypeCount, setVehicleByTypeCountData] =
    useState<IVehicleByTypeCountData>();
  const [openZoomDialog, setOpenZoomDialog] = useState(false);
  const [vehicleDataWithTime, setVehicleDataWithTime] =
    useState<IVehicleByTypeCountData[]>();
  const [loadingCount, setLoadingCount] = useState(0);
  const selectedIntervalNameRef = useRef<string>("");
  const { IsDisplayLoader } = useSignalRContext();
  const [type, setType] = useState<"In" | "Out">("In");

  useEffect(() => {
    fetchVehicleByTypeDataWithTime();
  }, [floor, zones, selectedStartDate, selectedEndDate, size, expanded]);

  useExportHandler({
    apiEndpoint: `${apiUrls.VehicleByTypeLineChartData}/csv`,
    startDate: convertDateToISOLikeString(selectedStartDate),
    endDate: convertDateToISOLikeString(selectedEndDate),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  const fetchVehicleByTypeDataWithTime = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const data = {
        floorIds: floor,
        zoneIds: zones,
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const response: any = await fetchVehicleByTypeLineChartDataService(
        data as IWidgetPayload
      );

      if (response?.data.length > 0) {
        setVehicleDataWithTime(response?.data as IVehicleByTypeCountData[]);

        const totals = response?.data.reduce(
          (acc: { [x: string]: any; }, item: { [x: string]: any; }) => {
            Object.keys(acc).forEach((key) => {
              if (
                key === "totalInVehicleCount" ||
                key === "totalOutVehicleCount" ||
                key === "dateTime"
              )
                return;

              acc[key as keyof IVehicleByTypeCountData] =
                (acc[key as keyof IVehicleByTypeCountData] ?? 0) as number +
                (item[key as keyof IVehicleByTypeCountData] ?? 0) as number;
            });
            return acc;
          },
          {
            truckInCount: 0,
            motorCycleInCount: 0,
            busInCount: 0,
            bicycleInCount: 0,
            carInCount: 0,
            truckOutCount: 0,
            motorCycleOutCount: 0,
            busOutCount: 0,
            bicycleOutCount: 0,
            carOutCount: 0,
          } as IVehicleByTypeCountData
        );


        Object.keys(totals).forEach((key) => {
          if (key !== "totalInVehicleCount" && key !== "totalOutVehicleCount") {
            totals[key as keyof IVehicleByTypeCountData] =
              ((totals[key as keyof IVehicleByTypeCountData] as number) ?? 0) 
          }
        });

        totals.totalInVehicleCount =
          totals.truckInCount +
          totals.motorCycleInCount +
          totals.busInCount +
          totals.bicycleInCount +
          totals.carInCount;

        totals.totalOutVehicleCount =
          totals.truckOutCount +
          totals.motorCycleOutCount +
          totals.busOutCount +
          totals.bicycleOutCount +
          totals.carOutCount;

        setVehicleByTypeCountData(totals);

      } else {
        setVehicleDataWithTime([]);
        setVehicleByTypeCountData({
          truckInCount: 0,
          motorCycleInCount: 0,
          busInCount: 0,
          bicycleInCount: 0,
          carInCount: 0,
          truckOutCount: 0,
          motorCycleOutCount: 0,
          busOutCount: 0,
          bicycleOutCount: 0,
          carOutCount: 0,
          totalInVehicleCount: 0,
          totalOutVehicleCount: 0,
        });
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

  const handleTypeChange = (type: "In" | "Out") => {
    if (type) {
      setType(type);
    }
  };

  const renderLayout = () => {
    return (
      <Box sx={{ height: height, display: "flex" }}>
        <VehicleByType1_1
          vehicleByTypeCountData={vehicleByTypeCount}
          customizedWidth={customizedWidth}
          customizedHeight={height}
          displayName={displayName}
          onZoomClick={handleZoomClick}
          openZoomDialog={openZoomDialog}
          setIsDraggable={setIsDraggable}
          handleTypeChange={handleTypeChange}
          finalRulesValue={finalRulesValue}
        ></VehicleByType1_1>

        {size === "2x1" &&
          (expanded === "Option3" ? (
            <VehicleByType2_1_Option3
              customizedWidth={customizedWidth}
              customizedHeight={height}
              vehicleByTypeCountData={vehicleByTypeCount}
              type={type}
            />
          ) : expanded === "Option2" ? (
            <VehicleByType2_1_Option2
              customizedWidth={customizedWidth}
              customizedHeight={height}
              vehicleDataWithTime={vehicleDataWithTime}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              vehicleByTypeCountData={vehicleByTypeCount}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
              type={type}
              finalRulesValue={finalRulesValue}
            />
          ) : (
            <VehicleByType2_1_Option1
              customizedWidth={customizedWidth}
              customizedHeight={height}
              vehicleByTypeCountData={vehicleByTypeCount}
              type={type}
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

  return (
    <>
      {loadingCount > 0 && !IsDisplayLoader ? (
        <LocalLoader width={width} height={height} size={50} color="warning" />
      ) : (
        <>
          {renderLayout()}
           
            {pdfMode && vehicleDataWithTime && (
              <div style={{ marginTop: 8 }}>
                {(() => {
                  const map: Record<string, IVehicleByTypeCountData[]> = {};
                  const date1 = new Date(selectedStartDate);
                  const date2 = new Date(selectedEndDate);
                  const diffDays = Math.max(
                    1,
                    Math.floor((date2.getTime() - date1.getTime()) / (1000 * 60 * 60 * 24)) + 1
                  );
                  // Group by date
                  vehicleDataWithTime.forEach((d) => {
                    if (!d?.dateTime) return;

                    // const key = new Date(d.dateTime).toISOString().split("T")[0];
                    const key = formatDateToCustomFormat(formatDateToConfiguredTimezone(d.dateTime),"DD/MM/YYYY");
                    //const key = formatDateToConfiguredTimezone(d.dateTime).split(" ")[0];

                    if (!map[key]) map[key] = [];
                    map[key].push(d);
                  });
                   const multiplier = 1 + (finalRulesValue || 0) / 100;

                  const groupedDates = Object.keys(map).sort();
                  const allDates = groupedDates; // includes zero days also
                  const filteredData = groupedDates.filter((dateKey) => {
                    const dayData = map[dateKey];

                    const totals = dayData.reduce(
                      (acc, item) => {
                        acc +=
                          (item.truckInCount ?? 0) +
                          (item.motorCycleInCount ?? 0) +
                          (item.busInCount ?? 0) +
                          (item.bicycleInCount ?? 0) +
                          (item.carInCount ?? 0) +
                          (item.truckOutCount ?? 0) +
                          (item.motorCycleOutCount ?? 0) +
                          (item.busOutCount ?? 0) +
                          (item.bicycleOutCount ?? 0) +
                          (item.carOutCount ?? 0);

                        return acc;
                      },
                      0
                    );

                    return totals !== 0;
                  });

                  let totalIn = 0;
                  let totalOut = 0;

                  allDates.forEach((dateKey) => {
                    const dayData = map[dateKey];

                    const totals = dayData.reduce((acc, item) => {
                      acc.in +=
                        (item.truckInCount ?? 0) +
                        (item.motorCycleInCount ?? 0) +
                        (item.busInCount ?? 0) +
                        (item.bicycleInCount ?? 0) +
                        (item.carInCount ?? 0);

                      acc.out +=
                        (item.truckOutCount ?? 0) +
                        (item.motorCycleOutCount ?? 0) +
                        (item.busOutCount ?? 0) +
                        (item.bicycleOutCount ?? 0) +
                        (item.carOutCount ?? 0);

                      return acc;
                    }, { in: 0, out: 0 });

                    totalIn += totals.in * multiplier;
                    totalOut += totals.out * multiplier;
                  });

                  const avgIn = allDates.length > 0 ? totalIn / diffDays : 0;
                  const avgOut = allDates.length > 0 ? totalOut / diffDays : 0;                 

                  const overallTypes = {
                    Truck: 0,
                    Car: 0,
                    Bus: 0,
                    MotorCycle: 0,
                    Bicycle: 0,
                  };                 

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
                              backgroundColor: "#f0f0f0",
                              pageBreakInside: "avoid",
                              breakInside: "avoid",
                            }}>
                            <th style={thStyle}>Date</th>
                            <th style={thStyle}>Vehicle Type</th>
                            <th style={thStyle}>In</th>
                            <th style={thStyle}>Out</th>
                          </tr>
                        </thead>

                        <tbody>
                          {filteredData.length > 0 ? (
                            <>
                              {filteredData.flatMap((dateKey) => {
                                const dayData = map[dateKey];

                                const totals = dayData.reduce<IVehicleByTypeCountData>(
                                  (acc, item) => {
                                    acc.truckInCount += item.truckInCount ?? 0;
                                    acc.motorCycleInCount += item.motorCycleInCount ?? 0;
                                    acc.busInCount += item.busInCount ?? 0;
                                    acc.bicycleInCount += item.bicycleInCount ?? 0;
                                    acc.carInCount += item.carInCount ?? 0;

                                    acc.truckOutCount += item.truckOutCount ?? 0;
                                    acc.motorCycleOutCount += item.motorCycleOutCount ?? 0;
                                    acc.busOutCount += item.busOutCount ?? 0;
                                    acc.bicycleOutCount += item.bicycleOutCount ?? 0;
                                    acc.carOutCount += item.carOutCount ?? 0;

                                    return acc;
                                  },
                                  {
                                    truckInCount: 0,
                                    motorCycleInCount: 0,
                                    busInCount: 0,
                                    bicycleInCount: 0,
                                    carInCount: 0,
                                    truckOutCount: 0,
                                    motorCycleOutCount: 0,
                                    busOutCount: 0,
                                    bicycleOutCount: 0,
                                    carOutCount: 0,
                                    totalInVehicleCount: 0,
                                    totalOutVehicleCount: 0,
                                    dateTime: "",
                                  }
                                );

                                // Apply %
                                Object.keys(totals).forEach((key: any) => {
                                  if (typeof totals[key] === "number") {
                                    totals[key] = totals[key] * multiplier;
                                  }
                                });

                                // Update overall summary
                                overallTypes.Truck += totals.truckInCount;
                                overallTypes.Car += totals.carInCount;
                                overallTypes.Bus += totals.busInCount;
                                overallTypes.MotorCycle += totals.motorCycleInCount;
                                overallTypes.Bicycle += totals.bicycleInCount;

                                // const dateLabel = new Date(dateKey).toLocaleDateString();    
                                //const dateLabel = formatDateToCustomFormat(formatDateToConfiguredTimezone(dateKey),"DD/MM/YYYY");
                                const dateLabel = dateKey;

                                return [
                                  ["Truck", totals.truckInCount, totals.truckOutCount],
                                  ["Car", totals.carInCount, totals.carOutCount],
                                  ["Bus", totals.busInCount, totals.busOutCount],
                                  ["Bicycle", totals.bicycleInCount, totals.bicycleOutCount],
                                  ["MotorCycle", totals.motorCycleInCount, totals.motorCycleOutCount],
                                ].map(([type, inVal, outVal]) => (
                                  <tr
                                    key={`${dateKey}-${type}`}
                                    style={{
                                      pageBreakInside: "avoid",
                                      breakInside: "avoid",
                                    }}
                                  >
                                    <td style={tdStyle}>{dateLabel}</td>
                                    <td style={tdStyle}>{type}</td>
                                    <td style={tdStyle}>{formatNumber(inVal)}</td>
                                    <td style={tdStyle}>{formatNumber(outVal)}</td>
                                  </tr>
                                ));
                              })}
                              {/* Total */}
                              <tr style={{ fontWeight: 600, backgroundColor: "#f5f5f5" }}>
                                <td style={tdStyle}>Total</td>
                                <td style={tdStyle}></td>
                                <td style={tdStyle}>{formatNumber(totalIn)}</td>
                                <td style={tdStyle}>{formatNumber(totalOut)}</td>
                              </tr>

                              {/* Average */}
                              <tr style={{ fontWeight: 600 }}>
                                <td style={tdStyle}>Average</td>
                                <td style={tdStyle}></td>
                                <td style={tdStyle}>{formatNumber(avgIn)}</td>
                                <td style={tdStyle}>{formatNumber(avgOut)}</td>
                              </tr>
                            </>) : (
                            <tr>
                              <td
                                colSpan={4}
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
                     
                        {filteredData.length> 0 && (() => {
                          
                          const entries = Object.entries(overallTypes).map(([name, value]) => ({
                            name,
                            value,
                          }));

                          const highest = entries.reduce((a, b) =>
                            b.value > a.value ? b : a
                          );
                          const lowest = entries.reduce((a, b) =>
                            b.value < a.value ? b : a
                          );

                          return (
                            <div style={{ marginTop: 8 }}>
                            <div style={{ fontWeight: 600, marginBottom: 6 }}>Summary</div>
                             <ul style={{ marginTop: 0, marginBottom: 0, paddingLeft: 18 }}>
                              <li style={{ lineHeight: "18px" }}>
                                {highest.name} has the highest vehicle In count ({formatNumber(highest.value)})
                              </li>
                              <li style={{ lineHeight: "18px" }}>
                                {lowest.name} has the lowest vehicle In count ({formatNumber(lowest.value)})
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
export { VehicleByTypeWidget };
