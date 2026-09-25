import { Box, Typography, CircularProgress } from "@mui/material";
import MiniLineChart from "./MiniLineChart";
import { useTranslation } from "react-i18next";

interface Props {
  used: number;
  breakdown: {
    label: string;
    value: number;
    color: string;
  }[];
  lineChart?:any;
  startDate : Date;
  endDate : Date;
}

const RamUsageCard: React.FC<Props> = ({ used, breakdown, lineChart, startDate, endDate  }) => {
  const { t } = useTranslation();
  
  return (
    <Box className="ssm-pop-card--inner-main">
      <Box  className="ssm-pop-card-wrapper">
        
        {/* LEFT */}
        <Box className="ssm-pop-card-left">
          <Box className="ssm-pop-card-inner-head">
            <Typography>{t("SSM_Dashboard.RAM")}</Typography>
            <Typography variant="h2">
              {t("SSM_Dashboard.Used")} {used}%
            </Typography>
          </Box>
          {breakdown.map((item, i) => {
            const color = (item.color);

            return (
              <Box className='ssm-pop-card-details' key={i} >
                <Typography>
                    {item.label.split(" ").join("\n")}
                </Typography>
                 
                <Box className='ssm-details-circular-progress' position="relative">
                  <CircularProgress
                    variant="determinate"
                    value={100}
                    size={35}
                    thickness={15}
                    sx={{ color: "#eee", position: "absolute" }}
                  />
                  <CircularProgress
                    variant="determinate"
                    value={item.value}
                    size={35}
                    thickness={15}
                    sx={{ color }}
                  />
                </Box>
                <Typography>
                  {item.value}<br/>% {t("SSM_Dashboard.Used")}
                </Typography>
              </Box>
            );
          })}
        </Box>
        {/* RIGHT BIG RADIAL */}
        <Box className="ssm-pop-card-right">
          {/* DONUT */}
          <Box position="relative">
            <CircularProgress
              variant="determinate"
              value={100}
              size={150}
              thickness={6}
              sx={{ color: "#eee", position: "absolute" }}
            />

            <CircularProgress
              variant="determinate"
              value={used}
              size={150}
              thickness={6}
              sx={{ color: "#0393DB" }}
            />

            {/* CENTER TEXT */}
            <Box
              position="absolute"
              top={0}
              left={0}
              width="100%"
              height="100%"
              display="flex"
              flexDirection="column"
              alignItems="center"
              justifyContent="center"
            >
              <Typography fontSize={20} fontWeight={600}>
                {used}%
              </Typography>
              <Typography fontSize={12}>{t("SSM_Dashboard.Used")}</Typography>
            </Box>
          </Box>

          {/* LABEL BELOW */}
          <Typography mt={0.5} fontSize={12}>
            {t("SSM_Dashboard.RAM")}
          </Typography>
        </Box>
        
      </Box>
      {/* HORIZONTAL LINE */}
      
      
      {/* Line Chart */}
     <Box className="ssm-pop-card-bottom-chart">
      {lineChart?.length > 0 && (
      <MiniLineChart
        logs={lineChart}
        startDate={startDate}
        endDate={endDate}
        color="#DBC203"
      />
        )}
      </Box>
    </Box>
  );
};

export default RamUsageCard;