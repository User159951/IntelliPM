import { apiClient } from './client';
import type {
  TaskDependencyDto,
  DependencyGraphDto,
  AddTaskDependencyRequest,
} from '@/types/dependencies';

/**
 * API client for task dependencies management.
 * Provides functions to create, retrieve, and delete task dependencies,
 * as well as retrieve dependency graphs for visualization.
 */
export const dependenciesApi = {
  /**
   * Add a dependency to a task.
   * Creates a dependency where the specified task depends on another task.
   * 
   * @param taskId - The task that will depend on another task (source task ID)
   * @param data - Dependency data (dependentTaskId and dependencyType)
   * @returns Created dependency information
   * 
   * @throws {Error} If the dependency would create a cycle
   * @throws {Error} If validation fails (tasks not found, same task, etc.)
   * @throws {Error} If the task already has the maximum number of dependencies (20)
   * 
   * @example
   * // Add a Finish-to-Start dependency
   * const dependency = await dependenciesApi.addTaskDependency(123, {
   *   dependentTaskId: 456,
   *   dependencyType: 'FinishToStart'
   * });
   * 
   * @example
   * // Add a Start-to-Start dependency
   * const dependency = await dependenciesApi.addTaskDependency(123, {
   *   dependentTaskId: 789,
   *   dependencyType: 'StartToStart'
   * });
   */
  addTaskDependency: async (
    taskId: number,
    data: AddTaskDependencyRequest
  ): Promise<TaskDependencyDto> => {
    try {
      // Endpoint: POST /api/v1/tasks/{taskId}/dependencies
      return apiClient.post<TaskDependencyDto>(`/tasks/${taskId}/dependencies`, data);
    } catch (error) {
      // Re-throw with more context
      if (error instanceof Error) {
        throw new Error(`Failed to add task dependency: ${error.message}`);
      }
      throw error;
    }
  },

  /**
   * Remove a task dependency.
   * Deletes the dependency with the specified ID.
   * 
   * @param dependencyId - The ID of the dependency to remove
   * @returns Promise that resolves when the dependency is removed
   * 
   * @throws {Error} If the dependency is not found
   * @throws {Error} If the dependency does not belong to the user's organization
   * 
   * @example
   * // Remove dependency with ID 42
   * await dependenciesApi.removeTaskDependency(42);
   */
  removeTaskDependency: async (dependencyId: number): Promise<void> => {
    try {
      // Endpoint: DELETE /api/v1/tasks/dependencies/{dependencyId}
      return apiClient.delete(`/tasks/dependencies/${dependencyId}`);
    } catch (error) {
      // Re-throw with more context
      if (error instanceof Error) {
        throw new Error(`Failed to remove task dependency: ${error.message}`);
      }
      throw error;
    }
  },

  /**
   * Get all dependencies for a specific task.
   * Returns dependencies where the task is either the source or the dependent task.
   * 
   * @param taskId - The task ID to get dependencies for
   * @returns Array of dependencies for the specified task
   * 
   * @throws {Error} If the task is not found
   * 
   * @example
   * // Get all dependencies for task 123
   * const dependencies = await dependenciesApi.getTaskDependencies(123);
   * 
   * // Filter dependencies where task is the source
   * const outgoingDeps = dependencies.filter(d => d.sourceTaskId === 123);
   * 
   * // Filter dependencies where task is the dependent
   * const incomingDeps = dependencies.filter(d => d.dependentTaskId === 123);
   */
  getTaskDependencies: async (taskId: number): Promise<TaskDependencyDto[]> => {
    try {
      // Endpoint: GET /api/v1/tasks/{taskId}/dependencies
      return apiClient.get<TaskDependencyDto[]>(`/tasks/${taskId}/dependencies`);
    } catch (error) {
      // Re-throw with more context
      if (error instanceof Error) {
        throw new Error(`Failed to get task dependencies: ${error.message}`);
      }
      throw error;
    }
  },

  /**
   * Get the complete dependency graph for a project.
   * Returns all tasks and their dependencies in the project for visualization.
   * 
   * @param projectId - The project ID to get the dependency graph for
   * @returns Dependency graph with nodes (tasks) and edges (dependencies)
   * 
   * @throws {Error} If the project is not found
   * 
   * @example
   * // Get dependency graph for project 1
   * const graph = await dependenciesApi.getProjectDependencyGraph(1);
   * 
   * // Use with react-flow
   * const nodes = graph.nodes.map(node => ({
   *   id: node.taskId.toString(),
   *   data: { label: node.title },
   *   position: { x: 0, y: 0 }
   * }));
   * 
   * const edges = graph.edges.map(edge => ({
   *   id: edge.id.toString(),
   *   source: edge.sourceTaskId.toString(),
   *   target: edge.dependentTaskId.toString(),
   *   label: edge.label
   * }));
   */
  getProjectDependencyGraph: async (projectId: number): Promise<DependencyGraphDto> => {
    try {
      // Endpoint: GET /api/v1/projects/{projectId}/dependency-graph
      return apiClient.get<DependencyGraphDto>(`/projects/${projectId}/dependency-graph`);
    } catch (error) {
      // Re-throw with more context
      if (error instanceof Error) {
        throw new Error(`Failed to get project dependency graph: ${error.message}`);
      }
      throw error;
    }
  },
};

