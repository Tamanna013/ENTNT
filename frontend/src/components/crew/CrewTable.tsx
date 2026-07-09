import React from 'react';
import { useNavigate } from 'react-router-dom';
import { CrewMember } from '../../types/crew';
import { Table, Column } from '../ui/Table';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';

interface CrewTableProps {
  crew: CrewMember[];
  isLoading: boolean;
  canWrite: boolean;
  onEdit?: (crew: CrewMember) => void;
  onDeactivate?: (crew: CrewMember) => void;
  onAssign?: (crew: CrewMember) => void;
  onUnassign?: (crew: CrewMember) => void;
}

export const CrewTable: React.FC<CrewTableProps> = ({ 
  crew, 
  isLoading, 
  canWrite,
  onEdit,
  onDeactivate,
  onAssign,
  onUnassign
}) => {
  const navigate = useNavigate();

  const getStatusBadgeColor = (status: string) => {
    switch (status) {
      case 'Active': return 'green';
      case 'OnLeave': return 'yellow';
      case 'Terminated': return 'red';
      case 'Unassigned':
      default:
        return 'gray';
    }
  };

  const columns: Column<CrewMember>[] = [
    {
      key: 'name',
      header: 'Name',
      render: (row) => `${row.firstName} ${row.lastName}`,
    },
    { key: 'rank', header: 'Rank' },
    { key: 'nationality', header: 'Nationality' },
    { key: 'licenseNumber', header: 'License Number' },
    {
      key: 'status',
      header: 'Status',
      render: (row) => <Badge color={getStatusBadgeColor(row.status) as any} text={row.status} />,
    },
    {
      key: 'ship',
      header: 'Ship',
      render: (row) => row.shipName || <span className="italic text-text-muted">— Unassigned —</span>,
    },
    {
      key: 'actions',
      header: 'Actions',
      render: (row) => (
        <div className="flex gap-2">
          <Button variant="secondary" className="px-2 py-1 text-xs" onClick={() => navigate(`/crew/${row.id}`)}>
            View
          </Button>
          {canWrite && (
            <>
              {onEdit && (
                <Button variant="secondary" className="px-2 py-1 text-xs" onClick={() => onEdit(row)}>
                  Edit
                </Button>
              )}
              {row.shipId ? (
                onUnassign && (
                  <Button variant="secondary" className="px-2 py-1 text-xs" onClick={() => onUnassign(row)}>
                    Unassign
                  </Button>
                )
              ) : (
                onAssign && (
                  <Button variant="secondary" className="px-2 py-1 text-xs" onClick={() => onAssign(row)}>
                    Assign to Ship
                  </Button>
                )
              )}
              {onDeactivate && (
                <Button variant="secondary" className="px-2 py-1 text-xs text-red-400 border-red-500/50 hover:bg-red-500/10" onClick={() => onDeactivate(row)}>
                  Deactivate
                </Button>
              )}
            </>
          )}
        </div>
      ),
    },
  ];

  return <Table data={crew} columns={columns} isLoading={isLoading} />;
};
