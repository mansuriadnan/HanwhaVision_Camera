import { useEffect, useRef, useState } from "react";
import { Box } from "@mui/material";
import {
  Parking1_1,
  Parking2_1_Option1,
  Parking2_1_Option2,
  Parking2_1_Option3,
  CommonDialog,
  LocalLoader,
} from "../../../index";
import {
  IWidgetPayload,
  LayoutItem,
  CommonWidgetProps,
  IParkingChartData,
  IParkingZoneWiseData,
  IParkingTreeChartData,
} from "../../../../interfaces/IChart";
import {
  fetchVehicleParkingAnalysisService,
  fetchVehicleParkingByZonesService,
} from "../../../../services/dashboardService";
import { convertToUTC } from "../../../../utils/convertToUTC";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import apiUrls from "../../../../constants/apiUrls";
import { useSignalRContext } from "../../../../context/SignalRContext";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";

const ParkingWidget: React.FC<CommonWidgetProps> = ({
  item,
  floor,
  zones,
  selectedStartDate,
  selectedEndDate,
  setExportHandler,
  setIsDraggable,
}) => {
  const [parkingChartData, setParkingChartData] =
    useState<IParkingChartData[]>();
  const [parkingZoneWiseData, setParkingZoneWiseData] =
    useState<IParkingZoneWiseData>();
  const [parkingTreeChartData, setParkingTreeChartData] =
    useState<IParkingTreeChartData[]>();
  const [openZoomDialog, setOpenZoomDialog] = useState(false);
  const [totalCapacity, setTotalCapacity] = useState();
  const { width, height, size, expanded, displayName, chartName } =
    item as LayoutItem;
  const customizedWidth =
    size === "3x1" ? width / 3 : size === "2x1" ? width / 2 : width;
  const pHeight = height;
  const [loadingCount, setLoadingCount] = useState(0);
  const selectedIntervalNameRef = useRef<string>("");
  const { IsDisplayLoader } = useSignalRContext();

  useEffect(() => {
    fetchParkingDataByZone();
    fetchParkingchartData();
  }, [floor, zones, selectedStartDate, selectedEndDate, size]);

  useExportHandler({
    apiEndpoint: `${apiUrls.VehicleParkingAnalysis}/csv`,
    startDate: convertDateToISOLikeString(selectedStartDate),
    endDate: convertDateToISOLikeString(selectedEndDate),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  const fetchParkingDataByZone = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const requestData = {
        floorIds: floor,
        zoneIds: zones,
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const responseChart: any = await fetchVehicleParkingByZonesService(
        requestData as unknown as IWidgetPayload,
      );

      setParkingTreeChartData(responseChart?.data);

      const zoneData = responseChart?.data || [];
      const totalOccupied = zoneData.reduce(
        (sum: number, item: any) => sum + item.parkingCount,
        0,
      );

      const totalSlots = zoneData.reduce(
        (sum: number, item: any) => sum + item.totalParkingOccupancy,
        0,
      );

      setTotalCapacity(totalSlots);

      const available = totalSlots - totalOccupied;

      const occupancy = totalSlots > 0 ? (totalOccupied / totalSlots) * 100 : 0;

      const finalData: IParkingZoneWiseData = {
        occupied: totalOccupied,
        available,
        occupancy,
        totalSlot: totalSlots,
      };

      setParkingZoneWiseData(finalData as IParkingZoneWiseData);
    } catch (error) {
      console.error("Error fetching New vs Total chart data:", error);
      throw error;
    } finally {
      setLoadingCount((c) => c - 1);
    }
  };

  const fetchParkingchartData = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const requestData = {
        floorIds: floor,
        zoneIds: zones,
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const responseChart: any = await fetchVehicleParkingAnalysisService(
        requestData as unknown as IWidgetPayload,
      );

      const formattedData: IParkingChartData[] =
        responseChart?.data?.map((item: any) => {
          const occupied = item.queueCount;
          const available = item.queueCount;

          return {
            dateTime: item.dateTime,
            occupied,
            available,
          };
        }) || [];

      setParkingChartData(formattedData);
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
        <Parking1_1
          parkingZoneWiseData={parkingZoneWiseData}
          customizedWidth={customizedWidth}
          customizedHeight={pHeight}
          displayName={displayName}
          onZoomClick={handleZoomClick}
          openZoomDialog={openZoomDialog}
          setIsDraggable={setIsDraggable}
        />

        {size === "2x1" &&
          (expanded === "Option3" ? (
            <Parking2_1_Option3
              customizedWidth={customizedWidth}
              customizedHeight={pHeight}
              parkingChartData={parkingChartData}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
              totalCapacity={totalCapacity}
            />
          ) : expanded === "Option2" ? (
            <Parking2_1_Option2
              customizedWidth={customizedWidth}
              customizedHeight={pHeight}
              parkingTreeChartData={parkingTreeChartData}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
            />
          ) : (
            <Parking2_1_Option1
              customizedWidth={customizedWidth}
              customizedHeight={pHeight}
              parkingChartData={parkingChartData}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
              totalCapacity={totalCapacity}
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
export { ParkingWidget };
