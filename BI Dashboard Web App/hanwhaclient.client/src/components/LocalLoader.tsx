import React from "react";
import { Box, CircularProgress } from "@mui/material";

interface LocalLoaderProps {
  width: number;
  height: number;
  size?: number; // Spinner size (default 40)
  color?: "primary" | "secondary" | "inherit" | "warning" | "info" | "success" | "error";
}

const LocalLoader: React.FC<LocalLoaderProps> = ({
  width,
  height,
  size = 40,
  // color = "primary",
}) => {

  const loaderColor =
  localStorage.getItem("loaderColor") || "#FE6500";
  return (
    <Box
      sx={{
        width,
        height,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        backgroundColor: "transparent",
      }}
    >
      <CircularProgress size={size} 
       sx={{
          color: loaderColor,
        }} />
    </Box>
  );
};

export { LocalLoader };
