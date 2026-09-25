import React, { useEffect, useState } from "react";
import { Grid, Box, Typography, Paper } from "@mui/material";
import { ILicenseData } from "../../interfaces/ILicense";
import { GetCurrentLicenseDetailService } from "../../services/settingService";
import { formatDateDDMMYY } from "../../utils/dateUtils";
import { useTranslation } from "react-i18next";
import { he } from "date-fns/locale";

const CurrentLicenseDetail = () => {
  const [license, setLicense] = useState<ILicenseData | null>(null);
  // console.log(`license details => `, license);
  const { t } = useTranslation();
  useEffect(() => {
    const fetchData = async () => {
      try {
        const response: any = await GetCurrentLicenseDetailService();
        if (response !== null) {
          setLicense(response as ILicenseData);
        }
      } catch (err: any) {
        console.error("Error fetching data:", err);
      }
    };

    fetchData();
  }, []);

  const boxStyle = {
    display: "flex",
    flexDirection: "column" as const,
    alignItems: "center",
    justifyContent: "center",
    padding: "12px",
    backgroundColor: "#f5f5f5",
    borderRadius: "12px",
    width: "100%"
  };

  const items = license
    ? [
        { label: t("License_Setting.Current_License_Detail.Product_Name"), value: "Vision Insight", color: "#FF8A00" },
        { label: t("License_Setting.Current_License_Detail.License_Version"), value: "1.0.0", color: "#FF8A00" },
        {
          label: t("License_Setting.Current_License_Detail.Type"),
          value: license.licenseType == null ? "-" : license.licenseType,
          color: "#2DB400",
        },
        {
          label: t("License_Setting.Current_License_Detail.Camera_Utilized"),
          value: `${license.utilizedCamera}/${license.cameras}`,
          color: "#FF8A00",
        },
        {
          label: t("License_Setting.Current_License_Detail.User_Utilized"),
          value: `${license.utilizedUser}/${license.users}`,
          color: "#FF8A00",
        },
        {
          label: t("License_Setting.Current_License_Detail.Start_Date"),
          value: formatDateDDMMYY(license.startDate),
          color: "#FF8A00",
        },
        {
          label: t("License_Setting.Current_License_Detail.End_Date"),
          value: formatDateDDMMYY(license.expiryDate),
          color: "#FF8A00",
        },
      ]
    : [];

  return (
    <Paper className="current-license-detail">
      <Typography variant="h5">{t("License_Setting.Current_License_Detail.Title")}</Typography>
      <Grid className="current-license-main">
        {items.map((item, index) => (
          <Grid key={index} item className="current-license-detail-box-wrapper">
            <Box sx={boxStyle} className="current-license-detail-box">
              <Typography variant="body2" fontWeight={500}>
                {item.label}
              </Typography>
              <Typography
                variant="subtitle1"
                fontWeight={600}
                color={item.color}
                mt={1}
              >
                {item.value}
              </Typography>
            </Box>
          </Grid>
        ))}
      </Grid>
    </Paper>
  );
};

export { CurrentLicenseDetail };
