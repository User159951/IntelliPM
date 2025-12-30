import { apiClient } from './client';

export interface SearchResult {
  type: 'project' | 'task' | 'user';
  id: number;
  title: string;
  description?: string;
  subtitle?: string;
  url?: string;
}

export interface SearchResponse {
  results: SearchResult[];
}

export const searchApi = {
  search: async (query: string, limit = 20): Promise<SearchResponse> => {
    const params = new URLSearchParams();
    params.append('q', query);
    params.append('limit', limit.toString());
    const response = await apiClient.get<{ Results?: SearchResult[]; results?: SearchResult[] }>(`/api/Search?${params.toString()}`);
    // Normalize response
    return {
      results: response.Results || response.results || [],
    };
  },
};
