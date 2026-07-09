import React from 'react';

type BadgeColor = 'blue' | 'green' | 'red' | 'gray' | 'yellow' | 'purple';

interface BadgeProps {
  text: string;
  color?: BadgeColor;
}

export const Badge: React.FC<BadgeProps> = ({ text, color = 'gray' }) => {
  const colorStyles = {
    blue: 'bg-blue-500/10 text-blue-400 ring-blue-500/20',
    green: 'bg-emerald-500/10 text-emerald-400 ring-emerald-500/20',
    red: 'bg-rose-500/10 text-rose-400 ring-rose-500/20',
    gray: 'bg-slate-500/10 text-text-muted ring-slate-500/20',
    yellow: 'bg-yellow-500/10 text-yellow-400 ring-yellow-500/20',
    purple: 'bg-purple-500/10 text-purple-400 ring-purple-500/20',
  };

  return (
    <span className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ring-1 ring-inset ${colorStyles[color]}`}>
      {text}
    </span>
  );
};
