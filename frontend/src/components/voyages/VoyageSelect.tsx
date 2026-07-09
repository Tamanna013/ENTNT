import React from 'react';
import { useVoyagesQuery } from '../../hooks/useVoyages';

interface VoyageSelectProps extends Omit<React.SelectHTMLAttributes<HTMLSelectElement>, 'onChange'> {
  value: string;
  onChange: (value: string) => void;
  error?: string;
  label?: string;
}

export const VoyageSelect: React.FC<VoyageSelectProps> = ({ 
  value, 
  onChange, 
  error, 
  label = "Voyage",
  className = "", 
  ...props 
}) => {
  const { data: voyagesData, isLoading } = useVoyagesQuery({
    pageNumber: 1,
    pageSize: 100,
    sortBy: "departureDate",
    sortDescending: true
  });

  return (
    <div className={`flex flex-col ${className}`}>
      {label && (
        <label className="mb-1 block text-sm font-medium text-text-primary">
          {label}
        </label>
      )}
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        disabled={isLoading}
        className={`block w-full rounded-md border-0 py-1.5 text-slate-100 bg-surface shadow-sm ring-1 ring-inset ${ error ? 'ring-red-500 focus:ring-red-500' : 'ring-border focus:ring-indigo-500' } focus:ring-2 focus:ring-inset sm:text-sm sm:leading-6 disabled:opacity-50`}
        {...props}
      >
        <option value="">Select a voyage...</option>
        {voyagesData?.items.map((voyage: any) => (
          <option key={voyage.id} value={voyage.id}>
            {voyage.voyageNumber} ({voyage.departureFrom} → {voyage.destinationTo})
          </option>
        ))}
      </select>
      {error && <p className="mt-1 text-sm text-red-500">{error}</p>}
    </div>
  );
};
