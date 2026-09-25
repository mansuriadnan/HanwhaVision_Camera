import { Grid, Paper, Stack, Typography } from '@mui/material'
import React, { useEffect } from 'react'
import { CustomButton } from '../Reusable/CustomButton'
import { CustomTextField } from '../Reusable/CustomTextField'
import { useForm } from 'react-hook-form'
import { saveGoogleMapApIKeySetting } from '../../services/settingService'
import { HasPermission } from '../../utils/screenAccessUtils'
import { LABELS } from '../../utils/constants'
import { t } from 'i18next'

type googleMapKeyFormValues = {
    googlemapkey: string;
};

const GoogleMapApiKey = ({ googleApiKey }) => {

    const {
        control,
        handleSubmit,
        reset,
        formState: { isSubmitting },
    } = useForm<googleMapKeyFormValues>({
        defaultValues: {
            googlemapkey: ""
        },
    });

    useEffect(() => {
        if (googleApiKey) {
            reset({
                googlemapkey: googleApiKey ?? "",
            });
        }
    }, [googleApiKey, reset]);
    const onSubmit = async (data: googleMapKeyFormValues) => {

        try {
            const payload = {
                apiKey: data.googlemapkey,
            };

            const response = await saveGoogleMapApIKeySetting(payload);
        } catch (error) {
            console.error("Error during save:", error);
        }
    };


    return (
        <Paper className="smtp-setup-wrapper">
            <Stack>
                <Typography variant="h4">
                    {t("General_Settings.Google_Map_Key.Title")}
                </Typography>
                {HasPermission(LABELS.CanGoogleMapApiKey) && (
                    <CustomButton
                        className="common-btn-design"
                        onClick={handleSubmit(onSubmit)}
                    // disabled={isSubmitting}
                    >
                        {t("Save_btn")}
                    </CustomButton>
                )}
            </Stack>

            <Grid container spacing={3}>
                <Grid item xs={12} md={6} lg={4}>
                    <CustomTextField
                        name="googlemapkey"
                        control={control}
                        // label="Reset At"
                        fullWidth
                        rules={{
                            required: t("General_Settings.Google_Map_Key.key_required"),
                            // pattern: {
                            //     // value: REGEX.Name_Regex,
                            //     message: "Enter a valid Reset At",
                            // },
                        }}
                        placeholder= {t("General_Settings.Google_Map_Key.Key_Placeholder")}
                        required
                    />
                </Grid>
            </Grid>
        </Paper>
    )
}

export { GoogleMapApiKey }