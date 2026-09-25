import React, { useEffect, useState } from "react";
import {
  Box,
  IconButton,
  TextField,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Typography,
  Tooltip,
} from "@mui/material";
import {
  DeviceList,
  ChannelList,
  DeviceListProps,
} from "../../interfaces/IFloorAndZone";
import { Search } from "@mui/icons-material";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import { GetAllDevicewithoutZoneService } from "../../services/floorPlanService";
import { useThemeContext } from "../../context/ThemeContext";
import { useTranslation } from "react-i18next";

const DeviceListComponent: React.FC<DeviceListProps> = ({
  reFreshDeviceList,
}) => {
  const [deviceList, setDeviceList] = useState<DeviceList[] | undefined>();
  const [devicesearchTerm, setDeviceSearchTerm] = useState("");
  const [expanded, setExpanded] = useState<string | false>(false);
  const [hoveredZoneName, setHoveredZoneName] = useState("");
  const [expandedChannel, setExpandedChannel] = useState<string | false>(false);
  const { theme, themeColor } = useThemeContext();
  const { t } = useTranslation();
  
  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
        ? `${themeColor}/dark-theme/`
        : `${themeColor}/`;

  useEffect(() => {
    fetchDeviceData();
  }, [reFreshDeviceList]);

  const fetchDeviceData = async () => {
    try {
      const Devices = await GetAllDevicewithoutZoneService();
      const tempDevices = Devices.map((item) => {
        if (item.deviceType === "Camera") {
          if (
            item?.channelEvent &&
            item?.channelEvent?.length &&
            item?.channelEvent?.length > 0 &&
            item?.peopleLines === null &&
            item?.vehicleLines === null
          ) {
            return {
              ...item,
              peopleLines: item.channelEvent[0].peopleLines,
              vehicleLines: item.channelEvent[0].vehicleLines,
            };
          } else {
            return item;
          }
        } else if (
          (item.deviceType === "AIB" || item.deviceType === "MLenses") &&
          Array.isArray(item.channelEvent)
        ) {
          return item;
        } else if (item.deviceType === "ANPR") {
          return item;
        }

        return item;
      });

      const updatedDevices = tempDevices?.map((item) => {
        if (item.deviceType === "Camera" || item.deviceType === "ANPR") {
          return {
            ...item,
            peopleLineIndex: [],
            vehicleLineIndex: [],
          };
        } else if (
          (item.deviceType === "AIB" || item.deviceType === "MLenses") &&
          Array.isArray(item.channelEvent)
        ) {
          const updatedChannelEvent = item.channelEvent.map((channel) => ({
            ...channel,
            peopleLineIndex: [],
            vehicleLineIndex: [],
            id: `${item.id}_${channel.channel}`,
            //  deviceName:`${item.deviceName}-${channel.channel}`,
            deviceName: item.deviceName,
            ipAddress: item.ipAddress,
          }));

          return {
            ...item,
            channelEvent: updatedChannelEvent,
          };
        }

        return item; // return unchanged if neither condition matches
      });
      setDeviceList(updatedDevices);
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };

  const handleDeviceSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    setDeviceSearchTerm(event.target.value);
  };

  const filteredDevices =
    devicesearchTerm?.length >= 3
      ? deviceList?.filter(
          (item) =>
            item.deviceName
              .toLowerCase()
              .includes(devicesearchTerm.toLowerCase()) ||
            item.model.toLowerCase().includes(devicesearchTerm.toLowerCase()) ||
            item.ipAddress
              .toLowerCase()
              .includes(devicesearchTerm.toLowerCase()),
        )
      : deviceList;

  const DraggableItemForDeviceList: React.FC<{
    device: DeviceList | ChannelList;
  }> = ({ device }) => {
   
    return (
      <div
        key={device.id}
        draggable
        style={{
          width: 30,
          height: 30,    
          cursor: "grab",
        }}
         onDragStart={(e) => {
          e.dataTransfer.setData("device", JSON.stringify(device));
        }}
      >
        <img
          src={
            theme === "light"
              ? "/images/activecamera.gif"
              : "/images/dark-theme/activecamera.gif"
          }
          alt="device"
          style={{
            width: 30,
            height: 30,
          }}
        />
      </div>
    );
  };

  return (
    <Box className="search-by-device">
      <Box className="search-by-device-search-bar">
        <TextField
          fullWidth
          variant="outlined"
          onChange={handleDeviceSearch}
          InputProps={{
            endAdornment: (
              <Box
                sx={{
                  borderRadius: "50px",
                  backgroundColor: "#ff8c00",
                  color: "#fff",
                  minWidth: "60px",
                  height: "40px",
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                  cursor: "pointer",
                }}
              >
                <Search />
              </Box>
            ),
          }}
          placeholder={t("Floor_Plan_zones.Device_Search_placeholder")}
          sx={{
            "& fieldset": { border: "none" },
            "& .MuiOutlinedInput-root": {
              borderRadius: "50px",
            },
            "& .MuiOutlinedInput-input": {
              padding: "10px 20px",
            },
          }}
        />
      </Box>

      {filteredDevices &&
      filteredDevices?.length &&
      filteredDevices?.length > 0 ? (
        <div className="search-by-accordion">
          {filteredDevices?.map((device) => {
            let total: number = 0;
            let enable: number = 0;

            if (device.deviceType === "Camera" || device.deviceType === "ANPR") {
              total =
                (device.peopleLines ? device.peopleLines.length : 0) +
                (device.vehicleLines ? device.vehicleLines.length : 0);

              enable =
                (device.peopleLines
                  ? device.peopleLines.filter((line) => line.enable).length
                  : 0) +
                (device.vehicleLines
                  ? device.vehicleLines.filter((line) => line.enable).length
                  : 0);
            } else {
              total = device.channelEvent ? device.channelEvent.length : 0;
              enable = device.channelEvent
                ? device.channelEvent.filter((channel) => channel.connected)
                    .length
                : 0;
            }

            return ((device.deviceType === "Camera" || device.deviceType === "ANPR") && device.peopleLines !== null && device.vehicleLines !== null) ||
              ((device.deviceType === "AIB" ||
                device.deviceType === "MLenses") &&
                device.peopleLines == null &&
                device.vehicleLines == null) ? (
              <Accordion
                key={device.id}
                expanded={expanded === device.id}
                onChange={() => {
                  setExpanded(expanded === device.id ? false : device.id);
                }}
                className="search-by-accordion-items"
              >
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <div className="search-device-accordion-inner">
                    {(device.deviceType === "Camera" || device.deviceType === "ANPR")? (
                      <DraggableItemForDeviceList
                        key={device.id}
                        device={device}
                      />
                    ) : device.deviceType === "MLenses" ? (
                      <img src="/images/M_Lenses.gif" alt="mlenses-device" />
                    ) : (
                      <img
                        src={
                          theme === "light"
                            ? "/images/AI_Box.gif"
                            : "/images/dark-theme/AI_Box.gif"
                        }
                        alt="device"
                      />
                    )}
                    <div className="details-of-seach-device">
                      <Tooltip title={device.deviceName}>
                        <strong>{device.deviceName}</strong>
                      </Tooltip>
                      <Typography>
                        {enable}/{total}{" "}
                        {(device.deviceType === "Camera" || device.deviceType === "ANPR") ? "Lines" : "Channels"}
                      </Typography>
                    </div>
                  </div>
                </AccordionSummary>
                {(device.deviceType === "Camera" || device.deviceType === "ANPR") ? (
                  <AccordionDetails>
                    <div className="floor-zones-accordion-main">
                      {device?.peopleLines
                        ?.filter((line) => line.enable)
                        ?.map((line) => {
                          const isPLineSelected =
                            device?.peopleLineIndex?.includes(line.line);

                          const handleTogglePeopleLine = () => {
                            setDeviceList((prevDevices) =>
                              prevDevices?.map(
                                (item) =>
                                  item.id === device.id
                                    ? {
                                        ...item,
                                        peopleLineIndex: isPLineSelected
                                          ? item.peopleLineIndex?.filter(
                                              (l) => l !== line.line,
                                            ) // Remove line
                                          : [
                                              ...(item.peopleLineIndex ?? []),
                                              line.line,
                                            ], // Add line
                                      }
                                    : item,
                              ),
                            );
                          };

                          return (
                            <div className="floor-zones-accordion">
                              {/* {line.enable ? ( */}
                              <div className="floor-zones-accordion-wrapper">
                                <img                
                                  src={"/images/gray_line.svg"}                    
                                  alt="device"
                                  style={{
                                    opacity: line.isMapped ? 0.5 : 1,
                                    cursor: line.isMapped
                                      ? "not-allowed"
                                      : "pointer",
                                  }}
                                  onClick={
                                    line.isMapped
                                      ? undefined
                                      : handleTogglePeopleLine
                                  }
                                  onMouseEnter={() => {
                                    if (line.isMapped) {
                                      setHoveredZoneName(
                                        line?.zoneName as string,
                                      );
                                    }
                                  }}
                                  onMouseLeave={() => {
                                    if (line.isMapped) {
                                      setHoveredZoneName("");
                                    }
                                  }}
                                  className={isPLineSelected ? "active" : ""}
                                />
                                <div
                                  style={{
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "center",
                                  }}
                                >
                                  <IconButton>
                                    <img
                                      src={"/images/InOutLine.svg"}
                                      alt="delete"
                                      width={15}
                                      height={15}
                                    />
                                  </IconButton>
                                </div>
                              </div>
                              {/* ) : null} */}
                            </div>
                          );
                        })}
                      {device?.vehicleLines
                        ?.filter((line) => line.enable)
                        ?.map((line) => {
                          const isVLineSelected =
                            device?.vehicleLineIndex?.includes(line.line);

                          const handleToggleVehicleLine = () => {
                            setDeviceList((prevDevices) =>
                              prevDevices?.map(
                                (item) =>
                                  item.id === device.id
                                    ? {
                                        ...item,
                                        vehicleLineIndex: isVLineSelected
                                          ? item.vehicleLineIndex?.filter(
                                              (l) => l !== line.line,
                                            ) // Remove line
                                          : [
                                              ...(item.vehicleLineIndex ?? []),
                                              line.line,
                                            ], // Add line
                                      }
                                    : item,
                              ),
                            );
                          };

                          return (
                            <div className="floor-zones-accordion">
                              {/* {line.enable ? ( */}
                              <div className="floor-zones-accordion-wrapper">
                                <img
                                  src={"/images/gray_line.svg"}
                                  alt="device"
                                  style={{
                                    width: 35,
                                    height: 35,
                                    opacity: line.isMapped ? 0.5 : 1,
                                    cursor: line.isMapped
                                      ? "not-allowed"
                                      : "pointer",
                                  }}
                                  onClick={
                                    line.isMapped
                                      ? undefined
                                      : handleToggleVehicleLine
                                  }
                                  onMouseEnter={() => {
                                    if (line.isMapped) {
                                      setHoveredZoneName(
                                        line?.zoneName as string,
                                      );
                                    }
                                  }}
                                  onMouseLeave={() => {
                                    if (line.isMapped) {
                                      setHoveredZoneName("");
                                    }
                                  }}
                                  className={isVLineSelected ? "active" : ""}
                                />
                                <div
                                  style={{
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "center",
                                  }}
                                >
                                  <IconButton>
                                    <img
                                      src={"/images/InOutLine.svg"}
                                      alt="delete"
                                      width={15}
                                      height={15}
                                    />
                                  </IconButton>
                                </div>
                              </div>
                              {/* ) : null} */}
                            </div>
                          );
                        })}
                    </div>
                    {hoveredZoneName ? (
                      <div className="hover-zone-name">{hoveredZoneName}</div>
                    ) : null}
                  </AccordionDetails>
                ) : (
                  <AccordionDetails>
                    {device?.channelEvent?.map((channel, index) => {                  
                      const isConnected = channel.connected;

                      let totalChannelLine: number = 0;
                      let enableChannelLine: number = 0;

                      totalChannelLine =
                        (channel.peopleLines ? channel.peopleLines.length : 0) +
                        (channel.vehicleLines
                          ? channel.vehicleLines.length
                          : 0);

                      enableChannelLine =
                        (channel.peopleLines
                          ? channel.peopleLines.filter((line) => line.enable)
                              .length
                          : 0) +
                        (channel.vehicleLines
                          ? channel.vehicleLines.filter((line) => line.enable)
                              .length
                          : 0);

                      return isConnected ? (
                        <Accordion
                          key={index}
                          sx={{ marginBottom: 1 }}
                          expanded={expandedChannel === channel.id}
                          onChange={() => {
                            const id = channel.id ?? "";
                            setExpandedChannel(
                              expandedChannel === id ? false : id,
                            );
                            // setExpandedChannel(expandedChannel === channel.id ? false : channel.id);
                          }}
                          className="search-by-accordion-items"
                        >
                          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                            <div className="search-device-accordion-inner">
                              <DraggableItemForDeviceList
                                key={channel.id}
                                device={channel}
                              />
                              <div className="details-of-seach-device">
                                <Tooltip title={`Camera ${channel.channel}`}>
                                  <strong>
                                    {channel?.deviceName === "AI Box"
                                      ? "Camera"
                                      : "Lense"}{" "}
                                    {channel.channel}
                                  </strong>
                                </Tooltip>
                                <Typography>
                                  {enableChannelLine}/{totalChannelLine} Lines
                                </Typography>
                              </div>
                            </div>
                          </AccordionSummary>
                          <AccordionDetails>
                            <div className="floor-zones-accordion-main">
                              {channel?.peopleLines
                                ?.filter((line) => line.enable)
                                ?.map((line, index) => {
                                  const isCPLineSelected =
                                    channel?.peopleLineIndex?.includes(
                                      line.line,
                                    );

                                  const handleToggleChannel_PeopleLine = () => {
                                    setDeviceList((prevDevices) =>
                                      prevDevices?.map((item) =>
                                        item.id === device.id
                                          ? {
                                              ...item,
                                              channelEvent: item.channelEvent
                                                ? item.channelEvent.map((ch) =>
                                                    ch.channel ===
                                                    channel.channel
                                                      ? {
                                                          ...ch,
                                                          peopleLineIndex:
                                                            ch.peopleLineIndex?.includes(
                                                              line.line,
                                                            )
                                                              ? ch.peopleLineIndex.filter(
                                                                  (l) =>
                                                                    l !==
                                                                    line.line,
                                                                ) // Remove
                                                              : [
                                                                  ...(ch.peopleLineIndex ??
                                                                    []),
                                                                  line.line,
                                                                ], // Add
                                                        }
                                                      : ch,
                                                  )
                                                : null, // If original channelEvent was null, keep it null
                                            }
                                          : item,
                                      ),
                                    );
                                  };

                                  return (
                                    <div
                                      key={index}
                                      className="floor-zones-accordion"
                                    >
                                      {/* {line.enable ? ( */}
                                      <div className="floor-zones-accordion-wrapper">
                                        <img
                                          src={"/images/gray_line.svg"}
                                          alt="device"
                                          style={{
                                            width: 35,
                                            height: 35,
                                            opacity: line.isMapped ? 0.5 : 1,
                                            cursor: line.isMapped
                                              ? "not-allowed"
                                              : "pointer",
                                          }}
                                          onClick={
                                            line.isMapped
                                              ? undefined
                                              : handleToggleChannel_PeopleLine
                                          }
                                          onMouseEnter={() => {
                                            if (line.isMapped) {
                                              setHoveredZoneName(
                                                line?.zoneName as string,
                                              );
                                            }
                                          }}
                                          onMouseLeave={() => {
                                            if (line.isMapped) {
                                              setHoveredZoneName("");
                                            }
                                          }}
                                          // onClick={
                                          //   handleToggleChannel_PeopleLine
                                          // }
                                          className={
                                            isCPLineSelected ? "active" : ""
                                          }
                                        />
                                        <div
                                          style={{
                                            display: "flex",
                                            alignItems: "center",
                                            justifyContent: "center",
                                          }}
                                        >
                                          <IconButton>
                                            <img
                                              src={"/images/InOutLine.svg"}
                                              alt="delete"
                                              width={15}
                                              height={15}
                                            />
                                          </IconButton>
                                        </div>
                                      </div>
                                      {/* ) : null} */}
                                    </div>
                                  );
                                })}
                              {channel?.vehicleLines
                                ?.filter((line) => line.enable)
                                ?.map((line, index) => {
                                  const isCVLineSelected =
                                    channel?.vehicleLineIndex?.includes(
                                      line.line,
                                    );

                                  const handleToggleChannel_VehicleLine =
                                    () => {
                                      setDeviceList((prevDevices) =>
                                        prevDevices?.map((item) =>
                                          item.id === device.id
                                            ? {
                                                ...item,
                                                channelEvent: item.channelEvent
                                                  ? item.channelEvent.map(
                                                      (ch) =>
                                                        ch.channel ===
                                                        channel.channel
                                                          ? {
                                                              ...ch,
                                                              vehicleLineIndex:
                                                                ch.vehicleLineIndex?.includes(
                                                                  line.line,
                                                                )
                                                                  ? ch.vehicleLineIndex.filter(
                                                                      (l) =>
                                                                        l !==
                                                                        line.line,
                                                                    ) // Remove
                                                                  : [
                                                                      ...(ch.vehicleLineIndex ??
                                                                        []),
                                                                      line.line,
                                                                    ], // Add
                                                            }
                                                          : ch,
                                                    )
                                                  : null, // If original channelEvent was null, keep it null
                                              }
                                            : item,
                                        ),
                                      );
                                    };

                                  return (
                                    <div
                                      key={index}
                                      className="floor-zones-accordion"
                                    >
                                      {/* {line.enable ? ( */}
                                      <div className="floor-zones-accordion-wrapper">
                                        <img
                                          src={"/images/gray_line.svg"}
                                          alt="device"
                                          style={{
                                            width: 35,
                                            height: 35,
                                            opacity: line.isMapped ? 0.5 : 1,
                                            cursor: line.isMapped
                                              ? "not-allowed"
                                              : "pointer",
                                          }}
                                          // onClick={
                                          //   handleToggleChannel_VehicleLine
                                          // }
                                          onClick={
                                            line.isMapped
                                              ? undefined
                                              : handleToggleChannel_VehicleLine
                                          }
                                          onMouseEnter={() => {
                                            if (line.isMapped) {
                                              setHoveredZoneName(
                                                line?.zoneName as string,
                                              );
                                            }
                                          }}
                                          onMouseLeave={() => {
                                            if (line.isMapped) {
                                              setHoveredZoneName("");
                                            }
                                          }}
                                          className={
                                            isCVLineSelected ? "active" : ""
                                          }
                                        />
                                        <div
                                          style={{
                                            display: "flex",
                                            alignItems: "center",
                                            justifyContent: "center",
                                          }}
                                        >
                                          <IconButton>
                                            <img
                                              src={"/images/InOutLine.svg"}
                                              alt="delete"
                                              width={15}
                                              height={15}
                                            />
                                          </IconButton>
                                        </div>
                                      </div>
                                      {/* ) : null} */}
                                    </div>
                                  );
                                })}
                            </div>
                            {hoveredZoneName ? (
                              <div className="hover-zone-name">
                                {hoveredZoneName}
                              </div>
                            ) : null}
                          </AccordionDetails>
                        </Accordion>
                      ) : null;
                      //  (
                      //   <div
                      //     key={index}
                      //     className="search-by-accordion-items search-by-accordion-items-inner"
                      //   >
                      //     <img
                      //       src={"/images/camera.gif"}
                      //       alt="device"
                      //       style={{ width: 30, height: 30 }}
                      //     />
                      //     <div className="details-of-seach-device">
                      //       <strong>
                      //         Camera {channel.channel}
                      //       </strong>
                      //       <Typography>
                      //         {enableChannelLine}/{totalChannelLine} Lines
                      //       </Typography>
                      //     </div>
                      //   </div>
                      // );
                    })}
                  </AccordionDetails>
                )}
              </Accordion>
            ) : null;
          })}
        </div>
      ) : (
        <Box className="sidebar-no-data">
          <Box className="sidebar-no-data-wrapper">
            <img src={`/images/${themeColorPath}noData.gif`} alt="No data" />
            <Typography>No data available</Typography>
            <span>There is no data to load the list of cameras.</span>
          </Box>
        </Box>
      )}
    </Box>
  );
};

export { DeviceListComponent };
