/**
 * Format a number according to the specified locale
 * @param number - Number to format
 * @param locale - Locale string (e.g., 'en-US', 'fr-FR')
 * @param options - Intl.NumberFormatOptions for customization
 * @returns Formatted number string
 */
export function formatNumber(
  number: number,
  locale: string,
  options?: Intl.NumberFormatOptions
): string {
  // Map language codes to full locale strings
  const fullLocale = getFullLocale(locale);
  
  try {
    return new Intl.NumberFormat(fullLocale, options).format(number);
  } catch (error) {
    // Fallback to default formatting if locale is invalid
    console.warn(`Invalid locale: ${locale}, falling back to default formatting`, error);
    return new Intl.NumberFormat('en-US', options).format(number);
  }
}

/**
 * Format a number as currency
 * @param number - Number to format
 * @param locale - Locale string (e.g., 'en', 'fr')
 * @param currency - Currency code (e.g., 'USD', 'EUR')
 * @param options - Additional Intl.NumberFormatOptions
 * @returns Formatted currency string
 */
export function formatCurrency(
  number: number,
  locale: string,
  currency: string = 'USD',
  options?: Intl.NumberFormatOptions
): string {
  return formatNumber(number, locale, {
    style: 'currency',
    currency,
    ...options,
  });
}

/**
 * Format a number as a percentage
 * @param number - Number to format (should be between 0 and 1 for decimal, or 0-100 for percentage)
 * @param locale - Locale string (e.g., 'en', 'fr')
 * @param options - Additional Intl.NumberFormatOptions
 * @returns Formatted percentage string
 */
export function formatPercentage(
  number: number,
  locale: string,
  options?: Intl.NumberFormatOptions
): string {
  return formatNumber(number, locale, {
    style: 'percent',
    ...options,
  });
}

/**
 * Format a number with a specific number of decimal places
 * @param number - Number to format
 * @param locale - Locale string (e.g., 'en', 'fr')
 * @param decimals - Number of decimal places (default: 2)
 * @returns Formatted number string
 */
export function formatDecimal(
  number: number,
  locale: string,
  decimals: number = 2
): string {
  return formatNumber(number, locale, {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  });
}

/**
 * Format a number with compact notation (e.g., "1.2K", "3.4M")
 * @param number - Number to format
 * @param locale - Locale string (e.g., 'en', 'fr')
 * @param options - Additional Intl.NumberFormatOptions
 * @returns Formatted compact number string
 */
export function formatCompact(
  number: number,
  locale: string,
  options?: Intl.NumberFormatOptions
): string {
  return formatNumber(number, locale, {
    notation: 'compact',
    ...options,
  });
}

/**
 * Map language code to full locale string
 * @param language - Language code (e.g., 'en', 'fr')
 * @returns Full locale string (e.g., 'en-US', 'fr-FR')
 */
function getFullLocale(language: string): string {
  switch (language.toLowerCase()) {
    case 'fr':
      return 'fr-FR';
    case 'en':
    default:
      return 'en-US';
  }
}

/**
 * Common number format presets
 */
export const NumberFormats = {
  /** Integer: no decimals */
  INTEGER: (locale: string) => formatNumber(0, locale, { maximumFractionDigits: 0 }),
  /** Decimal with 1 place */
  DECIMAL_1: (number: number, locale: string) => formatDecimal(number, locale, 1),
  /** Decimal with 2 places */
  DECIMAL_2: (number: number, locale: string) => formatDecimal(number, locale, 2),
  /** Percentage with 1 decimal place */
  PERCENTAGE_1: (number: number, locale: string) => formatPercentage(number, locale, { minimumFractionDigits: 1, maximumFractionDigits: 1 }),
  /** Percentage with 2 decimal places */
  PERCENTAGE_2: (number: number, locale: string) => formatPercentage(number, locale, { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
} as const;

