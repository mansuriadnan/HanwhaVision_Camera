// src/App.tsx
import React, { Suspense, useEffect, useLayoutEffect } from "react";

import { ErrorBoundary } from "./components/ErrorBoundary/ErrorBoundary";
import RoutesConfig from "./routes/RoutesConfig";
import { Toast } from "./components/Reusable/Toast";
import { ThemeProvider } from "../src/context/ThemeContext";
import { RouterProvider } from "react-router-dom";
import { LoadingProvider } from "./context/LoadingContext";
import { LicenseProvider } from "./context/LicenseContext";
import { PermissionsProvider } from "./context/PermissionsContext";
import "./css/App.css";
import { UserProvider } from "./context/UserContext";
import { SettingsProvider } from "./context/SettingContext";
import { MapDataProvider } from "./context/MapContext";
import { SignalRProvider } from "./context/SignalRContext";
import { TimeFormatProvider } from "./context/TimeFormatContext";
import { LOCAL_LOADER_THEME_COLORS } from "./utils/constants";
import { SignalRConnectionIDProvider } from "./context/SignalRConnectionIDContext";

const App: React.FC = () => {
  useLayoutEffect(() => {
    const savedTheme = localStorage.getItem("theme") || "light";
    const savedThemeColor = localStorage.getItem("themeColor") || "default-theme";  
    document.body.classList.remove("light", "dark");
    document.body.classList.add(savedTheme);
    document.body.classList.add(savedThemeColor);
    // Store corresponding loader color
    const loaderColor = LOCAL_LOADER_THEME_COLORS[savedThemeColor];
  if (loaderColor) {
    localStorage.setItem("loaderColor", loaderColor);
  }
  }, []);

  return (
    <UserProvider>
      <LoadingProvider>
        <ErrorBoundary>
          <PermissionsProvider>
            <ThemeProvider>
              <TimeFormatProvider>
                <Toast />
                <LicenseProvider>
                  <SettingsProvider>
                    <MapDataProvider>
                      <SignalRConnectionIDProvider>
                        <SignalRProvider>
                          <Suspense fallback={<div>Loading...</div>}>
                            <RouterProvider router={RoutesConfig} />
                          </Suspense>
                        </SignalRProvider>
                      </SignalRConnectionIDProvider>
                    </MapDataProvider>
                  </SettingsProvider>
                </LicenseProvider>
              </TimeFormatProvider>
            </ThemeProvider>
          </PermissionsProvider>
        </ErrorBoundary>
      </LoadingProvider>
    </UserProvider>
  );
};

export default App;
