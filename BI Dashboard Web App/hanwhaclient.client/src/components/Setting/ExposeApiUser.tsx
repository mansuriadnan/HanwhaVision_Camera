import React, { useEffect, useState } from "react";
import {
    Box,
    Typography,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    IconButton,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import {
    getExposeApiUsers,
    saveExposeApiUser,
    deleteExposeApiUser,
} from "../../services/settingService";
import { CommonDialog } from "../Reusable/CommonDialog";
import { CustomTextField } from "../Reusable/CustomTextField";
import { useForm } from "react-hook-form";
import { COMMON_CONSTANTS, REGEX } from "../../utils/constants";
import { CustomSelect } from "../Reusable/CustomSelect";
import { CustomSwitch } from "../Reusable/CustomSwitch";
import { CustomButton } from "../Reusable/CustomButton";

interface UserFormInputs {
    username: string;
    role: string;
    password: string;
    isActive: boolean;
    id?: string;
}

export const ExposeApiUser = () => {
    const { t } = useTranslation();
    const [users, setUsers] = useState<UserFormInputs[]>([]);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
    const [userToDelete, setUserToDelete] = useState<string | null>(null);
    const [showInfoDialogOpen, setShowInfoDialogOpen] = useState(false);
    const [selectedUser, setSelectedUser] = useState<UserFormInputs>();

    const defaultValues = {
        username: "",
        password: "",
        role: "",
        isActive: true,
    };

    const {
        control: addControl,
        handleSubmit: addSubmit,
        reset: resetAdd
    } = useForm<UserFormInputs>({ defaultValues });

    const {
        control: editControl,
        handleSubmit: editSubmit,
        reset: resetEdit
    } = useForm<UserFormInputs>({ defaultValues });

    const Rolelist = [
        { title: "Vaxtor", id: "vaxtor" },
        { title: "VI API", id: "viapi" },
        { title: "iDRAC", id: "idrac" },
    ]

    useEffect(() => {
        fetchUsers();
    }, []);

    const converttoBase64Utf8 = (input: string) => {
        const bytes = new TextEncoder().encode(input); // UTF-8 encoding
        let binary = '';
        bytes.forEach(b => binary += String.fromCharCode(b));
        return btoa(binary);
    }


    const fetchUsers = async () => {
        try {
            const data = await getExposeApiUsers();
            if (data?.length && data?.length > 0) {
                setUsers(data);
            } else {
                setUsers([]);
            }
        } catch (error) {
            console.error("Failed to fetch users", error);
        }
    };

    const handleAddSave = async (data: UserFormInputs) => {
        try {
            const res = await saveExposeApiUser(data);

            if (typeof res === "object" && res?.isSuccess) {
                resetAdd(defaultValues);
                fetchUsers();
            }
        } catch (err) {
            console.error("Add failed:", err);
        }
    };

    const handleEditSave = async (data: UserFormInputs) => {
        try {
            const res = await saveExposeApiUser({
                ...data,
                id: editingId,
            });

            if (typeof res === "object" && res?.isSuccess) {
                resetEdit(defaultValues);
                fetchUsers();
                setEditingId(null);
            }
        } catch (err) {
            console.error("Edit failed:", err);
        }
    };

    const handleEditClick = (user: any) => {
        resetEdit({
            username: user.username,
            password: user.password,
            role: user.role,
            isActive: user.isActive,
        });
        setEditingId(user.id);
    };

    const handleCancelClick = () => {
        setEditingId(null);
        resetAdd();
        resetEdit();
    };

    const handleDeleteClick = (id: string) => {
        setUserToDelete(id);
        setDeleteDialogOpen(true);
    };

    const confirmDelete = async () => {
        if (userToDelete) {
            try {
                await deleteExposeApiUser({ id: userToDelete });
                setDeleteDialogOpen(false);
                setUserToDelete(null);
                fetchUsers();
            } catch (error) {
                console.error("Failed to delete user", error);
            }
        }
    };



    return (
        <>
            <Box
                className="smtp-setup-wrapper"
                sx={{
                    p: 3,
                    mt: 3,
                    bgcolor: "background.paper",
                    borderRadius: 2,
                    boxShadow: 1,
                }}>
                <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                    <Typography variant="h4">{t("ExposeAPIUser.Expose_API_Users")}</Typography>
                </Box>

                <TableContainer className="exposeapi-users-table" component={Paper} elevation={0} variant="outlined">
                    <Table size="small" >
                        <TableHead>
                            <TableRow>
                                <TableCell width={"25%"}>{t("ExposeAPIUser.Username")}</TableCell>
                                <TableCell width={"25%"}>{t("ExposeAPIUser.Password")}</TableCell>
                                <TableCell width={"20%"}>{t("ExposeAPIUser.Role")}</TableCell>
                                <TableCell width={"10%"}>{t("ExposeAPIUser.IsActive")}</TableCell>
                                <TableCell align="center" width={"20%"}>{t("ExposeAPIUser.Actions")}</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {users.map((user) => (
                                <TableRow key={user.id}>
                                    {editingId === user.id ? (
                                        <>
                                            <TableCell>
                                                <CustomTextField
                                                    name="username"
                                                    control={editControl}
                                                    rules={{
                                                        required: t("ExposeAPIUser.Username_Required"),
                                                        pattern: {
                                                            value: REGEX.UserName_Regex,
                                                            message: t("ExposeAPIUser.Username_Valid"),
                                                        },
                                                        maxLength: {
                                                            value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                                                            message: t("ExposeAPIUser.Username_Strong_Validation", { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                                                        },
                                                    }}
                                                    placeholder={t("ExposeAPIUser.Username_Placeholder")}
                                                    required
                                                    fullWidth
                                                />
                                            </TableCell>
                                            <TableCell>
                                                <CustomTextField
                                                    name="password"
                                                    control={editControl}
                                                    type={"password"}
                                                    rules={{
                                                        required: t("ExposeAPIUser.Password_Required"),
                                                        pattern: {
                                                            value: REGEX.Password_Regex,
                                                            message: t("ExposeAPIUser.Password_Strong_Validation"),
                                                        },
                                                        maxLength: {
                                                            value: COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH,
                                                            message: t("ExposeAPIUser.Password_Max_Validation", { max: COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH }),
                                                        },

                                                    }}
                                                    placeholder={t("ExposeAPIUser.Password_Placeholder")}
                                                    required
                                                    fullWidth
                                                />
                                            </TableCell>
                                            <TableCell>
                                                <CustomSelect
                                                    name="role"
                                                    control={editControl}
                                                    options={Rolelist}
                                                    rules={{ required: t("ExposeAPIUser.Role_Required") }}
                                                    required
                                                    placeholder={t("ExposeAPIUser.Roles_Placeholder")}
                                                />
                                            </TableCell>
                                            <TableCell>
                                                <CustomSwitch
                                                    name="isActive"
                                                    control={editControl}
                                                />
                                            </TableCell>
                                            <TableCell align="center">
                                                <CustomButton customStyles={{ mr: 2 }} onClick={editSubmit(handleEditSave)} className="common-btn-design" >
                                                    {t("Save_btn")}
                                                </CustomButton>
                                                <CustomButton onClick={handleCancelClick} className="common-btn-design common-btn-design-transparent">
                                                    {t("Common_DELETE_Confirmation_Dialog.Cancel")}
                                                </CustomButton>
                                            </TableCell>
                                        </>
                                    ) : (
                                        <>
                                            <TableCell>{user.username}</TableCell>
                                            <TableCell>********</TableCell>
                                            <TableCell>{user.role}</TableCell>
                                            <TableCell>
                                                {user.isActive ? "Yes" : "No"}
                                            </TableCell>
                                            <TableCell align="center">
                                                <IconButton
                                                    onClick={() => handleEditClick(user)}
                                                >
                                                    <img
                                                        src={"/images/user-action-edit.svg"}
                                                        alt="User Action Edit Icon"
                                                        width={20}
                                                        height={20}
                                                    />
                                                </IconButton>
                                                <IconButton
                                                    onClick={() => handleDeleteClick(user.id as string)}
                                                >
                                                    <img
                                                        src={"/images/user-action-delete.svg"}
                                                        alt="User Action Delete Icon"
                                                        width={20}
                                                        height={20}
                                                    />
                                                </IconButton>

                                                <IconButton
                                                    onClick={() => {
                                                        setShowInfoDialogOpen(true);
                                                        setSelectedUser(user);
                                                    }}
                                                >
                                                    <img
                                                        src={"/images/user-action-key.svg"}
                                                        alt="User Action key Icon"
                                                        width={20}
                                                        height={20}
                                                    />
                                                </IconButton>
                                            </TableCell>
                                        </>
                                    )}
                                </TableRow>
                            ))}

                            {users.length === 0 && (
                                <TableRow>
                                    <TableCell colSpan={5} align="center">
                                        No users found.
                                    </TableCell>
                                </TableRow>
                            )}

                            <TableRow>
                                <TableCell>
                                    <CustomTextField
                                        name="username"
                                        control={addControl}
                                        rules={{
                                            required: t("ExposeAPIUser.Username_Required"),
                                            pattern: {
                                                value: REGEX.UserName_Regex,
                                                message: t("ExposeAPIUser.Username_Valid"),
                                            },
                                            maxLength: {
                                                value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                                                message: t("ExposeAPIUser.Username_Strong_Validation", { max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                                            },
                                        }}
                                        placeholder={t("ExposeAPIUser.Username_Placeholder")}
                                        required
                                        fullWidth
                                    />
                                </TableCell>
                                <TableCell>
                                    <CustomTextField
                                        name="password"
                                        control={addControl}
                                        type={"password"}
                                        rules={{
                                            required: t("ExposeAPIUser.Password_Required"),
                                            pattern: {
                                                value: REGEX.Password_Regex,
                                                message: t("ExposeAPIUser.Password_Strong_Validation"),
                                            },
                                            maxLength: {
                                                value: COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH,
                                                message: t("ExposeAPIUser.Password_Max_Validation", { max: COMMON_CONSTANTS.MAX_PASSWORD_FIELD_LENGTH }),
                                            },

                                        }}
                                        placeholder={t("ExposeAPIUser.Password_Placeholder")}
                                        required
                                        fullWidth
                                    />
                                </TableCell>
                                <TableCell>
                                    <CustomSelect
                                        name="role"
                                        control={addControl}
                                        options={Rolelist}
                                        rules={{ required: t("ExposeAPIUser.Role_Required") }}
                                        required
                                        placeholder={t("ExposeAPIUser.Roles_Placeholder")}
                                    />
                                </TableCell>
                                <TableCell>
                                    <CustomSwitch
                                        name="isActive"
                                        control={addControl}
                                    />
                                </TableCell>
                                <TableCell className="expose-users-btn">
                                    <CustomButton className="common-btn-design" onClick={addSubmit(handleAddSave)}>
                                        {t("Save_btn")}
                                    </CustomButton>
                                    <CustomButton className="common-btn-design-transparent common-btn-design" onClick={handleCancelClick}>
                                        {t("Common_DELETE_Confirmation_Dialog.Cancel")}
                                    </CustomButton>
                                </TableCell>
                            </TableRow>
                        </TableBody>
                    </Table>
                </TableContainer>
            </Box>
            <CommonDialog
                customClass="cmn-confirm-delete-icon"
                open={deleteDialogOpen}
                title={t("Common_DELETE_Confirmation_Dialog.Title")}
                content={t("Common_DELETE_Confirmation_Dialog.Content")}
                onConfirm={() => confirmDelete()}
                onCancel={() => setDeleteDialogOpen(false)}
                confirmText={t("Common_DELETE_Confirmation_Dialog.Delete")}
                cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
                type="delete"
                titleClass={true}
            />
            <CommonDialog
                open={showInfoDialogOpen}
                title={t("ExposeAPIUser.API_Authentication")}
                content={
                    <div className="api-authentication-pop">
                        <table>
                            <tbody>
                                <tr>
                                    <th>{t("ExposeAPIUser.Username")}</th>
                                    <td>{selectedUser?.username}</td>
                                </tr>
                                <tr>
                                    <th>{t("ExposeAPIUser.Password")}</th>
                                    <td>{selectedUser?.password}</td>
                                </tr>
                                <tr>
                                    <th>{t("ExposeAPIUser.Authorization")}</th>
                                    <td>Basic {converttoBase64Utf8(`${selectedUser?.username}:${selectedUser?.password}`)}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                }
                confirmText={t("Okay")}
                onCancel={() => {
                    setShowInfoDialogOpen(false);
                    setSelectedUser(undefined);
                }}
                onConfirm={() => {
                    setShowInfoDialogOpen(false);
                    setSelectedUser(undefined)
                }}
                type="exposAPIUser"
                customClass="forgot-pass"
                titleClass={true}
            />
        </>

    );
};
