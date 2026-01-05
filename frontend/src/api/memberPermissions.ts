import { apiClient } from './client';
import type { PagedResponse } from './projects';

export interface MemberPermissionDto {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  globalRole: string;
  organizationId: number;
  organizationName: string;
  permissions: string[];
  permissionIds: number[];
}

export interface UpdateMemberPermissionRequest {
  globalRole?: string | null;
  permissionIds?: number[] | null;
}

export interface GetMemberPermissionsParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
}

export const memberPermissionsApi = {
  /**
   * Get a paginated list of organization members with their permissions (Admin only - own organization).
   * @param params Query parameters for pagination and searching
   * @returns Paginated list of members with permissions
   */
  getMemberPermissions: (
    params: GetMemberPermissionsParams
  ): Promise<PagedResponse<MemberPermissionDto>> => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('page', params.page.toString());
    if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params.searchTerm) queryParams.append('searchTerm', params.searchTerm);
    return apiClient.get(`/api/admin/permissions/members?${queryParams.toString()}`);
  },

  /**
   * Update a member's role and/or permissions (Admin only - own organization).
   * @param userId User ID
   * @param data Permission update request
   * @returns Updated member permission information
   */
  updateMemberPermission: (
    userId: number,
    data: UpdateMemberPermissionRequest
  ): Promise<MemberPermissionDto> =>
    apiClient.put(`/api/admin/permissions/members/${userId}`, data),
};

