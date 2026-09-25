import React, { useState } from "react";
import {
  Box,
  Paper,
  TableContainer,
  Table,
  TableCell,
  TableRow,
  TableBody,
  Button,
  Grid,
  IconButton,
  TableHead,
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import { useForm } from "react-hook-form";
import { CustomButton, CustomSelect, CustomTextField } from "../../components";
import { useTranslation } from "react-i18next";

export interface CalculatorExportRule {
  category: string;
  value: number;
  unit: "Percentage" | "Number";
  operation: "Add" | "Subtract";
}

interface AdvanceExportCalculatorProps {
  initialRules?: CalculatorExportRule[];
  onRulesChange?: (rules: CalculatorExportRule[]) => void;
}

interface FormValues {
  category: string;
  value: number | "";
  unit: "Percentage" | "Number";
  operation: "Add" | "Subtract";
}

const AdvanceExportCalculator: React.FC<AdvanceExportCalculatorProps> = ({
  initialRules = [],
  onRulesChange,
}) => {
  const [rules, setRules] = useState<CalculatorExportRule[]>(initialRules);

  const { control, handleSubmit, reset, watch, setValue } = useForm<FormValues>(
    {
      defaultValues: {
        category: "",
        value: undefined as any,
        unit: "Percentage",
        operation: "Add",
      },
    },
  );
  const { t } = useTranslation();

  const categories = [
    t("Advance_Export_Dialog.categories.staff"),
    t("Advance_Export_Dialog.categories.security"),
    t("Advance_Export_Dialog.categories.exhibitors"),
    t("Advance_Export_Dialog.categories.repeatedCustomers"),
  ];
  //  const categories = defaultCategories;

  const selectedCategory = watch("category");

  const availableCategories = categories.filter(
    (cat) => !rules.some((rule) => rule.category === cat),
  );

  const handleAdd = (data: FormValues) => {
    if (!data.category) return;

    const newRule: CalculatorExportRule = {
      category: data.category,
      value: data.value === "" ? Number(undefined) : Number(data.value),
      unit: data.unit,
      operation: data.operation,
    };

    const updatedRules = [...rules, newRule];
    setRules(updatedRules);
    onRulesChange?.(updatedRules);

    reset({
      category: "",
      value: undefined as any,
      unit: "Percentage",
      operation: "Add",
    });
    //  Clear only value field
    setValue("value", "");
  };

  const handleRemove = (id: string) => {
    const updatedRules = rules.filter((rule) => rule.category !== id);
    setRules(updatedRules);
    onRulesChange?.(updatedRules);
  };

  return (
    <Box
      className="advance-export-grid"
    >
      {rules.length > 0 && (
        <TableContainer sx={{ mb: 2, mt: 1 }}>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ fontWeight: 700 }}>
                <TableCell sx={{ width: "26%" }}>
                  {t("Advance_Export_Dialog.table.Category")}
                </TableCell>
                <TableCell sx={{ width: "26%" }}>
                  {t("Advance_Export_Dialog.table.Value")}
                </TableCell>
                <TableCell sx={{ width: "26%" }}>
                  {t("Advance_Export_Dialog.table.Unit")}
                </TableCell>
                <TableCell sx={{ width: "16.68%" }}>
                  {t("Advance_Export_Dialog.table.Operation")}
                </TableCell>
                <TableCell sx={{ width: "8.33%" }}>
                  {t("Advance_Export_Dialog.table.Action")}
                </TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rules.map((rule) => (
                <TableRow key={rule.category}>
                  <TableCell>{rule.category}</TableCell>
                  <TableCell>{rule.value}</TableCell>
                  <TableCell>{rule.unit}</TableCell>
                  <TableCell>{rule.operation}</TableCell>
                  <TableCell>
                    <IconButton onClick={() => handleRemove(rule.category)}>
                      <img
                        src={"/images/user-action-delete.svg"}
                        alt="User Action Delete Icon"
                        width={20}
                        height={20}
                      />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
      <Grid
        container
        spacing={1}
        component="form"
        onSubmit={handleSubmit(handleAdd)}
        // alignItems="center"
        className="advance-export"
      >
        {/* Category */}
        <Grid item xs={12} sm={3}>
          <CustomSelect
            name="category"
            control={control}
            placeholder={t("Advance_Export_Dialog.Category_Placeholder")}
            options={availableCategories.map((cat) => ({
              id: cat,
              title: cat,
            }))}
            rules={{ required: t("Advance_Export_Dialog.Category_Required") }}
            sx={{ margin: 0 }}
          />
        </Grid>

        {/* Value */}
        <Grid item xs={12} sm={3}>
          <CustomTextField
            name="value"
            control={control}
            type="number"
            placeholder={t("Advance_Export_Dialog.Value_Placeholder")}
            rules={{
             required:t("Advance_Export_Dialog.Value_required"),
              min: {
                value: 1,
                message: t("Advance_Export_Dialog.Minimum_Value"),
              },
              max: {
                value: 9999,
                message: t("Advance_Export_Dialog.Maximum_Value"),
              },
            }}
          />
        </Grid>

        {/* Unit */}
        <Grid item xs={12} sm={3}>
          <CustomSelect
            name="unit"
            control={control}
            options={[
              {
                id: "Percentage",
                title: t("Advance_Export_Dialog.units.percentage"),
              },
              // { id: "Number", title: t("Advance_Export_Dialog.units.number") },
            ]}
          />
        </Grid>

        {/* Operation */}
        <Grid item xs={12} sm={2}>
          <CustomSelect
            name="operation"
            control={control}
            options={[
              { id: "Add", title: t("Advance_Export_Dialog.operations.add") },
              {
                id: "Subtract",
                title: t("Advance_Export_Dialog.operations.subtract"),
              },
            ]}
          />
        </Grid>

        {/* Button */}
        <Grid
          item
          xs={12}
          sm={1}
          sx={{ display: "flex", justifyContent: "center" }}
          className="advance-report-buttons-wrapper"
        >
          <CustomButton
            type="submit"
            size="medium"
            className="common-btn-design"
          >
            {t("Add_btn")}
          </CustomButton>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AdvanceExportCalculator;
