import { apiClient } from './client';
import { AuthResponse, LoginPayload, RegisterPayload } from '../types/auth';

export const authApi = {
    register: async (payload: RegisterPayload): Promise<AuthResponse> => {
        const response = await apiClient.post<AuthResponse>('/auth/register', payload);
        return response.data;
    },

    login: async (payload: LoginPayload): Promise<AuthResponse> => {
        const response = await apiClient.post<AuthResponse>('/auth/login', payload);
        return response.data;
    },

    logout: async (): Promise<void> => {
        await apiClient.post('/auth/logout');
    },

    refresh: async (): Promise<AuthResponse> => {
        const response = await apiClient.post<AuthResponse>('/auth/refresh');
        return response.data;
    },

    forgotPassword: async (email: string): Promise<void> => {
        await apiClient.post('/auth/forgot-password', { email });
    },

    resetPassword: async (token: string, newPassword: string): Promise<void> => {
        await apiClient.post('/auth/reset-password', { token, newPassword });
    },

    verifyEmail: async (token: string): Promise<void> => {
        await apiClient.get(`/auth/verify-email?token=${encodeURIComponent(token)}`);
    },

    changePassword: async (payload: { currentPassword: string; newPassword: string }): Promise<void> => {
        await apiClient.put('/auth/change-password', payload);
    }
};
