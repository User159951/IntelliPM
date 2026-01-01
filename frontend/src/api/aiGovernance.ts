import { apiClient } from './client';
import type { QuotaStatus } from '@/types/aiGovernance';

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface AIDecisionLog {
  decisionId: string;
  decisionType: string;
  agentType: string;
  entityType: string;
  entityId: number;
  entityName: string;
  question: string;
  decision: string;
  confidenceScore: number;
  status: string;
  requiresHumanApproval: boolean;
  approvedByHuman: boolean | null;
  createdAt: string;
  tokensUsed: number;
}

export interface AIQuota {
  id: number;
  organizationId: number;
  organizationName: string;
  tierName: string;
  isActive: boolean;
  usage: QuotaUsage;
  periodEndDate: string;
  isExceeded: boolean;
  alertSent: boolean;
}

export interface QuotaUsage {
  tokensUsed: number;
  tokensLimit: number;
  tokensPercentage: number;
  requestsUsed: number;
  requestsLimit: number;
  requestsPercentage: number;
  costAccumulated: number;
  costLimit: number;
  costPercentage: number;
}

export interface AIOverviewStats {
  totalOrganizations: number;
  organizationsWithAIEnabled: number;
  organizationsWithAIDisabled: number;
  totalDecisionsLast30Days: number;
  pendingApprovals: number;
  approvedDecisions: number;
  rejectedDecisions: number;
  averageConfidenceScore: number;
  topAgents: Array<{
    agentType: string;
    decisionCount: number;
    totalTokensUsed: number;
  }>;
  quotaByTier: Array<{
    tierName: string;
    organizationCount: number;
    averageUsagePercentage: number;
    exceededCount: number;
  }>;
}

export const aiGovernanceApi = {
  // Admin endpoints
  getAllDecisions: (params: {
    page: number;
    pageSize: number;
    organizationId?: number;
    decisionType?: string;
    agentType?: string;
    startDate?: string;
    endDate?: string;
  }) => {
    const query = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== '') {
        query.append(key, value.toString());
      }
    });
    return apiClient.get<PagedResponse<AIDecisionLog>>(
      `/api/admin/ai/decisions/all?${query.toString()}`
    );
  },

  getAllQuotas: (params: {
    page: number;
    pageSize: number;
    tierName?: string;
    isActive?: boolean;
    isExceeded?: boolean;
  }) => {
    const query = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== '') {
        query.append(key, value.toString());
      }
    });
    return apiClient.get<PagedResponse<AIQuota>>(
      `/api/admin/ai/quotas?${query.toString()}`
    );
  },

  updateQuota: (
    organizationId: number,
    data: {
      tierName: string;
      maxTokensPerPeriod?: number;
      maxRequestsPerPeriod?: number;
      maxDecisionsPerPeriod?: number;
      maxCostPerPeriod?: number;
      allowOverage?: boolean;
      overageRate?: number;
      enforceQuota?: boolean;
      applyImmediately: boolean;
      reason?: string;
    }
  ) => apiClient.put(`/api/admin/ai/quota/${organizationId}`, data),

  disableAI: (
    organizationId: number,
    reason: string,
    notifyOrganization: boolean,
    isPermanent: boolean
  ) =>
    apiClient.post(`/api/admin/ai/disable/${organizationId}`, {
      reason,
      notifyOrganization,
      isPermanent,
    }),

  enableAI: (organizationId: number, tierName: string, reason: string) =>
    apiClient.post(`/api/admin/ai/enable/${organizationId}`, {
      tierName,
      reason,
    }),

  getOverviewStats: (): Promise<AIOverviewStats> => {
    return apiClient.get<AIOverviewStats>('/api/admin/ai/overview/stats');
  },

  // User endpoints
  getQuotaStatus: (organizationId: number): Promise<QuotaStatus> => {
    // TODO: Replace with actual endpoint when available
    // return apiClient.get(`/api/v1/ai/quota/status/${organizationId}`);
    // For now, return a promise that resolves with mock data
    return Promise.resolve({
      organizationId,
      tierName: 'Free',
      maxRequests: 100,
      maxTokens: 100000,
      maxDecisions: 50,
      currentRequests: 75,
      currentTokens: 45000,
      currentDecisions: 30,
      resetDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
      isAlertThreshold: true,
      isDisabled: false,
      requestsPercentage: 75,
      tokensPercentage: 45,
      decisionsPercentage: 60,
    });
  },
};
