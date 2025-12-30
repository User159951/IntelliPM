import { apiClient } from './client';

// Aligned with backend NotificationDto
export interface Notification {
  id: number;
  type: string;
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
   * Get unread notification count
   * Tries to use the dedicated endpoint first, falls back to getAll() if it fails.
   * 
   * Fallback strategy:
   * 1. Try dedicated endpoint: GET /Notifications/unread-count
   * 2. Fallback: GET /Notifications?unreadOnly=true&limit=1000
   *    - Uses unreadCount from response (more accurate than counting items)
   *    - If unreadCount is not available, counts the returned items
   * 
   * @returns Object with count of unread notifications
   * 
   * @example
   * const { count } = await notificationsApi.getUnreadCount();
   * console.log(`You have ${count} unread notifications`);
   */
  getUnreadCount: async (): Promise<{ count: number }> => {
    try {
      // Primary: Try to use the dedicated unread-count endpoint
      return await apiClient.get<{ count: number }>(`/Notifications/unread-count`);
    } catch (error) {
      // Fallback: get all unread notifications with large limit (equivalent to pageSize: 1000)
      // The backend's GetAll endpoint returns unreadCount directly in the response
      try {
        const result = await notificationsApi.getAll({ unreadOnly: true, limit: 1000 });
        
        // Use unreadCount from response (most accurate - total count from database)
        // If unreadCount is not available, fall back to counting returned items
        if (result.unreadCount !== undefined && result.unreadCount !== null) {
          return { count: result.unreadCount };
        } else {
          // Last resort: count the items returned (may be limited by limit parameter)
          return { count: result.notifications.length };
        }
      } catch (fallbackError) {
        // If fallback also fails, return 0 as a safe default
        console.warn(
          '[Notifications API] Failed to get unread count, both primary and fallback methods failed:',
          fallbackError
        );
        return { count: 0 };
      }
    }
  },

  // Legacy methods for backward compatibility
  markRead: (id: number): Promise<void> =>
    apiClient.patch(`/Notifications/${id}/read`, {}),

  markAllRead: (): Promise<void> =>
    apiClient.patch('/Notifications/mark-all-read', {}),
};
