import { apiClient } from './client';
import type {
  MilestoneDto,
  CreateMilestoneRequest,
  UpdateMilestoneRequest,
  CompleteMilestoneRequest,
  MilestoneStatistics,
} from '@/types/milestones';

/**
 * API client for milestone management endpoints.
 * Provides functions to interact with milestone-related API endpoints.
 */
export const milestonesApi = {
  /**
   * Get all milestones for a project with optional filtering.
   * @param projectId - The project ID
   * @param status - Optional status filter
   * @param includeCompleted - Whether to include completed milestones
   * @returns Array of milestones
   */
  getProjectMilestones: async (
    projectId: number,
    status?: string,
    includeCompleted?: boolean
  ): Promise<MilestoneDto[]> => {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    if (includeCompleted !== undefined) params.append('includeCompleted', includeCompleted.toString());

    const response = await apiClient.get(`/api/v1/projects/${projectId}/milestones?${params}`);
    return response.data;
  },

  /**
   * Get the next upcoming milestone for a project.
   * @param projectId - The project ID
   * @returns The next milestone or null if none found
   */
  getNextMilestone: async (projectId: number): Promise<MilestoneDto | null> => {
    try {
      const response = await apiClient.get(`/api/v1/projects/${projectId}/milestones/next`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) return null;
      throw error;
    }
  },

  /**
   * Get a specific milestone by ID.
   * @param id - The milestone ID
   * @returns The milestone
   */
  getMilestone: async (id: number): Promise<MilestoneDto> => {
    const response = await apiClient.get(`/api/v1/milestones/${id}`);
    return response.data;
  },

  /**
   * Create a new milestone for a project.
   * @param projectId - The project ID
   * @param data - The milestone creation data
   * @returns The created milestone
   */
  createMilestone: async (
    projectId: number,
    data: CreateMilestoneRequest
  ): Promise<MilestoneDto> => {
    const response = await apiClient.post(`/api/v1/projects/${projectId}/milestones`, data);
    return response.data;
  },

  /**
   * Update an existing milestone.
   * @param id - The milestone ID
   * @param data - The milestone update data
   * @returns The updated milestone
   */
  updateMilestone: async (
    id: number,
    data: UpdateMilestoneRequest
  ): Promise<MilestoneDto> => {
    const response = await apiClient.put(`/api/v1/milestones/${id}`, data);
    return response.data;
  },

  /**
   * Mark a milestone as completed.
   * @param id - The milestone ID
   * @param data - Optional completion data with completion date
   * @returns The completed milestone
   */
  completeMilestone: async (
    id: number,
    data?: CompleteMilestoneRequest
  ): Promise<MilestoneDto> => {
    const response = await apiClient.post(`/api/v1/milestones/${id}/complete`, data || {});
    return response.data;
  },

  /**
   * Delete a milestone.
   * @param id - The milestone ID
   */
  deleteMilestone: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/v1/milestones/${id}`);
  },

  /**
   * Get milestone statistics for a project.
   * @param projectId - The project ID
   * @returns Milestone statistics
   */
  getMilestoneStatistics: async (projectId: number): Promise<MilestoneStatistics> => {
    const response = await apiClient.get(`/api/v1/projects/${projectId}/milestones/statistics`);
    return response.data;
  },

  /**
   * Get all overdue milestones across all projects.
   * @returns Array of overdue milestones
   */
  getOverdueMilestones: async (): Promise<MilestoneDto[]> => {
    const response = await apiClient.get('/api/v1/milestones/overdue');
    return response.data;
  },
};

