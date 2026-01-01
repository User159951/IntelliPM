/**
 * Type definitions for AI Governance and Quota Management
 */

export interface QuotaStatus {
  organizationId: number;
  tierName: string; // 'Free' | 'Pro' | 'Enterprise' | 'Disabled'
  maxRequests: number;
  maxTokens: number;
  maxDecisions: number;
  currentRequests: number;
  currentTokens: number;
  currentDecisions: number;
  resetDate: string; // ISO date string
  isAlertThreshold: boolean; // true if >80%
  isDisabled: boolean;
  requestsPercentage: number;
  tokensPercentage: number;
  decisionsPercentage: number;
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

