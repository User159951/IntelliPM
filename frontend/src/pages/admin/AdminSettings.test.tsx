import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { server } from '@/mocks/server';
import AdminSettings from './AdminSettings';
import { AuthProvider } from '@/contexts/AuthContext';

const PROJECT_CREATION_KEY = 'ProjectCreation.AllowedRoles';

type CustomPutHandler = (info: { request: Request }) => Promise<Response> | Response;

const renderWithProviders = (initialSettings?: Record<string, string>, customPutHandler?: CustomPutHandler) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  const handlers = [
    http.get('*/api/v1/Auth/me', () => {
      return HttpResponse.json({
        userId: 1,
        username: 'admin',
        email: 'admin@test.com',
        globalRole: 'Admin',
        organizationId: 1,
        permissions: ['admin.settings.update'],
      });
    }),
    http.get('*/api/v1/Settings', () => {
      return HttpResponse.json(initialSettings || { [PROJECT_CREATION_KEY]: 'Admin,User' });
    }),
  ];

  // Add custom PUT handler if provided, otherwise use default
  if (customPutHandler) {
    handlers.push(
      http.put(`*/api/v1/Settings/${encodeURIComponent(PROJECT_CREATION_KEY)}`, customPutHandler)
    );
  } else {
    handlers.push(
      http.put(`*/api/v1/Settings/${encodeURIComponent(PROJECT_CREATION_KEY)}`, async ({ request }) => {
        const body = await request.json() as { value: string };
        return HttpResponse.json({ key: PROJECT_CREATION_KEY, value: body.value });
      })
    );
  }

  server.use(...handlers);

  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <AdminSettings />
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
};

describe('AdminSettings', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders settings page with current value', async () => {
    renderWithProviders({ [PROJECT_CREATION_KEY]: 'Admin,User' });

    await waitFor(() => {
      expect(screen.getByText('Settings')).toBeInTheDocument();
      expect(screen.getByText('Project Creation')).toBeInTheDocument();
    });

    // Wait for data to load (skeleton disappears)
    await waitFor(() => {
      expect(screen.queryByText('Allowed Roles')).toBeInTheDocument();
    }, { timeout: 3000 });

    const allUsersRadio = screen.getByLabelText('All Users');
    expect(allUsersRadio).toBeChecked();
  });

  it('displays Admin Only when setting is Admin', async () => {
    renderWithProviders({ [PROJECT_CREATION_KEY]: 'Admin' });

    await waitFor(() => {
      const adminOnlyRadio = screen.getByLabelText('Admin Only');
      expect(adminOnlyRadio).toBeChecked();
    });
  });

  it('updates setting when save button is clicked', { timeout: 10000 }, async () => {
    const user = userEvent.setup();
    let putCalled = false;
    let putValue = '';
    
    // Custom PUT handler to track calls
    const customPutHandler = async ({ request }: { request: Request }) => {
      putCalled = true;
      const body = await request.json() as { value: string };
      putValue = body.value;
      return HttpResponse.json({ key: PROJECT_CREATION_KEY, value: body.value });
    };

    renderWithProviders({ [PROJECT_CREATION_KEY]: 'Admin,User' }, customPutHandler);

    // Wait for data to load
    await waitFor(() => {
      expect(screen.getByText('Allowed Roles')).toBeInTheDocument();
    }, { timeout: 3000 });

    // Verify initial state - All Users should be selected
    const allUsersRadio = screen.getByLabelText('All Users');
    expect(allUsersRadio).toBeChecked();

    // Change to Admin Only
    const adminOnlyRadio = screen.getByLabelText('Admin Only');
    await user.click(adminOnlyRadio);

    // Verify radio button changed
    await waitFor(() => {
      expect(adminOnlyRadio).toBeChecked();
    });

    // Wait for button to become enabled (hasChanges should be true after radio change)
    await waitFor(() => {
      const saveButton = screen.getByText('Save Changes');
      expect(saveButton).not.toBeDisabled();
    }, { timeout: 3000 });

    // Find and click save button
    const saveButton = screen.getByText('Save Changes');
    expect(saveButton).not.toBeDisabled();
    
    await user.click(saveButton);

    // Verify mutation was called with correct value
    await waitFor(() => {
      expect(putCalled).toBe(true);
      expect(putValue).toBe('Admin');
    }, { timeout: 5000 });
  });
});

