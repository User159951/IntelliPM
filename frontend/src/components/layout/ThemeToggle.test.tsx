import { describe, it, expect, beforeEach, vi } from 'vitest'
import { render, screen, waitFor } from '@/test/test-utils'
import userEvent from '@testing-library/user-event'
import { Header } from './Header'
import { ThemeProvider } from '@/contexts/ThemeContext'

// Mock NotificationDropdown and other components
vi.mock('@/components/notifications/NotificationDropdown', () => ({
  NotificationDropdown: () => <div data-testid="notifications">Notifications</div>,
}))

vi.mock('@/components/ui/sidebar', async () => {
  const actual = await vi.importActual('@/components/ui/sidebar')
  return {
    ...actual,
    SidebarTrigger: () => <button data-testid="sidebar-trigger">Menu</button>,
  }
})

// Mock useAuth
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
      logout: vi.fn(),
    }),
  }
})

// Mock useNavigate
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => vi.fn(),
  }
})

describe('Theme Toggle', () => {
  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear()
    // Reset document classes
    document.documentElement.classList.remove('light', 'dark')
    vi.clearAllMocks()
  })

  it('renders theme toggle button', () => {
    render(
      <ThemeProvider>
        <Header />
      </ThemeProvider>
    )

    // Find toggle button by finding button with moon or sun icon
    const buttons = screen.getAllByRole('button')
    const toggleButton = buttons.find(btn => {
      const icon = btn.querySelector('svg')
      return icon && (icon.classList.contains('lucide-moon') || icon.classList.contains('lucide-sun'))
    })
    
    expect(toggleButton).toBeDefined()
  })

  it('toggles theme from light to dark', async () => {
    // Set initial theme to light
    localStorage.setItem('theme', 'light')
    document.documentElement.classList.remove('dark')
    document.documentElement.classList.add('light')

    render(
      <ThemeProvider>
        <Header />
      </ThemeProvider>
    )

    // Find toggle button by finding button with moon or sun icon
    const buttons = screen.getAllByRole('button')
    const toggleButton = buttons.find(btn => {
      const icon = btn.querySelector('svg')
      return icon && (icon.classList.contains('lucide-moon') || icon.classList.contains('lucide-sun'))
    })
    
    expect(toggleButton).toBeDefined()
    if (toggleButton) {
      await userEvent.click(toggleButton)

      // Wait for theme to update
      await waitFor(() => {
        expect(document.documentElement.classList.contains('dark')).toBe(true)
        expect(document.documentElement.classList.contains('light')).toBe(false)
      })

      // Check localStorage
      expect(localStorage.getItem('theme')).toBe('dark')
    }
  })

  it('toggles theme from dark to light', async () => {
    // Set initial theme to dark
    localStorage.setItem('theme', 'dark')
    document.documentElement.classList.remove('light')
    document.documentElement.classList.add('dark')

    render(
      <ThemeProvider>
        <Header />
      </ThemeProvider>
    )

    // Find toggle button by finding button with moon or sun icon
    const buttons = screen.getAllByRole('button')
    const toggleButton = buttons.find(btn => {
      const icon = btn.querySelector('svg')
      return icon && (icon.classList.contains('lucide-moon') || icon.classList.contains('lucide-sun'))
    })
    
    expect(toggleButton).toBeDefined()
    if (toggleButton) {
      await userEvent.click(toggleButton)

      // Wait for theme to update
      await waitFor(() => {
        expect(document.documentElement.classList.contains('light')).toBe(true)
        expect(document.documentElement.classList.contains('dark')).toBe(false)
      })

      // Check localStorage
      expect(localStorage.getItem('theme')).toBe('light')
    }
  })

  it('persists theme preference in localStorage', async () => {
    render(
      <ThemeProvider>
        <Header />
      </ThemeProvider>
    )

    const buttons = screen.getAllByRole('button')
    const toggleButton = buttons.find(btn => {
      const icon = btn.querySelector('svg')
      return icon && (icon.classList.contains('lucide-moon') || icon.classList.contains('lucide-sun'))
    })
    
    if (toggleButton) {
      await userEvent.click(toggleButton)

      await waitFor(() => {
        const theme = localStorage.getItem('theme')
        expect(theme).toBeTruthy()
        expect(theme === 'light' || theme === 'dark').toBe(true)
      })
    }
  })

  it('applies theme class to document element on mount', async () => {
    localStorage.setItem('theme', 'dark')

    render(
      <ThemeProvider>
        <Header />
      </ThemeProvider>
    )

    // Theme should be applied immediately
    await waitFor(() => {
      expect(document.documentElement.classList.contains('dark')).toBe(true)
    })
  })
})

