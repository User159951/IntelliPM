import { apiClient } from './client';
import type { MetricsSummary } from '@/types';

export interface SprintVelocityData {
  number: number;
  storyPoints: number;
  completedDate: string;
}

export interface SprintVelocityChartResponse {
  sprints: SprintVelocityData[];
}

export interface TaskDistributionData {
  status: string;
  count: number;
}

export interface TaskDistributionResponse {
  distribution: TaskDistributionData[];
}

export interface BurndownDayData {
  day: number;
  ideal: number;
  actual: number;
}

export interface SprintBurndownResponse {
  days: BurndownDayData[];
}

export interface DefectSeverityData {
  severity: string;
  count: number;
}

export interface DefectsBySeverityResponse {
  defects: DefectSeverityData[];
}

export interface TeamVelocityData {
  date: string;
  storyPoints: number;
  sprintNumber: number;
}

export interface TeamVelocityResponse {
  velocity: TeamVelocityData[];
}

export const metricsApi = {
  get: (projectId?: number): Promise<MetricsSummary> => {
    const query = projectId ? `?projectId=${projectId}` : '';
    return apiClient.get(`/api/Metrics${query}`);
  },

  getSprintVelocityChart: (projectId?: number): Promise<SprintVelocityChartResponse> => {
    const query = projectId ? `?projectId=${projectId}` : '';
    return apiClient.get(`/api/Metrics/sprint-velocity-chart${query}`);
  },

  getTaskDistribution: (projectId?: number): Promise<TaskDistributionResponse> => {
    const query = projectId ? `?projectId=${projectId}` : '';
    return apiClient.get(`/api/Metrics/task-distribution${query}`);
  },

  getSprintBurndown: (sprintId: number): Promise<SprintBurndownResponse> => {
    return apiClient.get(`/api/Metrics/sprint-burndown?sprintId=${sprintId}`);
  },

  getDefectsBySeverity: (projectId?: number): Promise<DefectsBySeverityResponse> => {
    const query = projectId ? `?projectId=${projectId}` : '';
    return apiClient.get(`/api/Metrics/defects-by-severity${query}`);
  },

  getTeamVelocity: (projectId?: number): Promise<TeamVelocityResponse> => {
    const query = projectId ? `?projectId=${projectId}` : '';
    return apiClient.get(`/api/Metrics/team-velocity${query}`);
  },

  getVelocity: (projectId: number, lastNSprints?: number, sprintId?: number): Promise<SprintVelocityResponse> => {
    const params = new URLSearchParams();
    params.append('projectId', projectId.toString());
    if (lastNSprints) params.append('lastNSprints', lastNSprints.toString());
    if (sprintId) params.append('sprintId', sprintId.toString());
    return apiClient.get(`/api/Metrics/velocity?${params.toString()}`);
  },
};

export interface SprintVelocityDto {
  sprintId: number;
  sprintName: string;
  startDate: string;
  endDate: string | null;
  completedStoryPoints: number;
  plannedStoryPoints: number;
  totalTasks: number;
  completedTasks: number;
  completionRate: number;
}

export interface SprintVelocityResponse {
  projectId: number;
  sprints: SprintVelocityDto[];
  averageVelocity: number;
  totalCompletedStoryPoints: number;
}
