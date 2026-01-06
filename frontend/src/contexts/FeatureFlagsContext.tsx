import React, { createContext, useContext, useState, useEffect, useCallback, useRef } from 'react';
import { featureFlagService } from '@/services/featureFlagService';
import { useAuth } from './AuthContext';
import type { FeatureFlagsContextType, FeatureFlagsRecord } from '@/types/featureFlags';
import { flagsToRecord } from '@/utils/featureFlags';

/**
 * Feature flags context
 */
const FeatureFlagsContext = createContext<FeatureFlagsContextType | undefined>(undefined);

/**
 * Feature flags provider props
 */
interface FeatureFlagsProviderProps {
  /**
   * Children components
   */
  children: React.ReactNode;

  /**
   * Cache TTL in milliseconds (default: 5 minutes)
   * Can be overridden via VITE_FEATURE_FLAGS_CACHE_TTL environment variable
   */
  cacheTtl?: number;

  /**
   * Auto-refresh interval in milliseconds (default: 5 minutes)
   * Set to 0 to disable auto-refresh
   */
  autoRefreshInterval?: number;
}

/**
 * FeatureFlagsProvider component that fetches and provides feature flags to all children.
 * 
 * Features:
 * - Fetches all flags on mount
 * - Auto-refreshes flags at specified interval
 * - Provides flags via context for global access
 * - Handles loading and error states
 * - Supports organization-specific flags
 * 
 * @example
 * ```tsx
 * <FeatureFlagsProvider>
 *   <App />
 * </FeatureFlagsProvider>
 * ```
 */
export const FeatureFlagsProvider: React.FC<FeatureFlagsProviderProps> = ({
  children,
  cacheTtl: _cacheTtl,
  autoRefreshInterval,
}) => {
  const { user, isAuthenticated } = useAuth();
  const [flags, setFlags] = useState<FeatureFlagsRecord>({});
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);
  const refreshIntervalRef = useRef<NodeJS.Timeout | null>(null);
  const isMountedRef = useRef(true);

  // Get auto-refresh interval from prop (default: 5 minutes)
  // Note: cacheTtl prop is available for future use but not currently implemented
  const refreshIntervalMs = autoRefreshInterval ?? 5 * 60 * 1000;

  /**
   * Fetch all feature flags for the current organization
   */
  const fetchFlags = useCallback(async () => {
    if (!isAuthenticated || !user) {
      setFlags({});
      setIsLoading(false);
      return;
    }

    try {
      setIsLoading(true);
      setError(null);

      // Get organization ID from user
      const organizationId = user.organizationId?.toString();

      // Fetch all flags
      const allFlags = await featureFlagService.getAllFlags(organizationId);

      // Convert to record format
      const flagsRecord = flagsToRecord(allFlags);

      if (isMountedRef.current) {
        setFlags(flagsRecord);
        setIsLoading(false);
      }
    } catch (err) {
      if (isMountedRef.current) {
        const error = err instanceof Error ? err : new Error('Failed to fetch feature flags');
        
        // Don't set error state for 401 errors - API client will handle token refresh or redirect
        // If token refresh fails, auth context will be updated and isAuthenticated will become false
        if (error.message.includes('Unauthorized') || error.message.includes('401')) {
          setIsLoading(false);
          // Don't set error - let the API client handle authentication
          // The query will be disabled automatically when isAuthenticated becomes false
          return;
        }
        
        setError(error);
        setIsLoading(false);

        // Log error in development
        if (import.meta.env.DEV) {
          console.error('[FeatureFlagsProvider] Error fetching flags:', error);
        }
      }
    }
  }, [user, isAuthenticated]);

  /**
   * Refresh flags manually
   */
  const refresh = useCallback(async () => {
    // Clear service cache before refreshing
    featureFlagService.clearCache();
    await fetchFlags();
  }, [fetchFlags]);

  /**
   * Check if a specific feature flag is enabled
   */
  const isEnabled = useCallback(
    (flagName: string): boolean => {
      return flags[flagName] === true;
    },
    [flags]
  );

  // Fetch flags on mount and when user/organization changes
  useEffect(() => {
    fetchFlags();
  }, [fetchFlags]);

  // Set up auto-refresh interval
  useEffect(() => {
    if (refreshIntervalMs > 0 && isAuthenticated) {
      refreshIntervalRef.current = setInterval(() => {
        fetchFlags();
      }, refreshIntervalMs);

      return () => {
        if (refreshIntervalRef.current) {
          clearInterval(refreshIntervalRef.current);
        }
      };
    }
    return undefined;
  }, [refreshIntervalMs, isAuthenticated, fetchFlags]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      isMountedRef.current = false;
      if (refreshIntervalRef.current) {
        clearInterval(refreshIntervalRef.current);
      }
    };
  }, []);

  const contextValue: FeatureFlagsContextType = {
    flags,
    isLoading,
    error,
    refresh,
    isEnabled,
  };

  return (
    <FeatureFlagsContext.Provider value={contextValue}>
      {children}
    </FeatureFlagsContext.Provider>
  );
};

/**
 * Hook to access feature flags context
 * 
 * @returns FeatureFlagsContextType
 * @throws Error if used outside FeatureFlagsProvider
 * 
 * @example
 * ```tsx
 * const { flags, isLoading, isEnabled } = useFeatureFlags();
 * 
 * if (isEnabled('EnableAIInsights')) {
 *   // Show AI features
 * }
 * ```
 */
// eslint-disable-next-line react-refresh/only-export-components
export const useFeatureFlags = (): FeatureFlagsContextType => {
  const context = useContext(FeatureFlagsContext);
  if (context === undefined) {
    throw new Error('useFeatureFlags must be used within a FeatureFlagsProvider');
  }
  return context;
};

