import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@/test/test-utils'
import { fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Register from './Register'
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

const mockRegister = vi.mocked(authApi.register)
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

describe('Register Page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockNavigate.mockClear()
    mockRegister.mockClear()
    mockGetMe.mockClear()
    mockToast.mockClear()
  })

  it('renders register form', () => {
    render(
      <AuthProvider>
        <Register />
      </AuthProvider>
    )

    expect(screen.getByLabelText(/first name/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/last name/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/username/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/confirm password/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /create account/i })).toBeInTheDocument()
  })

  it('validates required fields', async () => {
    render(
      <AuthProvider>
        <Register />
      </AuthProvider>
    )

    // Accept terms first so the button becomes enabled (required for form submission)
    const termsCheckbox = screen.getByLabelText(/i agree/i)
    await userEvent.click(termsCheckbox)

    // Now the button should be enabled
    const submitButton = screen.getByRole('button', { name: /create account/i })
    expect(submitButton).not.toBeDisabled()

    // Submit the form
    await userEvent.click(submitButton)

    // Check that validation errors appear
    await waitFor(() => {
      expect(screen.getByText(/first name is required/i)).toBeInTheDocument()
      expect(screen.getByText(/last name is required/i)).toBeInTheDocument()
      expect(screen.getByText(/email is required/i)).toBeInTheDocument()
      expect(screen.getByText(/username is required/i)).toBeInTheDocument()
      expect(screen.getByText(/password is required/i)).toBeInTheDocument()
    })

    // Form should not be submitted
    expect(mockRegister).not.toHaveBeenCalled()
  })

  it('validates email format', async () => {
    render(
      <AuthProvider>
        <Register />
      </AuthProvider>
    )

    // Fill other required fields so validation reaches email format check
    const firstNameInput = screen.getByLabelText(/first name/i)
    const lastNameInput = screen.getByLabelText(/last name/i)
    const emailInput = screen.getByLabelText(/email/i)
    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/^password$/i)
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i)
    const termsCheckbox = screen.getByLabelText(/i agree/i)
    
    await userEvent.type(firstNameInput, 'Test')
    await userEvent.type(lastNameInput, 'User')
    // Use clear + type to ensure the value is set correctly
    await userEvent.clear(emailInput)
    await userEvent.type(emailInput, 'invalid-email')
    await userEvent.type(usernameInput, 'testuser')
    await userEvent.type(passwordInput, 'Password123!')
    await userEvent.type(confirmPasswordInput, 'Password123!')
    await userEvent.click(termsCheckbox)

    // Get the form element
    const form = screen.getByRole('button', { name: /create account/i }).closest('form')
    expect(form).toBeInTheDocument()

    // Submit the form directly to bypass HTML5 validation that might block submit
    fireEvent.submit(form!)

    // Wait for validation error to appear
    await waitFor(() => {
      expect(screen.getByText(/invalid email format/i)).toBeInTheDocument()
    }, { timeout: 3000 })
  })

  it('validates username length', async () => {
    render(
      <AuthProvider>
        <Register />
      </AuthProvider>
    )

    // Fill other required fields so validation reaches username length check
    const firstNameInput = screen.getByLabelText(/first name/i)
    const lastNameInput = screen.getByLabelText(/last name/i)
    const emailInput = screen.getByLabelText(/email/i)
    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/^password$/i)
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i)
    const termsCheckbox = screen.getByLabelText(/i agree/i)
    
    await userEvent.type(firstNameInput, 'Test')
    await userEvent.type(lastNameInput, 'User')
    await userEvent.type(emailInput, 'test@example.com')
    await userEvent.type(usernameInput, 'ab') // Too short
    await userEvent.type(passwordInput, 'Password123!')
    await userEvent.type(confirmPasswordInput, 'Password123!')
    await userEvent.click(termsCheckbox)

    await userEvent.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => {
      expect(screen.getByText(/username must be at least 3 characters/i)).toBeInTheDocument()
    })
  })

  it('validates password requirements', async () => {
    render(
      <AuthProvider>
        <Register />
      </AuthProvider>
    )

    // Fill required fields so validation reaches password length check
    const firstNameInput = screen.getByLabelText(/first name/i)
    const lastNameInput = screen.getByLabelText(/last name/i)
    const emailInput = screen.getByLabelText(/email/i)
    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/^password$/i)
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i)
    const termsCheckbox = screen.getByLabelText(/i agree/i)
    
    await userEvent.type(firstNameInput, 'Test')
    await userEvent.type(lastNameInput, 'User')
    await userEvent.type(emailInput, 'test@example.com')
    await userEvent.type(usernameInput, 'testuser')
    await userEvent.type(passwordInput, 'weak') // Too short
    await userEvent.type(confirmPasswordInput, 'weak')
    await userEvent.click(termsCheckbox)

    await userEvent.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => {
      expect(screen.getByText(/password must be at least 8 characters/i)).toBeInTheDocument()
    })
  })

  it('validates password criteria (uppercase, lowercase, number, special char)', async () => {
    render(
      <AuthProvider>
        <Register />
      </AuthProvider>
    )

    // Fill required fields so validation reaches password criteria check
    const firstNameInput = screen.getByLabelText(/first name/i)
    const lastNameInput = screen.getByLabelText(/last name/i)
    const emailInput = screen.getByLabelText(/email/i)
    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/^password$/i)
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i)
    const termsCheckbox = screen.getByLabelText(/i agree/i)
    
    await userEvent.type(firstNameInput, 'Test')
    await userEvent.type(lastNameInput, 'User')
    await userEvent.type(emailInput, 'test@example.com')
    await userEvent.type(usernameInput, 'testuser')
    // Password without uppercase
    await userEvent.type(passwordInput, 'nouppercase123!')
    await userEvent.type(confirmPasswordInput, 'nouppercase123!')
    await userEvent.click(termsCheckbox)

    await userEvent.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => {
      expect(screen.getByText(/password must contain at least one uppercase letter/i)).toBeInTheDocument()
    })
  })

  it('validates password confirmation match', async () => {
    render(
      <AuthProvider>
        <Register />
      </AuthProvider>
    )

    // Fill required fields so validation reaches password confirmation check
    const firstNameInput = screen.getByLabelText(/first name/i)
    const lastNameInput = screen.getByLabelText(/last name/i)
    const emailInput = screen.getByLabelText(/email/i)
    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/^password$/i)
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i)
    const termsCheckbox = screen.getByLabelText(/i agree/i)
    
    await userEvent.type(firstNameInput, 'Test')
    await userEvent.type(lastNameInput, 'User')
    await userEvent.type(emailInput, 'test@example.com')
    await userEvent.type(usernameInput, 'testuser')
    await userEvent.type(passwordInput, 'Password123!')
    await userEvent.type(confirmPasswordInput, 'Password456!')
    await userEvent.click(termsCheckbox)

    await userEvent.click(screen.getByRole('button', { name: /create account/i }))

    await waitFor(() => {
      expect(screen.getByText(/passwords do not match/i)).toBeInTheDocument()
    })
  })

  it('submits form with valid data and redirects on success (happy path E2E)', async () => {
    const mockUser = {
      userId: 1,
      username: 'testuser',
      email: 'test@example.com',
      roles: ['Developer'],
      firstName: 'Test',
      lastName: 'User',
      globalRole: 'User' as const,
      organizationId: 1,
      permissions: [],
    }

    // Mock getMe for initial fetch (AuthContext calls this on mount - should fail to show register form)
    mockGetMe.mockRejectedValueOnce(new Error('Not authenticated'))
    
    // Mock successful register - backend sets httpOnly cookies, returns user info
    mockRegister.mockResolvedValueOnce({
      userId: 1,
      username: 'testuser',
      email: 'test@example.com',
      message: 'Registered successfully',
    })

    // Mock getMe after register (called by fetchUser in AuthContext to verify cookie)
    mockGetMe.mockResolvedValueOnce(mockUser)

    render(
      <AuthProvider>
        <Register />
      </AuthProvider>
    )

    // Wait for initial auth check to complete
    await waitFor(() => {
      expect(mockGetMe).toHaveBeenCalled()
    })

    // Fill in the form
    const firstNameInput = screen.getByLabelText(/first name/i) as HTMLInputElement
    const lastNameInput = screen.getByLabelText(/last name/i) as HTMLInputElement
    const emailInput = screen.getByLabelText(/email/i) as HTMLInputElement
    const usernameInput = screen.getByLabelText(/username/i) as HTMLInputElement
    const passwordInput = screen.getByLabelText(/^password$/i) as HTMLInputElement
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i) as HTMLInputElement
    const termsCheckbox = screen.getByLabelText(/i agree/i)
    
    await userEvent.type(firstNameInput, 'Test')
    await userEvent.type(lastNameInput, 'User')
    await userEvent.type(emailInput, 'test@example.com')
    await userEvent.type(usernameInput, 'testuser')
    await userEvent.type(passwordInput, 'Password123!')
    await userEvent.type(confirmPasswordInput, 'Password123!')
    await userEvent.click(termsCheckbox)

    // Verify form values (wait for React state to update)
    await waitFor(() => {
      expect(firstNameInput.value).toBe('Test')
      expect(lastNameInput.value).toBe('User')
      expect(emailInput.value).toBe('test@example.com')
      expect(usernameInput.value).toBe('testuser')
      expect(passwordInput.value).toBe('Password123!')
      expect(confirmPasswordInput.value).toBe('Password123!')
    })

    // Submit the form
    const submitButton = screen.getByRole('button', { name: /create account/i })
    await userEvent.click(submitButton)

    // Wait for register API to be called with correct data
    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalledWith({
        firstName: 'Test',
        lastName: 'User',
        email: 'test@example.com',
        username: 'testuser',
        password: 'Password123!',
      })
    })

    // Wait for getMe to be called (AuthContext fetches user after successful register)
    // Note: getMe might be called multiple times due to React strict mode or state updates
    await waitFor(() => {
      expect(mockGetMe).toHaveBeenCalled() // At least once on mount, and once after register
      // Verify it was called at least 2 times (mount + after register)
      expect(mockGetMe.mock.calls.length).toBeGreaterThanOrEqual(2)
    }, { timeout: 3000 })

    // Wait for success toast
    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith({
        title: 'Account created',
        description: 'Welcome to IntelliPM!',
      })
    })

    // Wait for navigation to dashboard (successful register flow - auto-login)
    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/dashboard')
    }, { timeout: 3000 })
  })

  it('displays error message on registration failure', async () => {
    // Mock getMe for initial fetch
    mockGetMe.mockRejectedValueOnce(new Error('Not authenticated'))
    
    // Mock register failure (e.g., email already exists)
    mockRegister.mockRejectedValueOnce(new Error('Email already exists'))

    render(
      <AuthProvider>
        <Register />
      </AuthProvider>
    )

    // Wait for initial auth check to complete
    await waitFor(() => {
      expect(mockGetMe).toHaveBeenCalled()
    })

    // Fill in the form
    const firstNameInput = screen.getByLabelText(/first name/i)
    const lastNameInput = screen.getByLabelText(/last name/i)
    const emailInput = screen.getByLabelText(/email/i)
    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/^password$/i)
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i)
    const termsCheckbox = screen.getByLabelText(/i agree/i)
    
    await userEvent.type(firstNameInput, 'Test')
    await userEvent.type(lastNameInput, 'User')
    await userEvent.type(emailInput, 'existing@example.com')
    await userEvent.type(usernameInput, 'testuser')
    await userEvent.type(passwordInput, 'Password123!')
    await userEvent.type(confirmPasswordInput, 'Password123!')
    await userEvent.click(termsCheckbox)

    // Submit the form
    const submitButton = screen.getByRole('button', { name: /create account/i })
    await userEvent.click(submitButton)

    // Wait for register API to be called
    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalled()
    })

    // Wait for error toast
    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith({
        title: 'Registration failed',
        description: 'Email already exists',
        variant: 'destructive',
      })
    }, { timeout: 3000 })

    // Should not navigate on error
    expect(mockNavigate).not.toHaveBeenCalled()
    
    // Form should still be visible (user can retry)
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/username/i)).toBeInTheDocument()
  })

  it('handles network/server errors gracefully', async () => {
    // Mock getMe for initial fetch
    mockGetMe.mockRejectedValueOnce(new Error('Not authenticated'))
    
    // Mock network error
    mockRegister.mockRejectedValueOnce(new Error('Network error: Failed to fetch'))

    render(
      <AuthProvider>
        <Register />
      </AuthProvider>
    )

    // Wait for initial auth check to complete
    await waitFor(() => {
      expect(mockGetMe).toHaveBeenCalled()
    })

    // Fill in the form
    const firstNameInput = screen.getByLabelText(/first name/i)
    const lastNameInput = screen.getByLabelText(/last name/i)
    const emailInput = screen.getByLabelText(/email/i)
    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/^password$/i)
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i)
    const termsCheckbox = screen.getByLabelText(/i agree/i)
    
    await userEvent.type(firstNameInput, 'Test')
    await userEvent.type(lastNameInput, 'User')
    await userEvent.type(emailInput, 'test@example.com')
    await userEvent.type(usernameInput, 'testuser')
    await userEvent.type(passwordInput, 'Password123!')
    await userEvent.type(confirmPasswordInput, 'Password123!')
    await userEvent.click(termsCheckbox)

    // Submit the form
    const submitButton = screen.getByRole('button', { name: /create account/i })
    await userEvent.click(submitButton)

    // Wait for register API to be called
    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalled()
    })

    // Wait for error toast
    await waitFor(() => {
      expect(mockToast).toHaveBeenCalledWith({
        title: 'Registration failed',
        description: 'Network error: Failed to fetch',
        variant: 'destructive',
      })
    }, { timeout: 3000 })

    // Should not navigate on error
    expect(mockNavigate).not.toHaveBeenCalled()
  })

  it('requires terms acceptance before submission', async () => {
    render(
      <AuthProvider>
        <Register />
      </AuthProvider>
    )

    // Fill in the form but don't accept terms
    const firstNameInput = screen.getByLabelText(/first name/i)
    const lastNameInput = screen.getByLabelText(/last name/i)
    const emailInput = screen.getByLabelText(/email/i)
    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/^password$/i)
    const confirmPasswordInput = screen.getByLabelText(/confirm password/i)
    
    await userEvent.type(firstNameInput, 'Test')
    await userEvent.type(lastNameInput, 'User')
    await userEvent.type(emailInput, 'test@example.com')
    await userEvent.type(usernameInput, 'testuser')
    await userEvent.type(passwordInput, 'Password123!')
    await userEvent.type(confirmPasswordInput, 'Password123!')

    // Submit button should be disabled
    const submitButton = screen.getByRole('button', { name: /create account/i })
    expect(submitButton).toBeDisabled()

    // Accept terms
    const termsCheckbox = screen.getByLabelText(/i agree/i)
    await userEvent.click(termsCheckbox)

    // Submit button should now be enabled
    await waitFor(() => {
      expect(submitButton).not.toBeDisabled()
    })
  })
})

