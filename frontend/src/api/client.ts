import axios from 'axios';
import { useAuthStore } from '../store/authStore';

export const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api/v1',
    // withCredentials is required so the backend's HttpOnly refresh cookie is sent
    withCredentials: true,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor: attach in-memory access token
apiClient.interceptors.request.use(
    (config) => {
        const { accessToken } = useAuthStore.getState();
        if (accessToken) {
            config.headers.Authorization = `Bearer ${accessToken}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response interceptor: handle 401 with silent refresh
apiClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        // Ensure we only retry once and don't intercept auth endpoints to prevent loops
        if (
            error.response?.status === 401 && 
            !originalRequest._retry &&
            originalRequest.url !== '/auth/login' &&
            originalRequest.url !== '/auth/register' &&
            originalRequest.url !== '/auth/refresh'
        ) {
            originalRequest._retry = true;

            try {
                // Use a plain axios call to avoid recursive interception
                const refreshResponse = await axios.post(
                    `${apiClient.defaults.baseURL}/auth/refresh`,
                    {},
                    { withCredentials: true }
                );

                const { accessToken, user } = refreshResponse.data;
                
                // Update the in-memory store
                useAuthStore.getState().setAuth(accessToken, user);

                // Retry original request with new token
                originalRequest.headers.Authorization = `Bearer ${accessToken}`;
                return apiClient(originalRequest);
            } catch (refreshError) {
                // If refresh fails, clear the auth store (force user to log in again)
                useAuthStore.getState().clearAuth();
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);
