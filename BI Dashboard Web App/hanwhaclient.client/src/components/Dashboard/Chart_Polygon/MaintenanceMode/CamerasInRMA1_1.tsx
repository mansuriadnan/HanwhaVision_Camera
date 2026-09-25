import React, { useEffect, useState } from "react";
import { Box, Typography } from "@mui/material";
import { IMaintenanceModeProps } from "../../../../interfaces/IChart";
import { convertImageToBase64 } from "../../../../utils/convertImageToBase64";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";

const CamerasInRMA1_1: React.FC<IMaintenanceModeProps> = ({
  customizedWidth,
  displayName,
  onZoomClick,
  openZoomDialog,
  setIsDraggable,
  mMData
}) => {
  const [bgBase64, setBgBase64] = useState<string | null>(null);
  const { theme } = useThemeContext();  
  const [finalCount, setFinalCount] = useState<number>(0);

  useEffect(() => {
    convertImageToBase64(
      "/images/dashboard/Pedestrian_Detection_background.svg",
      (base64: string | null) => {
        if (base64) {
          setBgBase64(base64);
        } else {
          console.error("Failed to convert image to Base64");
        }
      }
    );
    setFinalCount(mMData?.length ?? 0)
  }, [mMData, customizedWidth]);


  const backgroundStylPre = {
    backgroundImage: ` url('/images/back-pre.png')`,
    backgroundSize: "contain",
    backgroundRepeat: "no-repeat",
    backgroundPosition: "center bottom",
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
            <div className="image-wrapper"><img src="/images/cameras_in_rma.gif" /></div>
            <div className="camera-in-rma">
              
              <Typography variant="h5">Maintenance/RMA</Typography>
              <Typography variant="h5">{formatNumber(finalCount)}</Typography>
            </div>
          </div>
        </Box>

        <Box className="widget-main-footer">
         <Box className="widget-main-footer-value">
            <Typography>Total No. of Cameras in RMA : </Typography>
            <span> {formatNumber(finalCount)}
            </span>
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
            <Box className="widget-main-footer-zoom-i" onClick={onZoomClick} id="zoomwidgetBtnPedestrianDetection">
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

export { CamerasInRMA1_1 };
