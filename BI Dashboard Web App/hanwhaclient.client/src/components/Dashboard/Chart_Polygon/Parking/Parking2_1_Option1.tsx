import React, { useEffect, useRef, useState } from "react";
import {
  ParkingProps,
  IParsedDataWithCategory,
  IntervalData,
} from "../../../../interfaces/IChart";
import { Box, Typography } from "@mui/material";
import * as d3 from "d3";
import { ChartTicksSelector } from "../../../index";
import { formatDateToConfiguredTimezone } from "../../../../utils/formatDateToConfiguredTimezone";
import { formatNumber } from "../../../../utils/formatNumber";
import apiUrls from "../../../../constants/apiUrls";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";
import { useThemeContext } from "../../../../context/ThemeContext";
import { useTimeFormatContext } from "../../../../context/TimeFormatContext";
import { getTimeTickFormatter } from "../../../../utils/formatTimeTick";

const Parking2_1_Option1: React.FC<ParkingProps> = ({
  parkingChartData,
  customizedWidth,
  customizedHeight,
  startDate,
  endDate,
  floor,
  zones,
  setExportHandler,
  totalCapacity,
}) => {
  const svgRef = useRef<SVGSVGElement | null>(null);
  const [selectedInterval, setSelectedInterval] = useState<number>();
  const [finalData, setFinalData] = useState<IParsedDataWithCategory>({});
  const [parsedDataWithCategory, setParsedDataWithCategory] =
    useState<IParsedDataWithCategory>({});
  const [selectedIntervalName, setSelectedIntervalName] = useState<string>("");
  const selectedIntervalNameRef = useRef<string>("");
  const { theme } = useThemeContext();
  const { timeFormat } = useTimeFormatContext();

  type VisitorKey = "occupied" | "available";
  const categories: {
    key: VisitorKey;
    label: string;
    color: string;
  }[] = [
    {
      key: "occupied",
      label: "Occupied",
      color: "#939393",
    },
    {
      key: "available",
      label: "Available",
      color: "#A0CF9C",
    },
  ];

  const handleTickChanges = (intervalData: IntervalData) => {
    setSelectedInterval(intervalData.tickInterval);
    setSelectedIntervalName(intervalData.intervalName);
  };

  useExportHandler({
    apiEndpoint: `${apiUrls.VehicleParkingAnalysis}/csv`,
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

  useEffect(() => {
    if (!parkingChartData || !categories) return;
    const updatedParsedData: IParsedDataWithCategory = {};
    categories.forEach((cat) => {
      updatedParsedData[cat.key] = parkingChartData.map((d) => ({
        date: new Date(formatDateToConfiguredTimezone(d.dateTime) as string),
        value: d[cat.key] ?? 0,
      }));
    });

    setParsedDataWithCategory(updatedParsedData);
  }, [parkingChartData, selectedInterval]);

  useEffect(() => {
    if (!svgRef.current || !finalData) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll("*").remove();

    const width = customizedWidth as number;
    const height = customizedHeight as number;
    const margin = { top: 30, right: 30, bottom: 112, left: 30 };
    const innerWidth = width - margin.left - margin.right;
    const innerHeight = height - margin.top - margin.bottom;
    const maxBarWidth = 24;

    const MultiBarchartGroup = svg
      .attr("width", width)
      .attr("height", height)
      .append("g")
      .attr("transform", `translate(${margin.left},${margin.top})`);

    const xExtent = [startDate, endDate] as [Date, Date];

    const allValues = Object.values(finalData)
      .flat()
      .map((d) => d.value);

    const yMax = Math.max(d3.max(allValues) ?? 0, 10);

    const xScale = d3
      .scaleTime()
      .domain(xExtent as [Date, Date])
      .range([maxBarWidth, innerWidth]);

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

    MultiBarchartGroup.append("g")
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

    categories.forEach((cat, catIndex) => {
      const categoryWisedata = finalData[cat.key];
      if (!categoryWisedata || categoryWisedata.length === 0) return;

      const groupCount = categories.length;
      const groupSpacing = 0.8; // between 0 and 1
      const tickSpacing = innerWidth / categoryWisedata.length;
      const groupWidth = tickSpacing * groupSpacing;
      var barWidth = groupWidth / groupCount;
      if (barWidth > maxBarWidth) barWidth = maxBarWidth;
      const groupOffset = (groupCount * barWidth) / 2;

      MultiBarchartGroup.append("g")
        .selectAll(`.bar-${cat.key}`)
        .data(categoryWisedata)
        .enter()
        .append("rect")
        .attr("class", `bar-${cat.key}`)
        .attr(
          "x",
          (d) => xScale(d.date)! - groupOffset + catIndex * barWidth,
          // (d) => xScale(d.date)! - groupWidth / 2 + catIndex * barWidth
        )
        .attr("y", (d) => yScale(d.value))
        .attr("width", barWidth)
        .attr("height", (d) => innerHeight - yScale(d.value))
        .attr("fill", cat.color)
        .on("mouseover", (_, d) => {
          d3.select("#tooltip")
            .style("opacity", 1)
            .html(`<strong>${cat.label}</strong> :  ${formatNumber(d.value)}`);
        })
        .on("mousemove", (event) => {
          d3.select("#tooltip")
            .style("left", event.pageX + 10 + "px")
            .style("top", event.pageY - 20 + "px");
        })
        .on("mouseout", () => {
          d3.select("#tooltip").style("opacity", 0);
        });
    });

    // X Axis
    MultiBarchartGroup.append("g")
      .attr("transform", `translate(0, ${innerHeight})`)
      .call(
        d3
          .axisBottom(xScale)
          .tickValues(xTicks)
          .tickSize(4)
          .tickFormat((d, i) => getTimeTickFormatter(d, i, xTicks, timeFormat)),
      )
      // .call(d3.axisBottom(xScale).ticks(selectedInterval).tickSize(4))
      .call((g) => {
        g.select(".domain").attr("stroke", "#70757a");
        g.selectAll(".tick line").attr(
          "stroke",
          theme === "light" ? "#212121" : "#FFFFFF",
        );
      })
      .selectAll("text")
      .style("fill", theme === "light" ? "#212121" : "#FFFFFF")
      .style("font-size", "10px")
      .attr("transform", "rotate(-45)")
      .style("text-anchor", "end");

    // Y axis
    MultiBarchartGroup.append("g")
      .call(
        d3
          .axisLeft(yScale)
          .ticks(yTickSize)
          .tickSize(0)
          .tickFormat(d3.format("~s")),
      )
      .call((g) => g.select(".domain").attr("stroke", "none"))
      .selectAll("text")
      .style("fill", theme === "light" ? "#212121" : "#D4D4D4")
      .style("font-size", "10px");
  }, [customizedWidth, customizedHeight, finalData, theme, timeFormat]);

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
            // setFinalData(result);
            if (!result) return;
            const capacity = totalCapacity ?? 0;
            const occupied = result?.occupied ?? [];
            const updatedAvailable = occupied.map((item: any) => ({
              date: item.date,
              value: capacity - item.value,
            }));

            const updatedResult = {
              ...result,
              available: updatedAvailable,
            };

            setFinalData(updatedResult);
          }}
        />
      )}
      <svg ref={svgRef}></svg>
      <Box
        sx={{
          display: "flex",
          flexDirection: "row",
          gap: 6,
          justifyContent: "center",
          alignItems: "center",
        }}
      >
        {categories.map((item, idx) => {
          return (
            <Box
              key={idx}
              sx={{
                display: "flex",
                flexDirection: "column",
                alignItems: "center",
                gap: "2px",
              }}
            >
              <Box
                sx={{
                  display: "flex",
                  alignItems: "center",
                  gap: "6px",
                }}
              >
                <Box
                  sx={{
                    width: 10,
                    height: 10,
                    borderRadius: "50%",
                    backgroundColor: item.color,
                    display: "inline-block",
                  }}
                />
                <Typography
                  style={{
                    fontSize: "12px",
                    color: "#626262",
                    fontWeight: 700,
                  }}
                >
                  {item.label}
                </Typography>
              </Box>
            </Box>
          );
        })}
      </Box>
    </Box>
  );
};

export { Parking2_1_Option1 };
