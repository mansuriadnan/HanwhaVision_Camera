import { useEffect, useRef, useState } from "react";
import { Box } from "@mui/material";
import {
  ANPRParking1_1,
  ANPRParking2_1_Option1,
  ANPRParking2_1_Option2,
  ANPRParking2_1_Option3,
  CommonDialog,
  LocalLoader,
} from "../../../index";
import {
  IWidgetPayload,
  LayoutItem,
  CommonWidgetProps,
  IANPRParkingChartData,
  IParkingZoneWiseData,
  IParkingTreeChartData,
} from "../../../../interfaces/IChart";
import {
  fetchANPRVehicleParkingService,
  fetchANPRVehicleParkingByZonesService,
} from "../../../../services/dashboardService";
import { convertToUTC } from "../../../../utils/convertToUTC";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import apiUrls from "../../../../constants/apiUrls";
import { useSignalRContext } from "../../../../context/SignalRContext";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";

const ANPRParkingWidget: React.FC<CommonWidgetProps> = ({
  item,
  floor,
  zones,
  selectedStartDate,
  selectedEndDate,
  setExportHandler,
  setIsDraggable,
}) => {
  const [anprparkingChartData, setANPRParkingChartData] =
    useState<IANPRParkingChartData[]>();
  const [anprParkingZoneWiseData, setANPRParkingZoneWiseData] =
    useState<IParkingZoneWiseData>();
  const [anprParkingTreeChartData, setANPRParkingTreeChartData] =
    useState<IParkingTreeChartData[]>();
  const [openZoomDialog, setOpenZoomDialog] = useState(false);
  const { width, height, size, expanded, displayName, chartName } =
    item as LayoutItem;
  const customizedWidth =
    size === "3x1" ? width / 3 : size === "2x1" ? width / 2 : width;
  const pHeight = height;
  const [loadingCount, setLoadingCount] = useState(0);
  const selectedIntervalNameRef = useRef<string>("");
  const { IsDisplayLoader } = useSignalRContext();

  useEffect(() => {
    fetchANPRParkingDataByZone();
    fetchANPRParkingchartData();
  }, [floor, zones, selectedStartDate, selectedEndDate, size]);

  useExportHandler({
    apiEndpoint: `${apiUrls.ANPRVehicleParking}/csv`,
    startDate: convertDateToISOLikeString(selectedStartDate),
    endDate: convertDateToISOLikeString(selectedEndDate),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  const fetchANPRParkingDataByZone = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const requestData = {
        floorIds: floor,
        zoneIds: zones,
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const responseChart: any = await fetchANPRVehicleParkingByZonesService(
        requestData as unknown as IWidgetPayload,
      );

      setANPRParkingTreeChartData(responseChart?.data);

      const zoneData = responseChart?.data || [];
      const totalOccupied = zoneData.reduce(
        (sum: number, item: any) => sum + item.parkingCount,
        0,
      );

      const totalSlots = zoneData.reduce(
        (sum: number, item: any) => sum + item.totalParkingOccupancy,
        0,
      );

      const available = totalSlots - totalOccupied;

      const occupancy = totalSlots > 0 ? (totalOccupied / totalSlots) * 100 : 0;

      const finalData: IParkingZoneWiseData = {
        occupied: totalOccupied,
        available,
        occupancy,
        totalSlot: totalSlots,
      };

      setANPRParkingZoneWiseData(finalData as IParkingZoneWiseData);
    } catch (error) {
      console.error("Error fetching New vs Total chart data:", error);
      throw error;
    } finally {
      setLoadingCount((c) => c - 1);
    }
  };

  const fetchANPRParkingchartData = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const requestData = {
        floorIds: floor,
        zoneIds: zones,
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const responseChart: any = await fetchANPRVehicleParkingService(
        requestData as unknown as IWidgetPayload,
      );

      if (responseChart?.data != null && responseChart?.data?.length > 0) {
        setANPRParkingChartData(responseChart?.data as IANPRParkingChartData[]);
      } else {
        setANPRParkingChartData([]);
      }
    } catch (error) {
      console.error("Error fetching New vs Total chart data:", error);
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
      <Box sx={{ height: pHeight, display: "flex" }}>
        <ANPRParking1_1
          anprParkingZoneWiseData={anprParkingZoneWiseData}
          customizedWidth={customizedWidth}
          customizedHeight={pHeight}
          displayName={displayName}
          onZoomClick={handleZoomClick}
          openZoomDialog={openZoomDialog}
          setIsDraggable={setIsDraggable}
        />

        {size === "2x1" &&
          (expanded === "Option3" ? (
            <ANPRParking2_1_Option3
              customizedWidth={customizedWidth}
              customizedHeight={pHeight}
              anprparkingChartData={anprparkingChartData}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
              anprParkingZoneWiseData={anprParkingZoneWiseData}
            />
          ) : expanded === "Option2" ? (
            <ANPRParking2_1_Option2
              customizedWidth={customizedWidth}
              customizedHeight={pHeight}
              anprParkingTreeChartData={anprParkingTreeChartData}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
            />
          ) : (
            <ANPRParking2_1_Option1
              customizedWidth={customizedWidth}
              customizedHeight={pHeight}
              anprparkingChartData={anprparkingChartData}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
              anprParkingZoneWiseData={anprParkingZoneWiseData}
            />
          ))}
        <Box className="widget-label-bottom">
          {chartName}
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
export { ANPRParkingWidget };
