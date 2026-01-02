import { apiClient } from './client';
import type {
  ReleaseDto,
  CreateReleaseRequest,
  UpdateReleaseRequest,
  QualityGateDto,
  ReleaseStatistics,
  ReleaseSprintDto,
} from '@/types/releases';

/**
 * API client for release management endpoints.
 * Provides functions to interact with release-related API endpoints.
 */
export const releasesApi = {
  /**
   * Get all releases for a project with optional status filtering.
   */
  getProjectReleases: async (
    projectId: number,
    status?: string
  ): Promise<ReleaseDto[]> => {
    const params = new URLSearchParams();
    if (status) params.append('status', status);

    return apiClient.get<ReleaseDto[]>(
      `/api/v1/projects/${projectId}/releases${params.toString() ? `?${params}` : ''}`
    );
  },

  /**
   * Get a specific release by ID.
   */
  getRelease: async (id: number): Promise<ReleaseDto> => {
    return apiClient.get<ReleaseDto>(`/api/v1/releases/${id}`);
  },

  /**
   * Create a new release for a project.
   */
  createRelease: async (
    projectId: number,
    data: CreateReleaseRequest
  ): Promise<ReleaseDto> => {
    return apiClient.post<ReleaseDto>(`/api/v1/projects/${projectId}/releases`, data);
  },

  /**
   * Update an existing release.
   */
  updateRelease: async (id: number, data: UpdateReleaseRequest): Promise<ReleaseDto> => {
    return apiClient.put<ReleaseDto>(`/api/v1/releases/${id}`, data);
  },

  /**
   * Delete a release.
   */
  deleteRelease: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/v1/releases/${id}`);
  },

  /**
   * Deploy a release.
   */
  deployRelease: async (id: number): Promise<ReleaseDto> => {
    return apiClient.post<ReleaseDto>(`/api/v1/releases/${id}/deploy`);
  },

  /**
   * Add a sprint to a release.
   */
  addSprintToRelease: async (releaseId: number, sprintId: number): Promise<void> => {
    await apiClient.post(`/api/v1/releases/${releaseId}/sprints/${sprintId}`);
  },

  /**
   * Bulk add sprints to a release.
   */
  bulkAddSprintsToRelease: async (
    releaseId: number,
    sprintIds: number[]
  ): Promise<void> => {
    await apiClient.post(`/api/v1/releases/${releaseId}/sprints/bulk`, { sprintIds });
  },

  /**
   * Remove a sprint from a release.
   * Backend route: DELETE /api/v1/Releases/sprints/{sprintId}
   */
  removeSprintFromRelease: async (sprintId: number): Promise<void> => {
    await apiClient.delete(`/api/v1/Releases/sprints/${sprintId}`);
  },

  /**
   * Get available sprints for a release (sprints that can be added).
   */
  getAvailableSprintsForRelease: async (
    projectId: number,
    releaseId?: number
  ): Promise<ReleaseSprintDto[]> => {
    const params = new URLSearchParams();
    if (releaseId) params.append('releaseId', releaseId.toString());

    return apiClient.get<ReleaseSprintDto[]>(
      `/api/v1/projects/${projectId}/sprints/available${params.toString() ? `?${params}` : ''}`
    );
  },

  /**
   * Generate release notes for a release.
   */
  generateReleaseNotes: async (releaseId: number): Promise<string> => {
    return apiClient.post<string>(`/api/v1/releases/${releaseId}/notes/generate`);
  },

  /**
   * Update release notes (auto-generate or manual).
   */
  updateReleaseNotes: async (
    releaseId: number,
    notes?: string,
    autoGenerate: boolean = false
  ): Promise<void> => {
    await apiClient.put(`/api/v1/releases/${releaseId}/notes`, {
      releaseNotes: notes,
      autoGenerate,
    });
  },

  /**
   * Generate changelog for a release.
   */
  generateChangelog: async (releaseId: number): Promise<string> => {
    return apiClient.post<string>(`/api/v1/releases/${releaseId}/changelog/generate`);
  },

  /**
   * Update changelog (auto-generate or manual).
   */
  updateChangelog: async (
    releaseId: number,
    changelog?: string,
    autoGenerate: boolean = false
  ): Promise<void> => {
    await apiClient.put(`/api/v1/releases/${releaseId}/changelog`, {
      changeLog: changelog,
      autoGenerate,
    });
  },

  /**
   * Evaluate all quality gates for a release.
   */
  evaluateQualityGates: async (releaseId: number): Promise<QualityGateDto[]> => {
    return apiClient.post<QualityGateDto[]>(`/api/v1/releases/${releaseId}/quality-gates/evaluate`);
  },

  /**
   * Approve a quality gate manually.
   */
  approveQualityGate: async (
    releaseId: number,
    gateType: number
  ): Promise<void> => {
    await apiClient.post(`/api/v1/releases/${releaseId}/quality-gates/approve`, {
      gateType,
    });
  },

  /**
   * Get release statistics for a project.
   */
  getReleaseStatistics: async (projectId: number): Promise<ReleaseStatistics> => {
    return apiClient.get<ReleaseStatistics>(`/api/v1/projects/${projectId}/releases/statistics`);
  },
};

