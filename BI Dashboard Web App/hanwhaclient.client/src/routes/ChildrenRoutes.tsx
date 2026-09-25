import { lazy, Suspense } from "react";
import { RouteObject } from "react-router-dom";
import ProtectedRoute from "./ProtectedRoute";
import AuditLogs from "../Pages/Audit/AuditLogs";
import Dashboard from "../Pages/iDRAC/dashboard";
import LiveChart from "../Pages/iDRAC/LiveChart";
const ErrorLogsPage = lazy(() => import("../Pages/ErrorLogs/ErrorLogsPage"));
const UserManagementPage = lazy(() => import("../Pages/User/UserManagement"));
const DeviceManagement = lazy(() => import("../Pages/Camera/DeviceManagement"));
const Logout = lazy(() => import("../Pages/Auth/Logout"));
const FloorPlansAndZonesPage = lazy(
  () => import("../Pages/FloorPlansAndZones/FloorPlansAndZonesPage")
);
const MultisiteSetupPage = lazy(
  () => import("../Pages/MutlisiteSetup/MultisiteSetupPage")
);
const RolesAndPermissionsPage = lazy(
  () => import("../Pages/RolesAndPermissions/RolesAndPermissionsPage")
);
const SettingGeneralPage = lazy(
  () => import("../Pages/Settings/SettingGeneralPage")
);
const SettingLicensePage = lazy(
  () => import("../Pages/Settings/SettingLicensePage")
);
const MonitoringPage = lazy(() => import("../Pages/Monitoring/MonitoringPage"));
const WelcomePage = lazy(() => import("../Pages/Welcome/WelcomePage"));
const DashboardPage = lazy(
  () => import("../Pages/DashBoard_Home/DashboardPage")
);
const EventlogsManagement = lazy(
  () => import("../Pages/Eventlogs/EventlogsManagement")
);
const ReportsPage = lazy(() => import("../Pages/MyReports/ReportsPage"));
const BackupRestorePage = lazy(
  () => import("../Pages/BackupRestore/BackupRestorePage")
);
const OwnersPage = lazy(() => import("../Pages/ANPR/OwnersPage"));
const MaintenanceSchedule = lazy(() => import("../Pages/Maintenance/MaintenanceSchedule"));
const MaintenancePlan = lazy(() => import("../Pages/Maintenance/MaintenancePlan"));
const RMA = lazy(() => import("../Pages/Maintenance/RMA"));
const LPR = lazy(()=>import("../Pages/ANPR/LPR"));
const Server = lazy(()=>import("../Pages/ServerManagement/Server"));
const SSMDashboard = lazy(()=>import("../Pages/SSM/SSMDashboard"));
const SSMManageServer = lazy(()=>import("../Pages/SSM/SSMManageServer"));
const IDRACDashboard = lazy(()=>import("../Pages/iDRAC/iDRACDashboard"));
const ManageiDRAC = lazy(()=>import("../Pages/iDRAC/ManageiDRAC"));


const protectedRoutes = [
  "/dashboard",
  // "/dashboard/:id",
  "/manage-users",
  "/addedituser",
  "/roles",
  "/roleaddedit",
  "/screen-management",
  "/changepassword",
  "/sites",
  "/siteaddedit",
  "/zones",
  "/manage-devices",
  "/cameraaddedit",
  "/floorplans-and-zones",
  "/add-devices",
  "/multisite-setup",
  "/roles-and-permissions",
  "/google-map",
  "/general-settings",
  "/license",
  "/manage-eventlogs",
  "/reports",
  "/welcome",
  "/backup-and-restore",
  "/monitoring",
  "/owners",
  "/audit",
  "/maintenance-plan",
  "/maintenance-schedule",
  "/rma",
  "/advance-pdf",
  "/lpr",
  "/server-management",
  "/ssm-dashboard",
  "/ssm-manage-server",
  "/healthreport-pdf",
  "iDRAC-dashboard",
  "/manage-iDRAC"
];

export const ChildrenRoutes = (): RouteObject[] => {
  const routes = [
    {
      path: "/dashboard",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <DashboardPage />
        </Suspense>
      ),
    },
    // {
    //   path: "/dashboard/:id",
    //   element: <DashboardPage />,
    // },
    {
      path: "/manage-users",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <UserManagementPage />
        </Suspense>
      ),
    },
    // {
    //   path: "/addedituser",
    //   element: <UserAddEditForm />,
    // },
    // {
    //   path: "/roles",
    //   element: <RolesListPage />,
    // },
    // {
    //   path: "/roleaddedit",
    //   element: <RoleAddEditPage />,
    // },
    // {
    //   path: "/screen-management",
    //   element: <RolePermissionsManagement />,
    // },
    // {
    //   path: "/changepassword",
    //   element: <ChangePasswordPage />,
    // },
    // {
    //   path: "/sites",
    //   element: <SiteList />,
    // },
    // {
    //   path: "/siteaddedit",
    //   element: <SiteAddEdit />,
    // },
    {
      path: "/manage-devices",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <DeviceManagement />
        </Suspense>
      ),
    },
    // {
    //   path: "/cameraaddedit",
    //   element: <DeviceAddEdit/>,
    // },

    // {
    //   path: "/zones",
    //   element: <ZonePage />,
    // },
    {
      path: "/logout",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <Logout />
        </Suspense>
      ),
    },
    // {
    //   path: "/emailtemplates",
    //   element: <EmailTemplatesPage />,
    // },
    {
      path: "/floorplans-and-zones",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <FloorPlansAndZonesPage />
        </Suspense>
      ),
    },
    {
      path: "/multisite-setup",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <MultisiteSetupPage />
        </Suspense>
      ),
    },
    {
      path: "/roles-and-permissions",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <RolesAndPermissionsPage />
        </Suspense>
      ),
    },
    {
      path: "/general-settings",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <SettingGeneralPage />
        </Suspense>
      ),
    },
    {
      path: "/license",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <SettingLicensePage />
        </Suspense>
      ),
    },
    {
      path: "/manage-eventlogs",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <EventlogsManagement />
        </Suspense>
      ),
    },
    {
      path: "/monitoring",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <MonitoringPage />
        </Suspense>
      ),
    },
    {
      path: "/reports",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <ReportsPage />
        </Suspense>
      ),
    },
    {
      path: "/backup-and-restore",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <BackupRestorePage />
        </Suspense>
      ),
    },
    {
      path: "/welcome",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <WelcomePage />
        </Suspense>
      ),
    },
    {
      path: "/exception-logs",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <ErrorLogsPage />
        </Suspense>
      ),
    },
    {
      path: "/owners",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <OwnersPage/>
        </Suspense>
      ),
    },
    {
      path: "/audit",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <AuditLogs/>
        </Suspense>
      ),
    },
    {
      path: "/maintenance-plan",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <MaintenancePlan/>
        </Suspense>
      ),
    },
    {
      path: "/maintenance-schedule",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <MaintenanceSchedule/>
        </Suspense>
      ),
    },
    {
      path: "/rma",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <RMA/>
        </Suspense>
      ),
    },
    {
      path: "/lpr",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <LPR/>
        </Suspense>
      ),
    },
    {
      path: "/server-management",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <Server />
        </Suspense>
      ),
    },
    {
      path: "/ssm-dashboard",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <SSMDashboard />
        </Suspense>
      ),
    },
    {
      path: "/ssm-manage-server",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <SSMManageServer />
        </Suspense>
      ),
    },
    {
      path: "/iDRAC-dashboard",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <IDRACDashboard />
        </Suspense>
      ),
    },
    {
      path: "/manage-iDRAC",
      element: (
        <Suspense fallback={<p>Loading...</p>}>
          <ManageiDRAC />
        </Suspense>
      ),
    }
  ];

  return routes.map((route) => {
    var isLicenseValid = localStorage.getItem("isLicenseValid"); // ✅ Now inside the component
    // Check if the route is protected
    //console.log("isLicenseValid children= ", isLicenseValid);
    if (protectedRoutes.includes(route.path)) {
      return {
        ...route,
        element:
          isLicenseValid === "true" || isLicenseValid == null ? (
            <ProtectedRoute>{route.element}</ProtectedRoute>
          ) : (
            <>
              <ProtectedRoute>
                {route.path === "/welcome" ? (
                  <WelcomePage />
                ) : (
                  <>
                    {console.log("isLicenseValid route.license= ", route.path)}
                    <SettingLicensePage />
                  </>
                )}
              </ProtectedRoute>
            </>
          ),
        //<ProtectedRoute>{route.element}</ProtectedRoute>,
      };
    }
    return route;
  });
};
