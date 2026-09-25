import React, { useState } from "react";
import { Box, Typography } from "@mui/material";
import { ICameraInMaintenanceProps } from "../../../../interfaces/IChart";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";

const CameraInMaintenance1_1: React.FC<ICameraInMaintenanceProps> = ({
  floor,
  zones,
  customizedWidth,
  displayName,
  onZoomClick,
  openZoomDialog,
  setIsDraggable,
  animate,
  setAnimate,
  cameraMainData,
}) => {
  const { theme } = useThemeContext();

  const [finalCount, setFinalCount] = useState<number>(
    cameraMainData?.cameraInMaintenanceCount ?? 0
  );

  const width = 320;

  const cardHeight = Math.round(width * 0.62); // card aspect ratio
  const cameraSize = Math.round(width * 0.22); // camera icon size
  const rectWidth = Math.round(width * 0.7);
  const rectHeight = Math.round(rectWidth * 0.36);

  return (
    <Box sx={{ width: customizedWidth }}>
      <Box className="widget-main-wrapper">
        <Box className="widget-main-header">
          <Typography variant="h6" component="h2">
            {displayName}
          </Typography>
        </Box>
        <Box className="widget-main-body">
          {/* Background card */}
          <Box className="cameras-in">

          {/* Camera icon */}
          <img
            src="/images/CameraInMaintenance_Icon.png"
            alt="Camera icon"
            loading="lazy"
            draggable={false}
           
          />

          {/* Rectangle with count */}
          <div
           className="cameras-in-image"
          >
            
            {/* Centered count over the rectangle */}
            <h5
              aria-live="polite"
            >
              {formatNumber(finalCount)}
            </h5>
          </div>
          </Box>
        </Box>
        <Box className="widget-main-footer">
          <Box className="widget-main-footer-value">
            <Typography>Total no of Cameras in maintenance: </Typography>
            <span>{formatNumber(finalCount)}</span>
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
              id="zoomwidgetBtnPedestrianDetection"
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

  // return (
  //   <Box sx={{ width: customizedWidth }}>
  //     <Box className="widget-main-wrapper">
  //       <Box className="widget-main-header">
  //         <Typography variant="h6" component="h2">
  //           {displayName}
  //         </Typography>
  //       </Box>

  //       <Box className="widget-main-body">
  //         <div className="widget-data-wrapper">
  //           <div className="pedestrain-detection" style={backgroundStylPre}>
  //             <img src="/images/CameraInMaintenance.png" />
  //             {/* Went for maintenance/RMA {formatNumber(finalCount)}{" "} */}
  //             {/* {Number(formatNumber(finalCount)) > 1 ? "times" : "time"} */}
  //             <img src="/images/CameraIn_Rect.png" />
  //             {formatNumber(finalCount)}
  //           </div>
  //         </div>
  //       </Box>

  //       <Box className="widget-main-footer">
  //         {!openZoomDialog ? (
  //           <Box
  //             className="widget-main-footer-zoom-i"
  //             onMouseEnter={() => setIsDraggable?.(true)}
  //             onMouseLeave={() => setIsDraggable?.(false)}
  //           >
  //             <img
  //               src={
  //                 theme === "light"
  //                   ? "/images/dashboard/drag.svg"
  //                   : "/images/dark-theme/dashboard/drag.svg"
  //               }
  //               alt="vehicle"
  //               width={35}
  //               height={35}
  //             />
  //           </Box>
  //         ) : null}
  //         {!openZoomDialog ? (
  //           <Box
  //             className="widget-main-footer-zoom-i"
  //             onClick={onZoomClick}
  //             id="zoomwidgetBtnPedestrianDetection"
  //           >
  //             <img
  //               src={
  //                 theme === "light"
  //                   ? "/images/dashboard/ZoomWidget.svg"
  //                   : "/images/dark-theme/dashboard/ZoomWidget.svg"
  //               }
  //               alt="vehicle"
  //               width={35}
  //               height={35}
  //             />
  //           </Box>
  //         ) : null}
  //       </Box>
  //     </Box>
  //   </Box>
  // );
};

export { CameraInMaintenance1_1 };
