import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App";
import "./utils/i18n";

import "../src/css/index.css";
import "../src/css/dark-theme.css";
import "../src/css/multi-theme.css";
import "../src/css/sidebar.css"

// MUI + Emotion RTL Setup
import { ThemeProvider, createTheme } from "@mui/material/styles";
import { CacheProvider } from "@emotion/react";
import createCache from "@emotion/cache";
import rtlPlugin from "stylis-plugin-rtl";

// Load saved language
const savedLang = localStorage.getItem("i18nextLng") || "en";
const isRTL = savedLang === "ar";

// Set direction before app loads
document.documentElement.setAttribute("dir", isRTL ? "rtl" : "ltr");
// document.documentElement.classList.toggle("rtl", savedLang === "ar");
document.body.classList.toggle("rtl-body", isRTL);

// Emotion cache for RTL
const cache = createCache({
  key: isRTL ? "mui-rtl" : "mui",
  stylisPlugins: isRTL ? [rtlPlugin] : [],
});

// MUI theme
const theme = createTheme({
  direction: isRTL ? "rtl" : "ltr",
});

createRoot(document.getElementById("root")!).render(
  <CacheProvider value={cache}>
    <ThemeProvider theme={theme}>
      <App />
    </ThemeProvider>
  </CacheProvider>
);
  