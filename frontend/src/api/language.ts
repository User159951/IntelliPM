import { apiClient } from './client';

interface LanguageResponse {
  language: string;
}

/**
 * Get user's language preference from backend
 * Uses the new /api/v1/Settings/language endpoint which implements fallback chain:
 * 1. User's saved preference
 * 2. Organization default language
 * 3. Browser language (from Accept-Language header)
 * 4. System default ('en')
 * @returns Promise resolving to the language code (e.g., 'en', 'fr', 'ar')
 */
export const getUserLanguage = async (): Promise<string> => {
  try {
    const response = await apiClient.get<LanguageResponse>('/api/v1/Settings/language');
    return response.language || 'en'; // Default to 'en' if not found
  } catch (error) {
    // If backend fails, return default language
    console.warn('Failed to fetch user language from backend:', error);
    return 'en';
  }
};

/**
 * Update user's language preference on backend
 * Uses the new /api/v1/Settings/language endpoint
 * @param language - Language code to set (e.g., 'en', 'fr', 'ar')
 * @returns Promise that resolves when update is complete
 */
export const updateUserLanguage = async (language: string): Promise<void> => {
  try {
    await apiClient.put<LanguageResponse>(
      '/api/v1/Settings/language',
      { language }
    );
  } catch (error) {
    // Log error but don't throw - language change should still work locally
    console.warn('Failed to update user language on backend:', error);
    // Don't throw - allow language change to proceed locally even if backend fails
  }
};

export const languageApi = {
  getUserLanguage,
  updateUserLanguage,
};

