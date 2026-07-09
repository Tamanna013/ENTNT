import React from 'react';
import { ThemePreference } from '../../theme/ThemeProvider';
import { Sun, Moon, Monitor } from 'lucide-react';

interface ThemeSelectorProps {
  value: ThemePreference;
  onChange: (value: ThemePreference) => void;
}

export const ThemeSelector: React.FC<ThemeSelectorProps> = ({ value, onChange }) => {
  const options: { label: ThemePreference; icon: React.ReactNode }[] = [
    { label: 'Light', icon: <Sun size={18} /> },
    { label: 'Dark', icon: <Moon size={18} /> },
    { label: 'System', icon: <Monitor size={18} /> }
  ];

  return (
    <div className="flex bg-surface-hover p-1 rounded-lg border border-border w-fit">
      {options.map(option => (
        <button
          key={option.label}
          onClick={() => onChange(option.label)}
          className={`flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-md transition-colors ${
            value === option.label
              ? 'bg-surface text-text-primary shadow-sm ring-1 ring-border'
              : 'text-text-muted hover:text-text-primary'
          }`}
        >
          {option.icon}
          {option.label}
        </button>
      ))}
    </div>
  );
};
