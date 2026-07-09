import React, { ReactNode } from 'react';

interface DashboardSectionProps {
  title: string;
  children: ReactNode;
}

export const DashboardSection: React.FC<DashboardSectionProps> = ({ title, children }) => {
  return (
    <section className="mb-8">
      <h2 className="text-xl font-bold text-slate-100 mb-4 pb-2 border-b border-slate-700/50">
        {title}
      </h2>
      <div className="space-y-6">
        {children}
      </div>
    </section>
  );
};
