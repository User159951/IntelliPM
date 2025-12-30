import { setupServer, type SetupServer } from 'msw/node';
import type { RequestHandler } from 'msw';

// Create a default handler that can be overridden in tests
const handlers: RequestHandler[] = [
  // Default handlers can be added here if needed
  // For now, we'll let tests provide their own handlers via server.use()
];

// Create the MSW server instance
export const server: SetupServer = setupServer(...handlers);

