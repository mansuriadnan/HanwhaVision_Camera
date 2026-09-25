import { Box, Typography } from "@mui/material";
import { ParkingProps } from "../../../../interfaces/IChart";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";
import LinearProgress from "@mui/material/LinearProgress";

const Parking1_1: React.FC<ParkingProps> = ({
  parkingZoneWiseData,
  customizedWidth,
  displayName,
  onZoomClick,
  openZoomDialog,
  setIsDraggable,
}) => {
  const { theme } = useThemeContext();

  return (
    <Box sx={{ width: customizedWidth }}>
      <Box className="widget-main-wrapper">
        <Box className="widget-main-header">
          <Typography variant="h6" component="h2">
            {displayName}
          </Typography>
        </Box>

        <Box className="widget-main-body parking-widget">
          <div className="widget-data-wrapper">
            <Box className="parking-one">
              <Box className="parking-occupied">
                <Typography variant="h4">
                  {formatNumber(parkingZoneWiseData?.occupied ?? 0)}
                </Typography>
                <Typography>Occupied</Typography>
              </Box>
              <Box className="parking-icon">
                <img src="/images/dashboard/Parking_Gate.webp" alt="gate" />
              </Box>
              <Box className="parking-available">
                <Typography variant="h4">
                  {/* {formatNumber(parkingZoneWiseData?.available ?? 0)} */}
                   {formatNumber(Math.max(0, parkingZoneWiseData?.available ?? 0))}
                </Typography>
                <Typography>Available</Typography>
              </Box>
            </Box>
            <Box>
              <Box className="parking-widget-score">
                <Typography
                  style={{ color: theme === "light" ? "#212121" : "#FFFFFF" }}
                >
                  Occupancy
                </Typography>
                <Typography
                  variant="h5"
                  style={{ color: theme === "light" ? "#212121" : "#FFFFFF" }}
                >
                  {parkingZoneWiseData?.occupancy !== undefined
                    ? Number(parkingZoneWiseData.occupancy).toFixed(2)
                    : "0.00"}
                  %
                </Typography>
              </Box>
              <LinearProgress
                variant="determinate"
                value={
                  (parkingZoneWiseData?.occupancy ?? 0) >= 100
                    ? 100
                    : (parkingZoneWiseData?.occupancy ?? 0)
                }
              />
            </Box>
          </div>
        </Box>

        <Box className="widget-main-footer">
          <Box className="widget-main-footer-value">
            <Typography>Total No. of Slots : </Typography>
            <span>{formatNumber(parkingZoneWiseData?.totalSlot ?? 0)}</span>
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
              id="zoomwidgetBtnNewVsTotalVisitors"
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
export { Parking1_1 };
