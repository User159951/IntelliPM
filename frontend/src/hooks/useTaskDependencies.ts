import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { dependenciesApi } from '@/api/dependencies';
import type { TaskDependencyDto } from '@/types/dependencies';

interface BlockingTask {
  taskId: number;
  title: string;
  status: string;
}

interface UseTaskDependenciesResult {
  /** Array of all dependencies for the task */
  dependencies: TaskDependencyDto[] | undefined;
  /** Loading state */
  isLoading: boolean;
  /** Error state */
  isError: boolean;
  /** Whether the task is currently blocked */
  isBlocked: boolean;
  /** Number of dependencies blocking this task */
  blockedByCount: number;
  /** Array of tasks that are blocking this task */
  blockingTasks: BlockingTask[];
  /** Function to refetch dependencies */
  refetch: () => void;
}

/**
 * Custom hook to manage task dependencies and calculate blocking status.
 * 
 * A task is considered blocked if any of its dependencies (where sourceTaskId === taskId) meets:
 * - FinishToStart: dependent task status !== "Done"
 * - StartToStart: dependent task status === "Todo"
 * - FinishToFinish: dependent task status !== "Done"
 * - StartToFinish: dependent task status === "Todo" (rare case)
 * 
 * @param taskId - The ID of the task to check dependencies for
 * @param projectTasks - Optional map of project tasks for status lookup (for performance)
 * @returns Object containing dependencies, blocking status, and helper functions
 * 
 * @example
 * ```tsx
 * const { isBlocked, blockedByCount, blockingTasks } = useTaskDependencies(taskId);
 * 
 * if (isBlocked) {
 *   return <BlockedBadge blockedByCount={blockedByCount} blockingTasks={blockingTasks} />;
 * }
 * ```
 */
export function useTaskDependencies(
  taskId: number,
  projectTasks?: Map<number, { status: string; title: string }>
): UseTaskDependenciesResult {
  const {
    data: dependencies,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ['taskDependencies', taskId],
    queryFn: () => dependenciesApi.getTaskDependencies(taskId),
    enabled: !!taskId,
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  });

  const { isBlocked, blockedByCount, blockingTasks } = useMemo(() => {
    if (!dependencies || dependencies.length === 0) {
      return {
        isBlocked: false,
        blockedByCount: 0,
        blockingTasks: [] as BlockingTask[],
      };
    }

    // Get outgoing dependencies (where this task is the source)
    const outgoingDeps = dependencies.filter((dep) => dep.sourceTaskId === taskId);

    if (outgoingDeps.length === 0) {
      return {
        isBlocked: false,
        blockedByCount: 0,
        blockingTasks: [] as BlockingTask[],
      };
    }

    const blocking: BlockingTask[] = [];

    for (const dep of outgoingDeps) {
      // Get dependent task status
      // If projectTasks map is provided, use it for faster lookup
      // Otherwise, we need to fetch task status separately (less optimal)
      let dependentTaskStatus: string | undefined;
      let dependentTaskTitle: string = dep.dependentTaskTitle;

      if (projectTasks) {
        const task = projectTasks.get(dep.dependentTaskId);
        if (task) {
          dependentTaskStatus = task.status;
          dependentTaskTitle = task.title;
        }
      } else {
        // Fallback: assume we need to check status
        // In a real implementation, you might want to fetch task status
        // For now, we'll use a conservative approach
        dependentTaskStatus = 'Todo'; // Default assumption
      }

      if (!dependentTaskStatus) {
        continue; // Skip if we can't determine status
      }

      // Check blocking conditions based on dependency type
      let isBlocking = false;

      switch (dep.dependencyType) {
        case 'FinishToStart':
          // Task cannot start until dependent task finishes
          isBlocking = dependentTaskStatus !== 'Done';
          break;
        case 'StartToStart':
          // Task cannot start until dependent task starts
          isBlocking = dependentTaskStatus === 'Todo';
          break;
        case 'FinishToFinish':
          // Task cannot finish until dependent task finishes
          isBlocking = dependentTaskStatus !== 'Done';
          break;
        case 'StartToFinish':
          // Task cannot finish until dependent task starts (rare)
          isBlocking = dependentTaskStatus === 'Todo';
          break;
        default:
          // Unknown dependency type, assume not blocking
          isBlocking = false;
      }

      if (isBlocking) {
        blocking.push({
          taskId: dep.dependentTaskId,
          title: dependentTaskTitle,
          status: dependentTaskStatus,
        });
      }
    }

    return {
      isBlocked: blocking.length > 0,
      blockedByCount: blocking.length,
      blockingTasks: blocking,
    };
  }, [dependencies, taskId, projectTasks]);

  return {
    dependencies,
    isLoading,
    isError,
    isBlocked,
    blockedByCount,
    blockingTasks,
    refetch,
  };
}

