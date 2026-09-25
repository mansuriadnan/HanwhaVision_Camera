import React, { useEffect } from "react";
import { Box, Tooltip } from "@mui/material";
import { IServerManagement, ServerAddEditFormProps} from "../../interfaces/ISettings";
import { CustomTextField } from "../../components/Reusable/CustomTextField";
import { CustomButton } from "../../components/Reusable/CustomButton";
import { FetchDBStringService, SaveServerDataService } from "../../services/settingService";
import { COMMON_CONSTANTS, REGEX } from "../../utils/constants";
import { SubmitHandler, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { LocalLoader, showToast } from "../../components";

const AddEditServer: React.FC<ServerAddEditFormProps> = ({
  onClose,
  serverData,
  refreshData,
}) => {
  const { t } = useTranslation();
  const [isFetching, setIsFetching] = React.useState(false);
  const isEditMode = serverData !== null && serverData !== undefined;

  const {
    control,
    getValues,
    setValue,
    handleSubmit,
    trigger,
    formState: { isDirty },
  } = useForm<IServerManagement>({
    defaultValues: {
      serverName: "",
      databaseConnectionString: "",
      hostingAddress: "",
      username: "",
      password: "",
    },
  });

  useEffect(() => {
    if (isEditMode && serverData) {
      setValue("serverName", serverData?.serverName);
      setValue("databaseConnectionString", serverData?.databaseConnectionString || "");
      setValue("hostingAddress", serverData?.hostingAddress || "");
      setValue("username", serverData?.username || "");
      setValue("password", serverData?.password || "");
      
    }
  }, [isEditMode, serverData]);

 const handleFetchDBString = async () => {
  
   const isValid = await trigger(["hostingAddress", "username", "password"]);

   if (!isValid) return; // show existing field-level validation messages

   const { hostingAddress, username, password } = getValues();

  try {
    setIsFetching(true);

    const result = await FetchDBStringService({
      hostingAddress,
      username,
      password,
    });

    if (result && typeof result !== "string" && result.isSuccess && result.data) {
      setValue("databaseConnectionString", result.data as string, { shouldDirty: true });
    }else{
      showToast(result.toString(), "error");
    }
  } catch (error) {
    console.error("Failed to fetch DB string:", error);
  } finally {
    setIsFetching(false);
  }
};

  const addServer: SubmitHandler<IServerManagement> = async (data) => {
    const tempServerData = {
      serverName: data.serverName,
      databaseConnectionString: data.databaseConnectionString,
      hostingAddress: data.hostingAddress,
      username: data.username,
      password: data.password,
      ...(isEditMode && serverData?.id && { id: serverData.id }),
    };

    var result: any = await SaveServerDataService(
      tempServerData as IServerManagement,
    );

    if (result && result.isSuccess) {
      onClose();
      refreshData();
    }
  };

  return (
    <div className="cmn-pop-form">
      <div className="cmn-pop-form-wrapper">
        <Box
          component="form"
          onSubmit={handleSubmit(addServer)}
          noValidate
          sx={{ width: "100%" }}
        >
          <div className="cmn-pop-form-inner">
             <CustomTextField
              name="serverName"
              label={
                <span>
                  {t("ServerManagement.Server_Name")}<span className="star-error">*</span>
                </span>
              }
              control={control}
              rules={{
                required: t("ServerManagement.Server_Name_required"),
                maxLength: {
                  value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                  message: t("ServerManagement.Server_Name_length_validation", { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                },
              }}           
              placeholder={t("ServerManagement.Server_Name_placeholder")}
              required
              fullWidth
            />
            {/* <CustomTextField
              name="databaseConnectionString"
              label={
                <span>
                   {t("ServerManagement.Database_Connection_String")}<span className="star-error">*</span>
                </span>
              }
              control={control}
              rules={{
                required: t("ServerManagement.Database_Connection_String_required"),
              }}
              placeholder={t("ServerManagement.Database_Connection_String_placeholder")}
              required
              fullWidth
            /> */}
              <CustomTextField
                name="hostingAddress"
                label={
                  <span>
                    {t("ServerManagement.Hosting_Address")}<span className="star-error">*</span>
                  </span>
                }
                control={control}
                rules={{
                  required: t("ServerManagement.Hosting_Address_required"),
                  pattern: {
                    value: REGEX.IPAddress_With_HTTP_Regex,
                    message: t(
                      "ServerManagement.Hosting_Address_validation"
                    ),
                  },
                }}
                placeholder={t("ServerManagement.Hosting_Address_placeholder")}
                required
                fullWidth
              />

            <CustomTextField
              name="username"
              label={
                <span>
                  {t("ServerManagement.Username")}<span className="star-error">*</span>
                </span>
              }
              control={control}
              rules={{
                required: t("ServerManagement.Username_required"),
                maxLength: {
                  value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                  message: t("ServerManagement.Username_length_validation", { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                },

              }}
              placeholder={t("ServerManagement.Username_placeholder")}
              required
              fullWidth
            />
            <CustomTextField
              name="password"
              label={
                <span>
                  {t("ServerManagement.Password")}<span className="star-error">*</span>
                </span>
              }
              control={control}
              type={"password"}
              rules={{
                required: t("ServerManagement.Password_required"),
              }}
              placeholder= {t("ServerManagement.Password_placeholder")}
              required
              fullWidth
            />

             {/* DB String Field + Fetch Button Row */}
            <Box sx={{ display: "flex", alignItems: "flex-start", gap: 1, width: "100%" }}>
              <Box sx={{ flex: 1 }}>
                <CustomTextField
                  name="databaseConnectionString"
                  label={
                    <span>
                      {t("ServerManagement.Database_Connection_String")}<span className="star-error">*</span>
                    </span>
                  }
                  control={control}
                  rules={{
                    required: t("ServerManagement.Database_Connection_String_required"),
                  }}
                  placeholder={t("ServerManagement.Database_Connection_String_placeholder")}
                  required
                  fullWidth
                />
              </Box>             
                <span>
                  <CustomButton
                    onClick={handleFetchDBString}
                    disabled={isFetching}
                    size="small"
                    variant="text"
                    className="auto-fetch-link"
                  >
                    {isFetching
                      ? <LocalLoader width={20} height={20} size={20} color="warning" />
                      : "auto fetch"
                    }
                  </CustomButton>
                </span>
           
            </Box>

            <CustomButton
              className="common-btn-design"
              fullWidth
              customStyles={{ mt: 2 }}
              disabled={isEditMode && !isDirty}
            >
              {isEditMode ? t("Update") : t("Add_btn")}
            </CustomButton>
          </div>
        </Box>
      </div>
    </div>
  );
};

export { AddEditServer };
