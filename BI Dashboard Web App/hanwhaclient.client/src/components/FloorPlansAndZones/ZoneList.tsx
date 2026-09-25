import React, { useEffect, useState } from "react";
import {
  Box,
  Drawer,
  Grid,
  IconButton,
} from "@mui/material";
import { useForm } from "react-hook-form";
import { GridCloseIcon } from "@mui/x-data-grid";
import { COMMON_CONSTANTS, REGEX } from "../../utils/constants";
// import { REGEX } from "../../utils/constants";
import {
  IZoneList,
  XyPosition,
  ZoneListProps,
} from "../../interfaces/IFloorAndZone";
import {
  GetAllZoneByFloorIdService,
  AddZoneService,
  DeleteZoneService,
} from "../../services/floorPlanService";
import { HasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { CommonDialog } from "../Reusable/CommonDialog";
import { CustomButton } from "../Reusable/CustomButton";
import { CustomTextField } from "../Reusable/CustomTextField";
import { showToast } from "../Reusable/Toast";
import { useTranslation } from "react-i18next";
import { CustomSwitch } from "../Reusable/CustomSwitch";

const ZoneList: React.FC<ZoneListProps> = ({
  selectedFloorId,
  onSelectZone,
  reFreshZoneList,
  onDeleteZone,
  isFileUploaded,
}) => {
  const [openDrawer, setOpenDrawer] = useState(false);
  const [zoneData, setZoneData] = useState<IZoneList[] | undefined>();
  const [selectedZoneId, setSelectedZoneId] = useState<string | null>("");
  const [isZoneEditMode, setIsZoneEditMode] = useState(false);
  const [openZoneDeleteConfirm, setOpenZoneDeleteConfirm] = useState(false);
  const { t } = useTranslation();

  const {
    control,
    handleSubmit,
    setValue,
    reset,
    getValues,
    // formState: { errors },
  } = useForm<IZoneList>({
    defaultValues: {
      name: "",
      peopleOccupancy: null,
      peopleDefaultOccupancy: null,
      vehicleOccupancy: null,
      vehicleDefaultOccupancy: null,
      isParkingZone: false,
      // resetAt: "",
    },
  });

  useEffect(() => {
    GetAllZoneDataByFloorId(selectedFloorId);
  }, [selectedFloorId, reFreshZoneList]);

  const GetAllZoneDataByFloorId = async (id: string) => {
    if (id != "" && id != undefined) {
      const response = await GetAllZoneByFloorIdService(id);
      setZoneData(response);
      if (
        response != undefined &&
        response != null &&
        response?.length &&
        response?.length > 0
      ) {
        const prevSelectedZoneId = selectedZoneId;
        const zoneToSelect =
          prevSelectedZoneId &&
          response.some((z) => z.id === prevSelectedZoneId)
            ? prevSelectedZoneId
            : response[0].id;

        // console.log("zoneToSelect->", zoneToSelect);

        setSelectedZoneId(zoneToSelect as string);

        const zoneDetails = response.find((z) => z.id === zoneToSelect);
        onSelectZone &&
          onSelectZone(
            zoneDetails?.id as string,
            zoneDetails?.zoneArea as XyPosition[],
            zoneDetails?.mappedDevices as any,
          );

        // let tempZoneDetails = response[0];
        // console.log("tempZoneDetails->",tempZoneDetails.mappedDevices);
        // const prevZone = selectedZoneId;
        // if (prevZone && response.some(z => z.id === prevZone)) {
        //   setSelectedZoneId(prevZone);
        // } else if (response.length > 0) {
        //   setSelectedZoneId(tempZoneDetails?.id as string);
        // }
        // setSelectedZoneId((prev) => prev);
        // setSelectedZoneId(tempZoneDetails?.id as string);
        // onSelectZone &&
        //   onSelectZone(
        //     tempZoneDetails?.id as string,
        //     tempZoneDetails?.zoneArea,
        //     tempZoneDetails?.mappedDevices
        //   );
      } else {
        setSelectedZoneId(null);
        onSelectZone && onSelectZone("", [], null);
      }
    }
  };

  const AddZone = async (data: IZoneList) => {
    if (data.peopleOccupancy || data.vehicleOccupancy) {
      const cleanNumber = (value: any) =>
        value === "" || value === undefined ? null : Number(value);

      const zoneParams: IZoneList = {
        ...data,
        floorId: selectedFloorId,
        name: data.name,
        peopleOccupancy: cleanNumber(data.peopleOccupancy),
        peopleDefaultOccupancy: cleanNumber(data.peopleDefaultOccupancy),
        vehicleOccupancy: cleanNumber(data.vehicleOccupancy),
        vehicleDefaultOccupancy: cleanNumber(data.vehicleDefaultOccupancy),
        isParkingZone:data.isParkingZone,
        // resetAt: data.resetAt,
        id: isZoneEditMode && selectedZoneId ? selectedZoneId : "",
      };

      try {
        var res = await AddZoneService(zoneParams);
        if (
          typeof res === "object" &&
          res !== null &&
          "isSuccess" in res &&
          res.isSuccess
        ) {
          setOpenDrawer(false);
          reset();
          GetAllZoneDataByFloorId(selectedFloorId);
        }
      } catch (err) {
        console.error("Error saving in zone:", err);
      }
    } else {
      showToast(
        "Either people occupancy or vehicle occupancy is required.",
        "error",
      );
      return;
    }
  };

  const DeleteZone = async (id: string) => {
    try {
      const deleteData: any = await DeleteZoneService({ id });
      if (deleteData?.isSuccess) {
        setOpenZoneDeleteConfirm(false);
        GetAllZoneDataByFloorId(selectedFloorId);
        onDeleteZone();
      }
    } catch (err: any) {
      console.error("Error deleting role:", err);
    }
  };

  return (
    <>
      <CommonDialog
        customClass="cmn-confirm-delete-icon"
        open={openZoneDeleteConfirm}
        title="Delete Confirmation!"
        content="Are you sure you want to continue?"
        onConfirm={() => selectedZoneId && DeleteZone(selectedZoneId)}
        onCancel={() => setOpenZoneDeleteConfirm(false)}
        confirmText={t("Common_DELETE_Confirmation_Dialog.Delete")}
        cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
        type="delete"
        titleClass={true}
      />

      {isFileUploaded && (
        <Box className="add-zone-head">
          {zoneData?.map((item) => (
            <Box
              // className="add-zone-list"
              className={
                selectedZoneId === item.id
                  ? "add-zone-list active"
                  : "add-zone-list"
              }
              key={item.id}
              onClick={() => {
                if (selectedZoneId !== item.id) {
                  setSelectedZoneId(item.id as string);
                  onSelectZone &&
                    onSelectZone(
                      item.id as string,
                      item.zoneArea,
                      item?.mappedDevices,
                    );
                }
              }}
            >
              <Box className="add-zone-button">{item.name}</Box>

              <Box className="edit-delete-icons">
                {HasPermission(LABELS.CanAddUpdateZone) && (
                  <IconButton
                    size="small"
                    onClick={() => {
                      setSelectedZoneId(item.id as string);
                      setValue("name", item.name || "");
                      setValue("peopleOccupancy", item.peopleOccupancy || null);
                      setValue(
                        "peopleDefaultOccupancy",
                        item.peopleDefaultOccupancy || null,
                      );
                      setValue(
                        "vehicleOccupancy",
                        item.vehicleOccupancy || null,
                      );
                      setValue(
                        "vehicleDefaultOccupancy",
                        item.vehicleDefaultOccupancy || null,
                      );
                      setValue("isParkingZone",item.isParkingZone);
                      // if (item?.resetAt) {
                      //   const temresetAt = format(item.resetAt as string, "HH:mm");
                      //   setValue("resetAt", temresetAt || "");
                      // }

                      setOpenDrawer(true);
                      setIsZoneEditMode(true);
                    }}
                  >
                    <img src={"/images/edit_new.svg"} alt="edit" />
                  </IconButton>
                )}
                {HasPermission(LABELS.CanDeleteZone) && (
                  <IconButton
                    size="small"
                    onClick={() => {
                      setSelectedZoneId(item.id as string);
                      setOpenZoneDeleteConfirm(true);
                    }}
                  >
                    <img src={"/images/delete_new.svg"} alt="delete" />
                  </IconButton>
                )}
              </Box>
            </Box>
          ))}

          {HasPermission(LABELS.CanAddUpdateZone) && (
            <IconButton
              onClick={() => {
                setIsZoneEditMode(false);
                setOpenDrawer(true);
              }}
              className="plus-i-button"
            >
              <img src={"/images/plus.svg"} alt="add" width={50} height={50} />
            </IconButton>
          )}
        </Box>
      )}

      <Drawer
        anchor={"right"}
        open={openDrawer}
        onClose={() => {
          reset();
          setOpenDrawer(false);
        }}
        className="cmn-pop"
      >
        <Box component="form" onSubmit={handleSubmit(AddZone)}>
          <Box className="cmn-pop-head">
            {/* Title on Left */}
            <h6>
              {isZoneEditMode
                ? t("Floor_Plan_zones.Add_Edit_Zone_Drawer.Edit_Zone")
                : t("Floor_Plan_zones.Add_Edit_Zone_Drawer.Add_Zone")}
            </h6>

            {/* Close Icon on Right */}
            {/* <IconButton onClick={() => setOpenAddCamera(false)}> */}
            <IconButton
              onClick={() => {
                reset();
                setOpenDrawer(false);
              }}
            >
              <GridCloseIcon />
            </IconButton>
          </Box>
          <div className="cmn-pop-form">
            <div className="cmn-pop-form-wrapper">
              <div className="cmn-pop-form-inner">
                <Grid item xs={12} md={12}>
                  <CustomTextField
                    name="name"
                    control={control}
                    label={
                      <span>
                        {t("Floor_Plan_zones.Add_Edit_Zone_Drawer.Name")}{" "}
                        <span className="star-error">*</span>
                      </span>
                    }
                    fullWidth
                    rules={{
                      required: t(
                        "Floor_Plan_zones.validation.Zone_name_required",
                      ),
                      pattern: {
                        value: REGEX.Floor_Zone_Name_Regex,
                        message: t(
                          "Floor_Plan_zones.validation.Zone_name_strong_validation",
                        ),
                      },
                      maxLength: {
                        value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                        message: t(
                          "Floor_Plan_zones.validation.Zone_name_MAX_validation",
                          { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH },
                        ),
                      },
                      minLength: {
                        value: COMMON_CONSTANTS.MIN_TEXT_FIELD_LENGTH,
                        message: t(
                          "Floor_Plan_zones.validation.Zone_name_MIN_validation",
                          { max: COMMON_CONSTANTS.MIN_TEXT_FIELD_LENGTH },
                        ),
                      },
                    }}
                    placeholder={t(
                      "Floor_Plan_zones.Add_Edit_Zone_Drawer.Name_Placeholder",
                    )}
                  />
                </Grid>
                <div className="half-pop-field-wrapper">
                  <Grid item className="main-pop-har-column">
                    <CustomTextField
                      name="peopleOccupancy"
                      control={control}
                      label={t(
                        "Floor_Plan_zones.Add_Edit_Zone_Drawer.People_Occupancy",
                      )}
                      type="number"
                      rules={
                        {
                          // required: "People Occupancy of Zone is required",
                          // pattern: {
                          //   value: REGEX.Name_Regex,
                          //   message: "Enter a valid People Occupancy of Zone",
                          // },
                        }
                      }
                      placeholder={t(
                        "Floor_Plan_zones.Add_Edit_Zone_Drawer.MAX_Occupancy_Placeholder",
                      )}
                      // required
                    />
                  </Grid>
                  <Grid item className="main-pop-har-column">
                    <CustomTextField
                      name="peopleDefaultOccupancy"
                      control={control}
                      label=" "
                      type="number"
                      rules={{
                        // required: "People Default Occupancy is required",
                        validate: (value: number) => {
                          const peopleOccupancy = Number(
                            getValues("peopleOccupancy"),
                          );
                          const peopleDefault = Number(value);
                          if (!peopleOccupancy || isNaN(peopleOccupancy))
                            return true;
                          return peopleDefault < peopleOccupancy
                            ? true
                            : t(
                                "Floor_Plan_zones.validation.People_occupancy_validation",
                              );
                        },
                        // pattern: {
                        //   value: REGEX.Name_Regex,
                        //   message: "Enter a valid People Default Occupancy",
                        // },
                      }}
                      placeholder={t(
                        "Floor_Plan_zones.Add_Edit_Zone_Drawer.Default_Occupancy_Placeholder",
                      )}
                      // required
                    />
                  </Grid>
                  <Grid item className="main-pop-har-column">
                    <CustomTextField
                      name="vehicleOccupancy"
                      control={control}
                      label={t(
                        "Floor_Plan_zones.Add_Edit_Zone_Drawer.Vehicle_Occupancy",
                      )}
                      type="number"
                      rules={
                        {
                          // required: "Vehicle Occupancy is required",
                          // pattern: {
                          //   value: REGEX.Name_Regex,
                          //   message: "Enter a valid Vehicle Occupancy of Zone",
                          // },
                        }
                      }
                      placeholder={t(
                        "Floor_Plan_zones.Add_Edit_Zone_Drawer.MAX_Occupancy_Placeholder",
                      )}
                      // required
                    />
                  </Grid>
                  <Grid item className="main-pop-har-column">
                    <CustomTextField
                      name="vehicleDefaultOccupancy"
                      control={control}
                      label=" "
                      type="number"
                      rules={{
                        // required: "Vehicle Default Occupancy is required",
                        validate: (value: number) => {
                          const vehicleOccupancy = Number(
                            getValues("vehicleOccupancy"),
                          );
                          const vehicleDefault = Number(value);
                          if (!vehicleOccupancy || isNaN(vehicleOccupancy))
                            return true;
                          return vehicleDefault < vehicleOccupancy
                            ? true
                            : t(
                                "Floor_Plan_zones.validation.Vehicle_occupancy_validation",
                              );
                        },
                        // pattern: {
                        //   value: /^[0-9]+$/,
                        //   message: "Enter a valid Vehicle Default Occupancy of Zone",
                        // },
                      }}
                      placeholder={t(
                        "Floor_Plan_zones.Add_Edit_Zone_Drawer.Default_Occupancy_Placeholder",
                      )}
                      // required
                    />
                  </Grid>
                  <Grid item className="main-pop-har-column">
                    <CustomSwitch
                      name="isParkingZone"
                      control={control}
                      label={t(
                        "Floor_Plan_zones.Add_Edit_Zone_Drawer.Is_Parking_Zone",
                      )}
                    />
                  </Grid>
                  {/* <Grid item className="main-pop-har-column">
                    <CustomTextField
                      name="resetAt"
                      control={control}
                      label="Reset At"
                      fullWidth
                      rules={{
                        required: "Reset At is required",
                        pattern: {
                          // value: REGEX.Name_Regex,
                          message: "Enter a valid Reset At",
                        },
                      }}
                      placeholder="--:-- --"
                      type="time"
                      required
                    />
                  </Grid> */}
                </div>
              </div>

              <CustomButton
                className="common-btn-design"
                fullWidth={true}
                customStyles={{ mt: 2 }}
              >
                {isZoneEditMode ? t("Save_btn") : t("Add_btn")}
              </CustomButton>
            </div>
          </div>
        </Box>
      </Drawer>
    </>
  );
};

export { ZoneList };
