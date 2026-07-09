import React from 'react';
import { Outlet } from 'react-router-dom';

export const AuthLayout: React.FC = () => {
    return (
        <div className="min-h-screen bg-background flex flex-col justify-center items-center p-4">
            <Outlet />
        </div>
    );
};
