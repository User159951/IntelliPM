/**
 * Types for task dependencies management.
 * Aligned with backend DTOs and entities.
 */

/**
 * Enum matching backend DependencyType.
 * Represents the type of dependency relationship between tasks.
 * Values match backend enum (1-4).
 */
export enum DependencyType {
  /** Finish-to-Start: The dependent task cannot start until the source task finishes. */
  FinishToStart = 1,
  /** Start-to-Start: The dependent task cannot start until the source task starts. */
  StartToStart = 2,
  /** Finish-to-Finish: The dependent task cannot finish until the source task finishes. */
  FinishToFinish = 3,
  /** Start-to-Finish: The dependent task cannot finish until the source task starts. */
  StartToFinish = 4
}

/**
 * API response type for task dependencies.
 * Aligned with backend TaskDependencyDto.
 */
export interface TaskDependencyDto {
  /** Unique identifier for the dependency */
  id: number;
  /** The task that depends on another task (source task) */
  sourceTaskId: number;
  /** Title of the source task */
  sourceTaskTitle: string;
  /** The task being depended upon (dependent task) */
  dependentTaskId: number;
  /** Title of the dependent task */
  dependentTaskTitle: string;
  /** Type of dependency: "FinishToStart" | "StartToStart" | "FinishToFinish" | "StartToFinish" */
  dependencyType: string;
  /** Date and time when the dependency was created (ISO string) */
  createdAt: string;
  /** Name of the user who created the dependency */
  createdByName: string;
}

/**
 * Node in the dependency graph (represents a task).
 * Aligned with backend DependencyGraphNodeDto.
 */
export interface DependencyGraphNodeDto {
  /** Task ID */
  taskId: number;
  /** Task title */
  title: string;
  /** Task status */
  status: string;
  /** ID of the assigned user (null if unassigned) */
  assigneeId: number | null;
  /** Name of the assigned user (null if unassigned) */
  assigneeName: string | null;
}

/**
 * Edge in the dependency graph (represents a dependency).
 * Aligned with backend DependencyGraphEdgeDto.
 */
export interface DependencyGraphEdgeDto {
  /** Dependency ID */
  id: number;
  /** Source task ID (the task that depends on another) */
  sourceTaskId: number;
  /** Dependent task ID (the task being depended upon) */
  dependentTaskId: number;
  /** Type of dependency: "FinishToStart" | "StartToStart" | "FinishToFinish" | "StartToFinish" */
  dependencyType: string;
  /** Short label for the dependency type: "FS", "SS", "FF", "SF" */
  label: string;
}

/**
 * Complete dependency graph for a project.
 * Aligned with backend DependencyGraphDto.
 */
export interface DependencyGraphDto {
  /** List of nodes (tasks) in the graph */
  nodes: DependencyGraphNodeDto[];
  /** List of edges (dependencies) in the graph */
  edges: DependencyGraphEdgeDto[];
}

/**
 * Request type for adding a task dependency.
 * Aligned with backend AddTaskDependencyRequest.
 */
export interface AddTaskDependencyRequest {
  /** The task being depended upon (dependent task ID) */
  dependentTaskId: number;
  /** Type of dependency: "FinishToStart" | "StartToStart" | "FinishToFinish" | "StartToFinish" */
  dependencyType: string;
}

