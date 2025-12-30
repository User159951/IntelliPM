import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@/test/test-utils'
import userEvent from '@testing-library/user-event'
import { Header } from './Header'

// Mock the auth context
const mockLogout = vi.fn()
const mockUser = {
  userId: 1,
  username: 'testuser',
  email: 'test@example.com',
  firstName: 'Test',
  lastName: 'User',
  roles: ['Developer'],
}

vi.mock('@/contexts/AuthContext', async () => {
  const actual = await vi.importActual('@/contexts/AuthContext')
  return {
    ...actual,
    useAuth: () => ({
      user: mockUser,
      logout: mockLogout,
    }),
  }
})

// Mock the theme context
vi.mock('@/contexts/ThemeContext', async () => {
  const actual = await vi.importActual('@/contexts/ThemeContext')
  return {
    ...actual,
    useTheme: () => ({
      theme: 'light',
      toggleTheme: vi.fn(),
    }),
  }
})

// Mock NotificationDropdown
vi.mock('@/components/notifications/NotificationDropdown', () => ({
  NotificationDropdown: () => <div data-testid="notifications">Notifications</div>,
}))

// Mock SidebarTrigger (requires SidebarProvider)
vi.mock('@/components/ui/sidebar', async () => {
  const actual = await vi.importActual('@/components/ui/sidebar')
  return {
    ...actual,
    SidebarTrigger: () => <button data-testid="sidebar-trigger">Menu</button>,
  }
})

// Mock useNavigate
const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

describe('Header', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockNavigate.mockClear()
    mockLogout.mockClear()
  })

  it('renders header with user info', () => {
    render(<Header />)

    // Should show user avatar with initials
    expect(screen.getByText('TU')).toBeInTheDocument()
  })

  it('navigates to profile when Profile is clicked', async () => {
    render(<Header />)

    // Open dropdown menu - find button containing the avatar (by text content)
    const avatarButtons = screen.getAllByRole('button')
    const avatarButton = avatarButtons.find(btn => btn.querySelector('.h-8.w-8.rounded-full'))
    expect(avatarButton).toBeDefined()
    await userEvent.click(avatarButton!)

    // Click Profile
    const profileItem = screen.getByRole('menuitem', { name: /profile/i })
    await userEvent.click(profileItem)

    // Should navigate to /profile
    expect(mockNavigate).toHaveBeenCalledWith('/profile')
  })

  it('calls logout and redirects to /login when Log out is clicked', async () => {
    // Mock logout as async
    mockLogout.mockResolvedValue(undefined)

    render(<Header />)

    // Open dropdown menu - find button containing the avatar
    const avatarButtons = screen.getAllByRole('button')
    const avatarButton = avatarButtons.find(btn => btn.querySelector('.h-8.w-8.rounded-full'))
    expect(avatarButton).toBeDefined()
    await userEvent.click(avatarButton!)

    // Click Log out
    const logoutItem = screen.getByRole('menuitem', { name: /log out/i })
    await userEvent.click(logoutItem)

    // Should call logout
    await waitFor(() => {
      expect(mockLogout).toHaveBeenCalled()
    })

    // Should navigate to /login after logout
    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/login')
    }, { timeout: 2000 })
  })

  it('displays user name and email in dropdown', async () => {
    render(<Header />)

    // Open dropdown menu - find button containing the avatar
    const avatarButtons = screen.getAllByRole('button')
    const avatarButton = avatarButtons.find(btn => btn.querySelector('.h-8.w-8.rounded-full'))
    expect(avatarButton).toBeDefined()
    await userEvent.click(avatarButton!)

    // Should show user name and email
    expect(screen.getByText('Test User')).toBeInTheDocument()
    expect(screen.getByText('test@example.com')).toBeInTheDocument()
  })
})

