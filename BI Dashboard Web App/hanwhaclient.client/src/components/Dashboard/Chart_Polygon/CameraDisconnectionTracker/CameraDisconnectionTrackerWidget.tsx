import React, { useEffect, useMemo, useRef, useState } from "react";
import {
  IWidgetPayload,
  CommonWidgetProps,
  LayoutItem,
  ICDTData,
  IDisconnectChartPoint,
  refCDTData,
} from "../../../../interfaces/IChart";
import { fetchCameraDisconnectionAnalysisService } from "../../../../services/dashboardService";
import { Box } from "@mui/material";
import {
  CommonDialog,
  LocalLoader,
  CameraDisconnectionTracker1_1,
  CameraDisconnectionTracker2_1_Option2,
  CameraDisconnectionTracker2_1_Option1,
} from "../../../index";
import { convertToUTC } from "../../../../utils/convertToUTC";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import apiUrls from "../../../../constants/apiUrls";
import { useSignalRContext } from "../../../../context/SignalRContext";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";
import { formatDateToConfiguredTimezone } from "../../../../utils/formatDateToConfiguredTimezone";

const CameraDisconnectionTrackerWidget: React.FC<CommonWidgetProps> = ({
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
  const [cameraDisconnectionData, setCameraDisconnectionData] = useState<
    ICDTData[]
  >([]);
  const [referenceData, setReferenceData] = useState<refCDTData>();
  const [loadingCount, setLoadingCount] = useState(0);
  const selectedIntervalNameRef = useRef<string>("");
  const { IsDisplayLoader } = useSignalRContext();

  useExportHandler({
    apiEndpoint: `${apiUrls.CameraDisconnectedTrackerAnalysis}/csv`,
    startDate: convertDateToISOLikeString(selectedStartDate),
    endDate: convertDateToISOLikeString(selectedEndDate),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  useEffect(() => {
    fetchCameraDisconnectionAnalysisData();
  }, [floor, zones, selectedStartDate, selectedEndDate]);

  const deviceIdToIpMap = useMemo(() => {
  const map: Record<string, string> = {};
  referenceData?.IpAddress?.forEach((item) => {
    map[item.value] = item.label;
  });
  return map;
}, [referenceData]);

const cameraDisconnectionWithIp = useMemo(() => {
  return cameraDisconnectionData.map((item) => ({
    ...item,
    ipAddress: deviceIdToIpMap[item.deviceId] || "Unknown IP",
  }));
}, [cameraDisconnectionData, deviceIdToIpMap]);

  const fetchCameraDisconnectionAnalysisData = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const data = {
        floorIds: floor,
        zoneIds: zones,
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const response: any = await fetchCameraDisconnectionAnalysisService(
        data as IWidgetPayload
      );
      if (response?.data != null) {
        setCameraDisconnectionData(response.data);
        setReferenceData(response.referenceData)
      }
    } catch (error) {
      console.error("Error fetching Cumulative People data:", error);
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
      <Box sx={{ height: height, display: "flex" }}>
        <CameraDisconnectionTracker1_1
          customizedWidth={customizedWidth}
          customizedHeight={height}
          displayName={displayName}
          onZoomClick={handleZoomClick}
          openZoomDialog={openZoomDialog}
          cameraDisconnectionData={cameraDisconnectionWithIp}
          startDate={new Date(selectedStartDate)}
          endDate={new Date(selectedEndDate)}
          setIsDraggable={setIsDraggable}
        />

        {size === "2x1" &&
          (expanded === "Option3" ? null : expanded === "Option2" ? (
            <CameraDisconnectionTracker2_1_Option2
              customizedWidth={customizedWidth}
              customizedHeight={height}
              cameraDisconnectionData={cameraDisconnectionWithIp}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
            />
          ) : (
            <CameraDisconnectionTracker2_1_Option1
              customizedWidth={customizedWidth}
              customizedHeight={height}
              cameraDisconnectionData={cameraDisconnectionWithIp}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
            />
          ))}
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

export { CameraDisconnectionTrackerWidget };
