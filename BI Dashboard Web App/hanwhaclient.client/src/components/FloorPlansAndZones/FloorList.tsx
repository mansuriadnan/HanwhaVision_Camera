import React, { useEffect, useState } from "react";
import {
  IFloorPlan,
  FloorListProps,
  IAddFloor,
} from "../../interfaces/IFloorAndZone";
import {
  Box,
  TextField,
  List,
  ListItem,
  IconButton,
  Menu,
  MenuItem,
  ListItemText,
} from "@mui/material";
import { Search, MoreVert } from "@mui/icons-material";
import {
  DeleteFloorService,
  GetAllFloorService,
  AddFloorService,
} from "../../services/floorPlanService";
import { COMMON_CONSTANTS, REGEX } from "../../utils/constants";
import { useForm } from "react-hook-form";
import { HasPermission } from "../../utils/screenAccessUtils";
import { LABELS } from "../../utils/constants";
import { CommonDialog } from "../Reusable/CommonDialog";
import { CustomTextFieldWithButton } from "../Reusable/CustomTextFieldWithButton";
import { useTranslation } from "react-i18next";

const FloorList: React.FC<FloorListProps> = ({
  onSelectFloor,
  onDeleteFloor,
  OnDeleteAllFloor,
}) => {
  const { t } = useTranslation();
  const [floorList, SetfloorList] = useState<IFloorPlan[] | undefined>();
  const [selectedFloorId, setSelectedFloorId] = useState("");
  const [openFloorDeleteConfirm, setOpenFloorDeleteConfirm] = useState(false);
  const [floorToBeDelete, setFloorToBeDelete] = useState("");
  const [floorEditItem, setFloorEditItem] = useState<IFloorPlan | undefined>();
  const [searchTerm, setSearchTerm] = useState("");
  const [menuAnchor, setMenuAnchor] = useState<{
    anchorEl: HTMLElement | null;
    item: any | null;
  }>({
    anchorEl: null,
    item: null,
  });

  const {
    control: addControl,
    handleSubmit: handleAddSubmit,
    reset: resetAddForm,
  } = useForm<IAddFloor>({
    defaultValues: {
      floorName: "",
    },
  }); // for Add

  const {
    control: editControl,
    handleSubmit: handleEditSubmit,
    setValue: setEditValue,
    reset: resetEditForm,
  } = useForm<IAddFloor>({
    defaultValues: {
      floorName: "",
    },
  }); // for Edit

  const open = Boolean(menuAnchor.anchorEl);

  useEffect(() => {
    fetchFloorData();
  }, []);

  const handleSearch = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(event.target.value);
  };

  const filteredFloor =
    searchTerm?.length >= 3
      ? floorList?.filter((floor) =>
          floor.floorPlanName?.toLowerCase().includes(searchTerm.toLowerCase())
        )
      : floorList;

  const handleClose = () => {
    setMenuAnchor({ anchorEl: null, item: null });
  };

  const fetchFloorData = async () => {
    try {
      const floorData = await GetAllFloorService();

      // console.log("floorData->", floorData);

      if (floorData && floorData?.length && floorData?.length > 0) {
        const Id = floorData?.[0].id;
        setSelectedFloorId(Id as string);
        onSelectFloor && onSelectFloor(Id as string);
        SetfloorList(floorData as IFloorPlan[]);
      } else {
        OnDeleteAllFloor && OnDeleteAllFloor();
        SetfloorList([]);
      }
    } catch (err: any) {
      console.error("Error fetching initial data:", err);
    }
  };

  const AddFloorPlan = async (data: IAddFloor) => {
    try {
      const newFloor: IFloorPlan = {
        id: "",
        floorPlanName: data.floorName,
      };

      var result: any = await AddFloorService(newFloor as IFloorPlan);
      if (result && result.isSuccess) {
        fetchFloorData();
        resetAddForm();
        setFloorEditItem(undefined);
      }
      // console.log("result", result);
    } catch (error) {
      console.error("Submission Error:", error);
    }
  };

  const EditFloorPlan = async (data: IAddFloor) => {
    try {
      if (!floorEditItem) return;

      const updatedFloor: IFloorPlan = {
        ...floorEditItem,
        floorPlanName: data.floorName,
      };
      const result: any = await AddFloorService(updatedFloor as IFloorPlan);
      if (result && result.isSuccess) {
        fetchFloorData();
        resetEditForm();
        setFloorEditItem(undefined);
      }
    } catch (error) {
      console.error("Edit Error:", error);
    }
  };

  const DeleteFloor = async (id: string) => {
    try {
      const deleteData : any = await DeleteFloorService({id});

      if (deleteData?.isSuccess) {
        setOpenFloorDeleteConfirm(false);
        fetchFloorData();
        handleClose();
        onDeleteFloor();
      }
    } catch (err: any) {
      console.error("Error deleting role:", err);
    }
  };

  return (
    <>
      <Box className="search-plans-and-jones">
        <TextField
          className="search-bar-plans-and-jones"
          placeholder={t("Floor_Plan_zones.Search_floor_placeholder")}
          fullWidth
          onChange={handleSearch}
          variant="outlined"
          InputProps={{
            endAdornment: <Search />,
          }}
        />

        <List className="list-plans-and-jones">
          {filteredFloor?.map((item) => (
            <React.Fragment key={item.id}>
              {floorEditItem?.id === item.id ? (
                <Box
                  component="form"
                  onSubmit={handleEditSubmit(EditFloorPlan)}
                  key={item.id}
                >
                  <CustomTextFieldWithButton
                    ShowAddButton={false}
                    name="floorName"
                    control={editControl}
                    rules={{
                      required: "Floor name is required",
                      maxLength: {
                        value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                        message: "Floor name cannot exceed 50 characters",
                      },
                      minLength: {
                        value: COMMON_CONSTANTS.MIN_TEXT_FIELD_LENGTH,
                        message: "Floor name must have atleast 3 characters.",
                      },
                    }}
                    customStyles={{
                      backgroundColor:
                        selectedFloorId === item.id ? "orange" : "transparent",
                      color: selectedFloorId === item.id ? "white" : "black",
                      marginBottom: 5,
                      cursor: "pointer",
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center",
                      background: "url('images/background.svg') ",
                      backgroundSize: "cover",
                      backgroundPosition: "center",
                      backgroundRepeat: "no-repeat",
                      borderRadius: 20,
                    }}
                  />
                </Box>
              ) : (
                <ListItem
                  key={item.id}
                  className={selectedFloorId === item.id ? "active" : ""}
                  sx={{
                    backgroundImage:
                      selectedFloorId === item.id
                        ? `url('images/background.svg'), linear-gradient(91.75deg, #FFCD7D -8.36%, #FF8A01 32.16%, #FF8A01 77.32%, #FFCD7D 88.72%, #FF8A01 101.13%)`
                        : `url('images/background.svg')`,
                    bgcolor: "transparent",
                    color: selectedFloorId === item.id ? "#fff" : "#090909;",
                  }}
                  onClick={() => {
                    if (selectedFloorId !== item.id) {
                      setSelectedFloorId(item.id);
                      onSelectFloor && onSelectFloor(item.id);
                    }
                  }}
                >
                  {item.floorPlanName}
                  <IconButton
                    size="small"
                    // onClick={handleClick}
                    onClick={(e) => {
                      e.stopPropagation();
                      setMenuAnchor({ anchorEl: e.currentTarget, item });
                    }}
                  >
                    <MoreVert />
                  </IconButton>
                </ListItem>
              )}

              {menuAnchor?.item?.id === item.id && (
                <Menu
                  // anchorEl={anchorEl}
                  anchorEl={menuAnchor.anchorEl}
                  open={open}
                  onClose={handleClose}
                  PaperProps={{
                    elevation: 3,
                    sx: {
                      borderRadius: 2,
                      pl: 1,
                      pr: 1,
                    },
                  }}
                  className="edit-delete-pop"
                >
                  {HasPermission(LABELS.CanAddOrUpdateFloor) && (
                    <MenuItem
                      onClick={() => {
                        setFloorEditItem(item);
                        setEditValue("floorName", item.floorPlanName);
                        handleClose();
                      }}
                    >
                      <IconButton>
                        <img
                          src={"/images/edit.svg"}
                          alt="edit"
                          width={20}
                          height={20}
                        />
                      </IconButton>
                      <ListItemText primary={t("Common_Edit_Delte_Menu.Edit")} />
                    </MenuItem>
                  )}
                  {HasPermission(LABELS.CanDeleteFloor) && (
                    <MenuItem
                      onClick={() => {
                        setFloorToBeDelete(item.id);
                        setOpenFloorDeleteConfirm(true);
                      }}
                    >
                      <IconButton>
                        <img
                          src={"/images/delete_gray.svg"}
                          alt="delete"
                          width={20}
                          height={20}
                        />
                      </IconButton>
                      <ListItemText primary={t("Common_Edit_Delte_Menu.Delete")} />
                    </MenuItem>
                  )}
                </Menu>
              )}
            </React.Fragment>
          ))}
        </List>
        {HasPermission(LABELS.CanAddOrUpdateFloor) && (
          <Box
            component="form"
            onSubmit={handleAddSubmit(AddFloorPlan)}
            sx={{ marginTop: 2 }}
            className="search-custom-button"
          >
            <CustomTextFieldWithButton
              name="floorName"
              control={addControl}
              rules={{
                required: t("Floor_Plan_zones.validation.Floor_name_is_required"),
                pattern: {
                  value: REGEX.Floor_Zone_Name_Regex,
                  message:
                    t("Floor_Plan_zones.validation.Floor_name_strong_validation"),
                },
                maxLength: {
                  value: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH,
                  message:  t("Floor_Plan_zones.validation.Floor_name_MIN_validation",{ max: COMMON_CONSTANTS.MAX_TEXT_FIELD_LENGTH }),
                },
                minLength: {
                  value: COMMON_CONSTANTS.MIN_TEXT_FIELD_LENGTH,
                  message:  t("Floor_Plan_zones.validation.Floor_name_MIN_validation", { min : COMMON_CONSTANTS.MIN_TEXT_FIELD_LENGTH } ),
                },
              }}
              placeholder={t("Floor_Plan_zones.Floor_plan_name_placeholder")}
            />
          </Box>
        )}
      </Box>

      <CommonDialog
        open={openFloorDeleteConfirm}
        customClass="cmn-confirm-delete-icon"
        title={t("Common_DELETE_Confirmation_Dialog.Title")}
        content={t("Common_DELETE_Confirmation_Dialog.Content")}
        onConfirm={() => floorToBeDelete && DeleteFloor(floorToBeDelete)}
        onCancel={() => setOpenFloorDeleteConfirm(false)}
        confirmText={t("Common_DELETE_Confirmation_Dialog.Delete")}
        cancelText={t("Common_DELETE_Confirmation_Dialog.Cancel")}
        type="delete"
        titleClass={true}
      />
    </>
  );
};

export { FloorList };
