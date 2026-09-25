import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import { Collectiontype, FinalAuditLogData, IAuditLogsReferenceDatatype, IAuditLogsRequest, IAuditLogsResponse } from "../../interfaces/IAuditLog";
import { DataGrid, GridColDef, GridPaginationModel, GridSortModel} from "@mui/x-data-grid";
import { ExportAuditLogsCSVService, GetAuditLogs } from "../../services/auditService";
import { Box, Chip, FormControl, InputLabel, MenuItem, Select, TextField, Typography } from "@mui/material";
import { useThemeContext } from "../../context/ThemeContext";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { formatDate } from "../../utils/dateUtils";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { auditLogsCollections, LANGUAGE_OPTIONS } from "../../utils/constants";
import { useTranslation } from "react-i18next";
import { HasPermission } from "../../utils/screenAccessUtils";
import { CustomButton } from "../../components";

export interface AuditCollectionType {
    id: string;
    title: string;
    permission : string;
}
const AuditLogs: React.FC = () => {
    const [AuditLog, SetAuditLog] = useState<IAuditLogsResponse[]>([]);
    const [sortModel, setSortModel] = useState<GridSortModel>([
        { field: "createdOn", sort: "desc" },
      ]);
    const [TotalRecord, SetTotalRecord] = useState(0);
    const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
        pageSize: 10,
        page: 0,
    });
    const { state } = useLocation();
    const [referenceData, setReferenceData] = useState<IAuditLogsReferenceDatatype>();
    const { timeFormat } = useTimeFormatContext();
    const keyRenameMap: Record<string, string> = {
        roleIds: "Roles",
        vehicleOwnerId: "Owner Name",
        isOsSyncTimeZone : "Sync with OS",
        timezoneId : "Time zone",
        parentSiteId : "Parent Site",
        childSiteId : "Child Site"
    };
    const [collection, SetCollection] = useState<string>("");
    const [CollectionList, SetCollectionList] = useState<AuditCollectionType[]>([]);
    const [isStateApplied, setIsStateApplied] = useState(false);
    const [collectionId, SetCollectionId] = useState<string | null>("");
    const { t } = useTranslation();
    const { theme, themeColor } = useThemeContext();
    const isANPR = localStorage.getItem("isANPR") === "true";
    const hiddenWhenANPRDisabled = ["ANPRvehicle", "vehicleOwner"];

    const themeColorPath =
        themeColor === "default-theme"
            ? theme === "dark"
                ? "dark-theme/"
                : ""
            : theme === "dark"
                ? `${themeColor}/dark-theme/`
                : `${themeColor}/`;

    const backgroundStyle = {
        backgroundImage: ` url('/images/lines.png'), linear-gradient(287.68deg, #FE6500 -0.05%, #FF8A00 57.77%)`,
        backgroundSize: "cover",
        backgroundRepeat: "no-repeat",
        backgroundPosition: "center",
    };
    useEffect(() => {
        // SetCollectionList(auditLogsCollections);
        // SetCollection(auditLogsCollections[0].id);

        SetCollectionList(auditLogsCollections);

        if (state?.collectionName) {
            SetCollection(state.collectionName);
            SetCollectionId(state.id || null);
            setIsStateApplied(true);
        } else {
            SetCollection(auditLogsCollections[0].id);
            SetCollectionId(null);
        }

        // Remove URL state
        if (state) {
            window.history.replaceState({}, document.title, window.location.pathname);
        }

    }, [])
    useEffect(() => {
        
        if (!collection) return;
        if (!isStateApplied && state?.collectionName) {
            SetCollection(state.collectionName);
            SetCollectionId(state.id)
            setIsStateApplied(true);   // Prevent future overrides
        }
        if (state) {
            window.history.replaceState({}, document.title, window.location.pathname);
        }
        fetchAuditLogData();

    }, [collection, paginationModel, state, collectionId, sortModel]);
    const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
        SetCollectionId(event.target.value);
        setIsStateApplied(true);
        resetpagination();
    };
    const fetchAuditLogData = async () => {
        try {
             const sortBy = sortModel[0]?.field || "createdOn";
             const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
            let request: IAuditLogsRequest = {
                collectionName: collection,
                id: collectionId?.trim(),
                pageNumber: paginationModel.page + 1,
                pageSize: paginationModel.pageSize,
                sortBy: sortBy,
                sortOrder: sortOrder,
            };

            const auditLogsData: any = await GetAuditLogs(request);
            if (
                auditLogsData?.data?.auditLogsDetails &&
                auditLogsData?.data?.auditLogsDetails.length > 0
            ) {

                SetAuditLog(
                    auditLogsData?.data?.auditLogsDetails as IAuditLogsResponse[]
                );
                SetTotalRecord(auditLogsData?.data?.totalCount);
                setReferenceData(auditLogsData.referenceData);
            } else {
                SetAuditLog([]);
                SetTotalRecord(0);
            }
        } catch (err: any) {
            console.error("Error fetching initial data:", err);
        }
    };
    const formatKey = (key: string) => {
        const renamed = keyRenameMap[key] || key;      // replace if mapping exists
        return renamed.charAt(0).toUpperCase() + renamed.slice(1); // capitalize first letter only
    };

    const extractTime = (dateString : string) => {
        if (!dateString) return "";
        // Split date and time
        const parts = dateString.split(" ");        
        if (parts.length === 3) {
            // 12-hour format
            return `${parts[1]} ${parts[2]}`; // 12:00 AM
        }
        if (parts.length === 2) {
            // 24-hour format
            return parts[1]; // 23:15
        }
        return "";
    };
    

    const isObject = (val: any) =>
            val !== null && typeof val === "object" && !Array.isArray(val);

    const normalizeIds = (val: any[]) =>
    (val || []).map(v => (v?._id ? v._id : v));

    const getDetailText = (row: any) => {
        const ignoreKeys = ["createdBy", "updatedBy", "createdOn", "updatedOn", "_id", "isDeleted", "deletedOn", "password", "dataAccessPermission", "profileImage", "isPasswordReset","series","vehicleNumber"];
       
        const flattenObject = (
            obj: any,
            parentKey = "",
            result: Record<string, any> = {}
        ) => {
            Object.keys(obj || {}).forEach((key) => {
                if (ignoreKeys.includes(key)) return;

                const fullKey = parentKey ? `${parentKey}.${key}` : key;
                const value = obj[key];

                if (isObject(value)) {
                    //flattenObject(value, fullKey, result);
                    if ("_id" in value && Object.keys(value).length === 1) {
                        result[fullKey] = value._id;
                    } else {
                        flattenObject(value, fullKey, result);
                    }
                } else {
                    result[fullKey] = value;
                }
            });

            return result;
        };
        const isPlainObject = (val: any) =>
            val !== null && typeof val === "object" && !Array.isArray(val);

        const isArray = Array.isArray;

        const getDifferences = (
            oldVal: any,
            newVal: any,
            parentKey = ""
        ): { key: string; oldValue: any; newValue: any }[] => {
            let diffs: any[] = [];

            if (parentKey === "roleIds") {
                const oldIds = normalizeRoleIds(oldVal);
                const newIds = normalizeRoleIds(newVal);

                if (JSON.stringify(oldIds.sort()) !== JSON.stringify(newIds.sort())) {
                    return [{
                        key: "roleIds",
                        oldValue: oldIds,
                        newValue: newIds
                    }];
                }

                return [];
            }
            if (parentKey === "allowedGates") {
                const oldIds = normalizeIds(oldVal);
                const newIds = normalizeIds(newVal);

                if (JSON.stringify(oldIds.sort()) !== JSON.stringify(newIds.sort())) {
                    return [{
                        key: "allowedGates",
                        oldValue: oldIds,
                        newValue: newIds
                    }];
                }

                return [];
            }
            if (parentKey === "country") {
                const oldId = oldVal?._id || oldVal;
                const newId = newVal?._id || newVal;

                if (oldId !== newId) {
                    return [{
                        key: "country",
                        oldValue: oldId,
                        newValue: newId
                    }];
                }
                return [];
            }

            // ✅ ARRAY HANDLING
            if (isArray(oldVal) || isArray(newVal)) {
                const oldArr = oldVal || [];
                const newArr = newVal || [];
                const maxLength = Math.max(oldArr.length, newArr.length);

                for (let i = 0; i < maxLength; i++) {
                    const keyPath = `${parentKey}[${i}]`;
                    diffs.push(
                        ...getDifferences(oldArr[i], newArr[i], keyPath)
                    );
                }
                return diffs;
            }

            // ✅ OBJECT HANDLING
            if (isPlainObject(oldVal) || isPlainObject(newVal)) {
                const allKeys = new Set([
                    ...Object.keys(oldVal || {}),
                    ...Object.keys(newVal || {}),
                ]);

                allKeys.forEach((key) => {
                    if (ignoreKeys.includes(key)) return;

                    const fullKey = parentKey ? `${parentKey}.${key}` : key;
                    diffs.push(
                        ...getDifferences(oldVal?.[key], newVal?.[key], fullKey)
                    );
                });
                // allKeys.forEach((key) => {
                //     if (ignoreKeys.includes(key)) return;

                //     const fullKey = parentKey ? `${parentKey}.${key}` : key;

                //     const oldValue = oldVal?.[key];
                //     const newValue = newVal?.[key];

                //     // ✅ HANDLE REMOVED FIELD
                //     if (oldValue !== undefined && newValue === undefined) {
                //         diffs.push({
                //             key: fullKey,
                //             oldValue: oldValue,
                //             newValue: null
                //         });
                //         return;
                //     }

                //     // ✅ HANDLE ADDED FIELD
                //     if (oldValue === undefined && newValue !== undefined) {
                //         diffs.push({
                //             key: fullKey,
                //             oldValue: null,
                //             newValue: newValue
                //         });
                //         return;
                //     }

                //     diffs.push(
                //         ...getDifferences(oldValue, newValue, fullKey)
                //     );
                // });
                return diffs;
            }

            // ✅ PRIMITIVE COMPARISON
            if (JSON.stringify(oldVal) !== JSON.stringify(newVal)) {
                diffs.push({
                    key: parentKey,
                    oldValue: oldVal,
                    newValue: newVal,
                });
            }

            return diffs;
        };
        const parseJson = (jsonString: string | null) => {
            if (!jsonString) return null;
            try {
                return JSON.parse(jsonString.replace(/\$oid|\$date/g, (match) => match === "$oid" ? "_id" : "date"));
            } catch {
                return null;
            }
        };

        const formatNestedKey = (key: string) =>
            key
                .replace(/\[(\d+)\]/g, " → #$1")
                .split(".")
                .map(k => formatKey(k))
                .join(" → "); 

        const normalizeRoleIds = (val: any[]) =>
            (val || []).map(v => (v?._id ? v._id : v));
        // Replace IDs using reference data
        const replaceIdWithLabel = (key: string, value: any) => {
            // Do NOT replace values for ignore keys
            if (ignoreKeys.includes(key)) return value;

            if (referenceData) {
                if (key === "createdBy" || key === "updatedBy") {
                    const user = referenceData.createdBy?.find((u) => u.value === value);
                    return user ? user.label : value;
                }

                if (key === "roleIds") {
                    const ids = ([] as any[]).concat(value)
                        .map(v => typeof v === "object" && v?._id ? v._id : v);

                    return ids
                        .map(id => referenceData.roleIds?.find(r => r.value === id)?.label || id)
                        .join(", ");
                }
                
                if (key.startsWith("country")) {
                
                    // const country = referenceData.countryIds?.find((u) => u.value === value._id);
                    // return country ? country.label : value;
                    const id = value?._id || value;
                    const country = referenceData.countryIds?.find(
                        (u) => u.value === id
                    );
                    return country ? country.label : id;
                }
                if (key.startsWith("vehicleOwnerId")) {
                    const id = value?._id || value;
                    const owner = referenceData.ownerIds?.find(
                        (u) => u.value === id
                    );
                    return owner ? owner.label : id;
                }
                if (key == "visitorValidFrom" || key == "visitorValidTo"){
                    const convertedDateTime = formatDateToConfiguredTimezone(value?.date);
                    return formatDate(convertedDateTime, timeFormat);
                }
                if (key == "userPreferences.timezoneId"){
                   const timeZoneIds = value != null ? referenceData.timezoneId?.find((u) => u.value === value._id) : "";
                   return timeZoneIds ? timeZoneIds.label : value;
                }
                if (key == "userPreferences.language"){
                    const lang = LANGUAGE_OPTIONS?.find((u) => u.id === value);
                   return lang ? lang.title : value;
                }
                if (key == "allowedFromTime.date" || key == "allowedToTime.date"){
                    const convertedDateTime = formatDateToConfiguredTimezone(value);
                    let formatted = formatDate(convertedDateTime, timeFormat);
                    return extractTime(formatted);    
                }
                if (key == "visitorValidFrom.date" || key == "visitorValidTo.date"){
                    const convertedDateTime = formatDateToConfiguredTimezone(value);
                    return formatDate(convertedDateTime, timeFormat);
                }
                if (key === "allowedGates") {
                    const ids = ([] as any[]).concat(value)
                        .map(v => typeof v === "object" && v?._id ? v._id : v);

                    return ids
                        .map(id => referenceData.deviceIds?.find(g => g.value === id)?.label || id)
                        .join(", ");
                }
                if (key === "childSiteId") {
                    const id = value?._id || value;
                    const childSite = referenceData.childSite?.find(
                        (u) => u.value === id
                    );
                    return childSite ? childSite.label : id;
                }
                if (key === "parentSiteId") {
                    const id = value?._id || value;
                    const parentSite = referenceData.parentSite?.find(
                        (u) => u.value === id
                    );
                    return parentSite ? parentSite.label : id;
                }
                
            }

            return value;
        };

        const formatDisplayValue = (key: string, value: any) => {
            if (value === true) return "Yes";
            if (value === false) return "No";
            if (value === null || value === undefined) return "-";

            return replaceIdWithLabel(key, value);
        };
        const newData = parseJson(row.operationData);
        const oldData = parseJson(row.documentBeforeChange);

        if (row.operationType === "Insert") {
            if (!newData) return [];

            const flatData = flattenObject(newData);

            return Object.entries(flatData).map(([key, value]) => {
                const cleanValue =
                    Array.isArray(value)
                        ? value
                            .map(v => {
                                if (typeof v === "object" && v !== null) {
                                    if (v._id) return formatDisplayValue(key, v._id);
                                    if (v.$oid) return formatDisplayValue(key, v.$oid);

                                    // fix for features.sunapi
                                    if (v.eventSource) return v.eventSource;

                                    return JSON.stringify(v);
                                }
                                return formatDisplayValue(key, v);
                            })
                            .join(", ")
                            : formatDisplayValue(key, value);

                return (
                    <div key={key}>
                        <span style={{ fontWeight: 600 }}>{formatNestedKey(key)}:</span>{" "}
                        inserted with{" "}
                        <span style={{ fontWeight: 600 }}>"{cleanValue}"</span>
                    </div>
                );
            });
        }

        if (row.operationType === "Update") {
            if (!oldData || !newData) return [];

            const diffs = getDifferences(oldData, newData);
            const isOsSyncTimeZone =
                newData?.userPreferences?.isOsSyncTimeZone === true;
            return diffs.filter(({ key }) => {
                // Skip timezoneId when OS sync is ON
                if (
                    isOsSyncTimeZone &&
                    key === "userPreferences.timezoneId"
                ) {
                    return false;
                }
                return true;
            }).map(({ key, oldValue, newValue }) => {
                const cleanOld =
                    Array.isArray(oldValue)
                        ? oldValue.map(v => formatDisplayValue(key, v)).join(", ")
                        : formatDisplayValue(key, oldValue);

                const cleanNew =
                    Array.isArray(newValue)
                        ? newValue.map(v => formatDisplayValue(key, v)).join(", ")
                        : formatDisplayValue(key, newValue);

                return (
                    <div key={key}>
                        <span style={{ fontWeight: 600 }}>{formatNestedKey(key)}</span>
                        : changed from "{cleanOld}" to{" "}
                        <span style={{ fontWeight: 600 }}>"{cleanNew}"</span>
                    </div>
                );
            });
        }

        return "";
    };
    const columns: GridColDef[] = [
        { field: "documentKey", headerName: t("Audit_Log.Audit_Log_Grid_Column.Document_Id"), width: 250,  sortable: false, },
        
        {
            field: "createdBy",
            headerName: t("Audit_Log.Audit_Log_Grid_Column.User"),
            width: 250,
            sortable: false,
            // flex: 1,
            renderCell: (params) => {
                const createdByID = params.row.createdBy as string;
                const filteredData = referenceData?.createdBy?.find(
                    (item) => item.value === createdByID
                );
                return <Box>{filteredData && <div>{filteredData.label}</div>}</Box>;
            },
        },
        { 
            field: "operationType", headerName:t("Audit_Log.Audit_Log_Grid_Column.Type"), width: 250,
            sortable: false,
            renderCell: (params) => {
                let operationData = params.row.operationData;
                try {
                    if (typeof operationData === "string") {
                        operationData = JSON.parse(operationData);
                    }
                } catch (e) {
                    console.error("Invalid JSON:", e);
                }
                // If deleted → show "Deleted"
                if (operationData?.isDeleted === true) {
                    return "Delete";
                }
                // Otherwise → return whatever is in the field
                return params.value;
            }
         },
        {
            field: "createdOn",
            headerName: t("Audit_Log.Audit_Log_Grid_Column.Date"),
            width: 200,
            renderCell: (params) => {
                const convertedDateTime = formatDateToConfiguredTimezone(params.value);
                return formatDate(convertedDateTime, timeFormat);
            },
        },
        {
            field: "details", headerName: t("Audit_Log.Audit_Log_Grid_Column.Details"),
            width: 600,
            sortable: false,
            cellClassName: "audit-details-column-cell",
            renderCell: (params) => {
                const text = getDetailText(params.row);
                return <Typography sx={{
                    whiteSpace: "pre-line",
                    lineHeight: 1.6
                }}>{text}</Typography>;
            },
        }
    ];
    const handlePaginationModelChange = (
        newPaginationModel: GridPaginationModel
    ) => {
        // setPaginationModel(newPaginationModel);
        const isPageSizeChanged = newPaginationModel.pageSize !== paginationModel.pageSize;
        if (isPageSizeChanged) {
            setPaginationModel({
                ...newPaginationModel,
                page: 0,
            });
        } else {
            setPaginationModel(newPaginationModel);
        }
    };
    const CustomNoRowsOverlay = () => (
        <Box className="no-data-douns">
            <Box sx={{ width: 200, justifyItems: "center", flex: 1, }}>
                <img src={`/images/${themeColorPath}noData.gif`} alt="Animated GIF" width="100" height="100" />                
                <Typography
                    sx={{ FontWeight: 600, fontSize: 24, color: "#090909" }}
                >
                    {t("No_data_found")}
                </Typography>

            </Box>
        </Box>
    );
    const handleCollectionChange = (event: any) => {
        const selectedValue = event.target.value;
        // SetCollection(selectedValue);
        const selectedOption = CollectionList.find(
            (item) => item.id === selectedValue
        );
        SetCollection(selectedOption ? selectedOption.id : "");
        setIsStateApplied(true);
        SetCollectionId(null)
        fetchAuditLogData();
        resetpagination();
    };
    const resetpagination = () => {
        setPaginationModel({
            ...paginationModel,
            page: 0,
        });
    };

    const handleSortModelChange = (newSortModel: GridSortModel) => {
        setSortModel(newSortModel);
    };

    const exportAudtiLog = async () => {
        const sortBy = sortModel[0]?.field || "maintenanceDueDate";
        const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
        try {
           let request: IAuditLogsRequest = {
                collectionName: collection,
                id: collectionId?.trim(),
                pageNumber: paginationModel.page + 1,
                pageSize: paginationModel.pageSize,
                sortBy: sortBy,
                sortOrder: sortOrder,
            };
            await ExportAuditLogsCSVService({ data: request });
        } catch (err: any) {
            console.error("Error fetching initial data:", err);
        }
    };

    const localeText = {
        // Column Menu
        columnMenuSortAsc: t("MUI_Grid_Filter_Paginition.columnMenuSortAsc"),
        columnMenuSortDesc: t("MUI_Grid_Filter_Paginition.columnMenuSortDesc"),
        columnMenuHideColumn: t("MUI_Grid_Filter_Paginition.columnMenuHideColumn"),
        columnMenuManageColumns: t("MUI_Grid_Filter_Paginition.columnMenuManageColumns"),

        // Pagination (TablePagination)
        MuiTablePagination: {
            labelRowsPerPage: t("MUI_Grid_Filter_Paginition.footerPaginationRowsPerPage"),
            labelDisplayedRows: ({ from, to, count }: {
                from: number;
                to: number;
                count: number;
            }) =>
                `${from}–${to} ${t("MUI_Grid_Filter_Paginition.footerTotalVisibleRows", { total: count })}`,
        },

        footerRowSelected: (count: number) =>
            t("MUI_Grid_Filter_Paginition.footerRowSelected", { count }),

        footerRowSelectedPlural: (count: number) =>
            t("MUI_Grid_Filter_Paginition.footerRowSelectedPlural", { count }),
    };
    return (
        <>
            <div className="main-dashbourd-wrapper">
                <div className="top-orange-head" style={backgroundStyle}>
                    <Box className="top-orange-head-left">
                        <Typography variant="h4"> {t("Audit_Log.Audit_Log_Header")}</Typography>
                        <Typography> {t("Audit_Log.Audit_Log_Descripltion")}</Typography>
                    </Box>
                </div>
                <div className="audit-logs">
                    <FormControl 
                        sx={{
                            minWidth: 250,
                            mb: 2,
                            marginInlineEnd: 5,
                        }}
                    size="small">                    
                        <Select
                            value={collection}                    
                            onChange={handleCollectionChange}>
                            {CollectionList
                                .filter(item =>
                                    HasPermission(item.permission) &&
                                    (isANPR || !hiddenWhenANPRDisabled.includes(item.id))
                                )
                                .map((role) => (
                                <MenuItem key={role.id} value={role.id}>
                                    {role.title}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <TextField
                        variant="outlined"
                        placeholder= {t("Audit_Log.Search_Placeholder")}
                        value={collectionId ?? ""}
                        onChange={handleSearch}
                        sx={{ borderRadius: 8, width: 500 }}
                        className="audit-search"
                    />  
                    <div className="top-orange-head-maintenance" >
                        <div className='dashbourd-retail-details-export maintenance-schedule-items-only top-orange-head-audit'>
                            <CustomButton
                                variant="outlined"
                                onClick={() => exportAudtiLog()}
                            >
                                  <img src={"/images/csv.svg"} alt="export" />
                            </CustomButton>
                        </div>
                    </div>
                </div>              
                <DataGrid
                    rows={AuditLog}
                    columns={columns}
                    getRowId={(row) => row.id}
                    paginationModel={paginationModel}
                    onPaginationModelChange={handlePaginationModelChange}
                    rowCount={AuditLog.length === 0 ? 0 : TotalRecord}
                    paginationMode="server"
                    pageSizeOptions={[5, 10, 15, 20, 25]}
                    // checkboxSelection
                    sortingMode="server"
                    sortModel={sortModel}
                    onSortModelChange={handleSortModelChange}
                    hideFooter={AuditLog.length === 0}
                    // disableRowSelectionOnClick
                    slots={{
                        noRowsOverlay: CustomNoRowsOverlay,
                    }}
                    localeText={localeText}
                    disableRowSelectionOnClick
                />               
            </div>
        </>
    );
};
export default AuditLogs