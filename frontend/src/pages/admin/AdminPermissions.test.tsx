import React from 'react';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { server } from '@/mocks/server';
import AdminPermissions from './AdminPermissions';

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0 },
      mutations: { retry: false },
    },
  });

const TestWrapper = ({ children }: { children: React.ReactNode }) => {
  const queryClient = createTestQueryClient();
  return (
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>{children}</MemoryRouter>
    </QueryClientProvider>
  );
};

const mockMatrix = {
  permissions: [
    { id: 1, name: 'projects.view', category: 'Projects', description: 'View projects' },
    { id: 2, name: 'projects.create', category: 'Projects', description: 'Create projects' },
    { id: 3, name: 'admin.permissions.update', category: 'Admin', description: 'Manage permissions' },
  ],
  rolePermissions: {
    Admin: [1, 2, 3],
    User: [1],
  },
};

describe('AdminPermissions', () => {
  beforeEach(() => {
    server.resetHandlers();
  });

  it('renders matrix grouped by category and shows checkboxes', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/permissions/matrix', () => HttpResponse.json(mockMatrix))
    );

    render(
      <TestWrapper>
        <AdminPermissions />
      </TestWrapper>
    );

    await waitFor(() => {
      expect(screen.getByText('Permissions Matrix')).toBeInTheDocument();
    });

    // Permissions should be visible (accordions open by default)
    expect(screen.getByText('projects.view')).toBeInTheDocument();
    expect(screen.getByText('projects.create')).toBeInTheDocument();
    expect(screen.getByText('admin.permissions.update')).toBeInTheDocument();
  });

  it('allows toggling permissions and saving updates', async () => {
    const putHandler = vi.fn((_role: string) => HttpResponse.json({}));
    server.use(
      http.get('http://localhost:5001/api/v1/permissions/matrix', () => HttpResponse.json(mockMatrix)),
      http.put('http://localhost:5001/api/v1/permissions/roles/:role', ({ params }) => {
        putHandler(params.role as string);
        return HttpResponse.json({});
      })
    );

    render(
      <TestWrapper>
        <AdminPermissions />
      </TestWrapper>
    );

    await waitFor(() => {
      expect(screen.getByText('Permissions Matrix')).toBeInTheDocument();
    });

    // Toggle User permission for projects.create
    const projectsCreateUser = screen.getByLabelText('projects.create-User');
    fireEvent.click(projectsCreateUser);

    fireEvent.click(screen.getByText('Save Changes'));
    await waitFor(() => expect(screen.getByText('Save permission changes?')).toBeInTheDocument());
    fireEvent.click(screen.getByText('Save'));

    await waitFor(() => expect(putHandler).toHaveBeenCalledTimes(2));
    expect(putHandler).toHaveBeenCalledWith('Admin');
    expect(putHandler).toHaveBeenCalledWith('User');
  });
});

