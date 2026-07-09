import React, { useState, useRef, useEffect } from 'react';
import { Button } from './Button';
import { downloadAuthenticatedFile } from '../../lib/downloadFile';
import { buildExportUrl } from '../../lib/buildExportUrl';
import { Download } from 'lucide-react';

interface ExportButtonProps {
  exportPath: string;
  filters: Record<string, unknown>;
}

export const ExportButton: React.FC<ExportButtonProps> = ({ exportPath, filters }) => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleExport = async (format: 'csv' | 'xlsx') => {
    setIsOpen(false);
    const url = buildExportUrl(exportPath, filters, format);
    const filename = `export.${format}`;
    await downloadAuthenticatedFile(url, filename);
  };

  return (
    <div className="relative inline-block text-left" ref={dropdownRef}>
      <Button variant="secondary" onClick={() => setIsOpen(!isOpen)} className="flex items-center gap-2">
        <Download className="w-4 h-4" />
        Export
      </Button>

      {isOpen && (
        <div className="absolute right-0 mt-2 w-48 rounded-md shadow-lg bg-white dark:bg-slate-800 ring-1 ring-black ring-opacity-5 z-10">
          <div className="py-1" role="menu" aria-orientation="vertical" aria-labelledby="options-menu">
            <button
              onClick={() => handleExport('csv')}
              className="block w-full text-left px-4 py-2 text-sm text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700"
              role="menuitem"
            >
              Export as CSV
            </button>
            <button
              onClick={() => handleExport('xlsx')}
              className="block w-full text-left px-4 py-2 text-sm text-slate-700 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-700"
              role="menuitem"
            >
              Export as Excel
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
