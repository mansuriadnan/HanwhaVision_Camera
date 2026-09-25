import { ExtendedPeopleVehicleProps } from "../../../../interfaces/IChart";
import { Box, Typography} from "@mui/material";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";

const Vehicle1_1 = ({
  inOutValue,
  customizedWidth,
  displayName,
  onZoomClick,
  openZoomDialog,
  setIsDraggable,
  finalRulesValue
}: ExtendedPeopleVehicleProps) => {
  const { theme } = useThemeContext();

  const totalIn = inOutValue?.totalInCount ?? 0;
  const totalOut = inOutValue?.totalOutCount ?? 0;

  const adjustedIn =
    totalIn + (totalIn * (finalRulesValue || 0)) / 100;

  const adjustedOut =
    totalOut + (totalOut * (finalRulesValue || 0)) / 100;

  return (
    <Box sx={{ width: customizedWidth }}>
      <Box className="widget-main-wrapper">
        <Box className="widget-main-header">
          <Typography variant="h6" component="h2">
            {displayName}
          </Typography>
        </Box>
        <Box className="widget-main-body vehicle-in-out-new">
          <div className="widget-data-wrapper">
            <div className="vehicle-in-out">
              <img
                src={
                  theme === "light"
                    ? "/images/dashboard/vehiclein.gif"
                    : "/images/dark-theme/dashboard/vehiclein.gif"
                }
                alt="Vehicle In"
                style={{ width: "100px", height: "100px" }}
              />

              <img
                src={
                  theme === "light"
                    ? "/images/dashboard/vehiclein.gif"
                    : "/images/dark-theme/dashboard/vehiclein.gif"
                }
                alt="Vehicle Out"
                style={{
                  transform: "scaleX(-1)",
                  width: "100px",
                  height: "100px",
                }}
              />
            </div>

            <Box className="in-out-header-buttons">
              <Box className="in-out-header-buttons-wrapper">
                <Typography variant="h6"> Vehicle In</Typography>
                <Typography variant="h5">
                  {formatNumber(adjustedIn ?? 0)}
                </Typography>
              </Box>
              <Box className="in-out-header-buttons-wrapper">
                <Typography variant="h6"> Vehicle Out </Typography>
                <Typography variant="h5">
                  {formatNumber(adjustedOut ?? 0)}
                </Typography>
              </Box>
            </Box>
          </div>
        </Box>

        <Box className="widget-main-footer">
          <Box className="widget-main-footer-value">
            <Typography>Total No. of Vehicles In : </Typography>
            <span>{formatNumber(adjustedIn ?? 0)}</span>
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
              id="zoomwidgetBtnVehicle"
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

export { Vehicle1_1 };
