import { useEffect } from 'react';
import { useToast } from '@/hooks/use-toast';
import { getLastQuotaError, getLastAIDisabledError, clearQuotaError, clearAIDisabledError } from '@/api/client';

/**
 * Hook to automatically display toast notifications for AI quota and disabled errors.
 * Should be used in pages/components that interact with AI features.
 */
export function useAIErrorHandler() {
  const { toast } = useToast();

  useEffect(() => {
    // Check for quota error
    const quotaError = getLastQuotaError();
    if (quotaError) {
      const quotaTypeDisplay = quotaError.quotaType === 'Requests' 
        ? 'requêtes' 
        : quotaError.quotaType === 'Tokens' 
        ? 'tokens' 
        : quotaError.quotaType.toLowerCase();

      toast({
        variant: 'destructive',
        title: 'Quota AI dépassé',
        description: `Vous avez atteint la limite mensuelle de ${quotaTypeDisplay} (${quotaError.currentUsage.toLocaleString()}/${quotaError.maxLimit.toLocaleString()}). Passez au plan supérieur pour continuer.`,
        duration: 10000, // Show for 10 seconds
      });
    }

    // Check for AI disabled error
    const aiDisabledError = getLastAIDisabledError();
    if (aiDisabledError) {
      toast({
        variant: 'destructive',
        title: 'IA désactivée',
        description: 'L\'IA a été désactivée pour votre organisation. Contactez un administrateur pour plus d\'informations.',
        duration: 10000,
      });
    }

    // Clean up on unmount
    return () => {
      // Don't clear errors on unmount - let components handle that
    };
  }, [toast]);

  return {
    clearQuotaError,
    clearAIDisabledError,
    getQuotaError: getLastQuotaError,
    getAIDisabledError: getLastAIDisabledError,
  };
}

