import React from 'react';
import { useShipsQuery } from '../../hooks/useShips';

interface ShipSelectProps {
  value: string;
  onChange: (value: string) => void;
  className?: string;
  placeholder?: string;
  disabled?: boolean;
}

export const ShipSelect: React.FC<ShipSelectProps> = ({ 
  value, 
  onChange, 
  className = '',
  placeholder = 'Select a Ship...',
  disabled = false
}) => {
  const { data, isLoading, error } = useShipsQuery({
    pageNumber: 1,
    pageSize: 100, // Load a large batch for the dropdown
    sortBy: 'name',
    sortDescending: false
  });

  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      disabled={disabled || isLoading}
      className={`w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500 disabled:opacity-50 ${className}`}
    >
      <option value="">{isLoading ? 'Loading ships...' : placeholder}</option>
      {!isLoading && !error && data?.items.map((ship) => (
        <option key={ship.id} value={ship.id}>
          {ship.name} ({ship.imo})
        </option>
      ))}
    </select>
  );
};
