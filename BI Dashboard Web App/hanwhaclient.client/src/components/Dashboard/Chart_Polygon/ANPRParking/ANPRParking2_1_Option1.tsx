import React, { useEffect, useRef, useState } from "react";
import {
  ANPRParkingProps,
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

const ANPRParking2_1_Option1: React.FC<ANPRParkingProps> = ({
  anprparkingChartData,
  customizedWidth,
  customizedHeight,
  startDate,
  endDate,
  floor,
  zones,
  setExportHandler,
  anprParkingZoneWiseData,
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
      color: "#E7EB78",
    },
  ];

  const handleTickChanges = (intervalData: IntervalData) => {
    setSelectedInterval(intervalData.tickInterval);
    setSelectedIntervalName(intervalData.intervalName);
  };

  useExportHandler({
    apiEndpoint: `${apiUrls.ANPRVehicleParking}/csv`,
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
    if (!anprparkingChartData || !categories) return;
    const updatedParsedData: IParsedDataWithCategory = {};

    categories.forEach((cat) => {
      updatedParsedData[cat.key] = anprparkingChartData.map((d) => ({
        date: new Date(formatDateToConfiguredTimezone(d.entryTime) as string),
        value: 1,
      }));
    });

    setParsedDataWithCategory(updatedParsedData);
  }, [anprparkingChartData, selectedInterval]);

  const diffInMinutes = (from: Date, to: Date): number => {
    return Math.floor((to.getTime() - from.getTime()) / (1000 * 60));
  };

  const generateFinalData = (
    result: IParsedDataWithCategory,
    occupancy: number,
  ) => {
    if (!anprparkingChartData || !categories) return;

    var resultData = result;

    for (let i = 0; i < resultData?.occupied.length; i++) {
      const element = resultData.occupied[i];

      const bucketStart = new Date(element.date);
      const bucketEnd = new Date(bucketStart);
      bucketEnd.setMinutes(
        bucketEnd.getMinutes() +
          diffInMinutes(
            resultData.occupied[0].date,
            resultData.occupied[1].date,
          ),
      );

      let count = 0;
      var addedDeviceId: string[] = [];
      for (let j = 0; j < anprparkingChartData.length; j++) {
        const item = anprparkingChartData[j];
        const startDate = new Date(item.entryTime);
        const endDate = item.exitTime ? new Date(item.exitTime) : null;
        const effectiveEnd = endDate ?? new Date();
        if(addedDeviceId.filter(x => x == item.anprVehicleId).length > 0) {
          continue;
        }
        if (bucketStart < effectiveEnd && bucketEnd > startDate) {
          addedDeviceId.push(item.anprVehicleId);
          count++;
        }
      }

      element.value = count;

      // Set available value (row-wise)
      if (resultData.available && resultData.available[i]) {
        resultData.available[i].value = Math.max(occupancy - count, 0);
      }
    }

    setFinalData(resultData);
  };

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
          operation="sum"
          chartTicksChange={(data) => handleTickChanges(data)}
          onChartDataChange={(result) => {
            generateFinalData(result, anprParkingZoneWiseData?.totalSlot ?? 0);
            // setFinalData(result);
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

export { ANPRParking2_1_Option1 };
