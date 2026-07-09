import React from 'react';
import { Ship } from '../../types/ship';
import { Table } from '../ui/Table';
import { Badge } from '../ui/Badge';
import { useAuthenticatedImage } from '../../hooks/useAuthenticatedImage';
import { Link } from 'react-router-dom';

interface ShipsTableProps {
  ships: Ship[];
  onEdit: (ship: Ship) => void;
  onDeactivate: (id: string) => void;
  canWrite: boolean;
}

const ShipPhoto: React.FC<{ url: string | null }> = ({ url }) => {
  const objectUrl = useAuthenticatedImage(url);

  if (!url || !objectUrl) {
    return (
      <div className="w-10 h-10 rounded overflow-hidden bg-surface flex items-center justify-center text-text-muted">
        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
        </svg>
      </div>
    );
  }

  return (
    <div className="w-10 h-10 rounded overflow-hidden bg-surface">
      <img src={objectUrl} alt="Ship thumbnail" className="w-full h-full object-cover" />
    </div>
  );
};

export const ShipsTable: React.FC<ShipsTableProps> = ({ ships, onEdit, onDeactivate, canWrite }) => {
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Active': return 'success';
      case 'InMaintenance': return 'warning';
      case 'Decommissioned': return 'error';
      case 'Docked': return 'default';
      default: return 'default';
    }
  };

  return (
    <Table
      data={ships}
      columns={[
        {
          key: 'photo',
          header: 'Photo',
          render: (row: Ship) => <ShipPhoto url={row.primaryPhotoUrl} />
        },
        { key: 'name', header: 'Name' },
        { key: 'imo', header: 'IMO' },
        { key: 'fleetName', header: 'Fleet' },
        { key: 'type', header: 'Type' },
        {
          key: 'status',
          header: 'Status',
          render: (row: Ship) => <Badge text={row.status} color={getStatusColor(row.status) as any} />
        },
        { key: 'yearBuilt', header: 'Year Built' },
        {
          key: 'actions',
          header: 'Actions',
          render: (row: Ship) => (
            <div className="flex space-x-3">
              <Link to={`/ships/${row.id}`} className="text-primary-400 hover:text-primary-300">
                View
              </Link>
              {canWrite && (
                <>
                  <button onClick={() => onEdit(row)} className="text-primary-400 hover:text-primary-300">
                    Edit
                  </button>
                  <button onClick={() => onDeactivate(row.id)} className="text-red-400 hover:text-red-300">
                    Deactivate
                  </button>
                </>
              )}
            </div>
          )
        }
      ]}
    />
  );
};
