import { apiClient } from './client';

/**
 * Lookup item interface for reference data
 */
export interface LookupItem {
  value: string;
  label: string;
  displayOrder?: number;
  metadata?: {
    color?: string;
    icon?: string;
    bgColor?: string;
    textColor?: string;
    borderColor?: string;
    dotColor?: string;
  };
}

/**
 * Lookup response interface
 */
export interface LookupResponse {
  items: LookupItem[];
}

/**
 * API client for lookup/reference data endpoints
 */
export const lookupsApi = {
  /**
   * Get all task statuses with metadata (colors, icons, etc.)
   */
  getTaskStatuses: (): Promise<LookupResponse> =>
    apiClient.get<LookupResponse>('/api/Lookups/task-statuses'),

  /**
   * Get all task priorities with metadata
   */
  getTaskPriorities: (): Promise<LookupResponse> =>
    apiClient.get<LookupResponse>('/api/Lookups/task-priorities'),

  /**
   * Get all project types with metadata
   */
  getProjectTypes: (): Promise<LookupResponse> =>
    apiClient.get<LookupResponse>('/api/Lookups/project-types'),
};

