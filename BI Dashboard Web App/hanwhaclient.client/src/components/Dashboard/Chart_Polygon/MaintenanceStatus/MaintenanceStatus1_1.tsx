import React from "react";
import { MaintenanceStatusProps } from "../../../../interfaces/IChart";
import { Box, Typography } from "@mui/material";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";

const MaintenanceStatus1_1: React.FC<MaintenanceStatusProps> = ({
  customizedWidth,
  displayName,
  onZoomClick,
  openZoomDialog,
  setIsDraggable,
  statusData,
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
        <Box className="widget-main-body">
          <Box className="widget-data-wrapper">
         
            <Box className="maintenance-images">
              
              <Box
                component="img"
                src="/images/dashboard/maintenanceStatus_camera.svg"
                alt="maintenanceStatus camera"
              />

            </Box>

            {/* Purple container */}
            <Box
             className="maintenance-status-boxes"
            >
              {/* Row 1 */}
              <Box className="maintenance-top-boxes-wrapper">
                <Box
                   className="maintenance--top=boxes-repeat"
                >
                  <Typography variant="h4">
                    {formatNumber(statusData?.dueCount ?? 0)}
                  </Typography>
                  <Typography sx={{ fontSize: 14, fontWeight: 500 }}>
                    Due
                  </Typography>
                </Box>

                <Box
                   className="maintenance-boxes-repeat"
                >
                  <Typography
                   variant="h4"
                  >
                    {formatNumber(statusData?.aboutToDueCount ?? 0)}
                  </Typography>
                  <Typography sx={{ fontSize: 14, fontWeight: 500 }}>
                    About to be Due
                  </Typography>
                </Box>
              </Box>

              {/* Row 2 */}
              

              <Box className="maintenance-bottom-boxes-wrapper">
                <Box
                  className="maintenance--bottom-boxes-repeat"
                >
                  <Typography
                    variant="h4"
                  >
                    {formatNumber(statusData?.reworkCount ?? 0)}
                  </Typography>
                  <Typography sx={{ fontSize: 14, fontWeight: 500 }}>
                    Rework
                  </Typography>
                </Box>
                <Box
                   className="maintenance--bottom-boxes-repeat"
                >
                  <Typography
                     variant="h4"
                  >
                    {formatNumber(statusData?.inProgressCount ?? 0)}
                  </Typography>
                  <Typography sx={{ fontSize: 14, fontWeight: 500 }}>
                    In Progress
                  </Typography>
                </Box> 
                <Box
                   className="maintenance--bottom-boxes-repeat"
                >
                  <Typography
                   variant="h4"
                  >
                    {formatNumber(statusData?.completedCount ?? 0)}
                  </Typography>
                  <Typography sx={{ fontSize: 14, fontWeight: 500 }}>
                    Completed
                  </Typography>
                </Box>
              </Box>
            </Box>
          
          </Box>
        </Box>




        
       

        <Box className="widget-main-footer">
          <Box className="widget-main-footer-value">
            <Typography>Total No. of Cameras in Maintenance : </Typography>
            <span>{formatNumber(statusData?.totalCount ?? 0)}</span>
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
              id="zoomwidgetBtnGender"
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

export { MaintenanceStatus1_1 };
