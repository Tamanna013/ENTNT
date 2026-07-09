import React from 'react';
import { Search } from 'lucide-react';

interface InterpretedQueryBannerProps {
  module: string | null;
  filters: Record<string, unknown>;
}

export const InterpretedQueryBanner: React.FC<InterpretedQueryBannerProps> = ({ module, filters }) => {
  if (!module) return null;

  const filterKeys = Object.keys(filters);
  
  return (
    <div className="bg-indigo-50 border border-indigo-100 rounded-lg p-4 mb-6 flex items-start">
      <Search className="h-5 w-5 text-indigo-500 mt-0.5 mr-3 flex-shrink-0" />
      <div>
        <h3 className="text-sm font-medium text-indigo-900">
          Searching {module}
          {filterKeys.length > 0 && ' where:'}
        </h3>
        {filterKeys.length > 0 && (
          <ul className="mt-1 text-sm text-indigo-700 list-disc list-inside">
            {filterKeys.map(key => {
              const val = filters[key];
              const displayVal = typeof val === 'object' ? JSON.stringify(val) : String(val);
              return (
                <li key={key}>
                  <span className="font-semibold">{key}</span> is {displayVal}
                </li>
              );
            })}
          </ul>
        )}
      </div>
    </div>
  );
};
