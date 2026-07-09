import React from 'react';
import { Link } from 'react-router-dom';
import { Table, Column } from '../ui/Table';
import { Badge } from '../ui/Badge';
import { Fleet } from '../../types/fleet';

interface FleetsTableProps {
  fleets: Fleet[];
  isLoading: boolean;
  sortBy?: string;
  sortDescending?: boolean;
  onSortChange: (key: string) => void;
  onEdit: (fleet: Fleet) => void;
  onDeactivate: (fleetId: string) => void;
  canWrite: boolean;
}

export const FleetsTable: React.FC<FleetsTableProps> = ({
  fleets,
  isLoading,
  sortBy,
  sortDescending,
  onSortChange,
  onEdit,
  onDeactivate,
  canWrite
}) => {
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Active': return 'green';
      case 'Inactive': return 'gray';
      case 'UnderReview': return 'yellow';
      default: return 'gray';
    }
  };

  const columns: Column<Fleet>[] = [
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (fleet) => (
        <div>
          <div className="font-medium text-text-primary">{fleet.name}</div>
          {fleet.description && <div className="text-xs text-text-muted truncate max-w-xs">{fleet.description}</div>}
        </div>
      )
    },
    {
      key: 'homePortName',
      header: 'Home Port',
      sortable: false
    },
    {
      key: 'status',
      header: 'Status',
      sortable: false,
      render: (fleet) => (
        <Badge text={fleet.status} color={getStatusColor(fleet.status)} />
      )
    },
    {
      key: 'shipCount',
      header: 'Ship Count',
      sortable: false,
      render: (fleet) => (
        <span className="text-text-primary">{fleet.shipCount}</span>
      )
    },
    {
      key: 'createdAt',
      header: 'Created At',
      sortable: true,
      render: (fleet) => new Date(fleet.createdAt).toLocaleDateString()
    },
    {
      key: 'actions',
      header: '',
      sortable: false,
      render: (fleet) => (
        <div className="flex items-center gap-3 justify-end">
          <Link 
            to={`/fleets/${fleet.id}`}
            className="text-primary-400 hover:text-primary-300 text-sm font-medium transition-colors"
          >
            View
          </Link>
          {canWrite && (
            <>
              <button 
                onClick={() => onEdit(fleet)}
                className="text-blue-400 hover:text-blue-300 text-sm font-medium transition-colors"
              >
                Edit
              </button>
              <button 
                onClick={() => {
                  if (window.confirm(`Are you sure you want to deactivate fleet ${fleet.name}?`)) {
                    onDeactivate(fleet.id);
                  }
                }}
                className="text-rose-400 hover:text-rose-300 text-sm font-medium transition-colors"
              >
                Deactivate
              </button>
            </>
          )}
        </div>
      )
    }
  ];

  return (
    <Table
      columns={columns}
      data={fleets}
      sortBy={sortBy}
      sortDescending={sortDescending}
      onSortChange={onSortChange}
      isLoading={isLoading}
      emptyMessage="No fleets found."
    />
  );
};
