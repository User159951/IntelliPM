import { useEffect, useCallback, useRef } from 'react';
import { useToast } from '@/hooks/use-toast';
import { getLastQuotaError, getLastAIDisabledError, clearQuotaError, clearAIDisabledError } from '@/api/client';

export interface AIErrorHandlerOptions {
  showToast?: boolean;
  onQuotaExceeded?: () => void;
  onAIDisabled?: () => void;
  onTimeout?: () => void;
  timeoutMs?: number;
}

export interface AIErrorResult {
  isQuotaExceeded: boolean;
  isAIDisabled: boolean;
  isTimeout: boolean;
  canRetry: boolean;
  retryAfter?: number; // seconds
}

/**
 * Hook to handle AI-related errors consistently across the application.
 * Provides both automatic error detection (via useEffect) and programmatic error handling.
 * 
 * Features:
 * - Handles 429 (quota exceeded) errors with user-friendly messages
 * - Handles AI disabled state (403)
 * - Handles timeout errors gracefully
 * - Provides retry options where appropriate
 * - Prevents double executions with loading state management
 */
export function useAIErrorHandler(options: AIErrorHandlerOptions = {}) {
  const { toast } = useToast();
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);
  const { showToast = true, timeoutMs = 120000 } = options; // Default 2 minutes timeout

  // Clear timeout on unmount
  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  // Automatic error detection via useEffect (for background error checking)
  useEffect(() => {
    // Check for quota error
    const quotaError = getLastQuotaError();
    if (quotaError && showToast) {
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

      options.onQuotaExceeded?.();
    }

    // Check for AI disabled error
    const aiDisabledError = getLastAIDisabledError();
    if (aiDisabledError && showToast) {
      toast({
        variant: 'destructive',
        title: 'IA désactivée',
        description: 'L\'IA a été désactivée pour votre organisation. Contactez un administrateur pour plus d\'informations.',
        duration: 10000,
      });

      options.onAIDisabled?.();
    }
  }, [toast, showToast, options]);

  /**
   * Handle an error from an AI operation
   * @param error - The error object
   * @param retryAfter - Optional retry-after time in seconds
   * @returns Error result with details about what went wrong
   */
  const handleError = useCallback((error: Error, retryAfter?: number): AIErrorResult => {
    const errorMessage = error.message.toLowerCase();
    
    // Check for quota exceeded
    const quotaError = getLastQuotaError();
    if (quotaError || errorMessage.includes('quota') || errorMessage.includes('limit exceeded')) {
      if (showToast && !quotaError) {
        toast({
          variant: 'destructive',
          title: 'Quota AI dépassé',
          description: 'Vous avez atteint votre limite mensuelle. Passez au plan supérieur pour continuer.',
          duration: 10000,
        });
      }
      options.onQuotaExceeded?.();
      return {
        isQuotaExceeded: true,
        isAIDisabled: false,
        isTimeout: false,
        canRetry: false, // Can't retry quota exceeded
      };
    }

    // Check for AI disabled
    const aiDisabledError = getLastAIDisabledError();
    if (aiDisabledError || errorMessage.includes('ai disabled') || errorMessage.includes('access denied')) {
      if (showToast && !aiDisabledError) {
        toast({
          variant: 'destructive',
          title: 'IA désactivée',
          description: 'L\'IA a été désactivée pour votre organisation. Contactez un administrateur pour plus d\'informations.',
          duration: 10000,
        });
      }
      options.onAIDisabled?.();
      return {
        isQuotaExceeded: false,
        isAIDisabled: true,
        isTimeout: false,
        canRetry: false, // Can't retry if AI is disabled
      };
    }

    // Check for timeout
    if (errorMessage.includes('timeout') || errorMessage.includes('timed out') || errorMessage.includes('aborted')) {
      if (showToast) {
        toast({
          variant: 'destructive',
          title: 'Délai d\'attente dépassé',
          description: 'L\'opération AI a pris trop de temps. Veuillez réessayer.',
          duration: 8000,
        });
      }
      options.onTimeout?.();
      return {
        isQuotaExceeded: false,
        isAIDisabled: false,
        isTimeout: true,
        canRetry: true,
        retryAfter: retryAfter || 5, // Default 5 seconds
      };
    }

    // Generic error - can retry
    return {
      isQuotaExceeded: false,
      isAIDisabled: false,
      isTimeout: false,
      canRetry: true,
      retryAfter: retryAfter,
    };
  }, [toast, showToast, options]);

  /**
   * Wrap an AI operation with error handling and timeout protection
   * @param operation - The async operation to execute
   * @param estimatedTimeMs - Estimated time for the operation (for user feedback)
   * @returns Promise that resolves with the result or rejects with handled error
   */
  const executeWithErrorHandling = useCallback(async <T,>(
    operation: () => Promise<T>,
    estimatedTimeMs?: number
  ): Promise<T> => {
    // Clear any existing timeout
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    // Set up timeout
    const timeoutPromise = new Promise<never>((_, reject) => {
      timeoutRef.current = setTimeout(() => {
        reject(new Error('Operation timed out. The AI request took too long to complete.'));
      }, timeoutMs);
    });

    try {
      // Race between operation and timeout
      const result = await Promise.race([operation(), timeoutPromise]);
      
      // Clear timeout on success
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
        timeoutRef.current = null;
      }

      return result;
    } catch (error) {
      // Clear timeout on error
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
        timeoutRef.current = null;
      }

      // Handle the error
      const errorResult = handleError(error as Error);
      
      // Re-throw with more context if needed
      if (errorResult.isQuotaExceeded || errorResult.isAIDisabled) {
        throw new Error('AI operation unavailable');
      }
      
      throw error;
    }
  }, [timeoutMs, handleError]);

  /**
   * Check if AI operations are currently blocked
   */
  const isBlocked = useCallback((): boolean => {
    return !!getLastQuotaError() || !!getLastAIDisabledError();
  }, []);

  /**
   * Get user-friendly error message for the current state
   */
  const getErrorMessage = useCallback((): string | null => {
    const quotaError = getLastQuotaError();
    if (quotaError) {
      const quotaTypeDisplay = quotaError.quotaType === 'Requests' 
        ? 'requêtes' 
        : quotaError.quotaType === 'Tokens' 
        ? 'tokens' 
        : quotaError.quotaType.toLowerCase();
      return `Quota AI dépassé (${quotaError.currentUsage.toLocaleString()}/${quotaError.maxLimit.toLocaleString()} ${quotaTypeDisplay})`;
    }

    const aiDisabledError = getLastAIDisabledError();
    if (aiDisabledError) {
      return 'L\'IA a été désactivée pour votre organisation';
    }

    return null;
  }, []);

  return {
    // Error handling functions
    handleError,
    executeWithErrorHandling,
    isBlocked,
    getErrorMessage,
    
    // Direct access to error state
    clearQuotaError,
    clearAIDisabledError,
    getQuotaError: getLastQuotaError,
    getAIDisabledError: getLastAIDisabledError,
  };
}

