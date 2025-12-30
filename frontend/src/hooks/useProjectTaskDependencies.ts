import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { dependenciesApi } from '@/api/dependencies';

interface BlockingInfo {
  isBlocked: boolean;
  blockedByCount: number;
  blockingTasks: Array<{ taskId: number; title: string; status: string }>;
}

/**
 * Custom hook to fetch all task dependencies for a project and build a blocking map.
 * This is more efficient than fetching dependencies for each task individually.
 * 
 * @param projectId - The ID of the project
 * @returns Map of taskId -> blocking info, plus loading/error states
 * 
 * @example
 * ```tsx
 * const { blockingMap, isLoading } = useProjectTaskDependencies(projectId);
 * 
 * const taskBlockingInfo = blockingMap.get(taskId);
 * if (taskBlockingInfo?.isBlocked) {
 *   // Show blocked badge
 * }
 * ```
 */
export function useProjectTaskDependencies(projectId: number) {
  const {
    data: graphData,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ['projectDependencyGraph', projectId],
    queryFn: () => dependenciesApi.getProjectDependencyGraph(projectId),
    enabled: !!projectId,
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  });

  // Build a map of taskId -> blocking info
  const blockingMap = useMemo(() => {
    const map = new Map<number, BlockingInfo>();

    if (!graphData || !graphData.nodes || !graphData.edges) {
      return map;
    }

    // Create a map of taskId -> task info for quick lookup
    const taskMap = new Map(
      graphData.nodes.map((node) => [
        node.taskId,
        { status: node.status, title: node.title },
      ])
    );

    // Group edges by source task (tasks that depend on others)
    const edgesBySource = new Map<number, typeof graphData.edges>();
    for (const edge of graphData.edges) {
      if (!edgesBySource.has(edge.sourceTaskId)) {
        edgesBySource.set(edge.sourceTaskId, []);
      }
      edgesBySource.get(edge.sourceTaskId)!.push(edge);
    }

    // Calculate blocking info for each task
    for (const node of graphData.nodes) {
      const taskId = node.taskId;
      const outgoingEdges = edgesBySource.get(taskId) || [];

      if (outgoingEdges.length === 0) {
        map.set(taskId, {
          isBlocked: false,
          blockedByCount: 0,
          blockingTasks: [],
        });
        continue;
      }

      const blocking: Array<{ taskId: number; title: string; status: string }> = [];

      for (const edge of outgoingEdges) {
        const dependentTask = taskMap.get(edge.dependentTaskId);
        if (!dependentTask) {
          continue; // Skip if dependent task not found
        }

        const dependentTaskStatus = dependentTask.status;

        // Check blocking conditions based on dependency type
        let isBlocking = false;

        switch (edge.dependencyType) {
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
            taskId: edge.dependentTaskId,
            title: dependentTask.title,
            status: dependentTaskStatus,
          });
        }
      }

      map.set(taskId, {
        isBlocked: blocking.length > 0,
        blockedByCount: blocking.length,
        blockingTasks: blocking,
      });
    }

    // Initialize map for tasks without dependencies
    for (const node of graphData.nodes) {
      if (!map.has(node.taskId)) {
        map.set(node.taskId, {
          isBlocked: false,
          blockedByCount: 0,
          blockingTasks: [],
        });
      }
    }

    return map;
  }, [graphData]);

  return {
    blockingMap,
    isLoading,
    isError,
    refetch,
  };
}

