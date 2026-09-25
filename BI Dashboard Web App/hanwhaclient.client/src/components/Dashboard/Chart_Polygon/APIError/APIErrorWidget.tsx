import React, { useEffect, useRef, useState } from "react";
import {
  IWidgetPayload,
  LayoutItem,
  CommonWidgetProps,
  APIErrorRes,
} from "../../../../interfaces/IChart";
import { Box } from "@mui/material";
import { fetchDeviceExceptionDataService } from "../../../../services/dashboardService";
import {
  APIError1_1,
  CommonDialog,
  LocalLoader,
  APIError2_1_Option1,
  APIError2_1_Option2,
} from "../../../index";
import { convertToUTC } from "../../../../utils/convertToUTC";
import { useSignalRContext } from "../../../../context/SignalRContext";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";
import apiUrls from "../../../../constants/apiUrls";

const APIErrorWidget: React.FC<CommonWidgetProps> = ({
  item,
  floor,
  zones,
  selectedStartDate,
  selectedEndDate,
  setExportHandler,
  setIsDraggable,
}) => {
  const { width, height, size, expanded, displayName, chartName } =
    item as LayoutItem;
  const customizedWidth =
    size === "3x1" ? width / 3 : size === "2x1" ? width / 2 : width;
  const [openZoomDialog, setOpenZoomDialog] = useState(false);
  const [loadingCount, setLoadingCount] = useState(0);
  const { IsDisplayLoader } = useSignalRContext();
  const [apiErrordata, setApiErrordata] = useState<APIErrorRes>();
  const [finalapiErrordata, setFinalapiErrordata] = useState([]);
  const selectedIntervalNameRef = useRef<string>("");

  useEffect(() => {
    fetchDeviceExceptionCountData();
  }, [floor, zones, selectedStartDate, selectedEndDate]);

  useExportHandler({
    apiEndpoint: `${apiUrls.DeviceException}/csv`,
    startDate: convertDateToISOLikeString(selectedStartDate),
    endDate: convertDateToISOLikeString(selectedEndDate),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  const fetchDeviceExceptionCountData = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const data = {
        floorIds: floor || [],
        zoneIds: zones || [],
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const responseChart: any = await fetchDeviceExceptionDataService(
        data as unknown as IWidgetPayload
      );

      setApiErrordata(responseChart?.data as APIErrorRes);

      const tempapiErrordata = Object.entries(responseChart?.data).map(
        ([key, devices]) => ({
          featuresName: key,
          totalCount: Array.isArray(devices) ? devices.length : 0,
        })
      );
      setFinalapiErrordata(tempapiErrordata as any);
    } catch (error) {
      console.error("Error fetching API Error data:", error);
      throw error;
    } finally {
      setLoadingCount((c) => c - 1); // Stop local loader
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
        <APIError1_1
          apiErrordata={apiErrordata}
          customizedWidth={customizedWidth}
          displayName={displayName}
          onZoomClick={handleZoomClick}
          openZoomDialog={openZoomDialog}
          setIsDraggable={setIsDraggable}
        ></APIError1_1>

        {size === "2x1" &&
          (expanded === "Option3" ? null : expanded === "Option2" ? (
            <APIError2_1_Option2
              finalapiErrordata={finalapiErrordata}
              customizedWidth={customizedWidth}
              customizedHeight={height}
            />
          ) : (
            <APIError2_1_Option1
              customizedWidth={customizedWidth}
              customizedHeight={height}
              finalapiErrordata={finalapiErrordata}
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

export { APIErrorWidget };
