import React, { useEffect, useRef, useState } from "react";
import * as d3 from "d3";
import { Box } from "@mui/material";
import { ChartTicksSelector } from "../../components";
import { getTimeTickFormatter } from "../../utils/formatTimeTick";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";

interface Props {
  logs: { createdOn: string; totalUsage: number }[];
  startDate: Date;
  endDate: Date;
  color?: string;
}

const MiniLineChart: React.FC<Props> = ({
  logs,
  startDate,
  endDate,
  color = "#2196F3",
}) => {
  const svgRef = useRef<SVGSVGElement | null>(null);

  const [parsedData, setParsedData] = useState<any>({});
  const [finalData, setFinalData] = useState<any[]>([]);
  const [selectedInterval, setSelectedInterval] = useState<number>();
  const { timeFormat } = useTimeFormatContext();
  const savedLang = localStorage.getItem("i18nextLng") || "en";
  const isRTL = savedLang === "ar";
  
  // Convert logs → ChartTicksSelector format
  useEffect(() => {
    if (!logs?.length) return;

    setParsedData({
      usage: logs.map((d) => ({
        date:new Date(formatDateToConfiguredTimezone(d.createdOn)),
        value: +d.totalUsage, // convert to number
      })),
    });
  }, [logs]);

  useEffect(() => {
    if (!svgRef.current || !finalData.length) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll("*").remove();

    const tooltip = d3.select("body")
    .append("div")
    .attr("id", "mini-line-tooltip")
    .style("position", "fixed")
    .style("background", "#fbf1e9")
    .style("border", "1px solid #ff6c00")
    .style("border-radius", "19px")
    .style("padding", "0px 15px")
    .style("pointer-events", "none")
    .style("opacity", 0)
    .style("transition", "opacity 0.2s ease")
    .style("z-index", "9999");

    const width = 450;
    const height = 120;

    const margin = { top: 10, right: 10, bottom: 30, left: 35 };
    const innerWidth = width - margin.left - margin.right;
    const innerHeight = height - margin.top - margin.bottom;

    const chart = svg
      .attr("width", width)
      .attr("height", height)
      .append("g")
      .attr("transform", `translate(${margin.left},${margin.top})`);

    // SCALES
    const xScale = d3
      .scaleTime()
      .domain([startDate, endDate])
      .range([0, innerWidth]);

    const yMax = d3.max(finalData, (d) => d.value) || 100;

    const yScale = d3
      .scaleLinear()
      .domain([0, yMax])
      .nice()
      .range([innerHeight, 0]);

    // X TICKS (same logic as big chart)
    let xTicks = xScale.ticks(24);
    if (xTicks.length === 1) {
      xTicks = [startDate, xTicks[0], endDate];
    }

     

    // AREA
    const area = d3
      .area<any>()
      .x((d) => xScale(d.date))
      .y0(innerHeight)
      .y1((d) => yScale(d.value))
      .curve(d3.curveMonotoneX);

      const fillColor =
          d3.color(color)?.copy({ opacity: 0.2 })?.toString() || color;

    chart
      .append("path")
      .datum(finalData)
      .attr("fill",fillColor)
      .attr("d", area);

    // LINE
    const line = d3
      .line<any>()
      .x((d) => xScale(d.date))
      .y((d) => yScale(d.value))
      .curve(d3.curveMonotoneX);

    chart
      .append("path")
      .datum(finalData)
      .attr("fill", "none")
      .attr("stroke", color)
      .attr("stroke-width", 2)
      .attr("d", line);

    //  X AXIS
        chart
        .append("g")
        .attr("transform", `translate(-15, ${innerHeight + 10})`)
        .call(
            d3
            .axisBottom(xScale)
            .tickValues(xTicks)
            .tickSize(0) //  remove small tick lines
            .tickPadding(10)
            .tickFormat((d, i) =>
                getTimeTickFormatter(d, i, xTicks, timeFormat)
            )
        )
        .call((g) =>
            g.select(".domain") // main axis line
            .attr("stroke", "#E6E6E6") // set line color
        )
        .selectAll("text")
        .style("font-size", "9px")
        .attr("transform", "rotate(-45)");

    //  Y AXIS
        chart
        .append("g")
        .attr("transform", `translate(-10, 0)`) 
        .call(
            d3.axisLeft(yScale)
            .ticks(4)
            .tickSize(0) // removes small tick lines
        )
        .call((g) => g.select(".domain").remove()) // remove vertical axis line
        .selectAll("text")
        .style("font-size", "9px");
      
     // Dots with tooltip
  chart.selectAll("circle.hit")
    .data(finalData)
    .enter()
    .append("circle")
    .attr("class", "hit")
    .attr("cx", (d) => xScale(d.date))
    .attr("cy", (d) => yScale(d.value))
    .attr("r", 3)
    .attr("fill", color)
    .style("cursor", "pointer")
    .on("mouseover", (_, d) => {
      tooltip
        .style("opacity", 1)
        .html(`<strong>Utilization:</strong> ${d.value.toFixed(2)}%`);
    })
    .on("mousemove", (event) => {
      tooltip
        .style("left", event.pageX + 10 + "px")
        .style("top", event.pageY - 20 + "px");
    })
    .on("mouseout", () => {
      tooltip.style("opacity", 0);
    });

  // ✅ Cleanup on unmount/re-render
  return () => {
    tooltip.remove();
  };


  }, [finalData, selectedInterval]);

  return (
    <Box>
      {/* Hidden selector (logic only) */}
      <Box sx={{ display: "none" }}>
        <ChartTicksSelector
          startDate={startDate}
          endDate={endDate}
          parsedDataWithCategory={parsedData}
          operation="latest"
          chartTicksChange={(d) => setSelectedInterval(d.tickInterval)}
          onChartDataChange={(result) => {
            setFinalData(result?.["usage"] || []);
          }}
        />
      </Box>

      <svg ref={svgRef}></svg>
    </Box>
  );
};

export default MiniLineChart;