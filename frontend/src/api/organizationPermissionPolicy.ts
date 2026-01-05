import { apiClient } from './client';

export interface OrganizationPermissionPolicyDto {
  id: number;
  organizationId: number;
  organizationName: string;
  organizationCode: string;
  allowedPermissions: string[];
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface UpdateOrganizationPermissionPolicyRequest {
  allowedPermissions: string[];
  isActive?: boolean | null;
}

export const organizationPermissionPolicyApi = {
  /**
   * Get organization permission policy by organization ID (SuperAdmin only).
   * @param orgId Organization ID
   * @returns Organization permission policy details
   */
  getOrganizationPermissionPolicy: (orgId: number): Promise<OrganizationPermissionPolicyDto> =>
    apiClient.get(`/api/superadmin/organizations/${orgId}/permission-policy`),

  /**
   * Upsert (create or update) organization permission policy (SuperAdmin only).
   * @param orgId Organization ID
   * @param data Policy update request
   * @returns Updated organization permission policy details
   */
  upsertOrganizationPermissionPolicy: (
    orgId: number,
    data: UpdateOrganizationPermissionPolicyRequest
  ): Promise<OrganizationPermissionPolicyDto> =>
    apiClient.put(`/api/superadmin/organizations/${orgId}/permission-policy`, data),
};

