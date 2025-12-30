import { apiClient } from './client';
import type { Project, CreateProjectRequest, UpdateProjectRequest, ProjectMember, ProjectRole, ProjectType, ProjectStatus } from '@/types';

interface ProjectListDto {
  id: number;
  name: string;
  description: string;
  type: string;
  status: string;
  createdAt: string;
  members?: Array<{
    userId: number;
    firstName: string;
    lastName: string;
    email: string;
    avatar?: string | null;
  }>;
}

export interface PagedResponse<T> {
  Items?: T[];
  items?: T[];
  Page?: number;
  page?: number;
  PageSize?: number;
  pageSize?: number;
  TotalCount?: number;
  totalCount?: number;
  TotalPages?: number;
  totalPages?: number;
}

export const projectsApi = {
  getAll: async (page: number = 1, pageSize: number = 20): Promise<{ items: Project[]; page: number; pageSize: number; totalCount: number; totalPages: number }> => {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    
    const response = await apiClient.get<PagedResponse<ProjectListDto>>(`/api/Projects?${params.toString()}`);
    
    // Map ProjectListDto to Project
    const items = (response.Items || response.items || []).map((dto): Project => ({
      id: dto.id,
      name: dto.name,
      description: dto.description || '',
      type: dto.type as ProjectType,
      status: dto.status as ProjectStatus,
      sprintDurationDays: 14, // Default value, not in ProjectListDto
      ownerId: 0, // Not available in ProjectListDto
      createdAt: dto.createdAt,
      members: dto.members?.map(m => ({
        id: m.userId,
        userId: m.userId,
        userName: `${m.firstName} ${m.lastName}`,
        firstName: m.firstName,
        lastName: m.lastName,
        email: m.email,
        avatar: m.avatar || undefined,
      })),
      openTasksCount: undefined, // Not in ProjectListDto
      activeSprintId: undefined, // Not in ProjectListDto
    }));
    
    // Normalize response: backend returns { Items, Page, PageSize, TotalCount, TotalPages }
    return {
      items,
      page: response.Page ?? response.page ?? page,
      pageSize: response.PageSize ?? response.pageSize ?? pageSize,
      totalCount: response.TotalCount ?? response.totalCount ?? 0,
      totalPages: response.TotalPages ?? response.totalPages ?? 0,
    };
  },

  getById: (id: number): Promise<Project> =>
    apiClient.get(`/api/Projects/${id}`),

  create: (data: CreateProjectRequest): Promise<Project> =>
    apiClient.post('/api/Projects', data),

  update: (id: number, data: UpdateProjectRequest): Promise<Project> =>
    apiClient.put(`/api/Projects/${id}`, data),

  archive: (id: number): Promise<void> =>
    apiClient.delete(`/api/Projects/${id}`),

  deletePermanent: (id: number): Promise<void> =>
    apiClient.delete(`/api/Projects/${id}/permanent`),

  getMembers: async (id: number): Promise<ProjectMember[]> => {
    interface ProjectMemberDto {
      Id: number;
      id?: number;
      UserId: number;
      userId?: number;
      UserName: string;
      userName?: string;
      Email: string;
      email?: string;
      Role: ProjectRole;
      role?: ProjectRole;
      InvitedAt: string;
      invitedAt?: string;
      InvitedByName: string;
      invitedByName?: string;
    }
    
    const response = await apiClient.get<ProjectMemberDto[]>(`/api/Projects/${id}/members`);
    
    // Map ProjectMemberDto to ProjectMember
    return response.map((dto): ProjectMember => {
      const id = dto.Id ?? dto.id ?? 0;
      const userId = dto.UserId ?? dto.userId ?? 0;
      const userName = dto.UserName ?? dto.userName ?? '';
      const email = dto.Email ?? dto.email ?? '';
      const role = (dto.Role ?? dto.role) as ProjectRole;
      const invitedAt = dto.InvitedAt ?? dto.invitedAt ?? '';
      const invitedByName = dto.InvitedByName ?? dto.invitedByName ?? '';
      
      // Parse userName to firstName/lastName if possible
      const nameParts = userName.split(' ');
      const firstName = nameParts[0] || undefined;
      const lastName = nameParts.slice(1).join(' ') || undefined;
      
      return {
        id,
        userId,
        userName,
        firstName,
        lastName,
        email,
        role,
        invitedAt,
        invitedByName,
      };
    });
  },

  inviteMember: (id: number, data: { email: string; role: ProjectRole }): Promise<{ memberId: number; email: string; role: ProjectRole }> =>
    apiClient.post(`/api/Projects/${id}/members`, data),

  updateMemberRole: (projectId: number, userId: number, role: ProjectRole): Promise<void> =>
    apiClient.put(`/api/Projects/${projectId}/members/${userId}/role`, { NewRole: role }),

  removeMember: (projectId: number, userId: number): Promise<void> =>
    apiClient.delete(`/api/Projects/${projectId}/members/${userId}`),

  /**
   * Assign a team to a project
   * Uses relative path - apiClient will handle versioning automatically
   * Endpoint: POST /api/v1/Projects/{projectId}/assign-team
   * 
   * @param projectId - Project ID
   * @param data - Team assignment data (teamId, defaultRole, memberRoleOverrides)
   * @returns Assignment result with assigned members
   * 
   * @example
   * const result = await projectsApi.assignTeam(123, {
   *   teamId: 456,
   *   defaultRole: 'Developer',
   *   memberRoleOverrides: { 789: 'Lead' }
   * });
   */
  assignTeam: async (
    projectId: number,
    data: {
      teamId: number;
      defaultRole?: ProjectRole;
      memberRoleOverrides?: Record<number, ProjectRole>;
    }
  ): Promise<{
    projectId: number;
    teamId: number;
    assignedMembers: Array<{
      userId: number;
      username: string;
      role: ProjectRole;
      alreadyMember: boolean;
    }>;
  }> => {
    // Use relative path (no /api/ prefix) - apiClient will automatically add /api/v1/
    // This ensures consistent versioning across all endpoints
    return apiClient.post(`/Projects/${projectId}/assign-team`, data);
  },
};
