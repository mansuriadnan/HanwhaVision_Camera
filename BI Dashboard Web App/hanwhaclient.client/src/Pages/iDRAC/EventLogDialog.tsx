import { Box, Dialog, DialogContent, DialogTitle, IconButton, Typography } from '@mui/material';
import React, { useEffect, useState } from 'react'
import CloseIcon from "@mui/icons-material/Close";
import { fetchIdracServerEventData } from '../../services/iDRACService';
import { IEventLogDetail, IIdracServerEventPayload } from '../../interfaces/IManageiDRAC';
import { DataGrid, GridColDef, GridPaginationModel, GridSortModel } from '@mui/x-data-grid';
import { getSeverityClass } from '../../utils/healthStyle';
import { useTimeFormatContext } from '../../context/TimeFormatContext';
import { formatDateToConfiguredTimezone } from '../../utils/formatDateToConfiguredTimezone';
import { formatDate } from '../../utils/dateUtils';
import { useTranslation } from 'react-i18next';
import { useThemeContext } from '../../context/ThemeContext';
interface Props {
    open: boolean;
    onClose: () => void;
    serverIP: string;
}
const EventLogDialog: React.FC<Props> = ({ open, onClose, serverIP }) => {
    const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
       pageSize: 10,
       page: 0,
     });
    const [eventDetail, setEventDetail] = useState<IEventLogDetail[]>([]);
    const [rowCount, setRowCount] = useState(0);
    const [sortModel, setSortModel] = useState<GridSortModel>([
    { field: "eventTimestamp", sort: "desc" },
    ]);
    const { timeFormat } = useTimeFormatContext();
    const { t } = useTranslation();
    const { theme, themeColor } = useThemeContext();

      const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
        ? `${themeColor}/dark-theme/`
        : `${themeColor}/`;

    useEffect(() => {
        if (open && serverIP) {
            getEventLogs();
        }
    }, [open, serverIP, paginationModel]);

    const getEventLogs = async () => {
        const sortBy = sortModel[0]?.field || "eventTimestamp";
        const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
        try {
            const payload: IIdracServerEventPayload = {
                pageNumber: paginationModel.page + 1,
                pageSize: paginationModel.pageSize,
                sortBy:sortBy,
                sortOrder: sortOrder,
                ipAddress: serverIP,
            };

            const response: any = await fetchIdracServerEventData(payload);
            if (response.isSuccess) {
                setEventDetail(response?.data?.eventLogsDetails || []);
                setRowCount(response?.data?.totalCount || 0);
            }
        } catch (error) {
            console.error("Failed to fetch event logs", error);
        }
    };

    const columns: GridColDef[] = [
       
        {
            field: "description",
            headerName: t("IDRAC_Event_Logs.Grid.Description"),
            flex: 2,
            minWidth: 300,
            sortable: false,
        },       
        {
            field: "eventTimestamp",
            headerName: t("IDRAC_Event_Logs.Grid.Event_Time"),
            flex: 1.5,
            renderCell: (params) => {
                const convertedDateTime = (params.value);
                return <span>
                    {/* {formatDate(convertedDateTime, timeFormat)} */}
                    {convertedDateTime}
                </span>;
            }             
        },
        {
            field: "severity",
            headerName: t("IDRAC_Event_Logs.Grid.Severity"),
            flex: 1,
            minWidth: 150,
            sortable: false,
            renderCell: (params) => {
                return (
                    <Box
                        sx={{
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                            height: "100%",
                        }}
                    >
                        <Box
                            className={`status-chip ${getSeverityClass(params.value)}`}
                            sx={{
                                minWidth: 95,
                                px: 1.5,
                                py: 0.4,
                                borderRadius: "999px",
                                textAlign: "center",
                                fontSize: "12px",
                                fontWeight: 600,
                                lineHeight: 1.4,
                            }}
                        >
                            {params.value}
                        </Box>
                    </Box>
                );
            },
        },
    ];

     const handlePaginationModelChange = (
        newPaginationModel: GridPaginationModel,
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
    
      const handleSortModelChange = (newSortModel: GridSortModel) => {
        setSortModel(newSortModel);
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

     const CustomNoRowsOverlay = () => (
        <Box className="no-data-douns"
        >
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
      

    return (
        <Dialog className="inner-cmn-pop-design idrac-table-popup" open={open} onClose={onClose} maxWidth="xl" fullWidth >

            <DialogTitle className="inner-pop-head">
                  {t("IDRAC_Event_Logs.Title")}
                <IconButton onClick={onClose}>
                    <CloseIcon />
                </IconButton>
            </DialogTitle>

            <DialogContent>

                <Box sx={{ width: "100%", mt: 1 }}>
                    <DataGrid
                        rows={eventDetail}
                        columns={columns}
                        pagination
                        paginationMode="server"
                        rowCount={rowCount}
                        pageSizeOptions={[10, 25, 50]}
                        disableRowSelectionOnClick
                        paginationModel={paginationModel}
                        onPaginationModelChange={handlePaginationModelChange}
                        sortModel={sortModel}
                        onSortModelChange={handleSortModelChange}
                        localeText={localeText}
                        slots={{
                            noRowsOverlay: CustomNoRowsOverlay,
                        }}
                    />
                </Box>
            </DialogContent>

        </Dialog>
    )
}

export default EventLogDialog