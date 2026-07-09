import React, { useState } from 'react';
import { Modal } from '../ui/Modal';
import { Button } from '../ui/Button';
import { ShipSelect } from '../ships/ShipSelect';
import { useAssignToShipMutation } from '../../hooks/useCrew';

interface AssignToShipModalProps {
  isOpen: boolean;
  onClose: () => void;
  crewMemberId: string;
}

export const AssignToShipModal: React.FC<AssignToShipModalProps> = ({ isOpen, onClose, crewMemberId }) => {
  const [shipId, setShipId] = useState('');
  const { mutateAsync: assignToShip, isPending } = useAssignToShipMutation();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!shipId) return;

    try {
      await assignToShip({ id: crewMemberId, shipId });
      onClose();
      setShipId('');
    } catch (error) {
      console.error('Failed to assign to ship', error);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Assign to Ship">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-text-primary mb-1">Select Ship</label>
          <ShipSelect value={shipId} onChange={setShipId} disabled={isPending} />
        </div>
        <div className="flex justify-end gap-3 pt-4 border-t border-border">
          <Button type="button" variant="secondary" onClick={onClose} disabled={isPending}>
            Cancel
          </Button>
          <Button type="submit" disabled={!shipId || isPending}>
            {isPending ? 'Assigning...' : 'Assign'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};
