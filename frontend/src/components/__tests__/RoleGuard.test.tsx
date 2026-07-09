import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { RoleGuard } from '../RoleGuard';
import { useAuthStore } from '../../store/authStore';

/*
 * These tests verify RoleGuard's UX-layer rendering behavior only.
 * The actual security enforcement for every one of this application's
 * protected actions lives server-side and is comprehensively covered
 * by the backend's RoleBasedAccessMatrixTests - this file protects the
 * correctness of the CLIENT-SIDE experience, not the application's
 * real security boundary.
 */

describe('RoleGuard', () => {
    beforeEach(() => {
        useAuthStore.getState().clearAuth();
    });

    it('does not render children when user roles do not match allowedRoles', () => {
        useAuthStore.getState().setAuth('mock', { id: '1', email: 'test@example.com', firstName: 'T', lastName: 'U', roles: ['User'] });
        render(<RoleGuard allowedRoles={['Admin']}><div>Admin Content</div></RoleGuard>);
        expect(screen.queryByText('Admin Content')).not.toBeInTheDocument();
    });

    it('renders children when user roles match allowedRoles', () => {
        useAuthStore.getState().setAuth('mock', { id: '1', email: 'test@example.com', firstName: 'T', lastName: 'U', roles: ['Admin'] });
        render(<RoleGuard allowedRoles={['Admin']}><div>Admin Content</div></RoleGuard>);
        expect(screen.getByText('Admin Content')).toBeInTheDocument();
    });

    it('renders children for multi-role user if at least one role matches', () => {
        useAuthStore.getState().setAuth('mock', { id: '1', email: 'test@example.com', firstName: 'T', lastName: 'U', roles: ['FleetManager', 'User'] });
        render(<RoleGuard allowedRoles={['Admin', 'FleetManager']}><div>Admin Content</div></RoleGuard>);
        expect(screen.getByText('Admin Content')).toBeInTheDocument();
    });

    it('renders fallback content when user roles do not match and fallback is provided', () => {
        useAuthStore.getState().setAuth('mock', { id: '1', email: 'test@example.com', firstName: 'T', lastName: 'U', roles: ['User'] });
        render(<RoleGuard allowedRoles={['Admin']} fallback={<div>Access Denied</div>}><div>Admin Content</div></RoleGuard>);
        expect(screen.getByText('Access Denied')).toBeInTheDocument();
        expect(screen.queryByText('Admin Content')).not.toBeInTheDocument();
    });
});
