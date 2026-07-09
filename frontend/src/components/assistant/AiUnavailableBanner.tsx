import React from 'react';
import { AlertTriangle } from 'lucide-react';

interface AiUnavailableBannerProps {
    className?: string;
}

export const AiUnavailableBanner: React.FC<AiUnavailableBannerProps> = ({ className = '' }) => {
    return (
        <div className={`bg-amber-50 border border-amber-200 rounded-md p-4 flex gap-3 text-amber-800 ${className}`}>
            <AlertTriangle className="text-amber-600 shrink-0" size={20} />
            <div className="text-sm">
                <p className="font-semibold mb-1">AI Features Unavailable</p>
                <p>An AI provider is not currently configured for this environment. AI features have been safely disabled.</p>
            </div>
        </div>
    );
};
