import React from 'react';
import { AiRecommendationResult } from '../../types/ai';
import { AiUnavailableBanner } from './AiUnavailableBanner';
import { formatRelativeTime } from '../../lib/formatRelativeTime';
import { Bot, RefreshCw, AlertCircle } from 'lucide-react';

interface AiRecommendationCardProps {
    title: string;
    data: AiRecommendationResult | undefined;
    isLoading: boolean;
}

export const AiRecommendationCard: React.FC<AiRecommendationCardProps> = ({ title, data, isLoading }) => {
    return (
        <div className="bg-white rounded-lg shadow-sm border border-slate-200 overflow-hidden mb-6">
            <div className="bg-slate-50 px-4 py-3 border-b border-slate-200 flex items-center gap-2">
                <Bot className="text-indigo-500" size={18} />
                <h3 className="font-semibold text-slate-800">{title}</h3>
            </div>
            
            <div className="p-4">
                {isLoading ? (
                    <div className="animate-pulse flex flex-col gap-3">
                        <div className="flex gap-2 items-center">
                            <div className="h-2 w-2 bg-slate-200 rounded-full"></div>
                            <div className="h-4 bg-slate-200 rounded w-3/4"></div>
                        </div>
                        <div className="flex gap-2 items-center">
                            <div className="h-2 w-2 bg-slate-200 rounded-full"></div>
                            <div className="h-4 bg-slate-200 rounded w-5/6"></div>
                        </div>
                    </div>
                ) : !data ? (
                    <div className="text-sm text-text-muted italic">No recommendations available.</div>
                ) : !data.isAvailable ? (
                    <AiUnavailableBanner className="mt-0 mb-0" />
                ) : (
                    <div className="flex flex-col gap-4">
                        <ul className="list-disc pl-5 space-y-2 text-sm text-slate-700">
                            {data.recommendations.map((rec, index) => (
                                <li key={index} className="leading-relaxed">
                                    {rec}
                                </li>
                            ))}
                        </ul>
                        
                        <div className="flex justify-end items-center gap-1 text-xs text-text-muted border-t border-slate-100 pt-3">
                            <RefreshCw size={12} />
                            <span>Generated {formatRelativeTime(data.generatedAt)}</span>
                        </div>
                    </div>
                )}
            </div>

            {/* Persistent Disclaimer (Unconditionally shown if we have data) */}
            {data && data.isAvailable && data.disclaimer && (
                <div className="bg-amber-50/50 border-t border-slate-200 px-4 py-2.5 flex items-start gap-2 text-xs text-slate-600">
                    <AlertCircle className="text-amber-500 shrink-0 mt-0.5" size={14} />
                    <p>{data.disclaimer}</p>
                </div>
            )}
        </div>
    );
};
