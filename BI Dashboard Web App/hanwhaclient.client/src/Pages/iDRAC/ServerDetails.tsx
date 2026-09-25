import React, { useEffect, useState } from 'react';
import { Box, ListItemText, Menu, MenuItem, Typography } from '@mui/material';
import { IdracDashboardModel, ILEDPayload, IPowerPayload, IserverData, IserverHealth } from '../../interfaces/IManageiDRAC';
import EventLogDialog from './EventLogDialog';
import { powerActionservice, toggleServerLEDService } from '../../services/iDRACService';
import MemoryTab from './ServerDetailTabs/MemoryTab';
import ProcessorTab from './ServerDetailTabs/ProcessorTab';
import StorageTab from './ServerDetailTabs/StorageTab';
import CoolingTab from './ServerDetailTabs/CoolingTab';
import PowerSupplyTab from './ServerDetailTabs/PowerSupplyTab';
import NetworkDevicesTab from './ServerDetailTabs/NetworkDevicesTab';
import SystemInfoDialog from './SystemInfoDialog';
import {
    fetchEventSource
} from "@microsoft/fetch-event-source";
import { HasPermission } from '../../utils/screenAccessUtils';
import { LABELS } from '../../utils/constants';
import SystemLogDialog from './SystemLogDialog';

interface ServerDetailsProps {
  server: IserverData;
  serverHealth: IserverHealth[];
  setServerHealth: React.Dispatch<React.SetStateAction<IserverHealth[]>>;
  serverDetail: IdracDashboardModel | null;
  LEDIndicator : boolean;
}
import { useTranslation } from "react-i18next";
import apiUrls from '../../constants/apiUrls';
import { useThemeContext } from '../../context/ThemeContext';



type TabKey =
  | "MEMORY"
  | "PROCESSOR"
  | "STORAGE"
  | "COOLING"
  | "POWER_SUPPLY"
  | "NETWORK_DEVICES";



const ServerDetails: React.FC<ServerDetailsProps> = ({  
  server,
  serverHealth,
  setServerHealth,
  serverDetail,
  LEDIndicator
}) => {
  const [activeTab, setActiveTab] =
    useState<TabKey>("MEMORY");
  const [eventLogOpen, setEventLogOpen] = useState(false);
  const [systemLogOpen, setSystemLogOpen] = useState(false);
  const [isLedOn, setIsLedOn] = useState(LEDIndicator);
  const [powerAnchorEl, setPowerAnchorEl] = useState<null | HTMLElement>(null);
  const [openSytemInfoDialog, setOpenSytemInfoDialog] = useState(false);
  const { t } = useTranslation();
  const { theme, themeColor } = useThemeContext();

  const themeColorPathForiDRAC =
   themeColor === "default-theme"
    ? theme === "dark"
      ? "default-theme/dark-theme/iDRAC/"
      : "default-theme/light-theme/iDRAC/"
    : theme === "dark"
      ? "dark-theme/iDRAC/"
      : "iDRAC/";

    
  const TABS: {
    key: TabKey;
    label: string;
    icon: string;
  }[] = [
      {
        key: "MEMORY",
        label: t("IDRAC_Server_Details.Tabs.Memory"),
        icon: `/images/${themeColorPathForiDRAC}memory.gif`,
      },
      {
        key: "PROCESSOR",
        label: t("IDRAC_Server_Details.Tabs.Processor"),
        icon: `/images/${themeColorPathForiDRAC}processor.gif`,
      },
      {
        key: "STORAGE",
        label: t("IDRAC_Server_Details.Tabs.Storage"),
        icon: `/images/${themeColorPathForiDRAC}storage.gif`,
      },
      {
        key: "COOLING",
        label: t("IDRAC_Server_Details.Tabs.Cooling"),
        icon: `/images/${themeColorPathForiDRAC}fan.gif`,
      },
      {
        key: "POWER_SUPPLY",
        label: t("IDRAC_Server_Details.Tabs.Power_Supply"),
        icon: `/images/${themeColorPathForiDRAC}power-supply.gif`,
      },
      {
        key: "NETWORK_DEVICES",
        label: t("IDRAC_Server_Details.Tabs.Network_Devices"),
        icon: `/images/${themeColorPathForiDRAC}network-device.gif`,
      },
    ];

  useEffect(() => {
    setIsLedOn(LEDIndicator);
  }, [LEDIndicator]);
  
  const handleToggleLED = async () => {
    try {
      const nextStatus = !isLedOn;

      const payload: ILEDPayload = {
        serverId: server.id,
        ipAddress: server.ipAddress,
        ledState: nextStatus,
      };
      const response: any = await toggleServerLEDService(payload);

      if (response.isSuccess) {
        setIsLedOn(nextStatus);
      } else {
        setIsLedOn(!nextStatus);
      }
    } catch (error) {
      console.error('LED toggle failed', error);
    }
  };

  const openPowerMenu = (
    event: React.MouseEvent<HTMLElement>
  ) => {
    setPowerAnchorEl(event.currentTarget);
  };

  const closePowerMenu = () => {
    setPowerAnchorEl(null);
  };


  const handlePowerAction = async(type: string) => {

     const powerPayload: IPowerPayload = {
        serverId: server.id,
        ipAddress: server.ipAddress,
        resetType: type,
      };
      const response: any = await powerActionservice(powerPayload);


    closePowerMenu();
  };

 const handleLaunchRemoteConsole = () => {
  const remoteConsoleUrl = `https://${server.ipAddress}/console`;

  window.open(remoteConsoleUrl, '_blank');
};

const hasWarning = (tab: TabKey) => {
  if (!serverDetail) return false;

  switch (tab) {
    case "MEMORY":
      return (
        serverDetail.Memory?.Health !== "OK" ||
        serverDetail.Memory?.MemoryList?.some(
          (item) => item.Health !== "OK"
        )
      );

    case "PROCESSOR":
      return (
        serverDetail.Processor?.Health !== "OK" ||
        serverDetail.Processor?.ProcessorList?.some(
          (item) => item.Health !== "OK"
        )
      );

    case "STORAGE":
      return (
        serverDetail.Storage?.Drives?.some(
          (item) => item.Health !== "OK"
        )
      );

    case "COOLING":
      return serverDetail.Cooling?.some(
        (item) => item.Health !== "OK"
      );

    case "POWER_SUPPLY":
      return (
        serverDetail.PowerSupply?.Health !== "OK" ||
        serverDetail.PowerSupply?.PowerSupplyList?.some(
          (item) => item.Health !== "OK"
        )
      );

    case "NETWORK_DEVICES":
      return (
        serverDetail.EmbeddedNetworkCard?.Health !== "OK" ||
        serverDetail.IntegratedNetworkCard?.Health !== "OK" ||
        serverDetail.EmbeddedNetworkCard?.NetworkCardList?.some(
          (item) => item.health !== "OK"
        ) ||
        serverDetail.IntegratedNetworkCard?.NetworkCardList?.some(
          (item) => item.health !== "OK"
        )
      );

    default:
      return false;
  }
};
const currentServerHealth =
  serverHealth?.find(
    (item) => item.ServerId === server.id
  );

const showServerWarning =
  currentServerHealth?.Health !== "OK";

  return (
    <Box className="reports-tab idrac-right"> 
    {/* ── Detail Header ── */}
      <Box
      className="idrav-right-head"
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexWrap: 'wrap',
          gap: 1,
          px: 2,
          py: 1,         
          backgroundColor: '#fff',
          position: 'sticky',
          top: 0,
          zIndex: 10,
        }}
      >
        {/* Left: server name / ip / system info button */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
          <Typography sx={{ fontWeight: 700, fontSize: '14px', color: '#111' }}>
            {server.serverName ?? 'SRV-01'} /
          </Typography>
          <Typography sx={{ fontSize: '12px', color: '#555' }}>
            {server.ipAddress}:{server.port}
          </Typography>
          {showServerWarning  && (
            <img
              src="/images/iDRAC/Alert.gif"
              alt="Alert"
              style={{
                width: 20,
                height: 20,
                objectFit: "contain",
              }}
            />
          )}
         
            <Box
              component="button"
              className='idrac-system-btn'
              onClick={() => setOpenSytemInfoDialog(true)}
            >
              <img src={"/images/iDRAC/keyboard-open.svg"}
                alt="Alert"
                style={{
                  width: "20px",
                  height: "20px",
                  objectFit: "contain",
                }} />
             {t("IDRAC_Server_Details.System_Information")}
            </Box>
        
        </Box>

      
     {/* Right: action buttons */}
        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>

          {/* BULB BUTTON */}
             
          {HasPermission(LABELS.IdracServerLedIndicator) && (
          <Box
            component="button"
            onClick={handleToggleLED}
            className={`idrac-action-btn ${isLedOn ? 'active-bulb' : ''
              }`}
          >           
              <img
                src={
                  isLedOn
                    ? '/images/iDRAC/bulb-on.svg'
                    : '/images/iDRAC/bulb-off.svg'
                }
                alt="bulb"
                style={{
                  width: '18px',
                  height: '18px',
                  objectFit: 'contain',
                }}
              />
        
          </Box>
          )}

          {/* POWER BUTTON */}
           {HasPermission(LABELS.IdracServerPowerAction) && (
          <Box
            component="button"
            onClick={openPowerMenu}
            className='idrac-action-btn'
          >
            <img
              src="/images/iDRAC/power-btn.svg"
              alt="power"
              style={{
                width: '18px',
                height: '18px',
                objectFit: 'contain',
              }}
            />
          </Box>
           )}

          {/* REMOTE CONSOLE */}
            {HasPermission(LABELS.IdracServerLaunchRemoteConsole) && (
          <Box
            component="button"
            onClick={handleLaunchRemoteConsole}
            className='idrac-action-btn'
          >
            <img
              src="/images/iDRAC/launch-remote.svg"
              alt="remote"
              style={{
                width: '18px',
                height: '18px',
                objectFit: 'contain',
              }}
            />

            <span><span>{t("IDRAC_Server_Details.Launch_Remote_Console")}</span></span>
          </Box>
            )}

          {/* EVENT LOGS */}
          {HasPermission(LABELS.ViewEventLogsIdracServers) && (
          <Box
            component="button"
            onClick={() => setEventLogOpen(true)}
            className='idrac-action-btn'
          >
            <img
              src="/images/iDRAC/checklist.svg"
              alt="event logs"
              style={{
                width: '18px',
                height: '18px',
                objectFit: 'contain',
              }}
            />

            <span>{t("IDRAC_Server_Details.Event_Logs")}</span>
          </Box>
          )}

          {/* SYSTEM LOGS */}
          {HasPermission(LABELS.ViewSystemLogsIdracServers) && (
          <Box
            component="button"
            onClick={() => setSystemLogOpen(true)}
            className='idrac-action-btn'
          >
            <img
              src="/images/iDRAC/checklist.svg"
              alt="system logs"
              style={{
                width: '18px',
                height: '18px',
                objectFit: 'contain',
              }}
            />

            <span>{t("IDRAC_Server_Details.System_Logs")}</span>
          </Box>
          )}
        </Box>
      </Box>
    <div className="server-detail-tab ">
      {/* ── Tab Bar ── */}
        <Box className="server-tabs-wrapper">
          {TABS.map((tab) => (
            <Box
              key={tab.key}
              component="button"
              onClick={() => setActiveTab(tab.key)}
              className={`server-tab-button ${activeTab === tab.key ? "active" : ""
                }`}
            >
              <Box
                component="span"
                className="server-tab-icon"
              >
                <img
                  src={tab.icon}
                  alt={tab.label}
                  style={{
                    width: 24,
                    height: 24,
                    objectFit: "contain",
                  }}
                />
              </Box>

              <Box
                sx={{
                  display: "flex",
                  alignItems: "center",
                  gap: "6px",
                }}
              >
                <span>{tab.label}</span>

                {hasWarning(tab.key) && (
                  <img
                    src="/images/iDRAC/Alert.gif"
                    alt="Alert"
                    style={{
                      width: "18px",
                      height: "18px",
                      objectFit: "contain",
                    }}
                  />
                )}
              </Box>
            </Box>
          ))}
        </Box>



    {/* ── Tab Content — single big bordered box ── */}
    <Box>

      {activeTab === "MEMORY" && (
        <MemoryTab memoryDetail={serverDetail?.Memory} server= {server} />
      )}

      {activeTab === "PROCESSOR" && (
        <ProcessorTab processorDetail={serverDetail?.Processor} server= {server} />
      )}

      {activeTab === "STORAGE" && (
        <StorageTab storageDetail={serverDetail?.Storage} />
      )}

      {activeTab === "COOLING" && (
        <CoolingTab coolingDetail={serverDetail?.Cooling} server= {server} />
      )}

      {activeTab === "POWER_SUPPLY" && (
        <PowerSupplyTab PowerSupply={serverDetail?.PowerSupply} />
      )}

      {activeTab === "NETWORK_DEVICES" && (
        <NetworkDevicesTab
          EmbeddedNetworkDetail={serverDetail?.EmbeddedNetworkCard}
          IntegratedNetworkDetail={serverDetail?.IntegratedNetworkCard}
          server= {server}
        />
      )}

    </Box>

    </div>
     {eventLogOpen && server && (
          <EventLogDialog
            open={eventLogOpen}
            onClose={() => setEventLogOpen(false)}
            serverIP={server.ipAddress}
          />
        )}
      
       {systemLogOpen && server && (
          <SystemLogDialog
            open={systemLogOpen}
            onClose={() => setSystemLogOpen(false)}
            serverID={server.id}
          />
        )}
      
     {openSytemInfoDialog && server && (
          <SystemInfoDialog
            open={openSytemInfoDialog}
            onClose={() => setOpenSytemInfoDialog(false)}
            serverIP={server.ipAddress}
          />
        )}

      <Menu
        anchorEl={powerAnchorEl}
        open={Boolean(powerAnchorEl)}
        onClose={closePowerMenu}  
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
        <MenuItem
          onClick={() => handlePowerAction('ForceRestart')}       
        >
         <ListItemText primary={"Force Restart "} />
        </MenuItem>

        <MenuItem
          onClick={() => handlePowerAction('ForceOff')}        
        >
          <ListItemText primary={"Force Shutdown"} />
        </MenuItem>

        <MenuItem
          onClick={() => handlePowerAction('ForceOn')}         
        >
         <ListItemText primary={"Force Start"} />
        </MenuItem>
      </Menu>
    </Box>
  );
};

export default ServerDetails;