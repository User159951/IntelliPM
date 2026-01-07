import { apiClient } from './client';
import { attachmentsApi, type Attachment } from './attachments';
import type { Task, CreateTaskRequest, UpdateTaskRequest, TaskStatus, TaskAttachment, TaskActivity } from '@/types';

interface TaskFilters {
  status?: TaskStatus;
  assigneeId?: number;
  priority?: string;
}

export const tasksApi = {
  getByProject: (projectId: number, filters?: TaskFilters): Promise<{ tasks: Task[] }> => {
    const params = new URLSearchParams();
    if (filters?.status) params.append('status', filters.status);
    if (filters?.assigneeId) params.append('assigneeId', filters.assigneeId.toString());
    if (filters?.priority) params.append('priority', filters.priority);
    const query = params.toString();
    return apiClient.get(`/api/Tasks/project/${projectId}${query ? `?${query}` : ''}`);
  },

  getById: (taskId: number): Promise<Task> =>
    apiClient.get(`/api/Tasks/${taskId}`),

  getBlockedByProject: (projectId: number): Promise<{ tasks: Task[] }> =>
    apiClient.get(`/api/Tasks/project/${projectId}/blocked`),

  getByAssignee: (assigneeId: number): Promise<Task[]> =>
    apiClient.get(`/api/Tasks/assignee/${assigneeId}`),

  create: (data: CreateTaskRequest): Promise<Task> =>
    apiClient.post('/api/Tasks', data),

  update: (taskId: number, data: UpdateTaskRequest): Promise<Task> =>
    apiClient.put(`/api/Tasks/${taskId}`, data),

  changeStatus: (taskId: number, newStatus: TaskStatus): Promise<Task> =>
    apiClient.patch(`/api/Tasks/${taskId}/status`, { newStatus }),

  assign: (taskId: number, assigneeId?: number): Promise<Task> =>
    apiClient.patch(`/api/Tasks/${taskId}/assign`, { assigneeId }),


  /**
   * Get all attachments for a task
   * @deprecated Use attachmentsApi.getAll('Task', taskId) directly
   * This method is kept for backward compatibility but will be removed in a future version.
   */
  getAttachments: async (taskId: number): Promise<{ attachments: TaskAttachment[] }> => {
    const attachments: Attachment[] = await attachmentsApi.getAll('Task', taskId);
    // Map Attachment to TaskAttachment (they should be compatible)
    return { attachments: attachments as unknown as TaskAttachment[] };
  },

  /**
   * Upload an attachment to a task
   * @deprecated Use attachmentsApi.upload(formData) directly with FormData containing entityType and entityId
   * This method is kept for backward compatibility but will be removed in a future version.
   */
  uploadAttachment: async (taskId: number, file: File): Promise<TaskAttachment> => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('entityType', 'Task');
    formData.append('entityId', taskId.toString());
    const attachment: Attachment = await attachmentsApi.upload(formData);
    return attachment as unknown as TaskAttachment;
  },

  /**
   * Get activity for a task
   * @deprecated This method is deprecated because Activity API doesn't support entity-specific filtering.
   * Use activityApi.getRecent(limit, projectId) and filter by entityId client-side.
   * 
   * @example
   * // Instead of:
   * const activity = await tasksApi.getActivity(taskId);
   * 
   * // Use:
   * const { activities } = await activityApi.getRecent(50, projectId);
   * const taskActivities = activities.filter(a => a.entityType === 'Task' && a.entityId === taskId);
   * 
   * @param _taskId - Task ID (not used, method always returns empty array)
   * @returns Empty activities array
   */
  getActivity: async (_taskId: number): Promise<{ activities: TaskActivity[] }> => {
    // Note: This method is deprecated. Use activityApi.getRecent(limit, projectId) and filter by entityId client-side.
    // The Activity API does not support entity-specific filtering.
    return { activities: [] };
  },
};
