import { useState, useEffect } from 'react';
import axios from 'axios';
import { useAuthStore } from '../store/authStore';

export const useBootstrapSession = () => {
    const [isBootstrapping, setIsBootstrapping] = useState(true);
    const setAuth = useAuthStore(state => state.setAuth);

    useEffect(() => {
        const bootstrap = async () => {
            try {
                // Call /auth/refresh via plain axios to avoid infinite interceptor loops
                // Need withCredentials to send the HttpOnly cookie
                const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api/v1';
                const response = await axios.post(`${baseURL}/auth/refresh`, {}, {
                    withCredentials: true
                });
                
                const { accessToken, user } = response.data;
                setAuth(accessToken, user);
            } catch (error) {
                // Expected if not logged in - do nothing, leave store unauthenticated.
            } finally {
                setIsBootstrapping(false);
            }
        };

        bootstrap();
    }, [setAuth]);

    return isBootstrapping;
};
