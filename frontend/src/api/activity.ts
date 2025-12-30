import { apiClient } from './client';

// Aligned with backend ActivityDto
export interface Activity {
  id: number;
  type: string;
  userId: number;
  userName: string;
  userAvatar?: string | null;
  entityType: string;
  entityId: number;
  entityName?: string | null;
  projectId: number;
  projectName?: string | null;
  timestamp: string; // Backend: DateTimeOffset -> ISO string
}

// Aligned with backend GetRecentActivityResponse
// Backend returns { Activities: [...] } -> camelCase: { activities: [...] }
export interface GetRecentActivityResponse {
  activities: Activity[]; // Backend: Activities (PascalCase) -> activities (camelCase)
}

export const activityApi = {
  getRecent: (limit?: number, projectId?: number): Promise<GetRecentActivityResponse> => {
    const params = new URLSearchParams();
    if (limit) params.append('limit', limit.toString());
    if (projectId) params.append('projectId', projectId.toString());
    const query = params.toString();
    return apiClient.get(`/api/Activity/recent${query ? `?${query}` : ''}`);
  },
};
