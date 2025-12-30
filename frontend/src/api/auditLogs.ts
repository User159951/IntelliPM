import { apiClient } from './client';
import type { PagedResponse } from './projects';

export interface AuditLogDto {
  id: number;
  userId?: number;
  userName?: string;
  action: string;
  entityType: string;
  entityId?: number;
  entityName?: string;
  changes?: string;
  ipAddress?: string;
  userAgent?: string;
  createdAt: string;
}

export const auditLogsApi = {
  getAll: async (
    page = 1,
    pageSize = 20,
    action?: string,
    entityType?: string,
    userId?: number,
    startDate?: string,
    endDate?: string
  ): Promise<PagedResponse<AuditLogDto>> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (action) params.append('action', action);
    if (entityType) params.append('entityType', entityType);
    if (userId !== undefined) params.append('userId', userId.toString());
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    return apiClient.get<PagedResponse<AuditLogDto>>(`/api/admin/audit-logs?${params.toString()}`);
  },
};

