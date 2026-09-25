import { useEffect, useRef, useState } from "react";
import * as d3 from "d3";
import { Box, Typography } from "@mui/material";
import {
  IParsedDataWithCategory,
  IntervalData,
  IMaintenanceModeProps,
  IMaintenanceModeData,
} from "../../../../interfaces/IChart";
import { ChartTicksSelector } from "../../../index";
import { formatDateToConfiguredTimezone } from "../../../../utils/formatDateToConfiguredTimezone";
import { formatNumber } from "../../../../utils/formatNumber";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";
import apiUrls from "../../../../constants/apiUrls";
import { useThemeContext } from "../../../../context/ThemeContext";
import { useTimeFormatContext } from "../../../../context/TimeFormatContext";
import { getTimeTickFormatter } from "../../../../utils/formatTimeTick";

export interface SplitRmaDate {
  date: Date;
  deviceId: string;
  rmaStatus: string;
}
const CamerasInRMA2_1_Option2: React.FC<IMaintenanceModeProps> = ({
  mMData,
  customizedWidth,
  customizedHeight,
  startDate,
  endDate,
  floor,
  zones,
    setExportHandler
}) => {
  type StatusKey = "inProgress" | "completed";
  const categories: { key: StatusKey; label: string; color: string }[] = [
    { key: "inProgress", label: "InProgress", color: "#EFBA48" },
    { key: "completed", label: "Completed", color: "#9E9E9E" },
  ];

  const svgRef = useRef<SVGSVGElement | null>(null);
  const [selectedInterval, setSelectedInterval] = useState<number>();
  const [finalData, setFinalData] = useState<IParsedDataWithCategory>({});
  const [parsedDataWithCategory, setParsedDataWithCategory] =
    useState<IParsedDataWithCategory>({});
  const [selectedIntervalName, setSelectedIntervalName] = useState<string>("");
  const selectedIntervalNameRef = useRef<string>("");
  const { theme } = useThemeContext();
  const { timeFormat } = useTimeFormatContext();
    const [rmaData, setRmaData] = useState<IMaintenanceModeData[] | null>(mMData ?? []);

  useEffect(() => {
    if (!mMData || !categories) return;

    const updatedParsedData: IParsedDataWithCategory = {};
        const updatedMmData = mMData.map(m => ({
      ...m,
      startDate: formatDateToConfiguredTimezone(m.startDate) as string,
            endDate: formatDateToConfiguredTimezone(m.endDate) as string
    }));
    categories.forEach((cat) => {
      updatedParsedData[cat.key] = mMData.map((d) => ({
        date: new Date(
          formatDateToConfiguredTimezone(d.startDate ?? "") as string
        ),
        value: 1,
      }));
    });
    setRmaData(updatedMmData);
    setParsedDataWithCategory(updatedParsedData);
  }, [mMData, selectedInterval]);

  useExportHandler({
    apiEndpoint: `${apiUrls.ExportCamerasInRMAWidgetCSV}/csv`,
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
    if (!svgRef.current || !finalData) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll("*").remove();

    const width = customizedWidth as number;
    const height = customizedHeight as number;
    const margin = { top: 30, right: 30, bottom: 115, left: 30 };
    const innerWidth = width - margin.left - margin.right;
    const innerHeight = height - margin.top - margin.bottom;

    const MultiLinechartGroup = svg
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
      .range([0, innerWidth]);

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

    MultiLinechartGroup.append("g")
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
          .attr("opacity", theme === 'light' ? "100%" : "20%")
      .attr("stroke-width", 1);

    const line = d3
      .line<{ date: Date; value: number }>()
      .x((d) => xScale(d.date))
      .y((d) => yScale(d.value));

    categories.forEach((cat) => {
      const categoryWisedata = finalData[cat.key];
      if (!categoryWisedata || categoryWisedata.length === 0) return;

      MultiLinechartGroup.append("path")
        .datum(categoryWisedata)
        .attr("class", "line")
        .attr("fill", "none")
        .attr("stroke", cat.color)
        .attr("d", line);

      // Circles
      MultiLinechartGroup.selectAll(`.circle-${cat.key}`)
        .data(categoryWisedata)
        .enter()
        .append("circle")
        .attr("class", `circle-${cat.key}`)
        .attr("cx", (d) => xScale(d.date))
        .attr("cy", (d) => yScale(d.value))
        .attr("r", 3)
        .attr("fill", cat.color)
        .on("mouseover", (_, d) => {
          d3.select("#tooltip")
            .style("opacity", 1)
            .html(`<strong>${cat.label}</strong> : ${formatNumber(d.value)}`);
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
    MultiLinechartGroup.append("g")
      .attr("transform", `translate(0, ${innerHeight})`)
      .call(
            d3.axisBottom(xScale)
          .tickValues(xTicks)
          .tickSize(4)
          .tickFormat((d, i) => getTimeTickFormatter(d, i, xTicks, timeFormat))
      )
      // .call(d3.axisBottom(xScale).ticks(selectedInterval).tickSize(4))
      .call((g) => {
        g.select(".domain").attr("stroke", "#70757a");
            g.selectAll(".tick line").attr("stroke", theme === 'light' ? "#212121" : "#FFFFFF");
      })
      .selectAll("text")
          .style("fill", theme === 'light' ? "#212121" : "#FFFFFF")
      .style("font-size", "10px")
      .attr("transform", "rotate(-45)")
      .style("text-anchor", "end");

    // Y axis
    MultiLinechartGroup.append("g")
      .call(
        d3
          .axisLeft(yScale)
          .ticks(yTickSize)
          .tickSize(0)
          .tickFormat(d3.format("~s"))
      )
      .call((g) => g.select(".domain").attr("stroke", "none"))
      .selectAll("text")
          .style("fill",  theme === 'light' ? "#212121" : "#D4D4D4")
      .style("font-size", "10px");
  }, [customizedWidth, finalData, theme, timeFormat]);

  const handleTickChanges = (intervalData: IntervalData) => {
    setSelectedInterval(intervalData.tickInterval);
    setSelectedIntervalName(intervalData.intervalName);
  };
  const diffInMinutes = (from: Date, to: Date): number => {
    return Math.floor((to.getTime() - from.getTime()) / (1000 * 60));
    }

  const generateFinalData = (result: IParsedDataWithCategory) => {
    if (!rmaData || !categories) return;

    var resultData = result;
    // resultData.inProgress.forEach(element => {
    //     const tmpInProgressData = rmaData.filter(x => new Date(x.startDate) <= new Date(element.date) && (!x.endDate || new Date(x.endDate) > new Date(element.date)))
    //     element.value = tmpInProgressData.length;
    //     console.log("tmpInProgressData:",tmpInProgressData);

    // });
    // resultData.completed.forEach(element => {
    //     const tmpInProgressData11 = rmaData.filter(x => x.rmaStatus === "Completed" && new Date(x.endDate) <= new Date(element.date))
    //     element.value = tmpInProgressData11.length;
    // });

    for (let i = 0; i < resultData.inProgress.length; i++) {
      const element = resultData.inProgress[i];

      const bucketStart = new Date(element.date);
      const bucketEnd = new Date(bucketStart);
            bucketEnd.setMinutes(bucketEnd.getMinutes() + diffInMinutes(resultData.inProgress[0].date,resultData.inProgress[1].date)); 

      let count = 0;

      for (let j = 0; j < rmaData.length; j++) {
        const item = rmaData[j];

        const startDate = new Date(item.startDate);
        const endDate = item.endDate ? new Date(item.endDate) : null;

        const effectiveEnd = endDate ?? new Date(); // still in progress

                if (
                    bucketStart < effectiveEnd &&
                    bucketEnd > startDate
                ) {
          count++;
        }
      }

      element.value = count;
    }
    for (let i = 0; i < resultData.completed.length; i++) {
      const element = resultData.completed[i];

      const bucketStart = new Date(element.date);
      const bucketEnd = new Date(bucketStart);
            bucketEnd.setMinutes(bucketEnd.getMinutes() + diffInMinutes(resultData.completed[0].date,resultData.completed[1].date));

      let count = 0;

      for (let j = 0; j < rmaData.length; j++) {
        const item = rmaData[j];

        if (item.rmaStatus !== "Completed" || !item.endDate) continue;

        const endDate = new Date(item.endDate);

        // ✅ same bucket overlap logic, but endDate-driven
        if (endDate >= bucketStart && endDate < bucketEnd) {
          count++;
        }
      }

      element.value = count;
    }

    setFinalData(resultData);
    }


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
            setFinalData(result);
            generateFinalData(result);
          }}
        />
      )}

      <svg ref={svgRef}></svg>
      <Box
        sx={{
          display: "flex",
          flexDirection: "row",
          gap: 3,
          justifyContent: "center",
          marginBottom: 1,
        }}
      >
        {categories.map((item, idx) => (
          <Box
            key={idx}
            sx={{ display: "flex", alignItems: "center", gap: "8px" }}
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
              sx={{
                fontSize: "10px",
                color: theme === "light" ? "#212121" : "#D4D4D4",
              }}
            >
              {item.label}
            </Typography>
          </Box>
        ))}
      </Box>
    </Box>
  );
};
export { CamerasInRMA2_1_Option2 };
