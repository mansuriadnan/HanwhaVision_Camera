import React, { useEffect, useRef, useState } from "react";
import {
  CommonWidgetProps,
  IWidgetPayload,
  LayoutItem,
  IstatusCount,
  IMaintenanceStatusDataWithTime,
} from "../../../../interfaces/IChart";
import {
  MaintenanceStatus1_1,
  CommonDialog,
  LocalLoader,
  MaintenanceStatus2_1_Option1,
  MaintenanceStatus2_1_Option2,
} from "../../../index";
import { CameraMaintenanceStatusWidgetService } from "../../../../services/dashboardService";
import { Box } from "@mui/material";
import { convertToUTC } from "../../../../utils/convertToUTC";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import apiUrls from "../../../../constants/apiUrls";
import { useSignalRContext } from "../../../../context/SignalRContext";
import { formatDateToConfiguredTimezone } from "../../../../utils/formatDateToConfiguredTimezone";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";

const MaintenanceStatusWidget: React.FC<CommonWidgetProps> = ({
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
  const [statusDataWithTime, setStatusDataWithTime] = useState<
    IMaintenanceStatusDataWithTime[] | null
  >([]);
  const [statusData, setStatusData] = useState<IstatusCount>({
    totalCount: 0,
    completedCount: 0,
    reworkCount: 0,
    aboutToDueCount: 0,
    dueCount: 0,
    inProgressCount: 0,
  });

  useExportHandler({
    apiEndpoint: `${apiUrls.CameraMaintenanceStatusWidget}/csv`,
    startDate: convertDateToISOLikeString(selectedStartDate),
    endDate: convertDateToISOLikeString(selectedEndDate),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });

  useEffect(() => {
    fetchGenderWisePeopleCountDataWithTime();
  }, [floor, zones, selectedStartDate, selectedEndDate]);

  const fetchGenderWisePeopleCountDataWithTime = async () => {
    setLoadingCount((c) => c + 1);
    try {
      const data = {
        floorIds: floor,
        zoneIds: zones,
        startDate: convertToUTC(selectedStartDate),
        endDate: convertToUTC(selectedEndDate),
      };

      const response: any = await CameraMaintenanceStatusWidgetService(
        data as IWidgetPayload
      );

      if (response?.data.length > 0) {
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        // For 1x1 chart - Start
        const counts = response.data.reduce(
          (acc: any, item: any) => {
            const dueDateIST = new Date(
              formatDateToConfiguredTimezone(item.dueDate)
            );

            const latestStatus = item.statusHistory
              .filter((s: any) => s.statusDatetime)
              .reduce((latest: any, current: any) =>
                new Date(current.statusDatetime) >
                new Date(latest.statusDatetime)
                  ? current
                  : latest
              );

            if (latestStatus.status === "Done") {
              acc.completedCount++;
              acc.totalCount++;
            } else if (latestStatus.status === "Rework") {
              acc.reworkCount++;
              acc.totalCount++;
            } else if (latestStatus.status === "In Progress") {
              acc.inProgressCount++;
              acc.totalCount++;
            } else if (
              latestStatus.status === "Not Started" &&
              dueDateIST.getTime() > today.getTime()
            ) {
              acc.aboutToDueCount++;
              acc.totalCount++;
            } else if (
              latestStatus.status === "Not Started" &&
              dueDateIST.getTime() < today.getTime()
            ) {
              acc.dueCount++;
              acc.totalCount++;
            }
            return acc;
          },
          {
            totalCount: 0,
            completedCount: 0,
            reworkCount: 0,
            aboutToDueCount: 0,
            dueCount: 0,
            inProgressCount: 0,
          }
        );
        setStatusData(counts);
        //End

        // For 2x1 multiLine chart data - start
        const flattened = response.data.flatMap((item: any) =>
          item.statusHistory.map((history: any) => ({
            status: history.status,
            dateTime: history.statusDatetime,
          }))
        );

        const grouped = flattened.reduce((acc: any, curr: any) => {
          const dateKey = curr.dateTime;

          if (!acc[dateKey]) {
            acc[dateKey] = {
              dateTime: dateKey,
              notStartedCount: 0,
              inProgressCount: 0,
              reworkCount: 0,
              doneCount: 0,
            };
          }

          switch (curr.status) {
            case "Not Started":
              acc[dateKey].notStartedCount++;
              break;
            case "In Progress":
              acc[dateKey].inProgressCount++;
              break;
            case "Rework":
              acc[dateKey].reworkCount++;
              break;
            case "Done":
              acc[dateKey].doneCount++;
              break;
          }

          return acc;
        }, {});

        const resultArray = Object.values(grouped);

        resultArray.sort(
          (a: any, b: any) =>
            new Date(a.dateTime).getTime() - new Date(b.dateTime).getTime()
        );
        // console.log("resultArray->", resultArray);
        setStatusDataWithTime(resultArray as IMaintenanceStatusDataWithTime[]);
        // end
      } else {
        setStatusData({
          totalCount: 0,
          completedCount: 0,
          reworkCount: 0,
          aboutToDueCount: 0,
          dueCount: 0,
          inProgressCount: 0,
        });

        setStatusDataWithTime([]);
      }
    } catch (error) {
      console.error("Error fetching Gender data:", error);
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
        <MaintenanceStatus1_1
          customizedWidth={customizedWidth}
          displayName={displayName}
          onZoomClick={handleZoomClick}
          openZoomDialog={openZoomDialog}
          setIsDraggable={setIsDraggable}
          statusData={statusData}
        ></MaintenanceStatus1_1>

        {size === "2x1" &&
          (expanded === "Option3" ? null : expanded === "Option2" ? (
            <MaintenanceStatus2_1_Option2
              customizedWidth={customizedWidth}
              customizedHeight={height}
              statusDataWithTime={statusDataWithTime}
              startDate={new Date(selectedStartDate)}
              endDate={new Date(selectedEndDate)}
              floor={floor}
              zones={zones}
              setExportHandler={setExportHandler}
            />
          ) : (
            <MaintenanceStatus2_1_Option1
              customizedWidth={customizedWidth}
              customizedHeight={height}
              statusDataWithTime={statusDataWithTime}
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

export { MaintenanceStatusWidget };
