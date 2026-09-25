import React, {
  useState,
  useEffect,
} from "react";
import {
  Typography,
  TextField,
  Box,
  InputAdornment,
  FormLabel,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Divider,
  IconButton,
  CircularProgress,
  LinearProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Tooltip,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { GetAllSiteService } from "../../services/siteManagementService";
import { CustomStyleMultiSelect } from "../../components/Reusable/CustomStyleMultiSelect";
import { DatePicker, LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { convertToUTC } from "../../utils/convertToUTC";
import { Controller, useForm } from "react-hook-form";
import dayjs, { Dayjs } from "dayjs";
import { CommonDialog, CustomButton, showToast } from "../../components";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import SSMServerChartDialog from "./SSMServerChartDialog";
import { ApiServer, ApiSiteData, ISSMPayload, MappedServer, MappedSite, SiteSummary } from "../../interfaces/IManageServer";
import { fetchSSMSiteDataService } from "../../services/SSMService";
import { getUsageColor } from "../../utils/getUsageColor ";
import { useThemeContext } from "../../context/ThemeContext";
import SSMDeviceScreen from "./SSMDeviceScreen";
import { CustomDateTimeRangePicker } from "../../components/Reusable/CustomDateTimeRangePicker";
import { HasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";


interface HeaderInputs {
  siteIds: string[];
  startDate: Dayjs | null;
}

type ServerDevice = {
  id: string;
  name: string;
};
//  OUTSIDE SSMDashboard — these are pure functions, no need to recreate
const mbToGb = (mb: string): number => Math.round(parseInt(mb) / 1024);

const getServerIcon = (status: string | number): string => {
  if (status === "Connected") return "/images/SSM/online_server.svg";
  if (status === "Disconnected" || status === 0) return "/images/SSM/offline_server.svg";
  return "/images/SSM/warning_server.svg";
};
const SSMDashboard = () => {
  const [siteList, setSiteList] = useState<any[]>([]);
  const [selectedDate, setSelectedDate] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false); 
  const [selectedServer, setSelectedServer] = useState<any>(null);  
  const [siteData, setSiteData] =  useState<MappedSite[]>([]);
  const [summary, setSummary] = useState<SiteSummary>({
    totalCamera: 0,
    totalSSM: 0,
    failureCamera: 0,
    failureSSM: 0,
  });

  const [currentView, setCurrentView] = useState<"dashboard" | "devices">("dashboard");
  const [selectedServerForDevice, setSelectedServerForDevice] = useState<{ id: string; name: string } | null>(null);
  const [openExportDialog, setOpenExportDialog] = useState(false);
  const [selectedStartDate, setSelectedStartDate] = useState<Dayjs | null>(
    dayjs().startOf("day")
  );
  const [selectedEndDate, setSelectedEndDate] = useState<Dayjs | null>(
    dayjs().endOf("day")
  );
  const { theme, themeColor } = useThemeContext();
  const { t } = useTranslation();
  const scrollRef = React.useRef<number>(0);

  const themeColorPath =
  themeColor === "default-theme"
    ? theme === "dark"
      ? "dark-theme/"
      : ""
    : theme === "dark"
      ? `${themeColor}/dark-theme/`
      : `${themeColor}/`;

  const {
    control,
    setValue,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<HeaderInputs>({
    defaultValues: {
      siteIds: [],
      startDate: null,
    },
  });
  

  useEffect(() => {
    GetAllSite();
  }, []);

  useEffect(() => {
    if (currentView === "dashboard") {
      window.scrollTo(0, scrollRef.current);
    }
  }, [currentView]);

  const GetAllSite = async () => {
    try {
      const response: any = await GetAllSiteService();
      if (response?.isSuccess) {
        const options = getDropdownOptions(response?.data);
        setSiteList(options);
      }
    } catch (err: any) {
    } finally {
      // hideLoading();
    }
  };

  //--------------------------------------------------
  

// Maps a single API server → siteData server shape
const mapServer = (srv: ApiServer): MappedServer  => {

  const drives = (srv.disks ?? []).map((d) => ({
    name:          d.drive,
    totalCapacity: mbToGb(String(d.total)),
    used:          mbToGb(String(d.total)) - mbToGb(String(d.free)),
  }));  

  return {
    id:           srv.id,
    name:         srv.name,
    ip:           `${srv.ipAddress}:${srv.port}`,
    cameras:      srv.totalCameraCount,
    online:       srv.totalCameraCount - srv.failureCameraCount,
    offline:      srv.failureCameraCount,
    cpu:          Math.round(Number(srv.totalProcessorUsage)),
    ram:          Math.round(Number(srv.totalMemoryUsage)),
    totalStorage: srv.diskFreePercentage ?  Math.round(100 - Number(srv.diskFreePercentage)) : 0, // used %
    drive:        drives,
    status :      srv.status
  };
};

// Maps full API response array → siteData shape
const mapApiToSiteData = (apiData: ApiSiteData[]): {
  mappedData: MappedSite[];
  summary: SiteSummary;
} => {


const mappedData = apiData.map((parentSite) => {


    // ── Map parent servers ──
    const servers = parentSite.parentServers.map(mapServer);

    // ── Map subsites (recursive-ready) ──
    const subsite = (parentSite.subSites ?? []).map((sub) => {
      const subServers = sub.servers.map(mapServer);

      const subTotalCamera   = sub.servers.reduce((s, srv) => s + srv.totalCameraCount,   0);
      const subFailureCamera = sub.servers.reduce((s, srv) => s + srv.failureCameraCount, 0);

      return {
        siteID :       sub.childSiteId,
        siteName:      sub.childSiteName,
        totalCamera:   subTotalCamera,
        totalSSM:      subServers.length,
        failureCamera: subFailureCamera,
        failureSSM:    subServers.filter((s) => s.status === "Disconnected").length,
        servers:       subServers,
      };
    });

    // ── Sum counts for parent site (parent servers + all subsites) ──
    const parentTotalCamera   = parentSite.parentServers.reduce((s, srv) => s + srv.totalCameraCount,   0);
    const parentFailureCamera = parentSite.parentServers.reduce((s, srv) => s + srv.failureCameraCount, 0);
    const subTotalCamera      = subsite.reduce((s, sub) => s + sub.totalCamera,   0);
    const subFailureCamera    = subsite.reduce((s, sub) => s + sub.failureCamera, 0);
    const subTotalSSM         = subsite.reduce((s, sub) => s + sub.totalSSM,      0);
    const subFailureSSM       = subsite.reduce((s, sub) => s + sub.failureSSM, 0);

    return {
      siteID : parentSite.parentSiteId,
      siteName:     parentSite.parentSiteName,
      totalCamera:   parentTotalCamera + subTotalCamera,
      totalSSM:      servers.length    + subTotalSSM,
      failureCamera: parentFailureCamera + subFailureCamera,
      failureSSM:    servers.filter((s) => s.status === "Disconnected").length + subFailureSSM,
      servers,
      subsite,
    };
  }
  );

    //  Grand total — sum across all parent sites in one reduce
  const summary = mappedData.reduce(
    (acc, site) => ({
      totalCamera:   acc.totalCamera   + site.totalCamera,
      totalSSM:      acc.totalSSM      + site.totalSSM,
      failureCamera: acc.failureCamera + site.failureCamera,
      failureSSM:    acc.failureSSM    + site.failureSSM,
    }),
    { totalCamera: 0, totalSSM: 0, failureCamera: 0, failureSSM: 0 }
  );

  return { mappedData, summary };
};


  //--------------------------------------------------


  const getDropdownOptions = (data: any) => {
    const options: any = [];

    data.forEach((parent: any) => {
      options.push({
        title: parent.siteName,
        id: parent.id,
        isParent: true,
        disabled: false, // Optional: disable parent selection if needed
      });

      parent.childSites.forEach((child: any) => {
        options.push({
          title: `${child.siteName}`,
          id: child.id,
          parentId: parent.id,
          isParent: false,
        });
      });
    });

    return options;
  };


  const exportHealthReport = (
    siteIds: string[],
    startDate: Dayjs | null,
    endDate: Dayjs | null
  ) => {

     const siteNames = siteIds
    .map((id) => siteList.find((s) => s.id === id)?.title)
    .filter(Boolean) as string[];
    const data = {
      siteList: siteIds,
      siteNames: siteNames,   
      startDateUtc: startDate
        ? startDate.toISOString()
        : null,
      endDateUtc: endDate
        ? endDate.toISOString()
        : null,
    };

    const url = `/healthreport-pdf?data=${encodeURIComponent(
      JSON.stringify(data)
    )}`;

    window.open(url, "_blank");
  };

  const onSearch = async (data: HeaderInputs) => {
    const utcDate = data.startDate
      ? convertToUTC(dayjs(data.startDate).format("YYYY-MM-DDTHH:mm:ss"))
      : convertToUTC(dayjs().startOf("day").format("YYYY-MM-DDTHH:mm:ss"));
      
      setSelectedDate(utcDate)
   await fetchData(data.siteIds, utcDate);
  
  };

  const fetchData = async (siteIds: string[], date: string | null) => {
  try {
      const sitedatapayload = {
        parentSiteIds: siteIds,
        date: date,
      };

      const response: any = await fetchSSMSiteDataService(
        sitedatapayload as ISSMPayload
      );
    
      if(response.isSuccess){
      const { mappedData, summary } = mapApiToSiteData(response.data as ApiSiteData[]);
      setSiteData(mappedData);
      setSummary(summary);
      }
      
    } catch (error) {
      console.error(
        "Error fetching in SSM site data:",
        error
      );
      throw error;
    } 

  }

 
  return (
    <>
       {/* ✅ SSMDevice Screen */}
    {currentView === "devices" && selectedServerForDevice && (
      <SSMDeviceScreen
        server={selectedServerForDevice}
        selectedDate={selectedDate}
        onBack={() => setCurrentView("dashboard")}   // ✅ back to dashboard
        
      />
    )}
       {/*  Dashboard — hidden via display:none, NOT unmounted */}
    <Box className="ssm-monitoring" sx={{ display: currentView === "dashboard" ? "block" : "none" }}>
      <Box className="dashbourd-retail-details-head">
        <div className="dashbourd-retail-details-floor">         
          <CustomStyleMultiSelect
            name="siteIds"
            control={control}
            label={
              <span>
                {t("Manage_SSM.Select_Site")} <span className="star-error">*</span>
              </span>
            }
            options={siteList}
            rules={{
              required: t("Manage_SSM.Site_required"),
             
            }}
            placeholder={t("Manage_SSM.Select_Site")}
            hideChildCheckbox =  {true}
          />
        </div>

        <div className="dashbourd-retail-date-picker">
          <LocalizationProvider dateAdapter={AdapterDayjs}>
            <Box>
              <FormLabel>
                <span>{t("RMA.RMA_Filter_StartDate")}</span>
              </FormLabel>
              <Controller
             
                name="startDate"
                control={control}
                render={({ field, fieldState }) => (
                  <DatePicker
                    maxDate={dayjs()}
                   className="test12212"
                    value={field.value || dayjs()}
                    onChange={(date) => field.onChange(date)}
                    slotProps={{
                      textField: {
                        error: !!fieldState.error,
                        helperText: fieldState.error?.message,
                      },
                      layout: {
                        sx: {
                          ".MuiPickersLayout-contentWrapper": {
                            alignItems: "center",
                          },
                          ".MuiPickersLayout-toolbar": {
                            display: "none",
                          },
                        },
                      },
                    }}
                    format="DD-MM-YYYY"
                  />
                )}
              />
            </Box>
          </LocalizationProvider>
        </div>

        <div className="dashbourd-retail-details-export margin-left-auto">
          <CustomButton variant="outlined"
           onClick={handleSubmit(onSearch)} 
          >
            <img src="images/search.svg" alt="" />
          </CustomButton>
        </div>

        <div className="dashbourd-retail-details-export ">
          <CustomButton
            variant="outlined"
            onClick={() => {
              const currentValues = watch(); //  get current form values
              const utcDate = currentValues.startDate
                ? convertToUTC(dayjs(currentValues.startDate).format("YYYY-MM-DD"))
                : null;
              fetchData(currentValues.siteIds, selectedDate);
              // fetchData(currentValues.siteIds, utcDate);
            }} 
          >
            <img src={"/images/reload.svg"} alt="Reload SSM Server" />
            {/* {t("Manage_Device.Reload_btn")} */}
          </CustomButton>
        </div>
          {HasPermission(LABELS.ViewSsmReport) && (
            <Tooltip title={t("SSM_Dashboard.health_report")}>
            <div className="dashbourd-retail-details-export">
              <CustomButton
                variant="outlined"
                onClick={() => {
                  setSelectedStartDate(dayjs().startOf("day"));
                  setSelectedEndDate(dayjs().endOf("day"));
                  setOpenExportDialog(true);
                }}
              >
                <img src="images/directbox-notif.svg" alt="" />
              </CustomButton>
            </div>
            </Tooltip>
          )}
         
      </Box>

      {siteData.length > 0 ?
      (
      <div className="ssm-monitoring-main">
        {/* <div className="top-list-bar">
          <Typography variant="h5" gutterBottom>
            {t("SSM_Dashboard.List_Of_SSM")}
          </Typography>      
        </div> */}

        <Box 
          className="top-listbar-main-box"
        >
          {/* Total Camera */}
          <Box className="list-of-ccm-box">
            <Box className="ccm-list-image-box">
              <img src="/images/SSM/Total_Camera.svg" alt="Total Camera" width={36} height={36} />
            </Box>
            <Box sx={{ marginLeft: "12px" }}>
              <Typography>
                 {t("SSM_Dashboard.Total_Camera")}
              </Typography>
              <Typography  variant="h5" >
                {summary.totalCamera.toLocaleString()}
              </Typography>
            </Box>
          </Box>

          {/* Total SSM */}
          <Box className="list-of-ccm-box"
            
          >
            <Box className="ccm-list-image-box">
              <img src="/images/SSM/Total_SSM.svg" alt="Total SSM" width={36} height={36} />
            </Box>
            <Box sx={{ marginLeft: "12px" }}>
              <Typography >
                 {t("SSM_Dashboard.Total_SSM")}
              </Typography>
              <Typography variant="h5">
              {summary.totalSSM.toLocaleString()}
              </Typography>
            </Box>
          </Box>

          {/* Failure Camera */}
          <Box className="list-of-ccm-box"
            
          >
            <Box className="ccm-list-image-box">
              <img src="/images/SSM/Fail_Camera.svg" alt="Failure Camera" width={36} height={36} />
            </Box>
            <Box sx={{ marginLeft: "12px" }}>
              <Typography >
                 {t("SSM_Dashboard.Failure_Camera_Count")}
              </Typography>
              <Typography variant="h5">
                {summary.failureCamera.toLocaleString()}
              </Typography>
            </Box>
          </Box>

          {/* Failure SSM */}
          <Box className="list-of-ccm-box"
            >
            <Box className="ccm-list-image-box">
              <img src="/images/SSM/Fail_SSM.svg" alt="Failure SSM" width={36} height={36} />
            </Box>
            <Box sx={{ marginLeft: "12px" }}>
              <Typography>
                 {t("SSM_Dashboard.Failure_SSM_Count")}
              </Typography>
              <Typography variant="h5">
                {summary.failureSSM.toLocaleString()}
              </Typography>
            </Box>
          </Box>
        </Box>

        <div className="top-list-bar">
          <Typography variant="h5" gutterBottom>
            {t("SSM_Dashboard.List_Of_Site")}
          </Typography>
        </div>

        <Box className="top-list-main"  sx={{ marginBottom: 5 }}>

          {siteData.map((site) => (
            <SiteAccordion
              key={site.siteID}
              site={site}
              setSelectedServer={setSelectedServer}
              setOpenDialog={setOpenDialog}
              onServerCardClick={(srv) => {  
                scrollRef.current = window.scrollY;             
                setSelectedServerForDevice(srv);
                setCurrentView("devices");
              }}      
              getServerIcon={getServerIcon}                   
              getUsageColor={getUsageColor}   
                        
            />
          ))}
          
        </Box>
      </div>
      )
      :(
       <Box className="no-data-douns">
            <Box sx={{ width: 200, justifyItems: "center", flex: 1, }}>
              <img src={`/images/${themeColorPath}noData.gif`} alt="Animated GIF" width="100" height="100" />
              <Typography
                sx={{ FontWeight: 600, fontSize: 24, color: "#090909" }}
              >
                {t("No_data_found")}
              </Typography>
              {/* <Typography
                sx={{ FontWeight: 400, fontSize: 12, color: "#212121" }}
              >
               No data available to display SSM server. Please search for site and date <strong>Add New User</strong> button.
              </Typography> */}
            </Box>
       </Box>
      )
      }
        {openDialog && selectedServer && (
          <SSMServerChartDialog
            open={openDialog}
            onClose={() => setOpenDialog(false)}
            server={selectedServer}
            selectedDate={selectedDate}
          />
        )}

        <Dialog
          open={openExportDialog}
          maxWidth="md"
          fullWidth
          disableEscapeKeyDown
          onClose={(_, reason) => {
            if (reason === "backdropClick") return;

            setOpenExportDialog(false);

            setSelectedStartDate(dayjs().startOf("day"));
            setSelectedEndDate(dayjs().endOf("day"));
          }}
          
          className="filter-pop-up-design health-report-pops"
        >
          <DialogTitle><Box display="flex" justifyContent="space-between" alignItems="center">
                    <Typography variant="h6">Export Health Report</Typography></Box></DialogTitle>

          <DialogContent>
            <Box   className="health-report-date-picker">
              <div className="dashbourd-retail-details-date">
                <CustomDateTimeRangePicker
                  key={openExportDialog ? "range-open" : "range-closed"}
                  initialStartDate={selectedStartDate?.toDate()}
                  initialEndDate={selectedEndDate?.toDate()}
                  onApply={({ startDate, endDate }) => {
                    if (!startDate || !endDate) return;
                    setSelectedStartDate(dayjs(startDate));
                    setSelectedEndDate(dayjs(endDate));
                  }}
                  label="Select Date Range"
                  rules={{ required: "Date is required" }}
                  disableFuture = {true}
                />
                <img src="images/calendar.png" alt="calendar-icon" />
              </div>
            </Box>
          </DialogContent>

          <DialogActions  className="advance-report-buttons-wrapper">
            <Button
              className="common-btn-design-transparent common-btn-design"
              onClick={() => {
                setOpenExportDialog(false);
                setSelectedStartDate(dayjs().startOf("day"));
                setSelectedEndDate(dayjs().endOf("day"));
              }}
            >
              Cancel
            </Button>
            <Button
              className="common-btn-design"
              onClick={() => {
                const currentValues = watch();

                if (!currentValues.siteIds?.length) {
                  showToast("Select at least one site for export.","error");
                  return;
                }

                if (!selectedStartDate || !selectedEndDate) {
                  showToast("Select date range","error");
                  return;
                }


                exportHealthReport(
                  currentValues.siteIds,
                  selectedStartDate,
                  selectedEndDate
                );

                setOpenExportDialog(false);

                setTimeout(() => {
                  setSelectedStartDate(dayjs().startOf("day"));
                  setSelectedEndDate(dayjs().endOf("day"));
                }, 100);
              }}
            >
              Export PDF
            </Button>
          </DialogActions>
        </Dialog>
         

        </Box>
    </>
  )
}

export default SSMDashboard

//  OUTSIDE SSMDashboard — above the component
const SiteAccordion = ({
  site,
  setSelectedServer,
  setOpenDialog,
  onServerCardClick,    // ✅ replace two props with one callback
  getServerIcon,
  getUsageColor,
}: {
  site: any;
  setSelectedServer: (s: any) => void;
  setOpenDialog: (v: boolean) => void;
  onServerCardClick: (server: { id: string; name: string }) => void;  // ✅
  getServerIcon: (status: string | number) => string;
  getUsageColor: (v: number) => string;
}) => {
   const { t } = useTranslation();
  return (
    <Accordion
      className="list-main-accordion"
      sx={{
        border: "1px solid #ddd",
        marginBottom: 2,
        boxShadow: "none",
        overflow: "hidden",
        padding: "12px",
        "&.MuiAccordion-root": { borderRadius: 3 },
      }}
    >
      <AccordionSummary
        expandIcon={<ExpandMoreIcon />}
        className="list-main-accordion-repeat-main"
        sx={{
          transition: "all 0.3s ease",
          borderRadius: "12px",
          backgroundColor: "#fff",
          "& .MuiAccordionSummary-content": { margin: "0px 0" },
          "& .MuiAccordionSummary-expandIconWrapper": { marginRight: "8px" },
          "&.Mui-expanded": {
            backgroundColor: "#FFF6EC",
            border: "1px solid #ddd",
            boxShadow: "0 2px 8px rgba(0,0,0,0.05)",
          },
          "&.Mui-expanded .MuiAccordionSummary-content": { margin: "0px 0" },
        }}
      >
        <Box className="list-main-accordion-repeat">
          <Typography variant="h4" className="list-title-here">
            {site.siteName}
          </Typography>
          <Divider orientation="vertical" flexItem sx={{ mx: 2 }} />
          <Box className="list-main-accordion-box">
            <Box className="list-of-site-icon"><img src="/images/SSM/Total_Camera.svg" width={28} /></Box>
            <Box>
              <Typography variant="h5">{t("SSM_Dashboard.Total_Camera")}</Typography>
              <Typography>{site.totalCamera}</Typography>
            </Box>
          </Box>
          <Divider orientation="vertical" flexItem sx={{ mx: 2 }} />
          <Box className="list-main-accordion-box" >
            <Box className="list-of-site-icon"><img src="/images/SSM/Total_SSM.svg" width={28} /></Box>
            <Box>
              <Typography variant="h5">{t("SSM_Dashboard.Total_SSM")}</Typography>
              <Typography>{site.totalSSM}</Typography>
            </Box>
          </Box>
          <Divider orientation="vertical" flexItem sx={{ mx: 2 }} />
          <Box className="list-main-accordion-box" >
            <Box className="list-of-site-icon"><img src="/images/SSM/Fail_Camera.svg" width={28} /></Box>
            <Box>
              <Typography variant="h5">{t("SSM_Dashboard.Failure_Camera")}</Typography>
              <Typography>{site.failureCamera}</Typography>
            </Box>
          </Box>
          <Divider orientation="vertical" flexItem sx={{ mx: 2 }} />
          <Box  className="list-main-accordion-box" >
            <Box className="list-of-site-icon"><img src="/images/SSM/Fail_SSM.svg" width={28} /></Box>
            <Box>
              <Typography variant="h5">{t("SSM_Dashboard.Failure_SSM")}</Typography>
              <Typography>{site.failureSSM}</Typography>
            </Box>
          </Box>
        </Box>
      </AccordionSummary>

      <AccordionDetails className="servers-data-main">
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
          {site.servers?.length > 0 && (
            <Box sx={{ display: "flex", gap: 3, flexWrap: "wrap" }}>
              {site.servers.map((srv: any) => (
                <ServerDetails
                  key={srv.id}
                  server={srv}
                  setSelectedServer={setSelectedServer}
                  setOpenDialog={setOpenDialog}
                  onServerCardClick={onServerCardClick}  
                  getServerIcon={getServerIcon}
                  getUsageColor={getUsageColor}
                />
              ))}
            </Box>
          )}
          {site.subsite?.length > 0 && (
            <Box sx={{ marginLeft: 2 }}>
              {site.subsite.map((sub: any, i: number) => (
                <SiteAccordion
                  key={sub.siteID ?? i}
                  site={sub}
                  setSelectedServer={setSelectedServer}
                  setOpenDialog={setOpenDialog}
                  onServerCardClick={onServerCardClick}  
                  getServerIcon={getServerIcon}
                  getUsageColor={getUsageColor}
                />
              ))}
            </Box>
          )}
        </Box>
      </AccordionDetails>
    </Accordion>
  );
};


// ✅ OUTSIDE SSMDashboard
const ServerDetails = ({
  server,
  setSelectedServer,
  setOpenDialog,
  onServerCardClick,    // ✅ single callback replaces setOpenDeviceDialog + setServerDeviceData
  getServerIcon,
  getUsageColor,
}: {
  server: any;
  setSelectedServer: (s: any) => void;
  setOpenDialog: (v: boolean) => void;
  onServerCardClick: (server: { id: string; name: string }) => void;  // ✅
  getServerIcon: (status: string | number) => string;
  getUsageColor: (v: number) => string;
}) => {
  if (!server) return null;
   const { t } = useTranslation();

  return (
    <Box
      className="list-box-repeat"
      list-box-repeat
      sx={{ cursor: "default"}}
    >
      <Box className="list-box-top-head">
        <Box className="online-offline-i"><img src={getServerIcon(server.status)}/></Box>
        <Box className="list-box-top-head-details">
          <Typography variant="h5">{server.name}</Typography>
          <Typography fontSize={13} color="#666">{server.ip}</Typography>
        </Box>
        <IconButton
          onClick={(e) => {
            e.stopPropagation(); // ✅ prevent card click
            setSelectedServer(server);
            setOpenDialog(true);
          }}
        >
          <img src="/images/SSM/chart-icon.svg" width={30} />
        </IconButton>
      </Box>

      <Divider sx={{ backgroundColor: "#DFDFDF" }} />

      <Box  className="list-box-listing">
        <Box className="list-box-listing-left" onClick={(e) => {
            e.stopPropagation();
            onServerCardClick({ id: server.id, name: server.name });
          }}
        >
          <Stat label={t("SSM_Dashboard.Cameras")} value={server.cameras} />
          <Stat label={t("SSM_Dashboard.Online_Offline")} value={`${server.online}/${server.offline}`} />
        </Box>
        <Box className="list-box-listing-right">
          <Stat label={t("SSM_Dashboard.CPU")} value={`${server.cpu}%`} />
          <Stat label={t("SSM_Dashboard.RAM")} value={`${server.ram}%`} />
        </Box>
      </Box>
      <Box  className="list-box-data-inner">
        <Box  className="list-box-data">
          {/* LEFT — Total Storage Circle */}
          <Box >
            <Typography className="total-storege-tilte">{t("SSM_Dashboard.Total_Storage")}</Typography>
            <Box position="relative" display="inline-flex">
              <CircularProgress variant="determinate" value={100} size={130} thickness={4} sx={{ color: "#E0E0E0" }} />
              <CircularProgress
                variant="determinate"
                value={server.totalStorage}
                size={130}
                thickness={4}
                sx={{
                  position: "absolute",
                  left: 0,
                  color: getUsageColor(server.totalStorage),
                  "& .MuiCircularProgress-circle": { strokeLinecap: "round" },
                }}
              />
              <Box position="absolute" top="50%" left="50%" sx={{ transform: "translate(-50%, -50%)", textAlign: "center" }}>
                <Typography fontSize={12} color="text.secondary">{t("SSM_Dashboard.Used")}</Typography>
                <Typography fontWeight={700} fontSize={18}>{server.totalStorage}%</Typography>
              </Box>
            </Box>
          </Box>

          {/* RIGHT — Drive List */}
          <Box
            flex={1}
            sx={{
              maxHeight: 180,
              overflowY: "auto",
              pr: 1,
              "&::-webkit-scrollbar": { width: 4 },
              "&::-webkit-scrollbar-thumb": { backgroundColor: "#ccc", borderRadius: 10 },
            }}
          >
            {server.drive?.map((d: any, i: number) => {
              const percent = Math.round((d.used / d.totalCapacity) * 100);
              return (
                <Box className="total-storage-right-repeat" key={i} mb={2} sx={{ display: "flex", alignItems: "center", gap: 2 }}>
                  <Box><img src="/images/SSM/drive.svg" style={{ width: 25, height: 25 }} alt="drive" /></Box>
                  <Box className="total-storage-details">
                    <Typography fontWeight={600} fontSize={15}>{d.name}</Typography>
                    <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                      <Typography fontSize={13} color="text.secondary" mt={0.5}>{d.used} GB / {d.totalCapacity} GB</Typography>
                      <Typography fontSize={13} color="text.secondary" mt={0.5}>{percent}% {t("SSM_Dashboard.Used")}</Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={percent}
                      sx={{
                        height: 5, borderRadius: 5, mt: 1,
                        backgroundColor: "#E0E0E0",
                        "& .MuiLinearProgress-bar": { borderRadius: 5, backgroundColor: getUsageColor(percent) },
                      }}
                    />
                  </Box>
                </Box>
              );
            })}
          </Box>
        </Box>
      </Box>
    </Box>
  );
};

// ✅ OUTSIDE SSMDashboard
const Stat = ({ label, value }: { label: string; value: any }) => (
  <Box className="list-box-listing-wrapper">
    <Typography>{label}</Typography>
    <Typography>{value}</Typography>
  </Box>
);