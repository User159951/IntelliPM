import { useMemo } from 'react';
import { useFeatureFlags } from '@/contexts/FeatureFlagsContext';
import { checkFeatureFlag } from '@/utils/featureFlags';
import type { FeatureFlagName } from '@/types/featureFlags';

/**
 * Hook return type for useFeatureFlag
 */
export interface UseFeatureFlagReturn {
  /**
   * Whether the feature flag is enabled
   * Returns false if flag doesn't exist (no hardcoded defaults)
   */
  isEnabled: boolean;

  /**
   * Loading state - true when fetching flags from API
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
 * - Gets flags from FeatureFlagsContext (which uses React Query)
 * - Returns false if flag doesn't exist (no hardcoded defaults)
 * - Returns loading/error states from context
 * 
 * @param flagName - Name of the feature flag to check (string or FeatureFlagName enum)
 * @param organizationId - Optional organization ID (for future use, currently flags are fetched per organization)
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
  _organizationId?: string
): UseFeatureFlagReturn {
  const { flags, isLoading, error } = useFeatureFlags();

  // Convert enum to string if needed
  const flagNameString = typeof flagName === 'string' ? flagName : flagName;

  // Check if flag is enabled (returns false if flag doesn't exist - no hardcoded defaults)
  const isEnabled = useMemo(() => {
    return checkFeatureFlag(flagNameString, flags);
  }, [flagNameString, flags]);

  return {
    isEnabled,
    isLoading,
    error,
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

