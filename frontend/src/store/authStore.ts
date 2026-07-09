import { create } from 'zustand';
import { UserDto } from '../types/auth';

interface AuthState {
    accessToken: string | null;
    user: UserDto | null;
    isAuthenticated: boolean;
    setAuth: (accessToken: string, user: UserDto) => void;
    clearAuth: () => void;
    updateUserDisplayFields: (firstName: string, lastName: string, phoneNumber?: string) => void;
}

// NOTE: We deliberately do NOT persist this store to localStorage or sessionStorage.
// The access token is kept entirely in-memory to reduce the risk of XSS token theft.
// On a hard refresh, the in-memory token is lost, triggering a silent refresh 
// via the HttpOnly refresh cookie (handled by Axios interceptors).
export const useAuthStore = create<AuthState>((set) => ({
    accessToken: null,
    user: null,
    isAuthenticated: false,
    
    setAuth: (accessToken, user) => 
        set({ 
            accessToken, 
            user, 
            isAuthenticated: true 
        }),
        
    clearAuth: () => 
        set({ 
            accessToken: null, 
            user: null, 
            isAuthenticated: false 
        }),

    updateUserDisplayFields: (firstName, lastName, phoneNumber) => 
        set((state) => ({
            user: state.user ? { ...state.user, firstName, lastName, phoneNumber: phoneNumber ?? null } : null
        })),
}));
