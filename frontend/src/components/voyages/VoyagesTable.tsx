import React from 'react';
import { Voyage } from '../../types/voyage';
import { Table, Column } from '../ui/Table';
import { Badge } from '../ui/Badge';
import { isTerminalStatus } from '../../lib/voyageStatusTransitions';
import { Link } from 'react-router-dom';
import { Pencil, Trash2, Eye } from 'lucide-react';

interface VoyagesTableProps {
  voyages: Voyage[];
  canWrite: boolean;
  onEdit: (voyage: Voyage) => void;
  onDelete: (voyage: Voyage) => void;
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Scheduled': return 'blue';
    case 'Delayed': return 'amber';
    case 'InTransit': return 'purple';
    case 'Completed': return 'green';
    case 'Cancelled': return 'red';
    default: return 'gray';
  }
};

export const VoyagesTable: React.FC<VoyagesTableProps> = ({ voyages, canWrite, onEdit, onDelete }) => {
  const columns: Column<Voyage>[] = [
    {
      key: 'voyageNumber',
      header: 'Voyage Number',
      render: (row: Voyage) => <span className="font-medium text-text-primary">{row.voyageNumber}</span>
    },
    {
      key: 'shipName',
      header: 'Ship Name',
    },
    {
      key: 'route',
      header: 'Route',
      sortable: false,
      render: (voyage: Voyage) => (
        <span className="text-text-primary">
          {voyage.originPortName} &rarr; {voyage.destinationPortName}
        </span>
      )
    },
    {
      key: 'departureDate',
      header: 'Departure Date',
      render: (row: Voyage) => new Date(row.departureDate).toLocaleDateString()
    },
    {
      key: 'estimatedArrivalDate',
      header: 'Estimated Arrival',
      render: (row: Voyage) => new Date(row.estimatedArrivalDate).toLocaleDateString()
    },
    {
      key: 'status',
      header: 'Status',
      render: (row: Voyage) => (
        <Badge text={row.status} color={getStatusColor(row.status) as any} />
      )
    },
    {
      key: 'actions',
      header: 'Actions',
      render: (row: Voyage) => {
        const terminal = isTerminalStatus(row.status);
        const canEdit = !terminal && canWrite;
        const canDelete = row.status !== 'InTransit' && canWrite;

        return (
          <div className="flex justify-end gap-2">
            <Link to={`/voyages/${row.id}`} className="text-text-muted hover:text-text-primary transition-colors">
              <Eye className="h-5 w-5" />
            </Link>
            {canEdit && (
              <button onClick={() => onEdit(row)} className="text-text-muted hover:text-indigo-400 transition-colors">
                <Pencil className="h-5 w-5" />
              </button>
            )}
            {canDelete && (
              <button onClick={() => onDelete(row)} className="text-text-muted hover:text-rose-400 transition-colors">
                <Trash2 className="h-5 w-5" />
              </button>
            )}
          </div>
        );
      }
    }
  ];

  return (
    <Table
      data={voyages}
      columns={columns}
      emptyMessage="No voyages found"
    />
  );
};
