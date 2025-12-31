import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { apiClient } from './client';

// Mock global fetch
const mockFetch = vi.fn();
global.fetch = mockFetch;

// Mock window.location
const mockLocation = { href: '', pathname: '/dashboard' };
// eslint-disable-next-line @typescript-eslint/no-explicit-any
delete (window as any).location;
// eslint-disable-next-line @typescript-eslint/no-explicit-any
(window as any).location = mockLocation;

// Mock sessionStorage
const mockSessionStorage = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
};
Object.defineProperty(window, 'sessionStorage', {
  value: mockSessionStorage,
  writable: true,
});

describe('ApiClient', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockLocation.pathname = '/dashboard';
    mockLocation.href = '';
    mockSessionStorage.getItem.mockReturnValue(null);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('credentials and baseUrl', () => {
    it('includes credentials in fetch request', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => ({ data: 'test' }),
        headers: new Headers(),
      });

      await apiClient.get('/api/test');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/v1/test'),
        expect.objectContaining({
          credentials: 'include',
        })
      );
    });

    it('transforms /api/ endpoints to /api/v1/', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => ({ data: 'test' }),
        headers: new Headers(),
      });

      await apiClient.get('/api/Projects');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/v1/Projects'),
        expect.any(Object)
      );
    });

    it('keeps /api/v1/ endpoints as-is', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => ({ data: 'test' }),
        headers: new Headers(),
      });

      await apiClient.get('/api/v1/Projects');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/v1/Projects'),
        expect.any(Object)
      );
    });
  });

  describe('401 Unauthorized handling', () => {
    it('redirects to /login when not on auth pages', async () => {
      mockLocation.pathname = '/dashboard';
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ error: 'Unauthorized' }),
        headers: new Headers(),
        clone: function() {
          return this;
        },
      });

      try {
        await apiClient.get('/api/Projects');
      } catch (error) {
        // Expected to throw
      }

      // Should redirect to login
      expect(mockLocation.href).toBe('/login');
    });

    it('does not redirect when already on login page', async () => {
      mockLocation.pathname = '/login';
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ error: 'Unauthorized' }),
        headers: new Headers(),
        clone: function() {
          return this;
        },
      });

      try {
        await apiClient.get('/api/Projects');
      } catch (error) {
        expect(error).toBeInstanceOf(Error);
        expect((error as Error).message).toBe('Unauthorized');
      }

      // Should not redirect
      expect(mockLocation.href).toBe('');
    });

    it('does not redirect for /Auth/me endpoint (auth check)', async () => {
      mockLocation.pathname = '/dashboard';
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ error: 'Unauthorized' }),
        headers: new Headers(),
        clone: function() {
          return this;
        },
      });

      try {
        await apiClient.get('/api/Auth/me');
      } catch (error) {
        expect(error).toBeInstanceOf(Error);
      }

      // Should not redirect for auth check
      expect(mockLocation.href).toBe('');
    });

    it('prevents infinite redirect loops', async () => {
      mockLocation.pathname = '/dashboard';
      
      // Simulate first 401
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ error: 'Unauthorized' }),
        headers: new Headers(),
        clone: function() {
          return this;
        },
      });

      // First call should redirect
      try {
        await apiClient.get('/api/Projects');
      } catch (error) {
        // Expected
      }

      expect(mockLocation.href).toBe('/login');

      // Reset href for second call
      mockLocation.href = '';
      mockLocation.pathname = '/dashboard'; // Simulate navigation back

      // Second 401 should also redirect (after timeout resets flag)
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ error: 'Unauthorized' }),
        headers: new Headers(),
        clone: function() {
          return this;
        },
      });

      // Wait a bit for the timeout to reset
      await new Promise(resolve => setTimeout(resolve, 150));

      try {
        await apiClient.get('/api/Projects');
      } catch (error) {
        // Expected
      }

      // Should redirect again after timeout
      expect(mockLocation.href).toBe('/login');
    });

    it('extracts error message from 401 response', async () => {
      mockLocation.pathname = '/login';
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ detail: 'Token expired' }),
        headers: new Headers(),
        clone: function() {
          return this;
        },
      });

      await expect(apiClient.get('/api/Projects')).rejects.toThrow('Token expired');
    });
  });

  describe('429 Rate Limit handling', () => {
    it('reads retryAfter from response body', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 429,
        json: async () => ({ error: 'Too many requests', retryAfter: 120 }),
        headers: new Headers(),
        clone: function() {
          return this;
        },
      });

      await expect(apiClient.get('/api/Projects')).rejects.toThrow(/120 seconds/);
    });

    it('reads retryAfter from Retry-After header', async () => {
      const headers = new Headers();
      headers.set('Retry-After', '90');
      
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 429,
        json: async () => ({ error: 'Too many requests' }),
        headers,
        clone: function() {
          return this;
        },
      });

      await expect(apiClient.get('/api/Projects')).rejects.toThrow(/90 seconds/);
    });

    it('prefers header retryAfter over body retryAfter', async () => {
      const headers = new Headers();
      headers.set('Retry-After', '45');
      
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 429,
        json: async () => ({ error: 'Too many requests', retryAfter: 120 }),
        headers,
        clone: function() {
          return this;
        },
      });

      await expect(apiClient.get('/api/Projects')).rejects.toThrow(/45 seconds/);
    });

    it('uses default 60 seconds if no retryAfter provided', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 429,
        json: async () => ({ error: 'Too many requests' }),
        headers: new Headers(),
        clone: function() {
          return this;
        },
      });

      await expect(apiClient.get('/api/Projects')).rejects.toThrow(/60 seconds/);
    });
  });

  describe('4xx/5xx error handling', () => {
    it('extracts error message from 400 response', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 400,
        json: async () => ({ detail: 'Validation failed', errors: { name: ['Name is required'] } }),
        headers: new Headers(),
      });

      await expect(apiClient.get('/api/Projects')).rejects.toThrow('Name is required');
    });

    it('extracts error message from 404 response', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 404,
        json: async () => ({ title: 'Not Found', detail: 'Resource not found' }),
        headers: new Headers(),
      });

      await expect(apiClient.get('/api/Projects/999')).rejects.toThrow('Resource not found');
    });

    it('extracts error message from 500 response', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: async () => ({ error: 'Internal server error', message: 'Something went wrong' }),
        headers: new Headers(),
      });

      await expect(apiClient.get('/api/Projects')).rejects.toThrow('Something went wrong');
    });

    it('handles non-JSON error responses gracefully', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: async () => {
          throw new Error('Not JSON');
        },
        headers: new Headers(),
      });

      await expect(apiClient.get('/api/Projects')).rejects.toThrow('Request failed');
    });

    it('handles 204 No Content response', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
        headers: new Headers(),
      });

      const result = await apiClient.delete('/api/Projects/1');
      expect(result).toEqual({});
    });
  });

  describe('request methods', () => {
    it('sends GET request correctly', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => ({ data: 'test' }),
        headers: new Headers(),
      });

      await apiClient.get('/api/test');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          method: 'GET',
        })
      );
    });

    it('sends POST request with body', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 201,
        json: async () => ({ id: 1 }),
        headers: new Headers(),
      });

      await apiClient.post('/api/test', { name: 'Test' });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify({ name: 'Test' }),
        })
      );
    });

    it('sends PUT request with body', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => ({ id: 1 }),
        headers: new Headers(),
      });

      await apiClient.put('/api/test/1', { name: 'Updated' });

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          method: 'PUT',
          body: JSON.stringify({ name: 'Updated' }),
        })
      );
    });

    it('sends DELETE request correctly', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
        headers: new Headers(),
      });

      await apiClient.delete('/api/test/1');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          method: 'DELETE',
        })
      );
    });
  });
});

