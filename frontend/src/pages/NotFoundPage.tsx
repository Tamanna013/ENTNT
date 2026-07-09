import React from 'react';
import { Link } from 'react-router-dom';

export const NotFoundPage: React.FC = () => {
    return (
        <div className="flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
            <h1 className="text-4xl font-bold text-text-primary mb-4">404</h1>
            <p className="text-xl text-text-primary mb-8">Page not found</p>
            <p className="text-text-muted mb-8 max-w-md">
                The page you are looking for doesn't exist or is still under construction.
            </p>
            <Link 
                to="/dashboard" 
                className="bg-indigo-600 hover:bg-indigo-700 text-white px-6 py-2 rounded-md font-medium transition-colors"
            >
                Back to Dashboard
            </Link>
        </div>
    );
};
