import { apiClient } from './client';
import type { PagedResponse } from './projects';

export interface AdminAiQuotaMember {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  userRole: string;
  organizationId: number;
  organizationName: string;
  effectiveQuota: EffectiveQuota;
  override: QuotaOverride | null;
  usage: UserUsage;
  period: PeriodInfo;
}

export interface EffectiveQuota {
  maxTokensPerPeriod: number;
  maxRequestsPerPeriod: number;
  maxDecisionsPerPeriod: number;
  maxCostPerPeriod: number;
  hasOverride: boolean;
}

export interface QuotaOverride {
  maxTokensPerPeriod: number | null;
  maxRequestsPerPeriod: number | null;
  maxDecisionsPerPeriod: number | null;
  maxCostPerPeriod: number | null;
  createdAt: string;
  reason: string | null;
}

export interface UserUsage {
  tokensUsed: number;
  requestsUsed: number;
  decisionsMade: number;
  costAccumulated: number;
  tokensPercentage: number;
  requestsPercentage: number;
  decisionsPercentage: number;
  costPercentage: number;
}

export interface PeriodInfo {
  periodStartDate: string;
  periodEndDate: string;
  daysRemaining: number;
}

export interface UpdateMemberQuotaRequest {
  maxTokensPerPeriod?: number | null;
  maxRequestsPerPeriod?: number | null;
  maxDecisionsPerPeriod?: number | null;
  maxCostPerPeriod?: number | null;
  reason?: string | null;
}

export interface UpdateMemberQuotaResponse {
  overrideId: number;
  userId: number;
  effectiveQuota: EffectiveQuota;
  override: QuotaOverride;
}

export interface ResetMemberQuotaResponse {
  userId: number;
  success: boolean;
  message: string;
}

export const adminAiQuotaApi = {
  /**
   * Get paginated list of organization members with their AI quota information.
   */
  getMembers: async (
    page = 1,
    pageSize = 20,
    searchTerm?: string
  ): Promise<PagedResponse<AdminAiQuotaMember>> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (searchTerm) params.append('searchTerm', searchTerm);
    return apiClient.get<PagedResponse<AdminAiQuotaMember>>(
      `/api/admin/ai-quota/members?${params.toString()}`
    );
  },

  /**
   * Update or create a user AI quota override.
   */
  updateMemberQuota: async (
    userId: number,
    request: UpdateMemberQuotaRequest
  ): Promise<UpdateMemberQuotaResponse> => {
    return apiClient.put<UpdateMemberQuotaResponse>(
      `/api/admin/ai-quota/members/${userId}`,
      request
    );
  },

  /**
   * Reset (delete) a user AI quota override, reverting to organization default.
   */
  resetMemberQuota: async (userId: number): Promise<ResetMemberQuotaResponse> => {
    return apiClient.post<ResetMemberQuotaResponse>(
      `/api/admin/ai-quota/members/${userId}/reset`
    );
  },
};

