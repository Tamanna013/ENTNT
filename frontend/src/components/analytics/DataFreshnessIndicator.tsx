import React from 'react';
import { Clock } from 'lucide-react';
import { formatRelativeTime } from '../../lib/formatRelativeTime';

interface DataFreshnessIndicatorProps {
  generatedAt: string;
}

export const DataFreshnessIndicator: React.FC<DataFreshnessIndicatorProps> = ({ generatedAt }) => {
  return (
    <div className="flex items-center text-xs text-gray-500">
      <Clock className="w-3.5 h-3.5 mr-1" />
      <span>Data as of {formatRelativeTime(generatedAt)}</span>
    </div>
  );
};
