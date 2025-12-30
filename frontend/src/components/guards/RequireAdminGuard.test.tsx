import React from 'react'
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { http, HttpResponse } from 'msw'
import { server } from '@/mocks/server'
import { AuthProvider } from '@/contexts/AuthContext'
import { ThemeProvider } from '@/contexts/ThemeContext'
import { RequireAdminGuard } from './RequireAdminGuard'

const toastMock = vi.fn()
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: toastMock,
  }),
}))

// Create a test query client
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
  })

// Wrapper component for tests
const TestWrapper = ({ children, initialEntries = ['/'] }: { children: React.ReactNode; initialEntries?: string[] }) => {
  const queryClient = createTestQueryClient()
  return (
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={initialEntries}>
        <ThemeProvider>
          <AuthProvider>
            {children}
          </AuthProvider>
        </ThemeProvider>
      </MemoryRouter>
    </QueryClientProvider>
  )
}

describe('RequireAdminGuard', () => {
  beforeEach(() => {
    server.resetHandlers()
    toastMock.mockClear()
    // Silence notification fetches triggered by Header/contexts
    server.use(
      http.get('http://localhost:5001/api/v1/Notifications', () =>
        HttpResponse.json({ items: [], totalCount: 0 })
      )
    )
  })

  it('allows admin users to access admin routes', async () => {
    // Mock successful admin user response
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json({
          userId: 1,
          username: 'admin',
          email: 'admin@test.com',
          firstName: 'Admin',
          lastName: 'User',
          globalRole: 'Admin',
          organizationId: 1,
          permissions: [],
        })
      })
    )

    render(
      <TestWrapper initialEntries={['/admin']}>
        <Routes>
          <Route
            path="/admin"
            element={
              <RequireAdminGuard>
                <div>Admin Content</div>
              </RequireAdminGuard>
            }
          />
        </Routes>
      </TestWrapper>
    )

    // Wait for loading to complete and admin content to appear
    await waitFor(
      () => {
        expect(screen.getByText('Admin Content')).toBeInTheDocument()
      },
      { timeout: 3000 }
    )

    expect(screen.getByText('Admin Content')).toBeInTheDocument()
  })

  it('redirects non-admin users to /dashboard', async () => {
    // Mock non-admin user response
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json({
          userId: 2,
          username: 'user',
          email: 'user@test.com',
          firstName: 'Regular',
          lastName: 'User',
          globalRole: 'User',
          organizationId: 1,
          permissions: [],
        })
      })
    )

    render(
      <TestWrapper initialEntries={['/admin']}>
        <Routes>
          <Route
            path="/admin"
            element={
              <RequireAdminGuard>
                <div>Admin Content</div>
              </RequireAdminGuard>
            }
          />
          <Route path="/dashboard" element={<div>Dashboard</div>} />
        </Routes>
      </TestWrapper>
    )

    // Wait for redirect to occur - Navigate component should redirect to /dashboard
    await waitFor(
      () => {
        expect(screen.getByText('Dashboard')).toBeInTheDocument()
      },
      { timeout: 3000 }
    )

    expect(screen.queryByText('Admin Content')).not.toBeInTheDocument()
    expect(toastMock).toHaveBeenCalledWith({
      title: 'Access Denied',
      description: "You don't have permission to access the admin area.",
      variant: 'destructive',
    })
  })

  it('shows loading state while checking authentication', async () => {
    // Mock delayed response
    let resolveRequest: (value: any) => void
    const delayedResponse = new Promise((resolve) => {
      resolveRequest = resolve
    })

    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', async () => {
        await delayedResponse
        return HttpResponse.json({
          userId: 1,
          username: 'admin',
          email: 'admin@test.com',
          firstName: 'Admin',
          lastName: 'User',
          globalRole: 'Admin',
          organizationId: 1,
          permissions: [],
        })
      })
    )

    render(
      <TestWrapper initialEntries={['/admin']}>
        <Routes>
          <Route
            path="/admin"
            element={
              <RequireAdminGuard>
                <div>Admin Content</div>
              </RequireAdminGuard>
            }
          />
        </Routes>
      </TestWrapper>
    )

    // Should show loading initially
    expect(screen.getByText('Loading...')).toBeInTheDocument()

    // Resolve the delayed request
    resolveRequest!(null)

    // Wait for loading to complete and admin content to appear
    await waitFor(
      () => {
        expect(screen.getByText('Admin Content')).toBeInTheDocument()
      },
      { timeout: 3000 }
    )

    expect(screen.queryByText('Loading...')).not.toBeInTheDocument()
  })

  it('blocks direct navigation to admin routes for non-admin and shows toast', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json({
          userId: 3,
          username: 'notadmin',
          email: 'user@test.com',
          firstName: 'Not',
          lastName: 'Admin',
          globalRole: 'User',
          organizationId: 1,
          permissions: [],
        })
      })
    )

    render(
      <TestWrapper initialEntries={['/admin/users'] /* direct deep-link */}>
        <Routes>
          <Route
            path="/admin/*"
            element={
              <RequireAdminGuard>
                <div>Admin Users</div>
              </RequireAdminGuard>
            }
          />
          <Route path="/dashboard" element={<div>Dashboard</div>} />
        </Routes>
      </TestWrapper>
    )

    await waitFor(() => {
      expect(screen.getByText('Dashboard')).toBeInTheDocument()
    })

    expect(screen.queryByText('Admin Users')).not.toBeInTheDocument()
    expect(toastMock).toHaveBeenCalledWith({
      title: 'Access Denied',
      description: "You don't have permission to access the admin area.",
      variant: 'destructive',
    })
  })
})
