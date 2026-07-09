import React from 'react';
import { usePortsQuery } from '../../hooks/usePorts';

interface PortSelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
}

export const PortSelect = React.forwardRef<HTMLSelectElement, PortSelectProps>(
  ({ label, error, className = '', ...props }, ref) => {
    const { data } = usePortsQuery({ pageNumber: 1, pageSize: 100, sortBy: 'name' });
    const ports = data?.items || [];

    return (
      <div className="space-y-1">
        {label && (
          <label htmlFor={props.id || props.name} className="block text-sm font-medium text-text-primary">
            {label}
          </label>
        )}
        <select
          ref={ref}
          id={props.id || props.name}
          className={`w-full bg-background border ${ error ? 'border-red-500' : 'border-border' } rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500 ${className}`}
          {...props}
        >
          <option value="">Select a port...</option>
          {ports.map((port) => (
            <option key={port.id} value={port.id}>
              {port.name} ({port.unLocode})
            </option>
          ))}
        </select>
        {error && <p className="text-sm text-red-500">{error}</p>}
      </div>
    );
  }
);
PortSelect.displayName = 'PortSelect';
