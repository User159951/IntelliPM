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
      projectId: (response.projectId ?? response.ProjectId) as number,
      todoCount: (response.todoCount ?? response.TodoCount) as number,
      inProgressCount: (response.inProgressCount ?? response.InProgressCount) as number,
      doneCount: (response.doneCount ?? response.DoneCount) as number,
      totalTaskCount: (response.totalTaskCount ?? response.TotalTaskCount) as number,
      todoStoryPoints: (response.todoStoryPoints ?? response.TodoStoryPoints) as number,
      inProgressStoryPoints: (response.inProgressStoryPoints ?? response.InProgressStoryPoints) as number,
      doneStoryPoints: (response.doneStoryPoints ?? response.DoneStoryPoints) as number,
      totalStoryPoints: (response.totalStoryPoints ?? response.TotalStoryPoints) as number,
      todoTasks: (response.todoTasks ?? response.TodoTasks ?? []) as TaskSummaryDto[],
      inProgressTasks: (response.inProgressTasks ?? response.InProgressTasks ?? []) as TaskSummaryDto[],
      doneTasks: (response.doneTasks ?? response.DoneTasks ?? []) as TaskSummaryDto[],
      lastUpdated: (response.lastUpdated ?? response.LastUpdated) as string,
      version: (response.version ?? response.Version) as number,
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
      sprintId: (response.sprintId ?? response.SprintId) as number,
      sprintName: (response.sprintName ?? response.SprintName) as string,
      status: (response.status ?? response.Status) as string,
      startDate: (response.startDate ?? response.StartDate) as string,
      endDate: (response.endDate ?? response.EndDate) as string,
      plannedCapacity: (response.plannedCapacity ?? response.PlannedCapacity) as number | null | undefined,
      totalTasks: (response.totalTasks ?? response.TotalTasks) as number,
      completedTasks: (response.completedTasks ?? response.CompletedTasks) as number,
      inProgressTasks: (response.inProgressTasks ?? response.InProgressTasks) as number,
      todoTasks: (response.todoTasks ?? response.TodoTasks) as number,
      totalStoryPoints: (response.totalStoryPoints ?? response.TotalStoryPoints) as number,
      completedStoryPoints: (response.completedStoryPoints ?? response.CompletedStoryPoints) as number,
      inProgressStoryPoints: (response.inProgressStoryPoints ?? response.InProgressStoryPoints) as number,
      remainingStoryPoints: (response.remainingStoryPoints ?? response.RemainingStoryPoints) as number,
      completionPercentage: (response.completionPercentage ?? response.CompletionPercentage) as number,
      velocityPercentage: (response.velocityPercentage ?? response.VelocityPercentage) as number,
      capacityUtilization: (response.capacityUtilization ?? response.CapacityUtilization) as number,
      estimatedDaysRemaining: (response.estimatedDaysRemaining ?? response.EstimatedDaysRemaining) as number,
      isOnTrack: (response.isOnTrack ?? response.IsOnTrack) as boolean,
      averageVelocity: (response.averageVelocity ?? response.AverageVelocity) as number,
      burndownData: (response.burndownData ?? response.BurndownData ?? []) as BurndownPointDto[],
      lastUpdated: (response.lastUpdated ?? response.LastUpdated) as string,
      version: (response.version ?? response.Version) as number,
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
      projectId: (response.projectId ?? response.ProjectId) as number,
      projectName: (response.projectName ?? response.ProjectName) as string,
      projectType: (response.projectType ?? response.ProjectType) as string,
      status: (response.status ?? response.Status) as string,
      ownerId: (response.ownerId ?? response.OwnerId) as number,
      ownerName: (response.ownerName ?? response.OwnerName) as string,
      totalMembers: (response.totalMembers ?? response.TotalMembers) as number,
      activeMembers: (response.activeMembers ?? response.ActiveMembers) as number,
      teamMembers: (response.teamMembers ?? response.TeamMembers ?? []) as MemberSummaryDto[],
      totalSprints: (response.totalSprints ?? response.TotalSprints) as number,
      activeSprintsCount: (response.activeSprintsCount ?? response.ActiveSprintsCount) as number,
      completedSprintsCount: (response.completedSprintsCount ?? response.CompletedSprintsCount) as number,
      currentSprintId: (response.currentSprintId ?? response.CurrentSprintId) as number | null | undefined,
      currentSprintName: (response.currentSprintName ?? response.CurrentSprintName) as string | null | undefined,
      totalTasks: (response.totalTasks ?? response.TotalTasks) as number,
      completedTasks: (response.completedTasks ?? response.CompletedTasks) as number,
      inProgressTasks: (response.inProgressTasks ?? response.InProgressTasks) as number,
      todoTasks: (response.todoTasks ?? response.TodoTasks) as number,
      blockedTasks: (response.blockedTasks ?? response.BlockedTasks) as number,
      overdueTasks: (response.overdueTasks ?? response.OverdueTasks) as number,
      totalStoryPoints: (response.totalStoryPoints ?? response.TotalStoryPoints) as number,
      completedStoryPoints: (response.completedStoryPoints ?? response.CompletedStoryPoints) as number,
      remainingStoryPoints: (response.remainingStoryPoints ?? response.RemainingStoryPoints) as number,
      totalDefects: (response.totalDefects ?? response.TotalDefects) as number,
      openDefects: (response.openDefects ?? response.OpenDefects) as number,
      criticalDefects: (response.criticalDefects ?? response.CriticalDefects) as number,
      averageVelocity: (response.averageVelocity ?? response.AverageVelocity) as number,
      lastSprintVelocity: (response.lastSprintVelocity ?? response.LastSprintVelocity) as number,
      velocityTrend: (response.velocityTrend ?? response.VelocityTrend ?? []) as VelocityTrendDto[],
      projectHealth: (response.projectHealth ?? response.ProjectHealth) as number,
      healthStatus: (response.healthStatus ?? response.HealthStatus) as string,
      riskFactors: (response.riskFactors ?? response.RiskFactors ?? []) as string[],
      lastActivityAt: (response.lastActivityAt ?? response.LastActivityAt) as string,
      activitiesLast7Days: (response.activitiesLast7Days ?? response.ActivitiesLast7Days) as number,
      activitiesLast30Days: (response.activitiesLast30Days ?? response.ActivitiesLast30Days) as number,
      overallProgress: (response.overallProgress ?? response.OverallProgress) as number,
      sprintProgress: (response.sprintProgress ?? response.SprintProgress) as number,
      daysUntilNextMilestone: (response.daysUntilNextMilestone ?? response.DaysUntilNextMilestone) as number,
      lastUpdated: (response.lastUpdated ?? response.LastUpdated) as string,
      version: (response.version ?? response.Version) as number,
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

