import React, { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import {
  GetAllFloorsListService,
  GetAllZonesByFloorIdService,
  GetDashboardDesign,
  SaveDashboardDesign,
  GetAllFloorsListByServerIdService,
  GetAllServersListService,
} from "../../services/dashboardService";
import { DraggableChart } from "../Draggable/DraggableChart";
import {
  CommonDialog,
  CustomButton,
  CustomMultiSelect,
  showToast,
} from "../../components";
import {
  Box,
  Checkbox,
  Grid,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Typography,
} from "@mui/material";
import { ILookup } from "../../interfaces/ILookup";
import {
  IFloorZoneIds,
  Layouts,
  FloorZoneState,
} from "../../interfaces/IChart";
import dayjs, { Dayjs } from "dayjs";
import {
  dashboardCharts,
  dashboardChartsType,
} from "../../constants/dashboardChartList";
import AddIcon from "@mui/icons-material/Add";
import { useForm } from "react-hook-form";
import { CustomDateTimeRangePicker } from "../../components/Reusable/CustomDateTimeRangePicker";
import ExportWidgetDialog from "./ExportWidgetDialog";
import {
  checkWidgetPermission,
  HasWidgetPermission,
} from "../../utils/screenAccessUtils";
import { useThemeContext } from "../../context/ThemeContext";
import { useSignalRContext } from "../../context/SignalRContext";
import { leftGroup } from "../../utils/signalRService";
import { useTranslation } from "react-i18next";
import { multipleLeftGroup } from "../../utils/multipleSignalRService";

const DashboardPage: React.FC = () => {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const id = searchParams.get("id");

  const [floorList, setFloorList] = useState<ILookup[]>([]);
  const [zoneList, setZoneList] = useState<ILookup[]>([]);
  const [layouts, setLayouts] = useState<Layouts | null>(null);
  const [open, setOpen] = React.useState(false);
  const [selectedWidgets, setSelectedWidgets] = useState<number[]>([]);
  const [selectedWidgetType, setSelectedWidgetType] = useState("All");
  const [selectedStartDate, setSelectedStartDate] = useState<Dayjs | null>(
    dayjs().startOf("day")
  );
  const [selectedEndDate, setSelectedEndDate] = useState<Dayjs | null>(
    dayjs(new Date())
  );
  const [exportDialogOpen, setExportDialogOpen] = useState(false);

  const [finalFloorZone, setFinalFloorZone] = useState<FloorZoneState>({
    finlafloorList: [],
    finalzoneList: [],
  });

  const [initalLoad, setInitalLoad] = useState(true);
  const [serverList, setServerList] = useState<ILookup[]>([]);

  const { control, setValue, watch } = useForm<IFloorZoneIds>({
    defaultValues: {
      selectedServerIds: [], 
      selectedFloorIds: [],
      selectedZonesIds: [],
      selectedExport: "",
    },
  });

  const checkedWidgetPermission = checkWidgetPermission();
  const permissionMap: Record<string, string> = Object.fromEntries(
    dashboardCharts.map((chart) => [chart.chartName, chart.permission])
  );

  const selectedServerIds = watch("selectedServerIds");
  const selectedFloorIds = watch("selectedFloorIds");
  const selectedZonesIds = watch("selectedZonesIds");

  const { theme, themeColor } = useThemeContext();
  const { triggerRefresh } = useSignalRContext();
  const { setIsTodayValue } = useSignalRContext();
  const defaultFloorId = "000000000000000000000000";
  const isMaintenance = localStorage.getItem("isMaintenance") === "true";
  const isANPR = localStorage.getItem("isANPR") === "true";


  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
        ? `${themeColor}/dark-theme/`
        : `${themeColor}/`;

  useEffect(() => {
    const fetchData = async () => {
      await fetchServerData();  //await fetchFloorData();
    };
    fetchData();
  }, []);

  useEffect(() => {
  fetchFloorDataByServer(selectedServerIds);
}, [selectedServerIds]);

  useEffect(() => {
    fetchZoneData(selectedFloorIds);
  }, [selectedFloorIds]);

  const handleDateTimeApply = ({
    startDate,
    endDate,
  }: {
    startDate: Date;
    endDate: Date;
  }) => {
    setSelectedStartDate(dayjs(startDate));
    setSelectedEndDate(dayjs(endDate));
  };

  useEffect(() => {
    setLayouts(null);
    fetchDashboardDesign();
    localStorage.setItem("activeRoute", `/dashboard?id=${id}`);
  }, [id]);

  useEffect(() => {
    if (!selectedFloorIds || selectedFloorIds.length === 0) return;

    const lastSelectedId = selectedFloorIds[selectedFloorIds.length - 1];

    if (lastSelectedId === defaultFloorId) {
      if (selectedFloorIds.length !== 1) {
        setValue("selectedFloorIds", [defaultFloorId], {
          shouldValidate: true,
        });
      }
    } else {
      if (selectedFloorIds.includes(defaultFloorId)) {
        setValue(
          "selectedFloorIds",
          selectedFloorIds.filter((id) => id !== defaultFloorId),
          { shouldValidate: true }
        );
      }
    }
  }, [selectedFloorIds]);

  const handleSetFloorAndZone = () => {
    triggerRefresh(false);
    setFinalFloorZone({
      finlafloorList: selectedFloorIds,
      finalzoneList: selectedZonesIds,
    });
  };

  const fetchDashboardDesign = async () => {
    // const data: any = await GetDashboardDesign();
    // const record = data.find((item: any) => item.id === id);

    // const json = record?.dashboardPreferenceJson;

    // const record = dashboardPrefData && dashboardPrefData.find((item: any) => item.id === id);
    // const json = record?.dashboardPreferenceJson;
    // let dashData = dashboardPrefData;
    // if(!dashboardPrefData){
    //   dashData = await GetDashboardDesign();
    //   setDashboardPrefData(dashData);
    // }
    const dashData: any = await GetDashboardDesign();
    const record = dashData?.find((item: any) => item.id === id);
    const json = record?.dashboardPreferenceJson;
    if (json && json !== "[]" && json !== "{}") {
      const parsedLayouts = JSON.parse(json);

      const filteredLayouts = (parsedLayouts || []).filter((item: any) => {
        const permission = permissionMap[item.chartName];
        return !permission || checkedWidgetPermission(permission); // ✅ valid usage
      });
      if (filteredLayouts.length > 0) {
        setLayouts({
          lg: filteredLayouts,
        });
      } else {
        setLayouts(null);
      }
    } else {
      setLayouts(null);
    }
  };

  const fetchFloorData = async () => {
    try {
      const response = await GetAllFloorsListService(true);
      const floorData = response?.map((item) => ({
        title: item.floorPlanName,
        id: item.id,
      }));
      setFloorList(floorData as ILookup[]);
      if (floorData && floorData.length > 0 && floorData[0].id) {
        const firstFloorId = floorData[0].id;
        setValue("selectedFloorIds", firstFloorId ? [firstFloorId] : []);
      }
    } catch (err: any) {
      console.error("Error while fetching the floor data");
    }
  };

  const fetchServerData = async () => {
  try {
    const response = await GetAllServersListService();
    const serverData = response?.map((item: any) => ({
      title: item.serverName,
      id: item.id,
    }));
    setServerList(serverData as ILookup[]);
    if (serverData && serverData.length > 0 && serverData[0].id) {
      const myServerId = "000000000000000000000000"; // Replace with your actual server ID
      setValue("selectedServerIds", myServerId ? [myServerId] : []);
    }
  } catch (err: any) {
    console.error("Error while fetching the server data");
  }
};

const fetchFloorDataByServer = async (serverIds: string[]) => {
  if (!Array.isArray(serverIds) || serverIds.length === 0) {
    setFloorList([]);
    setValue("selectedFloorIds", []);
    return;
  }

  try {
    const response: any = await GetAllFloorsListByServerIdService(serverIds);

    const floorData = (response?.data ?? response ?? []).map((item: any) => ({
      title: item.floorPlanName,
      id: item.id,
    }));

    setFloorList(floorData as ILookup[]);

    if (floorData.length > 0 && floorData[0].id) {
      setValue("selectedFloorIds", [floorData[0].id]);
    } else {
      setValue("selectedFloorIds", []);
    }
    // (removed the extra unconditional reset that was here)
  } catch (err: any) {
    console.error("Error while fetching floor data for server:", err?.message || err);
    setFloorList([]);
  }
};
  const fetchZoneData = async (floorIds: string[]) => {
    if (!Array.isArray(floorIds) || floorIds.length === 0) {
      setZoneList([]);
      return;
    }

    try {
      const response: any = await GetAllZonesByFloorIdService(floorIds);

      const allZones: ILookup[] = (response?.data ?? []).flatMap((floor: any) =>
        Array.isArray(floor?.zones)
          ? floor.zones.map((zone: any) => ({
              id: zone.id,
              title: zone.zoneName,
            }))
          : []
      );

      setZoneList(allZones);
      // if (allZones && allZones?.length > 0 && allZones[0]?.id) {

      setValue("selectedZonesIds", []);
      if (initalLoad) {
        setFinalFloorZone({
          finlafloorList: selectedFloorIds,
          finalzoneList: [],
        });
        setInitalLoad(false);
      }
      // } else {
      //   setValue("selectedZonesIds", []);
      //   setFinalFloorZone({
      //     finlafloorList: selectedFloorIds,
      //     finalzoneList: [],
      //   });
      //   setInitalLoad(false);
      // }
    } catch (err: any) {
      console.error("Error while fetching the zone data:", err?.message || err);
      setZoneList([]);
    }
  };

  const handleClickOpen = () => {
    setOpen(true);
  };

  const handleClose = () => {
    setSelectedWidgets([]);
    setOpen(false);
  };

  const backgroundStyle = {
    backgroundImage: ` url('/images/lines.png'), linear-gradient(287.68deg, #FE6500 -0.05%, #FF8A00 57.77%)`,
    backgroundSize: "cover",
    backgroundRepeat: "no-repeat",
    backgroundPosition: "center",
  };

  const addItem = () => {
    // Get the last item's index to continue numbering sequentially
    const lastIndex = layouts?.lg?.length || 0;

    const existingChartIds = new Set(
      layouts?.lg?.map((item) => Number(item.chartID))
    );

    const existingCount = existingChartIds.size;
    const newCount = selectedWidgets.length;

    if (existingCount + newCount > 10) {
      showToast("You can only add up to 10 widgets per dashboard", "error");
      return;
    }

    let totalWidth =
      layouts?.lg?.reduce((sum, item) => sum + item.w, 0) ??
      (lastIndex * 3) % 12;

    // const existingchartslength = existingChartIds.length
    const duplicateWidgets = selectedWidgets.filter((id) =>
      existingChartIds.has(id)
    );

    const nonDuplicateWidgets = selectedWidgets.filter(
      (id) => !existingChartIds.has(id)
    );

    setSelectedWidgets(nonDuplicateWidgets);

    const duplicateChartNames = dashboardCharts
      .filter((chart) => duplicateWidgets.includes(chart.id))
      .map((chart) => chart.chartName);

    if (duplicateWidgets.length > 0) {
      showToast(
        `Widgets are already in dashboard: ${duplicateChartNames.join(", ")}`,
        "error"
      );
    }

    if (nonDuplicateWidgets.length === 0) {
      handleClose();
      return;
    }

    nonDuplicateWidgets.forEach((chartId, index) => {
      const chart = dashboardCharts.find((chart) => chart.id === chartId);
      const width =
        chart?.id === 38 ||
        chart?.id === 39 ||
        chart?.id === 35 ||
        chart?.id === 36 ||
        chart?.id === 37 ||
        chart?.id === 41 ||
        chart?.id === 46
          ? 765
          : 366;
      const height =
        chart?.id === 38 ||
        chart?.id === 39 ||
        chart?.id === 35 ||
        chart?.id === 36 ||
        chart?.id === 37 ||
        chart?.id === 41 ||
        chart?.id === 46
          ? 768
          : 0;
      const w =
        chart?.id === 38 ||
        chart?.id === 39 ||
        chart?.id === 35 ||
        chart?.id === 36 ||
        chart?.id === 37 ||
        chart?.id === 41 ||
        chart?.id === 46
          ? 6
          : 3;
      const h =
        chart?.id === 38 ||
        chart?.id === 39 ||
        chart?.id === 35 ||
        chart?.id === 36 ||
        chart?.id === 37 ||
        chart?.id === 41 ||
        chart?.id === 46
          ? 10
          : 5;

      const x = totalWidth % 12;
      const y = Math.floor(totalWidth / 12) * 5;

      const newItem = {
        i: (chart?.id || 0).toString(),
        x,
        y,
        w,
        h,
        width,
        height,
        content: `New Item ${lastIndex + index + 1}`,
        chartID: chart?.id || 0,
        chartName: chart?.chartName || "",
        displayName: chart?.chartName || "",
        // size: "1x1",
        size:
          chart?.id === 38 ||
          chart?.id === 39 ||
          chart?.id === 35 ||
          chart?.id === 36 ||
          chart?.id === 37 ||
          chart?.id === 41 ||
          chart?.id === 46
            ? "2x1"
            : "1x1",
        expanded: "Option1",
      };

      // Save each new item one-by-one
      setLayouts((prevLayouts: Layouts | null) => ({
        lg: [...(prevLayouts?.lg || []), newItem],
      }));

      // Optional: update totalWidth if needed
      totalWidth += w;
    });

    setSelectedWidgets([]);
    handleClose();
  };

  const toggleChartSelection = (id: number) => {
    setSelectedWidgets((prev) =>
      prev.includes(id)
        ? prev.filter((chartId) => chartId !== id)
        : [...prev, id]
    );
  };


  const isChartVisible = (chart: any) => {
    // Backend permission check (ONLY here)
    const hasPermission =
      !chart.permission || HasWidgetPermission(chart.permission);

    // License checks
    if (chart.type === "Maintenance" && !isMaintenance) return false;
    if (chart.type === "ANPR" && !isANPR) return false;

    return hasPermission;
  };


  const permittedChartTypes = dashboardChartsType.filter((typeItem) => {
    if (typeItem.type === "All") {
      return dashboardCharts.some(isChartVisible);
    }

    return dashboardCharts.some(
      (chart) =>
        chart.type === typeItem.type &&
        isChartVisible(chart)
    );
  });

  const filteredWidgetsType =
    selectedWidgetType === "All"
      ? dashboardCharts.filter(isChartVisible)
      : dashboardCharts.filter(
        (chart) =>
          chart.type === selectedWidgetType &&
          isChartVisible(chart)
      );

  const saveDashboardItem = async () => {
    try {
      const DesignData = {
        id: id,
        dashboardDesignjson: JSON.stringify(layouts?.lg),
      };
      await SaveDashboardDesign(DesignData);
    } catch (err: any) {
      showToast(
        "An error occurred while saving user dashboard design.",
        "error"
      );
    }
  };

  return (
    <>
      {initalLoad == false && layouts && layouts.lg && layouts.lg.length > 0 ? (
        <>
          <Box className="dashbourd-retail-details-head">
              {serverList && serverList.length > 1 && (
            <div className="dashbourd-retail-details-server">            
              <CustomMultiSelect
                name="selectedServerIds"
                control={control}
                label="Select Server(s)*"
                options={serverList}
                placeholder="Select Server(s)*"
              />
             
            </div>
             )}
            <div className="dashbourd-retail-details-floor">
              <CustomMultiSelect
                name="selectedFloorIds"
                control={control}
                label="Select Floor(s)*"
                options={floorList}
                placeholder="Select Floor(s)*"
                // rules={{ required: "At least one role must be selected" }}
                // required
              />
            </div>
            <div className="dashbourd-retail-details-select-zone">
              <CustomMultiSelect
                name="selectedZonesIds"
                control={control}
                label="Select Zone(s)"
                options={zoneList}
                placeholder="Select Zone(s)"
                // rules={{ required: "At least one role must be selected" }}
                // required
              />
            </div>

            <div className="dashbourd-retail-details-date">
              <CustomDateTimeRangePicker
                initialStartDate={selectedStartDate?.toDate()}
                initialEndDate={selectedEndDate?.toDate()}
                onApply={({ startDate, endDate, endtime  }) => {
                  setSelectedStartDate(dayjs(startDate));      

                  const diffMs = endDate.getTime() - startDate.getTime(); // difference in milliseconds
                  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

                  // Check if startDate is today
                  const isStartDateToday = (() => {
                    const today = new Date();
                    return (
                      startDate.getDate() === today.getDate() &&
                      startDate.getMonth() === today.getMonth() &&
                      startDate.getFullYear() === today.getFullYear()
                    );
                  })();

                  // Parse endtime (expected format: "HH:mm")
                  let hours = 0;
                  let minutes = 0;
                  if (
                    endtime &&
                    typeof endtime === "string" &&
                    endtime.includes(":")
                  ) {
                    const [h, m] = endtime.split(":").map(Number);
                    hours = h;
                    minutes = m;
                  }

                  if (diffDays === 0 && isStartDateToday) {
                    const roundedEnd = new Date(
                      endDate.getFullYear(),
                      endDate.getMonth(),
                      endDate.getDate(),
                      hours,
                      minutes
                    );
                    setSelectedEndDate(dayjs(roundedEnd));
                    setIsTodayValue(true);
                  } else {
                    setSelectedEndDate(dayjs(endDate));
                    leftGroup();
                    multipleLeftGroup();
                    setIsTodayValue(false);
                  }
                }}
              />
              <img src="images/calendar.png" alt="" />
            </div>
            <div className="dashbourd-retail-details-export dashbourd-search-only">
              <CustomButton variant="outlined" onClick={handleSetFloorAndZone}>
                <img src="images/search.svg" alt="" />
              </CustomButton>
            </div>
            <div className="dashbourd-retail-details-export">
              <CustomButton
                variant="outlined"
                onClick={() => {
                  const todayMidnight = new Date();
                  todayMidnight.setHours(0, 0, 0, 0);
                  setSelectedStartDate(dayjs(todayMidnight));
                  setSelectedEndDate(dayjs(new Date()));
                }}
              >
                Live
              </CustomButton>
            </div>
            <div className="dashbourd-retail-details-export">
              {/* <CustomSelect
                name="selectedExport"
                variant="filled"
                control={control}
                label="Export your report"
                options={ExportDataList}
              /> */}
              <CustomButton
                variant="outlined"
                onClick={() => setExportDialogOpen(true)}
              >
                <img src="images/directbox-notif.svg" alt="" />
              </CustomButton>
              <ExportWidgetDialog
                open={exportDialogOpen}
                onClose={() => setExportDialogOpen(false)}
                layouts={layouts}
                selectedFloors={selectedFloorIds}
                selectedZones={selectedZonesIds}
                selectedStartDate={selectedStartDate}
                selectedEndDate={selectedEndDate}
                floorList={floorList}
                zoneList={zoneList}
              />
            </div>
          </Box>
          <DraggableChart
            selectedFloors={finalFloorZone.finlafloorList}
            selectedZones={finalFloorZone.finalzoneList}
            layouts={layouts}
            setLayouts={setLayouts}
            selectedStartDate={selectedStartDate}
            selectedEndDate={selectedEndDate}
          />
          <Box
            sx={{
              position: "fixed",
              bottom: 24,
              right: 24,
              zIndex: 1000,
            }}
          >
            <IconButton onClick={saveDashboardItem}>
              <img
                src={
                  theme === "light"
                    ? "images/save_dashboard1.svg"
                    : "images/dark-theme/save_dashboard1.svg"
                }
                alt="Save Dashboard Data"
                height={"50px"}
                width={"50px"}
              />
            </IconButton>
            <IconButton
              onClick={handleClickOpen}
              className="plus-i-widgets"
            >
              <AddIcon sx={{ color: "#fff", fontSize: "28px" }} />
            </IconButton>
          </Box>
        </>
      ) : (
        <>
          <Box className="top-orange-head" style={backgroundStyle}>
            <Box className="top-orange-head-left">
              <Typography variant="h4">{t("Dashboard.New_Dashboard_header")}</Typography>
              <Typography>
                {t("Dashboard.New_Dashboard_Description")}
              </Typography>
            </Box>
          </Box>
          <Box
            className="add-widget"
            sx={{
              backgroundImage:
                theme === "light"
                  ? "url('images/add-widget.png')"
                  : "url('images/dark-theme/add-widget-dark.png')",
            }}
          >
            <Box className="add-widget-wrapper">
              <img
                src={`/images/${themeColorPath}Add_Widget.gif`}
                alt="Animated GIF"
              />
              <h3>{t("Dashboard.Add_Widget")} </h3>
              <p>
                {t("Dashboard.Add_Widget_line1")}
                <br /> {t("Dashboard.Add_Widget_line2")}
              </p>
              <CustomButton variant="outlined" onClick={handleClickOpen}>
                <img src={"/images/adddevice.svg"} alt="Add Devices" />
                 {t("Dashboard.Add_Widget_btn_text")}
              </CustomButton>
            </Box>
          </Box>
        </>
      )}
      <CommonDialog
        open={open}
        title="Add Widgets"
        fullWidth={true}
        maxWidth={"xl"}
        customClass="add-widgets-main add-widget-outer-pop"
        content={
          <Box className="add-widets-butttons" sx={{ display: "flex" }}>
            <Box className="add-widets-listing">
              <List>
                {permittedChartTypes.map((widgetType) => (
                  <ListItem
                    key={widgetType.id}
                    disablePadding
                    className={
                      selectedWidgetType === widgetType.type ? "active" : ""
                    }
                  >
                    <ListItemButton
                      onClick={() => setSelectedWidgetType(widgetType.type)}
                      // sx={{
                      //   color:
                      //     selectedWidgetType === widgetType.type
                      //       ? "orange"
                      //       : null,
                      // }}
                    >
                      <ListItemText primary={widgetType.type} />
                    </ListItemButton>
                  </ListItem>
                ))}
              </List>
            </Box>

            {/* Main Content Area */}
            <Box className="add-widets-box-wrapper">
              {filteredWidgetsType.length === 0 ? (
                <Box
                  display="flex"
                  justifyContent="center"
                  alignItems="center"
                  height="100%"
                  color="text.secondary"
                >
                  No Widgets Available
                </Box>
              ) : (
                <Grid container spacing={2} alignItems="stretch">
                  {filteredWidgetsType.map((chart) => {
                    return (
                      <Grid
                        item
                        xs={12}
                        sm={6}
                        md={4}
                        key={chart.id}
                        className={
                          selectedWidgets.includes(chart.id) ? "active" : ""
                        }
                      >
                        <Box
                          sx={{
                            position: "relative",
                            display: "inline-block",
                          }}
                          className="add-widets-box-wrapper-content"
                        >
                          <Box
                            className="add-widets-box-wrapper-content-check"
                          >
                            {/* <Typography
                            variant="body2"
                            sx={{ flexGrow: 1, textAlign: "center" }}
                          >
                            {chart.chartName}
                          </Typography> */}

                            <Checkbox
                              checked={selectedWidgets.includes(chart.id)}
                              onChange={() => toggleChartSelection(chart.id)}
                            />
                          </Box>

                          <Box className="add-widets-box-wrapper-image">
                            <div className="add-widets-box-image-wrapper">
                              <img
                                src={
                                  theme === "light"
                                    ? `/images/dashboard/${chart.chartName.replace(
                                        /[^a-zA-Z0-9]/g,
                                        "_"
                                      )}_1x1.webp`
                                    : `/images/dark-theme/dashboard/${chart.chartName.replace(
                                        /[^a-zA-Z0-9]/g,
                                        "_"
                                      )}_1x1.webp`
                                }
                                alt={chart.chartName.replace(
                                  /[^a-zA-Z0-9]/g,
                                  "_"
                                )}
                              />
                            </div>
                            <span style={{ fontSize: "10px" }}>
                              {chart.chartName}
                            </span>
                          </Box>
                        </Box>
                      </Grid>
                    );
                  })}
                </Grid>
              )}
            </Box>
          </Box>
        }
        onConfirm={addItem}
        onCancel={handleClose}
        confirmText="Ok"
        cancelText="Cancel"
      />
    </>
  );
};

export default DashboardPage;
