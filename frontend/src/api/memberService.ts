import { apiClient } from './client';
import type { ProjectRole } from '@/types';

export interface ProjectMember {
  id: number;
  userId: number;
  userName: string;
  email: string;
  role: ProjectRole;
  invitedAt: string;
  invitedByName: string;
}

export interface InviteMemberRequest {
  email: string;
  role: ProjectRole;
}

export interface ChangeRoleRequest {
  NewRole: ProjectRole;
}

export const memberService = {
  getMembers: (projectId: number): Promise<ProjectMember[]> =>
    apiClient.get(`/api/Projects/${projectId}/members`),

  inviteMember: (projectId: number, data: InviteMemberRequest): Promise<{ memberId: number; email: string; role: ProjectRole }> =>
    apiClient.post(`/api/Projects/${projectId}/members`, data),

  changeRole: (projectId: number, userId: number, data: ChangeRoleRequest): Promise<void> =>
    apiClient.put(`/api/Projects/${projectId}/members/${userId}/role`, data),

  removeMember: (projectId: number, userId: number): Promise<void> =>
    apiClient.delete(`/api/Projects/${projectId}/members/${userId}`),

  getUserRole: (projectId: number): Promise<ProjectRole | null> =>
    apiClient.get(`/api/Projects/${projectId}/my-role`),
};

