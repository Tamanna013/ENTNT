import React from 'react';

interface FormErrorProps {
    message?: string | null;
    id?: string;
}

export const FormError: React.FC<FormErrorProps> = ({ message, id }) => {
    if (!message) return null;

    return (
        <div id={id} className="mb-4 p-3 rounded-md bg-red-900/50 border border-red-500/50 text-red-200 text-sm" role="alert">
            {message}
        </div>
    );
};
