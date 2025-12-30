/**
 * Known feature flag names in the system.
 * Add new feature flags here as they are created.
 */
export enum FeatureFlagName {
  /**
   * Enable AI-powered insights and recommendations
   */
  EnableAIInsights = 'EnableAIInsights',

  /**
   * Enable advanced project metrics and analytics
   */
  EnableAdvancedMetrics = 'EnableAdvancedMetrics',

  /**
   * Enable real-time collaboration features
   */
  EnableRealTimeCollaboration = 'EnableRealTimeCollaboration',

  /**
   * Enable dark mode (if not already enabled globally)
   */
  EnableDarkMode = 'EnableDarkMode',

  /**
   * Enable experimental features
   */
  EnableExperimentalFeatures = 'EnableExperimentalFeatures',

  /**
   * Enable advanced reporting
   */
  EnableAdvancedReporting = 'EnableAdvancedReporting',

  /**
   * Enable custom workflows
   */
  EnableCustomWorkflows = 'EnableCustomWorkflows',

  /**
   * Enable integrations with external tools
   */
  EnableIntegrations = 'EnableIntegrations',
}

/**
 * Feature flag data structure
 */
export interface FeatureFlag {
  /**
   * Unique identifier for the feature flag
   */
  id: string;

  /**
   * Unique name of the feature flag
   */
  name: string;

  /**
   * Indicates whether the feature is currently enabled
   */
  isEnabled: boolean;

  /**
   * Organization ID for organization-specific feature flags.
   * Undefined/null indicates a global feature flag that applies to all organizations.
   */
  organizationId?: string;

  /**
   * Optional description explaining what the feature flag controls
   */
  description?: string;

  /**
   * The date and time when the feature flag was created
   */
  createdAt?: string;

  /**
   * The date and time when the feature flag was last updated
   */
  updatedAt?: string;
}

/**
 * Record of feature flags where key is flag name and value is enabled state
 */
export type FeatureFlagsRecord = Record<string, boolean>;

/**
 * Feature flags context value type
 */
export interface FeatureFlagsContextType {
  /**
   * Record of all feature flags (flag name -> enabled state)
   */
  flags: FeatureFlagsRecord;

  /**
   * Loading state - true when fetching flags
   */
  isLoading: boolean;

  /**
   * Error state - Error object if any error occurred, null otherwise
   */
  error: Error | null;

  /**
   * Refresh flags from the server
   */
  refresh: () => Promise<void>;

  /**
   * Check if a specific feature flag is enabled
   * @param flagName - Name of the feature flag to check
   * @returns true if enabled, false otherwise
   */
  isEnabled: (flagName: string) => boolean;
}

