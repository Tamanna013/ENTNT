import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Modal } from '../ui/Modal';
import { Button } from '../ui/Button';
import { Input } from '../ui/Input';
import { FUEL_TYPES } from '../../lib/constants';
import { FuelLog, CreateFuelLogPayload, UpdateFuelLogPayload } from '../../types/fuel';
import { ShipSelect } from '../ships/ShipSelect';
import { useVoyagesQuery } from '../../hooks/useVoyages';

const createSchema = z.object({
  shipId: z.string().min(1, 'Ship is required'),
  voyageId: z.string().optional(),
  fuelType: z.enum([...FUEL_TYPES] as [string, ...string[]]),
  quantityLiters: z.number().min(0.01, 'Quantity must be greater than 0'),
  costPerLiter: z.number().min(0, 'Cost per liter must be 0 or greater'),
  recordedDate: z.string().refine((val) => {
    const date = new Date(val);
    const now = new Date();
    now.setMinutes(now.getMinutes() + 5);
    return date <= now;
  }, 'Recorded date cannot be in the future'),
  notes: z.string().optional(),
});

const updateSchema = z.object({
  quantityLiters: z.number().min(0.01, 'Quantity must be greater than 0'),
  costPerLiter: z.number().min(0, 'Cost per liter must be 0 or greater'),
  recordedDate: z.string().refine((val) => {
    const date = new Date(val);
    const now = new Date();
    now.setMinutes(now.getMinutes() + 5);
    return date <= now;
  }, 'Recorded date cannot be in the future'),
  notes: z.string().optional(),
});



interface FuelFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateFuelLogPayload | UpdateFuelLogPayload) => void;
  fuelLog?: FuelLog | null;
  isLoading?: boolean;
}

export const FuelFormModal: React.FC<FuelFormModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  fuelLog,
  isLoading,
}) => {
  const isEditMode = !!fuelLog;

  const {
    register,
    handleSubmit,
    control,
    watch,
    setValue,
    reset,
    formState: { errors },
  } = useForm<any>({
    resolver: zodResolver(isEditMode ? updateSchema : createSchema),
    defaultValues: {
      shipId: '',
      voyageId: '',
      fuelType: 'HeavyFuelOil',
      quantityLiters: 0,
      costPerLiter: 0,
      recordedDate: new Date().toISOString().slice(0, 16),
      notes: '',
    },
  });

  useEffect(() => {
    if (isOpen) {
      if (fuelLog) {
        reset({
          quantityLiters: fuelLog.quantityLiters,
          costPerLiter: fuelLog.costPerLiter,
          recordedDate: new Date(fuelLog.recordedDate).toISOString().slice(0, 16),
          notes: fuelLog.notes || '',
        });
      } else {
        reset({
          shipId: '',
          voyageId: '',
          fuelType: 'HeavyFuelOil',
          quantityLiters: 0,
          costPerLiter: 0,
          recordedDate: new Date().toISOString().slice(0, 16),
          notes: '',
        });
      }
    }
  }, [isOpen, fuelLog, reset]);

  const watchedShipId = watch('shipId');
  const watchedVoyageId = watch('voyageId');

  // Query voyages only for the selected ship
  const { data: voyagesData } = useVoyagesQuery({
    pageNumber: 1,
    pageSize: 100,
    shipId: watchedShipId || undefined,
    sortBy: 'departureDate',
    sortDescending: true,
  });

  // Reset voyage field if the ship changes
  useEffect(() => {
    if (!isEditMode && watchedShipId) {
      const selectedVoyage = voyagesData?.items.find(v => v.id === watchedVoyageId);
      if (watchedVoyageId && !selectedVoyage) {
        setValue('voyageId', '');
      }
    }
  }, [watchedShipId, voyagesData, isEditMode, watchedVoyageId, setValue]);

  const submitHandler = (data: any) => {
    if (isEditMode) {
      onSubmit(data as UpdateFuelLogPayload);
    } else {
      const payload: CreateFuelLogPayload = {
        shipId: data.shipId,
        fuelType: data.fuelType,
        quantityLiters: data.quantityLiters,
        costPerLiter: data.costPerLiter,
        recordedDate: new Date(data.recordedDate).toISOString(),
      };
      if (data.voyageId) {
        payload.voyageId = data.voyageId;
      }
      if (data.notes) {
        payload.notes = data.notes;
      }
      onSubmit(payload);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={isEditMode ? 'Edit Fuel Log' : 'Add Fuel Log'}
    >
      <form onSubmit={handleSubmit(submitHandler)} className="space-y-4">
        {!isEditMode && (
          <>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Ship</label>
              <Controller
                name="shipId"
                control={control}
                render={({ field }) => (
                  <ShipSelect
                    value={field.value}
                    onChange={field.onChange}
                  />
                )}
              />
              {errors.shipId && (
                <p className="mt-1 text-sm text-red-600">{errors.shipId.message as string}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Voyage (Optional)</label>
              <select
                className="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md shadow-sm"
                {...register('voyageId')} aria-invalid={!!(errors as any)?.voyageId} aria-describedby={(errors as any)?.voyageId ? 'voyageId-error' : undefined}
                disabled={!watchedShipId}
              >
                <option value="">— None —</option>
                {voyagesData?.items.map((voyage) => (
                  <option key={voyage.id} value={voyage.id}>
                    {voyage.voyageNumber} ({voyage.originPortName} to {voyage.destinationPortName})
                  </option>
                ))}
              </select>
              {errors.voyageId && (
                <p className="mt-1 text-sm text-red-600">{errors.voyageId.message as string}</p>
              )}
              {!watchedShipId && (
                <p className="mt-1 text-xs text-gray-500">Select a ship first to view its voyages.</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Fuel Type</label>
              <select
                className="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md shadow-sm"
                {...register('fuelType')} aria-invalid={!!(errors as any)?.fuelType} aria-describedby={(errors as any)?.fuelType ? 'fuelType-error' : undefined}
              >
                {FUEL_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
              {errors.fuelType && (
                <p className="mt-1 text-sm text-red-600">{errors.fuelType.message as string}</p>
              )}
            </div>
          </>
        )}

        {isEditMode && fuelLog && (
          <div className="bg-gray-50 p-3 rounded-md space-y-1 mb-4">
            <p className="text-sm"><strong>Ship:</strong> {fuelLog.shipName}</p>
            <p className="text-sm"><strong>Voyage:</strong> {fuelLog.voyageNumber || 'None'}</p>
            <p className="text-sm"><strong>Type:</strong> {fuelLog.fuelType}</p>
          </div>
        )}

        <div className="grid grid-cols-2 gap-4">
          <div>
            <Input
              label="Quantity (Liters)"
              type="number"
              step="any"
              {...register('quantityLiters', { valueAsNumber: true })}
              error={errors.quantityLiters?.message as string}
            />
          </div>
          <div>
            <Input
              label="Cost per Liter"
              type="number"
              step="0.0001"
              {...register('costPerLiter', { valueAsNumber: true })}
              error={errors.costPerLiter?.message as string}
            />
          </div>
        </div>

        <div>
          <Input
            label="Recorded Date"
            type="datetime-local"
            {...register('recordedDate')}
            error={errors.recordedDate?.message as string}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
          <textarea
            className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
            rows={3}
            {...register('notes')} aria-invalid={!!(errors as any)?.notes} aria-describedby={(errors as any)?.notes ? 'notes-error' : undefined}
          />
          {errors.notes && (
            <p className="mt-1 text-sm text-red-600">{errors.notes.message as string}</p>
          )}
        </div>

        <div className="flex justify-end space-x-3 mt-6">
          <Button type="button" variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isLoading}>
            {isEditMode ? 'Save Changes' : 'Add Fuel Log'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};
