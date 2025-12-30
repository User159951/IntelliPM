import type { Project, ProjectType, ProjectStatus } from '@/types';

/**
 * Creates a mock project for testing
 */
export function createMockProject(overrides?: Partial<Project>): Project {
  const defaultProject: Project = {
    id: 1,
    name: 'Test Project',
    description: 'Test Description',
    type: 'Scrum' as ProjectType,
    status: 'Active' as ProjectStatus,
    sprintDurationDays: 14,
    ownerId: 1,
    ownerName: 'Test Owner',
    createdAt: '2024-01-01T00:00:00Z',
    startDate: '2024-01-01',
    endDate: '2024-12-31',
    members: [],
    openTasksCount: 0,
  };

  return {
    ...defaultProject,
    ...overrides,
  };
}

