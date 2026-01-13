import { format, formatDistanceToNow, Locale } from 'date-fns';
import { fr, enUS } from 'date-fns/locale';

/**
 * Locale cache to avoid recreating locale objects
 */
const localeCache = new Map<string, Locale>();

/**
 * Get date-fns locale object for a given language code
 * @param language - Language code (e.g., 'en', 'fr')
 * @returns Locale object for date-fns
 */
export function getDateLocale(language: string): Locale {
  // Check cache first
  if (localeCache.has(language)) {
    return localeCache.get(language)!;
  }

  // Map language codes to date-fns locales
  let locale: Locale;
  switch (language.toLowerCase()) {
    case 'fr':
      locale = fr;
      break;
    case 'en':
    default:
      locale = enUS;
      break;
  }

  // Cache the locale
  localeCache.set(language, locale);
  return locale;
}

/**
 * Format a date according to the specified format string and language
 * @param date - Date to format (Date object or string)
 * @param formatStr - Format string (e.g., 'PP', 'MM/dd/yyyy', 'MMMM d, yyyy')
 * @param language - Language code (e.g., 'en', 'fr')
 * @returns Formatted date string
 */
export function formatDate(
  date: Date | string,
  formatStr: string,
  language: string
): string {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  const locale = getDateLocale(language);
  return format(dateObj, formatStr, { locale });
}

/**
 * Format relative time (e.g., "2 hours ago", "in 3 days")
 * @param date - Date to format (Date object or string)
 * @param language - Language code (e.g., 'en', 'fr')
 * @param options - Additional options for formatDistanceToNow
 * @returns Formatted relative time string
 */
export function formatRelativeTime(
  date: Date | string,
  language: string,
  options?: { addSuffix?: boolean; includeSeconds?: boolean }
): string {
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  const locale = getDateLocale(language);
  return formatDistanceToNow(dateObj, {
    addSuffix: options?.addSuffix ?? true,
    includeSeconds: options?.includeSeconds ?? false,
    locale,
  });
}

/**
 * Common date format presets
 */
export const DateFormats = {
  /** Short date: "01/08/2026" (US) or "08/01/2026" (FR) */
  SHORT: (language: string) => (language === 'fr' ? 'dd/MM/yyyy' : 'MM/dd/yyyy'),
  /** Long date: "January 8, 2026" (US) or "8 janvier 2026" (FR) */
  LONG: (language: string) => (language === 'fr' ? 'd MMMM yyyy' : 'MMMM d, yyyy'),
  /** Date with time: "January 8, 2026 3:45 PM" (US) or "8 janvier 2026 15:45" (FR) */
  DATETIME: (language: string) => (language === 'fr' ? 'd MMMM yyyy HH:mm' : 'MMMM d, yyyy h:mm a'),
  /** Time only: "3:45 PM" (US) or "15:45" (FR) */
  TIME: (language: string) => (language === 'fr' ? 'HH:mm' : 'h:mm a'),
  /** Month and day: "Jan 8" (US) or "8 janv." (FR) */
  MONTH_DAY: (language: string) => (language === 'fr' ? 'd MMM' : 'MMM d'),
  /** Day of week: "Mon" (US) or "lun." (FR) */
  DAY_OF_WEEK: (language: string) => (language === 'fr' ? 'EEE' : 'EEE'),
  /** Pretty print: "January 8, 2026" (US) or "8 janvier 2026" (FR) */
  PRETTY: (language: string) => (language === 'fr' ? 'PP' : 'PP'),
} as const;

