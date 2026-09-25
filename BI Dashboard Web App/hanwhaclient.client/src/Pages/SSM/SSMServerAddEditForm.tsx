import { Box, Checkbox, FormControlLabel, Grid } from "@mui/material";
import React, { useEffect, useState } from 'react'
import { Controller, SubmitHandler, useForm } from 'react-hook-form';
import { CustomButton, CustomSelect, CustomTextField } from "../../components";
import { COMMON_CONSTANTS, REGEX } from "../../utils/constants";
import { useTranslation } from "react-i18next";
import { GetAllSiteService } from "../../services/siteManagementService";
import { AddSSMServerService } from "../../services/SSMService";

interface SSMServerAddEditFormProps {
  onClose: () => void;
  server?: any;
  refreshData: () => void;
}

interface SSMServerFormInputs {
  ipAddress: string;
  port: string;
  useHttps: boolean;
  username: string;
  password: string;
  siteId: string
}


const SSMServerAddEditForm : React.FC<SSMServerAddEditFormProps> = ({
  onClose,
  server,
  refreshData,
}) => {
const [siteList, setSiteList] = useState<any[]>([]);
const isEditMode = server !== null && server !== undefined;
const { t } = useTranslation();
      const {
        control,
        setValue,
        handleSubmit,
        watch,
        reset,
        formState: { isDirty },
      } = useForm<SSMServerFormInputs>({
          defaultValues: {
              ipAddress: "",
              port: "",
              useHttps : false,
              username: "",
              password: "",
              siteId: ""
          },
      });

    const useHttpsWatch = watch("useHttps");
    
      useEffect(() => {
        GetAllSite();
      }, []);
    
    useEffect(() => {
        setValue("port", useHttpsWatch ? "9991" : "9999");     
    }, [useHttpsWatch, setValue]);
      
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

  useEffect(() => {
    const initializeData = async () => {

      if (isEditMode) {
        const initialServer = server;
        setValue("ipAddress", initialServer?.ipAddress || "");
        setValue("port", initialServer?.port || "");
        setValue("useHttps", initialServer?.isHttps || false);
        setValue("username", initialServer?.username || "");
        setValue("password", initialServer?.password || "");
        const selectedSiteId =
          server.childSiteId && (server.childSiteId !== "" || server.childSiteId !== null)
            ? server.childSiteId
            : server.parentSiteId;

        setValue("siteId", selectedSiteId || "");

      }

    };

    initializeData();
  }, [isEditMode, server]);

    const handleAddServer: SubmitHandler<SSMServerFormInputs> = async (formData) => {
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

            
            const serverData: any = {
                ipAddress: formData.ipAddress, 
                port: formData.port,
                username: formData.username,
                password: formData.password,
                isHttps : formData.useHttps,
                parentSiteId,
                childSiteId,
                ...(isEditMode && server?.id && { id: server.id }),
            };          
            const response: any = await AddSSMServerService(serverData);

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
          onSubmit={handleSubmit(handleAddServer)}
          noValidate
        >
          <Grid className="cmn-pop-form-inner">                                   

            <CustomTextField
                name="ipAddress"
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
            <Box display="flex" gap={2}>
                          <Box flex={1}>
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
                              defaultValue={"9999"}
                              fullWidth
                              type="number"
                            />
                          </Box>
                          <Box
                            display="flex"
                            alignItems="center"
                            gap={2}
                            sx={{ marginTop: "25px" }}
                          >
                            {/* <FormControl component="fieldset" variant="filled" > */}
                            <Controller
                              name="useHttps"
                              control={control}
                              render={({ field }) => (
                                <FormControlLabel
                                  control={
                                    <Checkbox
                                      checked={field.value}
                                      onChange={() => setValue("useHttps", !field.value)}
                                    />
                                  }
                                  label={t(
                                    "Manage_Device.Add_Edit_Device_Drawer.Use_HTTPS"
                                  )}
                                />
                              )}
                            />
                            {/* </FormControl> */}
                          </Box>
                        </Box>

            <CustomTextField
                name="username"
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
                label={isEditMode ? t("Manage_User.Add_Edit_User_Drawer.Password") : <span>{t("Manage_User.Add_Edit_User_Drawer.Password")} <span className="star-error">*</span></span>}
                control={control}
                type={"password"}
                rules={{
                    required: t("Manage_User.Validation.Password_Required"),
                    maxLength: {
                        value: COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH,
                        message: t("Manage_User.Validation.Password_Max_Validation", { max: COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH }),
                    },
                }}
                placeholder={t("Manage_User.Add_Edit_User_Drawer.Password_Placeholder")}
                required
                fullWidth
            />
            
            <CustomSelect
                name="siteId"
                label={
                    <span>
                       {t("Manage_SSM.Select_Site")}<span className="star-error">*</span>
                    </span>
                }
                control={control}
                options={siteList}
                rules={{ required: t("Manage_SSM.Site_required")}}
                placeholder={t("Manage_SSM.Select_Site")}
            />

            <CustomButton fullWidth className="common-btn-design" disabled={isEditMode && !isDirty}>
              {isEditMode ? t("Save_btn") : t("Add_btn")}
            </CustomButton>

          </Grid>
        </Box>
      </div>
    </div>
  );
}

export default SSMServerAddEditForm