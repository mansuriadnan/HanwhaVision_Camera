import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useThemeContext } from "../../context/ThemeContext";
import { usePermissions } from "../../context/PermissionsContext";
import {
  COMMON_CONSTANTS,
  LABELS,
  SIDEBAR_TITLES,
} from "../../utils/constants";
import { RiArrowUpSLine, RiArrowDownSLine } from "react-icons/ri";
// import { GetClientLogo } from "../../services/settingService";
import {
  IDashboardNamePayload,
  IMonitoringNamePayload,
} from "../../interfaces/IChart";
import {
  DeleteDashboardService,
  GetDashboardDesign,
  SaveDashboardName,
} from "../../services/dashboardService";
import { Alert, Box, IconButton, ListItemText, Menu, MenuItem, Snackbar } from "@mui/material";
import { CustomTextFieldWithButton } from "../Reusable/CustomTextFieldWithButton";
import { useForm } from "react-hook-form";
import {
  DeleteMonitoringService,
  GetMonitoringDesign,
  SaveMonitoringName,
} from "../../services/monitoringService";
import { MoreVert } from "@mui/icons-material";
import { CommonDialog } from "../Reusable/CommonDialog";
import { HasPermission } from "../../utils/screenAccessUtils";
import SpeechDashboard from "../SpeechDashboard";
import { useTranslation } from "react-i18next";
import { IdeleteMonitoring } from "../../interfaces/IMonitoring";
import MenuIcon from "@mui/icons-material/Menu";
import { GetAppMainLogo } from "../../services/settingService";
import { IdracgetLiveData, onReceiveMessage } from "../../utils/signalRService";
import { useSignalRConnectionIDContext } from "../../context/SignalRConnectionIDContext";

type SidebarProps = {
  isSidebarOpen: boolean;
  toggleButton: () => void;
  handleSidebar:(state:boolean)=>void;
};

const Sidebar: React.FC<SidebarProps> = ({ isSidebarOpen, toggleButton,handleSidebar }) => {
  const navigate = useNavigate();
  const {
    control: dashboardControl,
    handleSubmit: handleDashboardSubmit,
    reset: resetDashboard,
  } = useForm<IDashboardNamePayload>({
    defaultValues: {
      dashboardName: "",
    },
  });
  const { t, i18n } = useTranslation();

  const {
    control: dashboardEditControl,
    handleSubmit: handleDashboardEditSubmit,
    // setValue: setEditDashboardValue,
    reset: resetEditDashboardForm,
  } = useForm<IDashboardNamePayload>({
    defaultValues: {
      dashboardName: "",
    },
  }); // for Edit Dashboard

  const {
    control: monitoringEditControl,
    handleSubmit: handleMonitoringEditSubmit,
    reset: resetEditMonitoringForm,
  } = useForm<IMonitoringNamePayload>({
    defaultValues: {
      monitoringName: "",
    },
  }); // for Edit Monitoring

  const {
    control: monitoringControl,
    handleSubmit: handleMonitoringSubmit,
    reset: resetMonitoring,
  } = useForm<IMonitoringNamePayload>({
    defaultValues: {
      monitoringName: "",
    },
  }); // for Add Monitoring

  const { theme } = useThemeContext();
  const { permissions } = usePermissions();
  const { ConnectionID } = useSignalRConnectionIDContext();

  const [clientLogo, setClientLogo] = useState<string>("");
  const [openMenus, setOpenMenus] = useState(() => {
    const storedMenus = localStorage.getItem("openMenus");
    return storedMenus ? JSON.parse(storedMenus) : {};
  });

  const [activeRoute, setActiveRoute] = useState(() => {
    return localStorage.getItem("activeRoute") || window.location.pathname;
  });

  const [customDashboards, setCustomDashboards] = useState([]);
  const [customMonitoring, setCustomMonitoring] = useState([]);
  //const [newDashboardName, setNewDashboardName] = useState("");
  const [isAdding, setIsAdding] = useState(false);
  const [isAddingMonitoring, setIsAddingMonitoring] = useState(false);
  const [menuAnchor, setMenuAnchor] = useState({
    anchorEl: null,
    dashboardId: null,
    title: null,
  });
  const [editingItem, setEditingItem] = useState({
    id: null,
    title: "",
    type: null,
  });
  const [deletingItem, setDeletingItem] = useState({
    id: null,
    title: "",
    type: null,
  });
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [toastList, setToastList] = useState<any[]>([]);
  const isANPR = localStorage.getItem("isANPR") === "true";
  const isMaintenance = localStorage.getItem("isMaintenance") === "true";

  useEffect(() => {
    fetchAppMainLogo();
    fetchDashboardSubMenus();
    if (userRights(LABELS.View_List_of_Monitorings)) {
      fetchMonitoringSubMenus();
    }
    
  }, []);


  useEffect(() => {
    IdracgetLiveData("iDRAC_Group");

    const handleSignalRMessage = (data: any) => {
      const liveData = JSON.parse(data);
      const parsedData = Array.isArray(liveData)
        ? liveData
        : Array.isArray(liveData?.data)
          ? liveData.data
          : [liveData];

      parsedData.forEach((item: any, index: number) => {
        let severity: "info" | "warning" | "error" = "warning";
        switch (item.Severity) {
          case "Informational":
            severity = "info";
            break;

          case "Critical":
            severity = "error";
            break;

          case "Warning":
            severity = "warning";
            break;

          default:
            severity = "warning";
            break;
        }

        setToastList((prev) => [
          ...prev,
          {
            id: Date.now() + index,
            message: item.Message,
            duration: (item.TimeLimit || 5) * 1000,
            severity: severity,
            sourceIP: item.SourceIP ? item.SourceIP : ""
          },
        ]);
      });
    };
    onReceiveMessage("idracEventNotification", handleSignalRMessage);

  }, [ConnectionID]);


  useEffect(() => {
    if (window.location.pathname === "/login") {
      localStorage.clear();
    } else {
      // setActiveRoute(window.location.pathname);
      // localStorage.setItem("activeRoute", window.location.pathname);

      const fullRoute =
        window.location.pathname + (window.location.search || "");
      setActiveRoute(fullRoute);
      localStorage.setItem("activeRoute", fullRoute);
    }
  }, [window.location.pathname]);

  useEffect(() => {
    if (editingItem && editingItem.type === "dashboard") {
      resetEditDashboardForm({
        dashboardName: editingItem.title || "", // set the name to edit
      });
    }
  }, [editingItem, resetEditDashboardForm]);

  useEffect(() => {
    if (editingItem && editingItem.type === "monitoring") {
      resetEditMonitoringForm({
        monitoringName: editingItem.title || "", // set the name to edit
      });
    }
  }, [editingItem, resetEditMonitoringForm]);

  // const fetchClientLogo = async () => {
  //   try {
  //     const response = await GetClientLogo();
  //     if (response !== null) {
  //       setClientLogo(response as string);
  //     }
  //   } catch (error) {
  //     console.error("Errror while fetching the logo", error);
  //   }
  // };
  const fetchAppMainLogo = async () => {
    try {
      const response: any = await GetAppMainLogo();
      if (response != undefined) {
        setClientLogo(response);
      }
    } catch (err) {
      console.error("Error fetching client settings", err);
    }
  };

  const fetchDashboardSubMenus = async () => {
    try {
      const response: any = await GetDashboardDesign();
      if (Array.isArray(response)) {
        const formattedDashboards = response.map((dashboard, index) => ({
          title: dashboard.dashboardName, // Assuming API returns `dashboardName`
          onClick: () =>{
            handleSubmenuClick(
              `/dashboard?id=${dashboard.id}`,
              SIDEBAR_TITLES.Dashboards,
              dashboard.dashboardName,
            );
            handleSidebar(false);
          },
          route: `/dashboard?id=${dashboard.id}`,
        }));
        setCustomDashboards(formattedDashboards);

        // Reset active route if current one is missing (After delete selected dashboard)
        const storedRoute = localStorage.getItem("activeRoute");

        if (
          storedRoute &&
          storedRoute.startsWith("/dashboard") && //added this condition for resolved the issue :with fresh database, when press f5 then navigate to welcome from any route
          !formattedDashboards.some((d) => d.route === storedRoute)
        ) {
          setActiveRoute(null);
          localStorage.removeItem("activeRoute");
          navigate("/welcome");
          // }
        }
        //end of code
      }
    } catch (error) {
      console.error("Errror while fetching dashboard sub menus", error);
    }
  };

  const fetchMonitoringSubMenus = async () => {
    try {
      const response: any = await GetMonitoringDesign();
      if (Array.isArray(response)) {
        if (response.length > 0) {
          const formattedMonitoringMenu = response.map((monitoring, index) => ({
            title: monitoring.monitoringName,
            onClick: () =>{
              handleSubmenuClick(
                `/monitoring?id=${monitoring.id}`,
                SIDEBAR_TITLES.Monitoring,
                monitoring.monitoringName,
              );
              handleSidebar(false);
            },
            route: `/monitoring?id=${monitoring.id}`,
          }));
          setCustomMonitoring(formattedMonitoringMenu);
        } else {
          setCustomMonitoring([]);
          // navigate("/welcome"); // resolved the issue :with fresh database, when press f5 then navigate to welcome from any route
        }
      }
    } catch (error) {
      console.error("Errror while fetching Monitoring sub menus", error);
    }
  };

  const userRights = (screen_name) => {
    let filtered = permissions.filter(
      (item: any) => item.screenName === screen_name,
    );
    return filtered && filtered.length > 0 ? true : false;
  };

  const handleSubmenuClick = (route, sidebarMenuName, screenName) => {
    setActiveRoute(route);
    localStorage.setItem("activeRoute", route);
    navigate(route, {
      state: { sidebarMenuName, screenName },
    });
  };

  const addNewDashboard = async (data: any) => {
    if (data.dashboardName.trim() === "") return;

    try {
      const newDashboardPayload: IDashboardNamePayload = {
        dashboardName: data.dashboardName.trim(),
      };
      const response: any = await SaveDashboardName(newDashboardPayload);
      if (response?.isSuccess) {
        const newDashboard = {
          title: data.dashboardName.trim(),
          onClick: () =>{
            handleSubmenuClick(
              `/dashboard?id=${response.data}`,
              SIDEBAR_TITLES.Dashboards,
              data.dashboardName.trim(),
            );
            handleSidebar(false);
          },
          route: `/dashboard?id=${response.data}`,
        };

        setCustomDashboards((prevDashboards) => [
          ...prevDashboards,
          newDashboard,
        ]);
      }
    } catch (error) {
      console.error(`Error while saving the new dashboard name: `, error);
    } finally {
      //setNewDashboardName("");
      resetDashboard();
      setIsAdding(false);
    }
  };

  const editDashboard = async (data: any) => {
    const path = typeof editingItem.id === "string" ? editingItem.id : "";
    const id = path.match(/id=([^&]+)/)?.[1] || "";

    try {
      const payload = {
        id: id,
        dashboardDesignjson: "",
        dashboardName: data.dashboardName,
      };
      const response: any = await SaveDashboardName(payload);
      if (response?.isSuccess) {
        fetchDashboardSubMenus();
        resetDashboard();
        setEditingItem({ id: null, title: "", type: null });
        resetEditDashboardForm(); // Reset after save
      }
    } catch (error) {
      console.error(`Error while saving the Edit dashboard name: `, error);
    } finally {
      //setNewDashboardName("");
      resetDashboard();
      setIsAdding(false);
    }
  };

  const editMonitoring = async (data: any) => {
    const path = typeof editingItem.id === "string" ? editingItem.id : "";
    const id = path.match(/id=([^&]+)/)?.[1] || "";

    try {
      const payload = {
        monitoringId: id,
        monitoringName: data.monitoringName,
      };
      const response: any = await SaveMonitoringName(payload);
      if (response?.isSuccess) {
        fetchMonitoringSubMenus();
        resetMonitoring();
        setEditingItem({ id: null, title: "", type: null });
        resetEditMonitoringForm(); // Reset after save
      }
    } catch (error) {
      console.error(`Error while saving the Edit Monitoring name: `, error);
    } finally {
      resetDashboard();
      setIsAdding(false);
    }
  };

  const addNewMonitoring = async (data: IMonitoringNamePayload) => {
    if (data.monitoringName.trim() === "") return;

    try {
      const newMonitoringPayload: IMonitoringNamePayload = {
        monitoringName: data.monitoringName.trim(),
      };
      const response: any = await SaveMonitoringName(newMonitoringPayload);
      if (response?.isSuccess) {
        const newMonitoring = {
          title: data.monitoringName.trim(),
          onClick: () =>{
            handleSubmenuClick(
              `/monitoring?id=${response.data}`,
              SIDEBAR_TITLES.Monitoring,
              data.monitoringName.trim(),
            );
            handleSidebar(false);
          },
          route: `/monitoring?id=${response.data}`,
        };

        setCustomMonitoring((prevMonitoring) => [
          ...prevMonitoring,
          newMonitoring,
        ]);
      }
    } catch (error) {
      console.error(`Error while saving the new Monitoring name: `, error);
    } finally {
      //setNewDashboardName("");
      resetMonitoring();
      setIsAddingMonitoring(false);
    }
  };

  const handleCloseConfirm = () => {
    setIsConfirmOpen(false);
    setDeletingItem({ id: null, title: "", type: null });
  };

  const handleDeleteSubMenu = async (menu) => {
    const path = menu.id;
    const id = path.match(/id=([^&]+)/)?.[1] || null;
    if (menu.type === "dashboard") {
      try {
        const deleteData: any = await DeleteDashboardService({ id: id });
        if (deleteData?.isSuccess) {
          fetchDashboardSubMenus();
        }
      } catch (err: any) {}
    } else if (menu.type === "monitoring") {
      const params = {
        monitoringId: id,
      };
      try {
        const deleteData: any = await DeleteMonitoringService(
          params as IdeleteMonitoring,
        );
        if (deleteData.isSuccess) {
          fetchMonitoringSubMenus();
        }
      } catch (err: any) {}
    }
    setIsConfirmOpen(false);
    setDeletingItem({ id: null, title: "", type: null });
  };

  const dashboardSubMenuList = [
    // {
    //   title: SIDEBAR_TITLES.Retail,
    //   onClick: () =>
    //     handleSubmenuClick(
    //       "/dashboard/retail",
    //       SIDEBAR_TITLES.Dashboards,
    //       SIDEBAR_TITLES.Retail
    //     ),
    //   route: "/dashboard/retail",
    // },
    // {
    //   title: SIDEBAR_TITLES.Traffic,
    //   onClick: () =>
    //     handleSubmenuClick(
    //       "/dashboard/traffic",
    //       SIDEBAR_TITLES.Dashboards,
    //       SIDEBAR_TITLES.Traffic
    //     ),
    //   route: "/dashboard/traffic",
    // },
    ...customDashboards,
  ];

  const monitoringSubMenuList = [
    // {
    //   title: "newMonitoring",
    //   onClick: () =>
    //     handleSubmenuClick(
    //       "/monitoring/newMonitoring",
    //       SIDEBAR_TITLES.Monitoring,
    //       "newMonitoring"
    //     ),
    //   route: "/monitoring/newMonitoring",
    // },
    ...customMonitoring,
  ];
  const menuItems_New = [
    {
      title: t(SIDEBAR_TITLES.Dashboards),
      icon: (
        <img
          src={"/images/sidebar_dashboard_logo.svg"}
          alt="Sidebar Dashboard Icon"
          width={25}
          height={25}
          onClick={()=> handleSidebar(true)}
        />
      ),
      submenu: [...dashboardSubMenuList],
    },

    userRights(LABELS.View_List_of_Devices) ||
    userRights(LABELS.View_List_of_Floors)
      ? {
          title: t(SIDEBAR_TITLES.Configurations),
          icon: (
            <img
              src={"/images/sidebar_configurations_logo.svg"}
              alt="Sidebar Configurations Icon"
              width={25}
              height={25}
              onClick={()=> handleSidebar(true)}
            />
          ),
          submenu: [
            userRights(LABELS.View_List_of_Devices)
              ? {
                  title: t(SIDEBAR_TITLES.ManageDevice),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/manage-devices",
                      SIDEBAR_TITLES.Configurations,
                      SIDEBAR_TITLES.ManageDevice,
                    );
                    handleSidebar(false);
                  },
                  route: "/manage-devices",
                }
              : null,
            userRights(LABELS.View_List_of_Floors)
              ? {
                  title: t(SIDEBAR_TITLES.FloorPlansAndZones),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/floorplans-and-zones",
                      SIDEBAR_TITLES.Configurations,
                      SIDEBAR_TITLES.FloorPlansAndZones,
                    );
                    handleSidebar(false);
                  },
                  route: "/floorplans-and-zones",
                }
              : null,
          ],
        }
      : null,

    userRights(LABELS.View_List_of_Events)
      ? {
          title: t(SIDEBAR_TITLES.Events),
          icon: (
            <img
              src={"/images/sidebar_events_logo.svg"}
              alt="Sidebar Events Icon"
              width={25}
              height={25}
              onClick={()=> handleSidebar(true)}
            />
          ),
          submenu: [
            userRights(LABELS.View_List_of_Events)
              ? {
                  title: t(SIDEBAR_TITLES.EventLogs),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/manage-eventlogs",
                      SIDEBAR_TITLES.Events,
                      SIDEBAR_TITLES.EventLogs,
                    );
                    handleSidebar(false);
                  },
                  route: "/manage-eventlogs",
                }
              : null,
          ],
        }
      : null,

    userRights(LABELS.View_List_of_Reports)
      ? {
          title: t(SIDEBAR_TITLES.Reports),
          icon: (
            <img
              src={"/images/sidebar_reports_logo.svg"}
              alt="Sidebar Reports Icon"
              width={25}
              height={25}
              onClick={()=> handleSidebar(true)}
            />
          ),
          submenu: [
            userRights(LABELS.View_List_of_Reports)
              ? {
                  title: t(SIDEBAR_TITLES.MyReports),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/reports",
                      SIDEBAR_TITLES.Reports,
                      SIDEBAR_TITLES.MyReports,
                    );
                    handleSidebar(false);
                  },
                  route: "/reports",
                }
              : null,
          ],
        }
      : null,

    userRights(LABELS.View_List_of_Monitorings)
      ? {
          title: t(SIDEBAR_TITLES.Monitoring),
          icon: (
            <img
              src={"/images/sidebar_monitoring_logo.svg"}
              alt="Sidebar Events Icon"
              width={25}
              height={25}
              onClick={()=> handleSidebar(true)}
            />
          ),

          submenu: [...monitoringSubMenuList],
        }
      : null,  

    (userRights(LABELS.View_List_of_Vehicle_Owners) ||
      userRights(LABELS.ViewLPR)) &&
    isANPR
      ? {
          title: t(SIDEBAR_TITLES.ANPR),
          icon: (
            <img
              src={"/images/sidebar_anpr_logo.svg"}
              alt="Sidebar Configurations Icon"
              width={25}
              height={25}
              onClick={()=> handleSidebar(true)}
            />
          ),
          submenu: [
            userRights(LABELS.View_List_of_Vehicle_Owners) && isANPR
              ? {
                  title: t(SIDEBAR_TITLES.VehicleOwners),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/owners",
                      t(SIDEBAR_TITLES.ANPR),
                      t(SIDEBAR_TITLES.VehicleOwners),
                    );
                    handleSidebar(false);
                  },
                  route: "/owners",
                }
              : null,
            userRights(LABELS.ViewLPR) && isANPR
              ? {
                  title: t(SIDEBAR_TITLES.LPR),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/lpr",
                      t(SIDEBAR_TITLES.ANPR),
                      t(SIDEBAR_TITLES.LPR),
                    );
                    handleSidebar(false);
                  },
                  route: "/lpr",
                }
              : null,
          ],
        }
      : null,

      userRights(LABELS.ViewMaintenancePlan) ||
      userRights(LABELS.ViewMaintenanceSchedule) ||
      userRights(LABELS.ViewRMA)
     && isMaintenance
      ? {
          title: t(SIDEBAR_TITLES.Maintenance),
          icon: (
            <img
              src={"/images/sidebar_maintenance_logo.svg"}
              alt="Sidebar Configurations Icon"
              width={25}
              height={25}
              onClick={()=> handleSidebar(true)}
            />
          ),
          submenu: [
            userRights(LABELS.ViewMaintenancePlan) && isMaintenance
              ? {
                  title: t(SIDEBAR_TITLES.Maintenance_Plan),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/maintenance-plan",
                      SIDEBAR_TITLES.Maintenance,
                      SIDEBAR_TITLES.Maintenance_Plan,
                    );
                    handleSidebar(false);
                  },
                  route: "/maintenance-plan",
                }
              : null,
            userRights(LABELS.ViewMaintenanceSchedule) && isMaintenance
              ? {
                  title: t(SIDEBAR_TITLES.Maintenance_Schedule),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/maintenance-schedule",
                      SIDEBAR_TITLES.Maintenance,
                      SIDEBAR_TITLES.Maintenance_Schedule,
                    );
                    handleSidebar(false);
                  },
                  route: "/maintenance-schedule",
                }
              : null,
            userRights(LABELS.ViewRMA) && isMaintenance
              ? {
                  title: t(SIDEBAR_TITLES.RMA),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/rma",
                      SIDEBAR_TITLES.Maintenance,
                      SIDEBAR_TITLES.RMA,
                    );
                    handleSidebar(false);
                  },
                  route: "/rma",
                }
              : null,
          ],
        }
      : null,

    userRights(LABELS.ViewSsmDashboard) ||
    userRights(LABELS.ViewListofSsmServers)
      ? {
          title: t(SIDEBAR_TITLES.SSMMonitoring),
        icon: (
          <img
            src={"/images/sidebar_dashboard_logo.svg"}
            alt="Sidebar Dashboard Icon"
            width={25}
            height={25}
            onClick={() => handleSidebar(true)}
          />
        ),
          submenu: [
            userRights(LABELS.ViewSsmDashboard)
              ? {
                  title: t(SIDEBAR_TITLES.SSMDashboards),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/ssm-dashboard",
                      SIDEBAR_TITLES.SSMMonitoring,
                      SIDEBAR_TITLES.SSMDashboards,
                    );
                    handleSidebar(false);
                  },
                  route: "/ssm-dashboard",
                }
              : null,
            userRights(LABELS.ViewListofSsmServers)
              ? {
                  title: t(SIDEBAR_TITLES.ManageServer),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/ssm-manage-server",
                      SIDEBAR_TITLES.SSMMonitoring,
                      SIDEBAR_TITLES.ManageServer,
                    );
                    handleSidebar(false);
                  },
                  route: "/ssm-manage-server",
                }
              : null,
          ],
        }
      : null,

    userRights(LABELS.ViewIdracDashboard) ||
    userRights(LABELS.ViewListofIdracServers)
      ? {
          title: t(SIDEBAR_TITLES.iDRAC),
        icon: (
          <img
            src={"/images/sidebar_dashboard_logo.svg"}
            alt="Sidebar Dashboard Icon"
            width={25}
            height={25}
            onClick={() => handleSidebar(true)}
          />
        ),
          submenu: [
            userRights(LABELS.ViewIdracDashboard)
              ? {
                  title: t(SIDEBAR_TITLES.iDRACDashboard),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/iDRAC-dashboard",
                      SIDEBAR_TITLES.iDRAC,
                      SIDEBAR_TITLES.iDRACDashboard,
                    );
                    handleSidebar(false);
                  },
                  route: "/iDRAC-dashboard",
                }
              : null,
            userRights(LABELS.ViewListofIdracServers)
              ? {
                  title: t(SIDEBAR_TITLES.ManageiDRAC),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/manage-iDRAC",
                      SIDEBAR_TITLES.iDRAC,
                      SIDEBAR_TITLES.ManageiDRAC,
                    );
                    handleSidebar(false);
                  },
                  route: "/manage-iDRAC",
                }
              : null,
          ],
        }
      : null,

    userRights(LABELS.View_List_of_Users) ||
    userRights(LABELS.View_List_of_Roles) ||
    userRights(LABELS.View_List_of_Child_SubChild_Sites) ||
    userRights(LABELS.View_and_Configure_SMTP_Setup_Details) ||
    userRights(LABELS.View_and_Configure_Report_Scheduler) ||
    userRights(LABELS.View_and_Configure_FTP_Setup_Details) ||
    userRights(LABELS.RetentionPeriod) ||
    userRights(LABELS.CanUploadClientLogo) ||
    userRights(LABELS.CanUploadSSLCertificate) ||
    userRights(LABELS.CanClientOperationalTiming) ||
    userRights(LABELS.CanGoogleMapApiKey) ||
    userRights(LABELS.ScheduledDatabaseBackup) ||
    userRights(LABELS.View_and_Configure_ANPR) ||
    userRights(LABELS.View_and_Configure_Reset_Vehicle_Parking_Count) ||
    userRights(LABELS.View_and_Config_Expose_API) ||
    userRights(LABELS.View_License_Details_and_History) ||
    // userRights(LABELS.Upload_New_License) ||
    userRights(LABELS.CanTakeFullDBBackup) ||
    userRights(LABELS.CanRestoreDatabase) ||
    userRights(LABELS.AuditLogMaster) ||
    userRights(LABELS.ViewListofMultiServers)
      ? {
          title: t(SIDEBAR_TITLES.Settings),
          icon: (
            <img
              src={"/images/sidebar_setting_logo.svg"}
              alt="Sidebar Settings Icon"
              width={25}
              height={25}
              onClick={()=> handleSidebar(true)}
            />
          ),
          submenu: [
            userRights(LABELS.View_List_of_Users)
              ? {
                  title: t(SIDEBAR_TITLES.ManageUsers),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/manage-users",
                      SIDEBAR_TITLES.Settings,
                      SIDEBAR_TITLES.ManageUsers,
                    );
                    handleSidebar(false);
                  },
                  route: "/manage-users",
                }
              : null,

            userRights(LABELS.View_List_of_Roles)
              ? {
                  title: t(SIDEBAR_TITLES.RolesAndPermissions),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/roles-and-permissions",
                      SIDEBAR_TITLES.Settings,
                      SIDEBAR_TITLES.RolesAndPermissions,
                    );
                    handleSidebar(false);
                  },
                  route: "/roles-and-permissions",
                }
              : null,

            userRights(LABELS.View_List_of_Child_SubChild_Sites)
              ? {
                  title: t(SIDEBAR_TITLES.MultisiteSetup),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/multisite-setup",
                      SIDEBAR_TITLES.Settings,
                      SIDEBAR_TITLES.MultisiteSetup,
                    );
                    handleSidebar(false);
                  },
                  route: "/multisite-setup",
                }
              : null,

            userRights(LABELS.View_and_Configure_SMTP_Setup_Details) ||
            userRights(LABELS.View_and_Configure_Report_Scheduler) ||
            userRights(LABELS.View_and_Configure_FTP_Setup_Details) ||
            userRights(LABELS.RetentionPeriod) ||
            userRights(LABELS.CanUploadClientLogo) ||
            userRights(LABELS.CanUploadSSLCertificate) ||
            userRights(LABELS.CanClientOperationalTiming) ||
            userRights(LABELS.CanGoogleMapApiKey) ||
            userRights(LABELS.ScheduledDatabaseBackup) ||
            userRights(LABELS.View_and_Configure_ANPR) ||
            userRights(LABELS.View_and_Configure_Reset_Vehicle_Parking_Count) ||
            userRights(LABELS.View_and_Config_Expose_API)
              ? {
                  title: t(SIDEBAR_TITLES.SettingsGeneral),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/general-settings",
                      SIDEBAR_TITLES.Settings,
                      SIDEBAR_TITLES.SettingsGeneral,
                    );
                    handleSidebar(false);
                  },
                  route: "/general-settings",
                }
              : null,

            // userRights(LABELS.Upload_New_License)
            //   ? {
            //       title: SIDEBAR_TITLES.SettingsLicense,
            //       onClick: () =>
            //         handleSubmenuClick(
            //           "/license",
            //           SIDEBAR_TITLES.Settings,
            //           SIDEBAR_TITLES.SettingsLicense
            //         ),
            //       route: "/license",
            //     }
            //   : null,
            userRights(LABELS.View_License_Details_and_History)
              ? {
                  title: t(SIDEBAR_TITLES.SettingsLicense),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/license",
                      SIDEBAR_TITLES.Settings,
                      SIDEBAR_TITLES.SettingsLicense,
                    );
                    handleSidebar(false);
                  },
                  route: "/license",
                }
              : null,

            userRights(LABELS.CanTakeFullDBBackup) ||
            userRights(LABELS.CanRestoreDatabase)
              ? {
                  title: t(SIDEBAR_TITLES.Backup_Restore),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/backup-and-restore",
                      SIDEBAR_TITLES.Settings,
                      SIDEBAR_TITLES.Backup_Restore,
                    );
                    handleSidebar(false);
                  },
                  route: "/backup-and-restore",
                }
              : null,

            userRights(LABELS.AuditLogMaster)
              ? {
                  title: t(SIDEBAR_TITLES.Audit),
                  onClick: () =>{
                    handleSubmenuClick(
                      "/audit",
                      SIDEBAR_TITLES.Settings,
                      SIDEBAR_TITLES.Audit,
                    );
                    handleSidebar(false);
                  },
                  route: "/audit",
                }
              : null,

            userRights(LABELS.ViewListofMultiServers)
              ? {
                title: t(SIDEBAR_TITLES.ServerManagement),
                onClick: () => {
                  handleSubmenuClick(
                    "/server-management",
                    SIDEBAR_TITLES.Settings,
                    SIDEBAR_TITLES.ServerManagement,
                  );
                  handleSidebar(false);
                },
                route: "/server-management",
              }
              : null,
            
          ],
        }
      : null,
    
  ];

  var isLicenseValid = localStorage.getItem("isLicenseValid");
  // console.log("isLicenseValid sidebar", isLicenseValid);
  const menuItemsFiltered =
    isLicenseValid === "true"
      ? menuItems_New
      : [
          {
            title: t(SIDEBAR_TITLES.Settings),
            icon: (
              <img
                src={"/images/sidebar_setting_logo.svg"}
                alt="Sidebar Settings Icon"
                width={25}
                height={25}
                onClick={()=> handleSidebar(true)}
              />
            ),
            submenu: [
              {
                title: t(SIDEBAR_TITLES.SettingsLicense),
                onClick: () =>{
                  handleSubmenuClick(
                    "/license",
                    SIDEBAR_TITLES.Settings,
                    SIDEBAR_TITLES.SettingsLicense,
                  );
                  handleSidebar(false);
                },
                route: "/license",
              },
            ],
          },
        ];

  const sidebarStyles: React.CSSProperties = {
    display: "flex",
    flexDirection: "column" as const, // Explicitly typed
    backgroundColor: theme === "light" ? "#ffffff" : "#1a202c",
    color: theme === "light" ? "#000000" : "#ffffff",
    transition: "width 0.3s ease",
    // width: isSidebarOpen ? "250px" : "60px",
    height: "100vh",
    boxShadow: "2px 0 5px rgba(0, 0, 0, 0.1)",
  };

  // const toggleMenuu = (title) => {
  //   setOpenMenus((prev) => {
  //     const updatedMenus = { ...prev, [title]: !prev[title] };
  //     localStorage.setItem("openMenus", JSON.stringify(updatedMenus));
  //     return updatedMenus;
  //   });
  // };

  const toggleMenuu = (title) => {
    setOpenMenus((prev) => {
      const updatedMenus = {
        [title]: !prev[title], // toggle current menu
      };
      localStorage.setItem("openMenus", JSON.stringify(updatedMenus));
      return updatedMenus;
    });
  };

  const menuItemGenerator = (menuItems) => {
    return (
      // <div className="sidebar">
      <>
        {/* Logo */}
        {/* <div className="logo-container">
          <img src="/images/logo.svg" alt="Logo" className="logo" />
        </div> */}

        {/* Menu */}
        {/* <div className="menu-container"> */}
        <>
          {menuItems.map((menu) =>
            menu ? (
              <div key={menu.title} className="menu-item">
                <button
                  // className="menu-button"
                  onClick={() => toggleMenuu(menu.title)}
                  className={`menu-button ${
                    menu.submenu?.some((sub) => sub?.route === activeRoute)
                      ? "active"
                      : ""
                  }`}
                >
                  <div className="menu-title">
                    {menu.icon}
                    <span>{menu.title}</span>
                  </div>
                  {openMenus[menu.title] ? (
                    <RiArrowUpSLine />
                  ) : (
                    <RiArrowDownSLine />
                  )}
                </button>
                {openMenus[menu.title] && (
                  <div className="submenu">
                    {menu.submenu
                      ?.filter((sub) => sub != null)
                      .map((sub) => {
                        // console.log("sub=>",sub)

                        // Check if this menu is Dashboards or Monitoring
                        const isSpecialMenu =
                          menu.title === t(SIDEBAR_TITLES.Dashboards) ||
                          menu.title === t(SIDEBAR_TITLES.Monitoring);
                        return (
                          <div
                            key={sub.title}
                            className="submenu-item-with-menu"
                          >
                            {editingItem.id === sub.route ? (
                              <Box
                                component="form"
                                onSubmit={
                                  editingItem.type === "dashboard"
                                    ? handleDashboardEditSubmit((data) => {
                                        editDashboard(data);
                                        setEditingItem({
                                          id: null,
                                          title: "",
                                          type: null,
                                        });
                                        resetEditDashboardForm(); // Reset after save
                                      })
                                    : handleMonitoringEditSubmit((data) => {
                                        editMonitoring(data);
                                        setEditingItem({
                                          id: null,
                                          title: "",
                                          type: null,
                                        });
                                        resetEditMonitoringForm(); // Reset after save
                                      })
                                }
                                // sx={{ marginTop: 2 }}
                              >
                                {editingItem.type === "dashboard" ? (
                                  <CustomTextFieldWithButton
                                    name="dashboardName"
                                    control={dashboardEditControl}
                                    rules={{
                                      maxLength: {
                                        value:
                                          COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                                        message: t(
                                          "Dashboard.Dashboard_Strong_Validation",
                                          {
                                            max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                                          },
                                        ),
                                      },
                                    }}
                                    placeholder={t(
                                      "Dashboard.Dashboard_Placeholder",
                                    )}
                                    ShowAddButton={false}
                                    size="small"
                                    autoFocus
                                  />
                                ) : (
                                  <CustomTextFieldWithButton
                                    name="monitoringName"
                                    control={monitoringEditControl}
                                    rules={{
                                      maxLength: {
                                        value:
                                          COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                                        message: t(
                                          "Dashboard.Monitoring_Strong_Validation",
                                          {
                                            max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                                          },
                                        ),
                                      },
                                    }}
                                    placeholder={t(
                                      "Dashboard.Monitoring_Placeholder",
                                    )}
                                    ShowAddButton={false}
                                    size="small"
                                    autoFocus
                                  />
                                )}
                              </Box>
                            ) : (
                              <>
                                <button
                                  key={sub.title}
                                  // className="submenu-item"
                                  className={`submenu-item ${
                                    activeRoute === sub.route ? "active" : ""
                                  }`}
                                  onClick={sub.onClick}
                                >
                                  {sub.title}
                                </button>

                                {isSpecialMenu &&
                                  // Always show for dashboards
                                  (menu.title ===
                                    t(SIDEBAR_TITLES.Dashboards) ||
                                    // Show for monitoring only if permission exists
                                    (menu.title ===
                                      t(SIDEBAR_TITLES.Monitoring) &&
                                      (HasPermission(
                                        LABELS.CanAddOrUpdateMonitoring,
                                      ) ||
                                        HasPermission(
                                          LABELS.CanDeleteMonitoring,
                                        )))) && (
                                    <IconButton
                                      size="small"
                                      onClick={(e) =>
                                        handleMenuOpen(
                                          e,
                                          sub.route,
                                          sub.title,
                                          menu.title ===
                                            t(SIDEBAR_TITLES.Dashboards)
                                            ? "dashboard"
                                            : "monitoring",
                                        )
                                      }
                                    >
                                      <MoreVert fontSize="small" />
                                    </IconButton>
                                  )}
                              </>
                            )}
                          </div>
                        );
                      })}
                    {menu.title === t(SIDEBAR_TITLES.Dashboards) && (
                      <>
                        {isAdding ? (
                          // <input
                          //   type="text"
                          //   placeholder="Enter dashboard name"
                          //   value={newDashboardName}
                          //   onChange={(e) =>
                          //     setNewDashboardName(e.target.value)
                          //   }
                          //   onKeyDown={(e) => {
                          //     e.stopPropagation();
                          //     if (e.key === "Enter") {
                          //       addNewDashboard();
                          //       setIsAdding(false);
                          //     }
                          //   }}
                          //   className="add-dashboard"
                          // />
                          <Box
                            component="form"
                            onSubmit={handleDashboardSubmit((data) => {
                              addNewDashboard(data);
                              setIsAdding(false);
                            })}
                            sx={{ marginTop: 2 }}
                          >
                            <CustomTextFieldWithButton
                              name="dashboardName"
                              control={dashboardControl}
                              rules={{
                                maxLength: {
                                  value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                                  message: t(
                                    "Dashboard.Dashboard_Strong_Validation",
                                    {
                                      max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                                    },
                                  ),
                                },
                              }}
                              placeholder={t("Dashboard.Dashboard_Placeholder")}
                              ShowAddButton={false}
                              autoFocus
                            />
                          </Box>
                        ) : (
                          // HasPermission(LABELS.can_add_update_dashboard_preference) ? (
                          <button
                            className="add-dashboard"
                            onClick={() => {
                              setIsAdding(true);
                            }}
                          >
                            + {t(SIDEBAR_TITLES.Add_Dashboard_btn)}
                          </button>
                          // ) : null
                        )}
                      </>
                    )}

                    {menu.title === t(SIDEBAR_TITLES.Monitoring) && (
                      <>
                        {isAddingMonitoring ? (
                          <Box
                            component="form"
                            onSubmit={handleMonitoringSubmit((data) => {
                              addNewMonitoring(data);
                              setIsAdding(false);
                            })}
                            sx={{ marginTop: 2 }}
                          >
                            <CustomTextFieldWithButton
                              name="monitoringName"
                              control={monitoringControl}
                              rules={{
                                maxLength: {
                                  value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                                  message: t(
                                    "Dashboard.Monitoring_Strong_Validation",
                                    {
                                      max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                                    },
                                  ),
                                },
                              }}
                              placeholder={t(
                                "Dashboard.Monitoring_Placeholder",
                              )}
                              ShowAddButton={false}
                              autoFocus
                            />
                          </Box>
                        ) : HasPermission(LABELS.CanAddOrUpdateMonitoring) ? (
                          <button
                            className="add-dashboard"
                            onClick={() => {
                              setIsAddingMonitoring(true);
                            }}
                          >
                            + {t(SIDEBAR_TITLES.Add_Monitoring_btn)}
                          </button>
                        ) : null}
                      </>
                    )}
                  </div>
                )}
              </div>
            ) : null,
          )}

          {Boolean(menuAnchor.anchorEl) && (
            <Menu
              anchorEl={menuAnchor.anchorEl}
              open={Boolean(menuAnchor.anchorEl)}
              onClose={handleMenuClose}
              PaperProps={{
                elevation: 3,
                sx: {
                  borderRadius: 2,
                  pl: 1,
                  pr: 1,
                },
              }}
              className="edit-delete-pop"
            >
              {menuAnchor.type === "monitoring" ? (
                <>
                  {HasPermission(LABELS.CanAddOrUpdateMonitoring) && (
                    <MenuItem
                      onClick={() => {
                        // console.log(
                        //   "Edit clicked for",
                        //   menuAnchor.dashboardId,
                        //   menuAnchor.type
                        // );
                        setEditingItem({
                          id: menuAnchor.dashboardId,
                          title: menuAnchor.title,
                          type: menuAnchor.type,
                        });
                        handleMenuClose();
                      }}
                    >
                      <IconButton>
                        <img
                          src="/images/edit.svg"
                          alt="edit"
                          width={20}
                          height={20}
                        />
                      </IconButton>
                      <ListItemText
                        primary={t("Common_Edit_Delte_Menu.Edit")}
                      />
                    </MenuItem>
                  )}

                  {HasPermission(LABELS.CanDeleteMonitoring) && (
                    <MenuItem
                      onClick={() => {
                        setDeletingItem({
                          id: menuAnchor.dashboardId,
                          title: menuAnchor.title,
                          type: menuAnchor.type,
                        });
                        setIsConfirmOpen(true);
                        handleMenuClose();
                      }}
                    >
                      <IconButton>
                        <img
                          src="/images/delete_gray.svg"
                          alt="delete"
                          width={20}
                          height={20}
                        />
                      </IconButton>
                      <ListItemText
                        primary={t("Common_Edit_Delte_Menu.Delete")}
                      />
                    </MenuItem>
                  )}
                </>
              ) : (
                <>
                  <MenuItem
                    onClick={() => {
                      setEditingItem({
                        id: menuAnchor.dashboardId,
                        title: menuAnchor.title,
                        type: menuAnchor.type,
                      });
                      handleMenuClose();
                    }}
                  >
                    <IconButton>
                      <img
                        src="/images/edit.svg"
                        alt="edit"
                        width={20}
                        height={20}
                      />
                    </IconButton>
                    <ListItemText primary={t("Common_Edit_Delte_Menu.Edit")} />
                  </MenuItem>

                  <MenuItem
                    onClick={() => {
                      setDeletingItem({
                        id: menuAnchor.dashboardId,
                        title: menuAnchor.title,
                        type: menuAnchor.type,
                      });
                      setIsConfirmOpen(true);
                      handleMenuClose();
                    }}
                  >
                    <IconButton>
                      <img
                        src="/images/delete_gray.svg"
                        alt="delete"
                        width={20}
                        height={20}
                      />
                    </IconButton>
                    <ListItemText
                      primary={t("Common_Edit_Delte_Menu.Delete")}
                    />
                  </MenuItem>
                </>
              )}
            </Menu>
          )}

          <CommonDialog
            open={isConfirmOpen}
            title={t("Common_DELETE_Confirmation_Dialog.Title")}
            content={t("Common_DELETE_Confirmation_Dialog.Content")}
            customClass="cmn-confirm-delete-icon"
            onConfirm={() => deletingItem && handleDeleteSubMenu(deletingItem)}
            onCancel={handleCloseConfirm}
            confirmText={t("Common_DELETE_Confirmation_Dialog.Delete")}
            cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
            type="delete"
            titleClass={true}
          />
        </>
        {/* </div> */}
        {/* </div> */}
      </>
    );
  };

  const handleMenuOpen = (event, dashboardId, title, type) => {
    setMenuAnchor({ anchorEl: event.currentTarget, dashboardId, title, type });
  };

  const handleMenuClose = () => {
    setMenuAnchor({ anchorEl: null, dashboardId: null, title: null });
  };

  return (
    <>
    <div
      className={isSidebarOpen ? "sidebar-main open" : "sidebar-main close"}
      key={i18n.language}
    >
      <div className="sidebar-wrapper">
        <div className="side-logo-wrapper">
          <img
            // src={clientLogo || `images/vision_insight_logo.svg`}
            src={clientLogo}
            alt="Vision Insight"
            className="hanwhaLogo"
            onClick={() =>{
              handleSubmenuClick(
                "/welcome",
                SIDEBAR_TITLES.WelcomePage,
                SIDEBAR_TITLES.WelcomePagesubtitle,
              );
              handleSidebar(false);
            }
            }
          />
          <IconButton onClick={toggleButton}>
            <MenuIcon />
          </IconButton>
        </div>

        <div className="main-menu-items-wrapper">
          {menuItemGenerator(menuItemsFiltered)}
        </div>
      </div>

      {/* <div className="sidebar-footer"> */}
      <div>
        <SpeechDashboard />
      </div>
      {/* </div> */}
      <div className="sidebar-version">
        {t("sidebar.Version")} <strong>4.0.2</strong>
      </div>
    </div>
      {toastList.map((toast, index) => (
        <Snackbar
          key={toast.id}
          open={true}
          autoHideDuration={toast.duration}
          onClose={() => {
            setToastList((prev) =>
              prev.filter((item) => item.id !== toast.id)
            );
          }}
          anchorOrigin={{
            vertical: "top",
            horizontal: "right",
          }}
          sx={{
            top: `${80 + index * 70}px !important`,
          }}
        >
          <Alert
            severity={toast.severity}
            variant="filled"
            onClose={() => {
              setToastList((prev) =>
                prev.filter((item) => item.id !== toast.id)
              );
            }}
            sx={{ width: "100%" }}
          >
            <div>
              <div>{toast.message}</div>
              <div style={{ marginTop: "4px", fontSize: "12px" }}>
                IDRAC IP : {toast.sourceIP}
              </div>
            </div>
          </Alert>
        </Snackbar>
      ))}
    </>
  );
};

export { Sidebar };
