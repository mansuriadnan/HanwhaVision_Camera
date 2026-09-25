import React, { useState, useEffect, useRef } from "react";
import { Box, Grid } from "@mui/material";
import { AddUserService, UpdateUserService } from "../../services/userService";
import { GetAllRoleService } from "../../services/roleService";
import { ILookup } from "../../interfaces/ILookup";
import { COMMON_CONSTANTS, REGEX } from "../../utils/constants";
import { SubmitHandler, useForm } from "react-hook-form";
import { IUsers } from "../../interfaces/IManageUsers";
import { IRole } from "../../interfaces/IRolePermission";
import { CustomButton } from "../Reusable/CustomButton";
import { CustomMultiSelect } from "../Reusable/CustomMultiSelect";
import { CustomTextField } from "../Reusable/CustomTextField";
import { useTranslation } from "react-i18next";

interface UserAddEditFormProps {
  onClose: () => void;
  user?: any;
  refreshData: () => void;
}

interface UserFormInputs {
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  roleIds: string[];
  password: string;
  confirmPassword: string;
}

const UserAddEditForm: React.FC<UserAddEditFormProps> = ({
  onClose,
  user,
  refreshData,
}) => {
  const isEditMode = user !== null && user !== undefined;
  const [Rolelist, setRolelist] = useState<ILookup[]>([]);
  const { t } = useTranslation();

  const {
    control,
    setValue,
    handleSubmit,
    watch,
    reset,
    formState: { errors },
  } = useForm<UserFormInputs>({
    defaultValues: {
      firstName: "",
      lastName: "",
      userName: "",
      email: "",
      roleIds: [],
      password: "",
      confirmPassword: "",
    },
  });

  const firstNameRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    if (firstNameRef.current) {
      firstNameRef.current.focus();
    }
  }, []);

  const password = watch("password");

  useEffect(() => {
    const initializeData = async () => {
      try {
        const response: any = await GetAllRoleService();
        const temprolelist = response as IRole[];
        const roleData = temprolelist?.map((item) => ({
          title: item.roleName,
          id: item.id,
        }));
        setRolelist(roleData as ILookup[]);
        if (isEditMode) {
          const initialUser = user;
          setValue("firstName", initialUser?.firstname || "");
          setValue("lastName", initialUser?.lastname || "");
          setValue("userName", initialUser?.username || "");
          setValue("email", initialUser?.email || "");
          setValue(
            "roleIds",
            Array.isArray(initialUser.roleIds) ? initialUser.roleIds : []
          );
          setValue("password", "");
          setValue("confirmPassword", "");
        }
      } catch (err) {
        console.error(err);
      }
    };

    initializeData();
  }, [isEditMode, user]);

  const handleAddUser: SubmitHandler<UserFormInputs> = (data) => {
    const userData: Omit<IUsers, "createdDateTime" | "lastUpdatedDateTime"> = {
      firstname: data.firstName,
      lastname: data.lastName,
      username: data.userName,
      email: data.email,
      roleIds: data.roleIds,
      password: data.password,
      ...(isEditMode && user?.id && { id: user.id }),
    };

    if (isEditMode) {
      const Updateuser = async () => {
        try {
          const data = await UpdateUserService(userData);
          if (typeof data === 'object' && data !== null && 'isSuccess' in data && data.isSuccess) {
            reset();
            onClose();
            refreshData();
          }
        } catch (err: any) {
          console.error(err);
        }
      };
      Updateuser();
    } else {
      const Adduser = async () => {
        try {
          const data = await AddUserService(userData);
          if (typeof data === 'object' && data !== null && 'isSuccess' in data && data.isSuccess) {
            reset();
            onClose();
            refreshData();
          }

        } catch (err: any) {
          console.error(err);
        }
      };
      Adduser();
    }
  };

  return (
    <div className="cmn-pop-form">
      <div className="cmn-pop-form-wrapper">
        <Box
          component="form"
          onSubmit={handleSubmit(handleAddUser)}
          noValidate
        >
          <Grid className="cmn-pop-form-inner">
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <CustomTextField
                  name="firstName"
                  label={<span>{t("Manage_User.Add_Edit_User_Drawer.First_Name")} <span className="star-error">*</span></span>}
                  control={control}
                  rules={{
                    required:t("Manage_User.Validation.FirstName_Required"),
                    pattern: {
                      value: REGEX.Name_Regex,
                      message:t("Manage_User.Validation.FistName_Valid"),
                    },
                    maxLength: {
                      value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                      message: t("Manage_User.Validation.FirstName_Strong_Validation", { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                    },
                  }}
                  placeholder={t("Manage_User.Add_Edit_User_Drawer.First_Name_Placeholder")}
                  required
                  fullWidth
                  // inputProps={{
                  //   maxLength: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                  // }}
                  inputRef={firstNameRef}
                />

              </Grid>

              <Grid item xs={12} md={6}>
                <CustomTextField
                  name="lastName"
                  label={<span>{t("Manage_User.Add_Edit_User_Drawer.Last_Name")} <span className="star-error">*</span></span>}
                  control={control}
                  rules={{
                    required:  t("Manage_User.Validation.LastName_Required"),
                    pattern: {
                      value: REGEX.Name_Regex,
                      message: t("Manage_User.Validation.LastName_Valid"),
                    },
                    maxLength: {
                      value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                      message: t("Manage_User.Validation.LastName_Strong_Validation", { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                    },
                  }}
                  placeholder={t("Manage_User.Add_Edit_User_Drawer.Last_Name_Placeholder")}
                  required
                  fullWidth
                // inputProps={{
                //   maxLength: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                // }}
                />
              </Grid>
            </Grid>

            <Grid item xs={12} md={6}>
              <CustomTextField
                name="userName"
                label={<span>{t("Manage_User.Add_Edit_User_Drawer.Username")} <span className="star-error">*</span></span>}
                control={control}
                rules={{
                  required: t("Manage_User.Validation.Username_Required"),
                  pattern: {
                    value: REGEX.UserName_Regex,
                    message: t("Manage_User.Validation.Username_Valid"),
                  },
                  maxLength: {
                    value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                    message: t("Manage_User.Validation.Username_Strong_Validation", { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                  },
                }}
                placeholder={t("Manage_User.Add_Edit_User_Drawer.Username_Placeholder")}
                required
                fullWidth
              // inputProps={{
              //   maxLength: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
              // }}
              />
            </Grid>

            <Grid item xs={12} md={6}>
              <CustomTextField
                name="email"
                label={<span>{t("Manage_User.Add_Edit_User_Drawer.Email")} <span className="star-error">*</span></span>}
                control={control}
                rules={{
                  required: t("Manage_User.Validation.Email_Required"),
                  pattern: {
                    value: REGEX.Email_Regex,
                    message: t("Manage_User.Validation.Email_Valid"),
                  },
                  maxLength: {
                    value: COMMON_CONSTANTS.MAX_EMAIL_FIELD_LENGTH,
                    message: t("Manage_User.Validation.Email_Strong_Validation",{ max : COMMON_CONSTANTS.MAX_EMAIL_FIELD_LENGTH}),
                  },
                }}
                placeholder={t("Manage_User.Add_Edit_User_Drawer.Email_Placeholder")}
                required
                fullWidth
              />
            </Grid>

            <Grid item xs={12} md={6}>
              <CustomTextField
                name="password"
                label={isEditMode ? t("Manage_User.Add_Edit_User_Drawer.Password") : <span>{t("Manage_User.Add_Edit_User_Drawer.Password")} <span className="star-error">*</span></span>}
                control={control}
                type={"password"}
                rules={{
                  ...(isEditMode
                    ? {
                      pattern: {
                        value: REGEX.Password_Regex,
                        message: t("Manage_User.Validation.Password_Strong_Validation"),
                      },
                      maxLength: {
                        value: COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH,
                        message:  t("Manage_User.Validation.Password_Max_Validation",{max : COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH}),
                      },
                    }
                    : {
                      required: t("Manage_User.Validation.Password_Required"),
                      pattern: {
                        value: REGEX.Password_Regex,
                        message:  t("Manage_User.Validation.Password_Strong_Validation"),
                      },
                      maxLength: {
                        value: COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH,
                        message:  t("Manage_User.Validation.Password_Max_Validation", { max : COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH}),
                      },
                    }),
                }}
                placeholder={t("Manage_User.Add_Edit_User_Drawer.Password_Placeholder")}
                required={!isEditMode}
                fullWidth
              />
            </Grid>

            <Grid item xs={12} md={6}>
              <CustomTextField
                name="confirmPassword"
                label={isEditMode ? t("Manage_User.Add_Edit_User_Drawer.Confirm_Password") : <span>{t("Manage_User.Add_Edit_User_Drawer.Confirm_Password")} <span className="star-error">*</span></span>}
                control={control}
                type={"password"}
                rules={{
                  ...(isEditMode
                    ? {
                      validate: (value: string) =>
                        value === password ||
                        t("Manage_User.Validation.Confirm_Password_Match"),
                    }
                    : {
                      required:t("Manage_User.Validation.Confirm_Password_Required"),
                      validate: (value: string) =>
                        value === password ||
                        t("Manage_User.Validation.Confirm_Password_Match"),
                    }),
                }}
                placeholder={t("Manage_User.Add_Edit_User_Drawer.Confirm_Password_Placeholder")}
                required={!isEditMode}
                fullWidth
              />
            </Grid>

            <Grid item xs={12} className="roles-user">
              <CustomMultiSelect
                name="roleIds"
                control={control}
                label={<span>{t("Manage_User.Add_Edit_User_Drawer.Roles")} <span className="star-error">*</span></span>}
                options={Rolelist}
                rules={{ required:  t("Manage_User.Validation.Role_Required") }}
                required
                placeholder={t("Manage_User.Add_Edit_User_Drawer.Roles_Placeholder")}
              />
            </Grid>

            <CustomButton fullWidth className="common-btn-design">
              {isEditMode ? t("Save_btn") : t("Add_btn")}
            </CustomButton>

          </Grid>
        </Box>
      </div>
    </div>
  );
};

export { UserAddEditForm };
