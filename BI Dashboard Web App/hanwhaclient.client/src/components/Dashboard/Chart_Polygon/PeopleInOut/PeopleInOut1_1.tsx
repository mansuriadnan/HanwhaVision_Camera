import React from "react";
import { ExtendedPeopleVehicleProps } from "../../../../interfaces/IChart";
import { Box, Typography, Button } from "@mui/material";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";

const PeopleInOut1_1 = ({
  inOutValue,
  customizedWidth,
  displayName,
  onZoomClick,
  openZoomDialog,
  setIsDraggable,
  finalRulesValue
}: ExtendedPeopleVehicleProps) => {
  const [status, setStatus] = React.useState<"In" | "Out">("In");

  const handleStatusChange = (newStatus: "In" | "Out") => {
    if (newStatus) {
      setStatus(newStatus);
    }
  };

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
        <Box className="in-out-header-buttons">
          <Button
            className={status === "In" ? "active" : ""}
            fullWidth={true}
            onClick={() => {
              handleStatusChange("In");
            }}
          >
            In
          </Button>
          <Button
            className={status === "Out" ? "active" : ""}
            fullWidth={true}
            onClick={() => {
              handleStatusChange("Out");
            }}
          >
            Out
          </Button>
        </Box>
        <Box className="widget-main-body people-in-out-main">
          <div className="widget-data-wrapper">
            <div className="people-in-out">
              {status === "In" ? (
                <img
                  src="/images/dashboard/PeopleIn.gif"
                  alt="Walking person"
                  style={{ width: "100px", height: "100px" }}
                />
              ) : (
                <img
                  src="/images/dashboard/People_Out.gif"
                  alt="Walking person"
                  style={{ width: "100px", height: "100px" }}
                />
              )}
              <Typography variant="h6">
                {status == "In"
                  ? formatNumber(adjustedIn ?? 0)
                  : formatNumber(adjustedOut ?? 0)}
              </Typography>
            </div>
          </div>
        </Box>

        <Box className="widget-main-footer">
          <Box className="widget-main-footer-value">
            <Typography>Total No. of People : </Typography>
            <span>
              {" "}
              {status == "In"
                ? formatNumber(adjustedIn ?? 0)
                : formatNumber(adjustedOut ?? 0)}
            </span>
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

export { PeopleInOut1_1 };
