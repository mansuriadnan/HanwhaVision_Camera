import { useTranslation } from "react-i18next";
import { useThemeContext } from "../../context/ThemeContext";

const WelcomePage = () => {
  const { theme, themeColor } = useThemeContext();
  const { t } = useTranslation();

  const isDefaultTheme = themeColor === "default-theme";

  const welcomeImgPath = isDefaultTheme
    ? theme === "dark"
      ? "dark-theme"
      : ""
    : theme === "dark"
      ? `${themeColor}/dark-theme`
      : `${themeColor}`;

  return (
    <div className="welcome-image">
      <h1>{t("welcome_to")}</h1>
      <span>{t("BI_DASHBOARD")}</span>

      <img
        src={`/images/${welcomeImgPath}/welcome-back.png`}
        alt="Welcome image"
      />
    </div>
  );
};

export default WelcomePage;
