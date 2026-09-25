import React, { useState, useEffect } from "react";
import {
  Typography,
  Container,
  TextField,
  Box,
  Chip,
  IconButton,
  Tooltip,
  InputAdornment,
} from "@mui/material";
// import {
//   Edit as EditIcon,
//   Delete as DeleteIcon,
//   Visibility as ViewIcon,
// } from "@mui/icons-material";
// import { useLocation, useNavigate } from "react-router-dom";
import { IUsers, referenceDatatype } from "../../interfaces/IGetAllUsers";
import {
  DeleteUserService,
  GetAllUsersService,
} from "../../services/userService";
import { IRole } from "../../interfaces/IRole";
import { IRoleDelete } from "../../interfaces/IRoleUser";
// import handleResponse from "../../utils/handleResponse";
import { DataGrid, GridColDef } from "@mui/x-data-grid";
import CommonDialog from "../../components/Reusable/CommonDialog";
// import { usePermissions } from "../../context/PermissionsContext";
// import { filterByScreenName } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { formatDate } from "../../utils/dateUtils";
import { CustomButton } from "../../components/index";
import { HasPermission } from "../../utils/screenAccessUtils";
import { RegionAddEditForm } from "./RegionAddEditForm";
import { DeleteRegionService, GetAllRegionService } from "../../services/regionService";
import { IRegion } from "../../interfaces/ICreateRegion";

const RegionManagement: React.FC = () => {
  const [regions, setRegions] = useState<IRegion[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [openDeleteConfirm, setOpenDeleteConfirm] = useState(false);
  const [regionToBeDelete, setRegionToBeDelete] = useState<IRoleDelete | null>(
    null
  );
  // const { permissions }: { permissions: any[] } = usePermissions();
  // const location = useLocation();
  // const label = location.state?.label;
  // const [filteredChildren, setFilteredChildren] = useState<any[]>([]);
  const [openAddRegion, setOpenAddRegion] = useState<boolean>(false);
  const [openEditRegion, setOpenEditRegion] = useState<boolean>(false);
  const [selectedRegion, setSelectedRegion] = useState<IUsers | undefined>(
    undefined
  );
  const [referenceData, setReferenceData] = useState<referenceDatatype>();
  const [paginationModel, setPaginationModel] = useState({
    pageSize: 10, // Default page size
    page: 0, // Default page index
  });

  useEffect(() => {
    fetchInitialData();
  }, []);

  const fetchInitialData = async () => {
    try {
      const regionData: any = await GetAllRegionService();
      setRegions(regionData.data as IRegion[]);
      setReferenceData(regionData.referenceData as any);
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };

  const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(event.target.value);
  };

  const handleCloseConfirm = () => {
    setOpenDeleteConfirm(false);
  };

  const finalDeleteRegion = async (id: string) => {
    try {
      const deleteData = await DeleteRegionService(id);
      if (deleteData.isSuccess) {
        setOpenDeleteConfirm(false);
        fetchInitialData();
      }
    } catch (err: any) { }
  };

  const filteredRegions = regions.filter(
    (region) =>
      region.name.toLowerCase().includes(searchTerm.toLowerCase()) 
    // Check if any role name matches the search term
    // user.roleIds.some((roleId) => {
    //   const role = totalRoles.find((role) => role.id === roleId);
    //   return role
    //     ? role.roleName.toLowerCase().includes(searchTerm.toLowerCase())
    //     : false;
    // })
  );

  const handleAddRegion = () => {
    setOpenAddRegion(true);
  };

  const handleCloseAddModal = () => {
    setOpenAddRegion(false);
  };

  const handleEdit = (user: IUsers) => {
    const userWithRoleIds = {
      ...user,
    };
    setSelectedRegion(userWithRoleIds);
    setOpenEditRegion(true);
  };

  const handleCloseEditModal = () => {
    setOpenEditRegion(false);
  };

  const handleDelete = (regionid: string) => {
    setRegionToBeDelete({ id: regionid });
    setOpenDeleteConfirm(true);
  };

  // const filteredEdit = filterByScreenName(
  //   filteredChildren,
  //   LABELS.Can_Add_Update_User
  // );

  // const filteredDelete = filterByScreenName(
  //   filteredChildren,
  //   LABELS.Can_Delete_USer
  // );

  const columns: GridColDef[] = [
    { field: "name", headerName: "Region Name", flex: 1, },
    {
      field: "createdBy",
      headerName: "Created By",
       flex: 1,
      renderCell: (params) => {
        const createdByID = params.row.createdBy as string;
        const filteredData =
          referenceData &&
          referenceData.createdBy.find((item) => item.value === createdByID);

        return <Box>{filteredData && <label>{filteredData.label}</label>}</Box>;
      },
    },
    {
      field: "createdOn",
      headerName: "Created On",
       flex: 1,
      renderCell: (params) => {
        return <span>{formatDate(params.value)}</span>;
      },
    },
    {
      field: "updatedBy",
      headerName: "Last Updated By",
       flex: 1,
      renderCell: (params) => {
        const updatedById = params.row.updatedBy as string;
        const filteredData =
          referenceData &&
          referenceData.updatedBy.find((item) => item.value === updatedById);

        return <Box>{filteredData && <label>{filteredData.label}</label>}</Box>;
      },
    },
    {
      field: "updatedOn",
      headerName: "Last Updated On",
      flex: 1,
      renderCell: (params) => {
        return <span>{formatDate(params.value)}</span>;
      },
    },
    {
      field: "actions",
      headerName: "Actions",
      // flex: 1,
      width:150,
      sortable: false,
      headerClassName: "static-column",
      cellClassName: "static-column",
      renderCell: (params) => (
        <Box>
          {HasPermission(LABELS.CanAddOrUpdateRegion) && (
            <Tooltip title="Edit Region">
              <IconButton onClick={() => handleEdit(params.row)}>
                <img
                  src={"/images/edit.svg"}
                  alt="Region Edit"
                  width={22}
                  height={22}
                />
              </IconButton>
            </Tooltip>
          )}
          {HasPermission(LABELS.CanDeleteRegion) && (
            <Tooltip title="Delete Region">
              <IconButton onClick={() => handleDelete(params.id as string)}>
                <img
                  src={"/images/trash.svg"}
                  alt="Region Delete"
                  width={22}
                  height={22}
                />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      ),
    },
  ];

  const CustomNoRowsOverlay = () => (
    <Box className="no-data-douns"
    >
      <Box sx={{ width: 200, justifyItems: "center", flex: 1, textAlign: "center", margin: "0 auto" }}>
        <img src={'/images/noData.gif'} alt="Animated GIF" width="100" height="100" />
        <Typography
          sx={{ FontWeight: 600, fontSize: 24, color: "#090909" }}
        >
          No data found
        </Typography>
        {/* <Typography
          sx={{ FontWeight: 400, fontSize: 12, color: "#212121" }}
        >
         No data available to display Users. Please add a new user by clicking the <strong>Add New User</strong> button.
        </Typography> */}
      </Box>
    </Box>
  );

  return (
    <>
      <Box className="rightbar-main-content">
        <Box className="top-dash-details">
          <Typography variant="h5">List of Region</Typography>
          <Box className="top-dash-details-right">
            <TextField
              //label="Search"
              variant="outlined"
              // fullWidth
              sx={{ flexGrow: 1, maxWidth: "500px", mr: 2, ml: 2 }}
              value={searchTerm}
              onChange={handleSearch}
              placeholder="Search by Region name"
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <img
                      src={"/images/search.svg"}
                      alt="Search"
                    />
                  </InputAdornment>
                ),
              }}
            />
            {HasPermission(LABELS.CanAddOrUpdateRegion) && (
              <CustomButton
                customStyles={{
                  ml: "auto",
                }}
                onClick={handleAddRegion}
              >
                Add New Region
              </CustomButton>
            )}
          </Box>
        </Box>
        <DataGrid
          rows={filteredRegions}
          columns={columns}
          getRowId={(row) => row.id}
          className="custom-grid"
          pagination
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          pageSizeOptions={[10, 20, 50, 100]}
          sx={{
            '& .MuiDataGrid-columnHeader': {
              backgroundColor: '#FDF1E8',
            }
          }}
          slots={{
            noRowsOverlay: CustomNoRowsOverlay,
          }}
          hideFooterSelectedRowCount   
       
        />
      </Box>

      <CommonDialog
        open={openDeleteConfirm}
        title="Delete Confirmation!"
        content="Are you sure you want to continue?"
        onConfirm={() => regionToBeDelete && finalDeleteRegion(regionToBeDelete.id)}
        onCancel={handleCloseConfirm}
        confirmText="Delete"
        cancelText="Cancel"
        type="delete"
        customClass="common-dialog-with-icon"
      />

      <CommonDialog
        open={openAddRegion}
        title={"Add New Region"}
        content={
          <RegionAddEditForm
            onClose={handleCloseAddModal}
            refreshData={fetchInitialData}
          />
        }
        onCancel={handleCloseAddModal}
        //cancelText="Cancel"
        customClass="add-edit-dialog"
      />
      <CommonDialog
        open={openEditRegion}
        title={"Edit Region"}
        content={
          <RegionAddEditForm
            region={selectedRegion}
            onClose={handleCloseEditModal}
            refreshData={fetchInitialData}
          />
        }
        onCancel={handleCloseEditModal}
        //cancelText="Cancel"
        customClass="add-edit-dialog"
      />
    </>
  );
};

export { RegionManagement };
