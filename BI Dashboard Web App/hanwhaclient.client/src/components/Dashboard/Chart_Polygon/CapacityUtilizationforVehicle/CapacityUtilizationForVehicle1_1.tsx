import React, { useEffect } from "react";
import { Box, Typography, Paper, Avatar } from "@mui/material";
import moment from "moment";
import { DateWiseUtilization, ICapacityUtilizationforVehicleProps } from "../../../../interfaces/IChart";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";

const CapacityUtilizationForVehicle1_1: React.FC<
  ICapacityUtilizationforVehicleProps
> = ({
  customizedWidth,
  CUForVehicleData,
  DateWiseUtilization,
  displayName,
  onZoomClick,
  openZoomDialog,
  setIsDraggable,
}) => {
    const { theme } = useThemeContext();

    const [overallMin, setOverallMin] = React.useState<DateWiseUtilization | null>(null);
    const [overallMax, setOverallMax] = React.useState<DateWiseUtilization | null>(null);
    const [overallTotal, setOverallTotal] = React.useState<number>(0);
    const [utilizationPercentage, setUtilizationPercentage] = React.useState<number | null>(null);

const isExceeded = overallTotal > (CUForVehicleData?.totalCapacity ?? 0);

    useEffect(() => {
      if (!DateWiseUtilization || DateWiseUtilization.length === 0) {
        setOverallMin(null);
        setOverallMax(null);
        setOverallTotal(0);
        setUtilizationPercentage(0);
        return;
      }

      function groupByDate(data: DateWiseUtilization[]) {
        const grouped: Record<string, DateWiseUtilization[]> = {};

        data.forEach((item) => {
          const date = moment(item.dateTime).format("YYYY-MM-DD");
          if (!grouped[date]) {
            grouped[date] = [];
          }
          grouped[date].push({
            ...item,
            totalCount: item.totalCount,
            // totalCount: item.totalCount < 0 ? 0 : item.totalCount, // optional clamp
          });
        });

        return grouped;
      }

      //  Compute min, max, and total using latest entries
      function findMaxByDate(data: DateWiseUtilization[]) {
        const grouped = groupByDate(data);

        const result = Object.entries(grouped).map(([date, values]) => {
          const max = values.reduce((max, item) =>
            item.totalCount > max.totalCount ? item : max
          );
          return { date, max };
        });

        return result;
      }


      const MaxByDate = findMaxByDate(DateWiseUtilization);

      const overallMinval = MaxByDate
        .filter(item => item.max.totalCount > 0) // ignore 0
        .reduce((min, item) =>
          item.max.totalCount < min.max.totalCount ? item : min
          , MaxByDate[0]);

      const overallMaxval = MaxByDate.reduce((max, item) =>
        item.max.totalCount > max.max.totalCount ? item : max
      );    
    
      let overallTotal = 0;
      if (MaxByDate.length === 1 && DateWiseUtilization.length > 0) {
        overallTotal =
          DateWiseUtilization[DateWiseUtilization.length - 1].totalCount;
      } else {
        overallTotal = MaxByDate.reduce(
          (sum, item) => sum + (item.max.totalCount ?? 0),
          0
        );
      }

      const vehicleDefaultOccupancy = CUForVehicleData?.vehicleDefaultOccupancy ?? 0;
      const adjustedOverallTotal = overallTotal + vehicleDefaultOccupancy;
      const totalCapacity = CUForVehicleData?.totalCapacity ?? 0;
      const utilizationPercentage =
        totalCapacity > 0 ? (adjustedOverallTotal / totalCapacity) * 100 : 0;


      //  Clamp all negatives to 0 before setting state
      const safeOverallMin =
        overallMinval && overallMinval.max
          ? { ...overallMinval.max, totalCount: Math.max(0, overallMinval.max.totalCount) }
          : null;

      const safeOverallMax =
        overallMaxval && overallMaxval.max
          ? { ...overallMaxval.max, totalCount: Math.max(0, overallMaxval.max.totalCount) }
          : null;

      const safeOverallTotal = Math.max(0, adjustedOverallTotal);      
      const safeUtilizationPercentage = Math.max(0, utilizationPercentage);

      //  Update states
      setOverallMin(safeOverallMin);
      setOverallMax(safeOverallMax);
      setOverallTotal(safeOverallTotal);
      setUtilizationPercentage(safeUtilizationPercentage);

      // //  Update states
      // setOverallMin(overallMinval?.max ?? null);
      // setOverallMax(overallMaxval.max);
      // setOverallTotal(overallTotal);
      // setUtilizationPercentage(utilizationPercentage);
    }, [DateWiseUtilization, CUForVehicleData]);


    return (
      <Box sx={{ width: customizedWidth }}>
        <Box className="widget-main-wrapper">
          <Box className="widget-main-header">
            <Typography variant="h6" component="h2">
              {displayName}
            </Typography>
          </Box>

          <Box className="widget-main-body">
            <div className="widget-data-wrapper">
              <Box className="capacity-utilization-image">
                <Box className="capacity-utilization-blue-box">
                  <Avatar className="capacity-utilization-car">
                    <img
                      src="/images/dashboard/Capacity_Utilization_for_Vehicle.gif"
                      alt="car image"
                      height={50}
                      width={50}
                    />
                  </Avatar>
                  <Box className="capacity-utilization-car-details">
                    <Box className="capacity-utilization-car-detail-left">
                      <Typography variant="h6" fontWeight="bold" className={isExceeded ? "blink-utilization" : ""}>
                        {formatNumber(
                          Math.round(overallTotal as number) ?? 0
                        )}
                      </Typography>
                      <Typography variant="body2">Utilization</Typography>
                    </Box>
                    <Box className="capacity-utilization-car-detail-right">
                      <Typography variant="h6" fontWeight="bold">
                        {formatNumber(Math.round(utilizationPercentage as number)) ?? 0}%
                      </Typography>
                      <Typography variant="body2">Percentage</Typography>
                    </Box>
                  </Box>
                </Box>

                <Box className="capacity-utilization-day-details">
                  {[
                    {
                      label: "Most Day",
                      value:
                        overallMax?.totalCount ?? 0,
                      date: (
                        overallMax?.dateTime ?? ""
                      ),
                      icon: "/images/up_arrow.svg",
                    },
                    {
                      label: "Least Day",
                      value:
                        overallMin?.totalCount ?? 0,
                      date: (overallMin?.dateTime ?? ""
                      ),
                      icon: "/images/down_arrow.svg",
                    },
                  ].map((item, idx) => (
                    <Paper
                      key={idx}
                      elevation={0}
                      className="capacity-utilization-day-details-main"
                    >
                      <Box className="capacity-utilization-day">
                        <Typography variant="body2" noWrap>
                          {item.label}
                        </Typography>
                        <Box
                          component="img"
                          src={item.icon}
                          alt={`${item.label} Arrow`}
                          sx={{ height: 11, width: 16 }}
                        />
                      </Box>
                      <Typography variant="h6" fontWeight="bold">
                        {formatNumber(Math.round(item.value as number) ?? 0)}
                      </Typography>
                      <Typography variant="caption">
                        {/* {moment(item.date).format("D MMM YYYY") ?? "—"} */}
                        {!item.date || item.date.startsWith("0001-01-01")
                          ? "—"
                          : moment(item.date).format("D MMM YYYY")}

                      </Typography>
                    </Paper>
                  ))}
                </Box>
              </Box>
            </div>
          </Box>

          <Box className="widget-main-footer">
            <Box className="widget-main-footer-value">
              <Typography>Total No. of Capacity : </Typography>
              <span>{formatNumber(CUForVehicleData?.totalCapacity ?? 0)}</span>
            </Box>
            {!openZoomDialog ? (
              <Box
                className="widget-main-footer-zoom-i"
                onMouseEnter={() => setIsDraggable?.(true)}
                onMouseLeave={() => setIsDraggable?.(false)}
              >
                <img
                  src={theme === 'light' ? "/images/dashboard/drag.svg" : "/images/dark-theme/dashboard/drag.svg"}
                  alt="vehicle"
                  width={35}
                  height={35}
                />
              </Box>
            ) : null}
            {!openZoomDialog ? (
              <Box className="widget-main-footer-zoom-i" onClick={onZoomClick} id="zoomWidgetBtnCapacityUtilizationforVehicle">
                <img
                  src={theme === 'light' ? "/images/dashboard/ZoomWidget.svg" : "/images/dark-theme/dashboard/ZoomWidget.svg"}
                  alt="vehicle"
                  width={35}
                  height={35}
                />
              </Box>
            ) : null}
          </Box>
        </Box>
      </Box>
    );
  };

export { CapacityUtilizationForVehicle1_1 };
