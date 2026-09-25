import React, { useEffect, useRef, useState } from "react";
import { AverageVehicleCountProps, IParsedDataWithCategory, ParsedDataFormat } from "../../../../interfaces/IChart";
import { Box, Typography } from "@mui/material";
import StatCard from "./StatCard";
import { formatDateToConfiguredTimezone } from "../../../../utils/formatDateToConfiguredTimezone";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import apiUrls from "../../../../constants/apiUrls";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";

const AverageVehicleCount1_1: React.FC<AverageVehicleCountProps> = ({
  averageVehicleCountChartData,
  customizedWidth,
  displayName,
  onZoomClick,
  openZoomDialog,
  setIsDraggable,
  floor,
  zones,
  startDate,
  endDate,
  setExportHandler,
  calculatedStats,
  setCalculatedStats
}) => {
  const { theme } = useThemeContext();
  const selectedIntervalNameRef = useRef<string>("");
  const [totalInVehicle, setTotalInVehicle] = useState<number>(0);

  useExportHandler({
    apiEndpoint: `${apiUrls.AverageVehicleCountChart}/csv`,
    startDate: convertDateToISOLikeString(startDate as Date),
    endDate: convertDateToISOLikeString(endDate as Date),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  type vehicleKey = "inCount" | "outCount";
  const categories: { key: vehicleKey; label: string; color: string }[] = [
    { key: "inCount", label: "Incoming Average", color: "#338BFF" },
    { key: "outCount", label: "Outgoing Average", color: "#FF8980" },
  ];

  useEffect(() => {
    if (!averageVehicleCountChartData) return;

    // Total
    const total = averageVehicleCountChartData.reduce(
      (sum, d) => sum + (d.inCount ?? 0),
      0
    );
    setTotalInVehicle(total);

    const updatedParsedData: IParsedDataWithCategory = {};
    categories.forEach((cat) => {
      updatedParsedData[cat.key] = averageVehicleCountChartData.map((d) => ({
        date: new Date(formatDateToConfiguredTimezone(d.dateTime) as string),
        value: d[cat.key] ?? 0,
      }));
    });

    // Group by day
    const inDaily = groupByDayAverage(updatedParsedData.inCount);
    const outDaily = groupByDayAverage(updatedParsedData.outCount);

    // Calculate stats
    const inStats = calculateStats(inDaily);
    const outStats = calculateStats(outDaily);

    // Set stats
    setCalculatedStats?.({
      averageInCount: inStats.avg,
      minInCount: inStats.min,
      maxInCount: inStats.max,
      minInDate: inStats.minDate
        ? formatDateToConfiguredTimezone(inStats.minDate.toISOString())
        : "",
      maxInDate: inStats.maxDate
        ? formatDateToConfiguredTimezone(inStats.maxDate.toISOString())
        : "",

      averageOutCount: outStats.avg,
      minOutCount: outStats.min,
      maxOutCount: outStats.max,
      minOutDate: outStats.minDate
        ? formatDateToConfiguredTimezone(outStats.minDate.toISOString())
        : "",
      maxOutDate: outStats.maxDate
        ? formatDateToConfiguredTimezone(outStats.maxDate.toISOString())
        : "",
    });

  }, [averageVehicleCountChartData]);

  const groupByDayAverage = (data: ParsedDataFormat[]): ParsedDataFormat[] => {
    const map = new Map<string, { sum: number; count: number }>();

    data.forEach(({ date, value }) => {
      const d = new Date(date);

      const key = `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;

      if (!map.has(key)) {
        map.set(key, { sum: 0, count: 0 });
      }

      const entry = map.get(key)!;
      entry.sum += value;
      entry.count += 1;
    });

    return Array.from(map.entries()).map(([key, { sum, count }]) => {
      const [year, month, day] = key.split("-").map(Number);

      return {
        date: new Date(year, month, day),
        value: count ? sum / count : 0,
      };
    });
  };


  const calculateStats = (data: ParsedDataFormat[]) => {
    const valid = data.filter((d) => d.value > 0);

    if (valid.length === 0) {
      return {
        avg: 0,
        min: 0,
        max: 0,
        minDate: null as Date | null,
        maxDate: null as Date | null,
      };
    }

    const avg =
      valid.reduce((sum, d) => sum + d.value, 0) / valid.length;

    const minObj = valid.reduce((min, d) =>
      d.value < min.value ? d : min
    );

    const maxObj = valid.reduce((max, d) =>
      d.value > max.value ? d : max
    );

    return {
      avg,
      min: minObj.value,
      max: maxObj.value,
      minDate: minObj.date,
      maxDate: maxObj.date,
    };
  };

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
            <div className="avg-vehicle-count incoming-counting">
              <StatCard
                title="Incoming Average"
                avg={calculatedStats?.averageInCount ?? null}
                min={calculatedStats?.minInCount ?? null}
                max={calculatedStats?.maxInCount ?? null}
                mindate={
                  calculatedStats?.minInDate
                    ? calculatedStats?.minInDate.split("T")[0]
                    : ""
                }
                maxdate={
                  calculatedStats?.maxInDate
                    ? calculatedStats?.maxInDate.split("T")[0]
                    : ""
                }
                color={{ bg: "#06B6F6", main: "#008ABD" }}
              />

              <StatCard
                title="Outgoing Average"
                avg={calculatedStats?.averageOutCount ?? null}
                min={calculatedStats?.minOutCount ?? null}
                max={calculatedStats?.maxOutCount ?? null}
                mindate={
                  calculatedStats?.minOutDate
                    ? calculatedStats.minOutDate.split("T")[0]
                    : ""
                }
                maxdate={
                  calculatedStats?.maxOutDate
                    ? calculatedStats.maxOutDate.split("T")[0]
                    : ""
                }
                color={{ bg: "#FFF235", main: "#8C8300" }}
              />
            </div>
          </div>
        </Box>

        <Box className="widget-main-footer">
          <Box className="widget-main-footer-value">
            <Typography>Total No. of Vehicles : </Typography>
            <span> {formatNumber(totalInVehicle ?? 0)}</span>
          </Box>
          {!openZoomDialog ? (
            <Box
              className="widget-main-footer-zoom-i"
              onMouseEnter={() => setIsDraggable?.(true)}
              onMouseLeave={() => setIsDraggable?.(false)}
            >
              <img
                src={
                  theme === "light"
                    ? "/images/dashboard/drag.svg"
                    : "/images/dark-theme/dashboard/drag.svg"
                }
                alt="vehicle"
                width={35}
                height={35}
              />
            </Box>
          ) : null}
          {!openZoomDialog ? (
            <Box
              className="widget-main-footer-zoom-i"
              onClick={onZoomClick}
              id="zoomwidgetBtnAverageVehicleCounting"
            >
              <img
                src={
                  theme === "light"
                    ? "/images/dashboard/ZoomWidget.svg"
                    : "/images/dark-theme/dashboard/ZoomWidget.svg"
                }
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

export { AverageVehicleCount1_1 };
