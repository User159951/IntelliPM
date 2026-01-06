import { apiClient } from './client';
import type { PagedResponse, ProjectListDto } from './projects';

export interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface UserListDto {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  globalRole: string;
  organizationId: number;
  organizationName: string;
  createdAt: string;
  isActive: boolean;
  projectCount: number;
  lastLoginAt?: string | null;
}

export interface BulkUpdateUsersStatusRequest {
  userIds: number[];
  isActive: boolean;
}

export interface BulkUpdateUsersStatusResponse {
  successCount: number;
  failureCount: number;
  errors: string[];
}

export interface GetAllUsersResponse {
  users: User[];
}

// Backend returns { Users: [...] } but we normalize to { users: [...] }
export interface GetAllUsersResponseRaw {
  Users: User[];
}

export interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
  globalRole?: string;
}

export interface InviteOrganizationUserRequest {
  email: string;
  role: 'Admin' | 'User';
  firstName: string;
  lastName: string;
}

export interface InviteOrganizationUserResponse {
  invitationId: string;
  email: string;
  invitationLink: string;
}

/**
 * Maps frontend sortField values to backend expected values.
 * Backend accepts: Username, Email, CreatedAt, LastLoginAt, Role, IsActive
 */
function mapSortFieldToBackend(frontendSortField: string): string | undefined {
  const mapping: Record<string, string> = {
    'name': 'CreatedAt', // Backend doesn't have a "name" field, use CreatedAt as default
    'email': 'Email',
    'role': 'Role',
    'createdAt': 'CreatedAt',
    'status': 'IsActive',
    // Also accept backend values directly
    'Username': 'Username',
    'Email': 'Email',
    'CreatedAt': 'CreatedAt',
    'LastLoginAt': 'LastLoginAt',
    'Role': 'Role',
    'IsActive': 'IsActive',
  };
  
  return mapping[frontendSortField];
}

export const usersApi = {
  getAll: async (excludeCurrent = false): Promise<GetAllUsersResponse> => {
    // Use paginated endpoint with max pageSize (100) to get all users
    // Note: Backend limits pageSize to 100, so we use the maximum allowed
    const params = new URLSearchParams({
      page: '1',
      pageSize: '100', // Backend maximum is 100
    });
    if (excludeCurrent) params.append('excludeCurrent', 'true');
    
    const response = await apiClient.get<PagedResponse<UserListDto>>(`/api/v1/Users?${params.toString()}`);
    // Convert paginated response to GetAllUsersResponse format
    const users = (response.items || response.Items || []).map((dto): User => ({
      id: dto.id,
      username: dto.username || '',
      email: dto.email,
      firstName: dto.firstName,
      lastName: dto.lastName,
    }));
    return { users };
  },

  // Admin endpoints
  getAllPaginated: async (
    page = 1,
    pageSize = 20,
    role?: string,
    isActive?: boolean,
    sortField?: string,
    sortDescending = false,
    searchTerm?: string
  ): Promise<PagedResponse<UserListDto>> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      sortDescending: sortDescending.toString(),
    });
    if (role) params.append('role', role);
    if (isActive !== undefined) params.append('isActive', isActive.toString());
    
    // Map frontend sortField values to backend expected values
    // Backend accepts: Username, Email, CreatedAt, LastLoginAt, Role, IsActive
    if (sortField) {
      const backendSortField = mapSortFieldToBackend(sortField);
      if (backendSortField) {
        params.append('sortField', backendSortField);
      }
    }
    
    if (searchTerm) params.append('searchTerm', searchTerm);
    return apiClient.get<PagedResponse<UserListDto>>(`/api/v1/Users?${params.toString()}`);
  },

  bulkUpdateStatus: async (
    data: BulkUpdateUsersStatusRequest
  ): Promise<BulkUpdateUsersStatusResponse> => {
    return apiClient.post<BulkUpdateUsersStatusResponse>('/api/v1/Users/bulk-status', data);
  },

  update: async (id: number, data: UpdateUserRequest): Promise<UserListDto> => {
    return apiClient.put<UserListDto>(`/api/v1/Users/${id}`, data);
  },

  delete: async (id: number): Promise<{ success: boolean }> => {
    return apiClient.delete<{ success: boolean }>(`/api/v1/Users/${id}`);
  },

  deactivate: async (id: number): Promise<{ userId: number; isActive: boolean; username: string; email: string }> => {
    return apiClient.post<{ userId: number; isActive: boolean; username: string; email: string }>(`/api/admin/users/${id}/deactivate`);
  },

  invite: async (data: InviteOrganizationUserRequest): Promise<InviteOrganizationUserResponse> => {
    return apiClient.post<InviteOrganizationUserResponse>('/api/admin/users/invite', data);
  },

  getUserProjects: async (userId: number, page = 1, pageSize = 20): Promise<PagedResponse<ProjectListDto>> => {
    return apiClient.get<PagedResponse<ProjectListDto>>(`/api/v1/Users/${userId}/projects?page=${page}&pageSize=${pageSize}`);
  },

  getUserActivity: async (userId: number, limit = 50): Promise<Array<{ type: string; description: string; timestamp: string }>> => {
    return apiClient.get<Array<{ type: string; description: string; timestamp: string }>>(`/api/v1/Users/${userId}/activity?limit=${limit}`);
  },
};
