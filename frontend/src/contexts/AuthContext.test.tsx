import { describe, it, expect, beforeEach, vi } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { server } from '@/mocks/server';
import { AuthProvider, useAuth } from './AuthContext';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { ReactNode } from 'react';

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>{children}</AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
};

describe('AuthContext', () => {
  beforeEach(() => {
    server.resetHandlers();
    vi.clearAllMocks();
  });

  it('provides initial loading state', () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);
    expect(result.current.user).toBeNull();
    expect(result.current.isAuthenticated).toBe(false);
  });

  it('fetches user on mount', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json({
          userId: 1,
          username: 'testuser',
          email: 'test@example.com',
          firstName: 'Test',
          lastName: 'User',
          globalRole: 'User',
          organizationId: 1,
          permissions: [],
        });
      })
    );

    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.user).not.toBeNull();
    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.user?.globalRole).toBe('User');
    expect(result.current.user?.organizationId).toBe(1);
    expect(result.current.user?.permissions).toEqual([]);
  });

  it('handles unauthenticated state', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json(
          { error: 'Unauthorized' },
          { status: 401 }
        );
      })
    );

    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.user).toBeNull();
    expect(result.current.isAuthenticated).toBe(false);
  });

  it('logs in successfully and returns user', async () => {
    const mockUser = {
      userId: 1,
      username: 'testuser',
      email: 'test@example.com',
      firstName: 'Test',
      lastName: 'User',
      globalRole: 'User' as const,
      organizationId: 1,
      permissions: [] as string[],
    };

    // Mock initial getMe (on mount) - should fail to show unauthenticated state
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json(
          { error: 'Unauthorized' },
          { status: 401 }
        );
      })
    );

    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    // Mock login and getMe after login
    server.use(
      http.post('http://localhost:5001/api/v1/Auth/login', () => {
        return HttpResponse.json({
          userId: 1,
          username: 'testuser',
          email: 'test@example.com',
        });
      }),
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json(mockUser);
      })
    );

    let returnedUser: typeof mockUser | null = null;
    await act(async () => {
      returnedUser = await result.current.login({
        username: 'testuser',
        password: 'password123',
      }) as typeof mockUser | null;
    });

    await waitFor(() => {
      expect(result.current.isAuthenticated).toBe(true);
    });

    expect(result.current.user).not.toBeNull();
    expect(returnedUser).toEqual(mockUser);
    expect((returnedUser as unknown as { globalRole?: string })?.globalRole).toBe('User');
  });

  it('handles login failure', async () => {
    server.use(
      http.post('http://localhost:5001/api/v1/Auth/login', () => {
        return HttpResponse.json(
          { error: 'Invalid credentials' },
          { status: 401 }
        );
      })
    );

    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    await act(async () => {
      await expect(
        result.current.login({
          username: 'wrong',
          password: 'wrong',
        })
      ).rejects.toThrow();
    });
  });

  it('registers successfully', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json({
          userId: 2,
          username: 'newuser',
          email: 'newuser@example.com',
          firstName: 'New',
          lastName: 'User',
          globalRole: 'User',
          organizationId: 1,
          permissions: [],
        });
      }),
      http.post('http://localhost:5001/api/v1/Auth/register', () => {
        return HttpResponse.json({
          userId: 2,
          username: 'newuser',
          email: 'newuser@example.com',
        });
      })
    );

    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    await act(async () => {
      await result.current.register({
        username: 'newuser',
        email: 'newuser@example.com',
        password: 'password123',
        firstName: 'New',
        lastName: 'User',
      });
    });

    await waitFor(() => {
      expect(result.current.isAuthenticated).toBe(true);
    });
  });

  it('logs out successfully', async () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    await act(async () => {
      await result.current.logout();
    });

    await waitFor(() => {
      expect(result.current.isAuthenticated).toBe(false);
    });

    expect(result.current.user).toBeNull();
  });

  it('isAdmin is true when user has Admin role', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json({
          userId: 1,
          username: 'admin',
          email: 'admin@example.com',
          firstName: 'Admin',
          lastName: 'User',
          globalRole: 'Admin',
          organizationId: 1,
          permissions: [],
        });
      })
    );

    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.user).not.toBeNull();
    expect(result.current.user?.globalRole).toBe('Admin');
    expect(result.current.isAdmin).toBe(true);
  });

  it('isAdmin is false when user has User role', async () => {
    server.use(
      http.get('http://localhost:5001/api/v1/Auth/me', () => {
        return HttpResponse.json({
          userId: 1,
          username: 'testuser',
          email: 'test@example.com',
          firstName: 'Test',
          lastName: 'User',
          globalRole: 'User',
          organizationId: 1,
          permissions: [],
        });
      })
    );

    const { result } = renderHook(() => useAuth(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.user).not.toBeNull();
    expect(result.current.user?.globalRole).toBe('User');
    expect(result.current.isAdmin).toBe(false);
  });
});

