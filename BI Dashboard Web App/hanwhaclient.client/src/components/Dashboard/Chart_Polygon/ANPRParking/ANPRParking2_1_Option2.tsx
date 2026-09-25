import React, { useEffect, useRef } from "react";
import {
  ANPRParkingProps,
  IParkingTreeChartData,
} from "../../../../interfaces/IChart";
import { Box } from "@mui/material";
import * as d3 from "d3";
import { formatNumber } from "../../../../utils/formatNumber";
import { useExportHandler } from "../../../../hooks/useExportHandler";
import { convertDateToISOLikeString } from "../../../../utils/convertDateToISOLikeString";
import apiUrls from "../../../../constants/apiUrls";

const ANPRParking2_1_Option2: React.FC<ANPRParkingProps> = ({
  anprParkingTreeChartData,
  customizedWidth,
  customizedHeight,
  startDate,
  endDate,
  floor,
  zones,
  setExportHandler,
}) => {
  const ref = useRef<SVGSVGElement>(null);
  const selectedIntervalNameRef = useRef<string>("");

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
    if (!anprParkingTreeChartData || anprParkingTreeChartData.length === 0)
      return;
    const width = customizedWidth as number;
    const height = customizedHeight as number;
    const margin = { top: 50, right: 10, bottom: 30, left: 10 };
    const innerWidth = width - margin.left - margin.right;
    const innerHeight = height - margin.top - margin.bottom;

    // const root = d3
    //   .hierarchy<{ children: IParkingTreeChartData[] }>({
    //     children: anprParkingTreeChartData,
    //   })
    //   .sum((d) => d.totalParkingOccupancy);

    const root = d3
      .hierarchy<IParkingTreeChartData>(
        { children: anprParkingTreeChartData } as any,
        (d) => (d as any).children,
      )
      .sum((d) => d.totalParkingOccupancy);

    d3
      .treemap<IParkingTreeChartData>()
      .size([innerWidth, innerHeight])
      .padding(0.5)(root);

    const maxCount =
      d3.max(anprParkingTreeChartData, (d) => d.totalParkingOccupancy) || 1;

    // const color = d3.scaleSequential()
    //     .domain([0, maxCount])
    //     .interpolator(d3.interpolateBlues);
    const color = d3
      .scaleSequential()
      .domain([0, maxCount])
      .interpolator(d3.interpolateRgb("#FCFFA7", "#BFC621"));

    const svg = d3.select(ref.current);
    svg.selectAll("*").remove();
    svg.attr("width", width).attr("height", height);

    const nodes = svg
      .selectAll("g")
      .data(root.leaves() as HierarchyRectangularNode<IParkingTreeChartData>[])
      .enter()
      .append("g")
      .attr(
        "transform",
        (d) => `translate(${d.x0 + margin.left},${d.y0 + margin.top})`,
      );

    nodes
      .on("mouseover", (_, d) => {
        d3.select("#tooltip")
          .style("opacity", 1)
          .style("z-index", 1000)
          .html(
            `<strong>${d.data.zoneName} : </strong> ${formatNumber(d.data.parkingCount)}/${formatNumber(d.data.totalParkingOccupancy)}`,
          );
      })
      .on("mousemove", (event) => {
        d3.select("#tooltip")
          .style("left", event.pageX + 10 + "px")
          .style("top", event.pageY - 20 + "px");
      })
      .on("mouseout", () => {
        d3.select("#tooltip").style("opacity", 0);
      });

    nodes
      .append("rect")
      .attr("width", (d) => d.x1 - d.x0)
      .attr("height", (d) => d.y1 - d.y0)
      .attr("fill", (d) => color(d.data.totalParkingOccupancy))
      .attr("stroke", "#fff");

    nodes
      .append("text")
      .attr("x", 4)
      .attr("y", 14)
      .attr("fill", "#003E5E")
      .style("font-size", "12px")
      .each(function (d) {
        const text = d3.select(this);
        const lineHeight = 14;
        const name = d.data.zoneName;
        const totalcount = d.data.totalParkingOccupancy;
        const parkingcount = d.data.parkingCount;

        const rectWidth = d.x1 - d.x0;
        const maxChars = Math.floor(rectWidth / 12); // ~7px per character
        const truncatedName =
          name.length > maxChars ? name.slice(0, maxChars) + "..." : name;

        // First line: Feature name
        text
          .append("tspan")
          .attr("x", 4)
          .attr("y", 14)
          .style("font-weight", "600")
          .style("font-size", "10px")
          .style("fill", "#003E5E")
          .text(truncatedName);

        // Second line: Total count
        text
          .append("tspan")
          .attr("x", 4)
          .attr("y", 14 + lineHeight)
          .style("font-weight", "600")
          .style("font-size", "12px")
          .style("fill", "#212121")
          .text(`${formatNumber(parkingcount)}/${formatNumber(totalcount)}`);
      });
  }, [anprParkingTreeChartData, customizedWidth, customizedHeight]);

  return (
    <div
      style={{
        width: customizedWidth,
        height: customizedHeight,
        display: "flex",
        flexDirection: "column",
        padding: "10px 0px",
        // overflow: "hidden",
      }}
    >
      {anprParkingTreeChartData?.length === 0 ? (
        <Box
          sx={{
            height: customizedHeight,
            width: customizedWidth,
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            fontSize: "16px",
            color: "#888",
          }}
        >
          No Data Found
        </Box>
      ) : (
        <svg
          ref={ref}
          width={customizedWidth}
          height={customizedHeight}
          // height={Math.max(
          //   (anprParkingTreeChartData || []).length * 40,
          //   customizedHeight
          // )}
        />
      )}
    </div>
  );
};

export { ANPRParking2_1_Option2 };
