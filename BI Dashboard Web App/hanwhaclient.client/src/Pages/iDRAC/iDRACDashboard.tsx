import { Box, Typography } from '@mui/material';
import React, { useEffect, useState } from 'react'
import { CustomButton, CustomSelect } from '../../components';
import { GetAllSiteService } from "../../services/siteManagementService";
import { useForm } from 'react-hook-form';
import { IserverPayload, IserverData, IReferenceData, IserverHealth, IdracDashboardModel } from '../../interfaces/IManageiDRAC';
import { fetchiDRACServerListService } from '../../services/iDRACService';
import InfoOutlinedIcon from "@mui/icons-material/InfoOutlined";
import ServerDetails from './ServerDetails';
import ServerList from './ServerList';
import { fetchEventSource } from '@microsoft/fetch-event-source';
import apiUrls from '../../constants/apiUrls';
import { refreshAccessToken } from '../../services/authService';
import { useAuth } from '../../hooks/useAuth';
import { useThemeContext } from '../../context/ThemeContext';
import { useTranslation } from 'react-i18next';
import { BorderBottom } from '@mui/icons-material';

interface HeaderInputs {
  siteId: string;
}


const iDRACDashboard = () => {
  const [siteList, setSiteList] = useState<any[]>([]);
  const [serverList, setServerList] =useState<IserverData[]>([]);
  const [referenceData, setReferenceData] = useState<IReferenceData>();
  const [selectedServer, setSelectedServer] = useState<any>(null);
  const [serverHealth, setServerHealth] = useState<IserverHealth[]>([]);
  const [serverDetail, setServerDetail] = useState<IdracDashboardModel | null>(null);
  const [LEDIndicator, setLEDIndicator] = useState<boolean>(false);
  const { handleLogout } = useAuth();  
  const { theme, themeColor } = useThemeContext();
  const { t } = useTranslation();
  
    const themeColorPath =
      themeColor === "default-theme"
        ? theme === "dark" ? "dark-theme/" : ""
        : theme === "dark" ? `${themeColor}/dark-theme/` : `${themeColor}/`;


    const {
      control,
      setValue,
      handleSubmit,
      watch,
      formState: { errors },
    } = useForm<HeaderInputs>({
      defaultValues: {
        siteId: "",
      },
    });
      const selectedSiteId = watch("siteId");
    

  useEffect(() => {
    GetAllSite();
  }, []);

  
 useEffect(() => {
    if (!selectedServer) return;

    let token = localStorage.getItem("accessToken");
    const controller = new AbortController();

    const connect = async () => {
      await fetchEventSource(
        `${apiUrls.stream}?serverId=${selectedServer.id}&siteId=${selectedServer.parentSiteId}`,
        {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
            Accept: "text/event-stream",
          },
          signal: controller.signal,
          openWhenHidden: true,
          async onopen(response) {
            if (response.status === 401) {
            // ── same as apiGet 401 block: refresh token then retry ──────────
            token = await refreshAccessToken();
            if (!token) {
              // refresh failed (refreshToken expired) → logout
              handleLogout();
              controller.abort();
              return;
            }

            // reconnect with the new token (same as apiGet re-fetches)
            controller.abort();
            connect();
            return;
          }

            if (!response.ok) throw new Error("SSE CONNECTION FAILED");
          },
          onmessage(event) {
            try {
              const parsed: any = JSON.parse(event.data);
              setServerDetail(parsed.IdracDetails as IdracDashboardModel);
              setServerHealth(parsed.ServerHealth);
              setLEDIndicator(parsed.LightIndigator);
            } catch (error) {
              console.error("PARSE ERROR", error);
            }
          },
          onclose() { console.log("STREAM CLOSED"); },
          onerror(error) {
            console.error("SSE ERROR:", error);
            throw error;
          },
        }
      );
    };

    connect();
    return () => controller.abort();
  }, [selectedServer]); // ← re-runs when server selection changes

  // ── Clear detail on server/site change ──
  const handleSelectServer = (srv: any) => {
    // setServerDetail(null);   // clear stale data immediately
    setSelectedServer(srv);
  };

  useEffect(() => {
    setSelectedServer(null);
    setServerList([]);
    setReferenceData(undefined);
    setServerDetail(null);   // ← also clear on site change
    setServerHealth([]);
  }, [selectedSiteId]);

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

    const onSearch = async (data: HeaderInputs) => {
     await fetchData(data.siteId);    
    };
  
    const fetchData = async (siteId: string) => {
    try {
        const serverData = {
          parentSiteId: siteId,
        };
    
        const response: any = await fetchiDRACServerListService(
          serverData as IserverPayload
        );
     
   
        if(response.isSuccess){
      
        setServerList(response.data);
        setReferenceData(response.referenceData)
          if (response.data?.length > 0) {
            handleSelectServer(response.data[0]);
          }
        }
        
      } catch (error) {
        console.error(
          "Error fetching in SSM site data:",
          error
        );
        throw error;
      } 
  
    }
  

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

  return (
    <>
     <Box className="dashbourd-retail-details-head">
        <div className="dashbourd-retail-details-floor">         
          
          <CustomSelect
            name="siteId"
            label={
              <span>
                Select Site<span className="star-error">*</span>
              </span>
            }
            control={control}
            options={siteList}
            rules={{ required: "Site is required" }}
            placeholder="Select site"
            disableChildrenOnly = {true}
            
          />
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
            onClick={handleSubmit(onSearch)}
          >
            <img src={"/images/reload.svg"} alt="Reload SSM Server" />
            {/* {t("Manage_Device.Reload_btn")} */}
          </CustomButton>
        </div>
      </Box>

      
      <Box className='floor-plans-zones-wrapper-main floor-plans-zones-wrapper-main-idrac'>
        <div className="floor-plans-zones">

          {/* Server List */}
          <ServerList
            server={serverList}
            referenceData={referenceData}
            selectedServer={selectedServer}
            onSelectServer={handleSelectServer}
            serverHealth={serverHealth}

          />

          {selectedServer ? (
            <ServerDetails 
              server={selectedServer} 
              serverHealth={serverHealth}
              setServerHealth={setServerHealth}
              serverDetail={serverDetail} 
              LEDIndicator = {LEDIndicator}
            />
          ) : (
            <div className="roles-permissions-tab empty-report-tab" >
               <Box className="memory-tab-wrapper no-data-found-idrac" sx={{border:'none',borderBottom: '0px !important'}}>
                <Box className="no-data-douns">
                  <Box sx={{ width: 200, justifyItems: "center", flex: 1 }}>
                    <img src={`/images/${themeColorPath}noData.gif`} alt="Animated GIF" width="100" height="100" />
                    <Typography sx={{ fontWeight: 600, fontSize: 24, color: "#090909" }}>
                      {t("No_data_found")}
                    </Typography>
                  </Box>
                </Box>
              </Box>             
            </div>
          )}
        </div>
      </Box>
        </>
  )
}

export default iDRACDashboard