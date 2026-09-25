import React, { useEffect, useState } from "react";
import {
  cameraDisconnectionTrackerProps,
} from "../../../../interfaces/IChart";
import { Box, Tooltip, Typography } from "@mui/material";
import { formatNumber } from "../../../../utils/formatNumber";
import { formatDateToConfiguredTimezone } from "../../../../utils/formatDateToConfiguredTimezone";
import { useThemeContext } from "../../../../context/ThemeContext";
import { formatDate } from "../../../../utils/dateUtils";

const CameraDisconnectionTracker1_1: React.FC<cameraDisconnectionTrackerProps> = ({
  customizedWidth,
  displayName,
  onZoomClick,
  openZoomDialog,
  cameraDisconnectionData,
  startDate,
  endDate,
  setIsDraggable,
}) => {
  const { theme } = useThemeContext();

type DisconnectionEvent = {
  deviceId: string;
  offlineTime: string;
  onlineTime?: string;
};

const calculateDisconnectionCountsFromOffline = (
  data: DisconnectionEvent[]
) => {
  const now = new Date();

  const oneHourAgo = new Date(now.getTime() - 1 * 60 * 60 * 1000);
  const oneDayAgo = new Date(now.getTime() - 24 * 60 * 60 * 1000);
  const oneWeekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
  const oneMonthAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);


  let last1Hour = 0;
  let previousDay = 0;
  let previousWeek = 0;
  let previousMonth = 0;

  data.forEach(item => {
    const offlineDate = new Date(item.offlineTime);

    if (offlineDate >= oneHourAgo && offlineDate <= now) {
      last1Hour++;
    }

    if (offlineDate >= oneDayAgo && offlineDate <= now) {
      previousDay++;
    }

    if (offlineDate >= oneWeekAgo && offlineDate <= now) {
      previousWeek++;
    }

    if (offlineDate >= oneMonthAgo && offlineDate <= now) {
      previousMonth++;
    }
  });

  return {
    last1Hour,
    previousDay,
    previousWeek,
    previousMonth,
    total: data.length,
  };
};

const disconnectionCounts = React.useMemo(() => {
  if (!cameraDisconnectionData?.length) {
    return {
      last1Hour: 0,
      previousDay: 0,
      previousWeek: 0,
      previousMonth: 0,
      total: 0,
    };
  }

  return calculateDisconnectionCountsFromOffline(
    cameraDisconnectionData
  );
}, [cameraDisconnectionData]);

const getDateRangeForPeriod = (period: "hour" | "day" | "week" | "month") => {
  const now = new Date();
  let start: Date;

  switch (period) {
    case "hour":
      start = new Date(now.getTime() - 1 * 60 * 60 * 1000);
      break;
    case "day":
      start = new Date(now.getTime() - 24 * 60 * 60 * 1000);
      break;
    case "week":
      start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
      break;
    case "month":
      start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
      break;
  }

  return `${formatDate(formatDateToConfiguredTimezone(start.toISOString()))} To ${formatDate(formatDateToConfiguredTimezone(
    now.toISOString()
  ))}`;
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
            <div className="camera-disconnection-tracker">
              {/* TOP SECTION */}
              <Box
               className="camera-disconnection-tracker-top"
              >
                {/* Clock Icon */}
                <Box
                 className="camera-disconnection-clock"
                >
                  <img src="/images/dashboard/clock.svg" width={26} />
                </Box>

                {/* Hours */}
                <Box
                  className="camera-disconnection-clock-details"
                >
                  <Typography variant="subtitle2" color="text.secondary">
                    Last 1 Hour
                  </Typography>
                  <Tooltip
                    title={getDateRangeForPeriod("hour")}
                    arrow
                    placement="top"
                  >
                    <span style={{ color: '#2081FF', fontWeight: 600, fontSize: '30px' }} >
                      {disconnectionCounts.last1Hour}
                    </span>
                  </Tooltip>
                </Box>
              </Box>

              {/* DAY / WEEK / MONTH */}
              <Box className="camera-disconnection-tracker-bottom">
                {[
                  { label: "Previous Day", value: disconnectionCounts.previousDay, imageName: 'Day',  period: "day" },
                  { label: "Previous Week", value: disconnectionCounts.previousWeek, imageName: 'Week', period: "week" },
                  { label: "Previous Month", value:  disconnectionCounts.previousMonth, imageName: 'Month', period: "month" },
                ].map((item) => (
                  <Box
                    key={item.label}
                    className="camera-disconnection-tracker-bottom-repeat"
                  >
                    <Box
                      className="camera-disconnection-tracker-bottom-icon"
                    >
                      <img src={`/images/dashboard/${item.imageName}.svg`} width={18} />
                    </Box>

                    <Typography variant="body2">{item.label}</Typography>
                    <Tooltip
                      title={getDateRangeForPeriod(item.period as any)}
                      arrow
                      placement="top"
                    >
                      <span style={{ color: '#2081FF' }}>
                        {item.value}
                      </span>
                    </Tooltip>
                  </Box>
                ))}
              </Box>
            </div>
          </div>
        </Box>

        <Box className="widget-main-footer">
          <Box className="widget-main-footer-value">
            <Typography>Total No. of Disconnected Cameras : </Typography>
            <span style={{ fontWeight: 600 }}> {formatNumber(disconnectionCounts.total)}</span>
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
            <Box className="widget-main-footer-zoom-i" onClick={onZoomClick} id="zoomwidgetBtnCumulativePeopleCount">
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

export { CameraDisconnectionTracker1_1 };
