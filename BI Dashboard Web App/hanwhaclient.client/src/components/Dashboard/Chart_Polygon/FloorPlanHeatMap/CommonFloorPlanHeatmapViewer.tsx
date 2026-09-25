import { Box } from '@mui/material';
import { Document, Page } from 'react-pdf';
import React, {
  RefObject,
  useEffect,
  useState,
  useRef,
  useCallback,
} from 'react';
import { CustomMapWidget } from '../../../index';
import { IFloorPlanData, IHeatmapData, IHeatmapPayload, IWidgetPayload } from '../../../../interfaces/IChart';
import { fetchForkliftCountForMapDataService, fetchHeatmapDatabyDeviceService, fetchPedestrianDetectionForMapDataService, fetchPeopleCountForMapDataService, fetchShoppingCountForMapDataService, fetchSlipandFallDetectionForMapDataService, fetchTrafficJamDetectionForMapDataService, fetchVehicleCountForMapDataService, fetchVehicleQueueManagementForMapDataService, fetchVehicleSpeedDetectionForMapDataService } from '../../../../services/dashboardService';
import { convertToUTC } from '../../../../utils/convertToUTC';
import * as d3 from "d3";
import { ThemeProvider, createTheme } from "@mui/material/styles";
import { getLiveData, onReceiveMessage } from '../../../../utils/signalRService';
import ltrCache from '../../../../utils/ltrCache';
import { CacheProvider } from '@emotion/react';
import staticHeatmap from "../../../../../public/images/static-heatmap.png";



interface FloorPlanViewerProps {
  IsSetUpView: boolean;
  viewMode: 'image' | 'pdf' | undefined;
  imageSrc?: string;
  pdfFile?: string | File;
  containerRef: RefObject<HTMLDivElement | HTMLImageElement>;
  mappedCameras: IFloorPlanData[];
  onDocumentLoadSuccess?: (pdf: any) => void;
  ondeleteWidget?: (
    deviceId: number | string,
    feature: string,
    channelNo?: number
  ) => void;
  floor?: string[];
  zones?: string[];
  selectedStartDate?: string;
  selectedEndDate?: string;
  selectedFloorId?: string;
  setLoadingCount: React.Dispatch<React.SetStateAction<number>>; 
}

interface IHeatmapImage {
  deviceId: string;
  channelNo: number;
  feature: string;
  heatmapImage: string;
}

const CommonFloorPlanHeatmapViewer: React.FC<FloorPlanViewerProps> = ({
  IsSetUpView,
  viewMode,
  imageSrc,
  pdfFile,
  containerRef,
  mappedCameras,
  onDocumentLoadSuccess,
  ondeleteWidget,
  floor,
  zones,
  selectedStartDate,
  selectedEndDate,
  selectedFloorId,
  setLoadingCount
}) => {
  const [mapCameras, setMapCameras] = useState<IFloorPlanData[]>(
    mappedCameras ? mappedCameras : []
  );
  const [imgSize, setImgSize] = useState({ width: 1, height: 1 });
  const pdfWrapperRef = useRef<HTMLDivElement>(null);
  const ruleCountMapRef = useRef<Record<number, Record<number, number>>>({});

  const [zoom, setZoom] = useState(1); // 1 = 100%
  const handleZoomIn = () => setZoom((z) => Math.min(z + 0.1, 2));
  const handleZoomOut = () => setZoom((z) => Math.max(z - 0.1, 0.5));
  const handleReset = () => setZoom(1);
  const [pan, setPan] = useState({ x: 0, y: 0 });            // current pan offset
  const [dragStart, setDragStart] = useState<{ x: number; y: number } | null>(null);
  const [heatMapImg, setHeatMapImg] = useState <IHeatmapImage[]>([]);

const ltrTheme = createTheme({
  direction: "ltr",
});
  const shouldShowFallback =
    !viewMode || (viewMode === 'image' && !imageSrc) || (viewMode === 'pdf' && !pdfFile);

  const handlePDFRender = useCallback(() => {
    if (pdfWrapperRef.current) {
      const { offsetWidth: width, offsetHeight: height } = pdfWrapperRef.current;
      setImgSize({ width, height });
    }
  }, []);
const heatmapCache = useRef<Map<string, string>>(new Map());
  useEffect(() => {
  if (!mappedCameras || mappedCameras.length === 0) {
    setMapCameras([]);
    return;
  }

  setMapCameras(mappedCameras);
  fetchWidgetDataForMap(mappedCameras);

  if (!IsSetUpView) {
    (async () => {
      const heatmapImages = await Promise.all(
        mappedCameras.map(async (cam) => ({
          deviceId: cam.deviceId,
          channelNo: cam.channelNo,
          feature: cam.feature,
          heatmapImage: await generateDeviceHeatmapImage(cam)
        }))
      );

      setHeatMapImg(heatmapImages as IHeatmapImage[]);
    })();
  }
}, [mappedCameras,floor, zones, selectedStartDate, selectedEndDate]);


// useEffect(() => {
//   if (!mappedCameras || mappedCameras.length === 0) {
//     setMapCameras([]);
//     return;
//   }
//   setMapCameras(mappedCameras);
//   fetchWidgetDataForMap(mappedCameras);
// }, [mappedCameras,selectedStartDate, selectedEndDate, floor, zones]);

// const heatmapFetchedRef = useRef(false);

// useEffect(() => {
//   if (IsSetUpView) return;
//   if (!mappedCameras || mappedCameras.length === 0) return;
//   if (heatmapFetchedRef.current) return;

//   heatmapFetchedRef.current = true;

//   (async () => {
//     const heatmapImages = await Promise.all(
//       mappedCameras.map(async (cam) => ({
//         deviceId: cam.deviceId,
//         heatmapImage: await generateDeviceHeatmapImage(cam),
//       }))
//     );

//     setHeatMapImg(heatmapImages as IHeatmapImage[]);
//   })();
// }, [mappedCameras, IsSetUpView,selectedStartDate, selectedEndDate, floor, zones]);




   useEffect(() => {
    if (viewMode === 'pdf') {
      const resizeObserver = new ResizeObserver(() => {
        handlePDFRender();
      });
      if (pdfWrapperRef.current) {
        resizeObserver.observe(pdfWrapperRef.current);
      }
      return () => resizeObserver.disconnect();
    }
  }, [viewMode, handlePDFRender]);
  

 const fetchHeatmapData = async (device: IFloorPlanData) => {
  if (!device) return;

  try {
    let requestHeatmapData: IHeatmapPayload = {
      deviceId: device.deviceId,
      channelNo: Number(device.channelNo), // IMPORTANT
      heatmapType:
        device.feature === "VehicleCount"
          ? "VehicleHeatMap"
          : "PeopleHeatMap",
      startDate: convertToUTC(selectedStartDate ?? ""),
      endDate: convertToUTC(selectedEndDate ?? ""),
    };

    const response = await fetchHeatmapDatabyDeviceService(requestHeatmapData);
    return response.data;

  } catch (error) {
    console.error("Error fetching Heatmap data:", error);
  }
};


  async function generateDeviceHeatmapImage(device : IFloorPlanData) {
    // const key = `${device.deviceId}-${device.channelNo}`;
      const key = `${device.deviceId}-${device.channelNo}-${device.feature}-${selectedStartDate}-${selectedEndDate}`;

  if (heatmapCache.current.has(key)) {
    return heatmapCache.current.get(key)!;
  }
    const value = await fetchHeatmapData(device);

    if (!value?.heatMapData || !value?.resolutionWidth || !value?.resolutionHeight) {
      console.warn("No heatmap data found for", key);
      return "";
    }

    const data: number[] = value.heatMapData; // flattened row-major array
    const W = Number(value.resolutionWidth);
    const H = Number(value.resolutionHeight);

    if (W <= 0 || H <= 0 || data.length === 0) {
      console.warn("Invalid heatmap dimensions or empty data", { W, H, len: data.length });
      return "";
    }

    // create a tiny canvas exactly the heatmap resolution
    const tiny = document.createElement("canvas");
    tiny.width = W;
    tiny.height = H;
    const tctx = tiny.getContext("2d");
    if (!tctx) return "";

    // Create an ImageData buffer
    const imageData = tctx.createImageData(W, H);
    // compute max for scale (avoid zero)
    const maxVal = Math.max(...data, 1);
    // logScale - safe: domain must be > 0, we use val+1 for safety
    const logScale = d3.scaleLog().domain([1, maxVal + 1]).range([0, 1]);
    const colorScale = d3.scaleSequential(d3.interpolateTurbo).domain([0, 1]);

    // Fill pixels (row-major: y then x)
    for (let y = 0; y < H; y++) {
      for (let x = 0; x < W; x++) {
        const i = y * W + x;
        const val = data[i] ?? 0;
        const ratio = logScale(val + 1); // map -> 0..1
        const colorStr = colorScale(ratio);
        const color = d3.color(colorStr)?.rgb();

        const px = (y * W + x) * 4;
        if (color) {
          imageData.data[px] = color.r;
          imageData.data[px + 1] = color.g;
          imageData.data[px + 2] = color.b;
          imageData.data[px + 3] = Math.round(160 * (val > 0 ? 1 : 0)); // alpha: 160 if value>0, else 0
        } else {
          imageData.data[px] = 0;
          imageData.data[px + 1] = 0;
          imageData.data[px + 2] = 0;
          imageData.data[px + 3] = 0;
        }
      }
    }

    tctx.putImageData(imageData, 0, 0);

    // OPTIONAL: produce a smoothed, fixed-size image to overlay (so you don't have to scale raw tiny canvas in CSS)
    // change DISPLAY_SIZE to how big you want the heatmap overlay (in px)
    const DISPLAY_SIZE = 200;
    const out = document.createElement("canvas");
    out.width = DISPLAY_SIZE;
    out.height = DISPLAY_SIZE;
    const outCtx = out.getContext("2d");
    if (!outCtx) return tiny.toDataURL("image/png");

    // smooth the small resolution to display size
    outCtx.imageSmoothingEnabled = true;
    outCtx.imageSmoothingQuality = "high";

    // draw tiny onto out (this will upscale and smooth)
    outCtx.clearRect(0, 0, DISPLAY_SIZE, DISPLAY_SIZE);
    outCtx.drawImage(tiny, 0, 0, DISPLAY_SIZE, DISPLAY_SIZE);

    // return the scaled image
    // return out.toDataURL("image/png");
     const img = out.toDataURL("image/png");

  heatmapCache.current.set(key, img);
  return img;
  }


  const onMouseDown = (e: React.MouseEvent) => {
    setDragStart({ x: e.clientX - pan.x, y: e.clientY - pan.y });
  };

  const onMouseMove = (e: React.MouseEvent) => {
    if (!dragStart) return;
    setPan({
      x: e.clientX - dragStart.x,
      y: e.clientY - dragStart.y,
    });
  };

  const onMouseUp = () => setDragStart(null); 


  const fetchWidgetDataForMap = async (mappedCameraList: IFloorPlanData[]) => {
    if (floor === undefined || floor === null) return;
      // setLoadingCount((c) => c + mappedCameraList.length);
    mappedCameraList?.map(async (item) => {
      try {
        const data = {
          floorIds: floor,
          zoneIds: zones,
          startDate: convertToUTC(selectedStartDate || ""),
          endDate: convertToUTC(selectedEndDate || ""),
          deviceId: item.deviceId,
          channel: item.channelNo,
        };

        let result = null;

        if (item.feature === "PeopleCount") {
          const res: any = await fetchPeopleCountForMapDataService(
            data as IWidgetPayload
          );
          result = res?.data;
        }  else if (item.feature === "VehicleCount") {
          const res: any = await fetchVehicleCountForMapDataService(
            data as IWidgetPayload
          );
          result = res?.data;
        } 
        
        if (result !== null && result !== undefined) {
          setMapCameras((prev) =>
            prev.map((cam) =>
              cam.deviceId === item.deviceId &&
                cam.feature === item.feature &&
                ((cam.channelNo ?? 0) === (item.channelNo ?? 0))
                ? { ...cam, widgetData: result }
                : cam
            )
          );
        }

      } catch (error) {
        console.error("Error fetching floorplan device data:", error);
        throw error;
      }
      // finally {
      //   // Decrement loader for each completed request
      //   setLoadingCount((c) => c - 1);
      // }
    });
  };

  const renderBasePlan = () => {
    if (viewMode === 'image' && imageSrc) {
      return (
        // <img
        //   src={imageSrc}
        //   alt="Floor plan"
        //   onLoad={(e) => {
        //     const width = e.currentTarget.offsetWidth;
        //     const height = e.currentTarget.offsetHeight;
        //     setImgSize({ width, height });
        //   }}
        //   style={{ display: 'block', maxWidth: '100%', height: 'auto' }}
        // />
        <Box
        // sx={{
        //   transform: `scale(${zoom})`,
        //   transformOrigin: 'top left',
        //   display: 'inline-block'
        // }}
        >
          <img
            src={imageSrc}
            alt="Floor plan"
            onLoad={(e) => {
              const width = e.currentTarget.naturalWidth; // use natural size
              const height = e.currentTarget.naturalHeight;
              //  const width ="auto"; // use natural size
              // const height ="auto"
              setImgSize({ width, height });
            }}
            style={{ display: 'block', maxWidth: '100%', height: 'auto' }}
          />
        </Box>

      );
    }

    if (viewMode === 'pdf' && pdfFile) {
      return (
        <Document
          file={pdfFile}
          onLoadSuccess={(pdf) => {
            onDocumentLoadSuccess?.(pdf);
            setTimeout(handlePDFRender, 100); // ensure page is rendered before measuring
          }}
          onLoadError={(error) => console.error('PDF Load Error:', error)}
        >
          <Page
            pageNumber={1}
            scale={1}
            // scale={zoom}
            renderTextLayer={false}
            renderAnnotationLayer={false}
          />
        </Document>
      );
    }

    return null;
  };

  return (
    <CacheProvider value={ltrCache}>
      <ThemeProvider theme={ltrTheme}>
        <div style={{ direction: 'ltr' }}>
          {/*  Fixed controls */}
          {!IsSetUpView && (
            <Box
              className="zoom-in-out-main"
            >
              <button onClick={handleZoomOut}>–</button>
              <span>{Math.round(zoom * 100)}%</span>
              <button onClick={handleZoomIn}>+</button>
              <button onClick={handleReset}>  <img src="/images/dashboard/reset_floorplan.png" alt="reset icon" /></button>
            </Box>
          )}


          <Box
            className="droppable-map-plans"
          // sx={{
          //   overflow: 'visible',
          //   display: 'flex',
          //   justifyContent: 'center',
          //   alignItems: 'center',
          //   position: 'relative',
          // }}
          >
            {shouldShowFallback && (
              <Box
                sx={{
                  height: '100%',
                  width: '100%',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: '#999',
                  fontSize: 14,
                  textAlign: 'center',
                  padding: 2,
                  alignSelf: 'center'
                }}
              >
                Please select a floor in setup. No floor data available.
              </Box>
            )}


            {!shouldShowFallback && (
              <div
                ref={viewMode === 'image' ? (containerRef as RefObject<HTMLDivElement>) : pdfWrapperRef}
                style={{ position: 'relative', display: 'inline-block', direction: 'ltr' }}
                className='floor-plan-wrapper'
              >
                <Box
                  sx={{
                    position: 'relative',
                    width: imgSize.width,
                    height: imgSize.height,
                    // width:'100%',
                    // height:'100%',
                    transform: `translate(${pan.x}px, ${pan.y}px) scale(${zoom})`,
                    transformOrigin: 'top left',
                    cursor: dragStart ? 'grabbing' : 'grab',
                  }}
                  {...(!IsSetUpView && {
                    onMouseDown,
                    onMouseMove,
                    onMouseUp,
                    onMouseLeave: onMouseUp,
                  })}
                >
                  {renderBasePlan()}                 

                  {/* render Heatmap */}
                 {(IsSetUpView ? mapCameras : heatMapImg)?.map((h) => {
                   const device = mapCameras.find(
                     (d) =>
                       d.deviceId === h.deviceId &&
                       d.channelNo === h.channelNo &&
                       d.feature === h.feature
                   );
                      if (!device || !device.position) return null;

                      const x = device.position.x;
                      const y = device.position.y;

                      const heatSize = 200;
                      const half = heatSize / 2;


                      const xPercent = ((x - half) / imgSize.width) * 100;
                      const yPercent = ((y - half) / imgSize.height) * 100;
                    
                       const heatmapSrc = (h as IHeatmapImage).heatmapImage;

                      // for no heatmap data
                      if (!IsSetUpView && (!heatmapSrc || heatmapSrc.trim() === "")) {
                        return (
                          <div
                            key={`${h.deviceId}_${h.channelNo}_${h.feature}`}
                            style={{
                              position: "absolute",
                              left: `${xPercent}%`,
                              top: `${yPercent}%`,
                              width: `${heatSize}px`,
                              height: `${heatSize}px`,
                              pointerEvents: "none",
                              transformOrigin: "center center",
                              zIndex: 2000,
                              mixBlendMode: "multiply",
                              opacity: 0.8
                            }}
                          >
                            No heatmap data available.
                          </div>
                        );
                      }

                      return (
                        <img
                         key={`${h.deviceId}_${h.channelNo}_${h.feature}`}
                          // src={h.heatmapImage}
                          src={
                            IsSetUpView
                              ? staticHeatmap   
                              : heatmapSrc            
                          }
                          alt="heatmap"
                          style={{
                            position: "absolute",
                            left: `${xPercent}%`,
                            top: `${yPercent}%`,
                            width:`${heatSize}px`,
                            height: `${heatSize}px`,
                            pointerEvents: "none",
                            transform: `scale(${zoom})`,
                            transformOrigin: "center center",
                            zIndex: 2000,
                            mixBlendMode: "multiply",
                            opacity: 0.7
                          }}
                        />
                      );
                    })}

                  {mapCameras.map((device, index) => {
                    // const x = device.position?.x ?? 0;
                    // const y = device.position?.y ?? 0;
                    const x = (device.position?.x ?? 0);
                    const y = (device.position?.y ?? 0);
                    const angle = device.position?.angle ?? 0;

                    const xPercent = (x / imgSize.width) * 100;
                    const yPercent = (y / imgSize.height) * 100;


                    return (
                      <Box
                        key={`${device.deviceId}-${device.channelNo}-${device.feature}`}

                        // key={`${device.deviceId}-${device.feature}-${index}-${Math.random()}`}
                        sx={{
                          position: 'absolute',
                          left: `${xPercent}%`,
                          top: `${yPercent}%`,
                          transform: `scale(${1 / zoom})`,
                          transformOrigin: 'top left',
                          zIndex: 9999,
                          direction: 'ltr',
                        }}
                      >                        
                        <CustomMapWidget
                          IsSetUpView={IsSetUpView}
                          // deleteWidget={() => {
                          //   ondeleteWidget?.(device.deviceId, device.feature);
                          // }}
                          deleteWidget={() => {
                            ondeleteWidget?.(
                              device.deviceId,
                              device.feature,
                              device.channelNo
                            );
                          }}

                          cam={device}
                        />
                      </Box>
                    );
                  })}

                </Box>

              </div>
            )}

          </Box>
        </div>
      </ThemeProvider>
    </CacheProvider>
  );
};

export { CommonFloorPlanHeatmapViewer };

