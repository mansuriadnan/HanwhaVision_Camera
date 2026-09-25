import React, { useState } from "react";
import { Box, IconButton, Tooltip, Typography, TextField } from "@mui/material";
import { IEditableWidgetNameProps } from "../../interfaces/IChart";

const CustomEditableWidgetName: React.FC<IEditableWidgetNameProps> = ({
  displayName,
  onChangeWidgetName,
}) => {
  const [hover, setHover] = useState(false);
  const [editable, setEditable] = useState(false);
  const [widgetName, setWidgetName] = useState(displayName);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      const inputValue = (e.target as HTMLInputElement).value;
      setEditable(false);
      setWidgetName(inputValue);
      onChangeWidgetName?.(inputValue);
    }
  };

  return (
    <Box
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      className="common-popup-head"
    >
      {editable ? (
        <TextField
          // variant="outlined"
          variant="standard"
          size="small"
          value={widgetName}
          onChange={(e) => setWidgetName(e.target.value)}
          onKeyDown={handleKeyDown}
          autoFocus
          InputProps={{
            disableUnderline: true,
            style: {
              border: "1px solid #CDCDCD",
              borderRadius: 5,
              padding: -2,
            },
          }}
          // InputProps={{
          //               style: {
          //                 borderRadius: "50px", // Rounded corners
          //                 height: "50px",
          //               },
          //               endAdornment: ShowAddButton ? (
          //                 <Button
          //                   type="submit"
          //                   variant="contained"
          //                   sx={{
          //                     borderRadius: "50px",
          //                     backgroundColor: "#ff8c00", // Orange color
          //                     color: "#fff",
          //                     minWidth: "80px",
          //                     height: "40px",
          //                     textTransform: "none",
          //                   }}
          //                 >
          //                   + Add
          //                 </Button>
          //               ) : null,
          //             }}
        />
      ) : (
        <>
          <Typography variant="h6" component="h2">
            {widgetName}
          </Typography>
          <Box
            sx={{
              visibility: hover ? "visible" : "hidden",
            }}
          ></Box>
          <Tooltip title="Edit Widget Name">
            <IconButton onClick={() => setEditable(true)}>
             
              <svg xmlns="http://www.w3.org/2000/svg" id="Line" viewBox="0 0 24 24"><path d="m4.76 18.81s.08 0 .11 0l2.93-.27c.41-.04.79-.22 1.08-.51l11.06-11.06c.52-.52.81-1.21.81-1.94s-.29-1.42-.81-1.94l-.71-.71c-1.04-1.04-2.85-1.04-3.89 0l-1.41 1.41s0 0 0 0l-9.64 9.64c-.29.29-.47.67-.5 1.08l-.27 2.93c-.03.37.1.73.36 1 .24.24.55.37.88.37zm12.53-15.74c.32 0 .64.12.88.37l.71.71c.24.24.37.55.37.88s-.13.65-.37.88l-.88.88-2.47-2.47.88-.88c.24-.24.56-.37.88-.37zm-12.01 11.58c0-.06.03-.11.07-.15l9.11-9.12 2.47 2.47-9.11 9.11s-.1.07-.15.07l-2.63.24.24-2.63zm17.47 7.35c0 .41-.34.75-.75.75h-20c-.41 0-.75-.34-.75-.75s.34-.75.75-.75h20c.41 0 .75.34.75.75z"/></svg>
            </IconButton>
          </Tooltip>
        </>
      )}
    </Box>
  );
};

export { CustomEditableWidgetName };
