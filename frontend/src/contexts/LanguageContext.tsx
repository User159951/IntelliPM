import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { useAuth } from './AuthContext';
import { languageApi } from '@/api/language';
import i18n from '@/i18n/config';

export interface Language {
  code: string;
  label: string;
}

interface LanguageContextType {
  language: string;
  changeLanguage: (lang: string) => Promise<void>;
  availableLanguages: Language[];
  isLoading: boolean;
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

// Available languages
const AVAILABLE_LANGUAGES: Language[] = [
  { code: 'en', label: 'English' },
  { code: 'fr', label: 'Français' },
  { code: 'ar', label: 'العربية' },
];

// localStorage key for language preference
const LANGUAGE_STORAGE_KEY = 'i18n-language';

/**
 * Get browser language preference
 * @returns Language code or null if not supported
 */
const getBrowserLanguage = (): string | null => {
  if (typeof window === 'undefined') return null;
  
  const browserLang = navigator.language.split('-')[0]; // Get base language (e.g., 'en' from 'en-US')
  return AVAILABLE_LANGUAGES.some(lang => lang.code === browserLang) ? browserLang : null;
};

/**
 * Get initial language based on priority:
 * 1. Backend (if authenticated) - will be loaded in useEffect
 * 2. localStorage (check both our key and i18next's key)
 * 3. Browser preference
 * 4. Default 'en'
 */
const getInitialLanguage = (): string => {
  if (typeof window === 'undefined') return 'en';
  
  // Check localStorage first (our key)
  const stored = localStorage.getItem(LANGUAGE_STORAGE_KEY);
  if (stored && AVAILABLE_LANGUAGES.some(lang => lang.code === stored)) {
    return stored;
  }
  
  // Check i18next's localStorage key for compatibility
  const i18nextStored = localStorage.getItem('i18nextLng');
  if (i18nextStored && AVAILABLE_LANGUAGES.some(lang => lang.code === i18nextStored)) {
    return i18nextStored;
  }
  
  // Check browser preference
  const browserLang = getBrowserLanguage();
  if (browserLang) {
    return browserLang;
  }
  
  // Default to 'en'
  return 'en';
};

export const LanguageProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const [language, setLanguage] = useState<string>(() => {
    // Initialize with localStorage or browser preference
    // Backend preference will be loaded in useEffect if authenticated
    return getInitialLanguage();
  });
  const [isLoading, setIsLoading] = useState(true);

  /**
   * Load language preference from backend if authenticated
   */
  useEffect(() => {
    const loadLanguageFromBackend = async () => {
      // Wait for auth to finish loading
      if (authLoading) return;
      
      // Only fetch from backend if authenticated
      if (!isAuthenticated) {
        // Not authenticated - use localStorage/browser preference
        const initialLang = getInitialLanguage();
        setLanguage(initialLang);
        await i18n.changeLanguage(initialLang);
        localStorage.setItem('i18nextLng', initialLang); // Sync with i18next's detector
        setIsLoading(false);
        return;
      }

      try {
        setIsLoading(true);
        const backendLanguage = await languageApi.getUserLanguage();
        
        // Validate that the language from backend is supported
        const validLanguage = AVAILABLE_LANGUAGES.some(lang => lang.code === backendLanguage)
          ? backendLanguage
          : 'en';
        
        setLanguage(validLanguage);
        await i18n.changeLanguage(validLanguage);
        
        // Update localStorage to match backend (both our key and i18next's key)
        localStorage.setItem(LANGUAGE_STORAGE_KEY, validLanguage);
        localStorage.setItem('i18nextLng', validLanguage); // Sync with i18next's detector
      } catch (error) {
        // Backend failed - fallback to localStorage/browser
        console.warn('Failed to load language from backend, using local preference:', error);
        const fallbackLang = getInitialLanguage();
        setLanguage(fallbackLang);
        await i18n.changeLanguage(fallbackLang);
        localStorage.setItem('i18nextLng', fallbackLang); // Sync with i18next's detector
      } finally {
        setIsLoading(false);
      }
    };

    loadLanguageFromBackend();
  }, [isAuthenticated, authLoading]);

  /**
   * Change language and persist to localStorage and backend
   */
  const changeLanguage = useCallback(async (lang: string) => {
    // Validate language code
    if (!AVAILABLE_LANGUAGES.some(l => l.code === lang)) {
      console.warn(`Invalid language code: ${lang}. Falling back to 'en'.`);
      return;
    }

    try {
      // Update i18next
      await i18n.changeLanguage(lang);
      
      // Update state
      setLanguage(lang);
      
      // Persist to localStorage (both our key and i18next's key)
      localStorage.setItem(LANGUAGE_STORAGE_KEY, lang);
      localStorage.setItem('i18nextLng', lang); // Sync with i18next's detector
      
      // Sync with backend if authenticated
      if (isAuthenticated) {
        try {
          await languageApi.updateUserLanguage(lang);
        } catch (error) {
          // Log error but don't block language change
          console.warn('Failed to sync language to backend:', error);
        }
      }
    } catch (error) {
      console.error('Failed to change language:', error);
      // Don't throw - language change should still work even if i18next fails
    }
  }, [isAuthenticated]);

  return (
    <LanguageContext.Provider
      value={{
        language,
        changeLanguage,
        availableLanguages: AVAILABLE_LANGUAGES,
        isLoading: isLoading || authLoading,
      }}
    >
      {children}
    </LanguageContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useLanguage = (): LanguageContextType => {
  const context = useContext(LanguageContext);
  if (context === undefined) {
    throw new Error('useLanguage must be used within a LanguageProvider');
  }
  return context;
};

