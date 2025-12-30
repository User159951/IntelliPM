import { apiClient } from './client';
import type { GlobalRole } from '@/types';

export interface PermissionDto {
  id: number;
  name: string;
  category: string;
  description?: string | null;
}

export interface PermissionsMatrixDto {
  permissions: PermissionDto[];
  rolePermissions: Record<GlobalRole, number[]>;
}

export const permissionsApi = {
  getMatrix: (): Promise<PermissionsMatrixDto> =>
    apiClient.get<PermissionsMatrixDto>('/permissions/matrix'),

  updateRolePermissions: (role: GlobalRole, permissionIds: number[]): Promise<void> =>
    apiClient.put(`/permissions/roles/${role}`, { permissionIds }),
};

