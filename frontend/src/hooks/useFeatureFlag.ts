import { useMemo, useState, useEffect } from 'react';
import { useFeatureFlags } from '@/contexts/FeatureFlagsContext';
import { featureFlagService } from '@/services/featureFlagService';
import { checkFeatureFlag } from '@/utils/featureFlags';
import type { FeatureFlagName } from '@/types/featureFlags';

/**
 * Hook return type for useFeatureFlag
 */
export interface UseFeatureFlagReturn {
  /**
   * Whether the feature flag is enabled
   */
  isEnabled: boolean;

  /**
   * Loading state - true when checking the flag
   */
  isLoading: boolean;

  /**
   * Error state - Error object if any error occurred, null otherwise
   */
  error: Error | null;
}

/**
 * Hook to check if a specific feature flag is enabled.
 * 
 * This hook:
 * 1. First checks the FeatureFlagsContext for cached flags
 * 2. If not in context, falls back to fetching from featureFlagService
 * 3. Returns the flag value, loading state, and error
 * 
 * @param flagName - Name of the feature flag to check (string or FeatureFlagName enum)
 * @param organizationId - Optional organization ID for organization-specific flags
 * @returns UseFeatureFlagReturn with isEnabled, isLoading, and error
 * 
 * @example
 * ```tsx
 * // Using string
 * const { isEnabled, isLoading } = useFeatureFlag('EnableAIInsights');
 * 
 * // Using enum
 * const { isEnabled } = useFeatureFlag(FeatureFlagName.EnableAIInsights);
 * 
 * if (isEnabled) {
 *   return <AIInsightsPanel />;
 * }
 * ```
 */
export function useFeatureFlag(
  flagName: string | FeatureFlagName,
  organizationId?: string
): UseFeatureFlagReturn {
  const { flags, isLoading: contextLoading, error: contextError } = useFeatureFlags();

  // Convert enum to string if needed
  const flagNameString = typeof flagName === 'string' ? flagName : flagName;

  // State for service fallback
  const [serviceEnabled, setServiceEnabled] = useState<boolean | null>(null);
  const [serviceLoading, setServiceLoading] = useState(false);
  const [serviceError, setServiceError] = useState<Error | null>(null);

  // Check if flag is in context
  const isInContext = flagNameString in flags;

  // Get value from context if available
  const contextValue = useMemo(() => {
    if (isInContext) {
      return checkFeatureFlag(flagNameString, flags);
    }
    return null;
  }, [flagNameString, flags, isInContext]);

  // Fetch from service if not in context
  useEffect(() => {
    if (!isInContext && !serviceLoading && serviceEnabled === null) {
      setServiceLoading(true);
      featureFlagService
        .isEnabled(flagNameString, organizationId)
        .then((enabled) => {
          setServiceEnabled(enabled);
          setServiceError(null);
        })
        .catch((err) => {
          setServiceError(err instanceof Error ? err : new Error('Failed to check feature flag'));
          setServiceEnabled(false); // Fail-safe
        })
        .finally(() => {
          setServiceLoading(false);
        });
    }
  }, [flagNameString, organizationId, isInContext, serviceLoading, serviceEnabled]);

  // If flag is in context, return immediately
  if (isInContext) {
    return {
      isEnabled: contextValue ?? false,
      isLoading: contextLoading,
      error: contextError,
    };
  }

  // Fallback to service result
  return {
    isEnabled: serviceEnabled ?? false,
    isLoading: serviceLoading,
    error: serviceError,
  };
}

/**
 * Hook to check multiple feature flags at once.
 * 
 * @param flagNames - Array of feature flag names to check
 * @returns Object with individual flag states and combined states
 * 
 * @example
 * ```tsx
 * const { flags, allEnabled, anyEnabled } = useMultipleFeatureFlags(['EnableAIInsights', 'EnableAdvancedMetrics']);
 * 
 * if (allEnabled) {
 *   // Both flags are enabled
 * }
 * ```
 */
export function useMultipleFeatureFlags(
  flagNames: Array<string | FeatureFlagName>
): {
  flags: Record<string, boolean>;
  allEnabled: boolean;
  anyEnabled: boolean;
  isLoading: boolean;
  error: Error | null;
} {
  const { flags, isLoading, error } = useFeatureFlags();

  const flagStates = useMemo(() => {
    const states: Record<string, boolean> = {};
    flagNames.forEach((flagName) => {
      const flagNameString = typeof flagName === 'string' ? flagName : flagName;
      states[flagNameString] = checkFeatureFlag(flagNameString, flags);
    });
    return states;
  }, [flagNames, flags]);

  const allEnabled = useMemo(() => {
    return Object.values(flagStates).every((enabled) => enabled === true);
  }, [flagStates]);

  const anyEnabled = useMemo(() => {
    return Object.values(flagStates).some((enabled) => enabled === true);
  }, [flagStates]);

  return {
    flags: flagStates,
    allEnabled,
    anyEnabled,
    isLoading,
    error,
  };
}

