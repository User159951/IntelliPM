/**
 * Mock data utilities for testing only.
 * 
 * This module should ONLY be imported in test files (*.test.ts, *.test.tsx, *.spec.ts, *.spec.tsx).
 * 
 * Production builds will fail if this module is imported outside of test context.
 */

// Build-time check: Fail if imported in production
if (import.meta.env.PROD) {
  throw new Error(
    'Mock data cannot be imported in production builds. ' +
    'This module is only available in test environments.'
  );
}

// Runtime check: Ensure we're in a test environment
// In production, this check is already handled by the build-time check above
// This is mainly for development mode warnings
if (import.meta.env.DEV && !import.meta.env.PROD) {
  const isTestEnvironment = 
    typeof process !== 'undefined' && 
    (process.env.NODE_ENV === 'test' || 
     process.env.VITEST === 'true' ||
     import.meta.env.MODE === 'test');
  
  if (!isTestEnvironment) {
    console.warn(
      '⚠️  WARNING: Mock data is being imported outside of test context. ' +
      'This should only be imported in test files (*.test.ts, *.test.tsx, *.spec.ts, *.spec.tsx).'
    );
  }
}

import type { Project, ProjectType, ProjectStatus } from '@/types';

/**
 * Creates a mock project for testing
 * 
 * @throws {Error} If called outside of test environment
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

