import { useState } from "react";
import { Box, Button, Grid, Typography } from "@mui/material";
import { VehicleByTypeProps } from "../../../../interfaces/IChart";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";

const VehicleByType1_1: React.FC<VehicleByTypeProps> = ({
  vehicleByTypeCountData,
  customizedWidth,
  displayName,
  onZoomClick,
  openZoomDialog,
  setIsDraggable,
  handleTypeChange,
  finalRulesValue
}) => {
  const [type, setType] = useState<"In" | "Out">("In");

  const handleChange = (newStatus: "In" | "Out") => {
    if (newStatus) {
      setType(newStatus);
      handleTypeChange?.(newStatus);
    }
  };

  const multiplier = 1 + (finalRulesValue || 0) / 100;

  const { theme } = useThemeContext();
  const vehicleTypes = [
    {
      label: "Truck",
      icon: "/images/dashboard/VehicleByType_Truck.gif",
      count:
        type === "In"
          ? (vehicleByTypeCountData?.truckInCount ?? 0) * multiplier
          : (vehicleByTypeCountData?.truckOutCount ?? 0) * multiplier,
    },
    {
      label: "Motorcycle",
      icon: "/images/dashboard/VehicleByType_MotorCycle.gif",
      count:
        type === "In"
          ? (vehicleByTypeCountData?.motorCycleInCount ?? 0) * multiplier
          : (vehicleByTypeCountData?.motorCycleOutCount ?? 0) * multiplier,
    },
    {
      label: "Bus",
      icon: "/images/dashboard/VehicleByType_Bus.gif",
      count:
        type === "In"
          ? (vehicleByTypeCountData?.busInCount ?? 0) * multiplier
          : (vehicleByTypeCountData?.busOutCount ?? 0) * multiplier,
    },
    {
      label: "Bicycle",
      icon: "/images/dashboard/VehicleByType_Bicycle.gif",
      count:
        type === "In"
          ? (vehicleByTypeCountData?.bicycleInCount ?? 0) * multiplier
          : (vehicleByTypeCountData?.bicycleOutCount ?? 0) * multiplier,
    },
    {
      label: "Car",
      icon: "/images/dashboard/VehicleByType_Car.gif",
      count:
        type === "In"
          ? (vehicleByTypeCountData?.carInCount ?? 0) * multiplier
          : (vehicleByTypeCountData?.carOutCount ?? 0) * multiplier,
    },
  ];

  // const formatNumbers = (num: any) => {
  //   if (num >= 1000000) return (num / 1000000).toFixed(1) + "M";
  //   if (num >= 1000) return (num / 1000).toFixed(1) + "K";
  //   return num.toString();
  // };

  return (
    <Box sx={{ width: customizedWidth }}>
      <Box className="widget-main-wrapper">
        <Box className="widget-main-header">
          <Typography variant="h6" component="h2">
            {displayName}
          </Typography>
        </Box>
        <Box className="in-out-header-buttons in-out-header-buttons-for-vehicle in-out-header-buttons-yellow">
          <Button
            className={type === "In" ? "active" : ""}
            fullWidth={true}
            onClick={() => {
              handleChange?.("In");
            }}
          >
            In
          </Button>
          <Button
            className={type === "Out" ? "active" : ""}
            fullWidth={true}
            onClick={() => {
              handleChange?.("Out");
            }}
          >
            Out
          </Button>
        </Box>

        <Box className="widget-main-body people-in-out-main people-in-out-main-vehicle">
          <div className="widget-data-wrapper">
            <div className="vehicle-by-type-data">
              <Grid className="vehicle-by-type-data-wrapper">
                {vehicleTypes.map((type) => (
                  <Grid
                    item
                    className="vehicle-by-type-data-wrapper-repeat"
                    sx={{
                      background:
                        theme === "light"
                          ? ""
                          : "linear-gradient(270deg, #C3B110 0%, #726700 100%)",
                    }}
                  >
                    <Box className="vehicle-by-type-data-image">
                      <img
                        src={type.icon}
                        alt={type.label}
                        style={{
                          width: "30px",
                          height: "30px",
                          objectFit: "contain",
                        }}
                      />
                    </Box>
                    <Box className="vehicle-by-type-data-main">
                      <Typography variant="body2" fontWeight="500">
                        {type.label}
                      </Typography>
                      <Typography variant="h6" fontWeight="bold">
                        {formatNumber(type.count)}
                      </Typography>
                    </Box>
                  </Grid>
                ))}
              </Grid>
            </div>
          </div>
        </Box>

        {/* Total Vehicles and Scan Icon */}
        <Box className="widget-main-footer">
          <Box className="widget-main-footer-value">
            <Typography>Total No. of Vehicles : </Typography>
            <span>
              {type === "In"
                ? (formatNumber((vehicleByTypeCountData?.totalInVehicleCount ?? 0) * multiplier))
                : (formatNumber((vehicleByTypeCountData?.totalOutVehicleCount ?? 0) * multiplier))}
            </span>
          </Box>
          {!openZoomDialog ? (
            <Box
              className="widget-main-footer-zoom-i"
              onMouseEnter={() => setIsDraggable?.(true)}
              onMouseLeave={() => setIsDraggable?.(false)}
            >
              <img
                src={
                  theme === "light"
                    ? "/images/dashboard/drag.svg"
                    : "/images/dark-theme/dashboard/drag.svg"
                }
                alt="vehicle"
                width={35}
                height={35}
              />
            </Box>
          ) : null}
          {!openZoomDialog ? (
            <Box
              className="widget-main-footer-zoom-i"
              onClick={onZoomClick}
              id="zoomwidgetBtnVehiclebyType"
            >
              <img
                src={
                  theme === "light"
                    ? "/images/dashboard/ZoomWidget.svg"
                    : "/images/dark-theme/dashboard/ZoomWidget.svg"
                }
                alt="vehicle"
                width={35}
                height={35}
              />
            </Box>
          ) : null}
        </Box>
      </Box>
    </Box>
  );
};
export { VehicleByType1_1 };
