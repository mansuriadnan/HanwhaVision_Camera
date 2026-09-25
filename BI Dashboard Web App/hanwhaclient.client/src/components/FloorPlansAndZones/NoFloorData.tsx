import React, { useState } from "react";
import { Box, Typography } from "@mui/material";
import { AddFloorService } from "../../services/floorPlanService";
import { COMMON_CONSTANTS, REGEX } from "../../utils/constants";
import { useForm } from "react-hook-form";
import { IAddFloor } from "../../interfaces/IFloorAndZone";
import { HasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { FloorAndZoneList } from "./FloorAndZoneList";
import { CustomTextFieldWithButton } from "../Reusable/CustomTextFieldWithButton";
import { useTranslation } from "react-i18next";
import { useThemeContext } from "../../context/ThemeContext";

const NoFloorData: React.FC = () => {
  const [showNoData, setShowNoData] = useState(true);
  const { t } = useTranslation();
  const { theme, themeColor } = useThemeContext();

  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
        ? `${themeColor}/dark-theme/`
        : `${themeColor}/`;

  const { control, handleSubmit, reset } = useForm<IAddFloor>({
    defaultValues: {
      floorName: "",
    },
  });

  const AddFloorPlan = async (data: any) => {
    try {
      const param = {
        floorPlanName: data.floorName,
        id: "",
      };

      var result: any = await AddFloorService(param);
      if (result && result.isSuccess) {
        // setShowList(true);
        setShowNoData(false);
        reset();
      }
      // console.log("result", result);
    } catch (error) {
      console.error("Submission Error:", error);
    }
  };

  const backgroundStyle = {
    backgroundImage: ` url('/images/lines.png'), linear-gradient(287.68deg, #FE6500 -0.05%, #FF8A00 57.77%)`,
    backgroundSize: "cover",
    backgroundRepeat: "no-repeat",
    backgroundPosition: "center",
  };

  return (
    <>
      {showNoData ? (
        <>
          <div className="top-orange-head" style={backgroundStyle}>
            <Box className="top-orange-head-left">
              <Typography variant="h4">
                {" "}
                {t("Floor_Plan_zones.Floor_Plan_zones_header")}
              </Typography>
              <Typography>
                {t("Floor_Plan_zones.Floor_Plan_zones_Description")}
              </Typography>
            </Box>
          </div>

          <Box
            className="add-widget"
            sx={{
              backgroundImage: "url('images/no-data.png')",
            }}
          >
            <Box className="add-widget-wrapper">
              <img src={`/images/${themeColorPath}noData.gif`} alt="Animated GIF" />
              <h3>{t("Floor_Plan_zones.No_Data.No_data_available")}</h3>
              <p>
                {t("Floor_Plan_zones.No_Data.No_data_description1")}
                <br></br>
                {t("Floor_Plan_zones.No_Data.No_data_description2")}
              </p>

              {HasPermission(LABELS.CanAddOrUpdateFloor) && (
                <Box
                  component="form"
                  onSubmit={handleSubmit(AddFloorPlan)}
                  sx={{ marginTop: 2 }}
                >
                  <CustomTextFieldWithButton
                    name="floorName"
                    control={control}
                    rules={{
                      required: t("Floor_Plan_zones.validation.Floor_name_is_required"),
                      pattern: {
                        value: REGEX.Floor_Zone_Name_Regex,
                        message:
                          t("Floor_Plan_zones.validation.Floor_name_strong_validation"),
                      },
                      maxLength: {
                        value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                        message: t("Floor_Plan_zones.validation.Floor_name_MAX_validation"),
                      },
                      minLength: {
                        value: COMMON_CONSTANTS.MIN_TEXT_FIELD_LENGTH,
                        message: t("Floor_Plan_zones.validation.Floor_name_MIN_validation"),  
                      },
                    }}
                    placeholder={t("Floor_Plan_zones.Floor_plan_name_placeholder")}   
                    customStyles={{ width: 350 }}
                  />
                </Box>
              )}
            </Box>
          </Box>
        </>
      )
       : (
        <FloorAndZoneList showNoData={showNoData}></FloorAndZoneList>
      )}
    </>
  );
};

export { NoFloorData };
