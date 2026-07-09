import React, { useState } from 'react';
import { AuditLogsTable } from '../../components/audit-logs/AuditLogsTable';
import { Pagination } from '../../components/ui/Pagination';
import { DateRangePicker } from '../../components/ui/DateRangePicker';
import { useAuditLogsQuery } from '../../hooks/useAuditLogs';
import { AuditLogQuery } from '../../types/auditLog';

const ENTITY_NAMES = ['User', 'Fleet', 'Ship', 'Voyage', 'CrewMember', 'Cargo', 'Container', 'MaintenanceRecord', 'FuelLog', 'Port', 'Incident', 'Document'];
const ACTIONS = ['Created', 'Updated', 'Deleted'];

export const AuditLogsPage: React.FC = () => {
  const [query, setQuery] = useState<AuditLogQuery>({
    pageNumber: 1,
    pageSize: 20,
    entityName: '',
    action: '',
    userId: '',
    dateFrom: undefined,
    dateTo: undefined,
  });

  const { data, isLoading, error } = useAuditLogsQuery(query);

  const handleFilterChange = (key: keyof AuditLogQuery, value: any) => {
    setQuery(prev => ({ ...prev, [key]: value, pageNumber: 1 }));
  };

  const handleDateFromChange = (dateStr: string | undefined) => {
    setQuery(prev => ({ ...prev, dateFrom: dateStr, pageNumber: 1 }));
  };

  const handleDateToChange = (dateStr: string | undefined) => {
    setQuery(prev => ({ ...prev, dateTo: dateStr, pageNumber: 1 }));
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-text-primary">Audit Logs</h1>
          <p className="text-sm text-text-muted mt-1">
            Compliance trail recording system changes across all platform entities.
          </p>
        </div>
      </div>

      <div className="bg-surface p-4 rounded-lg flex flex-wrap gap-4 items-end border border-border">
        <div className="w-48">
          <label className="block text-sm font-medium text-text-primary mb-1">Entity Name</label>
          <select
            value={query.entityName || ''}
            onChange={(e) => handleFilterChange('entityName', e.target.value)}
            className="w-full bg-surface-hover border border-border rounded-md shadow-sm py-2 pl-3 pr-10 text-sm text-text-primary focus:outline-none focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500"
          >
            <option value="">All Entities</option>
            {ENTITY_NAMES.map(name => (
              <option key={name} value={name}>{name}</option>
            ))}
          </select>
        </div>

        <div className="w-40">
          <label className="block text-sm font-medium text-text-primary mb-1">Action</label>
          <select
            value={query.action || ''}
            onChange={(e) => handleFilterChange('action', e.target.value)}
            className="w-full bg-surface-hover border border-border rounded-md shadow-sm py-2 pl-3 pr-10 text-sm text-text-primary focus:outline-none focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500"
          >
            <option value="">All Actions</option>
            {ACTIONS.map(action => (
              <option key={action} value={action}>{action}</option>
            ))}
          </select>
        </div>

        <div className="w-64">
          <label className="block text-sm font-medium text-text-primary mb-1">User ID (Guid)</label>
          <input
            type="text"
            placeholder="Search by User ID..."
            value={query.userId || ''}
            onChange={(e) => handleFilterChange('userId', e.target.value)}
            className="w-full bg-surface-hover border border-border rounded-md shadow-sm py-2 px-3 text-sm text-text-primary focus:outline-none focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500"
          />
        </div>

        <div className="flex-1 min-w-[300px]">
          <label className="block text-sm font-medium text-text-primary mb-1">Date Range</label>
          <DateRangePicker 
            fromValue={query.dateFrom}
            toValue={query.dateTo}
            onFromChange={handleDateFromChange}
            onToChange={handleDateToChange}
          />
        </div>
      </div>

      <div className="bg-surface shadow-sm rounded-lg border border-border overflow-hidden">
        <div className="p-4 border-b border-border flex justify-between items-center bg-slate-800/50">
          <h2 className="text-lg font-medium text-text-primary">Log Entries</h2>
          <span className="text-sm text-text-muted">
            Total records: {data?.totalCount || 0}
          </span>
        </div>
        
        {isLoading ? (
          <div className="p-8 text-center text-text-muted">Loading audit history...</div>
        ) : error ? (
          <div className="p-8 text-center text-red-400">Failed to load audit logs.</div>
        ) : !data?.items?.length ? (
          <div className="p-8 text-center text-text-muted">No logs found matching your criteria.</div>
        ) : (
          <div className="overflow-x-auto">
            <AuditLogsTable logs={data.items} />
          </div>
        )}
        
        {data && data.totalCount > 0 && (
          <div className="p-4 border-t border-border bg-slate-800/50">
            <Pagination
              pageNumber={query.pageNumber}
              totalPages={Math.ceil(data.totalCount / query.pageSize)}
              onPageChange={(page) => setQuery({ ...query, pageNumber: page })}
            />
          </div>
        )}
      </div>
    </div>
  );
};
