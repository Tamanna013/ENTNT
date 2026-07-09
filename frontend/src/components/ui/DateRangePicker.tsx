import React from 'react';

interface DateRangePickerProps {
  fromValue: string | undefined;
  toValue: string | undefined;
  onFromChange: (value: string | undefined) => void;
  onToChange: (value: string | undefined) => void;
}

export const DateRangePicker: React.FC<DateRangePickerProps> = ({
  fromValue,
  toValue,
  onFromChange,
  onToChange,
}) => {
  const idPrefix = React.useId();
  const fromId = `${idPrefix}-from`;
  const toId = `${idPrefix}-to`;

  return (
    <div className="flex items-center space-x-2">
      <div className="flex items-center">
        <label htmlFor={fromId} className="mr-2 text-sm text-text-muted">
          From
        </label>
        <input
          id={fromId}
          type="date"
          className="block w-full rounded-md border-0 bg-surface-hover py-1.5 px-3 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
          value={fromValue || ''}
          onChange={(e) => onFromChange(e.target.value || undefined)}
        />
      </div>
      <div className="flex items-center">
        <label htmlFor={toId} className="mr-2 text-sm text-text-muted">
          To
        </label>
        <input
          id={toId}
          type="date"
          className="block w-full rounded-md border-0 bg-surface-hover py-1.5 px-3 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
          value={toValue || ''}
          onChange={(e) => onToChange(e.target.value || undefined)}
        />
      </div>
    </div>
  );
};
