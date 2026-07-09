import React from 'react';
import { NavLink } from 'react-router-dom';
import { LayoutDashboard, Ship, Anchor, Users, Compass, Package, Box, PenTool, Fuel, MapPin, AlertTriangle, FileText, Bell, BarChart3, Bot, Settings, List, UserCog } from 'lucide-react';
import { RoleGuard } from '../RoleGuard';
import { AppRoles } from '../../lib/constants';

const navItems = [
    { label: 'Dashboard', path: '/dashboard', icon: <LayoutDashboard size={20} /> },
    { label: 'Analytics', path: '/analytics', icon: <BarChart3 size={20} /> },
    { label: 'Fleet', path: '/fleets', icon: <Anchor size={20} /> },
    { label: 'Ports', path: '/ports', icon: <MapPin size={20} /> },
    { label: 'Ships', path: '/ships', icon: <Ship size={20} /> },
    { label: 'Crew', path: '/crew', icon: <Users size={20} /> },
    { label: 'Voyages', path: '/voyages', icon: <Compass size={20} /> },
    { label: 'Cargo', path: '/cargo', icon: <Package size={20} /> },
    { label: 'Containers', path: '/containers', icon: <Box size={20} /> },
    { label: 'Maintenance', path: '/maintenance', icon: <PenTool size={20} /> },
    { label: 'Fuel', path: '/fuel', icon: <Fuel size={20} /> },
    { label: 'Incidents', path: '/incidents', icon: <AlertTriangle size={20} /> },
    { label: 'Documents', path: '/documents', icon: <FileText size={20} /> },
    { label: 'Notifications', path: '/notifications', icon: <Bell size={20} /> },
    { label: 'AI Assistant', path: '/ai-assistant', icon: <Bot size={20} /> },
    { label: 'Settings', path: '/settings', icon: <Settings size={20} /> },
    { label: 'Audit Logs', path: '/audit-logs', icon: <List size={20} />, allowedRoles: [AppRoles.Admin] }, // Let's restrict audit logs to admin too
    { label: 'User Management', path: '/users', icon: <UserCog size={20} />, allowedRoles: [AppRoles.Admin] },
    { label: 'AI Usage', path: '/ai-usage', icon: <BarChart3 size={20} />, allowedRoles: [AppRoles.Admin] },
];

interface SidebarProps {
    onLinkClick?: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ onLinkClick }) => {
    return (
        <aside className="w-64 fixed inset-y-0 left-0 bg-surface border-r border-border flex flex-col overflow-y-auto">
            <div className="p-4 flex items-center justify-center border-b border-border">
                <span className="text-xl font-bold text-text-primary tracking-tight">FleetMind AI</span>
            </div>
            
            <nav className="flex-1 py-4 flex flex-col gap-1 px-3">
                {navItems.map((item) => {
                    const navLink = (
                        <NavLink
                            to={item.path}
                            onClick={onLinkClick}
                            className={({ isActive }) => 
                                `flex items-center gap-3 px-3 py-2 rounded-md transition-colors ${
                                    isActive 
                                        ? 'bg-accent/10 text-accent' 
                                        : 'text-text-muted hover:bg-surface-hover hover:text-text-primary'
                                }`
                            }
                        >
                            {item.icon}
                            <span className="font-medium text-sm">{item.label}</span>
                        </NavLink>
                    );
                    
                    if (item.allowedRoles) {
                        return (
                            <RoleGuard key={item.label} allowedRoles={item.allowedRoles}>
                                {navLink}
                            </RoleGuard>
                        );
                    }
                    
                    return <React.Fragment key={item.label}>{navLink}</React.Fragment>;
                })}
            </nav>
        </aside>
    );
};
