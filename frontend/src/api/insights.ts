import { apiClient } from './client';
import type { Insight } from '@/types';

export const insightsApi = {
  getByProject: (projectId: number, status?: string, agentType?: string): Promise<{ insights: Insight[] }> => {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    if (agentType) params.append('agentType', agentType);
    const query = params.toString();
    return apiClient.get(`/api/projects/${projectId}/insights${query ? `?${query}` : ''}`);
  },
};
