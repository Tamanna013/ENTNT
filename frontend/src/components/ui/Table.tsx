import { ReactNode } from 'react';
import { ArrowDown, ArrowUp } from 'lucide-react';
import { useMediaQuery } from '../../hooks/useMediaQuery';

export interface Column<T> {
  key: string;
  header: string;
  sortable?: boolean;
  render?: (row: T) => ReactNode;
}

interface TableProps<T> {
  columns: Column<T>[];
  data: T[];
  sortBy?: string;
  sortDescending?: boolean;
  onSortChange?: (key: string) => void;
  isLoading?: boolean;
  emptyMessage?: string;
  mobileCardRenderer?: (row: T) => ReactNode;
}

export function Table<T extends Record<string, any>>({
  columns,
  data,
  sortBy,
  sortDescending,
  onSortChange,
  isLoading,
  emptyMessage = 'No data available',
  mobileCardRenderer
}: TableProps<T>) {
  const isMobile = useMediaQuery('(max-width: 768px)');

  if (isMobile) {
    return (
      <div className="w-full space-y-4">
        {isLoading ? (
          <div className="rounded-lg border border-border bg-surface p-6 text-center text-text-muted">
            Loading...
          </div>
        ) : data.length === 0 ? (
          <div className="rounded-lg border border-border bg-surface p-6 text-center text-text-muted">
            {emptyMessage}
          </div>
        ) : (
          data.map((row, rowIndex) => {
            if (mobileCardRenderer) {
              return <div key={rowIndex} className="rounded-lg border border-border bg-surface p-4">{mobileCardRenderer(row)}</div>;
            }

            const actionCol = columns.find(col => col.key.toLowerCase().includes('action') || col.header.toLowerCase().includes('action'));
            const dataCols = columns.filter(col => col !== actionCol);

            return (
              <div key={rowIndex} className="rounded-lg border border-border bg-surface overflow-hidden flex flex-col">
                <div className="p-4 flex-1 space-y-3">
                  {dataCols.map(col => (
                    <div key={col.key} className="flex flex-col sm:flex-row sm:justify-between sm:items-start gap-1">
                      <span className="text-xs font-medium text-text-muted uppercase tracking-wider">{col.header}</span>
                      <span className="text-sm text-text-primary break-words">
                        {col.render ? col.render(row) : (row[col.key] as ReactNode)}
                      </span>
                    </div>
                  ))}
                </div>
                {actionCol && actionCol.render && (
                  <div className="bg-surface-hover p-3 border-t border-border flex justify-end items-center gap-2">
                    {actionCol.render(row)}
                  </div>
                )}
              </div>
            );
          })
        )}
      </div>
    );
  }

  return (
    <div className="w-full overflow-x-auto rounded-lg border border-border">
      <table className="w-full text-left text-sm text-text-primary">
        <thead className="bg-slate-800/50 text-xs uppercase text-text-muted">
          <tr>
            {columns.map((col) => {
              const isSorted = col.sortable && sortBy === col.key;
              const ariaSort = isSorted ? (sortDescending ? 'descending' : 'ascending') : 'none';
              
              return (
                <th 
                  key={col.key} 
                  scope="col" 
                  aria-sort={col.sortable ? (ariaSort as React.AriaAttributes['aria-sort']) : undefined}
                  className={`px-6 py-4 font-medium ${col.sortable ? 'hover:bg-surface-hover' : ''}`}
                >
                  {col.sortable ? (
                    <button 
                      type="button"
                      className="flex items-center gap-2 w-full font-medium focus:outline-none rounded-sm"
                      onClick={() => onSortChange && onSortChange(col.key)}
                    >
                      {col.header}
                      {isSorted && (
                        sortDescending ? <ArrowDown className="h-4 w-4" /> : <ArrowUp className="h-4 w-4" />
                      )}
                    </button>
                  ) : (
                    <div className="flex items-center gap-2">
                      {col.header}
                    </div>
                  )}
                </th>
              );
            })}
          </tr>
        </thead>
        <tbody className="divide-y divide-border bg-surface">
          {isLoading ? (
            <tr>
              <td colSpan={columns.length} className="px-6 py-8 text-center text-text-muted">
                Loading...
              </td>
            </tr>
          ) : data.length === 0 ? (
            <tr>
              <td colSpan={columns.length} className="px-6 py-8 text-center text-text-muted">
                {emptyMessage}
              </td>
            </tr>
          ) : (
            data.map((row, rowIndex) => (
              <tr key={rowIndex} className="hover:bg-surface-hover transition-colors">
                {columns.map((col) => (
                  <td key={`${rowIndex}-${col.key}`} className="px-6 py-4 whitespace-nowrap">
                    {col.render ? col.render(row) : row[col.key]}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
