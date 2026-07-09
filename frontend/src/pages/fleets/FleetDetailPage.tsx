import React, { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useFleetQuery } from '../../hooks/useFleets';
import { useShipsByFleetQuery, useUpdateShipMutation, useDeactivateShipMutation } from '../../hooks/useShips';
import { useAuthStore } from '../../store/authStore';
import { Badge } from '../../components/ui/Badge';
import { ShipsTable } from '../../components/ships/ShipsTable';
import { ShipFormModal } from '../../components/ships/ShipFormModal';
import { Pagination } from '../../components/ui/Pagination';
import { Ship, ShipQuery, UpdateShipPayload } from '../../types/ship';

export const FleetDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  
  // Fleet Data
  const { data: fleet, isLoading: isFleetLoading, error: fleetError } = useFleetQuery(id || '');

  // Ships Data
  const [shipQuery, setShipQuery] = useState<Omit<ShipQuery, "fleetId">>({
    pageNumber: 1,
    pageSize: 5,
    sortBy: 'createdAt',
    sortDescending: true,
  });

  const { data: shipsData, isLoading: isShipsLoading } = useShipsByFleetQuery(id || '', shipQuery);
  const updateShipMutation = useUpdateShipMutation();
  const deactivateShipMutation = useDeactivateShipMutation();

  const user = useAuthStore(state => state.user);
  const canWrite = !!(user?.roles?.includes('Admin') || user?.roles?.includes('FleetManager'));

  // Ship Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingShip, setEditingShip] = useState<Ship | undefined>();

  const handleEditShip = (ship: Ship) => {
    setEditingShip(ship);
    setIsModalOpen(true);
  };

  const handleDeactivateShip = async (shipId: string) => {
    if (window.confirm('Are you sure you want to deactivate this ship?')) {
      await deactivateShipMutation.mutateAsync(shipId);
    }
  };

  const handleSubmitShip = async (payload: any) => {
    if (editingShip) {
      await updateShipMutation.mutateAsync({ id: editingShip.id, payload: payload as UpdateShipPayload });
    }
    setIsModalOpen(false);
  };

  if (isFleetLoading) return <div className="text-text-muted">Loading fleet details...</div>;
  if (fleetError || !fleet) return <div className="text-red-500">Failed to load fleet</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Link to="/fleets" className="text-text-muted hover:text-text-primary transition-colors">
          <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
        </Link>
        <h1 className="text-2xl font-bold text-text-primary">{fleet.name}</h1>
        <Badge 
          text={fleet.status} 
          color={fleet.status === 'Active' ? 'green' : fleet.status === 'UnderReview' ? 'yellow' : 'gray'} 
        />
      </div>

      {/* Fleet Metadata Card */}
      <div className="bg-surface rounded-lg p-6 border border-border">
        <h2 className="text-lg font-semibold text-text-primary mb-4">Fleet Details</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div className="col-span-1 md:col-span-2 lg:col-span-4">
            <span className="block text-sm text-text-muted">Description</span>
            <span className="block text-text-primary mt-1">{fleet.description || <span className="text-text-muted italic">No description</span>}</span>
          </div>
          <div>
            <span className="block text-sm text-text-muted">Home Port</span>
            <span className="block text-text-primary mt-1">{fleet.homePortName}</span>
          </div>
          <div>
            <span className="block text-sm text-text-muted">Ship Count</span>
            <span className="block text-text-primary mt-1 font-medium">{fleet.shipCount}</span>
          </div>
          <div>
            <span className="block text-sm text-text-muted">Created At</span>
            <span className="block text-text-primary mt-1">
              {new Date(fleet.createdAt).toLocaleDateString()}
            </span>
          </div>
        </div>
      </div>

      {/* Embedded Ships Table */}
      <div className="space-y-4">
        <h2 className="text-xl font-bold text-text-primary">Ships in this Fleet</h2>
        
        {isShipsLoading ? (
          <div className="text-text-muted">Loading ships...</div>
        ) : (
          <div className="bg-surface rounded-lg overflow-hidden border border-border">
            <ShipsTable
              ships={shipsData?.items || []}
              onEdit={handleEditShip}
              onDeactivate={handleDeactivateShip}
              canWrite={canWrite}
            />
            {shipsData && (
              <div className="p-4 border-t border-border">
                <Pagination
                  pageNumber={shipsData.pageNumber}
                  totalPages={shipsData.totalPages}
                  onPageChange={(page) => setShipQuery({ ...shipQuery, pageNumber: page })}
                />
              </div>
            )}
          </div>
        )}
      </div>

      {/* Reused Ship Form Modal for Edit Action from the Table */}
      {editingShip && (
        <ShipFormModal
          isOpen={isModalOpen}
          onClose={() => setIsModalOpen(false)}
          onSubmit={handleSubmitShip}
          ship={editingShip}
          isLoading={updateShipMutation.isPending}
        />
      )}
    </div>
  );
};
