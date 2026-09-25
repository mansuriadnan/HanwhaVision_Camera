import React, { useEffect, useRef, useState } from "react";
import * as d3 from "d3";
import {
  cameraDisconnectionTrackerProps,
  ICDTData,
  ICDTDataWithIP,
  IDisconnectChartPoint,
  IntervalData,
  IParsedDataWithCategory,
  ParsedDataFormat,
} from "../../../../interfaces/IChart";
import { ChartTicksSelector } from "../../../index";
import { Box } from "@mui/material";
import { formatDateToConfiguredTimezone } from "../../../../utils/formatDateToConfiguredTimezone";
import { formatNumber } from "../../../../utils/formatNumber";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import apiUrls from "../../../../constants/apiUrls";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";
import { useThemeContext } from "../../../../context/ThemeContext";
import { useTimeFormatContext } from "../../../../context/TimeFormatContext";
import { getTimeTickFormatter } from "../../../../utils/formatTimeTick";

const CameraDisconnectionTracker2_1_Option2: React.FC<
  cameraDisconnectionTrackerProps
> = ({
  customizedWidth,
  cameraDisconnectionData,
  customizedHeight,
  startDate,
  endDate,
  floor,
  zones,
  setExportHandler,
}) => {
  const svgRef = useRef<SVGSVGElement | null>(null);
  const [selectedInterval, setSelectedInterval] = useState<number>();
  const [finalData, setFinalData] = useState<ParsedDataFormat[]>([]);
  const [parsedDataWithCategory, setParsedDataWithCategory] =
    useState<IParsedDataWithCategory>({});
  const [selectedIntervalName, setSelectedIntervalName] = useState<string>("");
  const selectedIntervalNameRef = useRef<string>("");
  const [CDTData, setCDTData] = useState<ICDTDataWithIP[] | null>(
    cameraDisconnectionData ?? []
  );

  const handleTickChanges = (intervalData: IntervalData) => {
    setSelectedInterval(intervalData.tickInterval);
    setSelectedIntervalName(intervalData.intervalName);
  };

  useExportHandler({
    apiEndpoint: `${apiUrls.CameraDisconnectedTrackerAnalysis}/csv`,
    startDate: convertDateToISOLikeString(startDate as Date),
    endDate: convertDateToISOLikeString(endDate as Date),
    floor,
    zones,
    selectedIntervalNameRef,
    setExportHandler,
  });


  useEffect(() => {
    selectedIntervalNameRef.current = selectedIntervalName;
  }, [selectedIntervalName]);
  const { theme } = useThemeContext();
  const { timeFormat } = useTimeFormatContext();

  useEffect(() => {
    if (!cameraDisconnectionData) return;

    const updatedParsedData: IParsedDataWithCategory = {};

    // updatedParsedData["CDTData"] = finalData.map((d) => ({
    //   date: new Date(formatDateToConfiguredTimezone(d.date as string)),
    //   value: d.value,
    // }));
    const updatedDisconnectedData = cameraDisconnectionData.map((m) => ({
      ...m,
      offlineTime: formatDateToConfiguredTimezone(m.offlineTime) as string,
      onlineTime: formatDateToConfiguredTimezone(m.onlineTime) as string,
    }));

    updatedParsedData["CDTData"] = cameraDisconnectionData.map((d) => ({
      date: new Date(d.offlineTime as string),
      value: 1,
    }));
    setCDTData(updatedDisconnectedData);
    setParsedDataWithCategory(updatedParsedData);
  }, [cameraDisconnectionData, selectedInterval]);

  useEffect(() => {
    if (!svgRef.current || !finalData) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll("*").remove();

    const width = customizedWidth as number;
    const height = customizedHeight as number;
    const margin = { top: 30, right: 30, bottom: 100, left: 30 };
    const innerWidth = width - margin.left - margin.right;
    const innerHeight = height - margin.top - margin.bottom;
    const maxBarWidth = 24;

    const chartGroup = svg
      .attr("width", width)
      .attr("height", height)
      .append("g")
      .attr("transform", `translate(${margin.left},${margin.top})`);

    var xExtent = [startDate, endDate] as [Date, Date];

    const yMax = Math.max(d3.max(finalData, (d) => d.value) ?? 0, 10);

    let barWidth = (innerWidth - maxBarWidth) / finalData.length;
    if (barWidth > maxBarWidth) barWidth = maxBarWidth;

    var xScale = d3
      .scaleTime()
      .domain(xExtent)
      .range([maxBarWidth / 2, innerWidth]);

    let xTicks = xScale.ticks(selectedInterval);
    if (xTicks.length === 1) {
      xTicks = [xExtent[0], xTicks[0], xExtent[1]];
    }

    const yScale = d3
      .scaleLinear()
      .domain([0, yMax])
      .nice()
      .range([innerHeight, 0]);

    const yTickSize = 5;

    const yTicks = yScale.ticks(yTickSize);

    chartGroup
      .append("g")
      .attr("class", "y-grid-lines")
      .selectAll("line")
      .data(yTicks)
      .enter()
      .append("line")
      .attr("x1", 0)
      .attr("x2", innerWidth)
      .attr("y1", (d) => yScale(d))
      .attr("y2", (d) => yScale(d))
      .attr("stroke", "#E0E0E0")
      .attr("opacity", theme === "light" ? "100%" : "20%")
      .attr("stroke-width", 1);

    // Bars
    svg
      .select("g")
      .selectAll(".bar")
      .data(finalData)
      .enter()
      .append("rect")
      .attr("class", "bar")
      .attr("x", (d) => xScale(d.date)! - barWidth / 2)
      // .attr("x", (d) => xScale(d.date)!)
      .attr("y", (d) => yScale(d.value))
      .attr("width", barWidth - 2)
      .attr("height", (d) => innerHeight - yScale(d.value))
      .attr("fill", "#2081FF")
      // .on("mouseover", (_, d) => {
      //   d3.select("#tooltip")
      //     .style("opacity", 1)
      //     .html(
      //       `<strong>Disconnected Camera</strong> : ${formatNumber(d.value)}`
      //     );
      // })
       .on("mouseover", (event, d) => {
                const deviceCountMap = getDeviceEventCountsForBucket(d.date);
      
                const rows = Array.from(deviceCountMap.entries())
                  .map(
                    ([IPAddress, count]) => `
              <div style="display:flex;justify-content:space-between;gap:10px">
                <strong>${IPAddress}</strong> :
                <span>${count}</span>
              </div>
            `
                  )
                  .join("");
      
                d3.select("#tooltip")
                  .style("opacity", 1)
                  .html(`
            <div>
              <div style="margin-bottom:4px">
               <strong> Disconnect </strong> : ${d.value}
              </div>
              <hr style="margin:4px 0"/>
              ${rows}
            </div>
          `);
              })      
      .on("mousemove", (event) => {
        d3.select("#tooltip")
          .style("left", event.pageX + 10 + "px")
          .style("top", event.pageY - 20 + "px");
      })
      .on("mouseout", () => {
        d3.select("#tooltip").style("opacity", 0);
      });

    // X Axis
    chartGroup
      .append("g")
      .attr("transform", `translate(0, ${innerHeight})`)
      .call(
        d3
          .axisBottom(xScale)
          .tickValues(xTicks)
          .tickSize(4)
          .tickFormat((d, i) => getTimeTickFormatter(d, i, xTicks, timeFormat))
      )
      .call((g) => {
        g.select(".domain").attr("stroke", "#70757a");
        g.selectAll(".tick line").attr(
          "stroke",
          theme === "light" ? "#212121" : "#FFFFFF"
        );
      })
      .selectAll("text")
      .style("fill", theme === "light" ? "#212121" : "#FFFFFF")
      .style("font-size", "10px")
      .attr("transform", "rotate(-45)")
      .style("text-anchor", "end");

    // Y axis
    chartGroup
      .append("g")
      .call(
        d3
          .axisLeft(yScale)
          .ticks(yTickSize)
          .tickSize(0)
          .tickFormat(d3.format("~s"))
      )
      .call((g) => g.select(".domain").attr("stroke", "none"))
      .selectAll("text")
      .style("fill", theme === "light" ? "#212121" : "#D4D4D4")
      .style("font-size", "10px");
  }, [customizedWidth, customizedHeight, finalData, theme, timeFormat]);

  const diffInMinutes = (from: Date, to: Date): number => {
    return Math.floor((to.getTime() - from.getTime()) / (1000 * 60));
  };

  const generateFinalData = (result: IParsedDataWithCategory) => {
    if (!CDTData) return;

    var resultData = result.CDTData;
    for (let i = 0; i < resultData.length; i++) {
      const element = resultData[i];
      const bucketStart = new Date(element.date);
      const bucketEnd = new Date(bucketStart);
      bucketEnd.setMinutes(
        bucketEnd.getMinutes() +
          diffInMinutes(resultData[0].date, resultData[1].date)
      );

      let count = 0;

      for (let j = 0; j < CDTData.length; j++) {
        const item = CDTData[j];

        const startDate = new Date(item.offlineTime);
        const endDate = item.onlineTime ? new Date(item.onlineTime) : null;

        const effectiveEnd = endDate ?? new Date(); // still in progress

        if (bucketStart < effectiveEnd && bucketEnd > startDate) {
          count++;
        }
      }

      element.value = count;
    }

    setFinalData(resultData);
  };

    const getDeviceEventCountsForBucket = (bucketDate: Date) => {
    if (!CDTData || finalData.length < 2) return new Map<string, number>();

    const bucketStart = new Date(bucketDate);
    const bucketEnd = new Date(bucketStart);

    const bucketDurationMinutes = diffInMinutes(
      finalData[0].date,
      finalData[1].date
    );

    bucketEnd.setMinutes(bucketEnd.getMinutes() + bucketDurationMinutes);

    const deviceCountMap = new Map<string, number>();

    for (const item of CDTData) {
      const offline = new Date(item.offlineTime);
      const online = item.onlineTime ? new Date(item.onlineTime) : new Date();

      // overlap check
      if (bucketStart < online && bucketEnd > offline) {
        deviceCountMap.set(
          item.ipAddress,
          (deviceCountMap.get(item.ipAddress) ?? 0) + 1
        );
      }
    }

    return deviceCountMap;
  };

  return (
    <Box
      sx={{
        width: customizedWidth,
        height: customizedHeight,
        display: "flex",
        flexDirection: "column",
        padding: "10px 0px",
      }}
    >
      {parsedDataWithCategory && (
        <ChartTicksSelector
          startDate={startDate as Date}
          endDate={endDate as Date}
          parsedDataWithCategory={parsedDataWithCategory}
          operation="max"
          chartTicksChange={(data) => handleTickChanges(data)}
          onChartDataChange={(result) => {
            setFinalData(result?.["CDTData"] as ParsedDataFormat[]);
            generateFinalData(result);
          }}
        />
      )}
      <svg ref={svgRef}></svg>
    </Box>
  );
};

export { CameraDisconnectionTracker2_1_Option2 };
