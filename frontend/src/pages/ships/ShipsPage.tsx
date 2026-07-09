import React, { useState } from 'react';
import { Ship, ShipQuery, CreateShipPayload, UpdateShipPayload } from '../../types/ship';
import { useShipsQuery, useCreateShipMutation, useUpdateShipMutation, useDeactivateShipMutation } from '../../hooks/useShips';
import { useAuthStore } from '../../store/authStore';
import { ShipsTable } from '../../components/ships/ShipsTable';
import { ShipFormModal } from '../../components/ships/ShipFormModal';
import { Pagination } from '../../components/ui/Pagination';
import { SearchInput } from '../../components/ui/SearchInput';
import { FleetSelect } from '../../components/ships/FleetSelect';
import { SHIP_STATUSES, SHIP_TYPES } from '../../lib/constants';
import { ExportButton } from '../../components/ui/ExportButton';

export const ShipsPage: React.FC = () => {
  const [query, setQuery] = useState<ShipQuery>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'createdAt',
    sortDescending: true,
    searchTerm: '',
    fleetId: '',
    status: '',
    type: ''
  });

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingShip, setEditingShip] = useState<Ship | undefined>();

  const { data, isLoading, error } = useShipsQuery(query);
  const createMutation = useCreateShipMutation();
  const updateMutation = useUpdateShipMutation();
  const deactivateMutation = useDeactivateShipMutation();

  const user = useAuthStore(state => state.user);
  const canWrite = !!(user?.roles?.includes('Admin') || user?.roles?.includes('FleetManager'));

  const handleCreate = () => {
    setEditingShip(undefined);
    setIsModalOpen(true);
  };

  const handleEdit = (ship: Ship) => {
    setEditingShip(ship);
    setIsModalOpen(true);
  };

  const handleDeactivate = async (id: string) => {
    if (window.confirm('Are you sure you want to deactivate this ship?')) {
      await deactivateMutation.mutateAsync(id);
    }
  };

  const handleSubmit = async (payload: CreateShipPayload | UpdateShipPayload) => {
    if (editingShip) {
      await updateMutation.mutateAsync({ id: editingShip.id, payload: payload as UpdateShipPayload });
    } else {
      await createMutation.mutateAsync(payload as CreateShipPayload);
    }
    setIsModalOpen(false);
  };

  if (error) {
    return <div className="text-red-500">Error loading ships</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-text-primary">Ships Management</h1>
        <div className="flex items-center gap-2">
          <ExportButton exportPath="/ships/export" filters={query as unknown as Record<string, unknown>} />
          {canWrite && (
            <button 
              onClick={handleCreate}
              className="px-4 py-2 bg-primary-600 hover:bg-primary-500 text-text-primary rounded-md font-medium transition-colors"
            >
              Add Ship
            </button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 bg-surface p-4 rounded-lg">
        <SearchInput
          value={query.searchTerm || ''}
          onChange={(val) => setQuery({ ...query, searchTerm: val, pageNumber: 1 })}
          placeholder="Search ships..."
        />
        
        <FleetSelect
          value={query.fleetId || ''}
          onChange={(e) => setQuery({ ...query, fleetId: e.target.value, pageNumber: 1 })}
        />

        <select
          value={query.status || ''}
          onChange={(e) => setQuery({ ...query, status: e.target.value, pageNumber: 1 })}
          className="bg-background border border-border rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
        >
          <option value="">All Statuses</option>
          {SHIP_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
        </select>

        <select
          value={query.type || ''}
          onChange={(e) => setQuery({ ...query, type: e.target.value, pageNumber: 1 })}
          className="bg-background border border-border rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
        >
          <option value="">All Types</option>
          {SHIP_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
        </select>
      </div>

      {isLoading ? (
        <div className="text-text-muted">Loading ships...</div>
      ) : (
        <div className="bg-surface rounded-lg overflow-hidden border border-border">
          <ShipsTable
            ships={data?.items || []}
            onEdit={handleEdit}
            onDeactivate={handleDeactivate}
            canWrite={canWrite}
          />
          {data && (
            <div className="p-4 border-t border-border">
              <Pagination
                pageNumber={data.pageNumber}
                totalPages={data.totalPages}
                onPageChange={(page) => setQuery({ ...query, pageNumber: page })}
              />
            </div>
          )}
        </div>
      )}

      <ShipFormModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleSubmit}
        ship={editingShip}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
};
