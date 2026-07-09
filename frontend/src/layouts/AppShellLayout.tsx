import React from 'react';
import { Outlet } from 'react-router-dom';
import { Sidebar } from '../components/layout/Sidebar';
import { Topbar } from '../components/layout/Topbar';
import { SkipLink } from '../components/layout/SkipLink';
import { useFocusMainOnRouteChange } from '../hooks/useFocusMainOnRouteChange';
import { useMediaQuery } from '../hooks/useMediaQuery';
import { MobileDrawer } from '../components/layout/MobileDrawer';
import { useState } from 'react';

export const AppShellLayout: React.FC = () => {
    useFocusMainOnRouteChange();
    const isMobile = useMediaQuery('(max-width: 768px)');
    const [isDrawerOpen, setIsDrawerOpen] = useState(false);

    return (
        <div className="min-h-screen bg-background text-text-primary font-sans">
            <SkipLink />
            
            {isMobile ? (
                <MobileDrawer isOpen={isDrawerOpen} onClose={() => setIsDrawerOpen(false)}>
                    <Sidebar onLinkClick={() => setIsDrawerOpen(false)} />
                </MobileDrawer>
            ) : (
                <Sidebar />
            )}

            <Topbar onOpenDrawer={() => setIsDrawerOpen(true)} />
            
            <main id="main-content" tabIndex={-1} className={`${isMobile ? 'ml-0' : 'ml-64'} pt-16 min-h-screen p-6 focus:outline-none transition-all duration-300`}>
                <Outlet />
            </main>
        </div>
    );
};
