// MainLayout.tsx
import React, { useEffect, useState } from "react";
import { useThemeContext } from "../context/ThemeContext";
import { Outlet } from "react-router-dom";
import {
  startSignalRConnection,
  stopSignalRConnection,
} from "../utils/signalRService";
import { Sidebar } from "../components/Layout/Sidebar";
import { Header } from "../components/Layout/Header";
import { GetServerListService } from "../services/settingService";
import { initializeServer, stopAllConnections } from "../utils/multipleSignalRService";
import { useSignalRConnectionIDContext } from "../context/SignalRConnectionIDContext";


interface IServerList {
  id: string;
  hostingAddress: string;
  serverName: string;
  username: string;
  password: string;
  accessToken?: string;
  refreshToken?: string;
}

const MainLayout: React.FC = () => {
  const { theme } = useThemeContext();
  const [isSidebarOpen, setIsSidebarOpen] = useState(true);
  const { setConnectionIDValue } = useSignalRConnectionIDContext();

  useEffect(() => {
    const savedTheme = localStorage.getItem("theme") || "light";
    document.body.classList.remove("light", "dark");
    document.body.classList.add(savedTheme);

    startSignalRConnection(setConnectionIDValue); // passing setConnectionIDValue from context
    fetchServerData();

    return () => {
      stopSignalRConnection();
      stopAllConnections();
    };

  }, []);

  const fetchServerData = async () => {
    try {
      const response = await GetServerListService();

      const serverData: IServerList[] = response
        ?.filter(item => item.isActive)
        .map(item => ({
          id: item.id,
          hostingAddress: item.hostingAddress,
          serverName: item.serverName,
          username: item.username,
          password: item.password,
        }));

      await Promise.all(
        serverData.map((server) => initializeServer(server))
      );

    } catch (err) {
      console.error("Error while fetching server data");
    }
  };

  useEffect(() => {
    const handleResize = () => {
      if (window.innerWidth >= 1200) {
        setIsSidebarOpen(true);
      } else {
        setIsSidebarOpen(false);
      }
    };

    handleResize();

    window.addEventListener("resize", handleResize);

    return () => window.removeEventListener("resize", handleResize);
  }, []);

  window.addEventListener("beforeunload", () => {
    stopSignalRConnection();
  });

  const handleToggleButton = () => {
    setIsSidebarOpen((prev) => !prev);
  }

  const handleSidebar = (keepOpen?: boolean) => {
    setIsSidebarOpen((prev) => {
      if (keepOpen === true) {
        return true;
      }
      else if (keepOpen === false) {
        if (window.innerWidth >= 1200) {
          return true;
        }
      }
      return !prev;
    });
  };

  return (
    <div className={isSidebarOpen ? "main-layout open" : "main-layout close"}>
      <Header />
      <div className="content">
        <Sidebar
          isSidebarOpen={isSidebarOpen}
          toggleButton={handleToggleButton}
          handleSidebar={handleSidebar}
        />
        <main
          className={`main-content ${theme === "light" ? "main-content-light" : "main-content-dark"
            }`}
        >
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default MainLayout;
