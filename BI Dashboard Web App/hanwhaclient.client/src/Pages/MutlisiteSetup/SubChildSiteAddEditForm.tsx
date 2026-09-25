import { Box, Container, FormControl, Typography } from "@mui/material"
// import { CustomButton, CustomTextField, showToast } from "../../components"
import { CustomButton } from "../../components/Reusable/CustomButton"
import { CustomTextField } from "../../components/Reusable/CustomTextField"
import { showToast } from "../../components/Reusable/Toast"
import { useEffect, useRef, useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";
import { ChildSite, ISite } from "../../interfaces/IMultiSite";
import { REGEX } from "../../utils/constants";
import handleResponse from "../../utils/handleResponse";
import { AddChildSiteService, AddSubChildSiteService, UpdateChildSiteService, UpdateSubChildSiteService } from "../../services/siteManagementService";
import { t } from "i18next";

interface SiteAddEditFormProps {
    onClose: () => void;
    sites?: ISite;
    subSites?: ChildSite;
    refreshData: () => void;
}

interface SiteFormInputs {
    siteName: string;
    subsiteName: string;
    hostingAddress: string;
    userName: string;
    password: string;

}

const SubChildSiteAddEditForm: React.FC<SiteAddEditFormProps> = ({ onClose, sites, subSites, refreshData }) => {
    // const [error, setError] = useState<string | null>(null);
    const isEditMode = subSites !== null && subSites !== undefined;
    const {
        control,
        setValue,
        handleSubmit,
        watch,
        formState: { errors },
    } = useForm<SiteFormInputs>({
        defaultValues: {
            siteName: sites?.siteName || "",
            subsiteName: "",
            hostingAddress: "",
            userName: "",
            password: ""
        },
    });
    const childSiteNameRef = useRef<HTMLInputElement | null>(null);

    useEffect(() => {
        if (childSiteNameRef.current) {
            childSiteNameRef.current.focus();
        }
    }, []);

    useEffect(() => {
        if (isEditMode && subSites) {

            const initialSubChildSite = subSites;
            setValue("subsiteName", initialSubChildSite?.siteName);
            setValue("hostingAddress", initialSubChildSite?.hostingAddress);
            setValue("userName", initialSubChildSite?.username);
            setValue("password", initialSubChildSite?.password);
        }

    }, [isEditMode, subSites]);

    const subsiteSubmit: SubmitHandler<SiteFormInputs> = async (data) => {
        const subChildSiteData =
        {
            parentSiteId: sites?.id,
            siteName: data.subsiteName,
            hostingAddress: data.hostingAddress,
            username: data.userName,
            password: data.password,
            ...(isEditMode &&
                subSites?.id && { id: subSites.id }),
        };

        if (isEditMode) {
            const updateSubChildSite = async () => {
                // setError(null); // Reset error
                try {
                    const data = await UpdateSubChildSiteService(subChildSiteData);
                    if (typeof data === 'object' && data !== null && 'isSuccess' in data && data.isSuccess) {
                        onClose();
                        refreshData();
                    }

                } catch (err: any) {
                    // setError(err.message || "An error occurred while fetching users.");
                    showToast("An error occurred while updating sub child site.", "error");
                }
            };
            updateSubChildSite();
        } else {

            const AddSubChildSite = async () => {
                // setError(null); // Reset error
                try {
                    const data = await AddSubChildSiteService(subChildSiteData);
                    if (typeof data === 'object' && data !== null && 'isSuccess' in data && data.isSuccess) {
                        onClose();
                        refreshData();
                    }

                } catch (err: any) {
                    // setError(err.message || "An error occurred while fetching users.");
                    showToast("An error occurred while adding sub child site.", "error");
                }
            };
            AddSubChildSite();
        }
    }

    return (
        <Box className='cmn-pop-form'>
            <div className="cmn-pop-form-wrapper">
                {/* {error && (
                    <Typography color="error" sx={{ mt: 2 }}>
                        {error}
                    </Typography>
                )} */}

                <Box
                    component="form"
                    onSubmit={handleSubmit(subsiteSubmit)}
                    noValidate
                    sx={{ width: "100%", mt: '2rem' }}
                >
                    <Box className="cmn-pop-form-inner">

                        <CustomTextField
                            name="siteName"
                            label={<span>{t("Multisite_Setup.Add_Child_Drawer.Child_Site_Name")} <span className="star-error">*</span></span>}
                            control={control}
                            rules={{
                                required: t("Multisite_Setup.Validation.Child_Site_Required"),
                            }}
                            placeholder={t("Multisite_Setup.Add_Child_Drawer.Site_Name_Placeholder")}
                            required
                            fullWidth
                            disabled
                            className="disbled-child-site"                           
                        />

                        <CustomTextField
                            name="subsiteName"
                            label={<span>{t("Multisite_Setup.Add_Child_Drawer.Sub_Site_Name")} <span className="star-error">*</span></span>}
                            control={control}
                            rules={{
                                required: t("Multisite_Setup.Validation.Sub_Site_Name_Required"),
                            }}
                            placeholder={t("Multisite_Setup.Add_Child_Drawer.Sub_Site_Name_Placeholder")}
                            required
                            fullWidth
                            inputRef={childSiteNameRef}
                        />
                        <CustomTextField
                            name="hostingAddress"
                            label={<span>{t("Multisite_Setup.Add_Child_Drawer.Hosting_Address")} <span className="star-error">*</span></span>}
                            control={control}
                            rules={{
                                required: t("Multisite_Setup.Validation.Hosting_Address_Required"),
                                pattern: {
                                    value: REGEX.IPAddressURL_Regex,
                                    message: t("Multisite_Setup.Validation.Hosting_Address_Strong_validation"),
                                },
                            }}
                            placeholder={t("Multisite_Setup.Add_Child_Drawer.Hosting_Address_Placeholder")}
                            required
                            fullWidth
                        />
                        <CustomTextField
                            name="userName"
                            label={<span>{t("Multisite_Setup.Add_Child_Drawer.Username")} <span className="star-error">*</span></span>}
                            control={control}
                            rules={{
                                required: t("Multisite_Setup.Validation.Username_Required"),
                                pattern: {
                                    value: REGEX.UserName_Regex,
                                    message: t("Multisite_Setup.Validation.Username_Strong_Validation"),
                                },
                            }}
                            placeholder={t("Multisite_Setup.Add_Child_Drawer.Username_Placeholder")}
                            required
                            fullWidth
                        />
                        <CustomTextField
                            name="password"
                            label={<span>{t("Multisite_Setup.Add_Child_Drawer.Password")} <span className="star-error">*</span></span>}
                            control={control}
                            type={"password"}
                            placeholder={t("Multisite_Setup.Add_Child_Drawer.Password_Placeholder")}
                            rules={{
                                required: t("Multisite_Setup.Validation.Password_Required"),
                                // pattern: {
                                //     value: REGEX.Password_Regex,
                                //     message: "Enter a valid Password",
                                // }
                            }}
                        />

                        <CustomButton fullWidth className="common-btn-design" customStyles={{ mt: 2 }}>
                            {isEditMode ?  t("Update") : t("Add_btn")}
                        </CustomButton>
                    </Box>
                </Box>
            </div>
        </Box>

    )
}

export { SubChildSiteAddEditForm }