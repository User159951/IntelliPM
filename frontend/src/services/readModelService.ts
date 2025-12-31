import { apiClient } from '@/api/client';

// Type definitions matching backend DTOs
export interface TaskSummaryDto {
  id: number;
  title: string;
  priority: string;
  storyPoints?: number;
  assigneeId?: number;
  assigneeName?: string;
  assigneeAvatar?: string | null;
  dueDate?: string | null;
  displayOrder: number;
}

export interface TaskBoardDto {
  projectId: number;
  todoCount: number;
  inProgressCount: number;
  doneCount: number;
  totalTaskCount: number;
  todoStoryPoints: number;
  inProgressStoryPoints: number;
  doneStoryPoints: number;
  totalStoryPoints: number;
  todoTasks: TaskSummaryDto[];
  inProgressTasks: TaskSummaryDto[];
  doneTasks: TaskSummaryDto[];
  lastUpdated: string;
  version: number;
}

export interface BurndownPointDto {
  date: string;
  remainingStoryPoints: number;
  idealRemainingPoints: number;
}

export interface SprintSummaryDto {
  sprintId: number;
  sprintName: string;
  status: string;
  startDate: string;
  endDate: string;
  plannedCapacity?: number | null;
  totalTasks: number;
  completedTasks: number;
  inProgressTasks: number;
  todoTasks: number;
  totalStoryPoints: number;
  completedStoryPoints: number;
  inProgressStoryPoints: number;
  remainingStoryPoints: number;
  completionPercentage: number;
  velocityPercentage: number;
  capacityUtilization: number;
  estimatedDaysRemaining: number;
  isOnTrack: boolean;
  averageVelocity: number;
  burndownData: BurndownPointDto[];
  lastUpdated: string;
  version: number;
}

export interface MemberSummaryDto {
  userId: number;
  username: string;
  role: string;
  tasksAssigned: number;
  tasksCompleted: number;
}

export interface VelocityTrendDto {
  sprintName: string;
  velocity: number;
  date: string;
}

export interface ProjectOverviewDto {
  projectId: number;
  projectName: string;
  projectType: string;
  status: string;
  ownerId: number;
  ownerName: string;
  totalMembers: number;
  activeMembers: number;
  teamMembers: MemberSummaryDto[];
  totalSprints: number;
  activeSprintsCount: number;
  completedSprintsCount: number;
  currentSprintId?: number | null;
  currentSprintName?: string | null;
  totalTasks: number;
  completedTasks: number;
  inProgressTasks: number;
  todoTasks: number;
  blockedTasks: number;
  overdueTasks: number;
  totalStoryPoints: number;
  completedStoryPoints: number;
  remainingStoryPoints: number;
  totalDefects: number;
  openDefects: number;
  criticalDefects: number;
  averageVelocity: number;
  lastSprintVelocity: number;
  velocityTrend: VelocityTrendDto[];
  projectHealth: number;
  healthStatus: string;
  riskFactors: string[];
  lastActivityAt: string;
  activitiesLast7Days: number;
  activitiesLast30Days: number;
  overallProgress: number;
  sprintProgress: number;
  daysUntilNextMilestone: number;
  lastUpdated: string;
  version: number;
}

export interface PagedProjectOverviewDto {
  items: ProjectOverviewDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// Service class
class ReadModelService {
  private readonly basePath = '/read-models';

  /**
   * Get task board read model for a project
   * @param projectId Project ID
   * @returns Task board with pre-grouped tasks
   */
  async getTaskBoard(projectId: number): Promise<TaskBoardDto> {
    const response = await apiClient.get<Record<string, unknown>>(`${this.basePath}/task-board/${projectId}`);
    // Normalize response to handle both PascalCase and camelCase
    return {
      projectId: response.projectId ?? response.ProjectId,
      todoCount: response.todoCount ?? response.TodoCount,
      inProgressCount: response.inProgressCount ?? response.InProgressCount,
      doneCount: response.doneCount ?? response.DoneCount,
      totalTaskCount: response.totalTaskCount ?? response.TotalTaskCount,
      todoStoryPoints: response.todoStoryPoints ?? response.TodoStoryPoints,
      inProgressStoryPoints: response.inProgressStoryPoints ?? response.InProgressStoryPoints,
      doneStoryPoints: response.doneStoryPoints ?? response.DoneStoryPoints,
      totalStoryPoints: response.totalStoryPoints ?? response.TotalStoryPoints,
      todoTasks: response.todoTasks ?? response.TodoTasks ?? [],
      inProgressTasks: response.inProgressTasks ?? response.InProgressTasks ?? [],
      doneTasks: response.doneTasks ?? response.DoneTasks ?? [],
      lastUpdated: response.lastUpdated ?? response.LastUpdated,
      version: response.version ?? response.Version,
    };
  }

  /**
   * Get sprint summary read model
   * @param sprintId Sprint ID
   * @returns Sprint summary with pre-calculated metrics
   */
  async getSprintSummary(sprintId: number): Promise<SprintSummaryDto> {
    const response = await apiClient.get<Record<string, unknown>>(`${this.basePath}/sprint-summary/${sprintId}`);
    // Normalize response to handle both PascalCase and camelCase
    return {
      sprintId: response.sprintId ?? response.SprintId,
      sprintName: response.sprintName ?? response.SprintName,
      status: response.status ?? response.Status,
      startDate: response.startDate ?? response.StartDate,
      endDate: response.endDate ?? response.EndDate,
      plannedCapacity: response.plannedCapacity ?? response.PlannedCapacity,
      totalTasks: response.totalTasks ?? response.TotalTasks,
      completedTasks: response.completedTasks ?? response.CompletedTasks,
      inProgressTasks: response.inProgressTasks ?? response.InProgressTasks,
      todoTasks: response.todoTasks ?? response.TodoTasks,
      totalStoryPoints: response.totalStoryPoints ?? response.TotalStoryPoints,
      completedStoryPoints: response.completedStoryPoints ?? response.CompletedStoryPoints,
      inProgressStoryPoints: response.inProgressStoryPoints ?? response.InProgressStoryPoints,
      remainingStoryPoints: response.remainingStoryPoints ?? response.RemainingStoryPoints,
      completionPercentage: response.completionPercentage ?? response.CompletionPercentage,
      velocityPercentage: response.velocityPercentage ?? response.VelocityPercentage,
      capacityUtilization: response.capacityUtilization ?? response.CapacityUtilization,
      estimatedDaysRemaining: response.estimatedDaysRemaining ?? response.EstimatedDaysRemaining,
      isOnTrack: response.isOnTrack ?? response.IsOnTrack,
      averageVelocity: response.averageVelocity ?? response.AverageVelocity,
      burndownData: response.burndownData ?? response.BurndownData ?? [],
      lastUpdated: response.lastUpdated ?? response.LastUpdated,
      version: response.version ?? response.Version,
    };
  }

  /**
   * Get project overview read model
   * @param projectId Project ID
   * @returns Project overview with aggregated metrics
   */
  async getProjectOverview(projectId: number): Promise<ProjectOverviewDto> {
    const response = await apiClient.get<Record<string, unknown>>(`${this.basePath}/project-overview/${projectId}`);
    // Normalize response to handle both PascalCase and camelCase
    return {
      projectId: response.projectId ?? response.ProjectId,
      projectName: response.projectName ?? response.ProjectName,
      projectType: response.projectType ?? response.ProjectType,
      status: response.status ?? response.Status,
      ownerId: response.ownerId ?? response.OwnerId,
      ownerName: response.ownerName ?? response.OwnerName,
      totalMembers: response.totalMembers ?? response.TotalMembers,
      activeMembers: response.activeMembers ?? response.ActiveMembers,
      teamMembers: response.teamMembers ?? response.TeamMembers ?? [],
      totalSprints: response.totalSprints ?? response.TotalSprints,
      activeSprintsCount: response.activeSprintsCount ?? response.ActiveSprintsCount,
      completedSprintsCount: response.completedSprintsCount ?? response.CompletedSprintsCount,
      currentSprintId: response.currentSprintId ?? response.CurrentSprintId,
      currentSprintName: response.currentSprintName ?? response.CurrentSprintName,
      totalTasks: response.totalTasks ?? response.TotalTasks,
      completedTasks: response.completedTasks ?? response.CompletedTasks,
      inProgressTasks: response.inProgressTasks ?? response.InProgressTasks,
      todoTasks: response.todoTasks ?? response.TodoTasks,
      blockedTasks: response.blockedTasks ?? response.BlockedTasks,
      overdueTasks: response.overdueTasks ?? response.OverdueTasks,
      totalStoryPoints: response.totalStoryPoints ?? response.TotalStoryPoints,
      completedStoryPoints: response.completedStoryPoints ?? response.CompletedStoryPoints,
      remainingStoryPoints: response.remainingStoryPoints ?? response.RemainingStoryPoints,
      totalDefects: response.totalDefects ?? response.TotalDefects,
      openDefects: response.openDefects ?? response.OpenDefects,
      criticalDefects: response.criticalDefects ?? response.CriticalDefects,
      averageVelocity: response.averageVelocity ?? response.AverageVelocity,
      lastSprintVelocity: response.lastSprintVelocity ?? response.LastSprintVelocity,
      velocityTrend: response.velocityTrend ?? response.VelocityTrend ?? [],
      projectHealth: response.projectHealth ?? response.ProjectHealth,
      healthStatus: response.healthStatus ?? response.HealthStatus,
      riskFactors: response.riskFactors ?? response.RiskFactors ?? [],
      lastActivityAt: response.lastActivityAt ?? response.LastActivityAt,
      activitiesLast7Days: response.activitiesLast7Days ?? response.ActivitiesLast7Days,
      activitiesLast30Days: response.activitiesLast30Days ?? response.ActivitiesLast30Days,
      overallProgress: response.overallProgress ?? response.OverallProgress,
      sprintProgress: response.sprintProgress ?? response.SprintProgress,
      daysUntilNextMilestone: response.daysUntilNextMilestone ?? response.DaysUntilNextMilestone,
      lastUpdated: response.lastUpdated ?? response.LastUpdated,
      version: response.version ?? response.Version,
    };
  }

  /**
   * Get multiple project overviews (for dashboard)
   * @param options Query options
   * @returns Paged list of project overviews
   */
  async getProjectOverviews(options?: {
    organizationId?: number;
    status?: string;
    page?: number;
    pageSize?: number;
  }): Promise<PagedProjectOverviewDto> {
    const params = new URLSearchParams();

    if (options?.organizationId) {
      params.append('organizationId', options.organizationId.toString());
    }
    if (options?.status) {
      params.append('status', options.status);
    }
    if (options?.page) {
      params.append('page', options.page.toString());
    }
    if (options?.pageSize) {
      params.append('pageSize', options.pageSize.toString());
    }

    const queryString = params.toString();
    return apiClient.get<PagedProjectOverviewDto>(
      `${this.basePath}/project-overviews${queryString ? `?${queryString}` : ''}`
    );
  }

  /**
   * Check if read model is fresh using ETag
   * @param endpoint Endpoint path
   * @param etag ETag from previous response
   * @returns True if data hasn't changed
   */
  async checkFreshness(endpoint: string, etag: string): Promise<boolean> {
    try {
      const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001';
      const API_VERSION = '/api/v1';
      const versionedEndpoint = endpoint.startsWith('/api/v') 
        ? endpoint 
        : endpoint.startsWith('/api/') 
          ? endpoint.replace('/api/', `${API_VERSION}/`)
          : `${API_VERSION}${endpoint}`;
      
      const response = await fetch(`${API_BASE_URL}${versionedEndpoint}`, {
        method: 'HEAD',
        headers: {
          'If-None-Match': etag,
        },
        credentials: 'include',
      });

      return response.status === 304; // Not Modified
    } catch {
      return false;
    }
  }
}

export const readModelService = new ReadModelService();

