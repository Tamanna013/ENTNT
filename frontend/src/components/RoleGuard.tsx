import React from 'react';
import { useAuthStore } from '../store/authStore';

interface RoleGuardProps {
  allowedRoles: string[];
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

/**
 * RoleGuard is a UX convenience component only.
 * It hides UI elements or routes based on the user's roles.
 * REAL ENFORCEMENT HAPPENS SERVER-SIDE via the [Authorize(Policy = "AdminOnly")] attribute.
 * A determined user could bypass this client-side check trivially.
 */
export const RoleGuard: React.FC<RoleGuardProps> = ({ allowedRoles, children, fallback = null }) => {
  const user = useAuthStore((state) => state.user);

  if (!user || !user.roles) {
    return <>{fallback}</>;
  }

  const hasAllowedRole = user.roles.some((role) => allowedRoles.includes(role));

  if (!hasAllowedRole) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
};
