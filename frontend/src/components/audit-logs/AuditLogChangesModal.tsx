import React from 'react';
import { X } from 'lucide-react';

interface AuditLogChangesModalProps {
  changes: string | null;
  onClose: () => void;
}

export const AuditLogChangesModal: React.FC<AuditLogChangesModalProps> = ({ changes, onClose }) => {
  if (!changes) {
    return null;
  }

  let parsedChanges: Record<string, { old?: string | null, new?: string | null }> | null = null;
  let parseError = false;

  try {
    const obj = JSON.parse(changes);
    if (obj && typeof obj === 'object' && !Array.isArray(obj)) {
      parsedChanges = obj;
    } else {
      parseError = true;
    }
  } catch (e) {
    parseError = true;
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
      <div className="bg-white dark:bg-slate-800 rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] flex flex-col">
        <div className="flex justify-between items-center p-4 border-b dark:border-slate-700">
          <h2 className="text-lg font-semibold text-slate-900 dark:text-white">View Changes</h2>
          <button onClick={onClose} className="text-text-muted hover:text-slate-500 dark:hover:text-slate-300">
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-4 overflow-y-auto flex-1">
          {parseError || !parsedChanges ? (
            <div className="space-y-4">
              <p className="text-sm text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-900/20 p-3 rounded-md">
                This entry uses an older or non-standard format. Displaying raw data.
              </p>
              <pre className="bg-slate-100 dark:bg-slate-900 p-4 rounded-md text-sm text-slate-800 dark:text-slate-300 overflow-x-auto whitespace-pre-wrap">
                {changes}
              </pre>
            </div>
          ) : (
            <div className="border dark:border-slate-700 rounded-md overflow-hidden">
              <table className="w-full text-sm text-left text-slate-600 dark:text-slate-300">
                <thead className="text-xs text-slate-700 uppercase bg-slate-50 dark:bg-slate-700 dark:text-slate-300">
                  <tr>
                    <th scope="col" className="px-6 py-3">Field Name</th>
                    <th scope="col" className="px-6 py-3">Old Value</th>
                    <th scope="col" className="px-6 py-3">New Value</th>
                  </tr>
                </thead>
                <tbody>
                  {Object.entries(parsedChanges).map(([field, values], index) => (
                    <tr key={field} className={index % 2 === 0 ? 'bg-white dark:bg-slate-800' : 'bg-slate-50 dark:bg-slate-800/50'}>
                      <td className="px-6 py-4 font-medium text-slate-900 dark:text-white">
                        {field}
                      </td>
                      <td className="px-6 py-4 text-text-muted line-through">
                        {values?.old === null ? 'null' : String(values?.old ?? '')}
                      </td>
                      <td className="px-6 py-4 font-semibold text-indigo-600 dark:text-indigo-400">
                        {values?.new === null ? 'null' : String(values?.new ?? '')}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        <div className="flex justify-end p-4 border-t dark:border-slate-700">
          <button
            onClick={onClose}
            className="px-4 py-2 bg-slate-100 dark:bg-slate-700 text-slate-700 dark:text-slate-200 rounded-md hover:bg-slate-200 dark:hover:bg-slate-600 transition-colors text-sm font-medium"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
};
