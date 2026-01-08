import { apiClient } from './client';
import type { GlobalRole } from '@/types';

export interface PermissionDto {
  id: number;
  name: string;
  category: string;
  description?: string | null;
}

export interface PermissionsMatrixDto {
  permissions: PermissionDto[];
  rolePermissions: Record<GlobalRole, number[]>;
}

/**
 * Response from GET /api/v1/permissions/me
 * Returns the current user's permissions
 */
export interface UserPermissionsResponse {
  permissions: string[];
  globalRole: GlobalRole;
}

/**
 * Response from GET /api/v1/projects/{id}/permissions
 * Returns the current user's permissions for a specific project
 */
export interface ProjectPermissionsResponse {
  permissions: string[];
  projectRole: string | null;
  projectId: number;
}

export const permissionsApi = {
  getMatrix: (): Promise<PermissionsMatrixDto> =>
    apiClient.get<PermissionsMatrixDto>('/permissions/matrix'),

  updateRolePermissions: (role: GlobalRole, permissionIds: number[]): Promise<void> =>
    apiClient.put(`/permissions/roles/${role}`, { permissionIds }),

  /**
   * Get current user's permissions
   * GET /api/v1/permissions/me
   */
  getMyPermissions: (): Promise<UserPermissionsResponse> =>
    apiClient.get<UserPermissionsResponse>('/permissions/me'),

  /**
   * Get current user's permissions for a specific project
   * GET /api/v1/projects/{id}/permissions
   */
  getProjectPermissions: (projectId: number): Promise<ProjectPermissionsResponse> =>
    apiClient.get<ProjectPermissionsResponse>(`/Projects/${projectId}/permissions`),
};

