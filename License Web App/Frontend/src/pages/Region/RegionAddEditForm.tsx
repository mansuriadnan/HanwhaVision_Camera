import React, { useState, useEffect, useCallback } from "react";
import { Container, Box, Grid, Typography, TextField } from "@mui/material";
import {
  AddUserService,
  OtpVerificationService,
  sendOtpService,
  UpdateUserService,
} from "../../services/userService";
import { IUsers } from "../../interfaces/IGetAllUsers";
import { GetAllRoleService } from "../../services/roleService";
import { IRole } from "../../interfaces/IRole";
import { ILookup } from "../../interfaces/ILookup";
import { LABELS, REGEX } from "../../utils/constants";
import {
  CustomButton,
  CustomTextField,
  CustomMultiSelect,
} from "../../components/index";
import { SubmitHandler, useForm } from "react-hook-form";
import CommonDialog from "../../components/Reusable/CommonDialog";
import { ApiResponse } from "../../interfaces/IApiResponse";
import { AddUpdateRegionService } from "../../services/regionService";
import { IRegion } from "../../interfaces/ICreateRegion";

interface RegionAddEditFormProps {
  onClose: () => void;
  region?: any;
  refreshData: () => void;
}

interface RegionFormInputs {
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  roleIds: string[];
  password: string;
  confirmPassword: string;
}


interface RegionFormInputs {
  name: string;
}


const RegionAddEditForm: React.FC<RegionAddEditFormProps> = ({
  onClose,
  region,
  refreshData,
}) => {
  const isEditMode = region !== null && region !== undefined;
  const [Rolelist, setRolelist] = useState<ILookup[]>([]);
  const [previousEmailID, setPreviousEmailID] = useState<string>("");
  const [openOTPDialog, setOpenOTPDialog] = useState<boolean>(false);
  const [updatedUserData, setUpdatedUserData] = useState<IUsers>();
  const [otp, setOtp] = useState(["", "", "", "", "", ""]);
  const [error, setError] = useState("");
  const REACT_APP_TIMEOUT = process.env.REACT_APP_TIMEOUT as string;
  const otpTimeOut =  Number.parseInt(REACT_APP_TIMEOUT);
  const {
    control,
    setValue,
    handleSubmit,
    watch,
    formState: { errors, isDirty },
  } = useForm<RegionFormInputs>({
    defaultValues: {
      name: "",     
    },
  });

  useEffect(() => {
    const initializeData = async () => {
   
        if (isEditMode) {
          const initialRegion = region;
          setValue("name", initialRegion?.name || "");
        }
     
    };

    initializeData();
  }, [isEditMode, region]);

  const handleAddRegion: SubmitHandler<RegionFormInputs> = (data) => {
    if (isEditMode && !isDirty) {
      console.warn("No changes detected");
      return;
    }

    const regionData: Omit<IRegion, "createdDateTime" | "lastUpdatedDateTime"> = {
      name: data.name,    
      ...(isEditMode && region?.id && { id: region.id }),
    };

    if (isEditMode) {
        const UpdateRegion = async () => {
            try {
                const data: any = await AddUpdateRegionService(regionData);
                if (data?.isSuccess) {
                    onClose();
                    refreshData();
                }
            } catch (err: any) {
                console.error(err);
            }
        };
        UpdateRegion();
    } else {
      const AddRegion = async () => {
        try {
          const data: any = await AddUpdateRegionService(regionData);
          if (data?.isSuccess) {
            onClose();
            refreshData();
          }
        } catch (err: any) {
          console.error(err);
        }
      };
      AddRegion();
    }
  };


  return (
    <Box
      className="comn-pop-up-design"
      component="form"
      onSubmit={handleSubmit(handleAddRegion)}
      noValidate
    >
      <Grid  style={{display :'grid'}}>
        <Grid item xs={12} md={12} lg={12}> 
          <CustomTextField
            name="name"
            label="Region Name"
            control={control}
            rules={{
              required: "Region name is required.",
              pattern: {
                value: REGEX.Name_Regex,
                message: "Region Name must be 3–50 characters long and contain only letters and spaces.",
              },
            }}
            placeholder="Enter region name"
            required
            fullWidth
          />
        </Grid>

        <Grid item xs={12} container>
          <CustomButton fullWidth className="pop-bottom-cmn-btn"  disabled={isEditMode && !isDirty}>
            {isEditMode ? "Update Region" : "Add Region"}
          </CustomButton>
        </Grid>
      </Grid>
    </Box>
  );
};

export { RegionAddEditForm };
