/**
 * Type definitions for AI Governance and Quota Management
 */

export interface QuotaStatus {
  quotaId: number;
  tierName: string; // 'Free' | 'Pro' | 'Enterprise' | 'Disabled' | 'None'
  isActive: boolean;
  usage: {
    tokensUsed: number;
    tokensLimit: number;
    tokensPercentage: number;
    requestsUsed: number;
    requestsLimit: number;
    requestsPercentage: number;
    costAccumulated: number;
    costLimit: number;
    costPercentage: number;
  };
  periodEndDate: string; // ISO date string
  daysRemaining: number;
  isExceeded: boolean;
  alertSent: boolean;
  // Computed properties for convenience
  requestsPercentage: number;
  tokensPercentage: number;
  decisionsPercentage: number;
  currentRequests: number;
  maxRequests: number;
  currentTokens: number;
  maxTokens: number;
  currentDecisions: number;
  maxDecisions: number;
  isAlertThreshold: boolean;
  isDisabled: boolean;
  resetDate?: string;
}

export interface QuotaUsageHistory {
  date: string;
  requests: number;
  tokens: number;
  decisions: number;
}

export interface QuotaBreakdownByAgent {
  agentType: string;
  requests: number;
  tokens: number;
  decisions: number;
}

export interface QuotaDetails {
  status: QuotaStatus;
  usageHistory: QuotaUsageHistory[];
  breakdownByAgent: QuotaBreakdownByAgent[];
}

