import React, { useEffect, useState } from 'react';
import { Box, Drawer, IconButton, Typography } from '@mui/material';
// import { CustomButton } from '../../components';
import { CustomButton } from '../../components/Reusable/CustomButton';
import { MonitoringListProps } from '../../interfaces/IMonitoring';
import { GridCloseIcon } from '@mui/x-data-grid';
import { MonitoringGroupItemAddEditForm } from '../index'
import { HasPermission } from '../../utils/screenAccessUtils';
import { LABELS } from '../../utils/constants';
import { t } from 'i18next';
import { useThemeContext } from '../../context/ThemeContext';

const PoleMonitoring: React.FC<MonitoringListProps> = ({
  selectedGroup,
  refreshData,
  selectedItem
}) => {
  const [openAddItem, setOpenAddItem] = useState<boolean>(false);
  const [loadFailed, setLoadFailed] = useState(false);
  const [loaded, setLoaded] = useState(false);
  const { theme } = useThemeContext();
  const themeColor = localStorage.getItem("themeColor") || "default-theme";

  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
        ? `${themeColor}/dark-theme/`
        : `${themeColor}/`;



  // Optional fallback if iframe takes too long to load
  useEffect(() => {
    if (selectedItem?.url) {
      const timeout = setTimeout(() => {
        if (!loaded) setLoadFailed(true);
      }, 5000); // 5 seconds timeout

      return () => clearTimeout(timeout);
    }
  }, [selectedItem?.url, loaded]);

  const handleAddItem = () => {
    setOpenAddItem(true)
  }
  const handleCloseAddDrawer = () => {
    setOpenAddItem(false);
  };

  const handleIframeLoad = () => {
    setLoaded(true);
    setLoadFailed(false);
  };

  const handleIframeError = () => {
    setLoadFailed(true);
  };

  return (
    <>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          padding: '16px',
          background: '#FFFFFF',
          borderRadius: '12px',
        }}
      >
        <Typography variant="h6" style={{ fontWeight: 500 }}>
          {selectedGroup?.groupName} {selectedItem && `> ${selectedItem.name}`}
        </Typography>
        {(HasPermission(LABELS.CanAddOrUpdateGroupItem)) && selectedGroup && selectedItem && (
          <CustomButton
            variant="outlined"
            customStyles={{
              background: '#FF8A01',
              color: '#FFFFFF',
              borderRadius: 20,
              border: '1px solid #FF8A01',
              padding: '6px 16px',
              textTransform: 'none',
            }}
            onClick={handleAddItem}
          >
            {t("Monitoring.Add_URL_Preview")}
          </CustomButton>
        )}

      </div>

      {selectedGroup && selectedGroup?.groupItems.length > 0 && selectedItem?.url ? (
        loadFailed ? (
          <Typography variant="body2" color="error">
            ⚠️{t("Monitoring.Unable_Preview")}
          </Typography>
        ) :
          (
            <iframe
              key={selectedItem.url} // This will force iframe to reload when URL changes
              src={selectedItem.url}
              width="80%"
              height="85%"
              style={{ border: 'none' }}
              title="URL Preview"
              onLoad={handleIframeLoad}
              onError={handleIframeError}
            />

          )
      )
        : (
          <Box
            className="add-widget"

          >
            {HasPermission(LABELS.CanAddOrUpdateGroupItem) ?
              <Box className="add-widget-wrapper">
                <img src={`/images/${themeColorPath}NoMonitoring.gif`}alt="Animated GIF" />
                <h3>{t("Monitoring.Add_URL_Preview")}</h3>
                <p>{t("Monitoring.No_Preview_Line1")}
                  <br />{t("Monitoring.No_Preview_Line2")}w</p>
                <CustomButton variant="outlined"
                  onClick={() => setOpenAddItem(true)}
                >
                  <img src={"/images/adddevice.svg"} alt="Add URL Preview" />
                  {t("Monitoring.Add_URL_Preview")}
                </CustomButton>
              </Box>
              :
              <Box className="add-widget-wrapper">
                {t("Monitoring.validation.Add_URL_Preview_Permission_validation")}
              </Box>
            }
          </Box>
        )
      }

      <Drawer
        anchor={"right"}
        open={openAddItem}
        onClose={() => {
          handleCloseAddDrawer();
        }}
        className="cmn-pop"
        ModalProps={{
          onClose: (event, reason) => {
            if (reason !== 'backdropClick') {
              handleCloseAddDrawer();
            }
          }
        }}
      >
        <Box className="cmn-pop-head">
          <Typography variant="h6">  {t("Monitoring.Add_URL_Preview")}</Typography>
          {/* Close Icon on Right */}
          <IconButton onClick={handleCloseAddDrawer}>
            <GridCloseIcon />
          </IconButton>
        </Box>
        
        <MonitoringGroupItemAddEditForm
          onClose={handleCloseAddDrawer}
          groupId={selectedGroup.groupId}
          refreshData={refreshData}
        />
      </Drawer>
    </>
  );
};

export { PoleMonitoring };
