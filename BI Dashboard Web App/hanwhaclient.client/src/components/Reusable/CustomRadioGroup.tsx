import React from "react";
import {
  FormControl,
  FormControlLabel,
  FormLabel,
  FormHelperText,
  Radio,
  RadioGroup,
  FormControlProps,
  RadioGroupProps,
} from "@mui/material";
import { Control, Controller, FieldValues, Path } from "react-hook-form";

type RadioOption = {
  label: React.ReactNode;
  value: string;
  disabled?: boolean;
};

type CustomRadioGroupProps<T extends FieldValues> = {
  name: Path<T>;
  control: Control<T>;
  options: RadioOption[];
  label?: React.ReactNode;
  rules?: any;
  row?: boolean;
  formControlProps?: FormControlProps;
  radioGroupProps?: Omit<
    RadioGroupProps,
    "row" | "name" | "value" | "onChange"
  >;
};

const CustomRadioGroup = <T extends FieldValues>({
  name,
  control,
  options,
  label,
  rules,
  row = true,
  formControlProps,
  radioGroupProps,
}: CustomRadioGroupProps<T>) => (
  <FormControl>
    {label && (
      <FormLabel sx={{ mb: 0.5, fontWeight: 500, fontSize: "14px" }}>
        {label}
      </FormLabel>
    )}
    <Controller
      name={name}
      control={control}
      rules={rules}
      render={({ field, fieldState: { error } }) => (
        <FormControl fullWidth error={!!error} {...formControlProps}>
          <RadioGroup row={row} {...field} {...radioGroupProps}>
            {options.map((option) => (
              <FormControlLabel
                key={String(option.value)}
                value={option.value}
                control={<Radio />}
                label={option.label}
                disabled={option.disabled}
              />
            ))}
          </RadioGroup>
          {error && <FormHelperText>{error.message}</FormHelperText>}
        </FormControl>
      )}
    />
  </FormControl>
);

export {CustomRadioGroup};