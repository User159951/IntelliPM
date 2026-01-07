import { apiClient } from './client';
import type { Epic, Feature, Story, CreateEpicRequest, CreateFeatureRequest, CreateStoryRequest } from '@/types';

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

export interface GetEpicsResponse {
  items: Epic[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface GetFeaturesResponse {
  items: Feature[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface GetStoriesResponse {
  items: Story[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export const backlogApi = {
  createEpic: (projectId: number, data: CreateEpicRequest): Promise<Epic> =>
    apiClient.post(`/api/v1/projects/${projectId}/backlog/epics`, data),

  createFeature: (projectId: number, data: CreateFeatureRequest): Promise<Feature> =>
    apiClient.post(`/api/v1/projects/${projectId}/backlog/features`, data),

  createStory: (projectId: number, data: CreateStoryRequest): Promise<Story> =>
    apiClient.post(`/api/v1/projects/${projectId}/backlog/stories`, data),

  getEpics: (projectId: number, params?: {
    page?: number;
    pageSize?: number;
  }): Promise<GetEpicsResponse> => {
    const queryParams = new URLSearchParams();
    if (params?.page) queryParams.append('page', params.page.toString());
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    const query = queryParams.toString();
    return apiClient.get(`/api/v1/projects/${projectId}/backlog/epics${query ? `?${query}` : ''}`);
  },

  getFeatures: (projectId: number, params?: {
    page?: number;
    pageSize?: number;
    epicId?: number;
  }): Promise<GetFeaturesResponse> => {
    const queryParams = new URLSearchParams();
    if (params?.page) queryParams.append('page', params.page.toString());
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params?.epicId) queryParams.append('epicId', params.epicId.toString());
    const query = queryParams.toString();
    return apiClient.get(`/api/v1/projects/${projectId}/backlog/features${query ? `?${query}` : ''}`);
  },

  getStories: (projectId: number, params?: {
    page?: number;
    pageSize?: number;
    featureId?: number;
  }): Promise<GetStoriesResponse> => {
    const queryParams = new URLSearchParams();
    if (params?.page) queryParams.append('page', params.page.toString());
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params?.featureId) queryParams.append('featureId', params.featureId.toString());
    const query = queryParams.toString();
    return apiClient.get(`/api/v1/projects/${projectId}/backlog/stories${query ? `?${query}` : ''}`);
  },

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
    return apiClient.get(`/api/v1/projects/${projectId}/backlog/tasks${query ? `?${query}` : ''}`);
  },
};
