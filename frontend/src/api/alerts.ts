import { apiClient } from './client';

export interface Alert {
  id: number;
  projectId: number;
  type: string;
  severity: 'Info' | 'Warning' | 'Error' | 'Critical' | string;
  title: string;
  message: string;
  isRead: boolean;
  isResolved: boolean;
  createdAt: string;
}

export const alertsApi = {
  getUnread: (): Promise<Alert[]> =>
    apiClient.get('/api/Alerts?unreadOnly=true&limit=10'),

  markRead: (id: number): Promise<void> =>
    apiClient.post(`/api/Alerts/${id}/read`),
};

