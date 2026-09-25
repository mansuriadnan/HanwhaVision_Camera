export interface HealthStyle {
  color: string;
  border: string;
  background: string;
  label: string;
}
export const getHealthClass = (health: string): string => {
  switch (health?.toLowerCase()) {
    case "ok":
      return "health-ok";

    case "warning":
      return "health-warning";

    case "critical":
      return "health-critical";

    default:
      return "health-default";
  }
};



export const getSeverityClass = (severity: string): string => {
  switch (severity?.toLowerCase()) {
    case "critical":
      return "severity-critical";

    case "warning":
      return "severity-warning";

    case "ok":
      return "severity-ok";

    default:
      return "severity-default";
  }
};