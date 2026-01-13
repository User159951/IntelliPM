import { apiClient } from './client';

/**
 * Get user's language preference from backend
 * @returns Promise resolving to the language code (e.g., 'en', 'fr')
 */
export const getUserLanguage = async (): Promise<string> => {
  try {
    const settings = await apiClient.get<Record<string, string>>(
      '/api/v1/Settings?category=General'
    );
    // Extract "Default.Language" from the settings
    const language = settings['Default.Language'];
    return language || 'en'; // Default to 'en' if not found
  } catch (error) {
    // If backend fails, return default language
    console.warn('Failed to fetch user language from backend:', error);
    return 'en';
  }
};

/**
 * Update user's language preference on backend
 * @param language - Language code to set (e.g., 'en', 'fr')
 * @returns Promise that resolves when update is complete
 */
export const updateUserLanguage = async (language: string): Promise<void> => {
  try {
    await apiClient.put<{ key: string; value: string; category: string }>(
      `/api/v1/Settings/Default.Language`,
      { value: language, category: 'General' }
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

