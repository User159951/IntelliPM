import { render, RenderOptions } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactElement, ReactNode } from 'react'
import { AuthProvider } from '@/contexts/AuthContext'
import { ThemeProvider } from '@/contexts/ThemeContext'

// Create a test query client with default options
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

interface AllProvidersProps {
  children: ReactNode
  queryClient?: QueryClient
  initialAuthState?: {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    user?: any
    isAuthenticated?: boolean
  }
}

// eslint-disable-next-line react-refresh/only-export-components
const AllProviders = ({ 
  children, 
  queryClient = createTestQueryClient()
}: AllProvidersProps) => {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <ThemeProvider>
          <AuthProvider>
            {children}
          </AuthProvider>
        </ThemeProvider>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  queryClient?: QueryClient
  initialAuthState?: {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    user?: any
    isAuthenticated?: boolean
  }
}

const customRender = (
  ui: ReactElement,
  options?: CustomRenderOptions
) => {
  const { queryClient, initialAuthState, ...renderOptions } = options || {}
  
  return render(ui, {
    wrapper: ({ children }) => (
      <AllProviders queryClient={queryClient} initialAuthState={initialAuthState}>
        {children}
      </AllProviders>
    ),
    ...renderOptions,
  })
}

// Helper to create authenticated test query client
export const createAuthenticatedQueryClient = () => createTestQueryClient()

// Helper for authenticated tests
export const renderWithAuth = (
  ui: ReactElement,
  options?: CustomRenderOptions
) => {
  return customRender(ui, {
    ...options,
    initialAuthState: {
      isAuthenticated: true,
      user: { userId: 1, username: 'testuser', email: 'test@example.com' },
    },
  })
}

// eslint-disable-next-line react-refresh/only-export-components
export * from '@testing-library/react'
export { customRender as render }

