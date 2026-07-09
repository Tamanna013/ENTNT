import React from 'react';
import { useNavigate } from 'react-router-dom';
import { MaintenanceRecord } from '../../types/maintenance';
import { Table } from '../ui/Table';
import { Badge } from '../ui/Badge';

interface MaintenanceTableProps {
  records: MaintenanceRecord[];
  isLoading: boolean;
  canWrite: boolean;
  onEdit?: (record: MaintenanceRecord) => void;
  onDelete?: (record: MaintenanceRecord) => void;
}

export const MaintenanceTable: React.FC<MaintenanceTableProps> = ({
  records,
  isLoading,
  canWrite,
  onEdit,
  onDelete
}) => {
  const navigate = useNavigate();

  const getStatusBadgeColor = (status: string) => {
    switch (status) {
      case 'Scheduled': return 'blue';
      case 'InProgress': return 'yellow';
      case 'Completed': return 'green';
      case 'Overdue': return 'red';
      case 'Cancelled': return 'gray';
      default: return 'gray';
    }
  };

  return (
    <Table<MaintenanceRecord>
      data={records}
      isLoading={isLoading}
      columns={[
        {
          key: 'shipName',
          header: 'Ship',
          render: (item: MaintenanceRecord) => (
            <div className="font-medium text-text-primary">{item.shipName}</div>
          )
        },
        {
          key: 'type',
          header: 'Type',
          render: (item: MaintenanceRecord) => item.type
        },
        {
          key: 'description',
          header: 'Description',
          render: (item: MaintenanceRecord) => (
            <div className="truncate max-w-xs" title={item.description}>
              {item.description}
            </div>
          )
        },
        {
          key: 'status',
          header: 'Status',
          render: (item: MaintenanceRecord) => (
            <Badge text={item.status} color={getStatusBadgeColor(item.status)} />
          )
        },
        {
          key: 'scheduledDate',
          header: 'Scheduled Date',
          render: (item: MaintenanceRecord) => new Date(item.scheduledDate).toLocaleDateString()
        },
        {
          key: 'estimatedCost',
          header: 'Est. Cost',
          render: (item: MaintenanceRecord) => `$${item.estimatedCost.toFixed(2)}`
        },
        {
          key: 'actions',
          header: '',
          sortable: false,
          render: (row: MaintenanceRecord) => {
            const canEdit = canWrite && (row.status === 'Scheduled' || row.status === 'InProgress');
            const canDelete = canWrite && row.status !== 'InProgress';

            return (
              <div className="flex justify-end gap-2">
                <button onClick={() => navigate(`/maintenance/${row.id}`)} className="text-text-muted hover:text-text-primary transition-colors">
                  View
                </button>
                {canEdit && (
                  <button onClick={() => onEdit?.(row)} className="text-text-muted hover:text-indigo-400 transition-colors">
                    Edit
                  </button>
                )}
                {canDelete && (
                  <button onClick={() => onDelete?.(row)} className="text-text-muted hover:text-rose-400 transition-colors">
                    Delete
                  </button>
                )}
              </div>
            );
          }
        }
      ]}
    />
  );
};
