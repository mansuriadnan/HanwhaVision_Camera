import React, { useEffect, useMemo } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { Typography } from "@mui/material";
import { CalculatorExportRule } from "./AdvanceExportCalculator";
import { WidgetComponentMap } from "../../components/Dashboard/Chart_Polygon/ReportSchedulerWidgetPDF/WidgetMap";
import dayjs from "dayjs";
import {
  GetAllFloorsListService,
  GetAllZonesByFloorIdService,
} from "../../services/dashboardService";
import html2pdf from "html2pdf.js";
import { convertBase64ToPngBase64, convertImageToBase64 } from "../../utils/convertImageToBase64";
import { useRequestCounter } from "../../hooks/useHttpInterceptor";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { formatDate } from "../../utils/dateUtils";

export interface ExportAdvancePDFLocationState {
  rules: CalculatorExportRule[];
  filteredLayouts: any[];
  FloorIds: any[];
  ZoneIds: any[];
  StartDate: string | null;
  EndDate: string | null;
}

const ExportAdvancePDF: React.FC = () => {
  const [floorNames, setFloorNames] = React.useState<string>("");
  const [zoneNames, setZoneNames] = React.useState<string>("");
  const [widgetNames, setWidgetNames] = React.useState<string>("");
  const [userProfileDetails, setUserProfileDetails] = React.useState<any>();
  const [finalRulesValue, setFinalRulesValue] = React.useState<number>(0);
   const { pendingCount, isAllComplete } = useRequestCounter();
  // const { state } = useLocation();
  // const data = state as ExportAdvancePDFLocationState | null;

  const storedData = localStorage.getItem("advancePdfData");
  const exportStarted = React.useRef(false);
  const location = useLocation();
  const navigate = useNavigate();
  const { timeFormat } = useTimeFormatContext();

  const data: ExportAdvancePDFLocationState | null = useMemo(()=>{
return storedData ? JSON.parse(storedData) : null;
  },[storedData])
  

  // const  = data?.rules ?? [];
  // const filteredLayouts = useMemo(()=> data?.filteredLayouts ?? [],[data?.filteredLayouts]);
  // const FloorIds = data?.FloorIds ?? [];
  // const ZoneIds = data?.ZoneIds ?? [];
  // const StartDate = data?.StartDate;
  // const EndDate = data?.EndDate;
  const {rules = [],filteredLayouts = [],FloorIds = [],ZoneIds = [],StartDate,EndDate} = useMemo(()=>{
return data?? {rules: [],filteredLayouts: [],FloorIds: [],ZoneIds: [],StartDate: null,EndDate: null};
  },[data ?? {rules: [],filteredLayouts: [],FloorIds: [],ZoneIds: [],StartDate: null,EndDate: null}])

  useEffect(() => {
    const body = document.body;

    if (body.classList.contains("dark")) {
      body.classList.remove("dark");
    }

    return () => {
      body.classList.add("dark");
    };
  }, []);


  useEffect(() => {
    const prepareData = async () => {

      const userProfile = localStorage.getItem("userProfile");
      if (userProfile) {
        setUserProfileDetails(JSON.parse(userProfile));
      }
      await fetchFloorData();

      if (FloorIds?.length > 0) {
        await fetchZoneData(FloorIds);
      }
      await waitForWidgets();
      //await new Promise((r) => setTimeout(r, 1500));

      await new Promise((resolve) =>
        requestAnimationFrame(() =>
          requestAnimationFrame(() =>
            requestAnimationFrame(resolve)
          )
        )
      );

      exportToPDF();

    };

    if (pendingCount === 0 && !exportStarted.current) {
      exportStarted.current = true;
      prepareData();
    }
  }, [pendingCount]);

  useEffect(() => {
    if (!filteredLayouts?.length) {
      setWidgetNames("");
      return;
    }

    const names = filteredLayouts
      .map((item: any) => item?.displayName || item?.chartName)
      .filter(Boolean);

    const uniqueNames = [...new Set(names)].join(", ");
    setWidgetNames(uniqueNames);
  }, [filteredLayouts]);

  useEffect(() => {
    if (!rules?.length) {
      setFinalRulesValue(0);
      return;
    }

    const result = rules.reduce((total: number, rule: any) => {
      const value = Number(rule?.value) || 0;

      if (rule?.operation === "Add") {
        return total + value;
      }

      if (rule?.operation === "Subtract") {
        return total - value;
      }

      return total;
    }, 0);

    setFinalRulesValue(result);
  }, [rules]);

  const fetchFloorData = async () => {
    try {
      const response = await GetAllFloorsListService(true);
      const floors = response ?? [];

      if (FloorIds?.length > 0) {
        const matchedNames = floors
          .filter((floor: any) => FloorIds.includes(floor.id))
          .map((floor: any) => floor.floorPlanName)
          .join(", ");

        setFloorNames(matchedNames);
      }
    } catch (err) {
      console.error("Error while fetching floor data");
    }
  };

  const fetchZoneData = async (floorIds: string[]) => {
    if (!Array.isArray(floorIds) || floorIds.length === 0) {
      setZoneNames("");
      return;
    }

    try {
      const response: any = await GetAllZonesByFloorIdService(floorIds);
      const allZones = (response?.data ?? []).flatMap((floor: any) =>
        Array.isArray(floor?.zones)
          ? floor.zones.map((zone: any) => ({
              id: zone.id,
              title: zone.zoneName,
            }))
          : [],
      );

      if (ZoneIds?.length > 0) {
        const matchedZones = allZones
          .filter((zone: any) => ZoneIds.map(String).includes(String(zone.id)))
          .map((zone: any) => zone.title)
          .join(", ");

        setZoneNames(matchedZones);
      }
    } catch (err: any) {
      console.error("Error while fetching the zone data:", err?.message || err);
      setZoneNames("");
    }
  };

  const formatDateRange = (date: string): string => {
    return dayjs(date).format("MMM DD YYYY hh:mm A");
  };

  const rows = useMemo(() => {
    const advanceConfig =
    rules && rules.length > 0
      ? rules
          .map((r: any) => {
            const sign = r.operation === "Add" ? "+" : "-";
            return `${r.category}: ${sign}${r.value}%`;
          })
          .join("\n")
      : "N/A";
   return [
    {
      label: "Selected Floors",
      value: floorNames || "N/A",
    },
    {
      label: "Selected Zones",
      value: zoneNames || "N/A",
    },
    {
      label: "Selected Widgets",
      value: widgetNames || "N/A",
    },
    {
      label: "Selected Date",
      value:
        StartDate && EndDate
          ? `${formatDate(formatDateToConfiguredTimezone(StartDate),timeFormat)} - ${formatDate(formatDateToConfiguredTimezone(EndDate),timeFormat)}`
          : "N/A",
    },
    {
      label: "Advance Configuration",
      value: advanceConfig,
    },
    {
      label: "Created By",
      value: `${userProfileDetails?.firstname} ${
        userProfileDetails?.lastname || ""
      }`,
    },
    { label: "Created On", value: formatDate(formatDateToConfiguredTimezone(dayjs().toISOString()), timeFormat) },
  ]}, [floorNames, zoneNames, widgetNames, StartDate, EndDate, userProfileDetails,rules]);

const waitForWidgets = async () => {
  return new Promise<void>((resolve) => {
    const start = Date.now();
    const maxWait = 15000; // allow more time when many widgets

    const interval = setInterval(() => {
      const widgets = document.querySelectorAll(".widge-box-inner");

      const readyWidgets = Array.from(widgets).filter((widget) => {
        const svg = widget.querySelector("svg");

        if (!svg) return false;
        const hasSize =
          svg.clientWidth > 0 &&
          svg.clientHeight > 0;
        // check if chart actually has drawn elements
         const drawnElements =
          svg.querySelectorAll("path, rect, circle, line, text").length > 5;

        return hasSize && drawnElements;
      });

      if (readyWidgets.length >= filteredLayouts.length) {
        clearInterval(interval);
        resolve();
      }

      // fallback timeout
      if (Date.now() - start > maxWait) {
        clearInterval(interval);
        resolve();
      }
    }, 1000);
  });
};
  const exportToPDF = async () => {
    const element = document.getElementById("advance-pdf-content");
    if (!element) return;
    await waitForWidgets();
    // Force SVG layout
    document.querySelectorAll("svg").forEach((svg) => {
      svg.getBoundingClientRect();
    });

    // Wait for browser paint
    await new Promise((resolve) =>
      requestAnimationFrame(() =>
        requestAnimationFrame(() => resolve(null))
      )
    );


  requestAnimationFrame(async () => {
    const opt = {
      margin: [20, 10, 20, 10] as [number, number, number, number],
      filename: `Report_${dayjs().format("DD-MM-YYYY_HH-mm")}.pdf`,
      image: { type: "jpeg" as const, quality: 0.98 },
      html2canvas: {
        scale: 2,
        useCORS: true,
        scrollX: 0,
        scrollY: 0,
        logging: false,
        // windowWidth: document.body.scrollWidth,
        // windowHeight: document.body.scrollHeight
      },
      jsPDF: {
        unit: "mm",
        format: "a4" as const,
        orientation: "portrait" as const,
      },
      pagebreak: { mode: ["css", "legacy"] },
      
    };
    // Inject stronger CSS rules to discourage page breaks inside widget containers.
    // This helps html2pdf/html2canvas respect the "avoid" intent for complex widgets.
    const styleId = "pdf-pagebreak-fix-style";
    let injectedStyle = document.getElementById(styleId) as HTMLStyleElement | null;
    if (!injectedStyle) {
      injectedStyle = document.createElement("style");
      injectedStyle.id = styleId;
      injectedStyle.type = "text/css";
      injectedStyle.appendChild(
          document.createTextNode(`
          .widge-box-inner, .widge-box-inner * {
            page-break-inside: avoid !important;
            break-inside: avoid !important;
            -webkit-column-break-inside: avoid !important;
            -webkit-page-break-inside: avoid !important;
          }
          .widge-box-inner { page-break-after: auto !important; display: inline-block; vertical-align: top; }
          .widge-box-inner-pdf { page-break-inside: avoid !important; }
 
 
 
          .widge-box-inner {
  position: relative;
  padding-top: 10px; /* spacing instead of border */ border:none !important ;
}
.widge-box-inner > div > div:first-child:before{ display: none; }
.widge-box-inner::before {
  content: "";
  position: absolute;
  top: 10px; left: 0px; right: 0px; bottom: 0px;
  border-bottom: 1px solid #c9c6c6; z-index: 99;
}
.widget-main-footer-value p {
  font-size: 12px;
}
 .widge-box-inner .widget-label-bottom img {
width: 16px;  height: 16px; min-height: 16px; max-height: 16px; line-height: 16px; display: block; max-width: 16px; min-width: 16px; object-fit:contain; 
}
    #advance-pdf-content {
  padding-bottom: 20px !important;
}
 .report-table-sch {
  page-break-inside: auto !important;
}

.report-table-sch tr {
  page-break-inside: avoid !important;
  page-break-after: auto !important;
}
        `)
      );
      document.head.appendChild(injectedStyle);
    }

    try {
      const worker = html2pdf().set(opt).from(element);

      const pdf = await worker.toPdf().get("pdf");

      const totalPages = pdf.internal.getNumberOfPages();

      const pageWidth = pdf.internal.pageSize.getWidth();
      const pageHeight = pdf.internal.pageSize.getHeight();

      const logoUrl = await convertBase64ToPngBase64("/images/vision_insight_logo_pdf.png"); 

      for (let i = 1; i <= totalPages; i++) {
        pdf.setPage(i);

        // ===== HEADER LEFT TEXT =====
        const generalSettings = localStorage.getItem("generalSettings");

        if (generalSettings) {
          const parsed = JSON.parse(generalSettings);

          if (parsed?.logo) {
            const pngBase64Applogo = await convertBase64ToPngBase64(parsed.logo);

            pdf.addImage(
              pngBase64Applogo,
              "PNG",
              10,
              5,
              30,
              12
            );
          }
        }

        // ===== HEADER RIGHT LOGO =====
        // You can adjust width/height
        const logoWidth = 40;
        const logoHeight = 10;

        pdf.addImage(
          logoUrl,
          "PNG",
          pageWidth - logoWidth - 10, 
          5,
          logoWidth,
          logoHeight
        );

        // ===== HEADER LINE =====
        pdf.setDrawColor(180);
        pdf.setLineWidth(0.3);
        pdf.line(10, 20, pageWidth - 10, 20);
        
        // ===== FOOTER =====
        pdf.setFontSize(10);
        pdf.text(
          `Page ${i} of ${totalPages}`,
          pageWidth -35,
          pageHeight - 10,
        );
      }
      
      await worker.save();

      localStorage.removeItem("advancePdfData");
    } catch (err) {
      console.error("PDF generation failed", err);
    }finally {
      // Clean up injected style after PDF generation
      if (injectedStyle && injectedStyle.parentNode) injectedStyle.parentNode.removeChild(injectedStyle);
      // if (window.opener) {
      //   setTimeout(() => {
      //     window.close();
      //   }, 1000); // small delay ensures download starts
      // }
      const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent);

      const location = window.location;
      const params = new URLSearchParams(location.search);
      const id = params.get("id");

      setTimeout(() => {
        if (window.opener && !isIOS) {
          window.close();
        } else {
          window.location.href = `/dashboard${id ? `?id=${id}` : ""}`;
        }
      }, 1200)
    }
  });
};

  return (
    <div id="advance-pdf-content" style={{ width: "100%" }}>
      {/* <button
        onClick={async () => {
          await waitForWidgets();

          await new Promise((r) => setTimeout(r, 1200));

          await new Promise((resolve) =>
            requestAnimationFrame(() =>
              requestAnimationFrame(() =>
                requestAnimationFrame(resolve)
              )
            )
          );

          exportToPDF();
        }}
      >
        Generate PDF
      </button> */}
      <div
        className="widge-box-inner-pdf"
        style={{
          maxWidth: "190mm",
          margin: "0 auto",
          pageBreakInside: "avoid",
          pageBreakAfter: "avoid",
        }}
      >
        <div
          style={{
            pageBreakInside: "avoid",
            pageBreakAfter: "avoid",
          }}
        >
          <div
            style={{
              marginBottom: "20px",
              pageBreakInside: "avoid",
              pageBreakAfter: "avoid",
            }}
          >
            <Typography style={{ fontWeight: 600, marginBottom: "10px" }}>
              Applied Filters:
            </Typography>
            <table
              className="report-table-sch"
              style={{
                width: "100%",
                borderCollapse: "separate",
                borderSpacing: "0px",
                fontSize: "14px",
                borderRadius: "10px",
                overflow: "hidden",
                border: "1px solid #ccc",
              }}
            >
              <thead>
                <tr style={{ backgroundColor: "#f5f5f5" }}>
                  <th
                    style={{
                      padding: "0px 8px",
                      textAlign: "left",
                      //border: "none",
                      borderBottom: "1px solid #ccc",
                      borderRight: "1px solid #ccc",
                      width: "20%",
                      fontWeight: 600,
                    }}
                  >
                    Parameter
                  </th>
                  <th
                    style={{
                       padding: "0px 8px",
                      textAlign: "left",
                      borderBottom: "1px solid #ccc",
                      fontWeight: 600,
                    }}
                  >
                    Value
                  </th>
                </tr>
              </thead>
              <tbody>
                {rows.map((row, index) => {
                  const isFirst = index === 0;
                  const isLast = index === rows.length - 1;

                  return (
                    <tr key={index}>
                      <td
                        style={{
                           padding: "0px 8px",
                          verticalAlign: "top",
                          borderTop: isFirst ? "none" : "1px solid #ccc", // no top border on first row
                          borderLeft: "none", // let outer border show
                          borderRight: "1px solid #ccc",
                          // borderBottom: "1px solid #ccc",
                        }}
                      >
                        {row.label}
                      </td>
                      <td
                        style={{
                           padding: "0px 8px",
                          whiteSpace: "pre-wrap",
                          borderTop: isFirst ? "none" : "1px solid #ccc",
                          borderLeft: "none",
                          borderRight: "none", // rightmost cell — outer border will show
                          // borderBottom: "1px solid #ccc",
                        }}
                      >
                        {row.value}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          <div style={{ marginBottom: "10px" }}>
            <div
              style={{
                pageBreakInside: "avoid", // <== Force this block to stay together
                breakInside: "avoid",
              }}
            >
              <Typography style={{ marginBottom: "10px", fontWeight: "600" }}>
                Visualizations:
              </Typography>

              <div
                style={{
                  // display: "flex",
                  // flexWrap: "wrap",
                  // gap: "10px",
                  // marginBottom: "10px",
                  // justifyContent: "flex-start",
                  // pageBreakInside: "avoid",
                  // breakInside: "avoid",
                   marginLeft: "-10px",
                 marginRight: "-10px",
                }}
                // style={{
                //   marginBottom: "10px",
                //   fontSize: 0, // removes inline-block spacing
                //   gap: "10px",
                // }}
                // className="widget-content-wrap"
              >
                {filteredLayouts
                  // .sort((a, b) => (a.size === "2x1" ? -1 : 1))
                  .sort((a, b) => {
                    const forceFullWidthWidgets = [
                      "People",
                      "People In Out",
                      "Vehicle",
                      "Vehicle In Out",
                      "Vehicle by Type",
                      "Gender",
                      "Counting for forklift",
                    ];

                    const isAFull =
                      a.size === "2x1" || forceFullWidthWidgets.includes(a.chartName);
                    const isBFull =
                      b.size === "2x1" || forceFullWidthWidgets.includes(b.chartName);

                    if (isAFull && !isBFull) return -1; // A first
                    if (!isAFull && isBFull) return 1;  // B first
                    return 0; // keep original order
                  })
                  .map((item) => {
                    // console.log("item->", item);

                    if (!item?.chartName) return null;

                    const Component = WidgetComponentMap[item?.chartName];
                    if (!Component) return null;

                    const forceFullWidthWidgets = [
                      "People",
                      "People In Out",
                      "Vehicle",
                      "Vehicle In Out",
                      "Vehicle by Type",
                      "Gender",
                      "Counting for forklift",
                    ];

                    const isFullWidth =
                      item?.size === "2x1" ||
                      forceFullWidthWidgets.includes(item?.chartName);

                    //   const item = {
                    //     chartID: firstIndex + idx,
                    //     chartName: widgetTitle,
                    //     displayName: widgetTitle,
                    //     expanded: "Option1",
                    //     height: 358,
                    //     size: "1x1",
                    //     width: 364,
                    //   };

                  return (
                    
                    <div
                      key={item?.i}
                      // style={{
                      //   // width: item?.size === "2x1" ? "100%" : "48%",  
                      //   width: item?.size === "2x1" ? "100%" : "calc(50% - 5px)",
                      //   pageBreakInside: "avoid",
                      //   breakInside: "avoid",
                      //   display: "inline-block",
                      // }}
                      style={{
                        width: isFullWidth  ? "calc(100% - 20px)" : "calc(50% - 20px)",
                        // height: item?.size === "2x1" ? "100%" : "calc(50% - 10px)",
                        // display: "inline-block",
                        // verticalAlign: "top",
                        // padding: "10px",
                        // boxSizing: "border-box",
                        // pageBreakInside: "avoid",
                        // breakInside: "avoid",
                        marginTop: "20px",
                        marginBottom: "20px",
                        marginLeft: "10px",
                        marginRight: "10px",                      
                      }}
                      // className="widget-content-pdf"
                      className="widge-box-inner"
                    >
                  <Component
                    item={item}
                    floor={FloorIds}
                    zones={ZoneIds}
                    selectedStartDate={StartDate}
                    selectedEndDate={EndDate}
                    pdfMode={true}
                    finalRulesValue={finalRulesValue}
                  />
                    </div>
                  );
                })}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ExportAdvancePDF;
