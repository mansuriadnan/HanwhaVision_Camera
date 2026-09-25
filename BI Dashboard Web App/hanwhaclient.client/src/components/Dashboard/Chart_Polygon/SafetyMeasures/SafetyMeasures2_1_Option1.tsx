import { Box } from "@mui/material";
import { ISafetyMeasuresDataProps } from "../../../../interfaces/IChart";
import { useEffect, useRef } from "react";
import * as d3 from "d3";
import { useThemeContext } from "../../../../context/ThemeContext";

const SafetyMeasures2_1_Option1: React.FC<ISafetyMeasuresDataProps> = ({
  safetyMeasuresData,
  safetyMeasuresChartData,
  customizedWidth,
  customizedHeight,
}) => {
  const { theme } = useThemeContext();
  const svgRef = useRef<SVGSVGElement | null>(null);

  const allZero =
    !safetyMeasuresData ||
    ["maskCount", "withoutMaskCount"].every(
      (key) =>
        (safetyMeasuresData as any)[key] === 0 ||
        (safetyMeasuresData as any)[key] == null
    );

  const data = [
    {
      labelLeft: "Mask",
      valueLeft: safetyMeasuresData?.maskCount,
      colorLeft: "#EEFF83",
      labelRight: "Without Mask",
      valueRight: safetyMeasuresData?.withoutMaskCount,
    },
    // {
    //   labelLeft: "Helmet",
    //   valueLeft: safetyMeasuresData?.withHelmet,
    //   colorLeft: "#7BF8FF",
    //   labelRight: "Without Helmet",
    //   valueRight: safetyMeasuresData?.withoutHelmet,
    // },
    // {
    //   labelLeft: "Safety Jacket",
    //   valueLeft: safetyMeasuresData?.withSafetyJacket,
    //   colorLeft: "#6E7CFF",
    //   labelRight: "Without Safety Jacket",
    //   valueRight: safetyMeasuresData?.withoutSafetyJacket,
    // },
  ];

  useEffect(() => {
    if (
      !svgRef.current ||
      !safetyMeasuresChartData ||
      safetyMeasuresChartData.length === 0
    )
      return;

    const svg = d3.select(svgRef.current);
    svg.selectAll("*").remove();

    const width = customizedWidth as number;
    const height = customizedHeight as number;
    const margin = { top: 30, right: 30, bottom: 115, left: 30 };
    const innerWidth = width - margin.left - margin.right;
    const innerHeight = height - margin.top - margin.bottom;
    const barHeight = 30;
    const barGap = 70;

    // svg
    //   .attr("width", width)
    //   .attr("height", height)
    //   .attr("viewBox", `0 0 ${width} ${height}`)
    //   .style("overflow", "visible");

    svg
      .attr("width", width)
      .attr("height", height)
      .append("g")
      .attr("transform", `translate(${margin.left},${margin.top})`);

    // Patterns for "Without" bars
    const defs = svg.append("defs");
    const pattern = defs
      .append("pattern")
      .attr("id", "diagonalHatch")
      .attr("patternUnits", "userSpaceOnUse")
      .attr("width", 6)
      .attr("height", 6)
      .attr("patternTransform", "rotate(55)"); // Optional for angle

    // Transparent background
    pattern
      .append("rect")
      .attr("width", 6)
      .attr("height", 6)
      .attr("fill", theme === "light" ? "#EDEDED" : "#737373");
    pattern
      .append("path")
      .attr("d", "M0,0 L0,6") // vertical line, rotated to appear diagonal
      .attr("stroke", theme === "light" ? "#DADADA" : "#6A6A6A")
      .attr("stroke-width", 3)
      .attr("fill", theme === "light" ? "#EDEDED" : "#6A6A6A"); // prevent fill bleed

    const maxValue = d3.max(data, (d) =>
      Math.max(d.valueLeft ?? 0, d.valueRight ?? 0)
    ) as number;

    const barAreaWidth = (innerWidth - 20) / 2;

    const xScale = d3
      .scaleLinear()
      .domain([0, maxValue])
      .range([0, barAreaWidth]);

    data.forEach((d, i) => {
      const startY = (innerHeight - barHeight) / 2 + margin.top;
      const y = startY + i * (barHeight + barGap);
      // const leftX = margin.left;
      // const rightX = width / 2 + 20;
      // const dividerX = width / 2 - 10;
      // const maxLeftBarWidth = dividerX - leftX - 5;
      // const leftBarWidth = Math.min(xScale(d.valueLeft ?? 0), maxLeftBarWidth);
      // const rightBarWidth = Math.min(
      //   xScale(d.valueRight ?? 0),
      //   maxLeftBarWidth
      // );

      const centerX = width / 2;
      const dividerWidth = 4;
      const barGapFromDivider = 8;

      const leftX = margin.left;
      const leftBarMaxWidth =
        centerX - dividerWidth / 2 - barGapFromDivider - leftX;

      const rightX = centerX + dividerWidth / 2 + barGapFromDivider;

      const dividerX = centerX - dividerWidth / 2;

      const leftBarWidth = Math.min(xScale(d.valueLeft ?? 0), leftBarMaxWidth);

      const rightBarWidth = Math.min(
        xScale(d.valueRight ?? 0),
        leftBarMaxWidth
      );

      const textThreshold = 50;
      // Left bar
      svg
        .append("rect")
        .attr("x", leftX)
        .attr("y", y)
        .attr("width", leftBarWidth)
        .attr("height", barHeight)
        .attr("rx", 10)
        .attr("fill", d.colorLeft);

      // Right bar
      svg
        .append("rect")
        .attr("x", rightX)
        .attr("y", y)
        .attr("width", rightBarWidth)
        .attr("height", barHeight)
        .attr("rx", 10)
        .attr("fill", "url(#diagonalHatch)");
      // .attr("stroke", "#ccc");

      // Divider line
      svg
        .append("rect")
        .attr("x", dividerX)
        .attr("y", y)
        .attr("width", 4)
        .attr("height", barHeight)
        .attr("fill", "#888")
        .attr("rx", 2);

      // Left label
      svg
        .append("text")
        .attr("x", leftX)
        .attr("y", y - 6)
        .text(d.labelLeft)
        .attr("font-size", "12px")
        .attr("fill", theme === "light" ? "#333" : "#FFFFFF");

      // Right label
      if (rightBarWidth < textThreshold) {
        // Small bar: align text at start of bar (left-aligned)
        svg
          .append("text")
          .attr("x", rightX + 5) // slight right padding inside bar start
          .attr("y", y - 6)
          .text(d.labelRight)
          .attr("font-size", "12px")
          .attr("fill", theme === "light" ? "#333" : "#FFFFFF")
          .attr("text-anchor", "start");
      } else {
        // Large bar: align text at end of bar (right-aligned)
        svg
          .append("text")
          .attr("x", rightX + rightBarWidth - 5) // slight left padding from bar end
          .attr("y", y - 6)
          .text(d.labelRight)
          .attr("font-size", "12px")
          .attr("fill", theme === "light" ? "#333" : "#FFFFFF")
          .attr("text-anchor", "end");
      }

      // Left value
      svg
        .append("text")
        .attr("x", leftX)
        .attr("y", y + barHeight + 16)
        .text((d.valueLeft ?? 0).toLocaleString())
        .attr("text-anchor", "start")
        .attr("font-size", "14px")
        .attr("fill", theme === "light" ? "#333" : "#FFFFFF")
        .attr("font-weight", "bold");

      // Right value
      if (rightBarWidth < textThreshold) {
        // Small bar: align value at start of bar
        svg
          .append("text")
          .attr("x", rightX + 5)
          .attr("y", y + barHeight + 16)
          .text((d.valueRight ?? 0).toLocaleString())
          .attr("font-size", "14px")
          .attr("font-weight", "bold")
          .attr("text-anchor", "start");
      } else {
        // Large bar: align value at end of bar
        svg
          .append("text")
          .attr("x", rightX + rightBarWidth - 5)
          .attr("y", y + barHeight + 16)
          .text((d.valueRight ?? 0).toLocaleString())
          .attr("font-size", "14px")
          .attr("font-weight", "bold")
          .attr("fill", theme === "light" ? "#333" : "#FFFFFF")
          .attr("text-anchor", "end");
      }
    });
  }, [customizedWidth, customizedHeight, safetyMeasuresChartData, theme]);
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
      {allZero ? (
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
        <svg ref={svgRef}></svg>
      )}
    </Box>
  );
};
export { SafetyMeasures2_1_Option1 };
