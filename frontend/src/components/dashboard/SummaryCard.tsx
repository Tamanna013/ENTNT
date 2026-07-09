import React from 'react';
import { LucideIcon } from 'lucide-react';

interface SummaryCardProps {
  icon: LucideIcon;
  label: string;
  value: string | number;
}

export const SummaryCard: React.FC<SummaryCardProps> = ({ icon: Icon, label, value }) => {
  return (
    <div className="bg-surface rounded-lg p-6 flex items-center border border-border">
      <div className="rounded-full bg-primary-500/20 p-3 mr-4">
        <Icon className="text-primary-400" size={24} />
      </div>
      <div>
        <div className="text-3xl font-bold text-text-primary">{value}</div>
        <div className="text-sm text-text-muted font-medium">{label}</div>
      </div>
    </div>
  );
};
