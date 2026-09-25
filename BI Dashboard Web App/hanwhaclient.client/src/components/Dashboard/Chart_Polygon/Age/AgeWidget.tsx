import React, { useEffect, useRef, useState } from "react";
import {
  CommonWidgetProps,
  IWidgetPayload,
  LayoutItem,
  IAgeDataWithTime,
} from "../../../../interfaces/IChart";
import {
  Age1_1,
  CommonDialog,
  LocalLoader,
  Age2_1_Option1,
  Age2_1_Option2,
  Age3_1_Option1,
} from "../../../index";
import { fetchAgeWisePeopleCountService } from "../../../../services/dashboardService";
import { Box } from "@mui/material";
import { convertToUTC } from "../../../../utils/convertToUTC";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import apiUrls from "../../../../constants/apiUrls";
import { useSignalRContext } from "../../../../context/SignalRContext";
import { formatDateToConfiguredTimezone } from "../../../../utils/formatDateToConfiguredTimezone";
import moment from "moment";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";

const AgeWidget: React.FC<CommonWidgetProps> = ({
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
  const selectedIntervalNameRef = useRef<string>("");
  const { IsDisplayLoader } = useSignalRContext();
  const [groupbyDateAgeData, setGroupbyDateAgeData] = useState<
    IAgeDataWithTime[]
  >([]);
  const [ageDataWithTime, setAgeDataWithTime] = useState<
    IAgeDataWithTime[] | null
  >([]);

  useExportHandler({
    apiEndpoint: `${apiUrls.AgeWisePeopleCountAnalysisCsv}/csv`,
    startDate: convertDateToISOLikeString(selectedStartDate),
    endDate: convertDateToISOLikeString(selectedEndDate),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  useEffect(() => {
    fetchAgeWisePeopleCountData();
  }, [floor, zones, selectedStartDate, selectedEndDate]);

  const fetchAgeWisePeopleCountData = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const data = {
        floorIds: floor,
        zoneIds: zones,
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const response: any = await fetchAgeWisePeopleCountService(
        data as IWidgetPayload
      );

      if (response?.data.length > 0) {
        setAgeDataWithTime(response?.data as IAgeDataWithTime[]);
        const formattedData = response?.data.map((d: IAgeDataWithTime) => ({
          ...d,
          date: new Date(formatDateToConfiguredTimezone(d.dateTime) as string),
        }));

        // Group by "yyyy-MM-dd" (ignoring time part)
        const groupedData = formattedData.reduce((acc: any, curr: any) => {
          const key = moment(curr.date).format("YYYY-MM-DD");

          if (!acc[key]) {
            acc[key] = {
              date: key,
              youngCount: curr.youngCount,
              adultCount: curr.adultCount,
              seniorCount: curr.seniorCount,
              unknownCount: curr.unknownCount,
            };
          } else {
            acc[key].youngCount += curr.youngCount;
            acc[key].adultCount += curr.adultCount;
            acc[key].seniorCount += curr.seniorCount;
            acc[key].unknownCount += curr.unknownCount;
          }

          return acc;
        }, {});

        const groupedArray: IAgeDataWithTime[] = Object.values(groupedData);
        setGroupbyDateAgeData(groupedArray);
      } else {
        setGroupbyDateAgeData([]);
      }
    } catch (error) {
      console.error("Error fetching Age data:", error);
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
        <Age1_1
          customizedWidth={customizedWidth}
          displayName={displayName}
          onZoomClick={handleZoomClick}
          openZoomDialog={openZoomDialog}
          setIsDraggable={setIsDraggable}
          groupbyDateAgeData={groupbyDateAgeData}
        ></Age1_1>

        {size === "2x1" &&
          (expanded === "Option3" ? null : expanded === "Option2" ? (
            <Age2_1_Option2
              customizedWidth={customizedWidth}
              customizedHeight={height}
              ageDataWithTime={ageDataWithTime}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
            />
          ) : (
            <Age2_1_Option1
              customizedWidth={customizedWidth}
              customizedHeight={height}
              groupbyDateAgeData={groupbyDateAgeData}
            />
          ))}

        {/* {size === "3x1" &&
          (expanded === "Option1" ? (
            <Age3_1_Option1
              customizedWidth={773}
              customizedHeight={height}
              groupbyDateAgeData={groupbyDateAgeData}
            />
          ) : null)} */}
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

export { AgeWidget };
