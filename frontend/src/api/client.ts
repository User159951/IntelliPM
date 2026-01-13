import { authApi } from './auth';
import { toast } from '@/components/ui/sonner';
import { extractPermissionError, formatPermissionForError } from '@/lib/utils';
import i18next from 'i18next';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001';
const API_VERSION = '/api/v1';

// Helper to log errors to Sentry if configured
function logErrorToSentry(error: Error, context?: Record<string, unknown>): void {
  if (import.meta.env.VITE_SENTRY_DSN && typeof window !== 'undefined') {
    // Dynamic import to avoid bundling Sentry if not configured
    import('@sentry/react').then((Sentry) => {
      Sentry.captureException(error, {
        extra: context,
      });
    }).catch(() => {
      // Sentry not available, ignore
    });
  }
}

// Helper to get user-friendly error message based on HTTP status code
// Safe to call even when i18next is not yet initialized
function getUserFriendlyErrorMessage(status: number, defaultMessage: string): string {
  // Check if i18next is initialized before using it
  if (!i18next.isInitialized) {
    // Return default messages if i18next is not ready
    switch (status) {
      case 400:
        return 'Bad request. Please check your input.';
      case 401:
        return 'Session expired. Please log in again.';
      case 403:
        return "You don't have permission for this action.";
      case 404:
        return 'Resource not found.';
      case 429:
        return 'Too many requests. Please try again later.';
      case 500:
        return 'Server error. Please contact support.';
      case 502:
      case 503:
      case 504:
        return 'Service temporarily unavailable.';
      default:
        return defaultMessage;
    }
  }

  // Use i18next translations if available
  try {
    switch (status) {
      case 400:
        return i18next.t('errors:http.400', { defaultValue: 'Bad request. Please check your input.' });
      case 401:
        return i18next.t('errors:http.401', { defaultValue: 'Session expired. Please log in again.' });
      case 403:
        return i18next.t('errors:http.403', { defaultValue: "You don't have permission for this action." });
      case 404:
        return i18next.t('errors:http.404', { defaultValue: 'Resource not found.' });
      case 429:
        return i18next.t('errors:http.429', { defaultValue: 'Too many requests. Please try again later.' });
      case 500:
        return i18next.t('errors:http.500', { defaultValue: 'Server error. Please contact support.' });
      case 502:
      case 503:
      case 504:
        return i18next.t(`errors:http.${status}`, { defaultValue: 'Service temporarily unavailable.' });
      default:
        return defaultMessage;
    }
  } catch (error) {
    // Fallback to default messages if translation fails
    return defaultMessage;
  }
}

// Flag to prevent infinite redirect loops
let isRedirecting = false;
let isRefreshing = false;
// Queue to hold requests during token refresh
let refreshPromise: Promise<void> | null = null;

// Store quota error details globally to share across components
export interface QuotaErrorDetails {
  organizationId: number;
  quotaType: string;
  currentUsage: number;
  maxLimit: number;
  tierName: string;
}

export interface AIDisabledErrorDetails {
  organizationId: number;
  reason: string;
}

let lastQuotaError: QuotaErrorDetails | null = null;
let lastAIDisabledError: AIDisabledErrorDetails | null = null;

export function getLastQuotaError(): QuotaErrorDetails | null {
  return lastQuotaError;
}

export function clearQuotaError(): void {
  lastQuotaError = null;
}

export function getLastAIDisabledError(): AIDisabledErrorDetails | null {
  return lastAIDisabledError;
}

export function clearAIDisabledError(): void {
  lastAIDisabledError = null;
}

class ApiClient {
  private etagCache: Map<string, string> = new Map();

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string> || {}),
    };

    // Add If-None-Match header if ETag is cached and this is a GET request
    const isGetRequest = !options.method || options.method === 'GET';
    if (isGetRequest && this.etagCache.has(endpoint)) {
      headers['If-None-Match'] = this.etagCache.get(endpoint)!;
    }

    // Tokens are now in httpOnly cookies, no need to manually add Authorization header
    // The backend middleware will read from cookies and add to Authorization header

    // Ensure endpoint starts with /api/v1 if it doesn't already
    // BUT: Admin routes use /api/admin/... without versioning
    // SuperAdmin routes use /api/v1/superadmin/... with versioning
    // Ensure endpoint starts with / for proper URL construction
    const normalizedEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
    const versionedEndpoint = normalizedEndpoint.startsWith('/api/v') 
      ? normalizedEndpoint 
      : normalizedEndpoint.startsWith('/api/admin/')
        ? normalizedEndpoint // Admin routes don't use versioning
        : normalizedEndpoint.startsWith('/api/superadmin/')
          ? normalizedEndpoint.replace('/api/superadmin/', `${API_VERSION}/superadmin/`) // SuperAdmin routes use versioning
          : normalizedEndpoint.startsWith('/api/') 
            ? normalizedEndpoint.replace('/api/', `${API_VERSION}/`)
            : `${API_VERSION}${normalizedEndpoint}`;
    
    const response = await fetch(`${API_BASE_URL}${versionedEndpoint}`, {
      ...options,
      headers,
      credentials: 'include', // CRITICAL: Enable sending cookies for CORS with credentials
    });

    // Store ETag if present (for GET requests)
    if (isGetRequest) {
      const etag = response.headers.get('ETag');
      if (etag) {
        this.etagCache.set(endpoint, etag);
      }

      // Handle 304 Not Modified response
      // React Query will use cached data automatically
      if (response.status === 304) {
        // Return empty object - React Query will use cached data
        return {} as T;
      }
    }

    if (response.status === 401) {
      // Token expired or invalid
      // Only redirect if not already on login/register pages and not checking auth status
      const isAuthCheck = endpoint.includes('/Auth/me') || endpoint.includes('/auth/me');
      const isAuthPage = window.location.pathname === '/login' || window.location.pathname === '/register';
      
      // Extract error message from backend response
      let errorMessage = getUserFriendlyErrorMessage(401, 'Unauthorized');
      try {
        const error = await response.clone().json();
        // Use backend message if available, otherwise use user-friendly default
        errorMessage = error.error || error.message || error.detail || errorMessage;
      } catch {
        // If response is not JSON, use default message
      }
      
      // Try to refresh token if this is not an auth check and not on auth pages
      // Only try once to avoid infinite loops
      if (!isAuthCheck && !isAuthPage && !isRedirecting && !isRefreshing) {
        // If a refresh is already in progress, wait for it to complete
        if (refreshPromise) {
          try {
            await refreshPromise;
            // If refresh succeeded, retry the original request
            return this.request<T>(endpoint, options);
          } catch {
            // Refresh failed, throw error
            throw new Error(errorMessage);
          }
        }
        
        // Start a new refresh attempt
        isRefreshing = true;
        refreshPromise = (async () => {
          try {
            await authApi.refresh();
            // Refresh succeeded
          } catch (refreshError) {
            // Refresh failed, notify auth context and redirect to login
            isRefreshing = false;
            isRedirecting = true;
            
            // Log to Sentry
            logErrorToSentry(refreshError instanceof Error ? refreshError : new Error(String(refreshError)), {
              endpoint,
              action: 'token_refresh_failed',
            });
            
            // Notify auth context that authentication failed
            window.dispatchEvent(new Event('auth:failed'));
            
            // Use History API for SPA navigation instead of full page reload
            window.history.pushState(null, '', '/login');
            window.dispatchEvent(new PopStateEvent('popstate'));
            // Reset flag after navigation
            setTimeout(() => {
              isRedirecting = false;
            }, 100);
            
            // Reject the promise so waiting requests know refresh failed
            throw refreshError;
          } finally {
            // Clear the refresh promise after completion (success or failure)
            setTimeout(() => {
              refreshPromise = null;
              isRefreshing = false;
            }, 100);
          }
        })();
        
        try {
          await refreshPromise;
          // If refresh succeeded, retry the original request
          return this.request<T>(endpoint, options);
        } catch {
          // Refresh failed, throw error
          return Promise.reject(new Error(errorMessage));
        }
      }
      
      throw new Error(errorMessage);
    }

    if (response.status === 403) {
      // Check if it's an AI disabled error
      try {
        const errorData = await response.clone().json();
        if (errorData.error === 'AIDisabled' && errorData.details) {
          lastAIDisabledError = {
            organizationId: errorData.details.organizationId,
            reason: errorData.details.reason || 'AI has been disabled for your organization.',
          };
          // Clear quota error when AI is disabled
          lastQuotaError = null;
          const errorMessage = errorData.message || i18next.t('errors:ai.disabled', { defaultValue: 'AI features are currently disabled for your organization. Please contact an administrator for assistance.' });
          
          // Show toast notification
          toast.error(i18next.t('errors:ai.title', { defaultValue: 'Access Denied' }), {
            description: errorMessage,
          });
          
          throw new Error(errorMessage);
        }
      } catch (e) {
        if (e instanceof Error && e.message.includes('AI features')) {
          throw e;
        }
        // Not an AI disabled error, continue with normal 403 handling
      }
      
      // Try to extract permission error details from response
      let errorMessage = getUserFriendlyErrorMessage(403, "You don't have permission for this action.");
      try {
        const errorData = await response.clone().json();
        const permissionError = extractPermissionError({
          response: {
            status: 403,
            data: errorData,
          },
        });
        
        if (permissionError) {
          errorMessage = permissionError;
        } else if (errorData.message) {
          errorMessage = errorData.message;
        } else if (errorData.error) {
          errorMessage = errorData.error;
        } else if (errorData.detail) {
          errorMessage = errorData.detail;
        }
        
        // Check if error message contains permission name that can be formatted
        // Look for patterns like "projects.create" or "tasks.delete"
        const permissionPattern = /([a-z]+\.[a-z]+(?:\.[a-z]+)?)/i;
        const match = errorMessage.match(permissionPattern);
        if (match) {
          const permissionName = formatPermissionForError(match[1]);
          errorMessage = i18next.t('errors:permission.missing', {
            permission: permissionName,
            defaultValue: `You need ${permissionName} permission to perform this action.`
          });
        }
      } catch {
        // If response is not JSON, use default message
      }
      
      // Show toast notification with permission error
      toast.error(i18next.t('errors:permission.title', { defaultValue: 'Access Denied' }), {
        description: errorMessage,
      });
      throw new Error(errorMessage);
    }

    if (response.status === 429) {
      // Rate limit or quota exceeded
      // Check if it's a quota exceeded error
      try {
        const errorData = await response.clone().json();
        if (errorData.error === 'QuotaExceeded' && errorData.details) {
          lastQuotaError = {
            organizationId: errorData.details.organizationId,
            quotaType: errorData.details.quotaType || 'Requests',
            currentUsage: errorData.details.currentUsage || 0,
            maxLimit: errorData.details.maxLimit || 0,
            tierName: errorData.details.tierName || 'Free',
          };
          // Clear AI disabled error when quota is exceeded
          lastAIDisabledError = null;
          const errorMessage = errorData.message || i18next.t('errors:quota.exceeded', {
            quotaType: errorData.details.quotaType || 'request',
            currentUsage: errorData.details.currentUsage || 0,
            maxLimit: errorData.details.maxLimit || 0,
            defaultValue: `Monthly AI ${errorData.details.quotaType?.toLowerCase() || 'request'} limit exceeded (${errorData.details.currentUsage || 0}/${errorData.details.maxLimit || 0}). Please upgrade to continue using AI features.`
          });
          
          // Show toast notification
          toast.error(i18next.t('errors:quota.title', { defaultValue: 'Quota Exceeded' }), {
            description: errorMessage,
          });
          
          throw new Error(errorMessage);
        }
      } catch (e) {
        if (e instanceof Error && e.message.includes('limit exceeded')) {
          throw e;
        }
        // Not a quota error, continue with normal 429 handling
      }

      // Rate limit exceeded - check both response body and headers
      let retryAfter = 60;
      
      // Check Retry-After header (can be number of seconds or HTTP-date)
      const retryAfterHeader = response.headers.get('Retry-After');
      if (retryAfterHeader) {
        const parsed = parseInt(retryAfterHeader, 10);
        if (!isNaN(parsed)) {
          retryAfter = parsed;
        }
      }
      
      // Also check response body for retryAfter field
      try {
        const errorData = await response.clone().json();
        if (errorData.retryAfter && typeof errorData.retryAfter === 'number') {
          retryAfter = errorData.retryAfter;
        }
      } catch {
        // If response is not JSON, use header value or default
      }
      
      const errorMessage = getUserFriendlyErrorMessage(429, i18next.t('errors:http.429', { defaultValue: `Too many requests. Please wait ${retryAfter} seconds before trying again.` }));
      toast.error(i18next.t('notifications:titles.tooManyRequests', { defaultValue: 'Too Many Requests' }), {
        description: errorMessage,
      });
      throw new Error(errorMessage);
    }

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'An error occurred' }));

      // Try to extract a field-level validation message first (e.g. password rule)
      let fieldMessage: string | undefined;
      const errors = error.errors;

      if (errors) {
        if (Array.isArray(errors) && errors.length > 0) {
          fieldMessage = errors[0];
        } else if (typeof errors === 'object') {
          const firstKey = Object.keys(errors)[0];
          const value = errors[firstKey];
          if (Array.isArray(value) && value.length > 0) {
            fieldMessage = value[0];
          } else if (typeof value === 'string') {
            fieldMessage = value;
          }
        }
      }

      // Backend error shape (global exception handler) uses `error` and `errors`
      // in addition to possible `title` / `detail`. Prefer the most specific text available.
      const defaultMessage = fieldMessage ||
        error.detail ||
        error.title ||
        error.message ||
        error.error ||
        'Request failed';
      
      // Get user-friendly message based on status code
      const message = getUserFriendlyErrorMessage(response.status, defaultMessage);
      
      // Log error to Sentry for server errors (5xx)
      if (response.status >= 500) {
        logErrorToSentry(new Error(message), {
          endpoint,
          status: response.status,
          errorData: error,
        });
      }
      
      // Show toast notification for server errors
      if (response.status >= 500) {
        toast.error(i18next.t('notifications:titles.serverError', { defaultValue: 'Server Error' }), {
          description: message,
        });
      } else if (response.status >= 400 && response.status < 500) {
        // Show toast for client errors (except 401, 403, 429 which are handled above)
        if (response.status !== 401 && response.status !== 403 && response.status !== 429) {
          toast.error(i18next.t('notifications:titles.requestFailed', { defaultValue: 'Request Failed' }), {
            description: message,
          });
        }
      }

      throw new Error(message);
    }

    if (response.status === 204) {
      return {} as T;
    }

    return response.json();
  }

  /**
   * GET request with optional ETag caching
   * @param endpoint API endpoint
   * @param useCache Whether to use ETag caching (default: true)
   * @returns Response data
   */
  get<T>(endpoint: string, useCache = true): Promise<T> {
    if (!useCache) {
      // Clear ETag cache for this endpoint if cache is disabled
      this.etagCache.delete(endpoint);
    }
    return this.request<T>(endpoint, { method: 'GET' });
  }

  /**
   * Clear ETag cache for a specific endpoint or all endpoints
   * @param endpoint Optional endpoint to clear, or undefined to clear all
   */
  clearCache(endpoint?: string): void {
    if (endpoint) {
      this.etagCache.delete(endpoint);
    } else {
      this.etagCache.clear();
    }
  }

  post<T>(endpoint: string, data?: unknown): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  put<T>(endpoint: string, data?: unknown): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  patch<T>(endpoint: string, data?: unknown): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'PATCH',
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  delete<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE' });
  }
}

export const apiClient = new ApiClient();
