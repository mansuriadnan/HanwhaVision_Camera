import React, { useState, useEffect } from "react";
import {
  Typography,
  TextField,
  Box,
  InputAdornment,
  Button,
  FormLabel,
} from "@mui/material";
import {
  DataGrid,
  GridColDef,
  GridPaginationModel,
  GridSortModel,
} from "@mui/x-data-grid";
import {
  IGetAllLPRData,
  IOwnerList,
  LPRFilter,
  countryres,
  ICountryOption,
  ImagePayload,
} from "../../interfaces/IANPR";
import {
  GetAllLprDetailsService,
  GetAllCountryListService,
  ExportLPRCSVService,
  GetLPRImageService,
} from "../../services/anprService";
import { useThemeContext } from "../../context/ThemeContext";
import { formatDateToConfiguredTimezone } from "../../utils/formatDateToConfiguredTimezone";
import { useTimeFormatContext } from "../../context/TimeFormatContext";
import { formatDate } from "../../utils/dateUtils";
import { useTranslation } from "react-i18next";
import {
  CustomButton,
  CustomMultiSelect,
  CustomSelect,
} from "../../components";
import { ILookup } from "../../interfaces/ILookup";
import { convertToUTC } from "../../utils/convertToUTC";
import {
  GetAllFloorsListService,
  GetAllZonesByFloorIdService,
} from "../../services/dashboardService";
import { useForm, Controller } from "react-hook-form";
import { fetchDevicesByFloorZoneDataService } from "../../services/dashboardService";
import { LocalizationProvider, DateTimePicker } from "@mui/x-date-pickers";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs from "dayjs";
import { LabelImportant } from "@mui/icons-material";
import IosShareIcon from "@mui/icons-material/IosShare";
import { showToast } from "../../components/Reusable/Toast";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import { blobToBase64, convertBase64ToPngBase64 } from "../../utils/convertImageToBase64";

const LPR: React.FC = () => {
  const [floorList, setFloorList] = useState<ILookup[]>([]);
  const [zoneList, setZoneList] = useState<ILookup[]>([]);
  const [deviceList, setDeviceList] = useState<ILookup[]>([]);
  const [countryList, setCountryList] = useState<ICountryOption[]>([]);
  const [lPRList, setLPRList] = useState<IOwnerList[]>([]);
  const [TotalRecord, SetTotalRecord] = useState<number>(0);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [imageMap, setImageMap] = useState<Record<string, string>>({});
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    pageSize: 10,
    page: 0,
  });
  const [sortModel, setSortModel] = useState<GridSortModel>([
    { field: "createdOn", sort: "desc" },
  ]);

  const { theme, themeColor } = useThemeContext();
  const { timeFormat } = useTimeFormatContext();

  const backgroundStyle = {
    backgroundImage: ` url('/images/lines.png'), linear-gradient(287.68deg, #FE6500 -0.05%, #FF8A00 57.77%)`,
    backgroundSize: "cover",
    backgroundRepeat: "no-repeat",
    backgroundPosition: "center",
  };

  const { t } = useTranslation();

  const { control, setValue, watch, getValues } = useForm<LPRFilter>({
    defaultValues: {
      floorIds: [],
      zoneIds: [],
      deviceIds: [],
      countryName: "",
      fromDate: null, // dayjs().startOf("day"),
      toDate: null, // dayjs(),
    },
  });

  const floorIds = watch("floorIds") ?? [];
  const zoneIds = watch("zoneIds") ?? [];
  //   const deviceIds = watch("deviceIds") ?? [];
  //   const defaultFloorId = "000000000000000000000000";

  const themeColorPath =
    themeColor === "default-theme"
      ? theme === "dark"
        ? "dark-theme/"
        : ""
      : theme === "dark"
        ? `${themeColor}/dark-theme/`
        : `${themeColor}/`;

  useEffect(() => {
    fetchFloorData();
    getAllCountryListData();
  }, []);

  useEffect(() => {
    fetchZoneData(floorIds);
  
      setValue("zoneIds", []);
      setValue("deviceIds", []);
  
  }, [floorIds, setValue]);

  useEffect(() => {
    fetchDeviceList();
  }, [floorIds, zoneIds]);

  const fetchFloorData = async () => {
    try {
      const response = await GetAllFloorsListService();
      const floorData = response?.map((item) => ({
        title: item.floorPlanName,
        id: item.id,
      }));
      setFloorList(floorData as ILookup[]);
    } catch (err: any) {
      console.error("Error while fetching the floor data");
    }
  };

  const fetchZoneData = async (floorIds: string[]) => {
    if (!Array.isArray(floorIds) || floorIds.length === 0) {
      setZoneList([]);
      return;
    }

    try {
      const response: any = await GetAllZonesByFloorIdService(floorIds);
      const allZones: ILookup[] = (response?.data ?? []).flatMap(
        (floor: any) =>
          Array.isArray(floor?.zones)
            ? floor.zones.map((zone: any) => ({
              id: zone.id,
              title: zone.zoneName,
            }))
            : [],
      );

      setZoneList(allZones);
      setValue("zoneIds", []);
    } catch (err: any) {
      console.error("Error while fetching the zone data:", err?.message || err);
      setZoneList([]);
    }
  };

  const fetchDeviceList = async () => {
    try {
      const param = {
        floorIds: floorIds,
        zoneIds: zoneIds,
      };

      let response: any = await fetchDevicesByFloorZoneDataService(
        param as any,
      );

      const deviceData = response?.data?.map((item: any) => ({
        title: item.cameraName,
        id: item.deviceId,
      }));

      setDeviceList(deviceData as ILookup[]);
    } catch (error) {
      console.error("Error fetching device by floor and zone:", error);
      setDeviceList([]);
    }
  };

  const getAllCountryListData = async () => {
    try {
      const countryData: any = await GetAllCountryListService();
      if (countryData && countryData?.length && countryData?.length > 0) {
        const tempcountryList = countryData.map((item: countryres) => ({
          title: `${item.countryCode} - ${item.countryName}`,
          id: item.id,
          countryName: item.countryName,
        }));
        setCountryList(tempcountryList);
      } else {
        setCountryList([]);
      }
    } catch (err: any) {
      console.error("Error fetching country data:", err);
    }
  };

  const columns: GridColDef[] = [
    {
      field: "plate",
      headerName: t("LPR_Screen.Grid_column.Plate"),
      filterable: false,
      width:200,
      sortable: false,

    },
    {
      field: "smallImage",
      headerName: "Plate Image",
      width: 200,
      sortable: false,
      renderCell: (params) => {
        const imageSrc = imageMap[params.value];

        return imageSrc ? (
          <img
            src={imageSrc}
            alt="Plate"
            style={{
              width: 120,
              height: 60,
              objectFit: "contain",
            }}
            className="LPR-img"
          />
        ) : (
          <span>N/A</span>
        );
      },
    },
    {
      field: "country",
      headerName: t("LPR_Screen.Grid_column.Country"),
      filterable: false,
      width:200,
      sortable: false,
    },
    {
      field: "state",
      headerName: t("LPR_Screen.Grid_column.State"),
      filterable: false,
      width:200,
      sortable: false,
    },
    {
      field: "entryGate",
      headerName: t("LPR_Screen.Grid_column.EntryGate"),
      filterable: false,
      width:150,
      sortable: false,
    },
    {
      field: "entryTime",
      headerName: t("LPR_Screen.Grid_column.EntryTime"),
      renderCell: (params) => {
        const entry = formatDateToConfiguredTimezone(params.row.entryTime);
        const entryFormatted = formatDate(entry, timeFormat);
        return <Box>{entryFormatted}</Box>;
      },
     width:150
    },
    {
      field: "exitGate",
      headerName: t("LPR_Screen.Grid_column.ExitGate"),
      filterable: false,
      width:150,
      sortable: false,
    },
    {
      field: "exitTime",
      headerName: t("LPR_Screen.Grid_column.ExitTime"),
      renderCell: (params) => {
        const exit = formatDateToConfiguredTimezone(params.row.exitTime);
        const exitFormatted = formatDate(exit, timeFormat);
        return <Box>{exitFormatted}</Box>;
      },
       width:150
    },
    {
      field: "ownerName",
      headerName: t("LPR_Screen.Grid_column.OwnerName"),
      filterable: false,
      width:200,
      sortable: false,
    },
    {
      field: "registrationType",
      headerName: t("LPR_Screen.Grid_column.RegistrationType"),
      filterable: false,
      width:150,
      sortable: false,
    },
    {
      field: "building",
      headerName: t("LPR_Screen.Grid_column.Building"),
      filterable: false,
      width:150,
      sortable: false,
    },
    {
      field: "buildingUnit",
      headerName: t("LPR_Screen.Grid_column.BuildingUnit"),
      filterable: false,
      width:150,
      sortable: false,
    },
    {
      field: "contact",
      headerName: t("LPR_Screen.Grid_column.ContactNumber"),
      filterable: false,
      width:150,
      sortable: false,
    },
    {
      field: "email",
      headerName: t("LPR_Screen.Grid_column.Email"),
      filterable: false,
      width:200,
      sortable: false,
    },
    {
      field: "make",
      headerName: t("LPR_Screen.Grid_column.Make"),
      filterable: false,
      width:150,
      sortable: false,
    },
    {
      field: "model",
      headerName: t("LPR_Screen.Grid_column.Model"),
      filterable: false,
      width:150,
      sortable: false,
    },
    {
      field: "color",
      headerName: t("LPR_Screen.Grid_column.Color"),
      filterable: false,
      width:150,
      sortable: false,
    },
    {
      field: "message",
      headerName: t("LPR_Screen.Grid_column.Message"),
      filterable: false,
      width:200,
      sortable: false,
    },
    {
      field: "createdOn",
      headerName: t("LPR_Screen.Grid_column.CreatedOn"),
      renderCell: (params) => {
        const createdOn = formatDateToConfiguredTimezone(params.row.createdOn);
        const createdOnFormatted = formatDate(createdOn, timeFormat);
        return <Box>{createdOnFormatted}</Box>;
      },
      width:200
    },
  ];

  useEffect(() => {
    const delayDebounce = setTimeout(() => {
      if (searchTerm.length >= 3 || searchTerm.length === 0) {
        fetchLPRData();
      }
    }, 500);

    return () => clearTimeout(delayDebounce);
  }, [paginationModel, sortModel, searchTerm]);

  const fetchLPRData = async () => {
    const sortBy = sortModel[0]?.field || "createdOn";
    const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
    const formValues = watch();

    const selectedCountry = countryList.find(
      (c: any) => c.id === formValues.countryName,
    );

    const from = formValues.fromDate && dayjs(formValues.fromDate).isValid()
      ? dayjs(formValues.fromDate)
      : null;

    const to = formValues.toDate && dayjs(formValues.toDate).isValid()
      ? dayjs(formValues.toDate)
      : null;

    // Validation: only check if both exist
    if (from && to && to.isBefore(from)) {
      showToast(
        t("LPR_Screen.Filters.ExitTime_Validation"),
        "error"
      );
      return;
    }

    const fromDate = from
      ? convertToUTC(from.format("YYYY-MM-DDTHH:mm:ss"))
      : null;

    const toDate = to
      ? convertToUTC(to.format("YYYY-MM-DDTHH:mm:ss"))
      : null;

    try {
      let request: IGetAllLPRData = {
        searchText: searchTerm.length > 0 ? searchTerm : "",
        pageNumber: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        sortBy: sortBy,
        sortOrder: sortOrder,
        floorIds: formValues.floorIds,
        zoneIds: formValues.zoneIds,
        deviceIds: formValues.deviceIds,
        countryName: selectedCountry?.countryName || "", //formValues.countryName,
        fromDate: fromDate,
        toDate: toDate,
      };

      const LPRData: any = await GetAllLprDetailsService(request);

      if (LPRData && LPRData.isSuccess) {
        setLPRList(LPRData?.data?.items ?? []);
        SetTotalRecord(LPRData?.data?.totalCount ?? 0);
        loadImages(LPRData?.data?.items);
      } else {
        setLPRList([]);
        SetTotalRecord(0);
      }
    } catch (err: any) {
      console.error("Error fetching LPR data:", err);
    }
  };

  const loadImages = async (items: any[]) => {
  const imageCache: Record<string, string> = {};

  await Promise.all(
    items.map(async (item) => {
      if (!item.smallImage) return;

      const payload: ImagePayload = {
        imageName: item.smallImage,
        size: "small",
      };

      try {
        const blob = await GetLPRImageService(payload);

        const base64 = await blobToBase64(blob);

        imageCache[item.smallImage] = base64;
      } catch (error) {
        console.error(
          `Failed to load image ${item.smallImage}`,
          error
        );
      }
    })
  );

  setImageMap(imageCache);
};

  const handlePaginationModelChange = (
    newPaginationModel: GridPaginationModel,
  ) => {
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

  const CustomNoRowsOverlay = () => (
    <Box className="no-data-douns">
      <Box sx={{ width: 200, justifyItems: "center", flex: 1 }}>
        <img
          src={`/images/${themeColorPath}noData.gif`}
          alt="Animated GIF"
          width="100"
          height="100"
        />
        <Typography sx={{ FontWeight: 600, fontSize: 24, color: "#090909" }}>
           {t("No_data_found")}
        </Typography>
      </Box>
    </Box>
  );

  const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    const searchValue = event.target.value;
    setSearchTerm(searchValue);
  };

  const handleLPRCSVDownload = async () => {
    const sortBy = sortModel[0]?.field || "createdOn";
    const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
    const formValues = watch();

    const selectedCountry = countryList.find(
      (c: any) => c.id === formValues.countryName,
    );

    const fromDate =
      formValues.fromDate && dayjs(formValues.fromDate).isValid()
        ? convertToUTC(formValues.fromDate.format("YYYY-MM-DDTHH:mm:ss"))
        : null;

    const toDate =
      formValues.toDate && dayjs(formValues.toDate).isValid()
        ? convertToUTC(formValues.toDate.format("YYYY-MM-DDTHH:mm:ss"))
        : null;

    try {
      let request: IGetAllLPRData = {
        searchText: searchTerm.length > 0 ? searchTerm : "",
        pageNumber: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
        sortBy: sortBy,
        sortOrder: sortOrder,
        floorIds: formValues.floorIds,
        zoneIds: formValues.zoneIds,
        deviceIds: formValues.deviceIds,
        countryName: selectedCountry?.countryName || "", //formValues.countryName,
        fromDate: fromDate,
        toDate: toDate,
      };

      await ExportLPRCSVService({ data: request });
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };


const handleExportPdf = async () => {

  // ── validation ─────────────────────────────────────────────────────────────
  const formValues = watch();

  const from = formValues.fromDate && dayjs(formValues.fromDate).isValid()
    ? dayjs(formValues.fromDate) : null;
  const to = formValues.toDate && dayjs(formValues.toDate).isValid()
    ? dayjs(formValues.toDate) : null;

  if (!from) {
    showToast(t("LPR_Screen.Filters.EntryTime_Required"), "error");
    return;
  }

  if (!to) {
    showToast(t("LPR_Screen.Filters.ExitTime_Required"), "error");
    return;
  }

  if (to.isBefore(from)) {
    showToast(t("LPR_Screen.Filters.ExitTime_Validation"), "error");
    return;
  }

  const diffDays = to.diff(from, "day");
  if (diffDays > 30) {
    showToast(t("LPR_Screen.Filters.DateRange_Max30Days"), "error");
    return;
  }

  const sortBy = sortModel[0]?.field || "createdOn";
  const sortOrder = sortModel[0]?.sort === "desc" ? -1 : 1;
  const selectedCountry = countryList.find((c: any) => c.id === formValues.countryName);
  const fromDate = convertToUTC(from.format("YYYY-MM-DDTHH:mm:ss"));
  const toDate   = convertToUTC(to.format("YYYY-MM-DDTHH:mm:ss"));

  // ── step 1: get total count ────────────────────────────────────────────────
  let totalCount = 0;
  try {
    const countRequest: IGetAllLPRData = {
      searchText: searchTerm.length > 0 ? searchTerm : "",
      pageNumber: 1,
      pageSize: 1,
      sortBy,
      sortOrder,
      floorIds: formValues.floorIds,
      zoneIds:  formValues.zoneIds,
      deviceIds: formValues.deviceIds,
      countryName: selectedCountry?.countryName || "",
      fromDate,
      toDate,
    };

    const countRes: any = await GetAllLprDetailsService(countRequest);
    if (countRes?.isSuccess) {
      totalCount = countRes?.data?.totalCount ?? 0;
    }
  } catch (err) {
    console.error("Error fetching total count:", err);
    return;
  }

  if (totalCount === 0) {
    showToast(t("No_data_found"), "error");
    return;
  }

  // ── step 2: fetch all records ──────────────────────────────────────────────
  let allRows: IOwnerList[] = [];
  try {
    const allRequest: IGetAllLPRData = {
      searchText: searchTerm.length > 0 ? searchTerm : "",
      pageNumber: 1,
      pageSize: totalCount,
      sortBy,
      sortOrder,
      floorIds: formValues.floorIds,
      zoneIds:  formValues.zoneIds,
      deviceIds: formValues.deviceIds,
      countryName: selectedCountry?.countryName || "",
      fromDate,
      toDate,
    };

    const allRes: any = await GetAllLprDetailsService(allRequest);
    if (allRes?.isSuccess) {
      allRows = allRes?.data?.items ?? [];
    }
  } catch (err) {
    console.error("Error fetching all records for PDF:", err);
    return;
  }

  if (allRows.length === 0) {
    showToast(t("No_data_found"), "error");
    return;
  }

  // ── step 3: fetch images for all rows ─────────────────────────────────────
  const pdfImageMap: Record<string, string> = {};
  await Promise.all(
    allRows.map(async (item) => {
      if (!item.smallImage) return;
      // reuse already loaded imageMap first to avoid re-fetching
      if (imageMap[item.smallImage]) {
        pdfImageMap[item.smallImage] = imageMap[item.smallImage];
        return;
      }
      try {
        const blob = await GetLPRImageService({
          imageName: item.smallImage,
          size: "small",
        } as ImagePayload);
        pdfImageMap[item.smallImage] = await blobToBase64(blob);
      } catch {
        // image unavailable — cell will show N/A
      }
    })
  );

  // ── step 4: build PDF ──────────────────────────────────────────────────────
  const doc = new jsPDF({ unit: "in", format: "a4", orientation: "landscape" });
  const pageWidth  = doc.internal.pageSize.getWidth();
  const pageHeight = doc.internal.pageSize.getHeight();

  const viLogo = await convertBase64ToPngBase64("/images/vision_insight_logo_pdf.png");
  const generalSettings = localStorage.getItem("generalSettings");
  let appLogo: string | null = null;
  if (generalSettings) {
    const parsed = JSON.parse(generalSettings);
    if (parsed?.logo) appLogo = await convertBase64ToPngBase64(parsed.logo);
  }

  // ── draw header on page 1 ─────────────────────────────────────────────────
  if (appLogo) doc.addImage(appLogo, "PNG", 0.2, 0.1, 1.2, 0.3);
  doc.addImage(viLogo, "PNG", pageWidth - 1.6, 0.1, 1.2, 0.3);
  doc.setDrawColor(180);
  doc.setLineWidth(0.01);
  doc.line(0.2, 0.55, pageWidth - 0.2, 0.55);

  // ── filter summary table ───────────────────────────────────────────────────
  const selectedFloorNames = floorList
    .filter((f) => formValues.floorIds?.includes(f.id as string))
    .map((f) => f.title)
    .join(", ") || "-";

  const selectedZoneNames = zoneList
    .filter((z) => formValues.zoneIds?.includes(z.id as string))
    .map((z) => z.title)
    .join(", ") || "-";

  const selectedDeviceNames = (deviceList ?? [])  
    .filter((d) => formValues.deviceIds?.includes(d.id as string))
    .map((d) => d.title)
    .join(", ") || "-";

  const selectedCountryName = selectedCountry?.countryName || "-";

  const reportPeriod = `${from.format("MM/DD/YYYY hh:mm A")} – ${to.format("MM/DD/YYYY hh:mm A")}`;

  const filterRows = [
    [ "Report Period",   reportPeriod         ],
    ["CreatedOn",   dayjs().format("YYYY-MM-DD HH:mm:ss") ],
    [ "Select Floor",  selectedFloorNames   ],
    [ "Select Zone",   selectedZoneNames    ],
    [ "Device",          selectedDeviceNames  ],
    [ "Country",   selectedCountryName  ],
  ];

  autoTable(doc, {
    body: filterRows,
    startY: 0.65,
    margin: { left: 0.2, right: 0.2 },
    theme: "plain",                            
    styles: {
      fontSize: 8,
      cellPadding: 0.1,
      textColor: 30,
      lineColor: [220, 220, 220],         
      lineWidth: 0.01,
    },
    columnStyles: {
      0: {
        fontStyle: "bold",
        fillColor: [255, 255, 255],
        textColor: 30,
        cellWidth: 2,
      },
      1: {
        fillColor: [255, 255, 255],
        textColor: 30,
        cellWidth: pageWidth - 0.4 - 2,
      },
    },
  });
  // ── data table starts right after filter table ─────────────────────────────
  const filterTableEndY = (doc as any).lastAutoTable.finalY + 0.15;

  const tableColumns = [
    "Plate",
    "Plate Image",
    "Country",
    "State",
    "Entry Gate",
    "Entry Time",
    "Exit Gate",
    "Exit Time",
    "Owner Name",
    "Registration Type",
    "Building",
    "Building Unit",
    "Contact Number",
    "Email",
    "Make",
    "Model",
    "Color",
    "Message",
    "CreatedOn",
  ];

  const tableRows = allRows.map((row) => [
    row.plate            ?? "",
    row.smallImage       ?? "",
    row.country          ?? "",
    row.state            ?? "",
    row.entryGate        ?? "",
    row.entryTime        ? formatDate(formatDateToConfiguredTimezone(row.entryTime),  timeFormat) : "",
    row.exitGate         ?? "",
    row.exitTime         ? formatDate(formatDateToConfiguredTimezone(row.exitTime),   timeFormat) : "",
    row.ownerName        ?? "",
    row.registrationType ?? "",
    row.building         ?? "",
    row.buildingUnit     ?? "",
    row.contact          ?? "",
    row.email            ?? "",
    row.make             ?? "",
    row.model            ?? "",
    row.color            ?? "",
    row.message          ?? "",
    row.createdOn        ? formatDate(formatDateToConfiguredTimezone(row.createdOn),  timeFormat) : "",
  ]);

  const IMAGE_COL  = 1;
  const ROW_HEIGHT = 0.55;

  autoTable(doc, {
    head: [tableColumns],
    body: tableRows,
    startY: filterTableEndY,
    margin: { left: 0.2, right: 0.2, bottom: 0.6 },
    styles: {
      fontSize: 7,
      cellPadding: 0.04,
      overflow: "linebreak",
      minCellHeight: ROW_HEIGHT,
    },
    headStyles: {
      fillColor: [254, 101, 0],
      textColor: 255,
      fontStyle: "bold",
      fontSize: 7,
    },
    alternateRowStyles: { fillColor: [255, 245, 235] },
    columnStyles: {
      [IMAGE_COL]: { cellWidth: 1.2 },
    },

    willDrawCell: (data) => {
      if (data.section === "body" && data.column.index === IMAGE_COL) {
        data.cell.text = [];
      }
    },

    didDrawCell: (data) => {
      if (data.section === "body" && data.column.index === IMAGE_COL) {
        const imageKey = data.cell.raw as string;
        const base64   = pdfImageMap[imageKey];

        if (base64) {
          const padding = 0.03;
          doc.addImage(
            base64,
            "JPEG",
            data.cell.x + padding,
            data.cell.y + padding,
            data.cell.width  - padding * 2,
            data.cell.height - padding * 2,
          );
        } else {
          // ── no image — draw "N/A" centered in cell ──────────────────────
          doc.setFontSize(7);
          doc.setTextColor(150);
          const textX = data.cell.x + data.cell.width  / 2;
          const textY = data.cell.y + data.cell.height / 2;
          doc.text("N/A", textX, textY, { align: "center", baseline: "middle" });
          doc.setTextColor(0);
        }
      }
    },

    didDrawPage: () => {
      const pageNum = (doc as any).internal.getCurrentPageInfo().pageNumber;
      const total   = (doc as any).internal.getNumberOfPages();

      // ── header on page 2+ (page 1 already drawn above) ───────────────────
      if (pageNum > 1) {
        if (appLogo) doc.addImage(appLogo, "PNG", 0.2, 0.1, 1.2, 0.3);
        doc.addImage(viLogo, "PNG", pageWidth - 1.6, 0.1, 1.2, 0.3);
        doc.setDrawColor(180);
        doc.setLineWidth(0.01);
        doc.line(0.2, 0.55, pageWidth - 0.2, 0.55);
      }

      // ── footer every page ─────────────────────────────────────────────────
      doc.setFontSize(9);
      doc.setTextColor(100);
      doc.text(`Vision Insight | ${t("LPR_Screen.LPR_header")}`, 0.2, pageHeight - 0.2);
      doc.text(`Page ${pageNum} of ${total}`, pageWidth - 0.2, pageHeight - 0.2, { align: "right" });
      doc.line(0.2, pageHeight - 0.35, pageWidth - 0.2, pageHeight - 0.35);
    },
  });

  doc.save("LPR_Report.pdf");
}; 

return (
    <>
      <div className="main-dashbourd-wrapper">
        <div className="top-orange-head LPR-orange-header" style={backgroundStyle}>
          <Box className="top-orange-head-left">
            <Typography variant="h4">{t("LPR_Screen.LPR_header")}</Typography>
            <Typography>{t("LPR_Screen.Description")}</Typography>
          </Box>
          <CustomButton onClick={handleLPRCSVDownload}>
           <img src="images/csv.svg" alt="" />&nbsp;
            {t("LPR_Screen.CSV")}
          </CustomButton>
          <CustomButton
            variant="outlined"
            onClick={handleExportPdf}
          >
            <img src="images/pdf.svg" alt="" />&nbsp;
            {t("LPR_Screen.PDF")}
          </CustomButton>
        </div>
        

        <div className="top-list-bar">
          <Typography variant="h5" gutterBottom>
            {t("LPR_Screen.List_LPR_Label")}
          </Typography>

          <div className="top-listing-items">
            <TextField
              placeholder={t("LPR_Screen.Search_LPR_Placeholder")}
              variant="outlined"
              // size="small"
              value={searchTerm}
              onChange={handleSearch}
              sx={{ borderRadius: 8 }}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <img
                      src={"/images/search.svg"}
                      alt="Search"
                      style={{ cursor: "pointer" }}
                    />
                  </InputAdornment>
                ),
              }}
            />
          </div>
        </div>

        <div className="ncr-top-orange-head-maintenance top-orange-head-maintenance">
          <div className="maintenance-schedule-items">
            <CustomMultiSelect
              name="floorIds"
              control={control}
              label={t("Maintenance_Schedule.Select_Floor")}
              options={floorList}
              placeholder={t("Maintenance_Schedule.Select_PH_Floor")}
            />
          </div>
          <div className="maintenance-schedule-items">
            <CustomMultiSelect
              name="zoneIds"
              control={control}
              label={t("Maintenance_Schedule.Select_Zone")}
              options={zoneList}
              placeholder={t("Maintenance_Schedule.Select_PH_Zone")}
            />
          </div>

          <div className="maintenance-schedule-items">
            <CustomMultiSelect
              name="deviceIds"
              control={control}
              label={t("LPR_Screen.Filters.Device")}
              options={deviceList}
              placeholder={t("LPR_Screen.Filters.Device")}
            />
          </div>
          <div className="maintenance-schedule-items">
            <CustomSelect
              name="countryName"
              variant="filled"
              control={control}
              label={t("LPR_Screen.Filters.Country_Label")}
              options={countryList}
              placeholder={t("LPR_Screen.Filters.Country_PH")}
            />
          </div>

          <LocalizationProvider dateAdapter={AdapterDayjs}>
            <Box className="maintenance-schedule-items">
              <FormLabel><span>{t("LPR_Screen.Filters.EntryTime_Label")}</span></FormLabel>

              <Controller
                name="fromDate"
                control={control}
                render={({ field, fieldState }) => (
                  <DateTimePicker
                    value={field.value}
                    format={timeFormat === "24h"
                      ? "DD-MM-YYYY HH:mm"
                      : "DD-MM-YYYY hh:mm A"}
                    ampm={timeFormat !== "24h"}
                    onChange={field.onChange}
                    slotProps={{
                      textField: {
                        error: !!fieldState.error,
                        helperText: fieldState.error?.message,
                      },
                    }}
                  />
                )}
              />
            </Box>

            <Box className="maintenance-schedule-items">
              <FormLabel><span>{t("LPR_Screen.Filters.ExitTime_Label")}</span></FormLabel>
              <Controller
                name="toDate"
                control={control}
                render={({ field, fieldState }) => (
                  <DateTimePicker
                    value={field.value}
                    format={timeFormat === "24h"
                      ? "DD-MM-YYYY HH:mm"
                      : "DD-MM-YYYY hh:mm A"}
                    ampm={timeFormat !== "24h"}
                    onChange={field.onChange}
                    minDateTime={getValues("fromDate")}
                    slotProps={{
                      textField: {
                        error: !!fieldState.error,
                        helperText: fieldState.error?.message,
                      },
                    }}
                  />
                )}
              />
            </Box>
          </LocalizationProvider>

          <div className="dashbourd-retail-details-export">
            <CustomButton variant="outlined" onClick={() => fetchLPRData()}>
              <img src="images/search.svg" alt="" />
            </CustomButton>
          </div>
        </div>
        <div id="lpr-grid-wrapper">    
        <DataGrid
          rows={lPRList ?? []}
          columns={columns}
          getRowId={(row) => row.id}
          paginationModel={paginationModel}
          onPaginationModelChange={handlePaginationModelChange}
          rowCount={lPRList?.length === 0 ? 0 : TotalRecord}
          paginationMode="server"
          pageSizeOptions={[5, 10, 15, 20, 25,100]}
          sortingMode="server"
          sortModel={sortModel}
          onSortModelChange={handleSortModelChange}
          disableRowSelectionOnClick
          slots={{
            noRowsOverlay: CustomNoRowsOverlay,
          }}
          hideFooter={(lPRList ?? []).length === 0}
        />
       
        </div>
      </div>
    </>
  );
};

export default LPR;
