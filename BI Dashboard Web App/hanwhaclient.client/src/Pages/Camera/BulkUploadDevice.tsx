import { Box, Grid, IconButton, Paper, Stack, Typography } from "@mui/material";
import React, { useRef, useState } from "react";
import { CustomButton } from "../../components//Reusable/CustomButton";
import AddIcon from "@mui/icons-material/Add";
import CloudUploadOutlinedIcon from "@mui/icons-material/CloudUploadOutlined";
import { showToast } from "../../components/Reusable/Toast";

import { UploadBulkDevice } from "../../services/cameraService";
import { useTranslation } from "react-i18next";

type BulkUploadDeviceProps = {
  onClose: () => void;
  refreshData: () => void;
};

const BulkUploadDevice: React.FC<BulkUploadDeviceProps> = ({
  onClose,
  refreshData,
}) => {
  const [deviceFile, setDeviceFile] = useState<File | null>(null);
  const [previewLicenseFile, setPreviewLicenseFile] = useState<string | null>(
    null
  );
  const licenseFileInputRef = useRef<HTMLInputElement | null>(null);
  const { t } = useTranslation();

  const handleSaveFiles = async () => {
    if (!deviceFile) {
      showToast("Please upload device files.", "error");
      return;
    }

    // Validate file extensions
    const deviceFileName = deviceFile.name.toLowerCase();

    const validExtensions = [".xls", ".xlsx"];

    const isValid = validExtensions.some((ext) => deviceFileName.endsWith(ext));

    if (!isValid) {
      showToast(
        "Invalid file type. Please upload an Excel file (.xls or .xlsx).",
        "error"
      );
      return;
    }

    const formData = new FormData();
    formData.append("file", deviceFile);

    try {
      const data = await UploadBulkDevice(formData);
      if (
        typeof data === "object" &&
        data !== null &&
        "isSuccess" in data &&
        data.isSuccess
      ) {
        onClose();
        refreshData();
        //setShowLogoutDialog(true);
      } else {
        const errorMessage =
          typeof data === "object" &&
          data !== null &&
          "message" in data &&
          typeof data.message === "string"
            ? data.message
            : "Invalid license signature.";
        //showToast(errorMessage, "error");
      }
    } catch (error) {
      showToast("Failed to upload files. Please try again.", "error");
      console.error("Upload Error:", error);
    } finally {
      setDeviceFile(null);
      setPreviewLicenseFile(null);

      if (licenseFileInputRef.current) licenseFileInputRef.current.value = "";
    }
  };

  const handleBulkDeviceUpload = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const file = event.target.files?.[0];
    if (file) {
      setDeviceFile(file);
      setPreviewLicenseFile(URL.createObjectURL(file));
    }
  };

  const handleDeviceFileEdit = () => {
    setDeviceFile(null);
    setPreviewLicenseFile(null);
    if (licenseFileInputRef.current) {
      licenseFileInputRef.current.value = "";
    }
  };

  return (
    <Paper className="add-license-pop">
      <Grid className="add-license-pop-wrapper">
        <Grid item xs={10} md={10} lg={10}>
          <Typography variant="h6" fontWeight={600}>
            {t(
              "Manage_Device.Upload_Bulk_Device_Drawer.Upload_your_device_file"
            )}
          </Typography>

          <Box className="upload-your-file">
            {deviceFile ? (
              <Box className="upload-your-file-wrapper">
                <div className="upload-your-file-name">
                  <img src="../images/file.png" alt="logo preview" />
                  <Typography variant="subtitle1" gutterBottom>
                    {t(
                      "Manage_Device.Upload_Bulk_Device_Drawer.Selected_File",
                      {
                        fileName: deviceFile.name,
                      }
                    )}
                  </Typography>
                </div>
                <IconButton onClick={handleDeviceFileEdit}>
                  <img src="../images/CTA.png" alt="cta" />
                </IconButton>
              </Box>
            ) : (
              <Box
                // onClick={() =>
                //   document.getElementById("license-file-upload-input")?.click()
                // }
                onClick={() => {
                  if (licenseFileInputRef.current) {
                    licenseFileInputRef.current.value = ""; // force reset input
                    licenseFileInputRef.current.click();
                  }
                }}
                sx={{
                  display: "flex",
                  flexDirection: "column",
                  alignItems: "center",
                  justifyContent: "center",
                  width: "100%",
                  height: "100%",
                  cursor: "pointer",
                }}
              >
                <CloudUploadOutlinedIcon
                  sx={{ fontSize: 36, color: "#9e9e9e" }}
                />
                <Typography variant="body2" mt={1}>
                   {t("Manage_Device.Upload_Bulk_Device_Drawer.Browse_and_choose_files_line1")} <br />
                   {t("Manage_Device.Upload_Bulk_Device_Drawer.Browse_and_choose_files_line2")}
                </Typography>
                <IconButton
                  sx={{
                    background: "linear-gradient(to right, #FF8A00, #FE6500)",
                    color: "#fff",
                    mt: 1,
                    "&:hover": { background: "#FE6500" },
                  }}
                >
                  <AddIcon />
                </IconButton>
              </Box>
            )}
            <input
              type="file"
              accept=".xls,.xlsx"
              id="license-file-upload-input"
              hidden
              onChange={handleBulkDeviceUpload}
              ref={licenseFileInputRef}
            />
          </Box>
          {/* <Box sx={{ mt: 2, display: "flex", justifyContent: "flex-start" }}>
            <CustomButton onClick={handleLicenseFileSave}>Save</CustomButton>
          </Box> */}
        </Grid>

        <Grid item xs={10} md={10} lg={10}>
          <CustomButton
            className="common-btn-design"
            onClick={handleSaveFiles}
            fullWidth={true}
          >
            {t("Manage_Device.Upload_Bulk_Device_Drawer.Upload")}
          </CustomButton>
        </Grid>
      </Grid>
    </Paper>
  );
};

export { BulkUploadDevice };
