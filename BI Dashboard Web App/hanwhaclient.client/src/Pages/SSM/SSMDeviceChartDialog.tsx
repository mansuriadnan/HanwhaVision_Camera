import React, { useEffect, useState } from "react";
import { Dialog, DialogContent, DialogTitle, IconButton, Box, Typography } from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import { SsmServerAvailabilityResponse } from "../../interfaces/IManageServer";
import { deviceAvailabilityService } from "../../services/SSMService";
import AvailabilityChart from "./AvailabilityChart";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { useTranslation } from "react-i18next";


interface Props {
  open: boolean;
  onClose: () => void;
  device: string;
  selectedDate: string | null; 
}

const SSMDeviceChartDialog: React.FC<Props> = ({ open, onClose, device, selectedDate }) => {
  const [availabilityData, setAvailabilityData] = useState<SsmServerAvailabilityResponse[]>([]);
  const { t } = useTranslation();


useEffect(() => {
  if (open && device) {
    fetchSSMDeviceAvailabilityData();
  }
}, [open, device]);

  const fetchSSMDeviceAvailabilityData = async () => {
    try {
      const response: any = await deviceAvailabilityService({
        deviceId: device,
        searchDate: selectedDate,
      });
      if (response?.isSuccess && response?.data) {
        setAvailabilityData(response.data);
      } else {
        setAvailabilityData([]);
      }
    } catch (err) {
      console.error("Error fetching availability data:", err);
      setAvailabilityData([]);
    }
  };

  const startDate = new Date(formatDateToConfiguredTimezone(selectedDate as string));
  startDate.setHours(0, 0, 0, 0);   // ✅ 00:00:00

  const endDate = new Date(formatDateToConfiguredTimezone(selectedDate as string));
  endDate.setHours(23, 59, 59, 999); // ✅ 23:59:59

  return (
    <Dialog className="inner-cmn-pop-design " open={open} onClose={onClose} maxWidth="xl" fullWidth >      
   
      <DialogTitle  className="inner-pop-head">
        {t("SSM_Chart_Dialog.Camera_Availability_Chart")}
        <IconButton onClick={onClose}>
          <CloseIcon />
        </IconButton>
      </DialogTitle>
      

      <DialogContent>
            <AvailabilityChart
                availabilityData={availabilityData}
                startDate={startDate}
                endDate={endDate}
                customizedWidth={1470}
                customizedHeight={120}
            />
                     
            <Box className="camera-availability-chart-online-offline-status">
  
              <Box className="camera-availability-chart-online-status">
                <Box className="green-dot" />
                <Typography variant="h4">{t("SSM_Chart_Dialog.Camera_On")}</Typography>
              </Box>
               <Box className="camera-availability-chart-online-status">
                <Box className="red-dot" />
                <Typography variant="h4">{t("SSM_Chart_Dialog.Camera_Off")}</Typography>
              </Box>
  
            </Box>
                 
      </DialogContent>
    </Dialog>
  );
};

export default SSMDeviceChartDialog;