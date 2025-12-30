import { apiClient } from './client';

export const settingsApi = {
  getAll: async (category?: string): Promise<Record<string, string>> => {
    const params = category ? `?category=${encodeURIComponent(category)}` : '';
    const response = await apiClient.get<Record<string, string>>(`/api/v1/Settings${params}`);
    return response;
  },

  update: async (key: string, value: string, category?: string): Promise<{ key: string; value: string; category: string }> => {
    const response = await apiClient.put<{ key: string; value: string; category: string }>(
      `/api/v1/Settings/${encodeURIComponent(key)}`,
      { value, category }
    );
    return response;
  },

  sendTestEmail: async (email: string): Promise<{ success: boolean; message: string }> => {
    const response = await apiClient.post<{ success: boolean; message: string }>(
      '/api/v1/Settings/test-email',
      { email }
    );
    return response;
  },
};

