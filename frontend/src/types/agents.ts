/**
 * Type definitions for AI Agent outputs
 */

export interface PrioritizedItem {
  taskId: number;
  taskTitle: string;
  priority: number;
  rationale: string;
  confidenceScore: number;
}

export interface DefectPattern {
  pattern: string;
  frequency: number;
  severity: 'low' | 'medium' | 'high' | 'critical';
  affectedAreas: string[];
  recommendation: string;
}

export interface ValueMetric {
  metricName: string;
  currentValue: number;
  targetValue: number;
  unit: string;
  trend: 'up' | 'down' | 'stable';
}

export interface ProductAgentOutput {
  items: PrioritizedItem[];
  summary: string;
}

export interface QAAgentOutput {
  patterns: DefectPattern[];
  summary: string;
  overallQuality: number;
}

export interface BusinessAgentOutput {
  metrics: ValueMetric[];
  highlights: string[];
  recommendations: string[];
}

export interface ManagerAgentOutput {
  executiveSummary: string;
  keyDecisions: string[];
  highlights: string[];
  recommendations: string[];
  insights: string[];
}

export interface DeliveryAgentOutput {
  milestones: DeliveryMilestone[];
  risks: DeliveryRisk[];
  actionItems: DeliveryActionItem[];
  summary: string;
}

export interface DeliveryMilestone {
  id: string;
  name: string;
  targetDate: string;
  status: 'on-track' | 'at-risk' | 'delayed';
  progress: number;
}

export interface DeliveryRisk {
  id: string;
  description: string;
  severity: 'low' | 'medium' | 'high' | 'critical';
  impact: string;
  mitigation: string;
}

export interface DeliveryActionItem {
  id: string;
  action: string;
  priority: 'low' | 'medium' | 'high' | 'critical';
  owner?: string;
  dueDate?: string;
}

export type AgentType = 'product' | 'qa' | 'business' | 'manager' | 'delivery';

