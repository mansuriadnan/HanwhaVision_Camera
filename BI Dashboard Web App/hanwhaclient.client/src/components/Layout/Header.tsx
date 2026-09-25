import React, { useCallback, useEffect, useRef, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";

// import {
//   CommonDialog,
//   NotificationDeatil,
//   UserPreferences,
//   UserProfileDetails,
// } from "../index";

import { Drawer, Typography } from "@mui/material";
import { ISite } from "../../interfaces/IMultiSite";
import { useUser } from "../../context/UserContext";
import { useSettingsContext } from "../../context/SettingContext";
import {
  onReceiveMessage,
  startSignalRConnection,
} from "../../utils/signalRService";
import { INotification } from "../../interfaces/Inotifications";
import { GetUserNotificationCount } from "../../services/notificationServices";
import { HasPermission, useHasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { formatDate, formatDateToCustomFormat } from "../../utils/dateUtils";
import { CommonDialog } from "../Reusable/CommonDialog";
import { NotificationDeatil } from "../Layout/NotificationDeatil";
import { UserPreferences } from "../Users/UserPreferences";
import { UserProfileDetails } from "../Users/UserProfileDetails";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { useTranslation } from "react-i18next";

const Header: React.FC = () => {
  const location = useLocation();
  const { user } = useUser();
  const navigate = useNavigate();
  const { handleLogout } = useAuth();
  const { settings } = useSettingsContext();
  const { t } = useTranslation();

  const sidebarMenuName = location.state?.sidebarMenuName || null;
  const screenName = location.state?.screenName || null;

  const [isOpen, setIsOpen] = useState(false);
  const [openLogoutConfirm, setOpenLogoutConfirm] = useState<boolean>(false);
  const [openProfileDialog, setOpenProfileDialog] = useState<boolean>(false);
  const [openPreferencesDialog, setOpenPreferencesDialog] =
    useState<boolean>(false);
  const [openChangePasswordDialog, setOpenChangePasswordDialog] =
    useState<boolean>(false);
  const [openNotificationDrawer, setOpenNotificationDrawer] = useState(false);
  const [unreadCount, setUnreadCount] = useState<number>(0);
  const [isBouncing, setIsBouncing] = useState<boolean>(false);
  const [notifications, setNotifications] = useState<INotification[]>([]);

  const dropdownRef = useRef<HTMLDivElement>(null);

  const currentDate = new Date().toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });

  const [currentTime, setCurrentTime] = useState("");
  const [selectedTimeZone, setSelectedTimeZone] = useState("");
  const canViewNotificationPermission = useHasPermission(
    LABELS.View_List_of_Notifications
  );
  const { timeFormat } = useTimeFormatContext();
  // useEffect(() => {
  //   fetchInitialData();
  // }, []);

  // const fetchInitialData = async () => {
  //   try {
  //     const siteData: any = await GetAllSiteService();
  //     setSiteList(siteData.data as ISite[]);

  //     // console.log("siteData==>",siteData)
  //   } catch (err: any) {
  //     console.error("Error fetching initial data:", err);
  //   }
  // };

  useEffect(() => {
    const timeout = setTimeout(() => {
      onReceiveMessage("userNotification", handleSignalRMessage);
    }, 5000);

    const handleSignalRMessage = (data: any) => {
      try {
        const liveDataRaw = JSON.parse(data);
        const liveData = {
          notificationId: liveDataRaw.Id,
          title: liveDataRaw.Title,
          content: liveDataRaw.Content,
          isRead: liveDataRaw.IsRead,
          actionName: liveDataRaw.ActionName,
          actionParameter: liveDataRaw.ActionParameter,
          createdOn: liveDataRaw.CreatedOn || new Date().toISOString(),
        };

        if (!liveData.notificationId || !liveData.title || !liveData.content) {
          console.warn("Incomplete notification received:", liveData);
          return;
        }

        setNotifications((prev) => {
          const alreadyExists = prev.some(
            (n) => n.notificationId === liveData.notificationId
          );
          return alreadyExists ? prev : [liveData, ...prev];
        });

        setUnreadCount((prev) => prev + 1);
        setIsBouncing(true); // trigger animation

        // Optional: you can persist latest notification globally or via context
      } catch (e) {
        console.error("Error parsing live notification:", e);
      }
    };

    return () => {
      clearTimeout(timeout);
    };
  }, []);

  useEffect(() => {
    const updateCurrentTime = () => {
      try {
        const configuredTimeZoneDetails = localStorage.getItem(
          "userProfileReferenceData"
        );
        let configuredZoneLabel: any = null;

        if (configuredTimeZoneDetails) {
          const parsedData = JSON.parse(configuredTimeZoneDetails);
          configuredZoneLabel = parsedData?.timeZone?.label ?? null;
        }

        const timeFormatStr = timeFormat === "12h" ? "hh:mm A" : "HH:mm";

        if (configuredZoneLabel?.timeZoneAbbr) {
          setSelectedTimeZone(configuredZoneLabel.timeZoneAbbr);
        }

        const formattedCurrentTime = formatDateToCustomFormat(
          formatDateToConfiguredTimezone(new Date().toISOString()),
          timeFormatStr
        );
        setCurrentTime(formattedCurrentTime);
      } catch (e) {
        console.warn("Error updating time:", e);
      }
    };

    updateCurrentTime();
    const timer = setInterval(() => {
      updateCurrentTime();
    }, 60 * 1000);

    return () => clearInterval(timer);
  }, [timeFormat]);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    }

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen]);

  const fetchUnreadCount = useCallback(async () => {
    try {
      const count = await GetUserNotificationCount();
      setUnreadCount(count as number);
    } catch (error) {
      console.error("Error fetching unread count:", error);
    }
  }, []);

  useEffect(() => {
    if (canViewNotificationPermission) {
      fetchUnreadCount();
    }
  }, [canViewNotificationPermission, fetchUnreadCount]);

  const handleProfilePage = () => {
    setIsOpen(false);
    setOpenProfileDialog(true);
  };

  // Method to open Preferences Dialog
  const handlePreferences = () => {
    setIsOpen(false);
    setOpenPreferencesDialog(true);
  };

  const handleChangePassword = () => {
    setIsOpen(false);
    setOpenChangePasswordDialog(true);
  };

  const handleLogoutConfirm = () => {
    setIsOpen(false);
    setOpenLogoutConfirm(true);
  };

  const handleLogoutFromHeader = () => {
    setOpenLogoutConfirm(false);
    handleLogout();
    navigate("/");
  };

  const handleCloseConfirm = () => {
    setOpenLogoutConfirm(false);
  };

  const handleCloseProfileDialog = () => {
    setOpenProfileDialog(false);
  };

  const handleCloseChangePasswordDialog = () => {
    setOpenChangePasswordDialog(false);
  };

  // Method to close Preferences Dialog
  const handleClosePreferencesDialog = () => {
    setOpenPreferencesDialog(false);
  };

  const handleOpenNotificationDrawer = () => {
    setNotifications([]); // reset notifications
    setOpenNotificationDrawer(true);
  };

  const handleCloseNotificationDrawer = () => {
    setOpenNotificationDrawer(false);
  };

  return (
    <>
      <header className="header">
        <div className="header-wrapper">
          <div className="breadcrumbs">
            {/* {sidebarMenuName && screenName && (
              <>
                <span>{sidebarMenuName}</span> <span>{screenName}</span>
              </>
            )} */}
            {window.location.pathname === "/welcome" ? (
              <>
                <span>{t("Dashboards")}</span>{" "}
                <span>{t("Welcome_content")}</span>
              </>
            ) : (
              sidebarMenuName &&
              screenName && (
                <>
                  <span>{t(sidebarMenuName)}</span> <span>{t(screenName)}</span>
                </>
              )
            )}
          </div>
          <div className="header-logo">
            {settings?.logo ? (
              <a href="#">
                <img src={settings.logo} alt="logo" />
              </a>
            ) : null}
          </div>
          <div className="header-right">
            <Typography style={{ color: "#212121", fontSize: "14px" }}>
              <span style={{ fontWeight: 700 }}>{currentTime}</span>{" "}
              {selectedTimeZone == "" ? (
                ""
              ) : (
                <span style={{ fontWeight: 600 }}>({selectedTimeZone})</span>
              )}
            </Typography>
            {HasPermission(LABELS.View_List_of_Notifications) && (
              <div
                className="notifications"
                onClick={handleOpenNotificationDrawer}
              >
                <img
                  src="/images/bell.png"
                  alt="notification"
                  className={isBouncing ? "bell-bounce" : ""}
                  onAnimationEnd={() => setIsBouncing(false)} // reset after animation
                />
                <svg xmlns="http://www.w3.org/2000/svg" width="10" height="10" viewBox="0 0 10 10" fill="none">
                <path d="M4.93134 0C2.21217 0 0 2.22178 0 4.95272C0 7.68366 2.21217 9.90544 4.93134 9.90544C7.65051 9.90544 9.86273 7.68366 9.86273 4.95272C9.86273 2.22178 7.65051 0 4.93134 0ZM4.93134 8.87419C2.78081 8.87419 1.03125 7.11502 1.03125 4.95272C1.03125 2.79042 2.78081 1.03125 4.93134 1.03125C7.08187 1.03125 8.83148 2.79042 8.83148 4.95272C8.83148 7.11502 7.08187 8.87419 4.93134 8.87419Z" fill="#FF6600"/>
                <path d="M6.77899 4.2482H5.65265V3.11704C5.65265 2.73905 5.34514 2.43262 4.96582 2.43262C4.5865 2.43262 4.27899 2.73905 4.27899 3.11704V4.2482H3.15265C2.77333 4.2482 2.46582 4.55463 2.46582 4.93262C2.46582 5.3106 2.77333 5.61704 3.15265 5.61704H4.27899V6.7482C4.27899 7.12618 4.5865 7.43262 4.96582 7.43262C5.34514 7.43262 5.65265 7.12618 5.65265 6.7482V5.61704H6.77899C7.15831 5.61704 7.46582 5.3106 7.46582 4.93262C7.46582 4.55463 7.15837 4.2482 6.77899 4.2482Z" fill="#FF6600"/>
                </svg>
                {/* {unreadCount > 0 && (
                <span className="notification-count">{unreadCount}</span>
              )} */}

                {unreadCount > 0 && (
                  <span className="notification-count">
                    {unreadCount > 999 ? "999+" : unreadCount}
                  </span>
                )}
              </div>
            )}
            <div className="user-pro-wrapper">
              <div
                className="userProfileBox"
                onClick={() => setIsOpen(!isOpen)}
              >
                <img
                  src={
                    user?.profileImage ||
                    "/images/Super_Admin_User_Profile_Image.png"
                  }
                  className="useravatar"
                  alt="User Avatar"
                />
              </div>

              {isOpen && (
                <div className="userProfileDropdown" ref={dropdownRef}>
                  <div className="user-pro-name">
                    <p>
                      {user?.email}
                      {/* johndoe@email.com */}
                    </p>
                    <strong>
                      {user?.username ?? ""}
                      {/* +971-55-1234567 */}
                    </strong>
                  </div>

                  <ul>
                    <li
                      style={{ padding: "10px", cursor: "pointer" }}
                      onClick={handleProfilePage}
                    >
                      {t("Profile")}
                    </li>
                    <li
                      style={{ padding: "10px", cursor: "pointer" }}
                      onClick={handlePreferences}
                    >
                      {t("Preferences")}
                    </li>
                    <li
                      style={{ padding: "10px", cursor: "pointer" }}
                      // onClick={() => {
                      //   setIsOpen(false);
                      //   navigate("/changepassword", {
                      //     state: { screenName: LABELS.Change_Password },
                      //   });
                      // }}
                      onClick={handleChangePassword}
                    >
                      {t("Change_Password")}
                    </li>
                    <li
                      style={{ padding: "10px", cursor: "pointer" }}
                      onClick={handleLogoutConfirm}
                    >
                      {t("Logout")}
                    </li>
                  </ul>
                </div>
              )}
            </div>
          </div>
          <CommonDialog
            open={openLogoutConfirm}
            title={t("Login.Logout_Confirmation")}
            content={<p>{t("Login.Logout_Confirmation1")}</p>}
            onConfirm={handleLogoutFromHeader}
            onCancel={handleCloseConfirm}
            confirmText={t("Login.Confirm")}
            cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
            type="logout"
            customClass="logout-pop-main"
            titleClass={true}
          />

          {/* Profile Section Dialog */}
          <CommonDialog
            open={openProfileDialog}
            title= {t("Profile")}
            content={<UserProfileDetails />}
            onCancel={handleCloseProfileDialog}
            fullWidth={true}
            customClass="profile-pop-main"
          />

          {/* Change Password Dialog */}
          {/* <CommonDialog
        open={openChangePasswordDialog}
        title="Change Password"
        content={<ChangePasswordPage />}
        onCancel={handleCloseChangePasswordDialog}
        fullWidth={true}
      /> */}

          {/* Change Password Dialog */}
          <CommonDialog
            open={openChangePasswordDialog}
            title={t("Change_Password_Restriction")}
            content={t("Change_Password_Restriction_Content")}
            confirmText={t("Okay")}
            onCancel={handleCloseChangePasswordDialog}
            onConfirm={handleCloseChangePasswordDialog}
            type="contactAdministrator"
            customClass="forgot-pass"
            titleClass={true}
          />

          {/* Preferences Dialog Box*/}
          <CommonDialog
            open={openPreferencesDialog}
            title= {t("Preferences")}
            content={<UserPreferences onClose={handleClosePreferencesDialog} />}
            onCancel={handleClosePreferencesDialog}
            fullWidth={true}
            customClass="preferences-pop-main"
          />
        </div>
      </header>

      <Drawer
        anchor={"right"}
        open={openNotificationDrawer}
        onClose={() => {
          handleCloseNotificationDrawer();
        }}
        className="cmn-pop"
      >
        <NotificationDeatil
          onClose={handleCloseNotificationDrawer}
          unreadCount={unreadCount}
          setUnreadCount={setUnreadCount}
          triggerBellAnimation={() => setIsBouncing(true)}
          notifications={notifications}
          setNotifications={setNotifications}
          isDrawerOpen={openNotificationDrawer}
          fetchUnreadCount={fetchUnreadCount}
        />
      </Drawer>
    </>
  );
};

export { Header };
