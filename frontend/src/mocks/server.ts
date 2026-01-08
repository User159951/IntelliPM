/**
 * Mock Service Worker (MSW) server for testing only.
 * 
 * This module should ONLY be imported in test files (*.test.ts, *.test.tsx, *.spec.ts, *.spec.tsx)
 * or in test setup files (test/setup.ts).
 * 
 * Production builds will fail if this module is imported outside of test context.
 */

// Build-time check: Fail if imported in production or outside test context
if (import.meta.env.PROD) {
  throw new Error(
    'Mock server cannot be imported in production builds. ' +
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
      '⚠️  WARNING: Mock server is being imported outside of test context. ' +
      'This should only be imported in test files (*.test.ts, *.test.tsx, *.spec.ts, *.spec.tsx) ' +
      'or test setup files (test/setup.ts).'
    );
  }
}

import { setupServer, type SetupServer } from 'msw/node';
import type { RequestHandler } from 'msw';

// Create a default handler that can be overridden in tests
const handlers: RequestHandler[] = [
  // Default handlers can be added here if needed
  // For now, we'll let tests provide their own handlers via server.use()
];

// Create the MSW server instance
// This will only be used in test environments
export const server: SetupServer = setupServer(...handlers);

