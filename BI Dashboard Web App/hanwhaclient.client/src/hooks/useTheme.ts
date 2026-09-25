// hooks/useTheme.ts
import { useEffect, useState } from "react";
import { THEME_COLOR_OPTIONS } from "../utils/constants";
import { LOCAL_LOADER_THEME_COLORS } from "../utils/constants";

export type Theme = "light" | "dark";
export type ThemeColor = string;

function useTheme(): [
  Theme,
  (theme: Theme) => void,
  ThemeColor,
  (color: ThemeColor) => void
] {
  const [theme, setTheme] = useState<Theme>(() => {
    return (localStorage.getItem("theme") as Theme) || "light";
  });

    const [themeColor, setThemeColor] = useState<ThemeColor>(() => {
    return (localStorage.getItem("themeColor") as ThemeColor) || "default-theme";
  });

  
 useEffect(() => {
  // Remove old theme (light/dark)
  document.body.classList.remove("light", "dark");
 
  // Remove old theme colors based on config array
  const colorClassNames = THEME_COLOR_OPTIONS.map(opt => opt.id);
  const bodyClasses = Array.from(document.body.classList);
 
  bodyClasses.forEach((cls) => {
    if (colorClassNames.includes(cls)) {
      document.body.classList.remove(cls);
    }
  });
    if (themeColor) {
    document.body.classList.add(themeColor);
    const loaderColor = LOCAL_LOADER_THEME_COLORS[themeColor];
      if (loaderColor) {
        localStorage.setItem("loaderColor", loaderColor);
      }
  }
 
  document.body.classList.add(theme);
 
 
  localStorage.setItem("theme", theme);
  localStorage.setItem("themeColor", themeColor);
}, [theme, themeColor]);


  return [theme, setTheme, themeColor, setThemeColor];
}

export default useTheme;
