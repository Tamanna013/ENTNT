import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { ProtectedRoute } from '../ProtectedRoute';
import { useAuthStore } from '../../store/authStore';

describe('ProtectedRoute', () => {
    beforeEach(() => {
        useAuthStore.getState().clearAuth();
    });

    it('redirects an unauthenticated user to /login and does not render children', () => {
        render(
            <MemoryRouter initialEntries={['/dashboard']}>
                <Routes>
                    <Route path="/login" element={<div>Login Page</div>} />
                    <Route path="/dashboard" element={<ProtectedRoute />}>
                        <Route index element={<div>Dashboard Content</div>} />
                    </Route>
                </Routes>
            </MemoryRouter>
        );

        expect(screen.getByText('Login Page')).toBeInTheDocument();
        expect(screen.queryByText('Dashboard Content')).not.toBeInTheDocument();
    });

    it('renders children correctly for an authenticated user', () => {
        useAuthStore.getState().setAuth('mock-token', {
            id: '1',
            email: 'test@example.com',
            firstName: 'Test',
            lastName: 'User',
            roles: ['User']
        });

        render(
            <MemoryRouter initialEntries={['/dashboard']}>
                <Routes>
                    <Route path="/login" element={<div>Login Page</div>} />
                    <Route path="/dashboard" element={<ProtectedRoute />}>
                        <Route index element={<div>Dashboard Content</div>} />
                    </Route>
                </Routes>
            </MemoryRouter>
        );

        expect(screen.getByText('Dashboard Content')).toBeInTheDocument();
        expect(screen.queryByText('Login Page')).not.toBeInTheDocument();
    });
});
