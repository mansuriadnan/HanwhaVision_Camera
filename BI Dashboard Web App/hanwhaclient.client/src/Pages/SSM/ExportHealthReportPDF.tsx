import React, { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import html2pdf from "html2pdf.js";
import { convertBase64ToPngBase64 } from "../../utils/convertImageToBase64";
import { exportHealthReportService } from "../../services/SSMService";
import { HealthReportRequest } from "../../interfaces/IManageServer";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { formatDate } from "../../utils/dateUtils";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { ApiDevice, ApiServer, DerivedOverview, HealthReportRouteData, ServerStatus } from "../../interfaces/IHealthReport";


const diffMinutes = (start: string, end: string): number => {
  try {
    return Math.round((new Date(end).getTime() - new Date(start).getTime()) / 60000);
  } catch {
    return 0;
  }
};

const deriveOverview = (servers: ApiServer[], siteNamesOverride?: string[]): DerivedOverview => {
  const totalServer = servers.length;
  const totalCamera = servers.reduce((s, sv) => s + sv.totalCameras, 0);

  const avgServerUptime =
    totalServer > 0
      ? (servers.reduce((s, sv) => s + sv.uptimePercent, 0) / totalServer).toFixed(1)
      : "0.0";

 
  const avgCPU =
   totalServer > 0
      ? (servers.reduce((s, sv) => s + sv.cpuAvgPercent, 0) / totalServer).toFixed(1)
      : "0.0";

  const avgRAM =
    totalServer > 0
      ? (servers.reduce((s, sv) => s + sv.ramAvgPercent, 0) / totalServer).toFixed(1)
      : "0.0";

    const sitesLabel = siteNamesOverride?.length
    ? siteNamesOverride.join(", ")
    : [...new Set(servers.map((sv) => sv.siteName))].join(", ")

  return { totalServer, totalCamera, avgServerUptime, avgCPU, avgRAM, sitesLabel };
};

const StatusBadge: React.FC<{ status: ServerStatus }> = ({ status }) => {
  const cls = status === "Connected" ? "badge-healthy" : status === "Disconnected" ? "badge-critical" : "badge-warning";
  return <span className={`ssm-badge ${cls}`}>{status.toUpperCase()}</span>;
};

const UptimeCell: React.FC<{ value: string | number }> = ({ value }) => {
  const num     = typeof value === "number" ? value : parseFloat(value as string);
  const display = typeof value === "number" ? `${value.toFixed(2)}%` : value;
  return <span className={num < 99 ? "uptime-warn" : "uptime-ok"}>{display}</span>;
};

const formatDiskSpace = (mb: number): string => { // value comes from SSM server in MB unit
  let value = mb;
  let unit: string = "MB";

  if (value >= 1024) {
    value /= 1024;
    unit = "GB";
  }

  if (value >= 1024) {
    value /= 1024;
    unit = "TB";
  }

  return `${value.toFixed(2)} ${unit}`;
};

const CameraRow: React.FC<{ cam: ApiDevice }> = ({ cam }) => {
  const statusStr = (cam.status?.toUpperCase() ?? "Connected") as ServerStatus;
  return (
    <>
      <tr style={{ backgroundColor: "#FFF2E2" }}>
        <td>{cam.ip}</td>
        <td>{cam.name}</td>
        <td>{cam.model}</td>
        <td><UptimeCell value={cam.online} /></td>
        <td>{cam.offlineIncidence}</td>
        <td>{cam.offlineDuration}</td>
        <td><UptimeCell value={cam.rec} /></td>
        <td>{cam.noRecIncidence}</td>
        <td>{cam.noRecDuration}</td>
        <td><StatusBadge status={statusStr} /></td>
      </tr>

      {/* Offline events */}
      <tr>
        <td colSpan={10}>
          <div className="ssm-camera-expand">
            <table className="ssm-inner-table">
              <thead>
                <tr>
                  <th style={{ color: "#FF0004" }}>Offline</th>
                  <th>Sr.</th>
                  <th>Offline Date Time</th>
                  <th>Online Date Time</th>
                  <th>Duration</th>
                </tr>
              </thead>
              <tbody>
                {cam.offlineEvents.length === 0 ? (
                  <tr><td colSpan={5} style={{ color: "#aaa", padding: "4px 8px",  textAlign: "center"}}>No offline events</td></tr>
                ) : cam.offlineEvents.map((ev, i) => (
                  <tr key={ev.id}>
                    <td></td>
                    <td>{i + 1}</td>
                    <td>{formatDateToConfiguredTimezone(ev.offlineTime)}</td>
                    <td>{formatDateToConfiguredTimezone(ev.onlineTime)}</td>
                    <td>{ev.duration > 0 ? `${ev.duration} min` : `${diffMinutes(ev.offlineTime, ev.onlineTime)} min`}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </td>
      </tr>

      {/* Stop Recording events */}
      <tr>
        <td colSpan={10}>
          <div className="ssm-camera-expand">
            <table className="ssm-inner-table ">
              <thead>
                <tr>
                  <th style={{ color: "#FF8A01" }}>Recording</th>
                  <th>Sr.</th>
                  <th>No-Recording Date Time</th>
                  <th>Resume Date Time</th>
                  <th>Duration</th>
                </tr>
              </thead>
              <tbody>
                {cam.stopRecording.length === 0 ? (
                  <tr><td colSpan={5} style={{ color: "#aaa", padding: "4px 8px", textAlign: "center"}}>No recording stops</td></tr>
                ) : cam.stopRecording.map((ev, i) => (
                  <tr key={ev.id}>
                    <td></td>
                    <td>{i + 1}</td>
                    <td>{formatDateToConfiguredTimezone(ev.stopRecordingTime)}</td>
                    <td>{formatDateToConfiguredTimezone(ev.startRecordingTime)}</td>
                    <td>{ev.duration > 0 ? `${ev.duration} min` : `${diffMinutes(ev.stopRecordingTime, ev.startRecordingTime)} min`}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </td>
      </tr>

      <tr style={{ height: "12px" }}>
        <td colSpan={10} style={{ padding: 0, background: "transparent", border: "none" }} />
      </tr>
    </>
  );
};

const ServerDetailBlock: React.FC<{ srv: ApiServer; index: number }> = ({ srv, index }) => {
  return (
    <div className="ssm-server-block">
      <div className="ssm-server-block-header">
        <span className="ssm-server-id">{index} — {srv.ipAddress}</span>
        <div style={{display:"flex",justifyContent:'center', alignItems:"center"}}>
            <span className="ssm-server-id">Server Name: {srv.name}</span>|
            <span className="ssm-server-meta">Site Name: {srv.siteName}</span>
        </div>
        
      </div>

      {/* Mini stat cards */}
      <div className="ssm-srv-wrapper">
        <div className="ssm-srv-cards">
          <div className="ssm-srv-card sc2-purple">
            <div className="ssm-srv-label">Total Camera</div>
            <div className="ssm-srv-value">{srv.totalCameras}</div>
            <div className="ssm-srv-sub">Offline : {srv.offlineCameras}</div>
          </div>
          <div className="ssm-srv-card sc2-orange">
            <div className="ssm-srv-label">Uptime</div>
            <div className="ssm-srv-value">{srv.uptimePercent.toFixed(2)}%</div>
            <div className="ssm-srv-sub">Down: {srv.downTimeMin} min</div>
          </div>
          <div className="ssm-srv-card sc2-blue">
            <div className="ssm-srv-label">CPU Avg.</div>
            <div className="ssm-srv-value">{srv.cpuAvgPercent}%</div>
            <div className="ssm-srv-sub">Max: {srv.cpuMaxUtilizationPercent}</div>
          </div>
          <div className="ssm-srv-card sc2-orange">
            <div className="ssm-srv-label">RAM Avg.</div>
            <div className="ssm-srv-value">{srv.ramAvgPercent}%</div>
            
            <div className="ssm-srv-sub">Max: {srv.ramMaxUtilizationPercent} | Total: {srv.totalRamAvailable}</div>
          </div>
          <div className="ssm-srv-card sc2-purple">
            <div className="ssm-srv-label">DISK Usage</div>
            <div className="ssm-srv-value">
                {srv.diskUtilizationPercent}%
            </div>
            <div>
            <div className="ssm-srv-sub">Free: {formatDiskSpace(srv.freeDiskSpace)}  | Total: {formatDiskSpace(srv.totalDiskSpace)} </div>
             </div>
          </div>
        </div>
      </div>

      {/* Server Offline / Online Events */}   
        <div className="ssm-sub-section">
            <div className="ssm-sub-title">Offline / Online Events</div>
            <table className="ssm-table report-table-sch">
            <thead>
                <tr>
                <th>Sr.</th>
                <th>Offline Date Time</th>
                <th>Online Date Time</th>
                <th>Duration</th>
                </tr>
            </thead>
            <tbody>
                    {srv.serverOfflineOnlineData.length === 0 ? (
                <tr><td colSpan={4} className="empty">No server event recorded</td></tr>
            ) :                 
                srv.serverOfflineOnlineData.map((ev, i) => (
                <tr key={ev.id}>
                    <td>{i + 1}</td>
                    <td>{formatDateToConfiguredTimezone(ev.offlineTime)}</td>
                    <td>{formatDateToConfiguredTimezone(ev.onlineTime)}</td>
                    <td>
                      {ev.onlineTime
                        ? `${diffMinutes(ev.offlineTime, ev.onlineTime)} min`
                        : ""}
                    </td>
                </tr>
                ))
            }
            </tbody>
            </table>
        </div>

      {/* CPU Spike */}
      <div className="ssm-sub-section">
        <div className="ssm-sub-title">CPU Spike</div>
        <table className="ssm-table report-table-sch">
          <thead>
            <tr>
              <th>Server Name</th>
              <th>CPU Threshold Breach Time</th>
              <th>CPU Threshold Restored Time</th>
              <th>Duration</th>
            </tr>
          </thead>
          <tbody>
            {srv.cpuSpikeData.length === 0 ? (
              <tr><td colSpan={4} className="empty">No CPU spikes recorded</td></tr>
            ) : srv.cpuSpikeData.map((row) => (
              <tr key={row.id}>
                <td>{srv.name}</td>
                <td>{formatDateToConfiguredTimezone(row.cpuSpikeStartDatetime)}</td>
                <td>{formatDateToConfiguredTimezone(row.cpuNormalDatetime)}</td>
                <td>
                  {row.cpuNormalDatetime
                    ? `${diffMinutes(
                      row.cpuSpikeStartDatetime,
                      row.cpuNormalDatetime
                    )} min`
                    : ""}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* RAM Spike */}
      <div className="ssm-sub-section">
        <div className="ssm-sub-title">RAM Spike</div>
        <table className="ssm-table report-table-sch">
          <thead>
            <tr>
              <th>Server Name</th>
              <th>RAM Threshold Breach Time</th>
              <th>RAM Threshold Restored Time</th>
              <th>Duration</th>
            </tr>
          </thead>
          <tbody>
            {srv.ramSpikeData.length === 0 ? (
              <tr><td colSpan={4} className="empty">No RAM spikes recorded</td></tr>
            ) : srv.ramSpikeData.map((row) => (
              <tr key={row.id}>
                <td>{srv.name}</td>
                <td>{formatDateToConfiguredTimezone(row.ramSpikeStartDatetime)}</td>
                <td>{formatDateToConfiguredTimezone(row.ramNormalDatetime)}</td>
                <td>
                  {row.ramNormalDatetime
                    ? `${diffMinutes(
                      row.ramSpikeStartDatetime,
                      row.ramNormalDatetime
                    )} min`
                    : ""}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Storage Spike */}
      <div className="ssm-sub-section">
        <div className="ssm-sub-title">Storage Spike</div>
        <table className="ssm-table report-table-sch">
          <thead>
            <tr>
              <th>Server Name</th>
              <th>Drive</th>
              <th>IP Address</th>
              <th>Storage Threshold Breach Time</th>
              <th>RAM Threshold Restored Time</th>
              <th>Duration</th>
            </tr>
          </thead>
          <tbody>
            {srv.diskSpikeData.length === 0 ? (
              <tr><td colSpan={6} className="empty">No Storage spikes recorded</td></tr>
            ) : srv.diskSpikeData.map((row) => (
              <tr key={row.id}>
                <td>{srv.name}</td>
                <td>{row.drive}</td>
                <td>{row.ipAddress}</td>                
                <td>{formatDateToConfiguredTimezone(row.diskSpikeStartDatetime)}</td>
                <td>{formatDateToConfiguredTimezone(row.diskNormalDatetime)}</td>
                <td>
                  {row.diskNormalDatetime
                    ? `${diffMinutes(
                      row.diskSpikeStartDatetime,
                      row.diskNormalDatetime
                    )} min`
                    : ""}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Cameras */}
      {srv.deviceOffline.length > 0 && (
        <div className="ssm-sub-section">
          <table className="ssm-table report-table-sch">
            <thead>
              <tr>
                <th>Camera IP</th>
                <th>Name</th>
                <th>Camera Model</th>
                <th>Online %</th>
                <th>Offline Incidence</th>
                <th>Offline Duration</th>
                <th>Rec%</th>
                <th>No-Recording Incidence</th>
                <th>No-Recording Duration</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              <tr style={{ height: "12px" }}>
                <td colSpan={10} style={{ padding: 0, background: "transparent", border: "none" }} />
              </tr>
              {srv.deviceOffline.map((cam) => (
                <CameraRow key={cam.id} cam={cam} />
              ))}
            </tbody>
          </table>
        </div>
      )}

      
    </div>
  );
};

// ─── LOADING / ERROR SCREENS ──────────────────────────────────────────────────

const LoadingScreen: React.FC = () => (
  <div className="ssm-state-screen">
    <div className="ssm-spinner" />
    <div className="ssm-state-title">Generating Health Report…</div>
    <div className="ssm-state-sub">Fetching server and camera data, please wait.</div>
  </div>
);

const ErrorScreen: React.FC<{ message: string }> = ({ message }) => (
  <div className="ssm-state-screen">
    <div className="ssm-error-icon">⚠️</div>
    <div className="ssm-state-title">Failed to load report</div>
    <div className="ssm-state-sub">{message}</div>
  </div>
);


const ExportHealthReportPDF: React.FC = () => {
  const location = useLocation();


  const routeData = React.useMemo<HealthReportRouteData | null>(() => {
    try {
      const params = new URLSearchParams(location.search);
      const raw    = params.get("data");
      if (!raw) return null;
      return JSON.parse(decodeURIComponent(raw)) as HealthReportRouteData;
    } catch {
      return null;
    }
  }, [location.search]);


  const [servers, setServers] = useState<ApiServer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error,   setError]   = useState<string | null>(null);
  const { timeFormat } = useTimeFormatContext();


  useEffect(() => {    
    fetchReport();
  }, []); 

   const fetchReport = async () => {
    if (!routeData) {
      setError("No report parameters found in the URL.");
      setLoading(false);
      return;
    }   
    try {
        setLoading(true);
        setError(null);

        const payload: HealthReportRequest = {
            ssmSiteIds: routeData.siteList,
            startDateUtc: routeData.startDateUtc,
            endDateUtc: routeData.endDateUtc,
        };

        const response: any = await exportHealthReportService(payload);
              if (response?.isSuccess && response?.data) {
                setServers(response.data);
              } else {
                setServers([]);
              }

       
    } catch (err: any) {
        const msg =
            err?.response?.data?.message ??
            err?.message ??
            "An unexpected error occurred.";
        setError(msg);
    } finally {
        setLoading(false);
    }
    };

useEffect(() => {
  if (!loading && !error && servers.length > 0) {
    const timer = setTimeout(() => {
      handleExportPdf();
    }, 300);
    return () => clearTimeout(timer);
  }
}, [loading, error, servers]);

  const overview = React.useMemo(() => deriveOverview(servers, routeData?.siteNames), [servers,routeData]);

  const periodLabel = routeData
    ? `${formatDate(formatDateToConfiguredTimezone(routeData.startDateUtc),timeFormat)} – ${formatDate(formatDateToConfiguredTimezone(routeData.endDateUtc),timeFormat)}`
    : "—";

  const generatedOn = formatDateToConfiguredTimezone(new Date().toISOString())

 
  const handleExportPdf = async () => {
    const element = document.getElementById("health-report-div");
    if (!element) return;

    const opt = {
      margin: [0.8, 0.2, 0.8, 0.2],
      filename: "SSM_Health_Report.pdf",
      image: { type: "jpeg", quality: 0.98 },
      html2canvas: { scale: 2, useCORS: true },
      jsPDF: { unit: "in", format: "a4", orientation: "portrait" },
      // pagebreak: { mode: ["avoid-all", "css", "legacy"] },
      pagebreak: { mode: ["css"], avoid: ["tr", ".ssm-sub-section", ".ssm-server-block", ".ssm-camera-expand", ".ssm-srv-wrapper"] },
    };

    const worker = html2pdf().set(opt as any).from(element);
    const pdf    = await worker.toPdf().get("pdf");

    const totalPages = pdf.internal.getNumberOfPages();
    const pageWidth  = pdf.internal.pageSize.getWidth();
    const pageHeight = pdf.internal.pageSize.getHeight();

    const viLogo = await convertBase64ToPngBase64("/images/vision_insight_logo_pdf.png");
    const generalSettings = localStorage.getItem("generalSettings");
    let appLogo: string | null = null;
    if (generalSettings) {
      const parsed = JSON.parse(generalSettings);
      if (parsed?.logo) appLogo = await convertBase64ToPngBase64(parsed.logo);
    }

    for (let i = 1; i <= totalPages; i++) {
      pdf.setPage(i);
      if (appLogo) pdf.addImage(appLogo, "PNG", 0.5, 0.4, 1.2, 0.3);
      pdf.addImage(viLogo, "PNG", pageWidth - 1.8, 0.4, 1.2, 0.3);
      pdf.setDrawColor(180);
      pdf.setLineWidth(0.01);
      pdf.line(0.5, 0.7, pageWidth - 0.5, 0.7);
      pdf.setFontSize(9);
      pdf.text("Vision Insight | SSM Server Health Report", 0.5, pageHeight - 0.4);
      pdf.text(`Page ${i} of ${totalPages}`, pageWidth - 0.5, pageHeight - 0.4, { align: "right" });
      pdf.line(0.5, pageHeight - 0.55, pageWidth - 0.5, pageHeight - 0.55);
    }

       pdf.save("SSM_Health_Report.pdf");
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
  };

  // 6. Render
  return (
    <>
      <style>{reportStyles}</style>
      <div className="ssm-wrapper">

        {/* Loading */}
        {loading && <LoadingScreen />}

        {/* Error */}
        {!loading && error && <ErrorScreen message={error} />}

        {/* Report */}
        {!loading && !error && (
          <> 
            <div id="health-report-div">
              <div className="ssm-page">

                {/* Title */}
                <div className="ssm-header">
                  <div className="ssm-report-title">SSM Server &amp; Camera</div>
                  <div className="ssm-report-subtitle">Health Report</div>
                </div>
                <div className="ssm-report-desc">
                  This report provides a comprehensive health analysis of all SSM servers and associated camera systems.
                </div>

                {/* Meta */}
                <table className="ssm-meta-table report-table-sch">
                  <tbody>
                    <tr><td>Report Period</td><td>{periodLabel}</td></tr>
                    <tr><td>Generated On</td><td>{generatedOn}</td></tr>
                    <tr><td>Sites</td><td>{overview.sitesLabel || "—"}</td></tr>
                  </tbody>
                </table>
                <div className="ssm-meta-desc">
                  Covers server availability, CPU and memory utilization trends, incident summaries, and camera online/recording statistics.
                </div>

                {/* Sites Overview */}
                <div className="ssm-section-title">Sites Overview</div>
                <div className="ssm-stat-wrapper">
                  <div className="ssm-stat-cards">
                    <div className="ssm-stat-card sc-blue">
                      <div className="ssm-stat-label">TOTAL SERVER</div>
                      <div className="ssm-stat-value">{overview.totalServer}</div>
                    </div>
                    <div className="ssm-stat-card sc-blue">
                      <div className="ssm-stat-label">TOTAL CAMERA</div>
                      <div className="ssm-stat-value">{overview.totalCamera}</div>
                    </div>
                    <div className="ssm-stat-card sc-green">
                      <div className="ssm-stat-label">AVG SERVER UPTIME</div>
                      <div className="ssm-stat-value">{overview.avgServerUptime}%</div>
                    </div>
                    <div className="ssm-stat-card sc-green">
                      <div className="ssm-stat-label">AVG CPU</div>
                      <div className="ssm-stat-value">{overview.avgCPU}%</div>
                    </div>
                    <div className="ssm-stat-card sc-blue">
                      <div className="ssm-stat-label">AVG RAM</div>
                      <div className="ssm-stat-value">{overview.avgRAM}%</div>
                    </div>
                  </div>
                </div>

                {/* Server Summary */}
                <div className="ssm-section-box">
                  <div className="ssm-section-title">Server Summary</div>
                  <div className="ssm-section-subtitle">High-level health status for all monitored SSM servers</div>
                </div>
                <table className="ssm-table report-table-sch">
                  <thead>
                    <tr>
                      <th>Sr.</th>
                      <th>Server Name</th>
                      <th>Site Name</th>
                      <th>Uptime %</th>
                      <th>Down (min)</th>
                      <th>CPU Avg %</th>
                      <th>RAM Avg %</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {servers.map((srv, i) => (
                      <tr key={srv.id}>
                        <td>{i + 1}</td>
                        <td>{srv.name}</td>
                        <td>{srv.siteName}</td>
                        <td><UptimeCell value={srv.uptimePercent} /></td>
                        <td>{srv.downTimeMin}</td>
                        <td>{srv.cpuAvgPercent}%</td>
                        <td>{srv.ramAvgPercent}%</td>
                        <td><StatusBadge status={srv.status as ServerStatus} /></td>
                      </tr>
                    ))}
                  </tbody>
                </table>

                {/* Per-Server Detailed Analysis */}
                <div className="ssm-section-box" style={{ marginBottom: 20 }}>
                  <div className="ssm-section-title">Per-Server Detailed Analysis</div>
                  <div className="ssm-section-subtitle">CPU, RAM trends, offline events and camera details for each server</div>
                </div>

                {servers.map((srv, i) => (
                  <ServerDetailBlock key={srv.id} srv={srv} index={i + 1} />
                ))}

              </div>
            </div>
          </>
        )}
      </div>
    </>
  );
};

export default ExportHealthReportPDF;


const reportStyles = `

  .ssm-wrapper { background: #f0f2f5; min-height: 100vh; padding: 20px; }

  /* ACTION BAR */
  .ssm-action-bar { display: flex; justify-content: flex-end; gap: 12px; margin-bottom: 16px; }
  .ssm-btn { display: inline-flex; align-items: center; gap: 6px; padding: 10px 22px; border: none; border-radius: 6px; font-size: 13.5px; font-weight: 600; cursor: pointer; letter-spacing: 0.3px; transition: background 0.15s; }
  .ssm-btn-pdf { background: #dc2626; color: #fff; }
  .ssm-btn-pdf:hover { background: #b91c1c; }
  .ssm-btn:disabled { opacity: 0.6; cursor: not-allowed; }

  /* LOADING / ERROR STATE */
  .ssm-state-screen { display: flex; flex-direction: column; align-items: center; justify-content: center; min-height: 70vh; gap: 16px; }
  .ssm-spinner { width: 44px; height: 44px; border: 4px solid #e5e7eb; border-top-color: #e85d27; border-radius: 50%; animation: spin 0.8s linear infinite; }
  @keyframes spin { to { transform: rotate(360deg); } }
  .ssm-state-title { font-size: 16px; font-weight: 600; color: #374151; }
  .ssm-state-sub   { font-size: 13px; color: #6b7280; }
  .ssm-error-icon  { font-size: 40px; }

  /* PAGE */
  .ssm-page { background: #fff; width: 740px; margin: 0 auto 20px; padding: 28px 30px 30px; box-shadow: 0 2px 16px rgba(0,0,0,0.10); position: relative; }

  /* HEADER */
  .ssm-header { text-align: center; margin-bottom: 20px; background-color: #FFECD6; padding: 16px 20px; border-radius: 10px; }
  .ssm-report-title    { font-size: 35px; font-weight: 700; color: #000; line-height: 1.2; }
  .ssm-report-subtitle { font-size: 25px; font-weight: 600; color: #000; margin-top: 2px; }
  .ssm-report-desc     { font-size: 11.5px; color: #000; margin: 12px 0; text-align: center; font-weight: 600; }

  /* META TABLE */
  .ssm-meta-table { width: 100%; border-collapse: collapse; margin-bottom: 10px; }
  .ssm-meta-table td { border: 1px solid #969696; padding: 5px; font-size: 12.5px; }
  .ssm-meta-table td:first-child { background: #FFFAF5; font-weight: 700; color: #000; width: 160px; font-size: 15px; }
  .ssm-meta-table td:last-child  { color: #000; font-size: 15px; }
  .ssm-meta-desc { font-size: 11px; color: #000; margin-bottom: 10px; line-height: 1.5; font-weight: 600; }

  /* SECTION */
  .ssm-section-title    { font-size: 17px; font-weight: 600; color: #1a1a2e; margin-bottom: 2px; }
  .ssm-section-subtitle { font-size: 11.5px; color: #000; margin-bottom: 0px; }
  .ssm-section-box { background: #FFECD6; border-radius: 10px; padding:10px; margin-top: 15px; margin-bottom: 15px; line-height: 26px; }
  .ssm-section-box .ssm-section-title    { color: #000; }
  .ssm-section-box .ssm-section-subtitle { color: #5C554D; }

  /* STAT CARDS */
  .ssm-stat-wrapper { background: #e9e9e9; border-radius: 20px; padding: 20px; }
  .ssm-stat-cards   { display: flex; gap: 6px; }
  .ssm-stat-card { flex: 1; background: #f7f7f7; border-radius: 10px; padding: 14px 14px 12px; position: relative; border: 1px solid #dcdcdc; min-width: 0; }
  .ssm-stat-card::before { content: ""; position: absolute; top: 0; left: 0; height: 6px; width: 100%; border-top-left-radius: 10px; border-top-right-radius: 10px; }
  .sc-blue::before  { background: #2f80ed; }
  .sc-green::before { background: #27ae60; }
  .ssm-stat-label { font-size: 9px; font-weight: 600; color: #7a7a7a; margin-top: 6px; text-transform: uppercase; letter-spacing: 0.4px; line-height: 1.5; }
  .ssm-stat-value { font-size: 20px; font-weight: 700; color: #1a1a2e; margin-top: 4px; }

  /* DATA TABLE */
  .ssm-table { width: 100%; border-collapse: collapse; font-size: 11.5px; }
  .ssm-table th, .ssm-table td { border: 1px solid #969696;  }
  .ssm-table th { background: #DEDEDE; color: #000; padding: 5px 5px; text-align: left; font-weight: 600; font-size: 11px; line-height: 15px; }
  .ssm-table td { padding: 5px 5px; border-bottom: 1px solid #969696; color: #333; line-height: 15px; }
  .ssm-table td.empty { text-align: center; color: #aaa; font-style: italic; }

  /* BADGE */
  .ssm-badge { display: inline-block; padding: 2px 10px; border-radius: 20px; font-size: 10.5px; font-weight: 700; letter-spacing: 0.3px; }
  .badge-warning  { color: #b8860b; }
  .badge-healthy  { color: #1a6b2e; }
  .badge-critical { color: #9b1c1c; }
  .uptime-warn { color: #e67e22; font-weight: 600; }
  .uptime-ok   { color: #27ae60; font-weight: 600; }

  /* SERVER DETAIL BLOCK */
  .ssm-server-block { margin-bottom: 28px; }
  .ssm-server-block-header { display: flex; justify-content: space-between; align-items: center; background-color: #F4F4F4; border: 1px solid #DBDBDB; border-radius: 10px; padding: 0 10px; margin-bottom: 12px; }
  .ssm-server-id   { font-size: 13px; font-weight: 600; color: #000; }
  .ssm-server-meta { font-size: 11.5px; color: #000; font-weight: 600; }

  /* SERVER MINI CARDS */
  .ssm-srv-wrapper { background: #e9e9e9; border-radius: 20px; padding: 10px; margin: 10px 0; }
  .ssm-srv-cards   { display: flex; gap: 6px; }
  .ssm-srv-card { flex: 1; background: #f7f7f7; border-radius: 10px; padding: 10px; position: relative; border: 1px solid #dcdcdc; min-width: 0; line-height: 25px; }
  .ssm-srv-card::before { content: ""; position: absolute; top: 0; left: 0; height: 6px; width: 100%; border-top-left-radius: 10px; border-top-right-radius: 10px; }
  .sc2-blue::before   { background: #1E8BF1; }
  .sc2-orange::before { background: #FF8D00; }
  .sc2-purple::before { background: #9123A7; }
  .ssm-srv-label { font-size: 9px; font-weight: 600; color: #7a7a7a; margin-top: 6px; text-transform: uppercase; letter-spacing: 0.4px; line-height: 1.5; }
  .ssm-srv-value { font-size: 20px; font-weight: 700; color: #1a1a2e; margin-top: 4px; }
  .ssm-srv-sub   { font-size: 9.5px; color: #777; margin-top: 2px; }

  /* SUB SECTION */
  .ssm-sub-section { margin-bottom: 16px; }
  .ssm-sub-title { font-size: 12px; font-weight: 700; color: #4a5568; margin-bottom: 0; text-transform: uppercase; letter-spacing: 0.4px; }

  /* CAMERA INNER TABLE */
  // .ssm-camera-expand { padding: 0 10px 10px; }
  .ssm-inner-table { width: 100%; border-collapse: collapse; font-size: 11px; }
  .ssm-inner-table th { padding:5px; text-align: left; font-weight: 650; font-size: 10.5px; border: none !important; color: #000; background: none !important; }
  .ssm-inner-table td { padding: 0 8px; border-bottom: 1px solid #f0f0f0; color: #626262; border: none !important; }
table.ssm-inner-table th, table.ssm-inner-table td{ padding: 0px;}
   .report-table-sch {
  page-break-inside: auto !important;
}

.report-table-sch tr {
  page-break-inside: avoid !important;
  page-break-after: auto !important;
}
  
  @media print {
    body { background: #fff; }
    .ssm-action-bar { display: none !important; }
    .ssm-wrapper { padding: 0; background: #fff; }
    .ssm-page { box-shadow: none; margin: 0; page-break-after: always; width: 100%; }
  }
`;
