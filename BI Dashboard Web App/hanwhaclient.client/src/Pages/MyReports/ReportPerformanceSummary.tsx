import { Box, Typography, List, ListItem, ListItemIcon, ListItemText } from '@mui/material';
import CircleIcon from '@mui/icons-material/Circle';
import { PerformanceComparisonItem } from '../../interfaces/IReport';
import { useTranslation } from 'react-i18next';

interface PerformanceComparisonTableProps {
    performanceReportDetails: PerformanceComparisonItem[],
    comperisionType?: string[] | undefined;
}

const ReportPerformanceSummary:  React.FC<PerformanceComparisonTableProps> = ({performanceReportDetails, comperisionType}) =>{
   const { t } = useTranslation();

    // const generateSummaryInsights = (data: any[]) => {
    //     if (!data || data.length === 0) return ["No data available to generate insights."];

    //     // Filter out "Average" row and rows with zero values for comparisons
    //     const filteredData = data.filter(site =>
    //         (site?.siteZoneName && site?.siteZoneName?.toLowerCase() !== "average") &&
    //         site?.vehicleOccupancy > 0 &&
    //         site?.peopleOccupancy > 0 &&
    //         site?.vehicleCount > 0 &&
    //         site?.peopleCount > 0
    //     );

    //     if (filteredData.length === 0) return ["No valid data available to generate insights."];

    //     const insights: string[] = [];

    //     const maxVehicleOccupancySite = filteredData.reduce((max, site) =>
    //         site.vehicleOccupancy > max.vehicleOccupancy ? site : max
    //     );

    //     const maxPeopleOccupancySite = filteredData.reduce((max, site) =>
    //         site.peopleOccupancy > max.peopleOccupancy ? site : max
    //     );

    //     const maxVehicleCountSite = filteredData.reduce((max, site) =>
    //         site.vehicleCount > max.vehicleCount ? site : max
    //     );

    //     const maxPeopleCountSite = filteredData.reduce((max, site) =>
    //         site.peopleCount > max.peopleCount ? site : max
    //     );

    //     // Insight 1 – Highest occupancy rate and vehicle count
    //     if (
    //         maxVehicleOccupancySite.siteZoneName === maxPeopleOccupancySite.siteZoneName &&
    //         maxVehicleOccupancySite.vehicleOccupancy === maxPeopleOccupancySite.peopleOccupancy
    //     ) {
    //         insights.push(
    //             `${maxPeopleOccupancySite.siteZoneName} has the highest people occupancy rate (${maxPeopleOccupancySite.peopleOccupancy.toFixed(2)}%) and vehicle occupancy rate.`
    //         );
    //     } else if (
    //         maxVehicleOccupancySite.siteZoneName === maxPeopleOccupancySite.siteZoneName
    //     ) {
    //         insights.push(
    //             `${maxPeopleOccupancySite.siteZoneName} has the highest people occupancy rate (${maxPeopleOccupancySite.peopleOccupancy.toFixed(2)}%) and vehicle occupancy rate (${maxVehicleOccupancySite.vehicleOccupancy.toFixed(2)}%).`
    //         );
    //     } else {
    //         insights.push(
    //             `${maxVehicleOccupancySite.siteZoneName} has the highest vehicle occupancy rate (${maxVehicleOccupancySite.vehicleOccupancy.toFixed(2)}%).`
    //         );
    //         insights.push(
    //             `${maxPeopleOccupancySite.siteZoneName} has the highest people occupancy rate (${maxPeopleOccupancySite.peopleOccupancy.toFixed(2)}%).`
    //         );
    //     }

    //     // Insight 2 – Highest number of people counted
    //     insights.push(
    //         `${maxPeopleCountSite.siteZoneName} has the highest number of people counted (${maxPeopleCountSite.peopleCount.toLocaleString()}).`
    //     );

    //     // Insight 3 – Compare average or other occupancy insights
    //     const avgVehicleOccupancy =
    //         filteredData.reduce((sum, s) => sum + s.vehicleOccupancy, 0) / filteredData.length;

    //     const busierSites = filteredData.filter(site => site.vehicleOccupancy > avgVehicleOccupancy);
    //     const lessBusySites = filteredData.filter(site => site.vehicleOccupancy <= avgVehicleOccupancy);

    //     if (busierSites.length && lessBusySites.length) {
    //         insights.push(
    //             `Occupancy levels suggest ${busierSites.map(s => s.siteZoneName).join(" and ")} are busier than ${lessBusySites.map(s => s.siteZoneName).join(" and ")}.`
    //         );
    //     }

    //     return insights;
    // }

    // const insights = generateSummaryInsights(performanceReportDetails);

    // return (
    //     <Box className="summery-report">
    //         <Typography variant="h6">
    //             Summary Insights
    //         </Typography>
    //         <Box
    //            className="reports-repeated-insights-box"
    //         >
    //         <List dense>
    //             {insights.map((text, index) => (
    //                 <ListItem key={index}>
    //                     <ListItemIcon sx={{ minWidth: 24 }}>
    //                         <CircleIcon fontSize="small" sx={{ color: '#f97316' }} />
    //                     </ListItemIcon>
    //                     <ListItemText primary={text} primaryTypographyProps={{ fontSize: 14 }} />
    //                 </ListItem>
    //             ))}
    //         </List>
    //     </Box>
    // </Box>
        
    // );

    //  const generateSummaryInsights = (data: PerformanceComparisonItem[]) => {
    //     const types = comperisionType || ["vehicle", "people"]; // default to both if nothing selected
    //     if (!data || data.length === 0) return [t("My_Report.No_Insights_Data")];

    //     // const filteredData = data.filter(site =>
    //     //     (site?.siteZoneName && site?.siteZoneName.toLowerCase() !== "average") &&
    //     //     site?.vehicleOccupancy > 0 &&
    //     //     site?.peopleOccupancy > 0 &&
    //     //     site?.vehicleCount > 0 &&
    //     //     site?.peopleCount > 0
    //     // );

    //     // const filteredData = data.filter((site) => {
            
    //     //   // must have zone name and not "average"
    //     //   if (
    //     //     !site?.siteZoneName ||
    //     //     site?.siteZoneName.toLowerCase() === "average"
    //     //   ) {
    //     //     return false;
    //     //   }

    //     //   // conditions
    //     //   const checkVehicle =
    //     //     types.includes("vehicle") &&
    //     //     site?.vehicleOccupancy > 0 &&
    //     //     site?.vehicleCount > 0;

    //     //   const checkPeople =
    //     //     types.includes("people") &&
    //     //     site?.peopleOccupancy > 0 &&
    //     //     site?.peopleCount > 0;

    //     //   // If both types included, require both conditions
    //     //   if (types.includes("vehicle") && types.includes("people")) {
    //     //     return checkVehicle && checkPeople;
    //     //   }

    //     //   if (types.includes("vehicle")) {
    //     //     return checkVehicle;
    //     //   }
    //     //   if (types.includes("people")) {
    //     //     return checkPeople;
    //     //   }
    //     //   // If only one type included, allow either
    //     //   //return types.includes("vehicle") ? checkVehicle : checkPeople;
    //     // });
    //      const filteredData = data.filter((site) => {
    //          // must have zone name and not "average"
    //          if (!site?.siteZoneName || site.siteZoneName.toLowerCase() === "average") {
    //              return false;
    //          }

    //          // values (safe)
    //          const peopleCount = site?.peopleCount ?? 0;
    //          const peopleOcc = site?.peopleOccupancy ?? 0;
    //          const vehicleCount = site?.vehicleCount ?? 0;
    //          const vehicleOcc = site?.vehicleOccupancy ?? 0;

    //          // ignore row if all 4 are 0
    //          const allZero =
    //              peopleCount <= 1 &&
    //              peopleOcc <= 1 &&
    //              vehicleCount <= 1 &&
    //              vehicleOcc <= 1;

    //          if (allZero) return false;

    //          // category checks
    //          const checkVehicle =
    //              types.includes("vehicle") && vehicleCount >= 1 && vehicleOcc >= 1;

    //          const checkPeople =
    //              types.includes("people") && peopleCount >= 1 && peopleOcc >= 1;

    //          // If both selected → allow row if either is valid
    //          if (types.includes("vehicle") && types.includes("people")) {
    //              return checkVehicle || checkPeople;
    //          }

    //          // If only vehicle selected
    //          if (types.includes("vehicle")) {
    //              return checkVehicle;
    //          }

    //          // If only people selected
    //          if (types.includes("people")) {
    //              return checkPeople;
    //          }

    //          return false;
    //      });

    //     if (filteredData.length === 0) return [t("My_Report.No_Valid_Insights")];

    //     const insights: string[] = [];
        
    //     // Generate vehicle-related insights if selected
    //     if (types.includes("vehicle")) {
           
    //         const maxVehicleOccupancySite = filteredData.reduce((max, site) =>
               
    //          (site.vehicleOccupancy > max.vehicleOccupancy) ? site : max
    //         );

    //         const maxVehicleCountSite = filteredData.reduce((max, site) =>
    //           (site.vehicleCount > max.vehicleCount) ? site : max
    //         );

    //         // insights.push(
    //         //     `${maxVehicleOccupancySite.siteZoneName} has the highest vehicle occupancy rate (${maxVehicleOccupancySite.vehicleOccupancy.toFixed(2)}%).`
    //         // );
    //         insights.push(
    //             t("My_Report.HighestVehicleOccupancy", {
    //                 zone: maxVehicleOccupancySite?.siteZoneName,
    //                 occupancy: maxVehicleOccupancySite?.vehicleOccupancy.toFixed(2)
    //             })
    //         );

    //         // insights.push(
    //         //     `${maxVehicleCountSite.siteZoneName} has the highest number of vehicles counted (${maxVehicleCountSite.vehicleCount.toLocaleString()}).`
    //         // );

    //         insights.push(
    //             t("My_Report.HighestVehicleCount", {
    //                 zone: maxVehicleCountSite?.siteZoneName,
    //                 count: maxVehicleCountSite?.vehicleCount
    //             })
    //         );

    //         const avgVehicleOccupancy =
    //             filteredData.reduce((sum, s) => sum + s.vehicleOccupancy, 0) / filteredData.length;

    //         const busierSites = filteredData.filter(site => site.vehicleOccupancy > avgVehicleOccupancy);
    //         const lessBusySites = filteredData.filter(site => site.vehicleOccupancy <= avgVehicleOccupancy);

    //         if (busierSites.length && lessBusySites.length) {
    //             // insights.push(
    //             //     `Vehicle occupancy levels suggest ${busierSites.map(s => s.siteZoneName).join(", ")} are busier than ${lessBusySites.map(s => s.siteZoneName).join(", ")}.`
    //             // );
    //             insights.push(
    //                 t("My_Report.VehicleOccupancyComparison", {
    //                     busier: busierSites.map(s => s.siteZoneName).join(", "),
    //                     lessBusy: lessBusySites.map(s => s.siteZoneName).join(", ")
    //                 })
    //             );

    //         }
    //     }

        
    //     // Generate people-related insights if selected
    //     if (types.includes("people")) {
    //         const maxPeopleOccupancySite = filteredData.reduce((max, site) =>
    //             site.peopleOccupancy > max.peopleOccupancy ? site : max
    //         );

    //         const maxPeopleCountSite = filteredData.reduce((max, site) =>
    //             site.peopleCount > max.peopleCount ? site : max
    //         );

    //         // insights.push(
    //         //     `${maxPeopleOccupancySite.siteZoneName} has the highest people occupancy rate (${maxPeopleOccupancySite.peopleOccupancy.toFixed(2)}%).`
    //         // );

    //         insights.push(
    //             t("My_Report.HighestPeopleOccupancy", {
    //                 zone: maxPeopleOccupancySite.siteZoneName,
    //                 rate: maxPeopleOccupancySite.peopleOccupancy.toFixed(2)
    //             })
    //         );

    //         // insights.push(
    //         //     `${maxPeopleCountSite.siteZoneName} has the highest number of people counted (${maxPeopleCountSite.peopleCount.toLocaleString()}).`
    //         // );

    //         insights.push(
    //             t("My_Report.HighestPeopleCount", {
    //                 zone: maxPeopleCountSite.siteZoneName,
    //                 count: maxPeopleCountSite.peopleCount
    //             })
    //         );


    //         const avgPeopleOccupancy =
    //             filteredData.reduce((sum, s) => sum + s.peopleOccupancy, 0) / filteredData.length;

    //         const busierSites = filteredData.filter(site => site.peopleOccupancy > avgPeopleOccupancy);
    //         const lessBusySites = filteredData.filter(site => site.peopleOccupancy <= avgPeopleOccupancy);

    //         if (busierSites.length && lessBusySites.length) {
    //             // insights.push(
    //             //     `People occupancy levels suggest ${busierSites.map(s => s.siteZoneName).join(", ")} are busier than ${lessBusySites.map(s => s.siteZoneName).join(", ")}.`
    //             // );
    //             const busierNames = busierSites.map(s => s.siteZoneName).join(", ");
    //             const lessBusyNames = lessBusySites.map(s => s.siteZoneName).join(", ");

    //             insights.push(
    //                 t("My_Report.PeopleOccupancyComparison", {
    //                     busier: busierNames,
    //                     lessBusy: lessBusyNames
    //                 })
    //             );

    //         }
    //     }

    //     // Generate combined insights if both are selected
    //     if (types.includes("vehicle") && types.includes("people")) {
    //         const maxVehicleOccupancySite = filteredData.reduce((max, site) =>
    //             site.vehicleOccupancy > max.vehicleOccupancy ? site : max
    //         );

    //         const maxPeopleOccupancySite = filteredData.reduce((max, site) =>
    //             site.peopleOccupancy > max.peopleOccupancy ? site : max
    //         );

    //         if (
    //             maxVehicleOccupancySite.siteZoneName === maxPeopleOccupancySite.siteZoneName &&
    //             maxVehicleOccupancySite.vehicleOccupancy === maxPeopleOccupancySite.peopleOccupancy
    //         ) {
    //             // insights.push(
    //             //     `${maxPeopleOccupancySite.siteZoneName} has the highest people and vehicle occupancy rates (${maxPeopleOccupancySite.peopleOccupancy.toFixed(2)}%).`
    //             // );
    //             insights.push(
    //                 t("My_Report.HighestPeopleAndVehicleOccupancy", {
    //                     zone: maxPeopleOccupancySite.siteZoneName,
    //                     rate: maxPeopleOccupancySite.peopleOccupancy.toFixed(2)
    //                 })
    //             );

    //         } else if (
    //             maxVehicleOccupancySite.siteZoneName === maxPeopleOccupancySite.siteZoneName
    //         ) {
    //             // insights.push(
    //             //     `${maxPeopleOccupancySite.siteZoneName} has the highest people occupancy rate (${maxPeopleOccupancySite.peopleOccupancy.toFixed(2)}%) and vehicle occupancy rate (${maxVehicleOccupancySite.vehicleOccupancy.toFixed(2)}%).`
    //             // );
    //             insights.push(
    //                 t("My_Report.HighestPeopleAndVehicleOccupancyRate", {
    //                     zone: maxPeopleOccupancySite.siteZoneName,
    //                     peopleRate: maxPeopleOccupancySite.peopleOccupancy.toFixed(2),
    //                     vehicleRate: maxVehicleOccupancySite.vehicleOccupancy.toFixed(2)
    //                 })
    //             );

    //         }
    //     }

    //     if (insights.length === 0) {
    //         return [t("My_Report.No_Insights_Available")];
    //     }

    //     return insights;
    // }
    const generateSummaryInsights = (data: PerformanceComparisonItem[]) => {
        const types = comperisionType || ["vehicle", "people"]; // default to both
        if (!data || data.length === 0) return [t("My_Report.No_Insights_Data")];

        // -----------------------------
        // Step 1: Filter valid rows
        // -----------------------------
        const filteredData = data.filter((site) => {
            // must have zone name and not "average"
            if (!site?.siteZoneName || site.siteZoneName.toLowerCase() === "average") {
                return false;
            }

            // safe values
            const peopleCount = site?.peopleCount ?? 0;
            const peopleOcc = site?.peopleOccupancy ?? 0;
            const vehicleCount = site?.vehicleCount ?? 0;
            const vehicleOcc = site?.vehicleOccupancy ?? 0;

            // ignore row if all 4 are 0
            const allZero =
                peopleCount <= 0 &&
                peopleOcc <= 0 &&
                vehicleCount <= 0 &&
                vehicleOcc <= 0;

            if (allZero) return false;

            // category checks
            const checkVehicle =
                types.includes("vehicle") && vehicleCount > 0 && vehicleOcc > 0;

            const checkPeople =
                types.includes("people") && peopleCount > 0 && peopleOcc > 0;

            // If both selected → allow row if either is valid
            if (types.includes("vehicle") && types.includes("people")) {
                return checkVehicle || checkPeople;
            }

            // If only vehicle selected
            if (types.includes("vehicle")) return checkVehicle;

            // If only people selected
            if (types.includes("people")) return checkPeople;

            return false;
        });

        if (filteredData.length === 0) return [t("My_Report.No_Valid_Insights")];

        const insights: string[] = [];

        // =========================================================
        // VEHICLE INSIGHTS (ignore 0 values in reduce)
        // =========================================================
        if (types.includes("vehicle")) {
            const vehicleOccData = filteredData.filter(
                (s) => (s.vehicleOccupancy ?? 0) > 0
            );

            const vehicleCountData = filteredData.filter(
                (s) => (s.vehicleCount ?? 0) > 0
            );

            // Highest vehicle occupancy
            if (vehicleOccData.length > 0) {
                const maxVehicleOccupancySite = vehicleOccData.reduce((max, site) =>
                    site.vehicleOccupancy > max.vehicleOccupancy ? site : max
                );

                insights.push(
                    t("My_Report.HighestVehicleOccupancy", {
                        zone: maxVehicleOccupancySite.siteZoneName,
                        occupancy: maxVehicleOccupancySite.vehicleOccupancy.toFixed(2),
                    })
                );

                // Avg comparison only from >0 occupancy
                const avgVehicleOccupancy =
                    vehicleOccData.reduce((sum, s) => sum + s.vehicleOccupancy, 0) /
                    vehicleOccData.length;

                const busierSites = vehicleOccData.filter(
                    (site) => site.vehicleOccupancy > avgVehicleOccupancy
                );

                const lessBusySites = vehicleOccData.filter(
                    (site) => site.vehicleOccupancy <= avgVehicleOccupancy
                );

                if (busierSites.length && lessBusySites.length) {
                    insights.push(
                        t("My_Report.VehicleOccupancyComparison", {
                            busier: busierSites.map((s) => s.siteZoneName).join(", "),
                            lessBusy: lessBusySites.map((s) => s.siteZoneName).join(", "),
                        })
                    );
                }
            }

            // Highest vehicle count
            if (vehicleCountData.length > 0) {
                const maxVehicleCountSite = vehicleCountData.reduce((max, site) =>
                    site.vehicleCount > max.vehicleCount ? site : max
                );

                insights.push(
                    t("My_Report.HighestVehicleCount", {
                        zone: maxVehicleCountSite.siteZoneName,
                        count: maxVehicleCountSite.vehicleCount,
                    })
                );
            }
        }

        // =========================================================
        // PEOPLE INSIGHTS (ignore 0 values in reduce)
        // =========================================================
        if (types.includes("people")) {
            const peopleOccData = filteredData.filter(
                (s) => (s.peopleOccupancy ?? 0) > 0
            );

            const peopleCountData = filteredData.filter(
                (s) => (s.peopleCount ?? 0) > 0
            );

            // Highest people occupancy
            if (peopleOccData.length > 0) {
                const maxPeopleOccupancySite = peopleOccData.reduce((max, site) =>
                    site.peopleOccupancy > max.peopleOccupancy ? site : max
                );

                insights.push(
                    t("My_Report.HighestPeopleOccupancy", {
                        zone: maxPeopleOccupancySite.siteZoneName,
                        rate: maxPeopleOccupancySite.peopleOccupancy.toFixed(2),
                    })
                );

                // Avg comparison only from >0 occupancy
                const avgPeopleOccupancy =
                    peopleOccData.reduce((sum, s) => sum + s.peopleOccupancy, 0) /
                    peopleOccData.length;

                const busierSites = peopleOccData.filter(
                    (site) => site.peopleOccupancy > avgPeopleOccupancy
                );

                const lessBusySites = peopleOccData.filter(
                    (site) => site.peopleOccupancy <= avgPeopleOccupancy
                );

                if (busierSites.length && lessBusySites.length) {
                    insights.push(
                        t("My_Report.PeopleOccupancyComparison", {
                            busier: busierSites.map((s) => s.siteZoneName).join(", "),
                            lessBusy: lessBusySites.map((s) => s.siteZoneName).join(", "),
                        })
                    );
                }
            }

            // Highest people count
            if (peopleCountData.length > 0) {
                const maxPeopleCountSite = peopleCountData.reduce((max, site) =>
                    site.peopleCount > max.peopleCount ? site : max
                );

                insights.push(
                    t("My_Report.HighestPeopleCount", {
                        zone: maxPeopleCountSite.siteZoneName,
                        count: maxPeopleCountSite.peopleCount,
                    })
                );
            }
        }

        // =========================================================
        // COMBINED INSIGHTS (vehicle + people) ignoring 0
        // =========================================================
        if (types.includes("vehicle") && types.includes("people")) {
            const vehicleOccData = filteredData.filter(
                (s) => (s.vehicleOccupancy ?? 0) > 0
            );

            const peopleOccData = filteredData.filter(
                (s) => (s.peopleOccupancy ?? 0) > 0
            );

            if (vehicleOccData.length > 0 && peopleOccData.length > 0) {
                const maxVehicleOccupancySite = vehicleOccData.reduce((max, site) =>
                    site.vehicleOccupancy > max.vehicleOccupancy ? site : max
                );

                const maxPeopleOccupancySite = peopleOccData.reduce((max, site) =>
                    site.peopleOccupancy > max.peopleOccupancy ? site : max
                );

                // same zone has both highest
                if (
                    maxVehicleOccupancySite.siteZoneName ===
                    maxPeopleOccupancySite.siteZoneName
                ) {
                    insights.push(
                        t("My_Report.HighestPeopleAndVehicleOccupancyRate", {
                            zone: maxPeopleOccupancySite.siteZoneName,
                            peopleRate: maxPeopleOccupancySite.peopleOccupancy.toFixed(2),
                            vehicleRate: maxVehicleOccupancySite.vehicleOccupancy.toFixed(2),
                        })
                    );
                }
            }
        }

        if (insights.length === 0) {
            return [t("My_Report.No_Insights_Available")];
        }

        return insights;
    };

    const insights = generateSummaryInsights(performanceReportDetails);

    return (
        <Box className="summery-report">
            <Typography variant="h6">                                                     
                {t("My_Report.Summary_Insights")}  
            </Typography>
            <Box className="reports-repeated-insights-box">
                <List dense>
                    {insights.map((text, index) => (
                        <ListItem key={index}>
                            <ListItemIcon sx={{ minWidth: 24 }}>
                                <CircleIcon fontSize="small" sx={{ color: '#f97316' }} />
                            </ListItemIcon>
                            <ListItemText primary={text} primaryTypographyProps={{ fontSize: 14 }} />
                        </ListItem>
                    ))}
                </List>
            </Box>
        </Box>
    );
}

export { ReportPerformanceSummary };