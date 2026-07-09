import React from 'react';

interface TrailingWindowSelectorProps {
  value: number;
  onChange: (months: number) => void;
}

export const TrailingWindowSelector: React.FC<TrailingWindowSelectorProps> = ({ value, onChange }) => {
  const options = [
    { label: '3 Months', value: 3 },
    { label: '6 Months', value: 6 },
    { label: '12 Months', value: 12 },
    { label: '24 Months', value: 24 },
  ];

  return (
    <div className="inline-flex rounded-md shadow-sm" role="group">
      {options.map((option, index) => (
        <button
          key={option.value}
          type="button"
          onClick={() => onChange(option.value)}
          className={` px-4 py-2 text-sm font-medium ${index === 0 ? 'rounded-l-lg' : ''} ${index === options.length - 1 ? 'rounded-r-lg' : ''} ${value === option.value ? 'z-10 ring-2 ring-indigo-500 bg-indigo-50 text-indigo-700' : 'bg-white text-gray-700 hover:bg-gray-50 border border-gray-200' } ${index !== 0 && value !== option.value ? '-ml-px border-l-gray-200' : ''} `}
        >
          {option.label}
        </button>
      ))}
    </div>
  );
};
