// CommonAreaChart.tsx

import React, { useEffect, useRef } from 'react';
import * as d3 from 'd3';
import { useTimeFormatContext } from '../../context/TimeFormatContext';
import { IChartData } from '../../interfaces/IManageiDRAC';


interface Props {
    data: IChartData[];

    totalLabel?: string;
    totalValue?: string;

    usedLabel?: string;
    usedValue?: string;

    width?: number;
    height?: number;
}
const CommonAreaChart: React.FC<Props> = ({
    data,
    totalLabel,
    totalValue,
    usedLabel,
    usedValue,
    width = 340,
    height = 260,
}) => {
    const svgRef = useRef<SVGSVGElement | null>(null);
    const { timeFormat } = useTimeFormatContext();

    useEffect(() => {
        if (!data?.length) return;

        const svg = d3.select(svgRef.current);
        svg.selectAll('*').remove();       

        const margin = {
            top: 20,
            right: 20,
            bottom: 20,
            left: 45,
        };

        const chartWidth =
            width - margin.left - margin.right;

        const chartHeight =
            height - margin.top - margin.bottom;

        const chartGroup = svg
            .attr('width', width)
            .attr('height', height)
            .append('g')
            .attr(
                'transform',
                `translate(${margin.left},${margin.top})`
            );

        // ===== X SCALE =====

        const x = d3
            .scaleLinear()
            .domain([0, 59]) // fixed 60 seconds window
            .range([0, chartWidth]);

        // ===== Y SCALE =====
        const maxValue = d3.max(data, d => d.value) || 0;

        // Dynamically compute a clean ceiling above the current max
        const yMax = maxValue === 0
            ? 100
            : (() => {
                const rawMax = maxValue * 1.2;
                const step = d3.tickStep(0, rawMax, 5);
                return Math.ceil(rawMax / step) * step;
            })();

        // Y Scale
        const y = d3
            .scaleLinear()
            .domain([0, yMax])
            .nice()
            .range([chartHeight, 0]);

        // Dynamic tick values
        const tickValues = y.ticks(5);

        // ===== GRID =====
        chartGroup
            .selectAll('.grid-line')
            .data(tickValues)
            .enter()
            .append('line')
            .attr('x1', 0)
            .attr('x2', chartWidth)
            .attr('y1', (d) => y(d))
            .attr('y2', (d) => y(d))
            .attr('stroke', '#e5e7eb');

        // ===== Y AXIS =====

        chartGroup
            .append('g')
            .call(
                d3
                    .axisLeft(y)
                    .tickValues(tickValues)
                    .tickSize(0)
            )
            .call((g) => g.select('.domain').remove())
            .call((g) =>
                g
                    .selectAll('text')
                    .style('font-size', '12px')
                    .style('fill', '#9ca3af')
            );

        // ===== X AXIS =====

        const firstIndex = 60 - data.length;
        const lastIndex = 59;
        
        // Only show the first tick if there are enough data points to prevent text overlap
        const showFirstTick = data.length > 15;
        const xTickValues = showFirstTick ? [firstIndex, lastIndex] : [lastIndex];

        chartGroup
            .append('g')
            .attr('transform', `translate(0,${chartHeight})`)
            .call(
                d3
                    .axisBottom(x)
                    .tickValues(xTickValues)
                    .tickFormat((d) => {

                        const rawTime =
                            d === firstIndex && showFirstTick
                                ? data[0].time
                                : d === lastIndex
                                    ? data[data.length - 1].time
                                    : '';

                        if (!rawTime) return '';

                        const date = new Date(`1970-01-01T${rawTime}`);

                        return timeFormat === "12h"
                            ? d3.timeFormat("%-I:%M %p")(date)
                            : d3.timeFormat("%H:%M")(date);
                    })
                    .tickSize(0)
                    .tickPadding(10)
            )
            .call((g) => g.select('.domain').remove())
            .call((g) => {
                g.selectAll('text')
                    .style('font-size', '11px')
                    .style('fill', '#9ca3af');
                
                if (showFirstTick) {
                    g.select('.tick:first-of-type text').attr('text-anchor', 'start');
                }
                g.select('.tick:last-of-type text').attr('text-anchor', 'end');
            });

        // ===== VERTICAL DASHED LINES =====

        // chartGroup
        //     .selectAll('.vertical-line')
        //     .data(data)
        //     .enter()
        //     .append('line')
        //     .attr('x1', (d) => x(d.time) || 0)
        //     .attr('x2', (d) => x(d.time) || 0)
        //     .attr('y1', 0)
        //     .attr('y2', chartHeight)
        //     .attr('stroke', '#9ca3af')
        //     .attr('stroke-dasharray', '4 4');

        // ===== AREA =====

        const area = d3
    .area<IChartData>()
    .x((d, i) => x(60 - data.length + i))
    .y0(chartHeight)
    .y1((d) => y(d.value))
    .curve(d3.curveMonotoneX);

        chartGroup
            .append('path')
            .datum(data)
            .attr('fill', '#DDEEE3')
            .attr('d', area);

        // ===== LINE =====

        const line = d3
            .line<IChartData>()
            .x((d, i) => x(60 - data.length + i))
            .y((d) => y(d.value))
            .curve(d3.curveMonotoneX);

        chartGroup
            .append('path')
            .datum(data)
            .attr('fill', 'none')
            .attr('stroke', '#74C48B')
            .attr('stroke-width', 2)
            .attr('d', line);

        // ===== BOTTOM LINE =====

        chartGroup
            .append('line')
            .attr('x1', 0)
            .attr('x2', chartWidth)
            .attr('y1', chartHeight)
            .attr('y2', chartHeight)
            .attr('stroke', '#9ca3af')
            .attr('stroke-width', 1);

       
           
    }, [data, width, height]);

    return (
        <div className="common-area-chart-card">

            <div className="test"><svg ref={svgRef}></svg></div>

            <div className="common-chart-footer">

                {/* FIRST VALUE */}
                {totalLabel && totalValue && (
                    <div className='common-footer-item'>
                        <div className="common-footer-label">
                            {totalLabel}
                        </div>

                        <div className="common-footer-value">
                            {totalValue}
                        </div>
                    </div>
                )}

                {/* SECOND VALUE */}
                {usedLabel && usedValue && (
                    <div className='common-footer-item'>
                        <div className="common-footer-label">
                            {usedLabel}
                        </div>

                        <div className="common-footer-value">
                            {usedValue}
                        </div>
                    </div>
                )}

            </div>
        </div>
    );
};

export default CommonAreaChart;