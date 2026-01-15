import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import HttpBackend from 'i18next-http-backend';
import LanguageDetector from 'i18next-browser-languagedetector';

const isDevelopment = import.meta.env.DEV;

i18n
  .use(HttpBackend)
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    fallbackLng: 'en',
    debug: isDevelopment,

    // Namespaces
    ns: ['common', 'auth', 'projects', 'tasks', 'admin', 'navigation', 'notifications', 'errors', 'dashboard', 'sprints', 'teams', 'backlog', 'defects', 'metrics', 'insights', 'agents', 'milestones', 'releases'],
    defaultNS: 'common',
    
    // Preload default namespaces
    load: 'languageOnly',
    preload: ['en'],

    // Backend configuration
    backend: {
      loadPath: '/locales/{{lng}}/{{ns}}.json',
      // Wait for backend to load before resolving
      allowMultiLoading: false,
    },

    // Language detector configuration
    detection: {
      // Detection order: localStorage > querystring > cookie > navigator > htmlTag
      order: ['localStorage', 'querystring', 'cookie', 'navigator', 'htmlTag'],
      caches: ['localStorage', 'cookie'],
      
      // localStorage lookup
      lookupLocalStorage: 'i18nextLng',
      
      // querystring lookup (e.g., ?lang=fr)
      lookupQuerystring: 'lang',
      
      // cookie lookup
      lookupCookie: 'i18next',
      cookieMinutes: 10080, // 7 days
      cookieDomain: undefined, // Use current domain
      
      // htmlTag lookup (lang attribute on <html> tag)
      htmlTag: typeof document !== 'undefined' ? document.documentElement : undefined,
    },

    // Supported languages
    supportedLngs: ['en', 'fr', 'ar'],

    // Interpolation configuration
    interpolation: {
      escapeValue: false, // React already escapes values
    },

    // React i18next configuration
    react: {
      useSuspense: true,
    },
  })
  .then(() => {
    // Log initialization after it completes (when using HttpBackend, this happens after resources load)
    if (isDevelopment) {
      console.log('🌍 i18n initialized:', {
        language: i18n.language,
        languages: i18n.languages,
        namespaces: i18n.options.ns,
      });
    }
  })
  .catch((error) => {
    console.error('i18n initialization error:', error);
  });

// Listen for language changes to log when language is actually set
if (isDevelopment) {
  i18n.on('languageChanged', (lng) => {
    if (i18n.language && i18n.language !== 'undefined') {
      console.log('🌍 i18n language changed:', lng);
    }
  });
}

export default i18n;

