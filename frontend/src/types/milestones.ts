export enum MilestoneType {
  Release = 0,
  Sprint = 1,
  Deadline = 2,
  Custom = 3,
}

export enum MilestoneStatus {
  Pending = 0,
  InProgress = 1,
  Completed = 2,
  Missed = 3,
  Cancelled = 4,
}

export interface MilestoneDto {
  id: number;
  projectId: number;
  name: string;
  description: string;
  type: string; // "Release" | "Sprint" | "Deadline" | "Custom"
  status: string; // "Pending" | "InProgress" | "Completed" | "Missed" | "Cancelled"
  dueDate: string; // ISO date string
  completedAt: string | null;
  progress: number; // 0-100
  daysUntilDue: number;
  isOverdue: boolean;
  createdAt: string;
  createdByName: string;
}

export interface CreateMilestoneRequest {
  name: string;
  description?: string;
  type: string;
  dueDate: string;
  progress?: number;
}

export interface UpdateMilestoneRequest {
  name: string;
  description?: string;
  dueDate: string;
  progress: number;
}

export interface CompleteMilestoneRequest {
  completedAt?: string;
}

export interface MilestoneStatistics {
  totalMilestones: number;
  completedMilestones: number;
  missedMilestones: number;
  upcomingMilestones: number;
  pendingMilestones: number;
  inProgressMilestones: number;
  completionRate: number;
}

