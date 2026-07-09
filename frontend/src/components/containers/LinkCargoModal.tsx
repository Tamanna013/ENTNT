import React from 'react';
import { useCargoQuery } from '../../hooks/useCargo';
import { useLinkCargoMutation } from '../../hooks/useContainers';
import { Modal } from '../ui/Modal';
import { Button } from '../ui/Button';

interface LinkCargoModalProps {
  isOpen: boolean;
  onClose: () => void;
  containerId: string;
  linkedCargoIds: string[];
}

export const LinkCargoModal: React.FC<LinkCargoModalProps> = ({ isOpen, onClose, containerId, linkedCargoIds }) => {
  // Client-side filtering: fetch a broad list of cargo items (unpaginated-ish for current scale)
  // and exclude ones already linked. This simplification avoids needing a dedicated endpoint right now.
  const { data: cargoData, isLoading } = useCargoQuery({ pageNumber: 1, pageSize: 100 });
  const linkMutation = useLinkCargoMutation(containerId);

  const availableCargo = cargoData?.items.filter(c => !linkedCargoIds.includes(c.id)) || [];

  const handleLink = async (cargoId: string) => {
    await linkMutation.mutateAsync(cargoId);
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Link Cargo">
      <div className="space-y-4">
        {isLoading ? (
          <p className="text-text-muted">Loading cargo...</p>
        ) : availableCargo.length === 0 ? (
          <p className="text-text-muted">No unlinked cargo available.</p>
        ) : (
          <ul className="divide-y divide-white/10 max-h-96 overflow-y-auto">
            {availableCargo.map(cargo => (
              <li key={cargo.id} className="py-3 flex justify-between items-center">
                <div>
                  <p className="text-sm font-medium text-text-primary">{cargo.description}</p>
                  <p className="text-xs text-text-muted">Voyage: {cargo.voyageNumber || 'Unassigned'} • Type: {cargo.type}</p>
                </div>
                <Button 
                  onClick={() => handleLink(cargo.id)}
                  isLoading={linkMutation.isPending && linkMutation.variables === cargo.id}
                  disabled={linkMutation.isPending}
                >
                  Link
                </Button>
              </li>
            ))}
          </ul>
        )}
      </div>
    </Modal>
  );
};
