import React, { useEffect, useState } from "react";
import { Box, Typography } from "@mui/material";
import { AgeProps, IAgeTotals ,IMaxData} from "../../../../interfaces/IChart";
import moment from "moment";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";


const Age1_1: React.FC<AgeProps> = ({
  customizedWidth,
  displayName,
  onZoomClick,
  openZoomDialog,
  setIsDraggable,
  groupbyDateAgeData,
}) => {
  const [totalCount, setTotalCount] = useState<number>(0);
  const { theme } = useThemeContext();
  const [maxYoung, setMaxYoung] = useState<IMaxData | null>(null);
  const [maxAdult, setMaxAdult] = useState<IMaxData | null>(null);
  const [maxSenior, setMaxSenior] = useState<IMaxData | null>(null);
  const [maxUnknown, setMaxUnknown] = useState<IMaxData | null>(null);

  const [minYoung, setMinYoung] = useState<IMaxData | null>(null);
  const [minAdult, setMinAdult] = useState<IMaxData | null>(null);
  const [minSenior, setMinSenior] = useState<IMaxData | null>(null);
  const [minUnknown, setMinUnknown] = useState<IMaxData | null>(null);

  const [totals, setTotals] = useState<IAgeTotals>({
    totalYoung: 0,
    totalAdult: 0,
    totalSenior: 0,
    totalUnknown: 0,
  });

  useEffect(() => {
    if (!groupbyDateAgeData) return;
    if (!groupbyDateAgeData.length) return;
    const maxYoung = groupbyDateAgeData.reduce(
      (max, curr) => (curr.youngCount > max.youngCount ? curr : max),
      groupbyDateAgeData[0]
    );

    const maxAdult = groupbyDateAgeData.reduce(
      (max, curr) => (curr.adultCount > max.adultCount ? curr : max),
      groupbyDateAgeData[0]
    );

    const maxSenior = groupbyDateAgeData.reduce(
      (max, curr) => (curr.seniorCount > max.seniorCount ? curr : max),
      groupbyDateAgeData[0]
    );

    const maxUnknown = groupbyDateAgeData.reduce(
      (max, curr) => (curr.unknownCount > max.unknownCount ? curr : max),
      groupbyDateAgeData[0]
    );

    setMaxYoung({ date: maxYoung.date, count: maxYoung.youngCount });
    setMaxAdult({ date: maxAdult.date, count: maxAdult.adultCount });
    setMaxSenior({
      date: maxSenior.date,
      count: maxSenior.seniorCount,
    });
    setMaxUnknown({ date: maxUnknown.date, count: maxUnknown.unknownCount });

    // ✅ Min values
    const minYoung = groupbyDateAgeData.reduce(
      (min, curr) => (curr.youngCount < min.youngCount ? curr : min),
      groupbyDateAgeData[0]
    );

    const minAdult = groupbyDateAgeData.reduce(
      (min, curr) => (curr.adultCount < min.adultCount ? curr : min),
      groupbyDateAgeData[0]
    );

    const minSenior = groupbyDateAgeData.reduce(
      (min, curr) => (curr.seniorCount < min.seniorCount ? curr : min),
      groupbyDateAgeData[0]
    );

    const minUnknown = groupbyDateAgeData.reduce(
      (min, curr) => (curr.unknownCount < min.unknownCount ? curr : min),
      groupbyDateAgeData[0]
    );

    setMinYoung({ date: minYoung.date, count: minYoung.youngCount });
    setMinAdult({ date: minAdult.date, count: minAdult.adultCount });
    setMinSenior({
      date: minSenior.date,
      count: minSenior.seniorCount,
    });
    setMinUnknown({ date: minUnknown.date, count: minUnknown.unknownCount });

    const totalsResult: IAgeTotals = groupbyDateAgeData.reduce<IAgeTotals>(
      (acc, curr) => {
        acc.totalYoung += curr.youngCount;
        acc.totalAdult += curr.adultCount;
        acc.totalSenior += curr.seniorCount;
        acc.totalUnknown += curr.unknownCount;
        return acc;
      },
      { totalYoung: 0, totalAdult: 0, totalSenior: 0, totalUnknown: 0 }
    );

    setTotals(totalsResult as IAgeTotals);
    const grandTotal =
      totalsResult.totalYoung +
      totalsResult.totalAdult +
      totalsResult.totalSenior +
      totalsResult.totalUnknown;
    setTotalCount(grandTotal);
  }, [groupbyDateAgeData]);

  return (
    <Box sx={{ width: customizedWidth }}>
      <Box className="widget-main-wrapper age-widget-only">
        <Box className="widget-main-header">
          <Typography variant="h6" component="h2">
            {displayName}
          </Typography>
        </Box>

        <Box className="widget-main-body">
          <div className="widget-data-wrapper">
            <Box className="gender-data-repeat age-purple-bac">
              <Box className="gender-data-repeated-inner">
                <Box className="gender-data-image">
                  <img src={"/images/dashboard/Young.svg"} alt="Young" />
                </Box>

                <Box className="gender-data-column-data">
                  <Typography>Young</Typography>
                  <Typography>
                    {formatNumber(totals.totalYoung ?? 0)}
                  </Typography>
                </Box>
              </Box>
              <Box className="gender-data-repeated-inner">
                <Typography>
                  Least: <span>{formatNumber(minYoung?.count ?? 0)}</span>
                </Typography>
                <Typography>
                  {minYoung?.date
                    ? moment(minYoung.date).format("DD-MM-YYYY")
                    : " "}
                </Typography>
              </Box>

              <Box className="gender-data-repeated-inner">
                <Typography>
                  Most: <span>{formatNumber(maxYoung?.count ?? 0)}</span>
                </Typography>
                <Typography>
                  {maxYoung?.date
                    ? moment(maxYoung?.date).format("DD-MM-YYYY")
                    : " "}
                </Typography>
              </Box>
            </Box>
            <Box className="gender-data-repeat age-green-bac ">
              <Box className="gender-data-repeated-inner">
                <Box className="gender-data-image">
                  <img src={"/images/dashboard/Adult.svg"} alt="Adult" />
                </Box>
                <Box className="gender-data-column-data">
                  <Typography>Adult</Typography>
                  <Typography>
                    {formatNumber(totals.totalAdult ?? 0)}
                  </Typography>
                </Box>
              </Box>
              <Box className="gender-data-repeated-inner">
                <Typography>
                  Least: <span>{formatNumber(minAdult?.count ?? 0)}</span>
                </Typography>
                <Typography>
                  {minAdult?.date
                    ? moment(minAdult.date).format("DD-MM-YYYY")
                    : " "}
                </Typography>
              </Box>
              <Box className="gender-data-repeated-inner">
                <Typography>
                  Most: <span>{formatNumber(maxAdult?.count ?? 0)}</span>
                </Typography>
                <Typography>
                  {maxAdult?.date
                    ? moment(maxAdult.date).format("DD-MM-YYYY")
                    : " "}
                </Typography>
              </Box>
            </Box>
            <Box className="gender-data-repeat age-gray-bac">
              <Box className="gender-data-repeated-inner">
                <Box className="gender-data-image">
                  <img
                    src={"/images/dashboard/Senior.svg"}
                    alt="Senior"
                  />
                </Box>
                <Box className="gender-data-column-data">
                  <Typography>Senior</Typography>
                  <Typography>
                    {formatNumber(totals.totalSenior ?? 0)}
                  </Typography>
                </Box>
              </Box>

              <Box className="gender-data-repeated-inner">
                <Typography>
                  Least: <span>{formatNumber(minSenior?.count ?? 0)}</span>
                </Typography>
                <Typography>
                  {minSenior?.date
                    ? moment(minSenior.date).format("DD-MM-YYYY")
                    : " "}
                </Typography>
              </Box>

              <Box className="gender-data-repeated-inner">
                <Typography>
                  Most:
                  <span> {formatNumber(maxSenior?.count ?? 0)}</span>
                </Typography>
                <Typography>
                  {maxSenior?.date
                    ? moment(maxSenior.date).format("DD-MM-YYYY")
                    : " "}
                </Typography>
              </Box>
            </Box>

            <Box className="gender-data-repeat age-orange-bac">
              <Box className="gender-data-repeated-inner">
                <Box className="gender-data-image">
                  <img
                    src={"/images/dashboard/Unknown.svg"}
                    alt="Unknown"
                  />
                </Box>
                <Box className="gender-data-column-data">
                  <Typography>Unknown</Typography>
                  <Typography>
                    {formatNumber(totals.totalUnknown ?? 0)}
                  </Typography>
                </Box>
              </Box>

              <Box className="gender-data-repeated-inner">
                <Typography>
                  Least: <span>{formatNumber(minUnknown?.count ?? 0)}</span>
                </Typography>
                <Typography>
                  {minUnknown?.date
                    ? moment(minUnknown.date).format("DD-MM-YYYY")
                    : " "}
                </Typography>
              </Box>

              <Box className="gender-data-repeated-inner">
                <Typography>
                  Most:
                  <span> {formatNumber(maxUnknown?.count ?? 0)}</span>
                </Typography>
                <Typography>
                  {maxUnknown?.date
                    ? moment(maxUnknown.date).format("DD-MM-YYYY")
                    : " "}
                </Typography>
              </Box>
            </Box>
          </div>
        </Box>

        <Box className="widget-main-footer">
          <Box className="widget-main-footer-value">
            <Typography>Total No. of People : </Typography>
            <span>{formatNumber(totalCount)}</span>
          </Box>
          {!openZoomDialog ? (
            <Box
              className="widget-main-footer-zoom-i"
              onMouseEnter={() => setIsDraggable?.(true)}
              onMouseLeave={() => setIsDraggable?.(false)}
            >
              <img
                src={
                  theme === "light"
                    ? "/images/dashboard/drag.svg"
                    : "/images/dark-theme/dashboard/drag.svg"
                }
                alt="vehicle"
                width={35}
                height={35}
              />
            </Box>
          ) : null}

          {!openZoomDialog ? (
            <Box
              className="widget-main-footer-zoom-i"
              onClick={onZoomClick}
              id="zoomwidgetBtnGender"
            >
              <img
                src={
                  theme === "light"
                    ? "/images/dashboard/ZoomWidget.svg"
                    : "/images/dark-theme/dashboard/ZoomWidget.svg"
                }
                alt="vehicle"
                width={35}
                height={35}
              />
            </Box>
          ) : null}
        </Box>
      </Box>
    </Box>
  );
};

export { Age1_1 };
