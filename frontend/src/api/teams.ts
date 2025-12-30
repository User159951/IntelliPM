import { apiClient } from './client';
import type { Team, TeamCapacity, RegisterTeamRequest } from '@/types';

export const teamsApi = {
  getAll: (): Promise<{ teams: Team[] }> =>
    apiClient.get('/api/Teams'),

  getById: (id: number): Promise<Team> =>
    apiClient.get(`/api/Teams/${id}`),

  create: (data: RegisterTeamRequest): Promise<Team> =>
    apiClient.post('/api/Teams', data),

  updateCapacity: (id: number, newCapacity: number): Promise<void> =>
    apiClient.patch(`/api/Teams/${id}/capacity`, { newCapacity }),

  getAvailability: (id: number): Promise<TeamCapacity> =>
    apiClient.get(`/api/Teams/${id}/availability`),
};
