import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { User, LogOut, ChevronDown, Menu, Search } from 'lucide-react';
import { useAuthStore } from '../../store/authStore';
import { authApi } from '../../api/authApi';
import { NotificationBell } from './NotificationBell';
import { GlobalSearchBar } from './GlobalSearchBar';
import { useMediaQuery } from '../../hooks/useMediaQuery';

interface TopbarProps {
    onOpenDrawer?: () => void;
}

export const Topbar: React.FC<TopbarProps> = ({ onOpenDrawer }) => {
    const { user, clearAuth } = useAuthStore();
    const navigate = useNavigate();
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const [isSearchExpanded, setIsSearchExpanded] = useState(false);
    const menuRef = useRef<HTMLDivElement>(null);
    const isMobile = useMediaQuery('(max-width: 768px)');

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
                setIsMenuOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const handleLogout = async () => {
        try {
            await authApi.logout();
        } catch (error) {
            console.error('Logout API failed, continuing client-side clear', error);
        } finally {
            clearAuth();
            navigate('/login');
        }
    };

    return (
        <header className={`h-16 fixed top-0 right-0 ${isMobile ? 'left-0' : 'left-64'} bg-surface border-b border-border z-10 flex items-center justify-between px-4 sm:px-6 transition-all duration-300`}>
            {isMobile && !isSearchExpanded && (
                <button
                    onClick={onOpenDrawer}
                    className="p-2 -ml-2 mr-2 text-text-muted hover:text-text-primary rounded-md focus:outline-none focus:ring-2 focus:ring-inset focus:ring-blue-500"
                >
                    <span className="sr-only">Open sidebar</span>
                    <Menu className="h-6 w-6" aria-hidden="true" />
                </button>
            )}

            <div className={`flex-1 flex justify-end sm:justify-center px-2 sm:px-4 ${isSearchExpanded ? 'w-full absolute left-0 right-0 px-4 bg-surface z-50 h-16 items-center' : ''}`}>
                {isMobile ? (
                    isSearchExpanded ? (
                        <div className="flex w-full items-center gap-2">
                            <GlobalSearchBar />
                            <button 
                                onClick={() => setIsSearchExpanded(false)}
                                className="p-2 text-text-muted hover:text-text-primary"
                            >
                                Cancel
                            </button>
                        </div>
                    ) : (
                        <button
                            onClick={() => setIsSearchExpanded(true)}
                            className="p-2 text-text-muted hover:text-text-primary rounded-md"
                        >
                            <Search className="h-5 w-5" />
                        </button>
                    )
                ) : (
                    <GlobalSearchBar />
                )}
            </div>

            {!isSearchExpanded && (
                <div className="flex items-center space-x-2 sm:space-x-4">
                    <NotificationBell />
                    <div className="relative" ref={menuRef}>
                <button 
                    onClick={() => setIsMenuOpen(!isMenuOpen)}
                    className="flex items-center gap-2 text-text-muted hover:text-text-primary transition-colors p-2 rounded-md hover:bg-surface-hover"
                >
                    <User size={20} />
                    <span className="font-medium text-sm hidden sm:inline-block">
                        {user?.firstName} {user?.lastName}
                    </span>
                    <ChevronDown size={16} className={`transition-transform ${isMenuOpen ? 'rotate-180' : ''}`} />
                </button>

                {isMenuOpen && (
              <div className="absolute right-0 mt-2 w-48 bg-surface border border-border rounded-md shadow-lg py-1 z-50">
                <button
                  onClick={() => {
                    navigate('/profile');
                    setIsMenuOpen(false);
                  }}
                  className="w-full text-left px-4 py-2 text-sm text-text-primary hover:bg-surface-hover flex items-center gap-2"
                >
                  <User size={16} /> My Profile
                </button>
                <div className="border-t border-border my-1"></div>
                <button
                  onClick={handleLogout}
                  className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 flex items-center gap-2"
                >
                  <LogOut size={16} /> Logout
                </button>
              </div>
            )}
            </div>
            </div>
            )}
        </header>
    );
};
