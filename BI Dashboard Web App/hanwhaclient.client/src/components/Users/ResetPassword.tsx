import React, { useState } from "react";
import {
  Button, 
  Stack,
} from "@mui/material";
import {
  BrowserRouter as Router,
  useNavigate
} from "react-router-dom";
import { ResetPasswordService } from "../../services/userService";
import { showToast } from "../Reusable/Toast";
import { CustomTextField } from "../Reusable/CustomTextField";
import { SubmitHandler, useForm } from "react-hook-form";
import { REGEX } from "../../utils/constants";
import { useTranslation } from "react-i18next";
import { IPreferencesFormProps } from "../../interfaces/IUserPreferences";
interface IResetPassword {
  password: string;
  confirmPassword: string;
}
// Reset Password Component
const ResetPassword: React.FC<IPreferencesFormProps> = ({ onClose }) => {
   const { control, handleSubmit, watch } = useForm<IResetPassword>({
      defaultValues: {
        password: "",
        confirmPassword: "",
      },
    });
  const navigate = useNavigate();
  const password = watch("password");
  const { t } = useTranslation();

  const handleResetPassword: SubmitHandler<IResetPassword> = async (data) => {   

    try {
      const result = await ResetPasswordService({
        password: password,
      });
      if (typeof result === "string") {
        console.error("Error:", result);
        showToast(result, "error");
      } else {
        showToast(result.message, "success");
        if (onClose) {
          onClose();
        }
        navigate("/");
      }      
    } catch (error) {
      console.error("Error resetting password:", error);
    }
    
  };


   return (
      <form onSubmit={handleSubmit(handleResetPassword)} className="preferences-main">
       
         <CustomTextField
           name="password"
           label={<span>{t("Reset_Password_Screen.New_Password")} <span className="star-error">*</span></span>}
           type="password"
           control={control}
           rules={{
             required: t("Reset_Password_Screen.New_Password_Required"),
             validate: (value: any) => {
               if (REGEX.Password_Regex.test(value)) {
                 return true;
               }
               return t("Manage_User.Validation.Password_Strong_Validation");
             },
           }}
           fullWidth
           autoFocus
           customStyles={{
             marginBottom: "16px", // Add spacing
           }}
           placeholder={t("Reset_Password_Screen.New_Password_Placeholder")}
         />

         <CustomTextField
           name="confirmPassword"
           label={<span>{t("Manage_User.Add_Edit_User_Drawer.Confirm_Password")} <span className="star-error">*</span></span>}
           control={control}
           rules={{
             required: t("Manage_User.Validation.Confirm_Password_Required"),
             validate: (value: any) =>
               value === password || t("Manage_User.Validation.Confirm_Password_Match"),
           }}
           type="password"
           fullWidth
           customStyles={{
             marginBottom: "16px",
           }}
           placeholder={t("Manage_User.Add_Edit_User_Drawer.Confirm_Password_Placeholder")}
           inputProps={{
             onCopy: (e: React.ClipboardEvent) => e.preventDefault(),
             onPaste: (e: React.ClipboardEvent) => e.preventDefault(),
             onCut: (e: React.ClipboardEvent) => e.preventDefault(),
             onDrop: (e: React.DragEvent) => e.preventDefault(),
           }}
         />
  
        {/* Buttons Section */}
        <Stack direction="row" spacing={2} justifyContent="flex-end" mt={4}>
          <Button
            className="common-btn-design common-btn-design-transparent"
            onClick={onClose}
            sx={{
              backgroundColor: "#F9F9FA",
              color: "#424242",
              textTransform: "none",
            }}
          >
            {t("Common_DELETE_Confirmation_Dialog.Cancel")}
          </Button>
          <Button
            className="common-btn-design"
            type="submit"
            sx={{
              background: "linear-gradient(to right, #FF8A00, #FE6500)",
              color: "white",
              textTransform: "none",
            }}
            autoFocus
          >
            {t("Reset_Password_Screen.Reset_Password")}            
          </Button>
        </Stack>
      </form>
    );
};

export default ResetPassword;
