import React from 'react';
import { useFleetsQuery } from '../../hooks/useFleets';

interface FleetSelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
}

export const FleetSelect = React.forwardRef<HTMLSelectElement, FleetSelectProps>(
  ({ label, error, className = '', ...props }, ref) => {
    const { data } = useFleetsQuery({ pageNumber: 1, pageSize: 100, sortBy: 'name' });
    const fleets = data?.items || [];

    return (
      <div className="space-y-1">
        {label && (
          <label className="block text-sm font-medium text-text-primary">
            {label}
          </label>
        )}
        <select
          ref={ref}
          className={`w-full bg-background border ${ error ? 'border-red-500' : 'border-border' } rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500 ${className}`}
          {...props}
        >
          <option value="">Select a fleet...</option>
          {fleets.map((fleet) => (
            <option key={fleet.id} value={fleet.id}>
              {fleet.name}
            </option>
          ))}
        </select>
        {error && <p className="text-sm text-red-500">{error}</p>}
      </div>
    );
  }
);
FleetSelect.displayName = 'FleetSelect';
