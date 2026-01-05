import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import * as Sentry from '@sentry/react';
import { authApi } from '@/api/auth';
import type { User, LoginRequest, RegisterRequest, GlobalRole } from '@/types';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isAdmin: boolean;
  isSuperAdmin: boolean;
  getGlobalRole: () => GlobalRole | null;
  login: (data: LoginRequest) => Promise<User>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Event to notify auth context when authentication fails
const AUTH_FAILED_EVENT = 'auth:failed';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const fetchUser = useCallback(async () => {
    try {
      // Call /api/auth/me endpoint to verify cookie
      const userData = await authApi.getMe();
      setUser(userData);
    } catch {
      // Cookie invalid or expired, user is not authenticated
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchUser();
  }, [fetchUser]);

  // Listen for authentication failure events from API client
  useEffect(() => {
    const handleAuthFailed = () => {
      setUser(null);
    };

    window.addEventListener(AUTH_FAILED_EVENT, handleAuthFailed);
    return () => {
      window.removeEventListener(AUTH_FAILED_EVENT, handleAuthFailed);
    };
  }, []);

  // Set Sentry user context when user is loaded
  useEffect(() => {
    if (user && import.meta.env.VITE_SENTRY_DSN) {
      Sentry.setUser({
        id: user.userId.toString(),
        email: user.email,
        username: user.username,
      });
    } else if (!user && import.meta.env.VITE_SENTRY_DSN) {
      // Clear user context on logout
      Sentry.setUser(null);
    }
  }, [user]);

  const login = async (data: LoginRequest): Promise<User> => {
    // Tokens are now stored in httpOnly cookies by the backend
    // No need to manually store them
    await authApi.login(data);
    // Fetch full user data to get complete user object (includes globalRole, organizationId, permissions)
    const userData = await authApi.getMe();
    setUser(userData);
    // Return the user object for role-based redirection
    return userData;
  };

  const register = async (data: RegisterRequest) => {
    // Tokens are now stored in httpOnly cookies by the backend
    await authApi.register(data);
    // Response contains user info, tokens are in cookies
    // Fetch full user data to get complete user object
    await fetchUser();
  };

  const logout = async () => {
    try {
      await authApi.logout();
      // Backend will delete cookies
    } catch {
      // Ignore errors on logout
    }
    setUser(null);
  };

  const isAdmin = user?.globalRole === 'Admin' || user?.globalRole === 'SuperAdmin';
  const isSuperAdmin = user?.globalRole === 'SuperAdmin';
  const getGlobalRole = (): GlobalRole | null => {
    return user?.globalRole || null;
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        isAdmin,
        isSuperAdmin,
        getGlobalRole,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
