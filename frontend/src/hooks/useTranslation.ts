import { useTranslation as useI18nTranslation, UseTranslationResponse } from 'react-i18next';

/**
 * Wrapper hook for react-i18next's useTranslation with additional helper functions
 * 
 * @param ns - Optional namespace(s) to use
 * @returns Translation functions and utilities
 * 
 * @example
 * ```tsx
 * const { t, safeT } = useTranslation();
 * 
 * // Standard usage
 * const text = t('common.welcome');
 * 
 * // Safe usage with fallback
 * const safeText = safeT('common.welcome', 'Welcome');
 * ```
 */
export const useTranslation = (
  ns?: string | string[]
): UseTranslationResponse<string, string> & {
  /**
   * Safe translation function that returns a fallback if the key is not found
   * @param key - Translation key
   * @param fallback - Fallback text if key is not found
   * @param options - Optional translation options
   * @returns Translated text or fallback
   */
  safeT: (key: string, fallback: string, options?: Record<string, unknown>) => string;
} => {
  const translation = useI18nTranslation(ns);

  /**
   * Safe translation with fallback
   * Returns the fallback if the translation key doesn't exist or returns the key itself
   */
  const safeT = (
    key: string,
    fallback: string,
    options?: Record<string, unknown>
  ): string => {
    const translated = translation.t(key, options);
    
    // If i18next returns the key itself (meaning translation not found), use fallback
    if (translated === key) {
      return fallback;
    }
    
    return translated;
  };

  return {
    ...translation,
    safeT,
  };
};

// Re-export types for convenience
export type { UseTranslationResponse } from 'react-i18next';

