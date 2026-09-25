import React from "react";
import {
  FormControl,
  FormControlLabel,
  FormLabel,
  Switch,
} from "@mui/material";
import { Controller, Control, FieldValues, Path } from "react-hook-form";

type CustomSwitchProps<T extends FieldValues> = {
  name: Path<T>;
  control: Control<T>;
  label?: React.ReactNode;
  rules?: any;
  customStyles?: React.CSSProperties;
  disabled?: boolean;
};

export const CustomSwitch = <T extends FieldValues>({
  name,
  control,
  label,
  rules,
  customStyles,
  disabled = false,
}: CustomSwitchProps<T>) => {
  return (
    <FormControl style={{ width: "100%", ...customStyles }}>
      {label && (
        <FormLabel
          style={{ marginBottom: 4, fontWeight: 500, fontSize: "14px" }}
        >
          {label}
        </FormLabel>
      )}

      <Controller
        name={name}
        control={control}
        rules={rules}
        render={({ field }) => (
          // <FormControlLabel
          //   control={
              <Switch
                checked={Boolean(field.value)}
                onChange={(e) => field.onChange(e.target.checked)}
                disabled={disabled}
              />
            // }
          //   label=""
          // />
        )}
      />
    </FormControl>
  );
};
