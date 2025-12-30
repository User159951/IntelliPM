import { apiClient } from './client';
import type { Sprint, CreateSprintRequest } from '@/types';

export const sprintsApi = {
  getByProject: (projectId: number): Promise<{ sprints: Sprint[] }> =>
    apiClient.get(`/api/Sprints/project/${projectId}`),

  getById: (id: number): Promise<Sprint> =>
    apiClient.get(`/api/Sprints/${id}`),

  create: (data: CreateSprintRequest): Promise<Sprint> =>
    apiClient.post('/api/Sprints', data),

  assignTasks: (sprintId: number, taskIds: number[]): Promise<void> =>
    apiClient.post(`/api/Sprints/${sprintId}/assign-tasks`, { taskIds }),

  start: (id: number): Promise<void> =>
    apiClient.patch(`/api/Sprints/${id}/start`),

  complete: (id: number, incompleteTasksAction?: 'next_sprint' | 'backlog' | 'keep'): Promise<void> =>
    apiClient.patch(`/api/Sprints/${id}/complete`, { incompleteTasksAction }),

  addTasksToSprint: (sprintId: number, data: {
    taskIds: number[];
    ignoreCapacityWarning?: boolean;
  }): Promise<{ isOverCapacity?: boolean; capacityWarning?: string }> =>
    apiClient.post(`/api/Sprints/${sprintId}/add-tasks`, data),

  removeTasksFromSprint: (sprintId: number, data: {
    taskIds: number[];
  }): Promise<void> =>
    apiClient.post(`/api/Sprints/${sprintId}/remove-tasks`, data),
};
