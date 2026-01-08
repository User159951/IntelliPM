import React, { createContext, useContext, useCallback, useMemo } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { featureFlagsApi } from '@/api/featureFlags';
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
}

/**
 * React Query key for feature flags
 */
export const FEATURE_FLAGS_QUERY_KEY = 'feature-flags';

/**
 * FeatureFlagsProvider component that fetches and provides feature flags to all children.
 * 
 * Features:
 * - Fetches all flags on mount using React Query
 * - Provides flags via context for global access
 * - Handles loading and error states
 * - Supports organization-specific flags
 * - Uses React Query cache with stale time
 * - No hardcoded defaults - all flags come from API
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
}) => {
  const { user, isAuthenticated } = useAuth();
  const queryClient = useQueryClient();

  // Get organization ID from user
  const organizationId = useMemo(() => {
    return user?.organizationId?.toString();
  }, [user?.organizationId]);

  // Fetch feature flags using React Query
  const {
    data: flagsArray,
    isLoading,
    error: queryError,
    refetch,
  } = useQuery({
    queryKey: [FEATURE_FLAGS_QUERY_KEY, organizationId],
    queryFn: () => featureFlagsApi.getAllFlags(organizationId),
    enabled: isAuthenticated && !!user, // Only fetch when authenticated
    staleTime: 1000 * 60 * 5, // 5 minutes - flags are considered fresh for 5 minutes
    retry: 1, // Retry once on failure
    refetchOnWindowFocus: false, // Don't refetch on window focus
    refetchOnReconnect: true, // Refetch when network reconnects
  });

  // Convert flags array to record format
  const flags: FeatureFlagsRecord = useMemo(() => {
    if (!flagsArray) {
      return {}; // Return empty object if no flags loaded yet
    }
    return flagsToRecord(flagsArray);
  }, [flagsArray]);

  // Convert query error to Error object
  const error = useMemo(() => {
    if (!queryError) {
      return null;
    }
    if (queryError instanceof Error) {
      return queryError;
    }
    return new Error('Failed to fetch feature flags');
  }, [queryError]);

  /**
   * Refresh flags manually
   */
  const refresh = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: [FEATURE_FLAGS_QUERY_KEY] });
    await refetch();
  }, [queryClient, refetch]);

  /**
   * Check if a specific feature flag is enabled
   * Returns false if flag doesn't exist (no hardcoded defaults)
   */
  const isEnabled = useCallback(
    (flagName: string): boolean => {
      // No hardcoded defaults - flag must exist in API response
      return flags[flagName] === true;
    },
    [flags]
  );

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

