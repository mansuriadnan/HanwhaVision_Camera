import React, { useEffect, useRef, useState } from "react";
import { Box, Typography } from "@mui/material";
import { IMaintenanceModeData, IMaintenanceModeProps, IMaintenanceModeStatus } from "../../../../interfaces/IChart";
import * as d3 from "d3";

type RmaStatus = "InProgress" | "Completed";

const CamerasInRMA2_1_Option1: React.FC<IMaintenanceModeProps> = ({
    customizedWidth,
    customizedHeight,
    mMData
}) => {

    const svgRef = useRef<SVGSVGElement | null>(null);

    const [statusData, setStatusData] = useState<IMaintenanceModeStatus[] | null>([]);
    const isRmaStatus = (value: string): value is RmaStatus =>
        value === "InProgress" || value === "Completed";
    useEffect(() => {
        if (!mMData) return;

        if (!mMData.length) return;

        const grouped = mMData.reduce(
            (acc, item) => {
                if (isRmaStatus(item.rmaStatus)) {
                    acc.items[item.rmaStatus].push(item);
                    acc.counts[item.rmaStatus]++;
                    acc.total++;
                }
                return acc;
            },
            {
                items: {
                    InProgress: [] as IMaintenanceModeData[],
                    Completed: [] as IMaintenanceModeData[],
                },
                counts: {
                    InProgress: 0,
                    Completed: 0,
                },
                total: 0,
            }
        );

        const chartData: IMaintenanceModeStatus[] = [
            {
                status: "InProgress",
                count: grouped.counts.InProgress,
                minCount: grouped.counts.InProgress, // fallback: same as total
                minDate: "",
                maxCount: grouped.counts.InProgress,
                maxDate: "",
            },
            {
                status: "Completed",
                count: grouped.counts.Completed,
                minCount: grouped.counts.Completed,
                minDate: "",
                maxCount: grouped.counts.Completed,
                maxDate: "",
            }
        ];

        setStatusData(chartData);

    }, [mMData]);

    useEffect(() => {
        if (!svgRef.current || !statusData || statusData.length === 0) return;

        const svg = d3.select(svgRef.current);
        svg.selectAll("*").remove(); // Clear previous content

        const width = customizedWidth as number;
        const height = customizedHeight as number;
        const margin = { top: 70, right: 30, bottom: 50, left: 30 };
        const innerWidth = width - margin.left - margin.right;
        const innerHeight = height - margin.top - margin.bottom;

        const radius = Math.min(innerWidth, innerHeight) / 3;

        const colorMap: { [key: string]: string } = {
            InProgress: "#EFBA48", // Yellow
            completed: "#9E9E9E", // Blue
        };

        const PiechartGroup = svg
            .attr("width", width)
            .attr("height", height)
            .append("g")
            .attr("transform", `translate(${innerWidth / 2 + 40}, ${innerHeight / 2 + 40})`);

        const pie = d3
            .pie<IMaintenanceModeStatus>()
            .value((d) => d.count)
            .sort(null);

        const arcOuter = d3
            .arc()
            .innerRadius(0)
            .outerRadius(radius + 25);

        PiechartGroup.selectAll(".outer-arc")
            .data(pie(statusData))
            .enter()
            .append("path")
            .attr("class", "outer-arc")
            .attr("d", arcOuter as any)
            .attr("fill", "white")
            .attr("stroke", "#DBDADA")
            .style("stroke-width", "2px");


        const arcInner = d3.arc().innerRadius(0).outerRadius(radius);

        PiechartGroup.selectAll(".inner-arc")
            .data(pie(statusData))
            .enter()
            .append("path")
            .attr("class", "inner-arc")
            .attr("d", arcInner as any)
            .attr("fill", (d) => colorMap[d.data.status] || "#ccc")
            .attr("stroke", "#DBDADA")
            .style("stroke-width", "1px")
            .on("mouseover", (_, d) => {
                d3.select("#tooltip")
                    .style("opacity", 1)
                    .html(`<strong>${d.data.status}</strong> : ${d.data.count}`);
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
        };
    }, [customizedWidth, statusData]);

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
            {!statusData || statusData.length === 0 || statusData.every(item => item.count === 0) ? (
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
            )
                :
                (
                    <>
                        <svg ref={svgRef}></svg>
                        <Box sx={{ display: "flex", justifyContent: "center", gap: 4 }}>

                            <Box sx={{ display: "flex", flexDirection: "column", alignItems: "center" }}>
                                <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                    <Box sx={{ backgroundColor: "#EFBA48", width: 10, height: 10, borderRadius: "50%" }} />
                                    <Typography sx={{ color: "#626262", fontSize: "12px" }}>In progress</Typography>
                                </Box>

                            </Box>


                            <Box sx={{ display: "flex", flexDirection: "column", alignItems: "center" }}>
                                <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                    <Box sx={{ backgroundColor: "#9E9E9E", width: 10, height: 10, borderRadius: "50%" }} />
                                    <Typography sx={{ color: "#626262", fontSize: "12px" }}>Completed</Typography>
                                </Box>

                            </Box>


                        </Box>
                    </>
                )
            }
        </Box>
    );
};

export { CamerasInRMA2_1_Option1 };
