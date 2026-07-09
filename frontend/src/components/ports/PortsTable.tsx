import React from 'react';
import { Table, Column } from '../ui/Table';
import { Port } from '../../types/port';

interface PortsTableProps {
  ports: Port[];
  isLoading: boolean;
  sortBy?: string;
  sortDescending?: boolean;
  onSortChange: (key: string) => void;
  onEdit: (port: Port) => void;
  onDeactivate: (portId: string) => void;
  canWrite: boolean;
}

export const PortsTable: React.FC<PortsTableProps> = ({
  ports,
  isLoading,
  sortBy,
  sortDescending,
  onSortChange,
  onEdit,
  onDeactivate,
  canWrite
}) => {
  const columns: Column<Port>[] = [
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (port) => (
        <span className="font-medium text-text-primary">{port.name}</span>
      )
    },
    {
      key: 'unLocode',
      header: 'UN/LOCODE',
      sortable: true,
      render: (port) => (
        <span className="text-text-primary font-mono text-sm">{port.unLocode}</span>
      )
    },
    {
      key: 'country',
      header: 'Country',
      sortable: true,
      render: (port) => <span className="text-text-primary">{port.country}</span>
    },
    {
      key: 'city',
      header: 'City',
      sortable: true,
      render: (port) => <span className="text-text-primary">{port.city}</span>
    },
    {
      key: 'actions',
      header: '',
      sortable: false,
      render: (port) => (
        <div className="flex items-center gap-3 justify-end">
          {canWrite && (
            <>
              <button 
                onClick={() => onEdit(port)}
                className="text-blue-400 hover:text-blue-300 text-sm font-medium transition-colors"
              >
                Edit
              </button>
              <button 
                onClick={() => {
                  if (window.confirm(`Are you sure you want to deactivate port ${port.name}?`)) {
                    onDeactivate(port.id);
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
      data={ports}
      sortBy={sortBy}
      sortDescending={sortDescending}
      onSortChange={onSortChange}
      isLoading={isLoading}
      emptyMessage="No ports found."
    />
  );
};
