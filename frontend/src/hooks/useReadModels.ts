import { useQuery, UseQueryOptions } from '@tanstack/react-query';
import {
  readModelService,
  type TaskBoardDto,
  type SprintSummaryDto,
  type ProjectOverviewDto,
  type PagedProjectOverviewDto,
} from '@/services/readModelService';

/**
 * Hook to fetch task board read model for a project
 * @param projectId Project ID
 * @param options Optional React Query options
 * @returns Query result with task board data
 */
export function useTaskBoard(
  projectId: number,
  options?: Omit<UseQueryOptions<TaskBoardDto>, 'queryKey' | 'queryFn'>
) {
  return useQuery({
    queryKey: ['task-board', projectId],
    queryFn: () => readModelService.getTaskBoard(projectId),
    staleTime: 1000 * 60 * 2, // 2 minutes
    ...options,
  });
}

/**
 * Hook to fetch sprint summary read model
 * @param sprintId Sprint ID
 * @param options Optional React Query options
 * @returns Query result with sprint summary data
 */
export function useSprintSummary(
  sprintId: number,
  options?: Omit<UseQueryOptions<SprintSummaryDto>, 'queryKey' | 'queryFn'>
) {
  return useQuery({
    queryKey: ['sprint-summary', sprintId],
    queryFn: () => readModelService.getSprintSummary(sprintId),
    staleTime: 1000 * 60, // 1 minute
    ...options,
  });
}

/**
 * Hook to fetch project overview read model
 * @param projectId Project ID
 * @param options Optional React Query options
 * @returns Query result with project overview data
 */
export function useProjectOverview(
  projectId: number,
  options?: Omit<UseQueryOptions<ProjectOverviewDto>, 'queryKey' | 'queryFn'>
) {
  return useQuery({
    queryKey: ['project-overview', projectId],
    queryFn: () => readModelService.getProjectOverview(projectId),
    staleTime: 1000 * 60 * 2, // 2 minutes
    ...options,
  });
}

/**
 * Hook to fetch multiple project overviews (for dashboard)
 * @param options Query options (organizationId, status, page, pageSize)
 * @param queryOptions Optional React Query options
 * @returns Query result with paged project overviews
 */
export function useProjectOverviews(
  options?: {
    organizationId?: number;
    status?: string;
    page?: number;
    pageSize?: number;
  },
  queryOptions?: Omit<UseQueryOptions<PagedProjectOverviewDto>, 'queryKey' | 'queryFn'>
) {
  return useQuery({
    queryKey: ['project-overviews', options],
    queryFn: () => readModelService.getProjectOverviews(options),
    staleTime: 1000 * 60, // 1 minute
    ...queryOptions,
  });
}

