import React, { useEffect, useState } from 'react';
import { Box, Typography, Grid, Paper } from '@mui/material';
import { ReportHeader } from '../../interfaces/IReport';
import { formatDateToConfiguredTimezone } from '../../utils/formatDateToConfiguredTimezone';
import moment from 'moment';
import { useTimeFormatContext } from '../../context/TimeFormatContext';
import { t } from 'i18next';

type ReportTypeInfoProps = {
  reportHeader?: ReportHeader
};

const ReportTypeInfo: React.FC<ReportTypeInfoProps> = ({ reportHeader }) => {
   const [formatedStartDate, setFormatedStartDate] = useState<string>('');
  const [formatedEndDate, setFormatedEndDate] = useState<string>('');
  const { timeFormat } = useTimeFormatContext();

  

  useEffect(() => {
    const timeFormatStr = timeFormat === "12h" ? "MMM DD YYYY, hh:mm A" : "MMM DD YYYY, HH:mm";
    if (reportHeader?.reportStartDate && reportHeader?.reportEndDate) {
      const convertedStartDate = formatDateToConfiguredTimezone(reportHeader.reportStartDate);
      const convertedEndDate = formatDateToConfiguredTimezone(reportHeader.reportEndDate);

      setFormatedStartDate(moment(convertedStartDate).format(timeFormatStr));
      setFormatedEndDate(moment(convertedEndDate).format(timeFormatStr));
    }
  },[reportHeader, timeFormat]);
  return (
    <Box className="report-box">
       <Paper className="report-type-wrapper">
        <Grid className="report-type-wrapper-repeat">
          <Grid item >
            <Typography>{t("My_Report.Report_Type")}</Typography>
            <span>{reportHeader?.reportType == "site report" ? t("My_Report.Site_Report_Header") : t("My_Report.Zone_Report_Header")}</span>
          </Grid>
          <Grid item>
            <Typography>{t("My_Report.Report_Name")}</Typography>
            <span>{reportHeader?.reportName}</span>
          </Grid>
          <Grid item>
            <Typography>{t("My_Report.Sites")}</Typography>
            <span>{reportHeader?.sites}</span>
          </Grid>
          {
            (reportHeader?.reportType && reportHeader?.reportType?.toLowerCase() == "zone report") && (
              <>
                <Grid item>
                  <Typography>{t("My_Report.Floors")}</Typography>
                  <span>{reportHeader?.floors}</span>
                </Grid>
                <Grid item>
                  <Typography>{t("My_Report.Zones")}</Typography>
                  <span>{reportHeader?.zones}</span>
                </Grid>
              </>
            )
          }
          <Grid item>
            <Typography>{t("My_Report.Date_Time")}</Typography>
            <span>{formatedStartDate}-{formatedEndDate}</span>
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
};

export { ReportTypeInfo };