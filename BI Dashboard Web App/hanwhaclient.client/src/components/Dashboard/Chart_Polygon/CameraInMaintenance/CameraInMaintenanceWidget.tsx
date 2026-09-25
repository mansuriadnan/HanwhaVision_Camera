import React, { useEffect, useRef, useState } from "react";
import {
  CommonWidgetProps,
  ICameraInMaintenanceData,
  IWidgetPayload,
  LayoutItem,
} from "../../../../interfaces/IChart";
import { CommonDialog, LocalLoader } from "../../../index";
import { fetchCameraInMaintenanceDataService } from "../../../../services/dashboardService";
import { Box } from "@mui/material";
import { convertToUTC } from "../../../../utils/convertToUTC";
import { useSignalRContext } from "../../../../context/SignalRContext";
import { CameraInMaintenance1_1 } from "./CameraInMaintenance1_1";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";
import apiUrls from "../../../../constants/apiUrls";

const CameraInMaintenanceWidget: React.FC<CommonWidgetProps> = ({
  item,
  floor,
  zones,
  selectedStartDate,
  selectedEndDate,
  setExportHandler,
  setIsDraggable,
  onLoadComplete,
}) => {
  const { width, height, size, expanded, displayName, chartName } =
    item as LayoutItem;
  const customizedWidth =
    size === "3x1" ? width / 3 : size === "2x1" ? width / 2 : width;
  const [openZoomDialog, setOpenZoomDialog] = useState(false);
  const [cameraMainData, setCameraMainData] = useState<
    ICameraInMaintenanceData | undefined
  >(undefined);

  const [loadingCount, setLoadingCount] = useState(0);
  const selectedIntervalNameRef = useRef<string>("");
  const [animate, setAnimate] = useState(false);
  const { IsToday, IsDisplayLoader } = useSignalRContext();

  useEffect(() => {
    if (animate) {
      const timeout = setTimeout(() => setAnimate(false), 600);
      return () => clearTimeout(timeout);
    }
  }, [animate]);

  useEffect(() => {
    fetchCameraInMaintenanceData();
  }, [floor, zones, selectedStartDate, selectedEndDate]);

  useExportHandler({
    apiEndpoint: `${apiUrls.CameraInMaintenance}/csv`,
    startDate: convertDateToISOLikeString(selectedStartDate as Date),
    endDate: convertDateToISOLikeString(selectedEndDate as Date),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  const fetchCameraInMaintenanceData = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const data = {
        floorIds: floor,
        zoneIds: zones,
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const response: any = await fetchCameraInMaintenanceDataService(
        data as IWidgetPayload
      );

      if (response?.data.cameraInMaintenanceCount > 0) {
        setCameraMainData(response?.data as ICameraInMaintenanceData);
      } else {
        setCameraMainData(undefined);
      }
    } catch (error) {
      console.error("Error fetching Pedestrian Analysis data:", error);
      throw error;
    } finally {
      setLoadingCount((c) => c - 1);
    }
  };

  const handleZoomClick = () => {
    setOpenZoomDialog(true);
  };

  const handleCloseZoom = () => {
    setOpenZoomDialog(false);
  };

  const renderLayout = () => {
    return (
      <Box
        sx={{ height: height, display: "flex" }}
        className={animate ? "animate-widget" : ""}
      >
        <CameraInMaintenance1_1
          customizedWidth={customizedWidth}
          customizedHeight={height}
          cameraMainData={cameraMainData}
          displayName={displayName}
          onZoomClick={handleZoomClick}
          openZoomDialog={openZoomDialog}
          floor={floor}
          zones={zones}
          setIsDraggable={setIsDraggable}
          animate={animate}
          setAnimate={setAnimate}
        ></CameraInMaintenance1_1>
        {/* 
        {size === "2x1" &&
          (expanded === "Option3" ? null : expanded === "Option2" ? (
            <PedestrianDetection2_1_Option2
              customizedWidth={customizedWidth}
              customizedHeight={height}
              pDData={pDData}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
            />
          ) : (
            <PedestrianDetection2_1_Option1
              customizedWidth={customizedWidth}
              customizedHeight={height}
              pDData={pDData}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
            />
          ))} */}
        <Box className="widget-label-bottom">
          <span>{chartName}</span>
          <img
            src="/images/question_circle_icon_widget_bottom_title.svg"
            alt="info Icon"
            className="widget-label-icon-bottom"
          />
        </Box>
      </Box>
    );
  };

  return (
    <>
      {loadingCount > 0 && !IsDisplayLoader ? (
        <LocalLoader width={width} height={height} size={50} color="warning" />
      ) : (
        <>
          {renderLayout()}
          <CommonDialog
            open={openZoomDialog}
            title={"Expanded View"}
            onCancel={handleCloseZoom}
            maxWidth={"lg"}
            customClass={"widget_popup cmn-pop-design-parent"}
            content={renderLayout()}
            isWidget={true}
          />
        </>
      )}
    </>
  );
};

export { CameraInMaintenanceWidget };
