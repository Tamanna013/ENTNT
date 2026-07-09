import React, { forwardRef } from 'react';

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
    label: string;
    error?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
    ({ label, error, className = '', id, ...props }, ref) => {
        const generatedId = React.useId();
        const inputId = id || generatedId;

        return (
            <div className={`mb-4 ${className}`}>
                <label htmlFor={inputId} className="block text-sm font-medium text-text-primary mb-1">
                    {label}
                </label>
                <input
                    id={inputId}
                    ref={ref}
                    className={`w-full px-3 py-2 bg-background border ${ error ? 'border-red-500 focus:ring-red-500' : 'border-border focus:ring-indigo-500' } rounded-md text-text-primary placeholder-slate-500 focus:outline-none focus:ring-1 focus:border-transparent transition-colors`}
                    aria-invalid={!!error}
                    aria-describedby={error ? `${inputId}-error` : undefined}
                    {...props}
                />
                {error && <p id={`${inputId}-error`} className="mt-1 text-sm text-red-400">{error}</p>}
            </div>
        );
    }
);

Input.displayName = 'Input';
