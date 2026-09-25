export const getUsageColor = (value: number) => {
  if (value <= 50) return "#07A70D";
  if (value <= 80) return "#FF8A00";
  return "#F40101";
};