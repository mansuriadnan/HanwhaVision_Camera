import React from "react";
import { Box, Typography } from "@mui/material";
import CameraPieChart from "./CameraPieChart";
import { useTranslation } from "react-i18next";

interface Props {
  total: number;
  online: number;
  offline: number;
}

const CameraUsageCard: React.FC<Props> = ({
  total,
  online,
  offline,
}) => {
  const onlinePercent = total > 0
  ? Math.round((online / total) * 100)
  : 0;
  const { t } = useTranslation();

  return (
    <Box className="ssm-pop-card--inner-main">
      {/* TOP SECTION */}
        <Box  className="ssm-pop-card-wrapper">
        
        {/* LEFT */}
        <Box className="ssm-pop-card-left">
          <Box className="ssm-pop-card-inner-head">
            <Typography >
             {t("SSM_Chart_Dialog.Camera_Online_offline")} 
            </Typography>

            <Typography variant="h2">
              {online}/{offline}
            </Typography>
          </Box>

          <Box className="ssm-pop-card-inner-details">
            <Typography>
              {t("SSM_Chart_Dialog.Camera_Working")}
            </Typography>

            <Typography variant="h2">
              {onlinePercent}%
            </Typography>
          </Box>
        </Box>

      

        {/* RIGHT PIE */}
        <Box className="ssm-pop-card-right">
            <CameraPieChart
                data={[
                    { label:  t("SSM_Chart_Dialog.Online"), value: online, color: "#3BA66B" },
                    { label:  t("SSM_Chart_Dialog.Offline"), value: offline, color: "#FFC107" },
                ]}
                size={120}
                innerRadius={0}
                total={total}
            />
        
          {/* LEGEND */}
          <Box
            sx={{
              display: "flex",
              justifyContent: "center",
              gap: 2,
              mt: 1,
              fontSize: 12,
            }}
          >
            <Box display="flex" alignItems="center" gap={0.5}>
              <Box
                sx={{
                  width: 8,
                  height: 8,
                  borderRadius: "50%",
                  backgroundColor: "#3BA66B",
                }}
              />
              {t("SSM_Chart_Dialog.Camera_On")}
            </Box>

            <Box display="flex" alignItems="center" gap={0.5}>
              <Box
                sx={{
                  width: 8,
                  height: 8,
                  borderRadius: "50%",
                  backgroundColor: "#FFC107",
                }}
              />
              {t("SSM_Chart_Dialog.Camera_Off")}
            </Box>
          </Box>
        </Box>
      </Box>
    </Box>
  );
};

export default CameraUsageCard;