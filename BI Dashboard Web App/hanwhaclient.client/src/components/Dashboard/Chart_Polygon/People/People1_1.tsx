import { ExtendedPeopleVehicleProps } from "../../../../interfaces/IChart";
import { Box, Typography } from "@mui/material";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";

const People1_1 = ({
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

        <Box className="widget-main-body people-in-out-new">
          <div className="widget-data-wrapper">
            <div className="people-in-out">
              <img
                src="/images/dashboard/PeopleIn.gif"
                alt="Walking person"
                style={{ width: "100px", height: "100px" }}
              />

              <img
                src="/images/dashboard/People_Out.gif"
                alt="Walking person"
                style={{ width: "100px", height: "100px" }}
              />
            </div>
            
            <Box className="in-out-header-buttons">
              <Box className="in-out-header-buttons-wrapper">
                <Typography variant="h6"> People In</Typography>
                <Typography variant="h5">
                  {formatNumber(adjustedIn ?? 0)}
                </Typography>
              </Box>
              <Box className="in-out-header-buttons-wrapper">
                <Typography variant="h6"> People Out </Typography>
                <Typography variant="h5">
                  {formatNumber(adjustedOut ?? 0)}
                </Typography>
              </Box>
            </Box>

          </div>
        </Box>

        <Box className="widget-main-footer">
          <Box className="widget-main-footer-value">
            <Typography>Total No. of People In : </Typography>
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
              id="zoomwidgetBtnPeople"
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

export { People1_1 };
