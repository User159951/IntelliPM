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
  /**
   * Obtient le statut du quota AI pour l'organisation de l'utilisateur courant
   * @param organizationId Optional organization ID (defaults to current user's org)
   * @returns Statut du quota AI
   */
  getQuotaStatus: (organizationId?: number): Promise<QuotaStatus> => {
    const endpoint = organizationId 
      ? `/api/v1/ai/quota?organizationId=${organizationId}`
      : '/api/v1/ai/quota';
    return apiClient.get<QuotaStatus>(endpoint);
  },

  /**
   * Get AI quota usage history
   * @param params Query parameters for usage history
   * @returns Paginated usage history data
   */
  getUsageHistory: (params?: {
    startDate?: string;
    endDate?: string;
    organizationId?: number;
    page?: number;
    pageSize?: number;
  }): Promise<PagedResponse<{
    date: string;
    requests: number;
    tokens: number;
    decisions: number;
    cost: number;
  }>> => {
    const query = new URLSearchParams();
    if (params?.startDate) query.append('startDate', params.startDate);
    if (params?.endDate) query.append('endDate', params.endDate);
    if (params?.organizationId) query.append('organizationId', params.organizationId.toString());
    if (params?.page) query.append('page', params.page.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    
    return apiClient.get<PagedResponse<{
      date: string;
      requests: number;
      tokens: number;
      decisions: number;
      cost: number;
    }>>(`/api/admin/ai-quota/usage-history?${query.toString()}`);
  },

  /**
   * Get AI quota breakdown by agent type
   * @param params Query parameters for breakdown
   * @returns Breakdown data by agent type
   */
  getBreakdown: (params?: {
    period?: string; // 'day' | 'week' | 'month'
    organizationId?: number;
    startDate?: string;
    endDate?: string;
  }): Promise<{
    byAgent: Array<{
      agentType: string;
      requests: number;
      tokens: number;
      decisions: number;
    }>;
    byDecisionType?: Array<{
      decisionType: string;
      requests: number;
      tokens: number;
      decisions: number;
    }>;
  }> => {
    const query = new URLSearchParams();
    if (params?.period) query.append('period', params.period);
    if (params?.organizationId) query.append('organizationId', params.organizationId.toString());
    if (params?.startDate) query.append('startDate', params.startDate);
    if (params?.endDate) query.append('endDate', params.endDate);
    
    return apiClient.get<{
      byAgent: Array<{
        agentType: string;
        requests: number;
        tokens: number;
        decisions: number;
      }>;
      byDecisionType?: Array<{
        decisionType: string;
        requests: number;
        tokens: number;
        decisions: number;
      }>;
    }>(`/api/admin/ai-quota/breakdown?${query.toString()}`);
  },
};
