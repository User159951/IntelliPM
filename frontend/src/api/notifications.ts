import { apiClient } from './client';
import type { NotificationType } from '@/types/generated/enums';

// Aligned with backend NotificationDto
export interface Notification {
  id: number;
  type: NotificationType;
  message: string;
  entityType?: string | null;
  entityId?: number | null;
  projectId?: number | null;
  isRead: boolean;
  createdAt: string;
}

// Aligned with backend GetNotificationsResponse
// Backend returns { Notifications: [...], UnreadCount: number }
// ASP.NET Core serializes to camelCase: { notifications: [...], unreadCount: number }
export interface GetNotificationsResponse {
  notifications: Notification[]; // Backend: Notifications (PascalCase) -> notifications (camelCase)
  unreadCount: number; // Backend: UnreadCount (PascalCase) -> unreadCount (camelCase)
}

export interface PagedNotifications {
  items: Notification[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export const notificationsApi = {
  getAll: (params: {
    limit?: number;
    unreadOnly?: boolean;
  }): Promise<GetNotificationsResponse> => {
    const query = new URLSearchParams();
    if (params.limit) query.append('limit', params.limit.toString());
    if (params.unreadOnly) query.append('unreadOnly', params.unreadOnly.toString());

    return apiClient.get<GetNotificationsResponse>(`/Notifications?${query.toString()}`);
  },

  // Legacy method for backward compatibility
  getAllLegacy: (unreadOnly: boolean = false, limit: number = 10): Promise<GetNotificationsResponse> => {
    const params = new URLSearchParams();
    if (unreadOnly) params.append('unreadOnly', 'true');
    params.append('limit', limit.toString());
    return apiClient.get<GetNotificationsResponse>(`/Notifications?${params.toString()}`);
  },

  markAsRead: (notificationId: number): Promise<void> =>
    apiClient.patch(`/Notifications/${notificationId}/read`, {}),

  /**
   * Mark all notifications as read for the current user
   * Uses PATCH method: PATCH /Notifications/mark-all-read
   */
  markAllAsRead: (): Promise<void> =>
    apiClient.patch(`/Notifications/mark-all-read`, {}),

  /**
   * Get count of unread notifications for current user
   * Uses the dedicated endpoint: GET /Notifications/unread-count
   * 
   * @returns Promise with unread notification count
   * 
   * @example
   * const { unreadCount } = await notificationsApi.getUnreadCount();
   * console.log(`You have ${unreadCount} unread notifications`);
   */
  getUnreadCount: async (): Promise<{ unreadCount: number }> => {
    return apiClient.get<{ unreadCount: number }>('/Notifications/unread-count');
  },

  // Legacy methods for backward compatibility
  markRead: (id: number): Promise<void> =>
    apiClient.patch(`/Notifications/${id}/read`, {}),

  markAllRead: (): Promise<void> =>
    apiClient.patch('/Notifications/mark-all-read', {}),
};
