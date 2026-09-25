import React, { useEffect, useState } from 'react'
import { SubmitHandler, useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { AddAlarmService, AddiDRACService } from '../../services/iDRACService';
import { Box } from '@mui/material';
import Grid from 'antd/es/card/Grid';
import { CustomButton, CustomSelect, CustomTextField } from '../../components';
import { COMMON_CONSTANTS, REGEX } from '../../utils/constants';
import { GetAllSiteService } from '../../services/siteManagementService';
import { IalarmPayload, IiDRACServer } from '../../interfaces/IManageiDRAC';

interface iDRACAddEditFormProps {
  onClose: () => void;
  server?: IiDRACServer;
  refreshData: () => void;
}

interface iDRACFormInputs {
  name : string;
  event: string;
  limit: number | null;
}

const IDRACAddAlarmForm : React.FC<iDRACAddEditFormProps> = ({
  onClose,
  server,
  refreshData,
}) => {

const { t } = useTranslation();
      const {
        control,
        setValue,
        handleSubmit,
        watch,
        reset,
        formState: { errors },
      } = useForm<iDRACFormInputs>({
          defaultValues: {
              name: "",
              event: "",
              limit: null,
          }
       
      });

    const eventList = [
        {
            title: "CPU Load Above Threshold",
            id: "CPU",
        },
        {
            title: "Fan Alarm",
            id: "Fan",
        },
        {
            title: "HDD Alarm",
            id: "HDD",
        },
        {
            title: "Memory Load Above Threshold",
            id: "Memory",
        },
        {
            title: "NIC Alarm",
            id: "NIC",
        },
        {
            title: "Power Supply Alarm",
            id: "Power",
        },
        {
            title: "Temperature Above Threshold",
            id: "Temperature",
        },
    ];

    const timeLimitList = [
        {
            title: "05 seconds",
            id: 5,
        },
        {
            title: "10 seconds",
            id: 10,
        },
         {
            title: "15 seconds",
            id: 15,
        },
        {
            title: "20 seconds",
            id: 20,
        },
    ]
    
 
    const handleAddAlarm: SubmitHandler<iDRACFormInputs> = async (formData) => {
        try {
           if (!server) return;
            
            const alarmData: IalarmPayload = {
                name: formData.name,
                event: formData.event, 
                timeLimit: formData.limit,
                userName : server?.userName,
                password : server?.password,
                serverId : server?.id,
                ipAddress : server?.ipAddress
            };
          
            const response: any = await AddAlarmService(alarmData);

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
          onSubmit={handleSubmit(handleAddAlarm)}
          noValidate
        >
          <Grid className="cmn-pop-form-inner">    

            <CustomTextField
                name="name"
                label={
                    <span>
                        {t("IDRAC_Server.Add_Alarm_Form.Name")}{" "}
                        <span className="star-error">*</span>
                    </span>
                }
                control={control}
                rules={{
                     required: t("IDRAC_Server.Add_Alarm_Form.Name_Required"),
                    // pattern: {
                    //     value: REGEX.IPAddess_Regex,
                    //     message: "Enter a valid Ip Address",
                    // },
                }}
                placeholder={t("IDRAC_Server.Add_Alarm_Form.Enter_Name")}
                required
                fullWidth
            />                               

            <CustomSelect
                name="event"
                variant="filled"
                control={control}
                label={
                    <span>
                         {t("IDRAC_Server.Add_Alarm_Form.IDRAC_Event")}{" "}
                        <span className="star-error">*</span>
                    </span>
                }
                options={eventList}
                rules={{ required: t("IDRAC_Server.Add_Alarm_Form.IDRAC_Event_Required")}}
                required
                placeholder={t("IDRAC_Server.Add_Alarm_Form.Select_IDRAC_Event")}
            />
            
            <CustomSelect
                name="limit"
                variant="filled"
                control={control}
                label={
                    <span>
                         {t("IDRAC_Server.Add_Alarm_Form.Time_Limit")}{" "}
                        <span className="star-error">*</span>
                    </span>
                }
                options={timeLimitList}
                rules={{ required: t("IDRAC_Server.Add_Alarm_Form.Time_Limit_Required"), }}
                required
                placeholder={t("IDRAC_Server.Add_Alarm_Form.Select_Time_Limit")}
            />
         
          
            <CustomButton fullWidth className="common-btn-design">
                {t("Add_btn")}
            </CustomButton>

          </Grid>
        </Box>
      </div>
    </div>
  );
}
export default IDRACAddAlarmForm