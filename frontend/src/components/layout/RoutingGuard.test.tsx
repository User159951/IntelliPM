import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render as rtlRender, screen, waitFor } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { server } from '@/mocks/server'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { AuthProvider } from '@/contexts/AuthContext'
import { ThemeProvider } from '@/contexts/ThemeContext'
import { MainLayout } from './MainLayout'

// Create a test query client
const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

// Custom render function without BrowserRouter (we use MemoryRouter in tests)
const render = (ui: React.ReactElement, options?: { queryClient?: QueryClient }) => {
  const queryClient = options?.queryClient || createTestQueryClient()
  
  return rtlRender(
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>
          {ui}
        </AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  )
}

// Mock the child components
vi.mock('./AppSidebar', () => ({
  AppSidebar: () => <div data-testid="app-sidebar">Sidebar</div>,
}))

vi.mock('./Header', () => ({
  Header: () => <div data-testid="header">Header</div>,
}))

vi.mock('@/components/search/GlobalSearchModal', () => ({
  GlobalSearchModal: () => null,
}))

// Mock page components
const MockDashboard = () => <div>Dashboard Page</div>
const MockProjects = () => <div>Projects Page</div>
const MockProfile = () => <div>Profile Page</div>

describe('Routing Guard - Protected Routes', () => {
  beforeEach(() => {
    server.resetHandlers()
  })

  it('redirects /dashboard to /login when not authenticated', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json(
          { error: 'Unauthorized' },
          { status: 401 }
        )
      })
    )

    render(
      <MemoryRouter initialEntries={['/dashboard']}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route element={<MainLayout />}>
            <Route path="/dashboard" element={<MockDashboard />} />
          </Route>
        </Routes>
      </MemoryRouter>
    )

    // Should not show protected content
    await waitFor(() => {
      expect(screen.queryByTestId('app-sidebar')).not.toBeInTheDocument()
      expect(screen.queryByText('Dashboard Page')).not.toBeInTheDocument()
    }, { timeout: 3000 })
  })

  it('redirects /projects to /login when not authenticated', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json(
          { error: 'Unauthorized' },
          { status: 401 }
        )
      })
    )

    render(
      <MemoryRouter initialEntries={['/projects']}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route element={<MainLayout />}>
            <Route path="/projects" element={<MockProjects />} />
          </Route>
        </Routes>
      </MemoryRouter>
    )

    // Should not show protected content
    await waitFor(() => {
      expect(screen.queryByTestId('app-sidebar')).not.toBeInTheDocument()
      expect(screen.queryByText('Projects Page')).not.toBeInTheDocument()
    }, { timeout: 3000 })
  })

  it('redirects /profile to /login when not authenticated', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json(
          { error: 'Unauthorized' },
          { status: 401 }
        )
      })
    )

    render(
      <MemoryRouter initialEntries={['/profile']}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route element={<MainLayout />}>
            <Route path="/profile" element={<MockProfile />} />
          </Route>
        </Routes>
      </MemoryRouter>
    )

    // Should not show protected content
    await waitFor(() => {
      expect(screen.queryByTestId('app-sidebar')).not.toBeInTheDocument()
      expect(screen.queryByText('Profile Page')).not.toBeInTheDocument()
    }, { timeout: 3000 })
  })

  it('allows access to /dashboard when authenticated', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json({
          userId: 1,
          username: 'testuser',
          email: 'test@example.com',
          roles: ['Developer'],
        })
      })
    )

    render(
      <MemoryRouter initialEntries={['/dashboard']}>
        <Routes>
          <Route element={<MainLayout />}>
            <Route path="/dashboard" element={<MockDashboard />} />
          </Route>
        </Routes>
      </MemoryRouter>
    )

    // Should show protected content
    await waitFor(() => {
      expect(screen.getByTestId('app-sidebar')).toBeInTheDocument()
      expect(screen.getByTestId('header')).toBeInTheDocument()
      expect(screen.getByText('Dashboard Page')).toBeInTheDocument()
    }, { timeout: 3000 })
  })
})

