import { apiClient } from './client';

export interface AdminDashboardStatsDto {
  totalUsers: number;
  activeUsers: number;
  inactiveUsers: number;
  adminCount: number;
  userCount: number;
  totalProjects: number;
  activeProjects: number;
  totalOrganizations: number;
  userGrowth: UserGrowthDto[];
  recentActivities: RecentActivityDto[];
  systemHealth: SystemHealthDto;
}

export interface UserGrowthDto {
  month: string;
  count: number;
}

export interface RecentActivityDto {
  action: string;
  userName: string;
  timestamp: string;
}

export interface SystemHealthDto {
  cpuUsage: number;
  memoryUsage: number;
  totalMemoryBytes: number;
  usedMemoryBytes: number;
  availableMemoryBytes: number;
  databaseStatus: string;
  databaseResponseTimeMs: string;
  externalServices: Record<string, ExternalServiceStatus>;
  timestamp: string;
}

export interface ExternalServiceStatus {
  name: string;
  isHealthy: boolean;
  statusMessage?: string | null;
  responseTimeMs?: number | null;
  lastChecked?: string | null;
}

export const adminApi = {
  getDashboardStats: (): Promise<AdminDashboardStatsDto> =>
    apiClient.get<AdminDashboardStatsDto>('/api/admin/dashboard/stats'),

  getSystemHealth: (): Promise<SystemHealthDto> =>
    apiClient.get<SystemHealthDto>('/api/admin/system-health'),
};

