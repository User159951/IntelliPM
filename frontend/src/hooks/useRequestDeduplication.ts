import { useRef, useCallback } from 'react';

/**
 * Hook to prevent duplicate requests by tracking in-flight requests.
 * Returns a function that checks if a request is already in progress
 * and manages the request state.
 */
export function useRequestDeduplication() {
  const inFlightRequests = useRef<Set<string>>(new Set());

  /**
   * Check if a request with the given key is already in progress
   */
  const isRequestInFlight = useCallback((key: string): boolean => {
    return inFlightRequests.current.has(key);
  }, []);

  /**
   * Mark a request as started
   */
  const startRequest = useCallback((key: string): void => {
    inFlightRequests.current.add(key);
  }, []);

  /**
   * Mark a request as completed
   */
  const endRequest = useCallback((key: string): void => {
    inFlightRequests.current.delete(key);
  }, []);

  /**
   * Execute a request with deduplication
   * Returns a promise that resolves to the result, or null if request was deduplicated
   */
  const executeWithDeduplication = useCallback(
    async <T,>(key: string, requestFn: () => Promise<T>): Promise<T | null> => {
      if (isRequestInFlight(key)) {
        return null; // Request already in flight, skip
      }

      startRequest(key);
      try {
        const result = await requestFn();
        return result;
      } finally {
        endRequest(key);
      }
    },
    [isRequestInFlight, startRequest, endRequest]
  );

  return {
    isRequestInFlight,
    startRequest,
    endRequest,
    executeWithDeduplication,
  };
}

