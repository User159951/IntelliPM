import { apiClient } from './client';

export interface OrganizationAIQuotaDto {
  id: number;
  organizationId: number;
  organizationName: string;
  organizationCode: string;
  monthlyTokenLimit: number;
  monthlyRequestLimit: number | null;
  resetDayOfMonth: number | null;
  isAIEnabled: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface UpdateOrganizationAIQuotaRequest {
  monthlyTokenLimit: number;
  monthlyRequestLimit?: number | null;
  resetDayOfMonth?: number | null;
  isAIEnabled?: boolean | null;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export const superAdminAIQuotaApi = {
  /**
   * Get organization AI quota by organization ID (SuperAdmin only).
   */
  getOrganizationAIQuota: async (orgId: number): Promise<OrganizationAIQuotaDto> => {
    return apiClient.get<OrganizationAIQuotaDto>(
      `/api/superadmin/organizations/${orgId}/ai-quota`
    );
  },

  /**
   * Upsert (create or update) organization AI quota (SuperAdmin only).
   */
  upsertOrganizationAIQuota: async (
    orgId: number,
    request: UpdateOrganizationAIQuotaRequest
  ): Promise<OrganizationAIQuotaDto> => {
    return apiClient.put<OrganizationAIQuotaDto>(
      `/api/superadmin/organizations/${orgId}/ai-quota`,
      request
    );
  },

  /**
   * Get a paginated list of all organization AI quotas (SuperAdmin only).
   */
  getOrganizationAIQuotas: async (params?: {
    page?: number;
    pageSize?: number;
    searchTerm?: string;
    isAIEnabled?: boolean;
  }): Promise<PagedResponse<OrganizationAIQuotaDto>> => {
    const queryParams = new URLSearchParams();
    if (params?.page) queryParams.append('page', params.page.toString());
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params?.searchTerm) queryParams.append('searchTerm', params.searchTerm);
    if (params?.isAIEnabled !== undefined) queryParams.append('isAIEnabled', params.isAIEnabled.toString());
    
    const queryString = queryParams.toString();
    const endpoint = `/api/superadmin/organizations/ai-quotas${queryString ? `?${queryString}` : ''}`;
    return apiClient.get<PagedResponse<OrganizationAIQuotaDto>>(endpoint);
  },
};

