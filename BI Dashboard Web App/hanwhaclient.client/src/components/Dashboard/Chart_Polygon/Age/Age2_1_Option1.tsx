import React, { useEffect, useRef, useState } from "react";
import { Box, Typography } from "@mui/material";
import {
  IGenderData,
  AgeProps,
  IAgeTotals,
} from "../../../../interfaces/IChart";
import * as d3 from "d3";

const Age2_1_Option1: React.FC<AgeProps> = ({
  customizedWidth,
  customizedHeight,
  groupbyDateAgeData,
}) => {
  const svgRef = useRef<SVGSVGElement | null>(null);

  const [ageData, setAgeData] = useState<IGenderData[] | null>([]);

  useEffect(() => {
    if (!groupbyDateAgeData) return;

    if (!groupbyDateAgeData.length) return;

    const totals: IAgeTotals = groupbyDateAgeData.reduce<IAgeTotals>(
      (acc, curr) => {
        acc.totalYoung += curr.youngCount;
        acc.totalAdult += curr.adultCount;
        acc.totalSenior += curr.seniorCount;
        acc.totalUnknown += curr.unknownCount;
        return acc;
      },
      { totalYoung: 0, totalAdult: 0, totalSenior: 0, totalUnknown: 0 }
    );

    const chartData: IGenderData[] = [
      {
        gender: "Young",
        count: totals.totalYoung,
        minCount: totals.totalYoung,
        minDate: "",
        maxCount: totals.totalYoung,
        maxDate: "",
      },
      {
        gender: "Adult",
        count: totals.totalAdult,
        minCount: totals.totalAdult,
        minDate: "",
        maxCount: totals.totalAdult,
        maxDate: "",
      },
      {
        gender: "Senior",
        count: totals.totalSenior,
        minCount: totals.totalSenior,
        minDate: "",
        maxCount: totals.totalSenior,
        maxDate: "",
      },
      {
        gender: "Unknown",
        count: totals.totalUnknown,
        minCount: totals.totalUnknown,
        minDate: "",
        maxCount: totals.totalUnknown,
        maxDate: "",
      },
    ];

    setAgeData(chartData);
  }, [groupbyDateAgeData]);

  useEffect(() => {
    if (!svgRef.current || !ageData || ageData.length === 0) return;

    const svg = d3.select(svgRef.current);
    svg.selectAll("*").remove(); // Clear previous content

    const width = customizedWidth as number;
    const height = customizedHeight as number;
    const margin = { top: 70, right: 30, bottom: 50, left: 30 };
    const innerWidth = width - margin.left - margin.right;
    const innerHeight = height - margin.top - margin.bottom;

    const radius = Math.min(innerWidth, innerHeight) / 3;

    const colorMap: { [key: string]: string } = {
      Young: "#A1A2FF", 
      Adult: "#8EE46D", 
      Senior: "#C2C2C2", 
      Unknown: "#FFBB8D", 
    };

    const PiechartGroup = svg
      .attr("width", width)
      .attr("height", height)
      .append("g")
      .attr(
        "transform",
        `translate(${innerWidth / 2 + 40}, ${innerHeight / 2 + 40})`
      );
    // .attr("transform", `translate(${margin.left},${margin.top})`);

    const pie = d3
      .pie<IGenderData>()
      .value((d) => d.count)
      .sort(null);

    const arcOuter = d3
      .arc()
      .innerRadius(0)
      .outerRadius(radius + 25);

    PiechartGroup.selectAll(".outer-arc")
      .data(pie(ageData))
      .enter()
      .append("path")
      .attr("class", "outer-arc")
      .attr("d", arcOuter as any)
      .attr("fill", "white")
      .attr("stroke", "#DBDADA")
      .style("stroke-width", "2px");

    const arcInner = d3.arc().innerRadius(0).outerRadius(radius);

    PiechartGroup.selectAll(".inner-arc")
      .data(pie(ageData))
      .enter()
      .append("path")
      .attr("class", "inner-arc")
      .attr("d", arcInner as any)
      .attr("fill", (d) => colorMap[d.data.gender] || "#ccc")
      .attr("stroke", "#DBDADA")
      .style("stroke-width", "1px")
      .on("mouseover", (_, d) => {
        d3.select("#tooltip")
          .style("opacity", 1)
          .html(`<strong>${d.data.gender}</strong> : ${d.data.count}`);
      })
      .on("mousemove", (event) => {
        d3.select("#tooltip")
          .style("left", event.pageX + 10 + "px")
          .style("top", event.pageY - 20 + "px");
      })
      .on("mouseout", () => {
        d3.select("#tooltip").style("opacity", 0);
      });
    return () => {
      //tooltip.remove();
    };
  }, [customizedWidth, ageData]);

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
      {!ageData ||
      ageData.length === 0 ||
      ageData.every((item) => item.count === 0) ? (
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
        <>
          <svg ref={svgRef}></svg>
          <Box sx={{ display: "flex", justifyContent: "center", gap: 4 }}>
            {/* Young */}
            <Box
              sx={{
                display: "flex",
                flexDirection: "column",
                alignItems: "center",
              }}
            >
              <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                <Box
                  sx={{
                    backgroundColor: "#A1A2FF",
                    width: 10,
                    height: 10,
                    borderRadius: "50%",
                  }}
                />
                <Typography sx={{ color: "#626262", fontSize: "12px" }}>
                  Young
                </Typography>
              </Box>
            </Box>

            {/* Adult */}
            <Box
              sx={{
                display: "flex",
                flexDirection: "column",
                alignItems: "center",
              }}
            >
              <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                <Box
                  sx={{
                    backgroundColor: "#8EE46D",
                    width: 10,
                    height: 10,
                    borderRadius: "50%",
                  }}
                />
                <Typography sx={{ color: "#626262", fontSize: "12px" }}>
                  Adult
                </Typography>
              </Box>
            </Box>

            {/* Senior */}
            <Box
              sx={{
                display: "flex",
                flexDirection: "column",
                alignItems: "center",
              }}
            >
              <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                <Box
                  sx={{
                    backgroundColor: "#C2C2C2",
                    width: 10,
                    height: 10,
                    borderRadius: "50%",
                  }}
                />
                <Typography sx={{ color: "#626262", fontSize: "12px" }}>
                  Senior
                </Typography>
              </Box>
            </Box>

            {/* Unknown */}
            <Box
              sx={{
                display: "flex",
                flexDirection: "column",
                alignItems: "center",
              }}
            >
              <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                <Box
                  sx={{
                    backgroundColor: "#FFBB8D",
                    width: 10,
                    height: 10,
                    borderRadius: "50%",
                  }}
                />
                <Typography sx={{ color: "#626262", fontSize: "12px" }}>
                  Unknown
                </Typography>
              </Box>
            </Box>
          </Box>
        </>
      )}
    </Box>
  );
};

export { Age2_1_Option1 };
