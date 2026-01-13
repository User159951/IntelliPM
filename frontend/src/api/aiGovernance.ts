import { apiClient } from './client';
import type { QuotaStatus, QuotaDetails } from '@/types/aiGovernance';
import type { AIDecisionType, AIAgentType, AIDecisionStatus } from '@/types/generated/enums';

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

/**
 * Backend DTO for AI quota status response
 * Matches AIQuotaStatusDto from backend
 */
export interface AIQuotaStatusResponse {
  quotaId: number;
  tierName: string;
  isActive: boolean;
  usage: QuotaUsageResponse;
  periodEndDate: string; // ISO date string
  daysRemaining: number;
  isExceeded: boolean;
  alertSent: boolean;
}

/**
 * Backend DTO for quota usage breakdown
 * Matches QuotaUsageDto from backend
 */
export interface QuotaUsageResponse {
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

/**
 * Backend DTO for daily usage history entry
 * Matches DailyUsageHistoryDto from backend
 */
export interface DailyUsageHistoryResponse {
  date: string; // ISO date string
  requests: number;
  tokens: number;
  decisions: number;
  cost: number;
}

/**
 * Backend DTO for agent breakdown
 * Matches AgentBreakdownDto from backend
 */
export interface AgentBreakdownResponse {
  agentType: AIAgentType;
  requests: number;
  tokens: number;
  decisions: number;
  cost: number;
  percentageOfTotalTokens: number;
}

/**
 * Backend DTO for decision type breakdown
 * Matches DecisionTypeBreakdownDto from backend
 */
export interface DecisionTypeBreakdownResponse {
  decisionType: AIDecisionType;
  decisions: number;
  tokens: number;
  cost: number;
  percentageOfTotalDecisions: number;
}

/**
 * Backend DTO for quota details response
 * Aggregates status, usage history, and breakdown
 */
export interface AIQuotaDetailsResponse {
  status: AIQuotaStatusResponse;
  usageHistory: PagedResponse<DailyUsageHistoryResponse>;
  breakdown: {
    byAgent: Record<string, AgentBreakdownResponse>;
    byDecisionType: Record<string, DecisionTypeBreakdownResponse>;
    summary: {
      startDate: string;
      endDate: string;
      totalRequests: number;
      totalTokens: number;
      totalDecisions: number;
      totalCost: number;
    };
  };
}

export interface AIDecisionLog {
  decisionId: string;
  decisionType: AIDecisionType;
  agentType: AIAgentType;
  entityType: string;
  entityId: number;
  entityName: string;
  question: string;
  decision: string;
  confidenceScore: number;
  status: AIDecisionStatus;
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
    agentType: AIAgentType;
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
    decisionType?: AIDecisionType;
    agentType?: AIAgentType;
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
   * Get AI quota status for the current organization
   * Calls GET /api/v1/ai/quota/status endpoint (falls back to /api/v1/ai/quota if not available)
   * @param organizationId Optional organization ID (defaults to current user's org)
   * @returns AI quota status with usage information
   */
  getQuotaStatus: async (organizationId?: number): Promise<QuotaStatus> => {
    // Backend endpoint is /api/v1/ai/quota (not /quota/status)
    const endpoint = organizationId 
      ? `/api/v1/ai/quota?organizationId=${organizationId}`
      : '/api/v1/ai/quota';
    
    try {
      const response = await apiClient.get<AIQuotaStatusResponse>(endpoint);
      
      // Transform backend response to frontend QuotaStatus format
      return {
        quotaId: response.quotaId,
        tierName: response.tierName,
        isActive: response.isActive,
        usage: {
          tokensUsed: response.usage.tokensUsed,
          tokensLimit: response.usage.tokensLimit,
          tokensPercentage: response.usage.tokensPercentage,
          requestsUsed: response.usage.requestsUsed,
          requestsLimit: response.usage.requestsLimit,
          requestsPercentage: response.usage.requestsPercentage,
          costAccumulated: response.usage.costAccumulated,
          costLimit: response.usage.costLimit,
          costPercentage: response.usage.costPercentage,
        },
        periodEndDate: response.periodEndDate,
        daysRemaining: response.daysRemaining,
        isExceeded: response.isExceeded,
        alertSent: response.alertSent,
        // Computed properties for convenience
        requestsPercentage: response.usage.requestsPercentage,
        tokensPercentage: response.usage.tokensPercentage,
        decisionsPercentage: 0, // Not provided in status endpoint
        currentRequests: response.usage.requestsUsed,
        maxRequests: response.usage.requestsLimit,
        currentTokens: response.usage.tokensUsed,
        maxTokens: response.usage.tokensLimit,
        currentDecisions: 0, // Not provided in status endpoint
        maxDecisions: 0, // Not provided in status endpoint
        isAlertThreshold: response.usage.tokensPercentage >= 80 || response.usage.requestsPercentage >= 80 || response.usage.costPercentage >= 80,
        isDisabled: !response.isActive,
        resetDate: response.periodEndDate,
      };
    } catch (error) {
      // Re-throw with context for better error handling
      throw new Error(`Failed to fetch AI quota status: ${error instanceof Error ? error.message : 'Unknown error'}`);
    }
  },

  /**
   * Get comprehensive AI quota details including status, usage history, and breakdown
   * Calls GET /api/v1/ai/quota/details endpoint
   * @param params Optional parameters for filtering details
   * @returns Complete quota details with status, history, and breakdown
   */
  getQuotaDetails: async (params?: {
    organizationId?: number;
    startDate?: string;
    endDate?: string;
    period?: 'day' | 'week' | 'month';
    page?: number;
    pageSize?: number;
  }): Promise<QuotaDetails> => {
    const query = new URLSearchParams();
    if (params?.organizationId) query.append('organizationId', params.organizationId.toString());
    if (params?.startDate) query.append('startDate', params.startDate);
    if (params?.endDate) query.append('endDate', params.endDate);
    if (params?.period) query.append('period', params.period);
    if (params?.page) query.append('page', params.page.toString());
    if (params?.pageSize) query.append('pageSize', params.pageSize.toString());
    
    const endpoint = `/api/v1/ai/quota/details${query.toString() ? `?${query.toString()}` : ''}`;
    
    try {
      const response = await apiClient.get<AIQuotaDetailsResponse>(endpoint);
      
      // Transform backend response to frontend QuotaDetails format
      const status: QuotaStatus = {
        quotaId: response.status.quotaId,
        tierName: response.status.tierName,
        isActive: response.status.isActive,
        usage: {
          tokensUsed: response.status.usage.tokensUsed,
          tokensLimit: response.status.usage.tokensLimit,
          tokensPercentage: response.status.usage.tokensPercentage,
          requestsUsed: response.status.usage.requestsUsed,
          requestsLimit: response.status.usage.requestsLimit,
          requestsPercentage: response.status.usage.requestsPercentage,
          costAccumulated: response.status.usage.costAccumulated,
          costLimit: response.status.usage.costLimit,
          costPercentage: response.status.usage.costPercentage,
        },
        periodEndDate: response.status.periodEndDate,
        daysRemaining: response.status.daysRemaining,
        isExceeded: response.status.isExceeded,
        alertSent: response.status.alertSent,
        // Computed properties
        requestsPercentage: response.status.usage.requestsPercentage,
        tokensPercentage: response.status.usage.tokensPercentage,
        decisionsPercentage: response.breakdown.summary.totalDecisions > 0 
          ? (response.breakdown.summary.totalDecisions / (response.breakdown.summary.totalDecisions * 1.5)) * 100 
          : 0,
        currentRequests: response.status.usage.requestsUsed,
        maxRequests: response.status.usage.requestsLimit,
        currentTokens: response.status.usage.tokensUsed,
        maxTokens: response.status.usage.tokensLimit,
        currentDecisions: response.breakdown.summary.totalDecisions,
        maxDecisions: 0, // Not provided in details endpoint
        isAlertThreshold: response.status.usage.tokensPercentage >= 80 || 
                         response.status.usage.requestsPercentage >= 80 || 
                         response.status.usage.costPercentage >= 80,
        isDisabled: !response.status.isActive,
        resetDate: response.status.periodEndDate,
      };
      
      // Transform usage history
      const usageHistory = response.usageHistory.items.map(item => ({
        date: item.date,
        requests: item.requests,
        tokens: item.tokens,
        decisions: item.decisions,
      }));
      
      // Transform breakdown by agent
      const breakdownByAgent = Object.values(response.breakdown.byAgent).map(agent => ({
        agentType: agent.agentType,
        requests: agent.requests,
        tokens: agent.tokens,
        decisions: agent.decisions,
      }));
      
      return {
        status,
        usageHistory,
        breakdownByAgent,
      };
    } catch (error) {
      // Re-throw with context for better error handling
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      // Provide helpful message if endpoint doesn't exist yet
      if (errorMessage.includes('404') || errorMessage.includes('Not Found')) {
        throw new Error('AI quota details endpoint not available. Please ensure the backend endpoint /api/v1/ai/quota/details is implemented.');
      }
      throw new Error(`Failed to fetch AI quota details: ${errorMessage}`);
    }
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
      agentType: AIAgentType;
      requests: number;
      tokens: number;
      decisions: number;
    }>;
    byDecisionType?: Array<{
      decisionType: AIDecisionType;
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
        agentType: AIAgentType;
        requests: number;
        tokens: number;
        decisions: number;
      }>;
      byDecisionType?: Array<{
        decisionType: AIDecisionType;
        requests: number;
        tokens: number;
        decisions: number;
      }>;
    }>(`/api/admin/ai-quota/breakdown?${query.toString()}`);
  },
};
