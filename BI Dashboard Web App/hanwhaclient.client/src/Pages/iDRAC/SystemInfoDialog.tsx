import { Box, Dialog, DialogContent, DialogTitle, IconButton } from '@mui/material';
import React, { useEffect, useState } from 'react'
import CloseIcon from "@mui/icons-material/Close";
import { IIdracSystemInfoPayload, ISystemInformation } from '../../interfaces/IManageiDRAC';
import { fetchSystemInfo } from '../../services/iDRACService';
import { useTranslation } from 'react-i18next';

interface Props {
    open: boolean;
    onClose: () => void;
    serverIP: string;
}
const SystemInfoDialog: React.FC<Props> = ({ open, onClose, serverIP }) => {
    const [systemInfo, setSystemInfo] = useState<ISystemInformation | null>(null);
    const { t } = useTranslation();

    useEffect(() => {
        if (open && serverIP) {
            getSystemInfo();
        }
    }, [open, serverIP,]);

    const getSystemInfo = async () => {

        try {
            const payload: IIdracSystemInfoPayload = {
                ipAddress: serverIP,
            };

            const response: any = await fetchSystemInfo(payload);
            if (response.isSuccess) {
                setSystemInfo(response?.data);
            }
            // const dummySystemInfoResponse: ISystemInformation = {
            //     powerState: "On",
            //     model: "PowerEdge R750",
            //     hostName: "GDOT789BDSR",
            //     operatingSystem: "Microsoft Windows Server 2022 Standard",
            //     operatingSystemVersion: "10.0.20349 Build 20348 (64-bit)",
            //     serviceTag: "3YNM6F4",
            //     biosVersion: "2.7.5",
            //     idracFirmwareVersion: "7.20.60.60",
            //     ipAddress: "192.168.0.60",
            //     idracMacAddress: "fc:4c:ea:6e:09:1b",
            //     license: "Enterprise Edit",
            // };
            // setSystemInfo(dummySystemInfoResponse)
        } catch (error) {
            console.error("Failed to fetch event logs", error);
        }
    };


    return (
        <Dialog className="inner-cmn-pop-design" open={open} onClose={onClose} fullWidth >

            <DialogTitle className="inner-pop-head">
                {t("iDRAC_SystemInformation.System_Information")}
                <IconButton onClick={onClose}>
                    <CloseIcon />
                </IconButton>
            </DialogTitle>

            <DialogContent>
                <Box className="system-info-wrapper">
                    <Box className="system-info-card">
                        <Box className="system-info-row">
                            {t("iDRAC_SystemInformation.Power_State")}: <span className="system-info-value"> {systemInfo?.powerState || '-'}</span>
                        </Box>

                        <Box className="system-info-row">
                            {t("iDRAC_SystemInformation.Model")}:  <span className="system-info-value">{systemInfo?.model || '-'}</span>
                        </Box>

                        <Box className="system-info-row">
                            {t("iDRAC_SystemInformation.Host_Name")}:  <span className="system-info-value">{systemInfo?.hostName || '-'}</span>
                        </Box>

                        <Box className="system-info-row">
                            {t("iDRAC_SystemInformation.Operating_System")}:  <span className="system-info-value">{systemInfo?.operatingSystem || '-'}</span>
                        </Box>

                        <Box className="system-info-row">
                            {t("iDRAC_SystemInformation.Operating_System_Version")}:  <span className="system-info-value">{systemInfo?.operatingSystemVersion || '-'}</span>
                        </Box>

                        <Box className="system-info-row">{t("iDRAC_SystemInformation.Service_Tag")}:  <span className="system-info-value">{systemInfo?.serviceTag || '-'}</span>
                        </Box>

                        <Box className="system-info-row">
                            {t("iDRAC_SystemInformation.BIOS_Version")}:  <span className="system-info-value">{systemInfo?.biosVersion || '-'}</span>
                        </Box>

                        <Box className="system-info-row">
                            {t("iDRAC_SystemInformation.iDRAC_Firmware_Version")}:  <span className="system-info-value">{systemInfo?.idracFirmwareVersion || '-'}</span>
                        </Box>

                        <Box className="system-info-row">
                            {t("iDRAC_SystemInformation.IP_Address")}:  <span className="system-info-value">{systemInfo?.ipAddress || '-'}</span>
                        </Box>

                        <Box className="system-info-row">
                            {t("iDRAC_SystemInformation.iDRAC_MAC_Address")}:  <span className="system-info-value">{systemInfo?.idracMacAddress || '-'}</span>
                        </Box>

                        <Box className="system-info-row">
                            {t("iDRAC_SystemInformation.License")}:  <span className="system-info-value">{systemInfo?.license || '-'}</span>
                        </Box>
                    </Box>
                </Box>
            </DialogContent>

        </Dialog>
    )
}

export default SystemInfoDialog