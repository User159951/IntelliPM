import { apiClient } from './client';
import type { LoginRequest, RegisterRequest, AuthResponse, User } from '@/types';

export interface AcceptInviteRequest {
  token: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AcceptOrganizationInviteRequest {
  token: string;
  username: string;
  password: string;
  confirmPassword: string;
}

export interface ValidateInviteTokenResponse {
  email: string;
  organizationName: string;
}

export const authApi = {
  login: (data: LoginRequest): Promise<AuthResponse> =>
    apiClient.post('/Auth/login', data),

  /**
   * Register a new user (DEPRECATED)
   * 
   * @deprecated Public registration is disabled. This endpoint returns 403 Forbidden.
   * Please contact your administrator for an invitation link instead.
   * 
   * This method is kept for backward compatibility but will be removed in a future version.
   * Use invitation-based registration instead:
   * - Contact your administrator to receive an invitation link
   * - Use `validateInviteToken()` and `acceptInvite()` or `acceptOrganizationInvite()` to complete registration
   * 
   * @param data - Registration data (username, email, password, firstName, lastName)
   * @returns AuthResponse (will always fail with 403)
   * @throws {Error} Always throws with 403 Forbidden error
   * 
   * @example
   * // ❌ Don't use this - will always fail
   * try {
   *   await authApi.register({ username: 'user', email: 'user@example.com', ... });
   * } catch (error) {
   *   // Will always fail with 403
   * }
   * 
   * // ✅ Use invitation instead
   * const inviteInfo = await authApi.validateInviteToken(token);
   * await authApi.acceptInvite({ token, password, firstName, lastName });
   */
  register: (data: RegisterRequest): Promise<AuthResponse> =>
    apiClient.post('/Auth/register', data),

  logout: (): Promise<void> =>
    apiClient.post('/Auth/logout'),

  getMe: (): Promise<User> =>
    apiClient.get('/Auth/me'),

  refresh: (): Promise<void> =>
    apiClient.post('/Auth/refresh'),

  validateInviteToken: (token: string): Promise<ValidateInviteTokenResponse> =>
    apiClient.get(`/Auth/invite/${encodeURIComponent(token)}`),

  acceptInvite: (data: AcceptInviteRequest): Promise<AuthResponse> =>
    apiClient.post('/Auth/invite/accept', data),

  acceptOrganizationInvite: (data: AcceptOrganizationInviteRequest): Promise<AuthResponse> =>
    apiClient.post('/Auth/invite/accept', data),

  inviteUser: (data: { email: string; globalRole: string }): Promise<{ invitationId: number; email: string; token: string; expiresAt: string }> =>
    apiClient.post('/Auth/invite', data),

  requestPasswordReset: (data: { emailOrUsername: string }): Promise<{ success: boolean; message: string }> =>
    apiClient.post('/Auth/forgot-password', data),

  resetPassword: (data: { token: string; newPassword: string; confirmPassword: string }): Promise<{ success: boolean; message: string }> =>
    apiClient.post('/Auth/reset-password', data),
};
