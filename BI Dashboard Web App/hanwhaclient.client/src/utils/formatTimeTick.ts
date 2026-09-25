import * as d3 from "d3";
const formatHour12 = (date: Date) => {    
  const hours = d3.timeFormat("%I")(date); // hour in 12h (1-12)
  const minutes = +d3.timeFormat("%M")(date); // minutes (0-59)
  const period = d3.timeFormat("%p")(date); // AM/PM
 
  if (minutes === 0) {
    return `${hours} ${period}`; // e.g. 1 PM
  } else {
    return `${hours}:${d3.timeFormat("%M")(date)}`; // e.g. 1:30 PM
  }
};

export const getTimeTickFormatter = (
    d: Date | d3.NumberValue,
    i: number,
    xTicks: Date[],
    timeFormatType: "12h" | "24h"   // 👈 your tuple instead of boolean
): string => {
    const date = d instanceof Date ? d : new Date(+d);
    const hours = date.getHours();
    const minutes = date.getMinutes();
    // D3 formatters
    const formatYear = d3.timeFormat("%Y");          // 2025
    const formatMonth = d3.timeFormat("%B");         // November
    const formatDay = d3.timeFormat("%a %d");        // Thu 13
    const formatMonthDate = d3.timeFormat("%b %d");  // Nov 09
   // const formatHour12 = d3.timeFormat("%I:%M %p");  // 01:00 PM
    const formatHour24 = d3.timeFormat("%H:%M");     // 13:00 

    // Compute duration in days
    const startDate = xTicks[0];
    const endDate = xTicks[xTicks.length - 1];

    const totalDays =
        (endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24);

    const isMultiYear = totalDays > 365;
    const isMultiMonth = totalDays > 31 && totalDays <= 365;
    const isMultiDay = totalDays > 1 && totalDays <= 31;
    const isSingleDay = totalDays <= 1;

    // Previous tick date
    const prev = xTicks[i - 1];
    const prevDate = prev instanceof Date ? prev : prev ? new Date(+prev) : undefined;

    const isFirstTickOfDay =
        i === 0 ||
        !prevDate ||
        prevDate.getDate() !== date.getDate() ||
        prevDate.getMonth() !== date.getMonth() ||
        prevDate.getFullYear() !== date.getFullYear();
    
    const isSunday = date.getDay() === 0;

    //  Check if selected range is today's date
    const today = new Date();
    const isTodayRange =
        startDate.toDateString() === today.toDateString() &&
        endDate.toDateString() === today.toDateString();

    //  If today selected → first tick shows weekday name + date (e.g., "Thu 13")
    if (isTodayRange && hours === 0 && minutes === 0) {
        return d3.timeFormat("%a %d")(date); // e.g. Thu 13
    }

    if (isMultiYear) {
        if (date.getMonth() === 0 && (!prevDate || prevDate.getFullYear() !== date.getFullYear())) {
            return formatYear(date); // "2025"
        }
        return "";
    }

    if (isMultiMonth) {
        if (i === 0 || (prevDate && prevDate.getMonth() !== date.getMonth())) {
            if (
                date.getMonth() === 0 &&
                (!prevDate || prevDate.getFullYear() !== date.getFullYear())
            ) {
                return formatYear(date); // "2025" in January
            }
            return formatMonth(date); // "November", "December"
        }
        return "";
    }

    if (isMultiDay && isFirstTickOfDay) {
        return isSunday ? formatMonthDate(date) : formatDay(date);
    }

    if (isSingleDay) {
        // Show "Nov 09" only for first tick (00:00)
        if (date.getHours() === 0 && date.getMinutes() === 0) {
            return formatMonthDate(date);
        }
        // Show time based on format type
        return timeFormatType === "24h" ? formatHour24(date) : formatHour12(date);
    }
    

    return timeFormatType === "24h" ? formatHour24(date) : formatHour12(date);

};

 
 