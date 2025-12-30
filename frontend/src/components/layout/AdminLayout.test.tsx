import React from 'react';
import { describe, it, expect, beforeEach } from 'vitest';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { server } from '@/mocks/server';
import { render, screen, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@/contexts/ThemeContext';
import { AuthProvider } from '@/contexts/AuthContext';
import { RequireAdminGuard } from '../guards/RequireAdminGuard';
import { AdminLayout } from './AdminLayout';

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: 0,
      },
      mutations: {
        retry: false,
      },
    },
  });

const TestWrapper = ({
  children,
  initialEntries = ['/admin/dashboard'],
}: {
  children: React.ReactNode;
  initialEntries?: string[];
}) => {
  const queryClient = createTestQueryClient();

  return (
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={initialEntries}>
        <ThemeProvider>
          <AuthProvider>{children}</AuthProvider>
        </ThemeProvider>
      </MemoryRouter>
    </QueryClientProvider>
  );
};

const mockAdminUser = () =>
  server.use(
    http.get('http://localhost:5001/api/v1/Auth/me', () =>
      HttpResponse.json({
        userId: 1,
        username: 'admin',
        email: 'admin@test.com',
        firstName: 'Admin',
        lastName: 'User',
        globalRole: 'Admin',
        organizationId: 1,
        permissions: [],
      }),
    ),
  );

describe('AdminLayout', () => {
  beforeEach(() => {
    server.resetHandlers();
  });

  it('renders admin navigation and outlet content', async () => {
    mockAdminUser();

    render(
      <TestWrapper initialEntries={['/admin/dashboard']}>
        <Routes>
          <Route
            path="/admin/*"
            element={
              <RequireAdminGuard>
                <AdminLayout />
              </RequireAdminGuard>
            }
          >
            <Route path="dashboard" element={<div>Admin Dashboard Content</div>} />
          </Route>
        </Routes>
      </TestWrapper>,
    );

    await waitFor(() => {
      expect(screen.getByText('Admin Dashboard Content')).toBeInTheDocument();
    });

    expect(screen.getByText('Dashboard')).toBeInTheDocument();
    expect(screen.getByText('Users')).toBeInTheDocument();
    expect(screen.getByText('Permissions')).toBeInTheDocument();
    expect(screen.getByText('Settings')).toBeInTheDocument();
  });

  it('shows the Back to App link', async () => {
    mockAdminUser();

    render(
      <TestWrapper initialEntries={['/admin/dashboard']}>
        <Routes>
          <Route
            path="/admin/*"
            element={
              <RequireAdminGuard>
                <AdminLayout />
              </RequireAdminGuard>
            }
          >
            <Route path="dashboard" element={<div>Admin Dashboard Content</div>} />
          </Route>
        </Routes>
      </TestWrapper>,
    );

    await waitFor(() => {
      expect(screen.getByText('Admin Dashboard Content')).toBeInTheDocument();
    });

    const backLink = screen.getByText('Back to App');
    expect(backLink).toBeInTheDocument();
    expect((backLink.closest('a') as HTMLAnchorElement).getAttribute('href')).toBe('/dashboard');
  });
});

