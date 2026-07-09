import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Modal } from '../ui/Modal';
import { Fleet, CreateFleetPayload, UpdateFleetPayload } from '../../types/fleet';
import { FLEET_STATUSES } from '../../lib/constants';
import { useCreateFleetMutation, useUpdateFleetMutation } from '../../hooks/useFleets';
import { PortSelect } from '../ports/PortSelect';

const fleetSchema = z.object({
  name: z.string().min(1, 'Name is required').max(150, 'Name must be at most 150 characters'),
  description: z.string().max(1000, 'Description must be at most 1000 characters').optional().nullable(),
  homePortId: z.string().min(1, 'Home Port is required'),
  status: z.enum(["Active", "Inactive", "UnderReview"], { message: 'Invalid status' })
});

interface FleetFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  fleet?: Fleet;
}

export const FleetFormModal: React.FC<FleetFormModalProps> = ({ isOpen, onClose, fleet }) => {
  const isEditMode = !!fleet;
  
  const createMutation = useCreateFleetMutation();
  const updateMutation = useUpdateFleetMutation();

  const { register, handleSubmit, reset, formState: { errors } } = useForm({
    resolver: zodResolver(fleetSchema),
    defaultValues: {
      name: '',
      description: '',
      homePortId: '',
      status: 'Active'
    }
  });

  useEffect(() => {
    if (isOpen) {
      if (isEditMode && fleet) {
        reset({
          name: fleet.name,
          description: fleet.description || '',
          homePortId: fleet.homePortId,
          status: fleet.status as any
        });
      } else {
        reset({
          name: '',
          description: '',
          homePortId: '',
          status: 'Active'
        });
      }
    }
  }, [isOpen, isEditMode, fleet, reset]);

  const onSubmit = async (data: any) => {
    try {
      if (isEditMode && fleet) {
        const payload: UpdateFleetPayload = {
          name: data.name,
          description: data.description || undefined,
          homePortId: data.homePortId,
          status: data.status
        };
        await updateMutation.mutateAsync({ id: fleet.id, payload });
      } else {
        const payload: CreateFleetPayload = {
          name: data.name,
          description: data.description || undefined,
          homePortId: data.homePortId,
          status: data.status
        };
        await createMutation.mutateAsync(payload);
      }
      onClose();
    } catch (error: any) {
      console.error('Failed to save fleet:', error);
      alert(error.response?.data?.message || 'Failed to save fleet');
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={isEditMode ? 'Edit Fleet' : 'Create Fleet'}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        
        <div>
          <label htmlFor="name" className="block text-sm font-medium text-text-primary">Name</label>
          <input
            id="name"
            type="text"
            {...register('name')} aria-invalid={!!(errors as any)?.name} aria-describedby={(errors as any)?.name ? 'name-error' : undefined}
            className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
          />
          {errors.name && <p id="name-error" className="mt-1 text-sm text-red-400">{errors.name.message as string}</p>}
        </div>

        <div>
          <label htmlFor="description" className="block text-sm font-medium text-text-primary">Description</label>
          <textarea
            id="description"
            {...register('description')} aria-invalid={!!(errors as any)?.description} aria-describedby={(errors as any)?.description ? 'description-error' : undefined}
            rows={3}
            className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
          />
          {errors.description && <p id="description-error" className="mt-1 text-sm text-red-400">{errors.description.message as string}</p>}
        </div>

        <div>
          <PortSelect
            label="Home Port"
            id="homePortId"
            {...register('homePortId')}
            error={errors.homePortId?.message as string}
          />
        </div>

        <div>
          <label htmlFor="status" className="block text-sm font-medium text-text-primary">Status</label>
          <select
            id="status"
            {...register('status')} aria-invalid={!!(errors as any)?.status} aria-describedby={(errors as any)?.status ? 'status-error' : undefined}
            className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
          >
            {FLEET_STATUSES.map((status) => (
              <option key={status} value={status}>{status}</option>
            ))}
          </select>
          {errors.status && <p id="status-error" className="mt-1 text-sm text-red-400">{errors.status.message as string}</p>}
        </div>

        <div className="mt-6 flex justify-end gap-3">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-text-primary hover:text-slate-100 transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={createMutation.isPending || updateMutation.isPending}
            className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50"
          >
            {createMutation.isPending || updateMutation.isPending ? 'Saving...' : 'Save Fleet'}
          </button>
        </div>
      </form>
    </Modal>
  );
};
