import React, { useEffect } from "react";
import { createContext, useContext } from "react";

interface TimeFormatContextType {
  timeFormat: "12h" | "24h";
  setTimeFormat: (format: "12h" | "24h") => void;
}
const TimeFormatContext = createContext<TimeFormatContextType | undefined>(undefined);

export const TimeFormatProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  //const [theme, toggleTheme, setTheme] = useTheme();
  const [timeFormat, setTimeFormat] = React.useState<"12h" | "24h">("24h");
   
  useEffect(() => {
      const userProfileData = localStorage.getItem("userProfile");
      if (userProfileData) {
          const savedFormat = JSON.parse(userProfileData).userPreferences?.timeFormat;
          if (savedFormat === "12h" || savedFormat === "24h") {
              setTimeFormat(savedFormat);
          }
      }
  }, []);
  return (
    <TimeFormatContext.Provider value={{ timeFormat, setTimeFormat }}>
      {children}
    </TimeFormatContext.Provider>
  );
};

export const useTimeFormatContext = () => {
  const context = useContext(TimeFormatContext);
  if (!context) {
    throw new Error("useTimeFormatContext must be used within a TimeFormatContext");
  }
  return context;
};