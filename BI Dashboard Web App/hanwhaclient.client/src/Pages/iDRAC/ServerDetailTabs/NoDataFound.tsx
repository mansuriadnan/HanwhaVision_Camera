import React from 'react';
import { Box, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useThemeContext } from '../../../context/ThemeContext';

const NoDataFound: React.FC = () => {
  const { theme, themeColor } = useThemeContext();
  const { t } = useTranslation();

  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark" ? "dark-theme/" : ""
      : theme === "dark" ? `${themeColor}/dark-theme/` : `${themeColor}/`;

  return (
    <Box className="memory-tab-wrapper no-data-found-idrac">
      <Box className="no-data-douns">
        <Box sx={{ width: 200, justifyItems: "center", flex: 1 }}>
          <img src={`/images/${themeColorPath}noData.gif`} alt="Animated GIF" width="100" height="100" />
          <Typography sx={{ fontWeight: 600, fontSize: 24, color: "#090909" }}>
            {t("No_data_found")}
          </Typography>
        </Box>
      </Box>
    </Box>
  );
};

export default NoDataFound;