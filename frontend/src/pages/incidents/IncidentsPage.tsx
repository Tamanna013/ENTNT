import { useState } from 'react';
import { IncidentsTable } from '../../components/incidents/IncidentsTable';
import { ReportIncidentModal } from '../../components/incidents/ReportIncidentModal';
import { useIncidentsQuery } from '../../hooks/useIncidents';
import { Button } from '../../components/ui/Button';
import { Plus, Search } from 'lucide-react';
import { Pagination } from '../../components/ui/Pagination';
import { Input } from '../../components/ui/Input';
import { INCIDENT_SEVERITIES, INCIDENT_STATUSES } from '../../lib/constants';

export function IncidentsPage() {
  const [page, setPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState('');
  const [severityFilter, setSeverityFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [isReportModalOpen, setIsReportModalOpen] = useState(false);

  const { data, isLoading } = useIncidentsQuery({
    pageNumber: page,
    pageSize: 10,
    searchTerm: searchTerm || undefined,
    severity: severityFilter || undefined,
    status: statusFilter || undefined,
    sortBy: 'occurredAt',
    sortDescending: true,
  });

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Incident Management</h1>
          <p className="mt-1 text-sm text-gray-500">
            Report and track operational incidents across the fleet.
          </p>
        </div>
        
        {/* IMPORTANT: The Report Incident button is unconditionally rendered 
            for all authenticated users. DO NOT wrap this in a role check! */}
        <Button onClick={() => setIsReportModalOpen(true)} variant="primary" className="w-auto px-4 py-2 flex items-center">
          <Plus className="h-4 w-4 mr-2" />
          Report Incident
        </Button>
      </div>

      <div className="bg-white p-4 rounded-lg shadow-sm border border-gray-200 flex gap-4">
        <div className="flex-1">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-text-muted" />
            <Input
              label=""
              placeholder="Search incidents..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </div>
        <select
          className="border-gray-300 rounded-md text-sm"
          value={severityFilter}
          onChange={(e) => setSeverityFilter(e.target.value)}
        >
          <option value="">All Severities</option>
          {INCIDENT_SEVERITIES.map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
        <select
          className="border-gray-300 rounded-md text-sm"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="">All Statuses</option>
          {INCIDENT_STATUSES.map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
      </div>

      <IncidentsTable incidents={data?.items || []} isLoading={isLoading} />

      {data && (
        <Pagination
          pageNumber={data.pageNumber}
          totalPages={Math.ceil(data.totalCount / data.pageSize)}
          onPageChange={setPage}
        />
      )}

      <ReportIncidentModal
        isOpen={isReportModalOpen}
        onClose={() => setIsReportModalOpen(false)}
      />
    </div>
  );
}
