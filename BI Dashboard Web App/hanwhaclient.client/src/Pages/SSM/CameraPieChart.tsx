import React, { useEffect, useRef } from "react";
import * as d3 from "d3";

interface Props {
  data: { label: string; value: number; color: string }[];
  total: number;
  size?: number;
  innerRadius?: number;
}

const CameraPieChart: React.FC<Props> = ({
  data,
  total,
  size = 260,
  innerRadius = 50,
}) => {
  const ref = useRef<SVGSVGElement | null>(null);
  const savedLang = localStorage.getItem("i18nextLng") || "en";
  const isRTL = savedLang === "ar";

  useEffect(() => {
    if (!ref.current) return;

    const svg = d3.select(ref.current);
    svg.selectAll("*").remove();

    const width = size;
    const height = size;
    const radius = Math.min(width, height) / 2;

    const chart = svg
      .attr("width", width)
      .attr("height", height)
      .append("g")
      .attr("transform", `translate(${width / 2}, ${height / 2})`);

    const pie = d3
      .pie<any>()
      .value((d) => d.value)
      .sort(null);

    const arc = d3
      .arc<any>()
      .innerRadius(innerRadius)
      .outerRadius(radius);

    const arcs = chart.selectAll("arc").data(pie(data)).enter();

    // DRAW ARC
    arcs
      .append("path")
      .attr("d", arc)
      .attr("fill", (d) => d.data.color);
    //   .attr("stroke", "#666666")
    //   .style("stroke-width", "1px");

    // ADD this entire block after the arcs .append("path") chain

pie(data).forEach((d) => {
  const angle = d.startAngle;

  const extendBy = 12; // controls how far the line sticks out, increase/decrease as needed
  const outerX = Math.sin(angle) * (radius + extendBy);
  const outerY = -Math.cos(angle) * (radius + extendBy);
  const innerX = Math.sin(angle) * innerRadius;
  const innerY = -Math.cos(angle) * innerRadius;

  // Line from inner to outer radius
  chart
    .append("line")
    .attr("x1", innerX)
    .attr("y1", innerY)
    .attr("x2", outerX)
    .attr("y2", outerY)
    .attr("stroke", "#666666")
    .attr("stroke-width", 1.5);

    // add pin 

    const pinWidth = 6;
    const pinHeight = 16;

    chart
    .append("rect")
    .attr("x", outerX - pinWidth / 2)
    .attr("y", outerY - pinHeight / 2)
    .attr("width", pinWidth)
    .attr("height", pinHeight)
    .attr("rx", pinWidth / 2)  //  makes left and right sides fully curved (pill shape)
    .attr("ry", pinWidth / 2)  //  same value as rx for symmetrical curves
    .attr("fill", "#666666")
    .attr("transform", `rotate(${(angle * 180) / Math.PI}, ${outerX}, ${outerY})`);

    //add the label of total/online or total/offline
    const labelRadius = radius +5; // distance from center to label
    const labelX = Math.sin(angle + (d.endAngle - d.startAngle) / 2) * labelRadius;
    const labelY = -Math.cos(angle + (d.endAngle - d.startAngle) / 2) * labelRadius;

    const midAngle = d.startAngle + (d.endAngle - d.startAngle) / 2;
    const isRight = Math.sin(midAngle) > 0;
    const textAnchor = isRTL
      ? (isRight ? "end" : "start")
      : (isRight ? "start" : "end");


    chart
    .append("text")
    .attr("x", labelX-5)
    .attr("y", labelY)
    .attr("text-anchor", textAnchor)
    .attr("dominant-baseline", "central")
    .html(
        `<tspan style="font-size:12; font-weight:700; fill:#626262">${d.data.value.toLocaleString()}</tspan><tspan style="font-size:10px; font-weight:400; fill:#626262">/ ${total.toLocaleString()}</tspan>`
    );
    });



  }, [data, size, innerRadius,total]);

  return <svg ref={ref} style={{ overflow: "visible" }}></svg>;
};

export default CameraPieChart;