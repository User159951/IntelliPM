import { apiClient } from './client';
import type { FeatureFlag } from '@/types/featureFlags';

/**
 * API client for feature flags endpoints
 * Provides methods to interact with feature flags API
 */
export const featureFlagsApi = {
  /**
   * Get all feature flags for the current organization or global flags
   * @param organizationId - Optional organization ID to filter flags
   * @returns Promise resolving to array of feature flags
   */
  getAllFlags: (organizationId?: string): Promise<FeatureFlag[]> => {
    const params = new URLSearchParams();
    if (organizationId) {
      params.append('organizationId', organizationId);
    }
    const endpoint = `/api/v1/feature-flags${params.toString() ? `?${params.toString()}` : ''}`;
    return apiClient.get<FeatureFlag[]>(endpoint);
  },
};

