import React from 'react';
import { AiSummaryResult } from '../../types/ai';
import { AiUnavailableBanner } from './AiUnavailableBanner';
import { formatRelativeTime } from '../../lib/formatRelativeTime';
import { Bot, RefreshCw } from 'lucide-react';

interface AiSummaryCardProps {
    title: string;
    data: AiSummaryResult | undefined;
    isLoading: boolean;
}

export const AiSummaryCard: React.FC<AiSummaryCardProps> = ({ title, data, isLoading }) => {
    return (
        <div className="bg-white rounded-lg shadow-sm border border-slate-200 overflow-hidden mb-6">
            <div className="bg-slate-50 px-4 py-3 border-b border-slate-200 flex items-center gap-2">
                <Bot className="text-indigo-500" size={18} />
                <h3 className="font-semibold text-slate-800">{title}</h3>
            </div>
            <div className="p-4">
                {isLoading ? (
                    <div className="animate-pulse flex flex-col gap-2">
                        <div className="h-4 bg-slate-200 rounded w-3/4"></div>
                        <div className="h-4 bg-slate-200 rounded w-full"></div>
                        <div className="h-4 bg-slate-200 rounded w-5/6"></div>
                    </div>
                ) : !data ? (
                    <div className="text-sm text-text-muted italic">No summary available.</div>
                ) : !data.isAvailable ? (
                    <AiUnavailableBanner className="mt-0 mb-0" />
                ) : (
                    <div className="flex flex-col gap-3">
                        <p className="text-slate-700 leading-relaxed text-sm whitespace-pre-wrap">{data.summary}</p>
                        <div className="flex justify-end items-center gap-1 text-xs text-text-muted">
                            <RefreshCw size={12} />
                            <span>Generated {formatRelativeTime(data.generatedAt)}</span>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};
