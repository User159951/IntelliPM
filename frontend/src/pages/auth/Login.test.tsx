import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@/test/test-utils'
import userEvent from '@testing-library/user-event'
import Login from './Login'
import { AuthProvider } from '@/contexts/AuthContext'

// Mock the auth API
vi.mock('@/api/auth', () => ({
  authApi: {
    login: vi.fn(),
    getMe: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    refresh: vi.fn(),
  },
}))

// Import mocked authApi after mock
import { authApi } from '@/api/auth'

const mockLogin = vi.mocked(authApi.login)
const mockGetMe = vi.mocked(authApi.getMe)

// Mock the toast hook
const mockToast = vi.fn()
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: mockToast,
  }),
}))

// Mock react-router-dom navigate
const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

describe('Login Page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockNavigate.mockClear()
    mockLogin.mockClear()
    mockGetMe.mockClear()
    mockToast.mockClear()
  })

  it('renders login form', () => {
    render(
      <AuthProvider>
        <Login />
      </AuthProvider>
    )

    expect(screen.getByLabelText(/username/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
  })

  it('validates required fields', async () => {
    render(
      <AuthProvider>
        <Login />
      </AuthProvider>
    )

    const submitButton = screen.getByRole('button', { name: /sign in/i })
    await userEvent.click(submitButton)

    // Check for HTML5 validation (browser shows validation message)
    const usernameInput = screen.getByLabelText(/username/i) as HTMLInputElement
    const passwordInput = screen.getByLabelText(/password/i) as HTMLInputElement

    expect(usernameInput.validity.valueMissing).toBe(true)
    expect(passwordInput.validity.valueMissing).toBe(true)
  })

  it('submits form with valid data and redirects non-admin users to /dashboard', async () => {
    const mockUser = {
      userId: 1,
      username: 'testuser',
      email: 'test@example.com',
      globalRole: 'User' as const,
      organizationId: 1,
      permissions: [],
    }

    // Mock getMe for initial fetch (AuthContext calls this on mount - should fail to show login form)
    mockGetMe.mockRejectedValueOnce(new Error('Not authenticated'))
    
    // Mock successful login - backend sets httpOnly cookies, returns user info
    mockLogin.mockResolvedValueOnce({
      userId: 1,
      username: 'testuser',
      email: 'test@example.com',
      message: 'Logged in successfully',
    })

    // Mock getMe after login (called by login() in AuthContext to get full user data)
    // Use mockResolvedValue to handle any subsequent calls
    mockGetMe.mockResolvedValue(mockUser)

    render(
      <AuthProvider>
        <Login />
      </AuthProvider>
    )

    // Wait for initial auth check to complete
    await waitFor(() => {
      expect(mockGetMe).toHaveBeenCalled()
    })

    // Fill in the form
    const usernameInput = screen.getByLabelText(/username/i) as HTMLInputElement
    const passwordInput = screen.getByLabelText(/password/i) as HTMLInputElement
    
    await userEvent.type(usernameInput, 'testuser')
    await userEvent.type(passwordInput, 'password123')
    
    // Verify form values (wait for React state to update)
    await waitFor(() => {
      expect(usernameInput.value).toBe('testuser')
    })
    await waitFor(() => {
      expect(passwordInput.value).toBe('password123')
    })

    // Submit the form
    const submitButton = screen.getByRole('button', { name: /sign in/i })
    await userEvent.click(submitButton)

    // Wait for login API to be called with correct credentials
    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        username: 'testuser',
        password: 'password123',
      })
    })

    // Wait for getMe to be called (AuthContext fetches user after successful login)
    // Note: getMe may be called multiple times (mount, login, etc.)
    await waitFor(() => {
      expect(mockGetMe).toHaveBeenCalled()
    }, { timeout: 3000 })

    // Wait for navigation to dashboard (non-admin users)
    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/dashboard')
    }, { timeout: 3000 })

    // Verify no error toast was shown
    expect(mockToast).not.toHaveBeenCalled()
  })

  it('redirects admin users to /admin after successful login', async () => {
    const mockAdminUser = {
      userId: 1,
      username: 'admin',
      email: 'admin@example.com',
      globalRole: 'Admin' as const,
      organizationId: 1,
      permissions: [],
    }

    // Mock getMe for initial fetch (AuthContext calls this on mount - should fail to show login form)
    mockGetMe.mockRejectedValueOnce(new Error('Not authenticated'))
    
    // Mock successful login - backend sets httpOnly cookies, returns user info
    mockLogin.mockResolvedValueOnce({
      userId: 1,
      username: 'admin',
      email: 'admin@example.com',
      message: 'Logged in successfully',
    })

    // Mock getMe after login (called by login() in AuthContext to get full user data)
    // Use mockResolvedValue to handle any subsequent calls
    mockGetMe.mockResolvedValue(mockAdminUser)

    render(
      <AuthProvider>
        <Login />
      </AuthProvider>
    )

    // Wait for initial auth check to complete
    await waitFor(() => {
      expect(mockGetMe).toHaveBeenCalled()
    })

    // Fill in the form
    const usernameInput = screen.getByLabelText(/username/i) as HTMLInputElement
    const passwordInput = screen.getByLabelText(/password/i) as HTMLInputElement
    
    await userEvent.type(usernameInput, 'admin')
    await userEvent.type(passwordInput, 'password123')
    
    // Submit the form
    const submitButton = screen.getByRole('button', { name: /sign in/i })
    await userEvent.click(submitButton)

    // Wait for login API to be called
    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        username: 'admin',
        password: 'password123',
      })
    })

    // Wait for getMe to be called (AuthContext fetches user after successful login)
    // Note: getMe may be called multiple times (mount, login, etc.)
    await waitFor(() => {
      expect(mockGetMe).toHaveBeenCalled()
    }, { timeout: 3000 })

    // Wait for navigation to /admin (admin users)
    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/admin')
    }, { timeout: 3000 })

    // Verify no error toast was shown
    expect(mockToast).not.toHaveBeenCalled()
  })

  it('displays error message on 401 Invalid credentials (bad credentials)', async () => {
    // Mock getMe for initial fetch (AuthContext calls this on mount)
    mockGetMe.mockRejectedValueOnce(new Error('Not authenticated'))
    
    // Mock login failure with Invalid credentials message (backend returns this)
    mockLogin.mockRejectedValueOnce(new Error('Invalid credentials'))

    render(
      <AuthProvider>
        <Login />
      </AuthProvider>
    )

    // Wait for initial auth check to complete
    await waitFor(() => {
      expect(mockGetMe).toHaveBeenCalled()
    })

    await userEvent.type(screen.getByLabelText(/username/i), 'wronguser')
    await userEvent.type(screen.getByLabelText(/password/i), 'wrongpassword')
    
    const submitButton = screen.getByRole('button', { name: /sign in/i })
    await userEvent.click(submitButton)

    // Wait for login API to be called with correct data
    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        username: 'wronguser',
        password: 'wrongpassword',
      })
    })

    // Wait for error toast to be shown with the error message
    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith({
        title: 'Login failed',
        description: 'Invalid credentials',
        variant: 'destructive',
      })
    }, { timeout: 3000 })

    // Should not navigate on error
    expect(mockNavigate).not.toHaveBeenCalled()
    
    // Form should still be visible (user can retry)
    expect(screen.getByLabelText(/username/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument()
  })

  it('handles network/server errors gracefully', async () => {
    // Mock getMe for initial fetch (AuthContext calls this on mount)
    mockGetMe.mockRejectedValueOnce(new Error('Not authenticated'))
    
    // Mock network error
    mockLogin.mockRejectedValueOnce(new Error('Network error: Failed to fetch'))

    render(
      <AuthProvider>
        <Login />
      </AuthProvider>
    )

    // Wait for initial auth check to complete
    await waitFor(() => {
      expect(mockGetMe).toHaveBeenCalled()
    })

    await userEvent.type(screen.getByLabelText(/username/i), 'testuser')
    await userEvent.type(screen.getByLabelText(/password/i), 'password123')
    await userEvent.click(screen.getByRole('button', { name: /sign in/i }))

    // Wait for login API to be called
    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalled()
    })

    // Wait for error toast with the network error message
    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith({
        title: 'Login failed',
        description: 'Network error: Failed to fetch',
        variant: 'destructive',
      })
    }, { timeout: 3000 })

    // Should not navigate on error
    expect(mockNavigate).not.toHaveBeenCalled()
  })

  it('toggles remember me checkbox', async () => {
    // Mock getMe for initial fetch (AuthContext calls this on mount)
    mockGetMe.mockRejectedValueOnce(new Error('Not authenticated'))

    render(
      <AuthProvider>
        <Login />
      </AuthProvider>
    )

    // Wait for initial auth check to complete
    await waitFor(() => {
      expect(mockGetMe).toHaveBeenCalled()
    })

    const rememberCheckbox = screen.getByLabelText(/remember me/i)
    expect(rememberCheckbox).not.toBeChecked()

    await userEvent.click(rememberCheckbox)
    
    // Wait for checkbox state to update
    await waitFor(() => {
      expect(rememberCheckbox).toBeChecked()
    })
  })
})

