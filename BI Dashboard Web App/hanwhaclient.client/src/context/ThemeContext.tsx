// context/ThemeContext.tsx
import React, { createContext, useContext } from "react";
import useTheme, { Theme, ThemeColor } from "../hooks/useTheme";

type ThemeContextType = {
  theme: Theme;
  setTheme: (theme: Theme) => void;
  themeColor: ThemeColor;
  setThemeColor: (color: ThemeColor) => void;
};

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [theme, setTheme, themeColor, setThemeColor] = useTheme();
      //  Detect PDF page
  const isPdfPage = window.location.pathname.includes("advance-pdf");

  //  Force light theme only for this page
  const effectiveTheme: Theme = isPdfPage ? "light" : theme;

  return (
    <ThemeContext.Provider value={{ theme : effectiveTheme, setTheme, themeColor, setThemeColor  }}>
      {children}
    </ThemeContext.Provider>
  );
};

export const useThemeContext = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error("useThemeContext must be used within a ThemeProvider");
  }
  return context;
};
