import React from 'react';
import { useFeatureFlag, useMultipleFeatureFlags } from '@/hooks/useFeatureFlag';
import { Skeleton } from '@/components/ui/skeleton';
import type { FeatureFlagName } from '@/types/featureFlags';

/**
 * Props for the FeatureFlag component
 */
export interface FeatureFlagProps {
  /**
   * Name of the feature flag to check (string or FeatureFlagName enum)
   */
  flagName: string | FeatureFlagName;

  /**
   * Children to render if the feature flag is enabled
   */
  children: React.ReactNode;

  /**
   * Optional fallback content to render if the feature flag is disabled
   * If not provided, nothing will be rendered when disabled
   */
  fallback?: React.ReactNode;

  /**
   * Optional organization ID for organization-specific feature flags
   */
  organizationId?: string;

  /**
   * Optional custom loading component to show while checking the flag
   * Default: Skeleton component
   */
  loadingComponent?: React.ReactNode;

  /**
   * Optional flag to show loading state
   * Default: true
   */
  showLoading?: boolean;
}

/**
 * FeatureFlag component that conditionally renders children based on feature flag state.
 * 
 * This component:
 * - Checks if the specified feature flag is enabled
 * - Renders children if enabled
 * - Renders fallback (or nothing) if disabled
 * - Shows loading state while checking
 * - Supports organization-specific flags
 * 
 * @example
 * ```tsx
 * // Basic usage
 * <FeatureFlag flagName="EnableAIInsights">
 *   <AIInsightsPanel />
 * </FeatureFlag>
 * 
 * // With fallback
 * <FeatureFlag 
 *   flagName="EnableGanttChart" 
 *   fallback={<ComingSoonBadge />}
 * >
 *   <GanttChartView />
 * </FeatureFlag>
 * 
 * // Using enum
 * <FeatureFlag flagName={FeatureFlagName.EnableAdvancedMetrics}>
 *   <AdvancedMetrics />
 * </FeatureFlag>
 * 
 * // Organization-specific
 * <FeatureFlag 
 *   flagName="EnableCustomWorkflows" 
 *   organizationId="123"
 * >
 *   <CustomWorkflows />
 * </FeatureFlag>
 * ```
 */
export function FeatureFlag({
  flagName,
  children,
  fallback,
  organizationId,
  loadingComponent,
  showLoading = true,
}: FeatureFlagProps): React.ReactElement | null {
  const { isEnabled, isLoading, error } = useFeatureFlag(flagName, organizationId);

  // Show loading state
  if (isLoading && showLoading) {
    if (loadingComponent) {
      return <>{loadingComponent}</>;
    }
    return (
      <div className="flex items-center justify-center p-4">
        <Skeleton className="h-8 w-full" />
      </div>
    );
  }

  // Handle error state (fail-safe: don't show feature)
  if (error) {
    if (import.meta.env.DEV) {
      console.error(`[FeatureFlag] Error checking flag "${flagName}":`, error);
    }
    // On error, don't show the feature (fail-safe)
    return fallback ? <>{fallback}</> : null;
  }

  // Render based on flag state
  if (isEnabled) {
    return <>{children}</>;
  }

  // Flag is disabled, show fallback or nothing
  return fallback ? <>{fallback}</> : null;
}

/**
 * FeatureFlag variant that requires multiple flags to all be enabled.
 * 
 * @example
 * ```tsx
 * <FeatureFlagAll 
 *   flagNames={['EnableAIInsights', 'EnableAdvancedMetrics']}
 *   fallback={<div>Both features must be enabled</div>}
 * >
 *   <CombinedFeature />
 * </FeatureFlagAll>
 * ```
 */
export interface FeatureFlagAllProps {
  /**
   * Array of feature flag names that must all be enabled
   */
  flagNames: Array<string | FeatureFlagName>;
  children: React.ReactNode;
  fallback?: React.ReactNode;
  organizationId?: string;
  loadingComponent?: React.ReactNode;
  showLoading?: boolean;
}

export function FeatureFlagAll({
  flagNames,
  children,
  fallback,
  organizationId: _organizationId,
  loadingComponent,
  showLoading = true,
}: FeatureFlagAllProps): React.ReactElement | null {
  const { allEnabled, isLoading, error } = useMultipleFeatureFlags(flagNames);
  const hasError = error !== null;

  if (isLoading && showLoading) {
    if (loadingComponent) {
      return <>{loadingComponent}</>;
    }
    return (
      <div className="flex items-center justify-center p-4">
        <Skeleton className="h-8 w-full" />
      </div>
    );
  }

  if (hasError) {
    return fallback ? <>{fallback}</> : null;
  }

  if (allEnabled) {
    return <>{children}</>;
  }

  return fallback ? <>{fallback}</> : null;
}

/**
 * FeatureFlag variant that requires any of the specified flags to be enabled.
 * 
 * @example
 * ```tsx
 * <FeatureFlagAny 
 *   flagNames={['EnableAIInsights', 'EnableAdvancedMetrics']}
 * >
 *   <AnyFeature />
 * </FeatureFlagAny>
 * ```
 */
export interface FeatureFlagAnyProps {
  /**
   * Array of feature flag names - at least one must be enabled
   */
  flagNames: Array<string | FeatureFlagName>;
  children: React.ReactNode;
  fallback?: React.ReactNode;
  organizationId?: string;
  loadingComponent?: React.ReactNode;
  showLoading?: boolean;
}

export function FeatureFlagAny({
  flagNames,
  children,
  fallback,
  organizationId: _organizationId,
  loadingComponent,
  showLoading = true,
}: FeatureFlagAnyProps): React.ReactElement | null {
  const { flags: _flags, anyEnabled, isLoading, error } = useMultipleFeatureFlags(flagNames);
  const hasError = error !== null;

  if (isLoading && showLoading) {
    if (loadingComponent) {
      return <>{loadingComponent}</>;
    }
    return (
      <div className="flex items-center justify-center p-4">
        <Skeleton className="h-8 w-full" />
      </div>
    );
  }

  if (hasError) {
    return fallback ? <>{fallback}</> : null;
  }

  if (anyEnabled) {
    return <>{children}</>;
  }

  return fallback ? <>{fallback}</> : null;
}

