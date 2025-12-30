import { apiClient } from './client';
import type { Epic, Feature, Story, CreateEpicRequest, CreateFeatureRequest, CreateStoryRequest, Task } from '@/types';

export interface BacklogTaskDto {
  id: number;
  title: string;
  description: string;
  priority: string;
  status: string;
  storyPoints: number | null;
  assigneeId: number | null;
  assigneeName: string | null;
  createdAt: string;
  priorityOrder: number;
}

export interface GetBacklogResponse {
  items: BacklogTaskDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export const backlogApi = {
  createEpic: (projectId: number, data: CreateEpicRequest): Promise<Epic> =>
    apiClient.post(`/api/projects/${projectId}/backlog/epics`, data),

  createFeature: (projectId: number, data: CreateFeatureRequest): Promise<Feature> =>
    apiClient.post(`/api/projects/${projectId}/backlog/features`, data),

  createStory: (projectId: number, data: CreateStoryRequest): Promise<Story> =>
    apiClient.post(`/api/projects/${projectId}/backlog/stories`, data),

  getTasks: (projectId: number, params?: {
    page?: number;
    pageSize?: number;
    priority?: string;
    status?: string;
    searchTerm?: string;
  }): Promise<GetBacklogResponse> => {
    const queryParams = new URLSearchParams();
    if (params?.page) queryParams.append('page', params.page.toString());
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params?.priority) queryParams.append('priority', params.priority);
    if (params?.status) queryParams.append('status', params.status);
    if (params?.searchTerm) queryParams.append('searchTerm', params.searchTerm);
    const query = queryParams.toString();
    return apiClient.get(`/api/projects/${projectId}/backlog/tasks${query ? `?${query}` : ''}`);
  },
};
