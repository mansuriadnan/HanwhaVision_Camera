import React, { useEffect, useRef, useState } from "react";
import { Box } from "@mui/material";
import * as d3 from "d3";
import { IntervalData } from "../../interfaces/IChart";
import { ChartTicksSelector } from "../../components/Reusable/ChartTicksSelector";
import { SsmServerAvailabilityResponse } from "../../interfaces/IManageServer";
import { getTimeTickFormatter } from "../../utils/formatTimeTick";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";

interface AvailabilityChartProps {
  availabilityData: SsmServerAvailabilityResponse[];
  startDate: Date;
  endDate: Date;
  customizedWidth ?: number;
  customizedHeight: number;
}

const AvailabilityChart = ({
  availabilityData,
  startDate,
  endDate,
  customizedWidth,
  customizedHeight,
}: AvailabilityChartProps) => {
  const svgRef = useRef<SVGSVGElement | null>(null);
  const [selectedInterval, setSelectedInterval] = useState<number>(6);
  const { timeFormat } = useTimeFormatContext();
  const savedLang = localStorage.getItem("i18nextLng") || "en";
  const isRTL = savedLang === "ar";

  const handleTickChanges = (intervalData: IntervalData) => {
    setSelectedInterval(intervalData.tickInterval);
  };


const convertToSegments = () => {
  const now = new Date();
  const isToday =
    startDate.toDateString() === now.toDateString();

  // For today, cap the effective end at current time
  const effectiveEnd = isToday && now < endDate ? now : endDate;

  if (!availabilityData?.length) {
    // No outages → entire range is ON (up to now if today)
    return [{ start: startDate, end: effectiveEnd, status: "ON" as const }];
  }

  const sorted = [...availabilityData].sort(
    (a, b) =>
      new Date(a.offlineTime).getTime() - new Date(b.offlineTime).getTime()
  );

  type Segment = { start: Date; end: Date; status: "ON" | "OFF" };
  const segments: Segment[] = [];
  let cursor = startDate;

  for (const record of sorted) {
    const offAt = new Date(formatDateToConfiguredTimezone(record.offlineTime));
    const onAt = record.onlineTime
      ? new Date(formatDateToConfiguredTimezone(record.onlineTime))
      : effectiveEnd; // ← use effectiveEnd here too

    const segOff =
      offAt < startDate ? startDate : offAt > effectiveEnd ? effectiveEnd : offAt;
    const segOn =
      onAt > effectiveEnd ? effectiveEnd : onAt < startDate ? startDate : onAt;

    if (cursor < segOff) {
      segments.push({ start: cursor, end: segOff, status: "ON" });
    }

    if (segOff < segOn) {
      segments.push({ start: segOff, end: segOn, status: "OFF" });
    }

    cursor = segOn > cursor ? segOn : cursor;
  }

  if (cursor < effectiveEnd) {
    segments.push({ start: cursor, end: effectiveEnd, status: "ON" });
  }

  return segments;
};

  useEffect(() => {
    if (!svgRef.current) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll("*").remove();

    const tooltip = d3.select("body")
    .append("div")
    .attr("id", "availability-tooltip")
    .style("position", "fixed")
    .style("background", "#fbf1e9")
    .style("border", "1px solid #ff6c00")
    .style("border-radius", "19px")
    .style("padding", "0px 15px")
    .style("pointer-events", "none")
    .style("opacity", 0)
    .style("transition", "opacity 0.2s ease")
    .style("z-index", "9999");

    const width = customizedWidth;
    const height = customizedHeight;
    const margin = { top: 20, right: 20, bottom: 30, left: 20 };
    const innerWidth = width - margin.left - margin.right;
    const barHeight = 50;

    const g = svg
      .attr("width", width)
      .attr("height", height)
      .append("g")
      .attr("transform", `translate(${margin.left},${margin.top})`);

    const xScale = d3
      .scaleTime()
      .domain([startDate, endDate])
      .range([0, innerWidth]);

    const segments = convertToSegments();

    g.selectAll("rect")
      .data(segments)
      .enter()
      .append("rect")
      .attr("x", (d) => xScale(d.start))
      .attr("y", 0)
      .attr("width", (d) => Math.max(0, xScale(d.end) - xScale(d.start)))
      .attr("height", barHeight)
      .attr("fill", (d) => (d.status === "ON" ? "#36A769" : "#F62B2B"));
 

    // X-Axis ticks
    let xTicks = xScale.ticks(24);
    if (xTicks.length === 1) {
      xTicks = [startDate, xTicks[0], endDate];
    }

    g.append("g")
      .attr("transform", `translate(0, ${barHeight})`)
      .call(
        d3
          .axisBottom(xScale)
          .tickValues(xTicks)
          .tickFormat((d, i) =>
            getTimeTickFormatter(d, i, xTicks, timeFormat)
        )
      )
      
      .selectAll("text")
      .attr(
        "transform",
        isRTL ? "rotate(-35)" : "rotate(-45)"
      )
      .attr("dx", isRTL ? "-1.5em" : "-0.3em")
      .attr("dy", isRTL ? "1.2em" : "0.6em")
      .style("text-anchor", "end")
      .style("font-size", "10px");

      const fmt = d3.timeFormat("%H:%M");
      g.selectAll("rect")
      .style("cursor", "pointer")
      .on("mouseover", (_, d: any) => {
        tooltip
          .style("opacity", 1)
          .html(
            `<strong>${d.status === "ON" ? "🟢 Online" : "🔴 Offline"}</strong> &nbsp;
            ${fmt(d.start)} – ${fmt(d.end)}`
          );
      })
      .on("mousemove", (event) => {
        tooltip
          .style("left", event.pageX + 10 + "px")
          .style("top", event.pageY - 20 + "px");
      })
      .on("mouseout", () => {
        tooltip.style("opacity", 0);
      });

    return () => {
        tooltip.remove();  // ✅ cleanup on unmount/re-render
      };
   
   
  }, [availabilityData, selectedInterval, startDate, endDate]);

  return (
    <Box>
      <div style={{ display: "none" }}>
        <ChartTicksSelector
          startDate={startDate}
          endDate={endDate}
          parsedDataWithCategory={{ availability: [] }}
          operation="latest"
          chartTicksChange={(data) => handleTickChanges(data)}
          onChartDataChange={() => {}}
        />
      </div>
      <Box className="availability-chart-top-green"><svg ref={svgRef}></svg></Box>
      
    </Box>
  );
};

export default AvailabilityChart;