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
import EditIcon from "@mui/icons-material/Edit";
import { showToast } from "../Reusable/Toast";
import { GetClientSettingsData, uploadCustomerLogo } from "../../services/settingService";
import { HasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { useSettingsContext } from "../../context/SettingContext";
import { Trans, useTranslation } from "react-i18next";

interface UploadCustomerLogoProps {
  logoUploaded: string;
}

const UploadCustomerLogo: React.FC<UploadCustomerLogoProps> = ({ logoUploaded }) => {
  const [logo, setLogo] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const { saveSettings, settings } = useSettingsContext();
  const [fileName, setFileName] = useState<string | null>(null);
  const [logoError, setLogoError] = useState(false);
  const [logoErrorMsg, setLogoErrorMsg] = useState("");
  const { t } = useTranslation();

  useEffect(() => {
    if (logoUploaded) {
      setPreview(logoUploaded)
    }
  }, [logoUploaded])

  const handleUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (file) {
      const fileName = file.name.toLowerCase();
      const allowedExtensions = ["jpg", "jpeg", "png"];
      const isValid = allowedExtensions.some(ext => fileName.endsWith(ext));

      if (!isValid) {
        event.target.value = "";
        setLogo(null);
        setPreview("");
        setFileName("");
        setLogoError(true);
        setLogoErrorMsg(t("General_Settings.Upload_Logo.Validation.Allowed_File_Validation"))
        return;
      }
      
      if (file.size > (1 * 1024 * 1024)) { // check if file size exceeds 1 MB
        event.target.value = "";
        setLogo(null);
        setPreview("");
        setFileName("");
        setLogoError(true);
        setLogoErrorMsg(t("General_Settings.Upload_Logo.Validation.Valid_File_size"));
        return;
      }
      
      setLogo(file);
      setPreview(URL.createObjectURL(file));
      setFileName(file.name);
      setLogoError(false);
      setLogoErrorMsg("")
    }

  };

  const handleSave = async () => {
    if (!logo) {
      setLogoError(true);
      setLogoErrorMsg( t("General_Settings.Upload_Logo.Validation.Upload_logo_first"))
      return;
    } else {
      setLogoError(false);
      setLogoErrorMsg("")
    }

    const formData = new FormData();
    formData.append("file", logo);

    try {
      const response = await uploadCustomerLogo(formData);

      if (typeof response !== "string" && response?.isSuccess) {

        const generalSetting: any = await GetClientSettingsData();
        setPreview(generalSetting?.logo);
        setFileName(null)
        setLogo(null);
        if (generalSetting != undefined) {
          saveSettings(generalSetting);

        }
      }
    } catch (error) {
      setLogo(null);
      setPreview(null);
      console.error(`Failed to upload the file. Please try again.`, error);
    } finally {


      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  };

  const handleEdit = () => {
    setLogo(null);
    setPreview(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  return (
    <Paper elevation={1} className='smtp-setup-wrapper'>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        mb={2}
      >
        <Typography variant="h4">
          {t("General_Settings.Upload_Logo.Upload_logo_title")}
        </Typography>
        {HasPermission(LABELS.CanUploadClientLogo) && (
          <CustomButton onClick={handleSave} className="common-btn-design">{t("Save_btn")}</CustomButton>
        )}
      </Stack>


      <Grid className="upload-profile-main" container spacing={2}>
        <Grid item xs={12} md={4}>
          <Grid className="upload-profile-main-wrapper">

            {preview ? (
              <Box className="upload-your-file">
                <Box className="upload-your-file-wrapper">
                  <Box className="upload-your-file-name upload-your-file-name-image">
                    <img
                      src={preview}
                      alt="logo preview"

                    />
                  </Box>
                  <Typography variant="body2"> {fileName}</Typography>
                  <IconButton
                    onClick={handleEdit}
                  >
                    <img src="../images/CTA.png" alt="cta" />
                  </IconButton>

                </Box>
              </Box>

            ) : (
              <Box
                onClick={() =>
                  document.getElementById("logo-upload-input")?.click()
                }
                className="upload-pro-inner upload-pro-inner-general"
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
              accept="image/*"
              id="logo-upload-input"
              hidden
              onChange={handleUpload}
              ref={fileInputRef}
            />
            {logoError && (
              <Typography
                variant="caption"
                sx={{ color: "red !important", mt: "4px", ml: "15px", fontSize: "12px" }}
              >
                {logoErrorMsg}
              </Typography>
            )}
          </Grid>
        </Grid>
        <Grid item xs={12} md={8} >
          {/* <Typography  sx={{ fontSize: "14px" }}>
            <span style={{ color: '#FF8A01' }}>*</span> Please upload your company <b style={{ color: '#FF8A01' }}>logo in PNG or JPG</b> format with a recommended resolution of <b style={{ color: '#FF8A01' }}>174 x 46 pixels</b>.<br />
            Maximum file size allowed: <b style={{ color: '#FF8A01' }}>1 MB</b>.
            <br /><br />
            For best results, use a high-quality image with a white background.
          </Typography> */}

          <Typography sx={{ fontSize: "14px" }}>
            <span style={{ color: '#FF8A01' }}>*</span>{" "}
            {t("General_Settings.Upload_Logo.Upload_Note.Line1_Part1")}{" "}
            <b style={{ color: '#FF8A01' }}>
              {t("General_Settings.Upload_Logo.Upload_Note.Line1_Highlight1")}
            </b>{" "}
            {t("General_Settings.Upload_Logo.Upload_Note.Line1_Part2")}{" "}
            <b style={{ color: '#FF8A01' }}>
              {t("General_Settings.Upload_Logo.Upload_Note.Line1_Highlight2")}
            </b>
            <br />
            {t("General_Settings.Upload_Logo.Upload_Note.Line2_Part1")}{" "}
            <b style={{ color: '#FF8A01' }}>
              {t("General_Settings.Upload_Logo.Upload_Note.Line2_Highlight")}
            </b>.
            <br /><br />
            {t("General_Settings.Upload_Logo.Upload_Note.Line3")}
          </Typography>
        </Grid>

      </Grid>
    </Paper>
  );
};

export { UploadCustomerLogo };
