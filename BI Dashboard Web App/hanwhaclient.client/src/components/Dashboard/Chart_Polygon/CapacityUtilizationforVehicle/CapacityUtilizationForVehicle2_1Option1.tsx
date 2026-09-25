import React, { useEffect, useRef } from "react";
import * as d3 from "d3";
import { DateWiseUtilization, ICapacityUtilizationforVehicleProps } from "../../../../interfaces/IChart";
import { formatNumber } from "../../../../utils/formatNumber";
import { Box } from "@mui/material";
import { useThemeContext } from "../../../../context/ThemeContext";
import moment from "moment";

const CapacityUtilizationForVehicle2_1Option1: React.FC<
  ICapacityUtilizationforVehicleProps
> = ({ customizedWidth, customizedHeight, CUForVehicleData,DateWiseUtilization }) => {
  const [overallMin, setOverallMin] = React.useState<DateWiseUtilization | null>(null);
  const [overallMax, setOverallMax] = React.useState<DateWiseUtilization | null>(null);
  const [overallTotal, setOverallTotal] = React.useState<number>(0);
  const [utilizationPercentage, setUtilizationPercentage] = React.useState<number | null>(null);
  const svgRef = useRef<SVGSVGElement | null>(null);
  const { theme } = useThemeContext();
  const legendItems = [
    { label: "Utilization", color: "#A9F7FE" },
    { label: "Percentage", color: "#F05C5C" },
    { label: "Most Day", color: "#FFFF80" },
    { label: "Least Day", color: "#8D6AF6" },
  ];

  useEffect(() => {
        if (!DateWiseUtilization || DateWiseUtilization.length === 0) {
          setOverallMin(null);
          setOverallMax(null);
          setOverallTotal(0);
          setUtilizationPercentage(0);
          return;
        }
  
        function groupByDate(data: DateWiseUtilization[]) {
          const grouped: Record<string, DateWiseUtilization[]> = {};
  
          data.forEach((item) => {
            const date = moment(item.dateTime).format("YYYY-MM-DD");
            if (!grouped[date]) {
              grouped[date] = [];
            }
            grouped[date].push({
              ...item,
              totalCount: item.totalCount,
              // totalCount: item.totalCount < 0 ? 0 : item.totalCount, // optional clamp
            });
          });
  
          return grouped;
        }
  
        //  Compute min, max, and total using latest entries
        function findMaxByDate(data: DateWiseUtilization[]) {
          const grouped = groupByDate(data);
  
          const result = Object.entries(grouped).map(([date, values]) => {
            const max = values.reduce((max, item) =>
              item.totalCount > max.totalCount ? item : max
            );
            return { date, max };
          });
  
          return result;
        }
  
  
        const MaxByDate = findMaxByDate(DateWiseUtilization);
  
        const overallMinval = MaxByDate
          .filter(item => item.max.totalCount > 0) // ignore 0
          .reduce((min, item) =>
            item.max.totalCount < min.max.totalCount ? item : min
            , MaxByDate[0]);
  
        const overallMaxval = MaxByDate.reduce((max, item) =>
          item.max.totalCount > max.max.totalCount ? item : max
        );    
      
        let overallTotal = 0;
        if (MaxByDate.length === 1 && DateWiseUtilization.length > 0) {
          overallTotal =
            DateWiseUtilization[DateWiseUtilization.length - 1].totalCount;
        } else {
          overallTotal = MaxByDate.reduce(
            (sum, item) => sum + (item.max.totalCount ?? 0),
            0
          );
        }
  
        const vehicleDefaultOccupancy = CUForVehicleData?.vehicleDefaultOccupancy ?? 0;
        const adjustedOverallTotal = overallTotal + vehicleDefaultOccupancy;
        const totalCapacity = CUForVehicleData?.totalCapacity ?? 0;
        const utilizationPercentage =
          totalCapacity > 0 ? (adjustedOverallTotal / totalCapacity) * 100 : 0;
  
  
        //  Clamp all negatives to 0 before setting state
        const safeOverallMin =
          overallMinval && overallMinval.max
            ? { ...overallMinval.max, totalCount: Math.max(0, overallMinval.max.totalCount) }
            : null;
  
        const safeOverallMax =
          overallMaxval && overallMaxval.max
            ? { ...overallMaxval.max, totalCount: Math.max(0, overallMaxval.max.totalCount) }
            : null;
  
        const safeOverallTotal = Math.max(0, adjustedOverallTotal);      
        const safeUtilizationPercentage = Math.max(0, utilizationPercentage);
  
        //  Update states
        setOverallMin(safeOverallMin);
        setOverallMax(safeOverallMax);
        setOverallTotal(safeOverallTotal);
        setUtilizationPercentage(safeUtilizationPercentage);
  
        // //  Update states
        // setOverallMin(overallMinval?.max ?? null);
        // setOverallMax(overallMaxval.max);
        // setOverallTotal(overallTotal);
        // setUtilizationPercentage(utilizationPercentage);
      }, [DateWiseUtilization, CUForVehicleData]);
  

  useEffect(() => {
    const svg = d3.select(svgRef.current);
    svg.selectAll("*").remove();

    const width = customizedWidth as number;
    const height = customizedHeight as number;
    const centerX = width / 2;
    const centerY = height / 2;

    const rawValues = [
      overallTotal ?? 0,
      utilizationPercentage ?? 0,
      overallMax?.totalCount ?? 0,
      overallMin?.totalCount ?? 0,
    ];


    const maxVal = Math.max(...rawValues, 1); // avoid divide by zero
    const minRadius = Math.min(width, height) * 0.05; // 5% of chart size
    const maxRadius = Math.max(width, height) * 0.2; // 20% of chart size

    const radiusScale = d3
      .scaleSqrt()
      .domain([0, maxVal])
      .range([minRadius, maxRadius]);

    const radii = {
      utilization: radiusScale(overallTotal ?? 0),
      percentage: radiusScale(utilizationPercentage ?? 0),
      mostDay: radiusScale(overallMax?.totalCount ?? 0),
      leastDay: radiusScale(overallMin?.totalCount ?? 0),
    };

    function getSafeRadius(x: number, y: number, desiredRadius: number): number {
      const padding = 10;

      const maxLeft = x - padding;
      const maxRight = width - x - padding;
      const maxTop = y - padding;
      const maxBottom = height - y - padding;

      const maxAllowedRadius = Math.min(
        desiredRadius,
        maxLeft,
        maxRight,
        maxTop,
        maxBottom
      );

      return Math.max(minRadius, Math.min(maxAllowedRadius, desiredRadius));
    }

    const circles = [
      {
        label: "Utilization",
        value: Math.round(overallTotal ?? 0),
        color: "#A9F7FE",
        x: centerX - width * 0.15,
        y: centerY - height * 0.15,
        radius: getSafeRadius(centerX - width * 0.15, centerY - height * 0.15, radii.utilization),
        labelColor: "#000",
      },
      {
        label: "Percentage",
        value: Math.round(utilizationPercentage ?? 0),
        color: "#F05C5C",
        x: centerX + width * 0.05,
        y: centerY - height * 0.1,
        radius: getSafeRadius(centerX + width * 0.05, centerY - height * 0.1, radii.percentage),
        labelColor: "#fff",
      },
      {
        label: "Most Day",
        value: Math.round(overallMax?.totalCount ?? 0),
        color: "#FFFF80",
        x: centerX - width * 0.1,
        y: centerY + height * 0.1,
        radius: getSafeRadius(centerX - width * 0.1, centerY + height * 0.1, radii.mostDay),
        labelColor: "#000",
      },
      {
        label: "Least Day",
        value: Math.round(overallMin?.totalCount ?? 0),
        color: "#8D6AF6",
        x: centerX + width * 0.1,
        y: centerY + height * 0.075,
        radius: getSafeRadius(centerX + width * 0.1, centerY + height * 0.075, radii.leastDay),
        labelColor: "#fff",
      },
    ];

    svg.attr("width", width).attr("height", height);
    const g = svg.append("g");

    circles.forEach((circle) => {
      // Tooltip
      // const tooltip = d3
      //   .select("body")
      //   .append("div")
      //   .style("position", "absolute")
      //   // .style("background", "#333")
      //   .style("color", "#333")
      //   .style("padding", "6px 10px")
      //   .style("border-radius", "4px")
      //   .style("font-size", "12px")
      //   .style("pointer-events", "none")
      //   .style("opacity", 0);

      const group = g.append("g");

      group
        .append("circle")
        .attr("cx", circle.x)
        .attr("cy", circle.y)
        .attr("r", circle.radius)
        .attr("fill", circle.color)
        .attr("opacity", 0.85)
        .on("mouseover", (event) => {
          d3.select("#tooltip")
            .style("opacity", 1)
            .html(
              `<strong>${circle.label}:</strong> ${circle.label === "Percentage"
                ? `${circle.value}%`
                : formatNumber(circle.value)
              }`
            );
        })
        .on("mousemove", (event) => {
          d3.select("#tooltip")
            .style("left", event.pageX + 10 + "px")
            .style("top", event.pageY - 28 + "px");
        })
        .on("mouseout", () => {
          d3.select("#tooltip").style("opacity", 0);
        });

      group
        .append("text")
        .attr("x", circle.x)
        .attr("y", circle.y + 5)
        .attr("text-anchor", "middle")
        .attr("fill", circle.labelColor)
        .attr("font-size", 15)
        .text(
          circle.label === "Percentage"
            ?  `${formatNumber(circle.value)}%`
            : formatNumber(circle.value)
        );
    });
  }, [overallMin, overallMax, overallTotal, utilizationPercentage, customizedWidth, customizedHeight, theme]);

  // return <svg ref={svgRef}></svg>;
  return (
    <div
      style={{
        width: customizedWidth,
        height: customizedHeight,
        display: "flex",
        flexDirection: "column",
        padding: "10px 0px",
      }}
    >
      {!CUForVehicleData ||
        CUForVehicleData?.totalCapacity === 0 || CUForVehicleData.utilization === 0?
        <Box
          sx={{
            height: customizedHeight,
            width: customizedWidth,
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            fontSize: "12px",
            color: "#A8A8A8",
          }}
        >
          No Data Found
        </Box>
        :
        <>
          <svg ref={svgRef} />
          <Box
            sx={{
              display: "flex",
              justifyContent: "center",
              alignItems: "center",
              flexWrap: "wrap",
              gap: 1,
              marginTop: 2,
            }}
          >
            {legendItems.map((item, index) => (
              <Box
                key={index}
                sx={{
                  display: "flex",
                  alignItems: "center",
                  fontSize: "12px",
                }}
              >
                <Box
                  sx={{
                    width: 10,
                    height: 10,
                    borderRadius: "50%",
                    backgroundColor: item.color,
                    marginRight: 1,
                  }}
                />
                <span style={{ color: theme === 'light' ? "#212121" : "#FFFFFF" }}>
                  {item.label}
                </span>
              </Box>
            ))}
          </Box>
        </>
      }
    </div>
  );
};

export { CapacityUtilizationForVehicle2_1Option1 };
