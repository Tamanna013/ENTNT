
import { useNavigate } from 'react-router-dom';
import { Eye, AlertTriangle } from 'lucide-react';
import { Incident } from '../../types/incident';
import { Table, Column } from '../ui/Table';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';

interface IncidentsTableProps {
  incidents: Incident[];
  isLoading?: boolean;
}

export function IncidentsTable({ incidents, isLoading }: IncidentsTableProps) {
  const navigate = useNavigate();

  const getSeverityBadgeColor = (severity: string) => {
    switch (severity) {
      case 'Critical':
        return 'red';
      case 'High':
        return 'yellow';
      case 'Medium':
        return 'blue';
      case 'Low':
      default:
        return 'gray';
    }
  };

  const getStatusBadgeColor = (status: string) => {
    switch (status) {
      case 'Reported':
        return 'red';
      case 'UnderInvestigation':
        return 'yellow';
      case 'Resolved':
        return 'green';
      case 'Closed':
        return 'gray';
      default:
        return 'gray';
    }
  };

  const columns: Column<Incident>[] = [
    { key: 'shipName', header: 'Ship' },
    { key: 'title', header: 'Title', render: (row) => (
      <div className="max-w-[200px] truncate" title={row.title}>
        {row.title}
      </div>
    )},
    { key: 'severity', header: 'Severity', render: (row) => (
      <div className="flex items-center gap-1.5">
        <Badge text={row.severity} color={getSeverityBadgeColor(row.severity)} />
        {row.severity === 'Critical' && (
          <AlertTriangle className="h-4 w-4 text-red-600 animate-pulse" />
        )}
      </div>
    )},
    { key: 'status', header: 'Status', render: (row) => (
      <Badge text={row.status} color={getStatusBadgeColor(row.status)} />
    )},
    { key: 'reportedByUserName', header: 'Reported By' },
    { key: 'occurredAt', header: 'Occurred At', render: (row) => (
      new Date(row.occurredAt).toLocaleString()
    )},
    { key: 'actions', header: 'Actions', render: (row) => (
      <Button
        variant="secondary"
        onClick={() => navigate(`/incidents/${row.id}`)}
      >
        <Eye className="h-4 w-4 mr-1 inline-block" />
        View
      </Button>
    )}
  ];

  return (
    <Table
      columns={columns}
      data={incidents}
      isLoading={isLoading}
      emptyMessage="No incidents found."
    />
  );
}
