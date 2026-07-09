import React, { useState } from 'react';
import { Cargo } from '../../types/cargo';
import { Table } from '../ui/Table';
import { Badge } from '../ui/Badge';
import { Link } from 'react-router-dom';
import { Trash2, Edit, Eye } from 'lucide-react';
import { CargoFormModal } from './CargoFormModal';
import { useDeleteCargoMutation } from '../../hooks/useCargo';
import { useToast } from '../../hooks/useToast';

interface CargoTableProps {
  data: Cargo[];
  isLoading: boolean;
  canWrite?: boolean;
}

export const CargoTable: React.FC<CargoTableProps> = ({ data, isLoading, canWrite = false }) => {
  const [editingCargo, setEditingCargo] = useState<Cargo | null>(null);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  
  const deleteMutation = useDeleteCargoMutation();
  const { showToast } = useToast();

  const handleEdit = (cargo: Cargo) => {
    setEditingCargo(cargo);
    setIsEditModalOpen(true);
  };

  const handleDeleteClick = async (cargo: Cargo) => {
    if (window.confirm(`Are you sure you want to delete cargo "${cargo.description}"? This action cannot be undone.`)) {
      try {
        await deleteMutation.mutateAsync(cargo.id);
        showToast('Cargo deleted successfully', 'success');
      } catch (err: any) {
        showToast(err.response?.data?.message || 'Failed to delete cargo', 'error');
      }
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  };

  const columns = [
    {
      key: 'description',
      header: 'Description',
      render: (row: Cargo) => <span className="font-medium">{row.description}</span>,
    },
    {
      key: 'voyage',
      header: 'Voyage',
      render: (row: Cargo) => row.voyageNumber || 'Unassigned',
    },
    {
      key: 'type',
      header: 'Type',
      render: (row: Cargo) => (
        <Badge text={row.type} color={row.type === 'Hazardous' ? 'red' : 'blue'} />
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (row: Cargo) => (
        <Badge text={row.status} color={row.status === 'Delivered' ? 'green' : row.status === 'Damaged' || row.status === 'Lost' ? 'red' : 'yellow'} />
      ),
    },
    {
      key: 'weight',
      header: 'Weight (Kg)',
      render: (row: Cargo) => row.weightKg.toLocaleString(),
    },
    {
      key: 'value',
      header: 'Declared Value',
      render: (row: Cargo) => formatCurrency(row.declaredValue),
    },
    {
      key: 'consignee',
      header: 'Consignee',
      render: (row: Cargo) => row.consigneeName,
    },
    {
      key: 'actions',
      header: 'Actions',
      render: (row: Cargo) => (
        <div className="flex justify-end space-x-2">
          <Link 
            to={`/cargo/${row.id}`}
            className="text-text-muted hover:text-indigo-600 transition-colors p-1"
            title="View Details"
          >
            <Eye size={18} />
          </Link>
          {canWrite && (
            <>
              <button 
                onClick={() => handleEdit(row)}
                className="text-text-muted hover:text-indigo-600 transition-colors p-1"
              >
                <Edit size={18} />
              </button>
              <button 
                onClick={() => handleDeleteClick(row)}
                className="text-text-muted hover:text-red-600 transition-colors p-1"
              >
                <Trash2 size={18} />
              </button>
            </>
          )}
        </div>
      ),
    }
  ];

  return (
    <>
      <Table data={data} columns={columns} isLoading={isLoading} emptyMessage="No cargo found." />

      {editingCargo && (
        <CargoFormModal
          isOpen={isEditModalOpen}
          onClose={() => {
            setIsEditModalOpen(false);
            setEditingCargo(null);
          }}
          cargo={editingCargo}
        />
      )}
    </>
  );
};
