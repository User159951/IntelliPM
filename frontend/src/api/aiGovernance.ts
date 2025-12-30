import { apiClient } from './client';

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

export interface AIGovernanceStats {
  totalDecisions: number;
  decisionsLast24h: number;
  totalTokens: number;
  tokensLast24h: number;
  activeOrganizations: number;
  totalOrganizations: number;
  exceededQuotas: number;
  alertsSent: number;
  usageTrends: Array<{
    date: string;
    tokens: number;
    requests: number;
  }>;
  usageByAgent: Record<string, { tokensUsed: number; requestsCount: number }>;
  decisionTypes: Record<string, number>;
  organizationsAtRisk: Array<{
    organizationId: number;
    organizationName: string;
    quotaPercentage: number;
    isExceeded: boolean;
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

  getOverviewStats: async () => {
    // For now, aggregate from existing endpoints
    // TODO: Create dedicated overview stats endpoint in backend
    const [decisions, quotas] = await Promise.all([
      apiClient.get<PagedResponse<AIDecisionLog>>('/api/admin/ai/decisions/all?page=1&pageSize=1'),
      apiClient.get<PagedResponse<AIQuota>>('/api/admin/ai/quotas?page=1&pageSize=100'),
    ]);

    const now = new Date();
    const last24h = new Date(now.getTime() - 24 * 60 * 60 * 1000);

    // Calculate stats from decisions
    const allDecisions = decisions.items || [];
    const recentDecisions = allDecisions.filter(
      (d) => new Date(d.createdAt) >= last24h
    );

    const totalTokens = allDecisions.reduce((sum, d) => sum + d.tokensUsed, 0);
    const tokensLast24h = recentDecisions.reduce((sum, d) => sum + d.tokensUsed, 0);

    // Calculate stats from quotas
    const activeQuotas = quotas.items?.filter((q) => q.isActive) || [];
    const exceededQuotas = quotas.items?.filter((q) => q.isExceeded) || [];
    const alertsSent = quotas.items?.filter((q) => q.alertSent).length || 0;

    // Usage by agent
    const usageByAgent: Record<string, { tokensUsed: number; requestsCount: number }> = {};
    allDecisions.forEach((d) => {
      if (!usageByAgent[d.agentType]) {
        usageByAgent[d.agentType] = { tokensUsed: 0, requestsCount: 0 };
      }
      usageByAgent[d.agentType].tokensUsed += d.tokensUsed;
      usageByAgent[d.agentType].requestsCount += 1;
    });

    // Decision types
    const decisionTypes: Record<string, number> = {};
    allDecisions.forEach((d) => {
      decisionTypes[d.decisionType] = (decisionTypes[d.decisionType] || 0) + 1;
    });

    // Organizations at risk
    const organizationsAtRisk = activeQuotas
      .filter((q) => q.usage.tokensPercentage >= 80 || q.isExceeded)
      .map((q) => ({
        organizationId: q.organizationId,
        organizationName: q.organizationName,
        quotaPercentage: q.usage.tokensPercentage,
        isExceeded: q.isExceeded,
      }));

    return {
      totalDecisions: decisions.totalCount || 0,
      decisionsLast24h: recentDecisions.length,
      totalTokens,
      tokensLast24h,
      activeOrganizations: activeQuotas.length,
      totalOrganizations: quotas.totalCount || 0,
      exceededQuotas: exceededQuotas.length,
      alertsSent,
      usageTrends: [], // TODO: Implement when backend endpoint is available
      usageByAgent,
      decisionTypes,
      organizationsAtRisk,
    } as AIGovernanceStats;
  },
};

