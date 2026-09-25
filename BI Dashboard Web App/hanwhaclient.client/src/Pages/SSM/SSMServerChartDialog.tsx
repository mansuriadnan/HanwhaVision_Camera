import React, { useEffect, useState } from "react";
import { Dialog, DialogContent, DialogTitle, IconButton, Box, Typography } from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import AvailabilityChart from "./AvailabilityChart";
import CpuUsageCard from "./CpuUsageCard";
import RamUsageCard from "./RamUsageCard";
import CameraUsageCard from "./CameraUsageCard";
import { CpuUsageResponse, ISSMCPUPayload, RamUsageResponse, SsmServerAvailabilityResponse } from "../../interfaces/IManageServer";
import { GetCPUUtilizationService, GetRAMUtilizationService, serverAvailabilityService } from "../../services/SSMService";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { useTranslation } from "react-i18next";

interface Props {
  open: boolean;
  onClose: () => void;
  server: any;
  selectedDate : string | null;
}

const SSMServerChartDialog: React.FC<Props> = ({ open, onClose, server,selectedDate }) => {
  const [CPUdata, setCPUdata] = useState<CpuUsageResponse | null>(null);
  const [RAMdata, setRAMdata] = useState<RamUsageResponse | null>(null);
  const [availabilityData, setAvailabilityData] = useState<SsmServerAvailabilityResponse[]>([]);
  const { t } = useTranslation();
  

  useEffect(() => {
    if(server !== null){
    fetchSSMCPUData();
    fetchSSMRAMData();
    fetchAvailabilityData();
    }
  }, [server]);
  
  const fetchSSMCPUData = async () => {
    try {
      let request: ISSMCPUPayload = {
        serverId: server?.id,
        date: selectedDate
      };
      const ssmCPUData: any = await GetCPUUtilizationService(request);

      if (ssmCPUData && ssmCPUData?.isSuccess && ssmCPUData?.data) {
        setCPUdata(ssmCPUData?.data);
      } else {
        setCPUdata(null);
      }
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  }

  const fetchSSMRAMData = async () => {
    try {
      let request: ISSMCPUPayload = {
        serverId: server.id,
        date: selectedDate
      };
      const ssmRAMData: any = await GetRAMUtilizationService(request);

      if (ssmRAMData && ssmRAMData?.isSuccess && ssmRAMData?.data) {
        setRAMdata(ssmRAMData?.data);
      } else {
        setRAMdata(null);
      }
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  }

  const fetchAvailabilityData = async () => {
  try {
    const response: any = await serverAvailabilityService({
      serverId: server.id,
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
    <Dialog 
    open={open} 
    maxWidth="xl" 
    className="inner-cmn-pop-design ssm-serve-pop"
    fullWidth  
    disableEscapeKeyDown
    onClose={(_, reason) => {
      if (reason === "backdropClick") return;

      onClose();
    }}>      
   
      <DialogTitle className="inner-pop-head">
        {server?.name}
        <IconButton onClick={onClose}>
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent>
        <Box className="availability-chart" sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 1 }}>

          <Typography  variant="h4" sx={{ fontSize: "22px", fontWeight: 600, color: "#090909" }}>
            {t("SSM_Chart_Dialog.Availability_Chart")}
          </Typography>

          <Box className="availability-chart" >
            <Box className="server-on-off">
              <Box className="green-dot"/>
              <Box>{t("SSM_Chart_Dialog.Server_On")}</Box>
            </Box>
            <Box className="server-on-off">
              <Box className="red-dot"/>
              <Box>{t("SSM_Chart_Dialog.Server_Off")}</Box>
            </Box>
          </Box>
        </Box>
        <Box>               
          {/* Top Availability Bar */}
          <AvailabilityChart
              availabilityData={availabilityData}
              startDate={startDate}
              endDate={endDate}
              customizedWidth={1470}
              customizedHeight={120}
          />

          
          
            {/* Horizontal Divider */}
            <Box
              style={{
                borderBottom: "1px solid",
                borderImageSource: "linear-gradient(90deg, rgba(205, 205, 205, 0) 0%, #CDCDCD 12.98%, #CDCDCD 86.06%, rgba(205, 205, 205, 0) 100%)",
                borderImageSlice: 1,  // ← required for border-image to render
              }}
              sx={{ my: 2, mb: 5 }}
            />

          {/* Cards Section */}
          <Box className="ssm-pop-card-main">
            
            {/* Camera Card */}
            <Box className="ssm-pop-card-repeat">
              <CameraUsageCard
                total={server?.cameras}
                online={server?.online}
                offline={server?.offline}
              />
            </Box>

            {/* CPU Card */}
            <Box className="ssm-pop-card-repeat">
            
            <CpuUsageCard
              used={
                  CPUdata?.chartData?.length
                    ? +CPUdata.chartData[CPUdata.chartData.length - 1].totalUsage
                    : 0
                }
              breakdown={[
                { label: t("SSM_Chart_Dialog.System_Manager"), value: CPUdata?.cpuSystemUsage || 0,color: "#ACDB03" },

                { label: t("SSM_Chart_Dialog.Media_Server"), value:  CPUdata?.cpuMediaUsage || 0, color: "#FFD000"  },
              ]}
              lineChart={CPUdata?.chartData}
              startDate = {startDate}
              endDate = {endDate}
            />
            </Box>

            {/* RAM Card */}
            <Box className="ssm-pop-card-repeat">
              <RamUsageCard
                used={
                  RAMdata?.chartData?.length
                    ? +RAMdata.chartData[RAMdata.chartData.length - 1].totalUsage
                    : 0
                }
                breakdown={[
                  { label:  t("SSM_Chart_Dialog.System_Manager"), value: RAMdata?.memorySystemUsage || 0,color:"#4251FF" },

                  { label: t("SSM_Chart_Dialog.Media_Server"), value: RAMdata?.memoryMediaUsage || 0,color: "#FFA600" },

                ]}
                lineChart={RAMdata?.chartData}
                startDate = {startDate}
                endDate = {endDate}
              />
            </Box>

          </Box>
        </Box>
      </DialogContent>
    </Dialog>
  );
};

export default SSMServerChartDialog;