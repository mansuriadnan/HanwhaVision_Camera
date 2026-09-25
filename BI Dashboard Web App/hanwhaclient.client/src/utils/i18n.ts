// src/utils/i18n.ts
import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import LanguageDetector from "i18next-browser-languagedetector";

// Import translation JSON files
import en from "../locales/English/translation.json"; //english
import ar from "../locales/Arebic/translation.json"; //arebic
import zh from "../locales/Chinese/translation.json"; // Chinese
import cs from "../locales/Czech/translation.json"; // Czech
import da from "../locales/Danish/translation.json"; // Danish
import fr from "../locales/French/translation.json"; // French
import de from "../locales/German/translation.json"; // German
import it from "../locales/Italian/translation.json"; // Italian
import ko from "../locales/Korean/translation.json"; // Korean
import es from "../locales/Spanish/translation.json"; // Spanish
import ms from "../locales/Malay/translation.json"; // Malay
import sv from "../locales/Swedish/translation.json"; // Swedish
import pl from "../locales/Polish/translation.json"; // Polish
import pt from "../locales/Portuguese/translation.json"; // Portuguese
import no from "../locales/Norwegian/translation.json"; // Norwegian
import hu from "../locales/Hungarian/translation.json"; // Hungarian

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: en },
      ar: { translation: ar },
      zh: { translation: zh },
      cs: { translation: cs },
      da: { translation: da },
      fr: { translation: fr },
      de: { translation: de },
      it: { translation: it },
      ko: { translation: ko },
      es: { translation: es },
      ms: { translation: ms },
      sv: { translation: sv },
      pl: { translation: pl },
      pt: { translation: pt },
      no: { translation: no },
      hu: { translation: hu },
    },
    fallbackLng: "en",
    interpolation: {
      escapeValue: false,
    },
  });

export default i18n;
