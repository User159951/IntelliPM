import { useEffect, useRef } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '@/contexts/AuthContext';
import { useToast } from '@/hooks/use-toast';
import { aiGovernanceApi } from '@/api/aiGovernance';

/**
 * Hook to automatically show toast notifications when quota thresholds are reached
 * - Shows warning at 80%
 * - Shows error at 100%
 * - Only shows each notification once per session
 */
export function useQuotaNotifications() {
  const { user } = useAuth();
  const { toast } = useToast();
  const hasShownWarningRef = useRef(false);
  const hasShownErrorRef = useRef(false);
  const organizationId = user?.organizationId || 0;

  const { data: quota } = useQuery({
    queryKey: ['ai-quota', organizationId],
    queryFn: () => aiGovernanceApi.getQuotaStatus(organizationId),
    enabled: organizationId > 0,
    refetchInterval: 60000, // Refresh every 60 seconds
    retry: 3, // Retry up to 3 times on failure
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000), // Exponential backoff: 1s, 2s, 4s (max 30s)
    staleTime: 30000, // Consider data stale after 30 seconds
  });

  useEffect(() => {
    if (!quota || !organizationId) return;

    const maxPercentage = Math.max(
      quota.requestsPercentage ?? quota.usage.requestsPercentage,
      quota.tokensPercentage ?? quota.usage.tokensPercentage,
      quota.decisionsPercentage ?? 0
    );

    // Check if quota is exceeded (100%+)
    const isExceeded = maxPercentage >= 100;

    if (isExceeded && !hasShownErrorRef.current) {
      hasShownErrorRef.current = true;
      toast({
        variant: 'destructive',
        title: 'Quota Exceeded',
        description: 'Your monthly AI quota has been exceeded. Please upgrade to continue using AI features.',
        duration: 10000, // Show for 10 seconds
      });
    } else if (quota.isAlertThreshold && !hasShownWarningRef.current && !isExceeded) {
      hasShownWarningRef.current = true;
      toast({
        variant: 'default',
        title: 'Quota Warning',
        description: `You've used ${maxPercentage.toFixed(0)}% of your monthly AI quota. Consider upgrading to avoid interruptions.`,
        duration: 8000, // Show for 8 seconds
      });
    }

    // Reset flags if quota drops below thresholds (e.g., after reset)
    if (maxPercentage < 80) {
      hasShownWarningRef.current = false;
    }
    if (maxPercentage < 100) {
      hasShownErrorRef.current = false;
    }
  }, [quota, organizationId, toast]);
}

