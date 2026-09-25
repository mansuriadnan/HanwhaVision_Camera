import React, { useEffect, useRef, useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  DialogContentText,
  FormControl,
  InputLabel,
  Select,
  OutlinedInput,
  Box,
  Chip,
  MenuItem,
  ListItemText,
  Checkbox,
} from "@mui/material";
import html2canvas from "html2canvas";
import jsPDF from "jspdf";

import { Dayjs } from "dayjs";
import { ILookup } from "../../interfaces/ILookup";
import { convertToUTC } from "../../utils/convertToUTC";
import { csvWidgetService } from "../../services/reportServices";
import { showToast } from "../../components/Reusable/Toast";
import autoTable from "jspdf-autotable";
import {
  convertBase64ToPngBase64,
  convertImageToBase64,
} from "../../utils/convertImageToBase64";
import { LoadingManager } from "../../utils/LoadingManager";
import { Label } from "react-konva";
import { dashboardCharts } from "../../constants/dashboardChartList";
import { setExporting } from "../../utils/refreshManager";
import { useRequestCounter } from "../../hooks/useHttpInterceptor";
import { fi, is } from "date-fns/locale";
import AdvanceExportCalculator, {
  CalculatorExportRule,
} from "./AdvanceExportCalculator";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import dayjs from "dayjs";

//People,Gender,Vehicle,Vehicle Type,Forklift  In/Out widgets.

interface ExportReportDialogProps {
  open: boolean;
  onClose: () => void;
  layouts: any;
  selectedFloors?: string[];
  selectedZones?: string[];
  selectedStartDate?: Dayjs | null;
  selectedEndDate?: Dayjs | null;
  floorList: ILookup[];
  zoneList: ILookup[];
  WidgetNames?: string[];
  WidgetTitleNames?: string[];
}

const ExportWidgetDialog: React.FC<ExportReportDialogProps> = ({
  open,
  onClose,
  layouts,
  selectedFloors,
  selectedZones,
  selectedStartDate,
  selectedEndDate,
  floorList,
  zoneList,
}) => {
  const chartRefs = useRef<Record<string, HTMLDivElement | null>>({});
  const [selectedWidgets, setSelectedWidgets] = useState<string[]>([]);
  const [userProfileDetails, setUserProfileDetails] = useState<any>();
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [appUploadedLogo, setAppUploadedLogo] = useState<any>();
  const [appLogo, setAppLogo] = useState<any>();
  const { pendingCount, isAllComplete } = useRequestCounter();
  const [isExportPdfClicked, setIsExportPdfClicked] = useState(false);
  const [startPdfExport, setStartPdfExport] = useState(false);
  const [rules, setRules] = useState<CalculatorExportRule[]>([]);
  const { t } = useTranslation();
  const navigate = useNavigate();

  // Widget ID to Name mapping
  const widgetIdToNameMap: Record<string, { id: string; title: string }> = {
    "1": { id: "TotalCameraCount", title: "Camera Online/Offline" },
    "2": { id: "CameraCountByModel", title: "Model Types" },
    "3": { id: "CameraCountByFeatures", title: "Feature Types" },
    "4": {
      id: "PeopleCapacityUtilization",
      title: "Capacity Utilization for People",
    },
    "5": {
      id: "VehicleCapacityUtilization",
      title: "Capacity Utilization for Vehicle",
    },
    "6": {
      id: "VehicleCameraCapacityUtilizationAnalysisByZones",
      title: "Zone Wise Capacity Utilization for Vehicle",
    },
    "7": {
      id: "PeopleCameraCapacityUtilizationAnalysisByZones",
      title: "Zone Wise Capacity Utilization for People",
    },
    "8": { id: "SlipFallAnalysis", title: "Slip & Fall Detection" },
    "9": { id: "PeopleCountChart", title: "People" },
    "10": { id: "AveragePeopleCountChart", title: "Average People Counting" },
    "11": { id: "GenderWisePeopleCountAnalysis", title: "Gender" },
    "12": {
      id: "CumulativePeopleCountChart",
      title: "Cumulative People Count",
    },
    "13": { id: "NewVsTotalVisitorChart", title: "New vs Total Visitors" },
    "14": {
      id: "SafetyMeasuresStoppedVehicleByTypeAnalysis",
      title: "Safety Measures",
    },
    "15": { id: "PeopleCountByZones", title: "Zone wise People Counting" },
    "16": { id: "VehicleByType", title: "Vehicle by Type" },
    "17": { id: "WrongWayAnalysis", title: "Vehicle in Wrong Direction" },
    "18": { id: "VehicleUTurnAnalysis", title: "Vehicle U Turn detection" },
    "19": { id: "PedestrianAnalysis", title: "Pedestrian Detection" },
    "20": { id: "VehicleCountChart", title: "Vehicle" },
    "21": { id: "AverageVehicleCountChart", title: "Average Vehicle Counting" },
    "22": { id: "VehicleQueueAnalysis", title: "Vehicle Queue Analysis" },
    "23": {
      id: "StoppedVehicleByTypeAnalysis",
      title: "Stopped Vehicle Count Time",
    },
    "24": {
      id: "VehicleTurningMovementAnalysis",
      title: "Vehicle Turning Movement counts",
    },
    "25": { id: "ShoppingCartQueueAnalysis", title: "Shopping Cart Counting" },
    "26": {
      id: "ShoppingCartCountAnalysis",
      title: "Queue events for shopping cart",
    },
    "27": { id: "PeopleQueueAnalysis", title: "Queue events for people" },
    "28": {
      id: "VehicleSpeedViolationAnalysis",
      title: "Speed Violation by Vehicle",
    },
    "29": { id: "BlockedExitAnalysis", title: "Blocked exit detection" },
    "30": { id: "TrafficJamAnalysis", title: "Traffic Jam by Day" },
    "31": { id: "ForkliftCountAnalysis", title: "Counting for forklift" },
    "32": {
      id: "FactoryBlockedExitAnalysis",
      title: "Factory Blocked exit detection",
    },
    "33": { id: "BlockedExitAnalysis", title: "Blocked exit detection" },
    "34": { id: "ProxomityDetectionAnalysis", title: "Detect Forklifts" },
    "42": { id: "PeopleCountChart", title: "People In Out" },
    "43": { id: "VehicleCountChart", title: "Vehicle In Out" },
    "45": { id: "Age", title: "Age" },
    "47": { id: "CamerasInRMA", title: "Cameras In RMA" },
    "48": { id: "CameraMaintenanceStatus", title: "Camera Maintenance Status" },
    "49": { id: "CamerasInMaintenance", title: "Cameras In Maintenance" },
    "50": {
      id: "CameraDisconnectionTracker",
      title: "Camera Disconnection Tracker",
    },
    "51": {
      id: "APIError",
      title: "API Error",
    },
    "52": {
      id: "Parking",
      title: "Parking",
    },
    "53": {
      id: "ANPRParking",
      title: "ANPR Parking",
    },
  };

  const selectedWidgetsOptions = [
    { id: "all", title: "All" },
    ...(layouts?.lg?.map((layout: any) => ({
      id: layout.chartID,
      title: layout.chartName,
    })) || []),
  ];
  let selectedFloorTitles = "";
  let selectedZonesTitles = "";

  useEffect(() => {
    const userProfile = localStorage.getItem("userProfile");
    if (userProfile) {
      setUserProfileDetails(JSON.parse(userProfile));
    }
    let generalSettings = localStorage.getItem("generalSettings");
    if (generalSettings) {
      let parsedData = JSON.parse(generalSettings);
      setAppUploadedLogo(parsedData?.logo);
      // console.log("parsedData?.logo",parsedData?.logo);
    }

    const convertImageToBase641 = async () => {
      convertImageToBase64(
        "/images/vision_insight_logo_pdf.png",
        (base64: string | null) => {
          if (base64) {
            setAppLogo(base64);
          } else {
            console.error("Failed to convert image to Base64");
          }
        },
      );
    };

    convertImageToBase641();
  }, []);

  useEffect(() => {
    if (open) {
      setSelectedWidgets([]);
      setRules([]);  
    }
  }, [open]);

  useEffect(() => {
    const runExport = async () => {
      if (pendingCount === 0 && isAllComplete && isExportPdfClicked) {
        try {
          //await waitUntilWidgetsAreStable(selectedWidgets);
          await executeExportPDF();
        } catch (err) {
          console.error("Export aborted:", err);
          setIsExportPdfClicked(false);
          LoadingManager.hideLoading();
        }
      }
    };
    runExport();
  }, [pendingCount, isAllComplete, isExportPdfClicked]);
  //   useEffect(() => {
  //   if (startPdfExport) {
  //     const timer = setTimeout(async () => {
  //       try {
  //         await onExportPDF();   // this will call executeExportPDF
  //       } catch (err) {
  //         console.error("Export failed", err);
  //         LoadingManager.hideLoading(); // just in case
  //       } finally {
  //         setStartPdfExport(false);
  //       }
  //     }, 100); // give browser a chance to paint loader

  //     return () => clearTimeout(timer);
  //   }
  // }, [startPdfExport]);

  const formatDateRange = (date: Date): string => {
    const options: Intl.DateTimeFormatOptions = {
      month: "short",
      day: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      hour12: true,
    };

    const formattedDate = date
      .toLocaleString("en-US", options)
      .replace(",", "");
    return `${formattedDate}`;
  };

  const onExportCSV = async () => {
    try {
      setIsLoading(true);

      var newRule = rules;

      // Map selected widget IDs to widget names
      const widgetNames: string[] = selectedWidgets
        .map((id) => widgetIdToNameMap[id].id)
        .filter((name) => name !== undefined); // Filter out undefined values

      const widgetTitleNames: { id: string; title: string }[] = selectedWidgets
        .map((key) => widgetIdToNameMap[key])
        .filter((item) => item !== undefined);

      if (widgetNames.length === 0) {
        showToast(t("Advance_Export_Dialog.selectAtLeastOneWidget"), "error");
        return;
      }
      var startDateNew = selectedStartDate?.format(
        "YYYY-MM-DDTHH:mm:ssZ",
      ) as string;
      var endDateNew = selectedEndDate?.format(
        "YYYY-MM-DDTHH:mm:ssZ",
      ) as string;
      // Prepare the request payload
      const widgetRequest: any = {
        FloorIds: selectedFloors || [],
        ZoneIds: selectedZones || [],
        StartDate: convertToUTC(startDateNew),
        EndDate: convertToUTC(endDateNew),
        WidgetNames: widgetNames,
        widgetTitleNames: widgetTitleNames,
        IntervalMinute: 60,
        calculatorExportRule: newRule,
      };

      await csvWidgetService(
        {
          data: widgetRequest,
        },
        "/api/Widget/DownloadMultipleWidgetsCsv",
      );
      onClose();
    } catch (error) {
      console.error("Error exporting CSV:", error);
      alert("An error occurred while exporting the CSV. Please try again.");
    } finally {
      setIsLoading(false);
      setRules([]);
    }
  };

  const mergeHeatmap = async (
    imgEl: HTMLImageElement,
    overlayCanvasEl: HTMLCanvasElement,
  ) => {
    const mergedCanvas = document.createElement("canvas");
    mergedCanvas.width = imgEl.naturalWidth || imgEl.width;
    mergedCanvas.height = imgEl.naturalHeight || imgEl.height;
    const ctx: any = mergedCanvas.getContext("2d");

    // Base image
    ctx.drawImage(imgEl, 0, 0, mergedCanvas.width, mergedCanvas.height);

    // Overlay
    ctx.globalAlpha = 0.4; // make it slightly lighter for PDF
    ctx.drawImage(
      overlayCanvasEl,
      0,
      0,
      mergedCanvas.width,
      mergedCanvas.height,
    );
    ctx.globalAlpha = 1;

    // Return <img>
    const mergedImg = document.createElement("img");
    mergedImg.src = mergedCanvas.toDataURL("image/png");
    mergedImg.style.width = imgEl.style.width;
    mergedImg.style.height = imgEl.style.height;
    mergedImg.style.borderRadius = imgEl.style.borderRadius;
    return mergedImg;
  };
  const createLegendContainer = (legendCanvas: HTMLCanvasElement) => {
    // Create thin color bar from legend canvas
    const legendImg = document.createElement("img");
    legendImg.src = legendCanvas.toDataURL("image/png");
    legendImg.style.width = "100%";
    legendImg.style.height = "6px"; // thin like browser
    legendImg.style.borderRadius = "4px";
    legendImg.style.display = "block";

    // Create labels row (Low / High)
    const labelsRow = document.createElement("div");
    labelsRow.style.display = "flex";
    labelsRow.style.justifyContent = "space-between";
    labelsRow.style.alignItems = "center";
    labelsRow.style.fontSize = "10px";
    labelsRow.style.fontWeight = "bold";
    labelsRow.style.marginTop = "2px";
    labelsRow.style.width = "100%";
    labelsRow.style.color = "#000";

    const low = document.createElement("span");
    low.innerText = "Low";

    const high = document.createElement("span");
    high.innerText = "High";

    labelsRow.appendChild(low);
    labelsRow.appendChild(high);

    // Container for both bar + labels
    const legendContainer = document.createElement("div");
    legendContainer.style.width = "95%";
    legendContainer.style.margin = "6px auto 0 auto"; // center inside widget
    legendContainer.style.display = "flex";
    legendContainer.style.flexDirection = "column";

    legendContainer.appendChild(legendImg);
    legendContainer.appendChild(labelsRow);

    return legendContainer;
  };

  const waitUntilWidgetsAreStable = async (
    widgetIds: string[],
    options = { timeout: 15000, interval: 300 },
  ) => {
    const startTime = Date.now();

    return new Promise<void>((resolve, reject) => {
      const check = () => {
        // check global loader
        const globalLoader = document.querySelector(
          ".global-loader, .MuiCircularProgress-root",
        );
        if (globalLoader) {
          if (Date.now() - startTime > options.timeout) {
            reject(new Error("Timeout: Global loader still active"));
            return;
          }
          return setTimeout(check, options.interval);
        }

        // check widgets
        const allStable = widgetIds.every((id) => {
          const widget = document.getElementById("parent" + id);
          if (!widget) return false;

          const isLoading = widget.querySelector(
            ".loader, .spinner, .skeleton",
          );
          const isEmpty = widget.innerText
            .trim()
            .toLowerCase()
            .includes("no data found");

          return !isLoading && !isEmpty;
        });

        if (allStable) {
          resolve();
        } else if (Date.now() - startTime > options.timeout) {
          reject(new Error("Timeout: Widgets did not stabilize"));
        } else {
          setTimeout(check, options.interval);
        }
      };

      check();
    });
  };

  const waitForImagesToLoad = async (root: HTMLElement, timeout = 5000) => {
    const imgs = Array.from(root.querySelectorAll("img")) as HTMLImageElement[];
    if (imgs.length === 0) return;
    await Promise.race([
      Promise.all(
        imgs.map(
          (img) =>
            new Promise<void>((res) => {
              if (img.complete && img.naturalWidth !== 0) return res();
              const ondone = () => {
                img.removeEventListener("load", ondone);
                img.removeEventListener("error", ondone);
                res();
              };
              img.addEventListener("load", ondone);
              img.addEventListener("error", ondone);
              // fallback in case browser doesn't fire
              setTimeout(ondone, 3000);
            }),
        ),
      ),
      new Promise<void>((res) => setTimeout(res, timeout)),
    ]);
  };

  const captureElementWithoutTopCrop = async (
    element: HTMLElement,
    bufferCssPx: number = 2,
    scale: number = Math.max(1, window.devicePixelRatio || 1),
  ): Promise<{ dataUrl: string; width: number; height: number }> => {
    // measure element size
    const rect = element.getBoundingClientRect();
    const cssWidth = rect.width;

    // clone element to avoid reflowing the original DOM
    const cloned = element.cloneNode(true) as HTMLElement;
    cloned.style.boxSizing = "border-box";
    cloned.style.width = `${cssWidth}px`;
    cloned.style.margin = "0";
    cloned.style.display = getComputedStyle(element).display || "inline-block";

    // offscreen wrapper for clean capture
    const wrapper = document.createElement("div");
    wrapper.style.position = "absolute";
    wrapper.style.left = "-99999px";
    wrapper.style.top = "-99999px";
    wrapper.style.background = "#ffffff";
    wrapper.style.padding = `${bufferCssPx}px 0 0 0`;
    wrapper.style.overflow = "visible";
    wrapper.style.width = `${cssWidth}px`;
    wrapper.appendChild(cloned);
    document.body.appendChild(wrapper);

    // let styles/fonts settle
    await new Promise((res) => setTimeout(res, 80));
    await document.fonts.ready;
    await new Promise((r) => setTimeout(r, 300));
    wrapper.style.fontFamily = `"Open Sans", Arial, sans-serif`;
    wrapper.querySelectorAll("*").forEach((el: any) => {
      el.style.fontFamily = `"Open Sans", Arial, sans-serif`;
    });
    // capture
    const captureCanvas = await html2canvas(wrapper, {
      backgroundColor: "#fff",
      scale,
      useCORS: true,
      scrollY: 0,
      logging: false,
    });

    const bufferCapturePx = Math.round(bufferCssPx * scale);

    // crop out top buffer
    const croppedCanvas = document.createElement("canvas");
    croppedCanvas.width = captureCanvas.width;
    croppedCanvas.height = captureCanvas.height - bufferCapturePx;
    const ctx = croppedCanvas.getContext("2d");
    if (!ctx) throw new Error("Canvas context not available for cropping");
    ctx.drawImage(captureCanvas, 0, -bufferCapturePx);

    const dataUrl = croppedCanvas.toDataURL("image/png");

    // cleanup
    wrapper.remove();

    return {
      dataUrl,
      width: croppedCanvas.width,
      height: croppedCanvas.height,
    };
  };

  const svgToPngDataUrl = async (
    svgEl: SVGSVGElement,
    width: number,
    height: number,
  ): Promise<string> => {
    return new Promise((resolve, reject) => {
      try {
        const serializer = new XMLSerializer();
        svgEl.setAttribute("width", `${width}`);
        svgEl.setAttribute("height", `${height}`);
        svgEl.setAttribute("viewBox", `0 0 ${width} ${height}`);
        svgEl.setAttribute("preserveAspectRatio", "xMidYMid meet");
        const svgString = serializer.serializeToString(svgEl);
        const svgBlob = new Blob([svgString], {
          type: "image/svg+xml;charset=utf-8",
        });
        const url = URL.createObjectURL(svgBlob);
        const img = new Image();
        img.crossOrigin = "anonymous";
        img.onload = () => {
          const canvas = document.createElement("canvas");
          canvas.width = width || svgEl.clientWidth || 900;
          canvas.height = height || svgEl.clientHeight || 400;
          const ctx = canvas.getContext("2d");
          if (!ctx) return reject("Canvas ctx missing");
          ctx.fillStyle = "#ffffff";
          ctx.fillRect(0, 0, canvas.width, canvas.height);
          ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
          URL.revokeObjectURL(url);
          resolve(canvas.toDataURL("image/png"));
        };
        img.onerror = (e) => {
          URL.revokeObjectURL(url);
          reject(e);
        };
        img.src = url;
      } catch (err) {
        reject(err);
      }
    });
  };

  const onExportPDF = async () => {
    if (!selectedWidgets || selectedWidgets.length === 0) {
      showToast(t("Advance_Export_Dialog.selectAtLeastOneWidget"), "error");
      return;
    }

    LoadingManager.showLoading();
    requestAnimationFrame(() => setIsExportPdfClicked(true));
  };

  const executeExportPDF = async () => {
    // LoadingManager.showLoading();
    const selectedWidgetIds = selectedWidgets.map((id) => parseInt(id));

    try {
      setExporting(true); // stop refresh
      const selectedWidgetNames = dashboardCharts
        .filter((chart) => selectedWidgetIds.includes(chart.id))
        .map((chart) => chart.chartName);

      // await waitUntilWidgetsAreStable(selectedWidgets);
      selectedFloorTitles =
        floorList?.length > 0
          ? floorList
              .filter((floor) => selectedFloors?.includes(floor.id))
              .map((floor) => floor.title)
              .join(", ")
          : "";
      selectedZonesTitles =
        zoneList.length > 0
          ? zoneList
              .filter((zone) => selectedZones?.includes(zone.id))
              .map((zone) => zone.title)
              .join(", ")
          : "";
      //const logoUrl = await svgToPngDataUrl(appUploadedLogo, 100, 40);;

      const pdf = new jsPDF("p", "mm", "a4");
      const pdfWidth = pdf.internal.pageSize.getWidth();
      const pdfHeight = pdf.internal.pageSize.getHeight();
      const pagePadding = 10;
      const headerHeight = 20;
      const footerHeight = 15;
      const usableHeight = pdfHeight - headerHeight - footerHeight - 10;
      let pngBase64Applogo = "";
      if (appUploadedLogo) {
        pngBase64Applogo = await convertBase64ToPngBase64(appUploadedLogo);
      }

      const addHeaderFooter = (pageNumber: number, totalPages: number) => {
        const logoWidth = 30;
        const logoHeight = 10;
        const logoX = pagePadding;
        const logoY = pagePadding;
        if (appUploadedLogo) {
          pdf.addImage(
            pngBase64Applogo,
            "PNG",
            logoX,
            logoY,
            logoWidth,
            logoHeight,
          );
        }

        const secondLogoWidth = 45;
        const secondLogoHeight = 7;
        const secondLogoX = pdfWidth - pagePadding - secondLogoWidth;
        const secondLogoY = logoY + 2;

        // Example: Replace with your actual 2nd logo base64
        pdf.addImage(
          appLogo,
          "PNG",
          secondLogoX,
          secondLogoY,
          secondLogoWidth,
          secondLogoHeight,
        );

        // === Add horizontal line below the header ===
        const lineY = headerHeight + 3;
        pdf.setDrawColor(200);
        pdf.setLineWidth(0.3);
        pdf.line(pagePadding, lineY, pdfWidth - pagePadding, lineY);

        // Footer
        pdf.setFontSize(10);
        pdf.setFont("Open Sans", "normal");
        pdf.setTextColor(0, 0, 0);
        const footerText = `Page ${pageNumber} of ${totalPages}`;
        pdf.text(
          footerText,
          pdfWidth - pagePadding - pdf.getTextWidth(footerText),
          pdfHeight - 10,
        );
      };

      let currentY = headerHeight + 10;
      let pageNumber = 1;
      const verticalSpacing = 5;
      const maxChartHeight = pdfHeight - headerHeight - footerHeight - 10;

      // --- New Section: Displaying Info like zone, floor in an HTML-rendered Box using jsPDF.html() ---
      const selectedDate =
        selectedStartDate && selectedEndDate
          ? `${formatDateRange(selectedStartDate.toDate())} - ${formatDateRange(
              selectedEndDate.toDate(),
            )}`
          : "N/A";

      pdf.setFontSize(10);
      pdf.setFont("helvetica", "bold");
      pdf.setTextColor(0, 0, 0);
      pdf.text("Applied Filters:", pagePadding, currentY);
      currentY += 5;

      autoTable(pdf, {
        startY: currentY,
        margin: { left: pagePadding },
        tableWidth: pdfWidth - pagePadding * 2,
        head: [["Parameter", "Value"]],
        body: [
          ["Selected Floors", selectedFloorTitles || "N/A"],
          ["Selected Zones", selectedZonesTitles || "N/A"],
          ["Selected Date ", selectedDate || "N/A"],
          ["Selected Widgets", selectedWidgetNames.join(",")],
          [
            "Created By",
            `${userProfileDetails?.firstname} ${
              userProfileDetails?.lastname || "N/A"
            }`,
          ],
          ["Created On", formatDateRange(new Date()) || "N/A"],
        ],
        theme: "grid",
        styles: {
          fontSize: 9,
          fontStyle: "normal",
          cellPadding: 2,
          lineWidth: 0.1,
          lineColor: [204, 204, 204], // grey border
        },
        headStyles: {
          fillColor: [230, 230, 230],
          textColor: 0,
          fontStyle: "bold",
          halign: "left",
        },
        columnStyles: {
          0: { cellWidth: 45 },
          1: { cellWidth: pdfWidth - pagePadding * 2 - 45 },
        },
      });
      const table = (pdf as any).lastAutoTable;

      if (table) {
        const startX = table.settings.margin.left ?? pagePadding;
        const startY = table.startY;
        const endY = table.finalY;
        const width = table.table?.width ?? pdfWidth - pagePadding * 2;
        const height = endY - startY;
        const radius = 6;

        // Avoid drawing if table spans multiple pages
        if (table.startPageNumber === table.finalPageNumber) {
          pdf.setDrawColor(100); // border color
          pdf.setLineWidth(0.3);
          pdf.roundedRect(startX, startY, width, height, radius, radius, "S");
        }

        currentY = endY + 7;
      }

      pdf.setFontSize(10);
      pdf.setFont("helvetica", "bold");
      pdf.setTextColor(0, 0, 0);
      pdf.text("Visualizations:", pagePadding, currentY);
      currentY += 5;

      // --- End of New Section ---

      // Pre-render and measure all selected elements
      const renderedCharts = [];

      for (let chartId of selectedWidgets) {
        const original = document.getElementById("parent" + chartId);

        if (!original) continue;

        // Clone the widget for safe off-screen rendering
        const clone = original.cloneNode(true) as HTMLElement;
        clone.style.transform = "none";
        clone.style.position = "relative";
        clone.style.overflow = "visible";

        if (selectedWidgetNames.includes("Map Plan")) {
          clone
            .querySelectorAll(
              ".gm-fullscreen-control, .gm-svpc, a.leaflet-control-zoom-in,a.leaflet-control-zoom-out",
            )
            .forEach((el) => el.remove());
        }

        // Calculate dynamic size
        const width = original.scrollWidth;
        const height = original.scrollHeight;
        clone.style.width = `${width}px`;
        clone.style.height = `${height}px`;

        // Fix images
        clone.querySelectorAll("img").forEach((img: any) => {
          img.style.maxWidth = "100%";
          img.style.height = "auto";
          img.style.objectFit = "contain";
        });

        // Remove resize handles or undesired extras
        clone
          .querySelectorAll(".react-resizable-handle")
          .forEach((el) => el.remove());

        // Render off-screen
        const container = document.createElement("div");
        container.style.position = "absolute";
        container.style.left = "-9999px";
        container.style.top = "-9999px";
        container.style.zIndex = "-1";
        container.appendChild(clone);
        document.body.appendChild(container);

        await new Promise((res) => setTimeout(res, 300)); // let layout settle
        await document.fonts.ready;
        await new Promise((r) => setTimeout(r, 300));
        clone.style.fontFamily = `"Open Sans", Arial, sans-serif`;
        clone.querySelectorAll("*").forEach((el: any) => {
          el.style.fontFamily = `"Open Sans", Arial, sans-serif`;
        });
        const svgList = Array.from(
          clone.querySelectorAll("svg"),
        ) as SVGSVGElement[];
        for (const svg of svgList) {
          try {
            const svgRect = svg.getBoundingClientRect();
            const png = await svgToPngDataUrl(
              svg,
              Math.ceil(svgRect.width),
              Math.ceil(svgRect.height),
            );
            // replace svg with img
            const img = document.createElement("img");
            img.src = png;
            img.style.width = `${svgRect.width}px`;
            img.style.height = `${svgRect.height}px`;
            img.style.display = "block";
            svg.parentNode?.replaceChild(img, svg);
          } catch (e) {
            console.warn("SVG to PNG failed", e);
          }
        }
        clone.style.display = "inline-block";
        clone.style.height = "auto";
        clone.style.alignItems = "flex-start";

        const canvas = await html2canvas(clone, {
          backgroundColor: "#fff",
          useCORS: true,
          scale: 2,
          width,
          height,
          scrollX: 0,
          scrollY: 0,
        });
        document.body.removeChild(container);
        const imgData = canvas.toDataURL("image/png");

        const imgProps = pdf.getImageProperties(imgData);
        const contentWidth = pdfWidth - pagePadding * 2;
        const contentHeight = 81.9; //(imgProps.height * contentWidth) / imgProps.width;
        const sizeClassMap: Record<string, string> = {
          "one-by-one": "smallCharts",
          "two-by-one": "largeCharts",
          "extra-large-charts": "extraLargeCharts",
          "map-floor-plan-charts": "mapFloorCharts",
        };

        let chartSize = "";

        if (original) {
          const found = Object.keys(sizeClassMap).find((cls) =>
            original.classList.contains(cls),
          );
          chartSize = found ? sizeClassMap[found] : "";
        }

        renderedCharts.push({
          id: chartId,
          imgData,
          widthMm: contentWidth,
          heightMm: contentHeight,
          originalWidth: width,
          originalHeight: height,
          chartSize: chartSize,
        });
      }

      // Now continue with your existing chart rendering logic
      const largeCharts = renderedCharts.filter(
        (c) => c.chartSize == "largeCharts",
      );
      const smallCharts = renderedCharts.filter(
        (c) => c.chartSize == "smallCharts",
      );
      const extraLargeCharts = renderedCharts.filter(
        (c) => c.chartSize == "extraLargeCharts",
      );
      const mapFloorCharts = renderedCharts.filter(
        (c) => c.chartSize == "mapFloorCharts",
      );

      for (let chart of largeCharts) {
        let displayHeight = chart.heightMm;
        let displayWidth = chart.widthMm;
        const remainingHeight = pdfHeight - footerHeight - currentY;

        if (displayHeight > maxChartHeight) {
          const ratio = maxChartHeight / chart.heightMm;
          displayHeight *= ratio;
          displayWidth *= ratio;
        }

        if (displayHeight > remainingHeight) {
          pdf.addPage();
          pageNumber++;
          currentY = headerHeight + verticalSpacing;
        }

        const offsetX = (pdfWidth - displayWidth) / 2;

        pdf.addImage(
          chart.imgData,
          "PNG",
          offsetX,
          currentY,
          displayWidth,
          displayHeight,
        );
        currentY += displayHeight + verticalSpacing;
      }

      // ---- Then render small charts (width <= 421px) in 2 per row ----
      for (let i = 0; i < smallCharts.length; ) {
        const chart1 = smallCharts[i];
        const chart2 = smallCharts[i + 1];

        const colWidth = (pdfWidth - pagePadding * 3) / 2;
        const h1 = chart1.heightMm;
        const h2 = chart2?.heightMm || 0;
        const rowHeight = Math.max(h1, h2);
        const remainingHeight = pdfHeight - footerHeight - currentY;

        if (rowHeight > remainingHeight) {
          pdf.addPage();
          pageNumber++;
          currentY = headerHeight + verticalSpacing;
        }

        pdf.addImage(
          chart1.imgData,
          "PNG",
          pagePadding,
          currentY,
          colWidth,
          h1,
        );

        if (chart2) {
          pdf.addImage(
            chart2.imgData,
            "PNG",
            pagePadding * 2 + colWidth,
            currentY,
            colWidth,
            h2,
          );
          i += 2;
        } else {
          i += 1;
        }

        currentY += rowHeight + verticalSpacing;
      }

      // ---- Render Extra-Large Charts at the End ----
      for (let chart of extraLargeCharts) {
        const originalEl = document.getElementById("parent" + chart.id);
        if (!originalEl) continue;
        if (
          originalEl.innerText.trim().toLowerCase().includes("no data found")
        ) {
          continue;
        }

        const realImg = originalEl.querySelector(
          ".cmn-heatmap-wrapper img",
        ) as HTMLImageElement | null;
        const overlayCanvas = originalEl.querySelector(
          ".cmn-heatmap-wrapper canvas",
        ) as HTMLCanvasElement | null;
        const legendCanvas = originalEl.querySelector(
          ".cmn-heatmap-low-high canvas",
        ) as HTMLCanvasElement | null;

        let mergedImgEl: HTMLImageElement | null = null;
        if (realImg && overlayCanvas) {
          try {
            mergedImgEl = await mergeHeatmap(realImg, overlayCanvas);
            // ensure size:
            if (mergedImgEl) {
              mergedImgEl.width =
                mergedImgEl.naturalWidth || realImg.naturalWidth || 0;
              mergedImgEl.height =
                mergedImgEl.naturalHeight || realImg.naturalHeight || 0;
              mergedImgEl.style.display = "block";
              mergedImgEl.style.maxWidth = "100%";
              mergedImgEl.style.height = "auto";
            }
          } catch (err) {
            console.warn("mergeHeatmap failed for chart", chart.id, err);
          }
        }

        // clone and replace
        const clone = originalEl.cloneNode(true) as HTMLElement;
        if (mergedImgEl) {
          const wrapper = clone.querySelector(".cmn-heatmap-wrapper");
          if (wrapper) {
            // hide original image + canvas, but keep layout intact
            const origImg = wrapper.querySelector("img");
            const overlayCanvasClone = wrapper.querySelector("canvas");

            if (origImg) origImg.style.display = "none";
            if (overlayCanvasClone) overlayCanvasClone.style.display = "none";

            // append merged image but let layout (title, scroll) stay
            const mergedCopy = mergedImgEl.cloneNode(true) as HTMLImageElement;
            mergedCopy.style.position = "absolute";
            mergedCopy.style.top = "0";
            mergedCopy.style.left = "0";
            mergedCopy.style.width = "100%";
            mergedCopy.style.height = "100%";
            mergedCopy.style.zIndex = "2";
            mergedCopy.style.pointerEvents = "none";
            // wrapper.style.position = "relative";
            wrapper.appendChild(mergedCopy);
          }
        }
        const overlayCanvasClone = clone.querySelector(
          ".cmn-heatmap-wrapper canvas",
        );
        if (overlayCanvasClone) overlayCanvasClone.remove();
        if (legendCanvas) {
          try {
            const legendContainer = createLegendContainer(legendCanvas);
            const legendParent = clone.querySelector(".cmn-heatmap-low-high");
            if (legendParent) {
              legendParent.appendChild(legendContainer);
            }
          } catch (err) {
            console.warn("legend conversion failed for", chart.id, err);
          }
        }

        // render clone off-screen with proper waits
        const container = document.createElement("div");
        container.style.position = "absolute";
        container.style.left = "-9999px";
        container.style.top = "-9999px";
        document.body.appendChild(container);
        container.appendChild(clone);

        // ✅ wait for images (including Base64) to fully decode
        const imgs = clone.querySelectorAll("img");
        const decodePromises = Array.from(imgs).map(async (img) => {
          try {
            if (!img.complete) {
              await new Promise((res, rej) => {
                img.onload = res;
                img.onerror = rej;
              });
            }
            if (img.decode) await img.decode(); // ensures base64 is decoded before capture
          } catch (e) {
            console.warn("Image decode failed for", img.src.slice(0, 80), e);
          }
        });
        await Promise.all(decodePromises);

        await waitForImagesToLoad(clone, 6000);
        await new Promise((r) => setTimeout(r, 200));

        if (mergedImgEl?.src?.startsWith("data:image/png;base64")) {
          pdf.addPage();
          pageNumber++;

          // Start after header (logo + Vision Insight)
          currentY = headerHeight + verticalSpacing + 4;

          const contentMaxWidth = pdfWidth - pagePadding * 2;
          const borderPadding = 5;
          const cornerRadius = 2;
          const borderColor = [220, 220, 220];
          const fillColor = [255, 255, 255];
          const lineWidth = 0.4;

          // ===== Capture title + dropdown =====
          let headerHeightPx = 0;
          let headerImgData: string | null = null;

          try {
            const headerEl = originalEl.querySelector(
              ".widget-main-header.heatmap-ddl-header",
            ) as HTMLElement | null;

            if (headerEl) {
              const { dataUrl, width, height } =
                await captureElementWithoutTopCrop(headerEl);

              // calculate scaled PDF height (maintains correct aspect ratio)
              headerImgData = dataUrl;
              headerHeightPx = (height * contentMaxWidth) / width;
            }
          } catch (e) {
            console.warn("Header capture failed for chart", chart.id, e);
          }

          // ===== Capture legend =====
          let legendHeightPx = 0;
          let legendImgData: string | null = null;
          try {
            const legendEl = originalEl.querySelector(
              ".cmn-heatmap-low-high",
            ) as HTMLElement | null;
            if (legendEl) {
              const legendCanvasCapture = await html2canvas(legendEl, {
                backgroundColor: "#fff",
                scale: 2,
                useCORS: true,
              });
              legendImgData = legendCanvasCapture.toDataURL("image/png");
              legendHeightPx =
                (legendCanvasCapture.height * contentMaxWidth) /
                legendCanvasCapture.width;
            }
          } catch (e) {
            console.warn("Legend capture failed for chart", chart.id, e);
          }

          // ===== Calculate heatmap size =====
          let displayWidth = contentMaxWidth - borderPadding * 2;
          let displayHeight =
            (mergedImgEl.naturalHeight * displayWidth) /
            mergedImgEl.naturalWidth;
          if (displayHeight > maxChartHeight) {
            const ratio = maxChartHeight / displayHeight;
            displayWidth *= ratio;
            displayHeight *= ratio;
          }

          // ===== Outer border =====
          const totalHeight =
            borderPadding * 2 +
            headerHeightPx +
            displayHeight +
            legendHeightPx +
            14;

          const borderX = pagePadding;
          const borderY = currentY;

          pdf.setFillColor(fillColor[0], fillColor[1], fillColor[2]);
          pdf.setDrawColor(borderColor[0], borderColor[1], borderColor[2]);
          pdf.setLineWidth(lineWidth);
          pdf.roundedRect(
            borderX,
            borderY,
            contentMaxWidth,
            totalHeight,
            cornerRadius,
            cornerRadius,
            "FD",
          );

          // ===== Draw content inside border =====
          let innerY = borderY + borderPadding;
          const innerX = borderX + borderPadding;
          const availableWidth = contentMaxWidth - borderPadding * 2;

          // Title + dropdown
          if (headerImgData) {
            pdf.addImage(
              headerImgData,
              "PNG",
              innerX,
              innerY,
              availableWidth,
              headerHeightPx,
            );
            innerY += headerHeightPx + 2; // tight vertical spacing
          }
          const imgX = innerX;
          const imgY = innerY;
          const imgW = availableWidth;
          const imgH = displayHeight;
          const imgRadius = 3; // mm, increase for more visible roundness

          // convert mm → px (96dpi base)
          const pxPerMm = (96 * (window.devicePixelRatio || 1)) / 25.4;
          const canvasW = Math.round(imgW * pxPerMm);
          const canvasH = Math.round(imgH * pxPerMm);
          const radiusPx = Math.round(imgRadius * pxPerMm);

          // create canvas and white background
          const roundedCanvas = document.createElement("canvas");
          roundedCanvas.width = canvasW;
          roundedCanvas.height = canvasH;
          const ctx = roundedCanvas.getContext("2d");
          if (!ctx) return;
          // draw white background first
          ctx.fillStyle = "#ffffff";
          ctx.fillRect(0, 0, canvasW, canvasH);

          // create rounded clipping path
          ctx.save();
          ctx.beginPath();
          ctx.moveTo(radiusPx, 0);
          ctx.lineTo(canvasW - radiusPx, 0);
          ctx.quadraticCurveTo(canvasW, 0, canvasW, radiusPx);
          ctx.lineTo(canvasW, canvasH - radiusPx);
          ctx.quadraticCurveTo(canvasW, canvasH, canvasW - radiusPx, canvasH);
          ctx.lineTo(radiusPx, canvasH);
          ctx.quadraticCurveTo(0, canvasH, 0, canvasH - radiusPx);
          ctx.lineTo(0, radiusPx);
          ctx.quadraticCurveTo(0, 0, radiusPx, 0);
          ctx.closePath();
          ctx.clip();

          // draw the image inside the rounded region
          await new Promise<void>((resolve, reject) => {
            const img = new Image();
            img.onload = () => {
              ctx.drawImage(img, 0, 0, canvasW, canvasH);
              ctx.restore();
              resolve();
            };
            img.onerror = reject;
            img.src = mergedImgEl.src;
          });

          // convert to PNG and add to PDF
          const roundedImgData = roundedCanvas.toDataURL("image/png");
          pdf.addImage(
            roundedImgData,
            "PNG",
            imgX,
            imgY,
            imgW,
            imgH,
            undefined,
            "SLOW",
          );

          innerY += imgH + 6;
          // Legend (aligned with image width)
          if (legendImgData) {
            pdf.addImage(
              legendImgData,
              "PNG",
              innerX,
              innerY,
              imgW,
              legendHeightPx,
            );
            innerY += legendHeightPx + 6;
          }

          currentY = innerY + verticalSpacing;
          continue;
        }
      }

      // ---- Render Map/Floor Plan Charts (1 widget per page) ----
      for (let chart of mapFloorCharts) {
        const topOffset = 20; // space below global header
        const safeMargin = 14; // increased margin to prevent cropping
        const borderInset = 1.5; // extra inset inside safe zone
        const borderRadius = 2;
        const headerHeightPx = 20;
        const borderPadding = 6;
        const borderLineWidth = 0.3;

        // Ensure valid page dimensions
        const pageW = pdf.internal.pageSize.getWidth();
        const pageH = pdf.internal.pageSize.getHeight();

        const originalEl = document.getElementById("parent" + chart.id);
        if (!originalEl) continue;

        const reactPdfDoc = originalEl.querySelector(".react-pdf__Document");

        // === CASE 1: react-pdf widgets ===
        if (reactPdfDoc) {
          // Find the parent that wraps both react-pdf__Document and overlay widget
          const captureTarget = reactPdfDoc.parentElement as HTMLElement; // this includes overlays like .common-map-inner-pop

          // Wait to ensure everything is rendered
          await new Promise((res) => setTimeout(res, 300));

          // Ensure overlays are visible for html2canvas
          captureTarget.querySelectorAll("*").forEach((el) => {
            el.style.transform = "none";
            el.style.opacity = "1";
          });

          // Capture full widget (floor plan + overlays)
          const canvas = await html2canvas(captureTarget, {
            scale: 2,
            useCORS: true,
            backgroundColor: "#ffffff",
            logging: false,
            removeContainer: true,
            allowTaint: true,
          });

          const pdfImgData = canvas.toDataURL("image/png");

          // === same title & dropdown handling as before ===
          const titleEl = originalEl.querySelector(
            ".MuiTypography-h6, .MuiTypography-root.MuiTypography-h6, h2, h6",
          );
          const dropdownEl = originalEl.querySelector(".MuiSelect-select");
          const titleText = titleEl ? titleEl.textContent?.trim() || "" : "";
          const dropdownText = dropdownEl
            ? dropdownEl.textContent?.trim() || ""
            : "";

          // Add new page for this chart
          pdf.addPage();
          pageNumber++;
          currentY = headerHeight + verticalSpacing + topOffset;

          const availableWidth = pageW - pagePadding * 2 - safeMargin * 2;
          let displayWidth = availableWidth;
          let displayHeight = (canvas.height * displayWidth) / canvas.width;

          if (displayHeight > maxChartHeight) {
            const ratio = maxChartHeight / displayHeight;
            displayWidth *= ratio;
            displayHeight *= ratio;
          }

          const headerLineStartX = pagePadding;
          const headerLineEndX = pageW - pagePadding;
          const boxX = headerLineStartX;
          const boxWidth = headerLineEndX - headerLineStartX;
          const safeBoxX = Math.max(boxX + borderInset, 8);
          const offsetX = boxX + Math.max(0, (boxWidth - displayWidth) / 2);
          const boxY = currentY - headerHeightPx + 5;
          const boxHeight = displayHeight + borderPadding * 2 + headerHeightPx;

          // Border box
          pdf.setLineWidth(borderLineWidth);
          pdf.setDrawColor(200, 200, 200);
          pdf.setFillColor(255, 255, 255);
          pdf.roundedRect(
            safeBoxX,
            boxY + 0.6,
            boxWidth - borderInset * 2,
            boxHeight - 1.2,
            borderRadius,
            borderRadius,
            "FD",
          );

          // Title
          const headerY = boxY + 9;
          pdf.setFontSize(14.5);
          pdf.setFont("helvetica", "normal");
          pdf.setTextColor(40, 40, 40);
          if (titleText) pdf.text(titleText, safeBoxX + 4, headerY);

          // Dropdown
          if (dropdownText) {
            const dropdownPadding = 4;
            const textWidth = pdf.getTextWidth(dropdownText);
            const dropdownBoxWidth = Math.min(
              textWidth + dropdownPadding * 2 + 10,
              boxWidth - 24,
            );
            const dropdownBoxHeight = 9;
            const dropdownBoxX = Math.max(
              safeBoxX + 8,
              safeBoxX + boxWidth - dropdownBoxWidth - 8,
            );
            const dropdownBoxY = boxY + 4;

            pdf.setLineWidth(0.25);
            pdf.setDrawColor(210, 210, 210);
            pdf.setFillColor(255, 255, 255);
            pdf.roundedRect(
              dropdownBoxX,
              dropdownBoxY,
              dropdownBoxWidth,
              dropdownBoxHeight,
              5,
              5,
              "FD",
            );

            pdf.setFontSize(12);
            pdf.setTextColor(120, 120, 120);
            pdf.setFont("helvetica", "normal");
            pdf.text(
              dropdownText,
              dropdownBoxX + dropdownPadding + 1,
              dropdownBoxY + dropdownBoxHeight * 0.73,
            );

            pdf.setLineWidth(borderLineWidth);
          }

          // Add image (includes overlays)
          pdf.addImage(
            pdfImgData,
            "PNG",
            offsetX + borderInset,
            currentY,
            displayWidth,
            displayHeight,
          );

          currentY += displayHeight + verticalSpacing;
          continue;
        }

        // === CASE 2: react-image widgets ===
        if (!reactPdfDoc && chart.imgData) {
          // Add new page once per chart
          pdf.addPage();
          pageNumber++;

          // --- layout settings ---
          const topOffset = 2; // slightly more top space to avoid cropping
          currentY = headerHeight + verticalSpacing + topOffset;

          const availableWidth = pageW - pagePadding * 2 - safeMargin * 2;
          let displayWidth = availableWidth + 25;
          let displayHeight =
            (chart.originalHeight * displayWidth) / chart.originalWidth;

          if (displayHeight > maxChartHeight) {
            const ratio = maxChartHeight / displayHeight;
            displayWidth *= ratio;
            displayHeight *= ratio;
          }

          // --- improved safe margins ---
          const boxX = pagePadding;
          const boxWidth = pageW - pagePadding * 2;
          const offsetX = boxX + Math.max(0, (boxWidth - displayWidth) / 2);

          // --- image ---
          pdf.addImage(
            chart.imgData,
            "PNG",
            offsetX + borderInset - 2,
            currentY + 4, // pushed down slightly so border top is visible
            displayWidth,
            displayHeight,
          );

          currentY += displayHeight + verticalSpacing;
        }
      }

      // After all pages are added, update header/footer to reflect total pages
      const totalPages = pageNumber;
      for (let p = 1; p <= totalPages; p++) {
        pdf.setPage(p);
        addHeaderFooter(p, totalPages); // now with real total pages count
      }

      const now = new Date();
      const day = String(now.getDate()).padStart(2, "0");
      const month = String(now.getMonth() + 1).padStart(2, "0"); // Months are 0-based
      const year = now.getFullYear();
      const fileName = `Dashboard_Widgets_${day}_${month}_${year}.pdf`;
      //LoadingManager.hideLoading();
      pdf.save(fileName);
      onClose();
    } catch (error) {
      LoadingManager.hideLoading();
      console.error("Error exporting PDF:", error);
    } finally {
      setExporting(false); // stop refresh
      setIsExportPdfClicked(false); // Reset the flag after export
      LoadingManager.hideLoading();
    }
  };

  const handleChange = (event: any) => {
    const value = event.target.value;

    if (value.includes("all")) {
      const allOptionIds = selectedWidgetsOptions
        .filter((opt) => opt.id !== "all")
        .map((opt) => opt.id);
      const isAllSelected = allOptionIds.every((id) =>
        selectedWidgets.includes(id),
      );
      setSelectedWidgets(isAllSelected ? [] : allOptionIds);
    } else {
      setSelectedWidgets(value);
    }
  };
  const isAllSelected = selectedWidgetsOptions
    .filter((o) => o.id !== "all")
    .every((o) => selectedWidgets.includes(o.id));

  const handleRulesChange = (newRules: CalculatorExportRule[]) => {
    setRules(newRules);
    // Do whatever you need with the rules data
  };

  // const exportWidget = () => {
  //   const filteredLayouts = layouts?.lg?.filter((item: any) =>
  //     selectedWidgets.includes(item.chartID),
  //   );

  //   onClose();
  //   navigate("/advance-pdf", {
  //     state: {
  //       rules,
  //       filteredLayouts: filteredLayouts ?? [],
  //       FloorIds: selectedFloors || [],
  //       ZoneIds: selectedZones || [],
  //       StartDate: selectedStartDate,
  //       EndDate: selectedEndDate,
  //     },
  //   });
  // };

  const exportWidget = () => {
    if (selectedWidgets && selectedWidgets.length === 0) {
      showToast(t("Advance_Export_Dialog.selectAtLeastOneWidget"), "error");
      return;
    }

    const filteredLayouts = layouts?.lg?.filter((item: any) =>
      selectedWidgets.includes(item.chartID),
    );
    const cleanedRules =
      rules?.filter((r: any) => r?.value !== undefined && r?.value !== null && r?.value !== "") || [];

    const exportData = {
      rules : cleanedRules.length > 0 ? cleanedRules : [],
      filteredLayouts: filteredLayouts ?? [],
      FloorIds: selectedFloors || [],
      ZoneIds: selectedZones || [],
      // StartDate: selectedStartDate,
      // EndDate: selectedEndDate,
      StartDate: dayjs(selectedStartDate).format("YYYY-MM-DD HH:mm:ss"),
      EndDate: dayjs(selectedEndDate).format("YYYY-MM-DD HH:mm:ss"),
    };
    localStorage.removeItem("advancePdfData");
    // Store data temporarily
    localStorage.setItem("advancePdfData", JSON.stringify(exportData));

    onClose();

    // Open in new tab
    window.open("/advance-pdf", "_blank");
  };

  return (
    <Dialog
      open={open}
      onClose={(event, reason) => {
        if (reason !== "backdropClick") {
          onClose();
        }
      }}
      sx={{
        "& .MuiPaper-root": { borderRadius: "24px", position: "relative" },
      }}
      className="cmn-pop-design-parent advance-report-popup"
      maxWidth={"lg"}
    >
      <DialogTitle className="cmn-pop-header">
        <Typography component="h1" variant="h5">
          {t("Advance_Export_Dialog.Advance_Report")}
        </Typography>
      </DialogTitle>
      <DialogContent>
        <DialogContentText>
          <FormControl fullWidth margin="normal">
            <div className="advance-report-wrapper">
              <Label sx={{ display: "block" }}>
                {" "}
                {t("Advance_Export_Dialog.Widgets")}
              </Label>
              <Select
                displayEmpty
                multiple
                value={selectedWidgets}
                onChange={handleChange}
                input={<OutlinedInput />}
                renderValue={(selected) => {
                  if (selected.length === 0) {
                    return (
                      <span style={{ color: "#aaa" }}>
                        {t("Advance_Export_Dialog.Select_widgets")}
                      </span>
                    ); // placeholder text
                  }
                  const selectedTitles = selected
                    .map(
                      (id) =>
                        selectedWidgetsOptions.find((o) => o.id === id)?.title,
                    )
                    .filter(Boolean);

                  return selectedTitles.join(", ");
                  // return (
                  //   <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                  //     {selected.map((id) => (
                  //       <Chip
                  //         key={id}
                  //         label={
                  //           selectedWidgetsOptions.find((o) => o.id === id)?.title
                  //         }
                  //       />
                  //     ))}
                  //   </Box>
                  // )
                }}
              >
                {selectedWidgetsOptions.map((option) => (
                  <MenuItem key={option.id} value={option.id}>
                    <Checkbox
                      checked={
                        option.id === "all"
                          ? isAllSelected
                          : selectedWidgets.includes(option.id)
                      }
                    />
                    <ListItemText primary={option.title} />
                  </MenuItem>
                ))}
              </Select>
              <AdvanceExportCalculator onRulesChange={handleRulesChange} />
            </div>
            <DialogActions className="advance-report-buttons-wrapper">
              <Button
                className="common-btn-design common-btn-design-transparent"
                onClick={() => {
                  setSelectedWidgets([]); // clear only when cancel
                  onClose();
                }}
                sx={{
                  backgroundColor: "#F9F9FA",
                  color: "#424242",
                  textTransform: "none",
                }}
              >
                {t("Common_DELETE_Confirmation_Dialog.Cancel")}
              </Button>
              <Button
                className="common-btn-design"
                onClick={onExportCSV}
                sx={{
                  background: "linear-gradient(to right, #FF8A00, #FE6500)",
                  color: "white",
                  textTransform: "none",
                }}
                autoFocus
              >
                {t("Advance_Export_Dialog.Export_CSV")}
              </Button>
              {/* <Button
                className="common-btn-design"
                onClick={async () => {
                  try {
                    await onExportPDF();
                    //onClose();
                  } catch (err) {
                    LoadingManager.hideLoading();
                    console.error("Export failed", err);
                  }
                }}
                sx={{
                  background: "linear-gradient(to right, #FF8A00, #FE6500)",
                  color: "white",
                  textTransform: "none",
                }}
                autoFocus
              >
                {t("Advance_Export_Dialog.Export_PDF")}
              </Button> */}
              <Button className="common-btn-design" onClick={exportWidget}>
                {t("Advance_Export_Dialog.Export_PDF")}
              </Button>
            </DialogActions>
          </FormControl>
        </DialogContentText>
      </DialogContent>
    </Dialog>
  );
};

export default ExportWidgetDialog;
