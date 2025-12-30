import { apiClient } from './client';
import type { Defect, CreateDefectRequest, UpdateDefectRequest, DefectStatus, DefectSeverity } from '@/types';

export interface DefectDetail extends Defect {
  reportedById?: number;
  reportedByName?: string;
  assignedToId?: number;
  assignedToName?: string;
  resolution?: string;
  resolvedAt?: string;
  updatedAt?: string;
  reportedAt?: string;
  userStoryTitle?: string;
  sprintName?: string;
}

export const defectsApi = {
  getByProject: (
    projectId: number,
    filters?: {
      status?: DefectStatus;
      severity?: DefectSeverity;
      assignedToId?: number;
    }
  ): Promise<{ defects: Defect[]; total: number }> => {
    const params = new URLSearchParams();
    if (filters?.status) params.append('status', filters.status);
    if (filters?.severity) params.append('severity', filters.severity);
    if (filters?.assignedToId) params.append('assignedToId', filters.assignedToId.toString());
    const query = params.toString();
    return apiClient.get(`/api/projects/${projectId}/defects${query ? `?${query}` : ''}`);
  },

  getById: (projectId: number, id: number): Promise<DefectDetail> =>
    apiClient.get(`/api/projects/${projectId}/defects/${id}`),

  create: (projectId: number, data: CreateDefectRequest): Promise<Defect> =>
    apiClient.post(`/api/projects/${projectId}/defects`, data),

  update: (projectId: number, id: number, data: UpdateDefectRequest): Promise<Defect> =>
    apiClient.patch(`/api/projects/${projectId}/defects/${id}`, data),

  delete: (projectId: number, id: number): Promise<void> =>
    apiClient.delete(`/api/projects/${projectId}/defects/${id}`),
};
