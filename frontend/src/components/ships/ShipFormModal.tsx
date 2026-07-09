import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Modal } from '../ui/Modal';
import { Ship, CreateShipPayload, UpdateShipPayload } from '../../types/ship';
import { FleetSelect } from './FleetSelect';
import { SHIP_STATUSES, SHIP_TYPES } from '../../lib/constants';

const createShipSchema = z.object({
  fleetId: z.string().min(1, 'Fleet is required'),
  name: z.string().min(1, 'Name is required').max(150),
  imo: z.string().regex(/^\d{7}$/, 'IMO must be exactly 7 digits'),
  type: z.enum(['ContainerShip', 'BulkCarrier', 'Tanker', 'RoRo', 'GeneralCargo']),
  status: z.enum(['Active', 'InMaintenance', 'Decommissioned', 'Docked']),
  yearBuilt: z.coerce.number().min(1950).max(new Date().getFullYear()),
  grossTonnage: z.coerce.number().positive('Must be greater than 0'),
  flag: z.string().min(1, 'Flag is required').max(100),
});

const updateShipSchema = z.object({
  name: z.string().min(1, 'Name is required').max(150),
  type: z.enum(['ContainerShip', 'BulkCarrier', 'Tanker', 'RoRo', 'GeneralCargo']),
  status: z.enum(['Active', 'InMaintenance', 'Decommissioned', 'Docked']),
  yearBuilt: z.coerce.number().min(1950).max(new Date().getFullYear()),
  grossTonnage: z.coerce.number().positive('Must be greater than 0'),
  flag: z.string().min(1, 'Flag is required').max(100),
});

type CreateFormData = z.infer<typeof createShipSchema>;
type UpdateFormData = z.infer<typeof updateShipSchema>;

interface ShipFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateShipPayload | UpdateShipPayload) => void;
  ship?: Ship;
  isLoading?: boolean;
}

export const ShipFormModal: React.FC<ShipFormModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  ship,
  isLoading
}) => {
  const isEditing = !!ship;
  
  const { register, handleSubmit, formState: { errors }, reset } = useForm<any>({
    resolver: zodResolver(isEditing ? updateShipSchema : createShipSchema) as any,
    defaultValues: {
      type: SHIP_TYPES[0],
      status: SHIP_STATUSES[0],
      yearBuilt: new Date().getFullYear(),
      grossTonnage: 1000
    }
  });

  useEffect(() => {
    if (isOpen) {
      if (ship) {
        reset({
          name: ship.name,
          type: ship.type,
          status: ship.status,
          yearBuilt: ship.yearBuilt,
          grossTonnage: ship.grossTonnage,
          flag: ship.flag,
        } as UpdateFormData);
      } else {
        reset({
          fleetId: '',
          name: '',
          imo: '',
          type: SHIP_TYPES[0],
          status: SHIP_STATUSES[0],
          yearBuilt: new Date().getFullYear(),
          grossTonnage: 1000,
          flag: ''
        } as CreateFormData);
      }
    }
  }, [isOpen, ship, reset]);

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={isEditing ? 'Edit Ship' : 'Add Ship'}
    >
      <form onSubmit={handleSubmit(onSubmit as any)} className="space-y-4">
        {!isEditing && (
          <FleetSelect
            label="Fleet"
            {...register('fleetId')}
            error={(errors as any).fleetId?.message}
          />
        )}
        
        {isEditing && (
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-text-primary">Fleet</label>
              <div className="mt-1 px-3 py-2 bg-surface border border-border rounded-md text-text-muted cursor-not-allowed">
                {ship.fleetName}
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-text-primary">IMO</label>
              <div className="mt-1 px-3 py-2 bg-surface border border-border rounded-md text-text-muted cursor-not-allowed">
                {ship.imo}
              </div>
            </div>
          </div>
        )}

        <div className="grid grid-cols-2 gap-4">
          <div className="space-y-1">
            <label className="block text-sm font-medium text-text-primary">Name</label>
            <input
              type="text"
              {...register('name')} aria-invalid={!!(errors as any)?.name} aria-describedby={(errors as any)?.name ? 'name-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
            {errors?.name?.message && <p id="name-error" className="text-sm text-red-500">{errors.name.message as string}</p>}
          </div>

          {!isEditing && (
            <div className="space-y-1">
              <label className="block text-sm font-medium text-text-primary">IMO</label>
              <input
                type="text"
                {...register('imo')} aria-invalid={!!(errors as any)?.imo} aria-describedby={(errors as any)?.imo ? 'imo-error' : undefined}
                className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
              {errors?.imo?.message && <p id="imo-error" className="text-sm text-red-500">{errors.imo.message as string}</p>}
            </div>
          )}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="space-y-1">
            <label className="block text-sm font-medium text-text-primary">Type</label>
            <select
              {...register('type')} aria-invalid={!!(errors as any)?.type} aria-describedby={(errors as any)?.type ? 'type-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              {SHIP_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
            </select>
            {errors?.type?.message && <p id="type-error" className="text-sm text-red-500">{errors.type.message as string}</p>}
          </div>

          <div className="space-y-1">
            <label className="block text-sm font-medium text-text-primary">Status</label>
            <select
              {...register('status')} aria-invalid={!!(errors as any)?.status} aria-describedby={(errors as any)?.status ? 'status-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
            >
              {SHIP_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
            </select>
            {errors?.status?.message && <p id="status-error" className="text-sm text-red-500">{errors.status.message as string}</p>}
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="space-y-1">
            <label className="block text-sm font-medium text-text-primary">Year Built</label>
            <input
              type="number"
              {...register('yearBuilt')} aria-invalid={!!(errors as any)?.yearBuilt} aria-describedby={(errors as any)?.yearBuilt ? 'yearBuilt-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
            {errors?.yearBuilt?.message && <p id="yearBuilt-error" className="text-sm text-red-500">{errors.yearBuilt.message as string}</p>}
          </div>

          <div className="space-y-1">
            <label className="block text-sm font-medium text-text-primary">Gross Tonnage</label>
            <input
              type="number"
              {...register('grossTonnage')} aria-invalid={!!(errors as any)?.grossTonnage} aria-describedby={(errors as any)?.grossTonnage ? 'grossTonnage-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
            />
            {errors?.grossTonnage?.message && <p id="grossTonnage-error" className="text-sm text-red-500">{errors.grossTonnage.message as string}</p>}
          </div>
        </div>

        <div className="space-y-1">
          <label className="block text-sm font-medium text-text-primary">Flag</label>
          <input
            type="text"
            {...register('flag')} aria-invalid={!!(errors as any)?.flag} aria-describedby={(errors as any)?.flag ? 'flag-error' : undefined}
            className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
          />
          {errors?.flag?.message && <p id="flag-error" className="text-sm text-red-500">{errors.flag.message as string}</p>}
        </div>

        <div className="flex justify-end space-x-3 mt-6">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-text-primary hover:text-text-primary bg-surface hover:bg-surface-hover rounded-md transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isLoading}
            className="px-4 py-2 text-sm font-medium text-text-primary bg-primary-600 hover:bg-primary-500 rounded-md transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? 'Saving...' : 'Save'}
          </button>
        </div>
      </form>
    </Modal>
  );
};
