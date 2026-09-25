import React, { useEffect, useRef, useState } from "react";
import {
  Box,
  Button,
  Typography,
  IconButton,
  Stack,
  Paper,
  Grid,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import CloudUploadOutlinedIcon from "@mui/icons-material/CloudUploadOutlined";
import { CustomButton } from "../Reusable/CustomButton";
import {
  GetClientSettingsData,
  uploadCustomerLogo,
  uploadsslCertificateService,
} from "../../services/settingService";
import { HasPermission } from "../../utils/screenAccessUtils";
import { COMMON_CONSTANTS, LABELS, REGEX } from "../../utils/constants";
import { CustomTextField } from "../Reusable/CustomTextField";
import { useForm, Controller } from "react-hook-form";
import { set } from "date-fns";
import { Trans, useTranslation } from "react-i18next";

interface UploadSSLCertificateProps {
  sslCertificateFileName: string | null;
}

const UploadSSLCertificate: React.FC<UploadSSLCertificateProps> = ({
  sslCertificateFileName,
}) => {
  const fileInputRefSsl = useRef<HTMLInputElement | null>(null);
  const [fileName, setFileName] = useState<string | null>(null);
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState(false);
  const [errorMsg, setErrorMsg] = useState("");
  const [isEdit, setIsEdit] = useState(false);

  const {
    control,
    formState: { errors },
    watch,
    trigger,
  } = useForm<SmtpFormValues>({
    defaultValues: {
      password: "",
    },
  });
  const { t } = useTranslation();

  const password = watch("password");

  useEffect(() => {
    setFileName(sslCertificateFileName);
  }, [sslCertificateFileName]);

  const handleUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0];
    if (!selectedFile) return;

    const fileName = selectedFile.name.toLowerCase();
    const allowedExtensions = [".crt", ".pem", ".pfx", ".cer"];
    const isValid = allowedExtensions.some((ext) => fileName.endsWith(ext));

    if (!isValid) {
      setError(true);
      setErrorMsg(t("General_Settings.Upload_SSL.Allowed_File_Validation"));
      e.target.value = "";
      setFile(null); // Clear any previously stored file
      return;
    }

    setFile(selectedFile);
    setFileName(selectedFile.name);
    setError(false);
    setErrorMsg("");
  };

  const handleSave = async () => {
    // If editing but no new file uploaded → show error

    const isPasswordValid = await trigger("password");

    if (isEdit && !file) {
      setError(true);
      setErrorMsg(t("General_Settings.Upload_SSL.Upload_File_first"));
      return;
    }

    if (!file) {
      setError(true);
      setErrorMsg(t("General_Settings.Upload_SSL.Upload_File_first"));
      return;
    }

    // If not editing and no file uploaded → no error, just return
    if (!isEdit && !file) {
      return;
    }

    if (!isPasswordValid) {
      // You already render field-level errors via CustomTextField,
      // so just stop here; RHF will show the message next to the field.
      return;
    }

    const formData = new FormData();
    formData.append("password", password);

    if (file) {
      formData.append("CertificateFile", file);
    }

    //pass the "certificateFile" instead of file on param

    try {
      const response = await uploadsslCertificateService(formData);

      if (typeof response !== "string" && response?.isSuccess) {
        const generalSetting: any = await GetClientSettingsData();
        if (generalSetting != undefined) {
          setFileName(generalSetting.sslCertificateFileName);
          setFile(null);
          setError(false);
          setErrorMsg("");
          setIsEdit(false);
          // setFileName("");
        }
      }
    } catch (error) {
      console.error(
        `Failed to upload the ssl certificate file. Please try again.`,
        error
      );
    } finally {
      if (fileInputRefSsl.current) {
        fileInputRefSsl.current.value = "";
      }
    }
  };

  const handleEdit = () => {
    setIsEdit(true);
    if (error) {
      setError(false);
      setErrorMsg("");
    }
    setFile(null);
    setFileName("");

    if (fileInputRefSsl.current) {
      fileInputRefSsl.current.value = "";
    }
  };

  type SmtpFormValues = {
    password: string;
  };

  return (
    <Paper elevation={1} className="smtp-setup-wrapper">
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        mb={2}
      >
        <Typography variant="h4">{t("General_Settings.Upload_SSL.Upload_SSL_Title")}</Typography>
        {HasPermission(LABELS.CanUploadClientLogo) && (
          <CustomButton onClick={handleSave} className="common-btn-design">
            {t("Save_btn")}
          </CustomButton>
        )}
      </Stack>

      <Grid className="upload-profile-main" container spacing={2}>
        <Grid className="upload-profile-main-wrapper" item xs={12} md={4}>
          {fileName ? (
            <Box className="upload-your-file">
              <Box className="upload-your-file-wrapper">
                <Box className="upload-your-file-name">
                  <img src="../images/file.png" alt="logo preview" />
                  <Typography variant="h6">{fileName}</Typography>
                </Box>
                <IconButton onClick={handleEdit}>
                  <img src="../images/CTA.png" alt="cta" />
                </IconButton>
              </Box>
            </Box>
          ) : (
            <Box
              onClick={() =>
                document.getElementById("ssl-upload-input")?.click()
              }
              className="upload-pro-inner"
            >
              <CloudUploadOutlinedIcon />
              <Typography variant="body2">
                <Trans i18nKey="General_Settings.Upload_Logo.Browse_Upload_Text" />
              </Typography>
              <IconButton>
                <AddIcon />
              </IconButton>
            </Box>
          )}
          <input
            type="file"
            accept=".crt,.pem,.pfx,.cer"
            id="ssl-upload-input"
            hidden
            onChange={handleUpload}
            ref={fileInputRefSsl}
          />
          {error && (
            <Typography
              variant="caption"
              sx={{
                color: "red !important",
                mt: "4px",
                ml: "15px",
                fontSize: "12px",
              }}
            >
              {errorMsg}
            </Typography>
          )}
        </Grid>
        <Grid item xs={12} md={4}>
          <CustomTextField
            control={control}
            name="password"
            label={
              <span>
                { t("Manage_User.Add_Edit_User_Drawer.Password")} <span className="star-error">*</span>
              </span>
            }
            type="password"
            rules={{
              required: t("Manage_User.Validation.Password_Required")
              // pattern: {
              //   value: REGEX.Password_Regex,
              //   message:
              //     "Password must be at least 8 characters long and include one uppercase letter, one lowercase letter, one number, and one special character (excluding *)",
              // },
              // maxLength: {
              //   value: COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH,
              //   message: `Password cannot exceed ${COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH} characters`,
              // },
            }}
            required
            placeholder={t("Manage_User.Add_Edit_User_Drawer.Password_Placeholder")}
            autoComplete="new-password"
          />
        </Grid>
      </Grid>
    </Paper>
  );
};

export { UploadSSLCertificate };
