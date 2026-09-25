import React, { useState } from "react";
import { APIErrorCountProps } from "../../../../interfaces/IChart";
import {
  Box,
  Button,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import { formatNumber } from "../../../../utils/formatNumber";
import { useThemeContext } from "../../../../context/ThemeContext";
import { CommonDialog } from "../../../../components";
import { APIErrordeviceObj } from "../../../../interfaces/IChart";

const APIError1_1: React.FC<APIErrorCountProps> = ({
  apiErrordata,
  customizedWidth,
  displayName,
  onZoomClick,
  openZoomDialog,
  setIsDraggable,
}) => {
  const [openDialog, setOpenDialog] = useState(false);
  const [deviceDetail, setDeviceDetail] = useState<APIErrordeviceObj[]>([]);
  const { theme } = useThemeContext();

  const totalDevice = Object.values(apiErrordata ?? {})
    .filter((value) => Array.isArray(value))
    .reduce((sum, arr) => sum + arr.length, 0);

  const result = Object.entries(apiErrordata ?? {}).map(([key, value]) => {
    return {
      key,
      length: Array.isArray(value) ? value.length : 0,
      data: value,
    };
  });

  return (
    <Box sx={{ width: customizedWidth }} className="api-error-wrapper">
      <Box className="widget-main-wrapper">
        <Box className="widget-main-header">
          <Typography variant="h6" component="h2">
            {displayName}
          </Typography>
        </Box>
        <Box className="widget-main-body ">
          <div className="widget-data-wrapper">
            <div className="camera-feature-data widget-table">
              <Table stickyHeader>
                <TableHead>
                  <TableRow>
                    <TableCell>No</TableCell>
                    <TableCell>Device</TableCell>
                    <TableCell>Counts</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {result &&
                    result.map((row, index) => (
                      <TableRow key={index}>
                        <TableCell>
                          <span>{(index + 1).toString().padStart(2, "0")}</span>
                        </TableCell>
                        <TableCell>{row.key}</TableCell>
                        {/* <TableCell> */}
                        <Button
                          variant="outlined"
                          onClick={() => {
                            setOpenDialog(true);
                            setDeviceDetail(row.data);
                          }}
                          style={{
                            color: theme === "light" ? "black" : "white",
                            fontWeight:500
                          }}
                        >
                          {row.length}
                        </Button>
                        {/* </TableCell> */}
                      </TableRow>
                    ))}
                </TableBody>
              </Table>
            </div>
          </div>
        </Box>

        <Box className="widget-main-footer">
          <Box className="widget-main-footer-value">
            <Typography>Total No. of Devices : </Typography>
            <span>{formatNumber(totalDevice ?? 0)}</span>
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
              id="zoomwidgetBtnAveragePeopleCounting"
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
      <CommonDialog
        open={openDialog}
        title="API Error"
        fullWidth={true}
        maxWidth={"lg"}
        customClass="api-error-popup"
        // customClass="add-widgets-main add-widget-outer-pop"
        content={
          <Box className="api-pop-wrapper">
            {deviceDetail?.length > 0 &&
              deviceDetail.map((item, index) => (
                <Box className="api-pop-repeat" key={index}>
                  <div className="api-channel-no">
                    ChannelNo : {item?.channelNo}
                  </div>
                  <div className="api-address-no">
                    IpAddress : {item?.ipAddress}
                  </div>
                </Box>
              ))}
          </Box>
        }
        onConfirm={() => setOpenDialog(false)}
        onCancel={() => setOpenDialog(false)}
        confirmText="Ok"
        cancelText="Cancel"
      />
    </Box>
  );
};

export { APIError1_1 };
