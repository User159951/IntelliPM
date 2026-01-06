import { apiClient } from './client';
import type { PagedResponse } from './projects';
import type { UserListDto } from './users';

export interface OrganizationDto {
  id: number;
  name: string;
  code: string;
  createdAt: string;
  updatedAt?: string | null;
  userCount: number;
}

export interface CreateOrganizationRequest {
  name: string;
  code: string;
}

export interface CreateOrganizationResponse {
  organizationId: number;
  name: string;
  code: string;
  createdAt: string;
}

export interface UpdateOrganizationRequest {
  name: string;
  code: string;
}

export interface UpdateOrganizationResponse {
  organizationId: number;
  name: string;
  code: string;
  updatedAt: string;
}

export interface DeleteOrganizationResponse {
  organizationId: number;
  success: boolean;
  message: string;
}

export interface UpdateUserGlobalRoleRequest {
  userId: number;
  globalRole: 'User' | 'Admin' | 'SuperAdmin';
}

export interface UpdateUserGlobalRoleResponse {
  userId: number;
  globalRole: 'User' | 'Admin' | 'SuperAdmin';
  message: string;
}

export const organizationsApi = {
  // SuperAdmin endpoints
  getAll: async (page = 1, pageSize = 20, searchTerm?: string): Promise<PagedResponse<OrganizationDto>> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (searchTerm) params.append('searchTerm', searchTerm);
    return apiClient.get<PagedResponse<OrganizationDto>>(`/api/admin/organizations?${params.toString()}`);
  },

  getById: async (orgId: number): Promise<OrganizationDto> => {
    return apiClient.get<OrganizationDto>(`/api/admin/organizations/${orgId}`);
  },

  create: async (data: CreateOrganizationRequest): Promise<CreateOrganizationResponse> => {
    return apiClient.post<CreateOrganizationResponse>('/api/admin/organizations', data);
  },

  update: async (orgId: number, data: UpdateOrganizationRequest): Promise<UpdateOrganizationResponse> => {
    return apiClient.put<UpdateOrganizationResponse>(`/api/admin/organizations/${orgId}`, {
      organizationId: orgId,
      ...data,
    });
  },

  delete: async (orgId: number): Promise<DeleteOrganizationResponse> => {
    return apiClient.delete<DeleteOrganizationResponse>(`/api/admin/organizations/${orgId}`);
  },

  // Admin endpoints (own organization)
  getMyOrganization: async (): Promise<OrganizationDto> => {
    return apiClient.get<OrganizationDto>('/api/admin/organization/me');
  },

  getMembers: async (page = 1, pageSize = 20, searchTerm?: string): Promise<PagedResponse<UserListDto>> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (searchTerm) params.append('searchTerm', searchTerm);
    return apiClient.get<PagedResponse<UserListDto>>(`/api/admin/organization/members?${params.toString()}`);
  },

  updateMemberRole: async (userId: number, globalRole: 'User' | 'Admin'): Promise<UpdateUserGlobalRoleResponse> => {
    return apiClient.put<UpdateUserGlobalRoleResponse>(`/api/admin/organization/members/${userId}/global-role`, {
      userId,
      globalRole,
    });
  },
};

