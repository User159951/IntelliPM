import type { FeatureFlagsRecord } from '@/types/featureFlags';

/**
 * Check if a specific feature flag is enabled.
 * 
 * @param flagName - Name of the feature flag to check
 * @param flags - Record of feature flags (flag name -> enabled state)
 * @returns true if the flag is enabled, false otherwise
 * 
 * @example
 * ```ts
 * const flags = { EnableAIInsights: true, EnableAdvancedMetrics: false };
 * const isEnabled = checkFeatureFlag('EnableAIInsights', flags);
 * // Returns: true
 * ```
 */
export function checkFeatureFlag(
  flagName: string,
  flags: FeatureFlagsRecord
): boolean {
  if (!flags || typeof flags !== 'object') {
    return false;
  }

  return flags[flagName] === true;
}

/**
 * Get all enabled feature flags.
 * 
 * @param flags - Record of feature flags (flag name -> enabled state)
 * @returns Array of enabled feature flag names
 * 
 * @example
 * ```ts
 * const flags = { 
 *   EnableAIInsights: true, 
 *   EnableAdvancedMetrics: false,
 *   EnableRealTimeCollaboration: true 
 * };
 * const enabled = getEnabledFeatures(flags);
 * // Returns: ['EnableAIInsights', 'EnableRealTimeCollaboration']
 * ```
 */
export function getEnabledFeatures(flags: FeatureFlagsRecord): string[] {
  if (!flags || typeof flags !== 'object') {
    return [];
  }

  return Object.entries(flags)
    .filter(([_, isEnabled]) => isEnabled === true)
    .map(([flagName]) => flagName);
}

/**
 * Get all disabled feature flags.
 * 
 * @param flags - Record of feature flags (flag name -> enabled state)
 * @returns Array of disabled feature flag names
 * 
 * @example
 * ```ts
 * const flags = { 
 *   EnableAIInsights: true, 
 *   EnableAdvancedMetrics: false,
 *   EnableRealTimeCollaboration: true 
 * };
 * const disabled = getDisabledFeatures(flags);
 * // Returns: ['EnableAdvancedMetrics']
 * ```
 */
export function getDisabledFeatures(flags: FeatureFlagsRecord): string[] {
  if (!flags || typeof flags !== 'object') {
    return [];
  }

  return Object.entries(flags)
    .filter(([_, isEnabled]) => isEnabled === false)
    .map(([flagName]) => flagName);
}

/**
 * Check if multiple feature flags are all enabled.
 * 
 * @param flagNames - Array of feature flag names to check
 * @param flags - Record of feature flags (flag name -> enabled state)
 * @returns true if all flags are enabled, false otherwise
 * 
 * @example
 * ```ts
 * const flags = { EnableAIInsights: true, EnableAdvancedMetrics: true };
 * const allEnabled = areAllEnabled(['EnableAIInsights', 'EnableAdvancedMetrics'], flags);
 * // Returns: true
 * ```
 */
export function areAllEnabled(
  flagNames: string[],
  flags: FeatureFlagsRecord
): boolean {
  if (!flags || flagNames.length === 0) {
    return false;
  }

  return flagNames.every((flagName) => checkFeatureFlag(flagName, flags));
}

/**
 * Check if any of the specified feature flags are enabled.
 * 
 * @param flagNames - Array of feature flag names to check
 * @param flags - Record of feature flags (flag name -> enabled state)
 * @returns true if at least one flag is enabled, false otherwise
 * 
 * @example
 * ```ts
 * const flags = { EnableAIInsights: true, EnableAdvancedMetrics: false };
 * const anyEnabled = isAnyEnabled(['EnableAIInsights', 'EnableAdvancedMetrics'], flags);
 * // Returns: true
 * ```
 */
export function isAnyEnabled(
  flagNames: string[],
  flags: FeatureFlagsRecord
): boolean {
  if (!flags || flagNames.length === 0) {
    return false;
  }

  return flagNames.some((flagName) => checkFeatureFlag(flagName, flags));
}

/**
 * Convert array of FeatureFlag objects to FeatureFlagsRecord.
 * 
 * @param flags - Array of FeatureFlag objects
 * @returns Record of feature flags (flag name -> enabled state)
 * 
 * @example
 * ```ts
 * const flags = [
 *   { id: '1', name: 'EnableAIInsights', isEnabled: true },
 *   { id: '2', name: 'EnableAdvancedMetrics', isEnabled: false }
 * ];
 * const record = flagsToRecord(flags);
 * // Returns: { EnableAIInsights: true, EnableAdvancedMetrics: false }
 * ```
 */
export function flagsToRecord(
  flags: Array<{ name: string; isEnabled: boolean }>
): FeatureFlagsRecord {
  const record: FeatureFlagsRecord = {};

  flags.forEach((flag) => {
    record[flag.name] = flag.isEnabled;
  });

  return record;
}

