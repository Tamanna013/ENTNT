import React from 'react';
import { createBrowserRouter, Navigate } from 'react-router-dom';
import { PublicRoute } from './PublicRoute';
import { ProtectedRoute } from './ProtectedRoute';
import { AuthLayout } from '../layouts/AuthLayout';
import { AppShellLayout } from '../layouts/AppShellLayout';

const LoginPage = React.lazy(() => import('../pages/auth/LoginPage').then(m => ({ default: m.LoginPage })));
const RegisterPage = React.lazy(() => import('../pages/auth/RegisterPage').then(m => ({ default: m.RegisterPage })));
const ForgotPasswordPage = React.lazy(() => import('../pages/auth/ForgotPasswordPage').then(m => ({ default: m.ForgotPasswordPage })));
const ResetPasswordPage = React.lazy(() => import('../pages/auth/ResetPasswordPage').then(m => ({ default: m.ResetPasswordPage })));
const DashboardPage = React.lazy(() => import('../pages/DashboardPage').then(m => ({ default: m.DashboardPage })));
const NotFoundPage = React.lazy(() => import('../pages/NotFoundPage').then(m => ({ default: m.NotFoundPage })));
const UsersPage = React.lazy(() => import('../pages/users/UsersPage').then(m => ({ default: m.UsersPage })));
const FleetsPage = React.lazy(() => import('../pages/fleets/FleetsPage').then(m => ({ default: m.FleetsPage })));
const FleetDetailPage = React.lazy(() => import('../pages/fleets/FleetDetailPage').then(m => ({ default: m.FleetDetailPage })));
const ShipsPage = React.lazy(() => import('../pages/ships/ShipsPage').then(m => ({ default: m.ShipsPage })));
const ShipDetailPage = React.lazy(() => import('../pages/ships/ShipDetailPage').then(m => ({ default: m.ShipDetailPage })));
const CrewPage = React.lazy(() => import('../pages/crew/CrewPage').then(m => ({ default: m.CrewPage })));
const CrewDetailPage = React.lazy(() => import('../pages/crew/CrewDetailPage').then(m => ({ default: m.CrewDetailPage })));
const VoyagesPage = React.lazy(() => import('../pages/voyages/VoyagesPage').then(m => ({ default: m.VoyagesPage })));
const VoyageDetailPage = React.lazy(() => import('../pages/voyages/VoyageDetailPage').then(m => ({ default: m.VoyageDetailPage })));
const PortsPage = React.lazy(() => import('../pages/ports/PortsPage').then(m => ({ default: m.PortsPage })));
const MaintenancePage = React.lazy(() => import('../pages/maintenance/MaintenancePage').then(m => ({ default: m.MaintenancePage })));
const MaintenanceDetailPage = React.lazy(() => import('../pages/maintenance/MaintenanceDetailPage').then(m => ({ default: m.MaintenanceDetailPage })));
const CargoPage = React.lazy(() => import('../pages/cargo/CargoPage').then(m => ({ default: m.CargoPage })));
const CargoDetailPage = React.lazy(() => import('../pages/cargo/CargoDetailPage').then(m => ({ default: m.CargoDetailPage })));
const SearchResultsPage = React.lazy(() => import('../pages/search/SearchResultsPage').then(m => ({ default: m.SearchResultsPage })));
const ContainersPage = React.lazy(() => import('../pages/containers/ContainersPage').then(m => ({ default: m.ContainersPage })));
const ContainerDetailPage = React.lazy(() => import('../pages/containers/ContainerDetailPage').then(m => ({ default: m.ContainerDetailPage })));
const FuelPage = React.lazy(() => import('../pages/fuel/FuelPage').then(m => ({ default: m.FuelPage })));
const NotificationsPage = React.lazy(() => import('../pages/notifications/NotificationsPage').then(m => ({ default: m.NotificationsPage })));
const IncidentsPage = React.lazy(() => import('../pages/incidents/IncidentsPage').then(m => ({ default: m.IncidentsPage })));
const IncidentDetailPage = React.lazy(() => import('../pages/incidents/IncidentDetailPage').then(m => ({ default: m.IncidentDetailPage })));
const DocumentsPage = React.lazy(() => import('../pages/documents/DocumentsPage').then(m => ({ default: m.DocumentsPage })));
const DocumentDetailPage = React.lazy(() => import('../pages/documents/DocumentDetailPage').then(m => ({ default: m.DocumentDetailPage })));
const AuditLogsPage = React.lazy(() => import('../pages/audit-logs/AuditLogsPage').then(m => ({ default: m.AuditLogsPage })));
const AnalyticsPage = React.lazy(() => import('../pages/analytics/AnalyticsPage').then(m => ({ default: m.AnalyticsPage })));
const AiUsagePage = React.lazy(() => import('../pages/ai-usage/AiUsagePage').then(m => ({ default: m.AiUsagePage })));
const AiAssistantPage = React.lazy(() => import('../pages/assistant/AiAssistantPage').then(m => ({ default: m.AiAssistantPage })));
const ProfilePage = React.lazy(() => import('../pages/profile/ProfilePage').then(m => ({ default: m.ProfilePage })));
const SettingsPage = React.lazy(() => import('../pages/settings/SettingsPage').then(m => ({ default: m.SettingsPage })));
import { RoleGuard } from '../components/RoleGuard';
import { AppRoles } from '../lib/constants';

export const router = createBrowserRouter([
    {
        path: '/',
        element: <Navigate to="/dashboard" replace />
    },
    {
        // Public routes
        element: <PublicRoute />,
        children: [
            {
                element: <AuthLayout />,
                children: [
                    { path: 'login', element: <LoginPage /> },
                    { path: 'register', element: <RegisterPage /> },
                    { path: 'forgot-password', element: <ForgotPasswordPage /> },
                    { path: 'reset-password', element: <ResetPasswordPage /> }
                ]
            }
        ]
    },
    {
        // Protected routes
        element: <ProtectedRoute />,
        children: [
            {
                element: <AppShellLayout />,
                children: [
                    { path: 'dashboard', element: <DashboardPage /> },
                    { path: 'analytics', element: <AnalyticsPage /> },
                    { path: 'fleets', element: <FleetsPage /> },
                    { path: 'fleets/:id', element: <FleetDetailPage /> },
                    { path: 'ships', element: <ShipsPage /> },
                    { path: 'ships/:id', element: <ShipDetailPage /> },
                    { path: 'maintenance', element: <MaintenancePage /> },
                    { path: 'maintenance/:id', element: <MaintenanceDetailPage /> },
                    { path: 'fuel', element: <FuelPage /> },
                    { path: 'notifications', element: <NotificationsPage /> },
                    { path: 'incidents', element: <IncidentsPage /> },
                    { path: 'incidents/:id', element: <IncidentDetailPage /> },
                    { path: 'documents', element: <DocumentsPage /> },
                    { path: 'documents/:id', element: <DocumentDetailPage /> },
                    { path: 'ports', element: <PortsPage /> },
                    { path: 'crew', element: <CrewPage /> },
                    { path: 'crew/:id', element: <CrewDetailPage /> },
                    { path: 'voyages', element: <VoyagesPage /> },
                    { path: 'voyages/:id', element: <VoyageDetailPage /> },
                    { path: 'cargo', element: <CargoPage /> },
                    { path: 'cargo/:id', element: <CargoDetailPage /> },
                    { path: 'search', element: <SearchResultsPage /> },
                    { path: 'containers', element: <ContainersPage /> },
                    { path: 'containers/:id', element: <ContainerDetailPage /> },
                    { 
                          path: 'users', 
                          element: (
                            <RoleGuard 
                              allowedRoles={[AppRoles.Admin]}
                              fallback={
                                <div className="p-8 text-center text-red-500">
                                  You do not have permission to access the User Management area.
                                </div>
                              }
                            >
                              <UsersPage />
                            </RoleGuard>
                          )
                      },
                      { 
                          path: 'audit-logs', 
                          element: (
                            <RoleGuard 
                              allowedRoles={[AppRoles.Admin]}
                              fallback={
                                <div className="p-8 text-center text-red-500">
                                  You do not have permission to access the Audit Logs area.
                                </div>
                              }
                            >
                              <AuditLogsPage />
                            </RoleGuard>
                          )
                      },
                      { 
                          path: 'ai-usage', 
                          element: (
                            <RoleGuard 
                              allowedRoles={[AppRoles.Admin]}
                              fallback={
                                <div className="p-8 text-center text-red-500">
                                  You do not have permission to access the AI Usage area.
                                </div>
                              }
                            >
                              <AiUsagePage />
                            </RoleGuard>
                          )
                      },
                      { path: 'ai-assistant', element: <AiAssistantPage /> },
                      { path: 'profile', element: <ProfilePage /> },
                      { path: 'settings', element: <SettingsPage /> },
                    { path: '*', element: <NotFoundPage /> } // Catch-all for undefined module routes inside the app
                ]
            }
        ]
    },
    {
        // Absolute fallback catch-all
        path: '*',
        element: <Navigate to="/dashboard" replace />
    }
]);
