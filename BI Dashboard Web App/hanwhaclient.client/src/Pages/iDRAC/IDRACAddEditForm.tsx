import React, { useEffect, useState } from 'react'
import { SubmitHandler, useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { AddiDRACService } from '../../services/iDRACService';
import { Box } from '@mui/material';
import Grid from 'antd/es/card/Grid';
import { CustomButton, CustomSelect, CustomTextField } from '../../components';
import { COMMON_CONSTANTS, REGEX } from '../../utils/constants';
import { GetAllSiteService } from '../../services/siteManagementService';
import { IiDRACServer } from '../../interfaces/IManageiDRAC';

interface iDRACAddEditFormProps {
  onClose: () => void;
  server?: IiDRACServer;
  refreshData: () => void;
}

interface iDRACFormInputs {
  serverName : string;
  IPAddress: string;
  port: string;
  userName: string;
  password: string;
  siteId: string
}

const IDRACAddEditForm : React.FC<iDRACAddEditFormProps> = ({
  onClose,
  server,
  refreshData,
}) => {
const [siteList, setSiteList] = useState<any[]>([]);
// Add these to your state declarations at the top of the component:
const [cpuLoad, setCpuLoad] = useState<number>(75);
const [temperature, setTemperature] = useState<number>(75);
const [memoryUsage, setMemoryUsage] = useState<number>(75);
const [initialTelemetry, setInitialTelemetry] = useState({
cpuLoad: 75,
temperature: 75,
memoryUsage: 75,
});
const isEditMode = server !== null && server !== undefined;
const { t } = useTranslation();
      const {
        control,
        setValue,
        handleSubmit,
        watch,
        reset,
        formState: { isDirty },
      } = useForm<iDRACFormInputs>({
          defaultValues: {
              serverName: "",
              IPAddress: "",
              port: "",
              userName: "",
              password: "",
              siteId: ""
          },
      });

        const isTelemetryDirty =
            cpuLoad !== initialTelemetry.cpuLoad ||
            temperature !== initialTelemetry.temperature ||
            memoryUsage !== initialTelemetry.memoryUsage;

      useEffect(() => {
        GetAllSite();
      }, []);

      useEffect(() => {
          const initializeData = async () => {
            
              if (isEditMode) {
                const initialServer = server;
                setValue("serverName", initialServer?.serverName || "");
                setValue("IPAddress", initialServer?.ipAddress || "");
                setValue("port", initialServer?.port || "");
                setValue("userName", initialServer?.userName || "");
                setValue("password", initialServer?.password || "");
                const selectedSiteId =
                    server.childSiteId && (server.childSiteId !== "" || server.childSiteId !== null )
                        ? server.childSiteId
                        : server.parentSiteId;

                  setValue("siteId", selectedSiteId || "");
                  const telemetry = {
                      cpuLoad: initialServer?.cpuLoad ?? 75,
                      temperature: initialServer?.temperature ?? 75,
                      memoryUsage: initialServer?.memoryUsage ?? 75,
                  };


                  setInitialTelemetry(telemetry);


                  setCpuLoad(telemetry.cpuLoad);
                  setTemperature(telemetry.temperature);
                  setMemoryUsage(telemetry.memoryUsage);
              }
            
          };
      
          initializeData();
        }, [isEditMode, server]);
    
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
    
      const getDropdownOptions = (data: any) => {
          const options: any = [];
  
          data.forEach((parent: any) => {
              // Parent
              options.push({
                  title: parent.siteName,
                  id: parent.id,
                  isParent: true,
              });
  
              // Children (with indentation)
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

    const handleAddiDRAC: SubmitHandler<iDRACFormInputs> = async (formData) => {
        try {
            //  Find selected site
            const selectedSite = siteList.find(
                (site: any) => site.id === formData.siteId
            );

            // Extract parent & child IDs
            const parentSiteId = selectedSite?.isParent
                ? selectedSite.id
                : selectedSite?.parentId;

            const childSiteId = selectedSite?.isParent
                ? null
                : selectedSite?.id;

            
            const iDRACdata: any = {
                serverName: formData.serverName,
                ipAddress: formData.IPAddress, 
                port: formData.port,
                username: formData.userName,
                password: formData.password,
                parentSiteId,
                childSiteId,
                cpuLoad: cpuLoad,
                temperature: temperature,
                memoryUsage: memoryUsage,
                ...(isEditMode && server?.id && { id: server.id }),
            };
          
            const response: any = await AddiDRACService(iDRACdata);

            if (response && response?.isSuccess) {
                reset();
                onClose();
                refreshData?.();
            }

        } catch (err) {
            console.error("Error saving server:", err);
        }
    };

 return (
    <div className="cmn-pop-form">
      <div className="cmn-pop-form-wrapper">
        <Box
          component="form"
          onSubmit={handleSubmit(handleAddiDRAC)}
          noValidate
        >
          <Grid className="cmn-pop-form-inner">    

            <CustomTextField
                name="serverName"
                label={
                    <span>
                        {"Server Name"}{" "}
                        <span className="star-error">*</span>
                    </span>
                }
                control={control}
                rules={{
                    required: "Server name is required.",
                    // pattern: {
                    //     value: REGEX.IPAddess_Regex,
                    //     message: "Enter a valid Ip Address",
                    // },
                }}
                placeholder={"Enter server name"}
                required
                fullWidth
            />                               

            <CustomTextField
                name="IPAddress"
                label={
                    <span>
                        {t("Manage_Device.Add_Edit_Device_Drawer.IP_Address")}{" "}
                        <span className="star-error">*</span>
                    </span>
                }
                control={control}
                rules={{
                    required: t("Manage_Device.validation.IP_Address_required"),
                    pattern: {
                        value: REGEX.IP_Domain_Regex,
                        message: t(
                            "Manage_Device.validation.IP_Address_strong_validation"
                        ),
                    },
                }}
                placeholder={t(
                    "Manage_Device.Add_Edit_Device_Drawer.IP_Address_placeholder"
                )}
                required
                fullWidth
            />
            
            <CustomTextField
                name="port"
                label={
                    <span>
                        {t("Manage_Device.Add_Edit_Device_Drawer.Port")}{" "}
                        <span className="star-error">*</span>
                    </span>
                }
                control={control}
                rules={{
                    required: t("Manage_Device.validation.Port_required"),
                    pattern: {
                        pattern: {
                            value: REGEX.DigitsOnly_Regex,
                            message: t(
                                "Manage_Device.validation.Port_strong_validation"
                            ),
                        },
                    },
                }}
                placeholder={t(
                    "Manage_Device.Add_Edit_Device_Drawer.Port_placeholder"
                )}
                required
                defaultValue={"80"}
                fullWidth
                type="number"
            />

            <CustomTextField
                name="userName"
                label={<span>{t("Manage_User.Add_Edit_User_Drawer.Username")} <span className="star-error">*</span></span>}
                control={control}
                rules={{
                    required: t("Manage_User.Validation.Username_Required"),
                    pattern: {
                        value: REGEX.UserName_Regex,
                        message: t("Manage_User.Validation.Username_Valid"),
                    },
                    maxLength: {
                        value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                        message: t("Manage_User.Validation.Username_Strong_Validation", { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                    },
                }}
                placeholder={t("Manage_User.Add_Edit_User_Drawer.Username_Placeholder")}
                required
                fullWidth
            />

                     <CustomTextField
                         name="password"
                         label={
                             <span>
                                 {t("Manage_Device.Add_Edit_Device_Drawer.Password")}{" "}
                                 <span className="star-error">*</span>
                             </span>
                         }
                         control={control}  
                         type={"password"}
                         rules={{
                             required: t("Manage_Device.validation.Password_required"),
                         }}
                         placeholder={t("Manage_User.Add_Edit_User_Drawer.Password_Placeholder")}
                         required={!isEditMode}
                         fullWidth
                     />
            
            <CustomSelect
                name="siteId"
                label={
                    <span>
                        Select Site<span className="star-error">{" "}*</span>
                    </span>
                }
                control={control}
                options={siteList}
                rules={{ required: "Site is required" }}
                placeholder="Select site"
            />
          
            <Box className="place-floor-buttons-bar" sx={{maxWidth:'100%'}}>
                <h4 style={{ fontSize: '14px', fontWeight: 600, marginBottom: '12px', color: '#333' }}>
                    Set Telemetry
                </h4>

                {/* CPU Load */}
                <div style={{ marginBottom: '12px' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <label htmlFor="cpuLoad">CPU Load</label>
                        <label htmlFor="cpuLoad" id="cpuLoadValue" className='idrac-form-value-label'>{cpuLoad} %</label>
                    </div>
                    <input
                        id="cpuLoad"
                        type="range"
                        min="0"
                        max="100"
                        value={cpuLoad}
                        onChange={(e) => setCpuLoad(Number(e.target.value))}
                    />
                </div>

                {/* Temperature */}
                <div style={{ marginBottom: '12px' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <label htmlFor="temperature">Temperature</label>
                        <label htmlFor="temperature" id="temperatureValue" className='idrac-form-value-label'>{temperature} °C</label>
                    </div>
                    <input
                        id="temperature"
                        type="range"
                        min="0"
                        max="100"
                        value={temperature}
                        onChange={(e) => setTemperature(Number(e.target.value))}
                    />
                </div>

                {/* Memory Usage */}
                <div style={{ marginBottom: '12px' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <label htmlFor="memoryUsage">Memory Usage</label>
                        <label htmlFor="memoryUsage" id="memoryUsageValue" className='idrac-form-value-label'>{memoryUsage} %</label>
                    </div>
                    <input
                        id="memoryUsage"
                        type="range"
                        min="0"
                        max="100"
                        value={memoryUsage}
                        onChange={(e) => setMemoryUsage(Number(e.target.value))}
                    />
                </div>
            </Box>
            <CustomButton fullWidth className="common-btn-design" disabled={isEditMode && !isDirty && !isTelemetryDirty}>
              {isEditMode ? t("Save_btn") : t("Add_btn")}
            </CustomButton>

          </Grid>
        </Box>
      </div>
    </div>
  );
}
export default IDRACAddEditForm