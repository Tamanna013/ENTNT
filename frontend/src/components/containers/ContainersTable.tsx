import React from 'react';
import { Container } from '../../types/container';
import { Table, Column } from '../ui/Table';
import { Badge } from '../ui/Badge';
import { Link } from 'react-router-dom';
import { Pencil, Trash2, Eye } from 'lucide-react';
import { useDeleteContainerMutation } from '../../hooks/useContainers';

interface ContainersTableProps {
  containers: Container[];
  canWrite: boolean;
  onEdit: (container: Container) => void;
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Empty': return 'gray';
    case 'Loaded': return 'blue';
    case 'InTransit': return 'purple';
    case 'AtPort': return 'amber';
    case 'Delivered': return 'green';
    default: return 'gray';
  }
};

export const ContainersTable: React.FC<ContainersTableProps> = ({ containers, canWrite, onEdit }) => {
  const deleteMutation = useDeleteContainerMutation();

  const handleDelete = async (container: Container) => {
    if (window.confirm(`Are you sure you want to delete container ${container.containerNumber}?`)) {
      await deleteMutation.mutateAsync(container.id);
    }
  };

  const columns: Column<Container>[] = [
    {
      key: 'containerNumber',
      header: 'Container Number',
      render: (row: Container) => <span className="font-medium text-text-primary">{row.containerNumber}</span>
    },
    {
      key: 'type',
      header: 'Type',
      render: (row: Container) => row.type
    },
    {
      key: 'status',
      header: 'Status',
      render: (row: Container) => (
        <Badge text={row.status} color={getStatusColor(row.status) as any} />
      )
    },
    {
      key: 'currentVoyage',
      header: 'Current Voyage',
      render: (row: Container) => row.voyageNumber ? row.voyageNumber : <span className="italic text-gray-500">— Unassigned —</span>
    },
    {
      key: 'linkedCargo',
      header: 'Linked Cargo Count',
      render: (row: Container) => row.linkedCargoIds.length
    },
    {
      key: 'actions',
      header: 'Actions',
      render: (row: Container) => (
        <div className="flex justify-end gap-2">
          <Link to={`/containers/${row.id}`} className="text-text-muted hover:text-text-primary transition-colors">
            <Eye className="h-5 w-5" />
          </Link>
          {canWrite && (
            <button onClick={() => onEdit(row)} className="text-text-muted hover:text-indigo-400 transition-colors">
              <Pencil className="h-5 w-5" />
            </button>
          )}
          {canWrite && (
            <button onClick={() => handleDelete(row)} className="text-text-muted hover:text-rose-400 transition-colors">
              <Trash2 className="h-5 w-5" />
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <Table
      data={containers}
      columns={columns}
      emptyMessage="No containers found"
    />
  );
};
