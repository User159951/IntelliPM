import { apiClient } from './client';

export interface MemberAIQuotaDto {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  globalRole: string;
  organizationId: number;
  organizationName: string;
  effectiveQuota: EffectiveMemberQuotaDto;
  override: UserQuotaOverrideDto | null;
  organizationQuota: OrganizationQuotaBaseDto;
}

export interface EffectiveMemberQuotaDto {
  monthlyTokenLimit: number;
  monthlyRequestLimit: number | null;
  isAIEnabled: boolean;
  hasOverride: boolean;
}

export interface UserQuotaOverrideDto {
  monthlyTokenLimitOverride: number | null;
  monthlyRequestLimitOverride: number | null;
  isAIEnabledOverride: boolean | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface OrganizationQuotaBaseDto {
  monthlyTokenLimit: number;
  monthlyRequestLimit: number | null;
  isAIEnabled: boolean;
}

export interface UpdateMemberAIQuotaRequest {
  monthlyTokenLimitOverride?: number | null;
  monthlyRequestLimitOverride?: number | null;
  isAIEnabledOverride?: boolean | null;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export const adminAIQuotasApi = {
  /**
   * Get a paginated list of organization members with their effective AI quotas (Admin only).
   */
  getMemberAIQuotas: async (params?: {
    page?: number;
    pageSize?: number;
    searchTerm?: string;
  }): Promise<PagedResponse<MemberAIQuotaDto>> => {
    const queryParams = new URLSearchParams();
    if (params?.page) queryParams.append('page', params.page.toString());
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params?.searchTerm) queryParams.append('searchTerm', params.searchTerm);
    
    const queryString = queryParams.toString();
    const endpoint = `/api/admin/ai-quota/ai-quotas/members${queryString ? `?${queryString}` : ''}`;
    return apiClient.get<PagedResponse<MemberAIQuotaDto>>(endpoint);
  },

  /**
   * Update or create a user AI quota override (Admin only).
   */
  updateMemberAIQuota: async (
    userId: number,
    request: UpdateMemberAIQuotaRequest
  ): Promise<MemberAIQuotaDto> => {
    return apiClient.put<MemberAIQuotaDto>(
      `/api/admin/ai-quota/ai-quotas/members/${userId}`,
      request
    );
  },
};

