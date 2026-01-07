import { apiClient } from '@/api/client';

/**
 * Feature flag data structure
 */
export interface FeatureFlag {
  /**
   * Unique identifier for the feature flag
   */
  id: string;

  /**
   * Unique name of the feature flag (e.g., "EnableAIInsights", "EnableAdvancedMetrics")
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
 * Cache entry structure for feature flag values
 */
interface CacheEntry {
  /**
   * Cached value (enabled/disabled)
   */
  value: boolean;

  /**
   * Timestamp when the cache entry expires (milliseconds since epoch)
   */
  expiresAt: number;

  /**
   * Full feature flag object (optional, for getFlag method)
   */
  flag?: FeatureFlag;
}

/**
 * Cache key format: "flagName:organizationId" or "flagName:global"
 */
type CacheKey = string;

/**
 * Feature flag service for fetching and caching feature flags.
 * 
 * This service provides:
 * - In-memory caching with 5-minute TTL
 * - Automatic cache refresh for expired entries
 * - Fail-safe behavior (returns false on errors)
 * - Support for global and organization-specific flags
 * 
 * @example
 * ```ts
 * // Check if a feature is enabled
 * const isEnabled = await featureFlagService.isEnabled('EnableAIInsights');
 * 
 * // Get full flag details
 * const flag = await featureFlagService.getFlag('EnableAdvancedMetrics', '123');
 * 
 * // Get all flags for an organization
 * const flags = await featureFlagService.getAllFlags('123');
 * ```
 */
class FeatureFlagService {
  /**
   * In-memory cache for feature flags
   * Key format: "flagName:organizationId" or "flagName:global"
   */
  private cache: Map<CacheKey, CacheEntry> = new Map();

  /**
   * Cache TTL in milliseconds (5 minutes)
   */
  private readonly CACHE_TTL_MS = 5 * 60 * 1000;

  /**
   * Pending fetch promises to prevent duplicate API calls
   */
  private pendingFetches: Map<CacheKey, Promise<boolean>> = new Map();

  /**
   * Pending flag fetches to prevent duplicate API calls
   */
  private pendingFlagFetches: Map<CacheKey, Promise<FeatureFlag | null>> = new Map();

  /**
   * Pending all flags fetches to prevent duplicate API calls
   */
  private pendingAllFlagsFetches: Map<string, Promise<FeatureFlag[]>> = new Map();

  /**
   * Generate cache key from flag name and organization ID
   * @param flagName - Feature flag name
   * @param organizationId - Optional organization ID
   * @returns Cache key string
   */
  private getCacheKey(flagName: string, organizationId?: string): CacheKey {
    return organizationId ? `${flagName}:${organizationId}` : `${flagName}:global`;
  }

  /**
   * Check if a cache entry is expired
   * @param entry - Cache entry to check
   * @returns true if expired, false otherwise
   */
  private isExpired(entry: CacheEntry): boolean {
    return Date.now() > entry.expiresAt;
  }

  /**
   * Create a new cache entry with expiration
   * @param value - Feature flag enabled value
   * @param flag - Optional full feature flag object
   * @returns Cache entry
   */
  private createCacheEntry(value: boolean, flag?: FeatureFlag): CacheEntry {
    return {
      value,
      expiresAt: Date.now() + this.CACHE_TTL_MS,
      flag,
    };
  }

  /**
   * Get cached value if available and not expired
   * @param key - Cache key
   * @returns Cached entry or null
   */
  private getCached(key: CacheKey): CacheEntry | null {
    const entry = this.cache.get(key);
    if (!entry) {
      return null;
    }

    if (this.isExpired(entry)) {
      // Remove expired entry
      this.cache.delete(key);
      return null;
    }

    return entry;
  }

  /**
   * Fetch feature flag from API
   * @param flagName - Feature flag name
   * @param organizationId - Optional organization ID
   * @returns Feature flag or null if not found
   */
  private async fetchFlagFromAPI(
    flagName: string,
    organizationId?: string
  ): Promise<FeatureFlag | null> {
    try {
      const params = new URLSearchParams();
      if (organizationId) {
        params.append('organizationId', organizationId);
      }

      // Use public endpoint accessible to all authenticated users (not just admins)
      // The admin endpoint is only for managing feature flags, not reading them
      const endpoint = `/api/v1/feature-flags/${encodeURIComponent(flagName)}${params.toString() ? `?${params.toString()}` : ''}`;
      const flag = await apiClient.get<FeatureFlag>(endpoint);
      return flag;
    } catch (error) {
      // Error fetching flag, return null (fail-safe)
      return null;
    }
  }

  /**
   * Fetch all feature flags from API
   * @param organizationId - Optional organization ID
   * @returns Array of feature flags
   */
  private async fetchAllFlagsFromAPI(organizationId?: string): Promise<FeatureFlag[]> {
    try {
      const params = new URLSearchParams();
      if (organizationId) {
        params.append('organizationId', organizationId);
      }

      // Use public endpoint accessible to all authenticated users (not just admins)
      // The admin endpoint is only for managing feature flags, not reading them
      const endpoint = `/api/v1/feature-flags${params.toString() ? `?${params.toString()}` : ''}`;
      const flags = await apiClient.get<FeatureFlag[]>(endpoint);
      return Array.isArray(flags) ? flags : [];
    } catch (error) {
      // Don't catch 401 errors - let the API client handle token refresh
      if (error instanceof Error && (error.message.includes('Unauthorized') || error.message.includes('401'))) {
        // Re-throw 401 errors so the API client can handle token refresh
        throw error;
      }
      
      // For other errors, return empty array (fail-safe)
      return [];
    }
  }

  /**
   * Check if a feature flag is enabled.
   * 
   * Uses caching to minimize API calls. Returns false (fail-safe) on errors.
   * 
   * @param flagName - Name of the feature flag to check
   * @param organizationId - Optional organization ID for organization-specific flags
   * @returns Promise resolving to true if enabled, false otherwise
   * 
   * @example
   * ```ts
   * const isEnabled = await featureFlagService.isEnabled('EnableAIInsights');
   * if (isEnabled) {
   *   // Show AI features
   * }
   * ```
   */
  async isEnabled(flagName: string, organizationId?: string): Promise<boolean> {
    const key = this.getCacheKey(flagName, organizationId);

    // Check cache first
    const cached = this.getCached(key);
    if (cached) {
      return cached.value;
    }

    // Check if there's already a pending fetch for this key
    const pending = this.pendingFetches.get(key);
    if (pending) {
      return pending;
    }

    // Create new fetch promise
    const fetchPromise = (async () => {
      try {
        const flag = await this.fetchFlagFromAPI(flagName, organizationId);
        const isEnabled = flag?.isEnabled ?? false;

        // Cache the result
        this.cache.set(key, this.createCacheEntry(isEnabled, flag ?? undefined));

        return isEnabled;
      } catch (error) {
        // Fail-safe: return false on error
        return false;
      } finally {
        // Remove from pending fetches
        this.pendingFetches.delete(key);
      }
    })();

    // Store pending fetch
    this.pendingFetches.set(key, fetchPromise);

    return fetchPromise;
  }

  /**
   * Get full feature flag object.
   * 
   * Uses caching to minimize API calls. Returns null if not found or on error.
   * 
   * @param flagName - Name of the feature flag
   * @param organizationId - Optional organization ID for organization-specific flags
   * @returns Promise resolving to FeatureFlag or null
   * 
   * @example
   * ```ts
   * const flag = await featureFlagService.getFlag('EnableAdvancedMetrics', '123');
   * if (flag?.isEnabled) {
   *   // Show advanced metrics
   * }
   * ```
   */
  async getFlag(flagName: string, organizationId?: string): Promise<FeatureFlag | null> {
    const key = this.getCacheKey(flagName, organizationId);

    // Check cache first
    const cached = this.getCached(key);
    if (cached && cached.flag) {
      return cached.flag;
    }

    // Check if there's already a pending fetch for this key
    const pending = this.pendingFlagFetches.get(key);
    if (pending) {
      return pending;
    }

    // Create new fetch promise
    const fetchPromise = (async () => {
      try {
        const flag = await this.fetchFlagFromAPI(flagName, organizationId);

        if (flag) {
          // Cache the result
          this.cache.set(key, this.createCacheEntry(flag.isEnabled, flag ?? undefined));
        }

        return flag;
      } catch (error) {
        // Error fetching flag, return null (fail-safe)
        return null;
      } finally {
        // Remove from pending fetches
        this.pendingFlagFetches.delete(key);
      }
    })();

    // Store pending fetch
    this.pendingFlagFetches.set(key, fetchPromise);

    return fetchPromise;
  }

  /**
   * Get all feature flags for an organization (or global flags if no organization ID).
   * 
   * This method does not use the same cache as individual flag checks.
   * It fetches all flags and updates the cache for each flag.
   * 
   * @param organizationId - Optional organization ID to filter flags
   * @returns Promise resolving to array of feature flags
   * 
   * @example
   * ```ts
   * const flags = await featureFlagService.getAllFlags('123');
   * flags.forEach(flag => {
   *   // Process flag: flag.name, flag.isEnabled
   * });
   * ```
   */
  async getAllFlags(organizationId?: string): Promise<FeatureFlag[]> {
    const cacheKey = organizationId ?? 'global';

    // Check if there's already a pending fetch
    const pending = this.pendingAllFlagsFetches.get(cacheKey);
    if (pending) {
      return pending;
    }

    // Create new fetch promise
    const fetchPromise = (async () => {
      try {
        const flags = await this.fetchAllFlagsFromAPI(organizationId);

        // Update cache for each flag
        flags.forEach((flag) => {
          const key = this.getCacheKey(flag.name, flag.organizationId);
          this.cache.set(key, this.createCacheEntry(flag.isEnabled, flag));
        });

        return flags;
      } catch (error) {
        // Don't catch 401 errors - let the API client handle token refresh
        if (error instanceof Error && (error.message.includes('Unauthorized') || error.message.includes('401'))) {
          // Re-throw 401 errors so the API client can handle token refresh
          throw error;
        }
        
        // For other errors, log and return empty array (fail-safe)
        if (import.meta.env.DEV) {
          // Error fetching all flags (fail-safe)
        }
        return [];
      } finally {
        // Remove from pending fetches
        this.pendingAllFlagsFetches.delete(cacheKey);
      }
    })();

    // Store pending fetch
    this.pendingAllFlagsFetches.set(cacheKey, fetchPromise);

    return fetchPromise;
  }

  /**
   * Refresh the cache by fetching fresh data from the API.
   * 
   * This will clear expired entries and fetch fresh data for all cached flags.
   * 
   * @returns Promise that resolves when cache refresh is complete
   * 
   * @example
   * ```ts
   * // Refresh cache manually
   * await featureFlagService.refreshCache();
   * ```
   */
  async refreshCache(): Promise<void> {
    // Clear expired entries
    const keysToRefresh: CacheKey[] = [];
    this.cache.forEach((entry, key) => {
      if (this.isExpired(entry)) {
        this.cache.delete(key);
      } else {
        keysToRefresh.push(key);
      }
    });

    // Fetch fresh data for all cached flags
    const refreshPromises = keysToRefresh.map(async (key) => {
      const [flagName, orgId] = key.split(':');
      const organizationId = orgId === 'global' ? undefined : orgId;

      try {
        const flag = await this.fetchFlagFromAPI(flagName, organizationId);
        if (flag) {
          this.cache.set(key, this.createCacheEntry(flag.isEnabled, flag));
        } else {
          // Flag no longer exists, remove from cache
          this.cache.delete(key);
        }
      } catch (error) {
        // Error refreshing flag, keep existing cache entry (fail-safe)
      }
    });

    await Promise.all(refreshPromises);
  }

  /**
   * Clear all cached feature flags.
   * 
   * This will remove all entries from the cache. Next calls to isEnabled/getFlag
   * will fetch fresh data from the API.
   * 
   * @example
   * ```ts
   * // Clear cache
   * featureFlagService.clearCache();
   * ```
   */
  clearCache(): void {
    this.cache.clear();
    this.pendingFetches.clear();
    this.pendingFlagFetches.clear();
    this.pendingAllFlagsFetches.clear();
  }

  /**
   * Get cache statistics (for debugging)
   * @returns Object with cache statistics
   */
  getCacheStats(): {
    size: number;
    expired: number;
    valid: number;
  } {
    let expired = 0;
    let valid = 0;

    this.cache.forEach((entry) => {
      if (this.isExpired(entry)) {
        expired++;
      } else {
        valid++;
      }
    });

    return {
      size: this.cache.size,
      expired,
      valid,
    };
  }
}

/**
 * Singleton instance of the feature flag service
 * Export this instance for use throughout the application
 */
export const featureFlagService = new FeatureFlagService();

