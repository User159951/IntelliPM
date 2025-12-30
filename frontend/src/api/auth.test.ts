import { describe, it, expect, beforeEach } from 'vitest';
import { http, HttpResponse } from 'msw';
import { server } from '@/mocks/server';
import { authApi } from './auth';
import type { LoginRequest, RegisterRequest } from '@/types';

describe('authApi', () => {
  beforeEach(() => {
    server.resetHandlers();
  });

  describe('login', () => {
    it('successfully logs in with valid credentials', async () => {
      const loginData: LoginRequest = {
        username: 'testuser',
        password: 'password123',
      };

      const response = await authApi.login(loginData);

      expect(response).toHaveProperty('userId');
      expect(response).toHaveProperty('username');
      expect(response.username).toBe('testuser');
    });

    it('handles login failure', async () => {
      // Override handler for this test
      server.use(
        http.post('http://localhost:5001/api/v1/Auth/login', () => {
          return HttpResponse.json(
            { error: 'Invalid credentials' },
            { status: 401 }
          );
        })
      );

      const loginData: LoginRequest = {
        username: 'wronguser',
        password: 'wrongpass',
      };

      await expect(authApi.login(loginData)).rejects.toThrow();
    });
  });

  describe('register', () => {
    it('successfully registers a new user', async () => {
      const registerData: RegisterRequest = {
        username: 'newuser',
        email: 'newuser@example.com',
        password: 'password123',
        firstName: 'New',
        lastName: 'User',
      };

      const response = await authApi.register(registerData);

      expect(response).toHaveProperty('userId');
      expect(response).toHaveProperty('username');
      expect(response.username).toBe('newuser');
    });

    it('handles registration failure', async () => {
      server.use(
        http.post('http://localhost:5001/api/v1/Auth/register', () => {
          return HttpResponse.json(
            { error: 'Registration failed' },
            { status: 400 }
          );
        })
      );

      const registerData: RegisterRequest = {
        username: '',
        email: '',
        password: '',
        firstName: '',
        lastName: '',
      };

      await expect(authApi.register(registerData)).rejects.toThrow();
    });
  });

  describe('getMe', () => {
    it('fetches current user', async () => {
      const user = await authApi.getMe();

      expect(user).toHaveProperty('userId');
      expect(user).toHaveProperty('username');
      expect(user).toHaveProperty('email');
    });
  });

  describe('logout', () => {
    it('successfully logs out', async () => {
      await expect(authApi.logout()).resolves.not.toThrow();
    });
  });

  describe('refresh', () => {
    it('successfully refreshes token', async () => {
      await expect(authApi.refresh()).resolves.not.toThrow();
    });
  });
});

