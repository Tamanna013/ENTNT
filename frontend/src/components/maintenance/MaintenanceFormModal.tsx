import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Modal } from '../ui/Modal';
import { Button } from '../ui/Button';
import { Controller } from 'react-hook-form';
import { ShipSelect } from '../ships/ShipSelect';
import { MAINTENANCE_TYPES } from '../../lib/constants';
import { MaintenanceRecord, CreateMaintenanceRecordPayload, UpdateMaintenanceRecordPayload } from '../../types/maintenance';

interface MaintenanceFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateMaintenanceRecordPayload | UpdateMaintenanceRecordPayload) => Promise<void>;
  maintenanceRecord?: MaintenanceRecord | null;
  
}

export const MaintenanceFormModal: React.FC<MaintenanceFormModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  maintenanceRecord,
  
}) => {
  const schema = z.object({
    shipId: !maintenanceRecord ? z.string().min(1, 'Ship is required') : z.string().optional(),
    type: !maintenanceRecord ? z.enum(MAINTENANCE_TYPES as any) : z.string().optional(),
    description: z.string().min(1, 'Description is required').max(1000, 'Max 1000 characters'),
    scheduledDate: z.string().min(1, 'Scheduled Date is required').refine((val) => {
      if (!!maintenanceRecord) return true; // Rescheduling can be any date ideally (or less strictly prevented)
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      const inputDate = new Date(val);
      return inputDate >= today;
    }, { message: 'Scheduled date cannot be in the past' }),
    estimatedCost: z.coerce.number().min(0, 'Must be positive'),
    actualCost: z.any(),
    performedBy: z.any(),
  });

  type FormData = z.infer<typeof schema>;

  const { register, handleSubmit, formState: { errors, isLoading }, reset, control } = useForm<FormData>({
    resolver: zodResolver(schema as any),
    defaultValues: {
      shipId: maintenanceRecord?.shipId || '',
      type: maintenanceRecord?.type || MAINTENANCE_TYPES[0],
      description: maintenanceRecord?.description || '',
      scheduledDate: maintenanceRecord?.scheduledDate ? maintenanceRecord.scheduledDate.split('T')[0] : '',
      estimatedCost: maintenanceRecord?.estimatedCost || 0,
      actualCost: maintenanceRecord?.actualCost || '',
      performedBy: maintenanceRecord?.performedBy || '',
    }
  });

  React.useEffect(() => {
    if (isOpen) {
      reset({
        shipId: maintenanceRecord?.shipId || '',
        type: maintenanceRecord?.type || MAINTENANCE_TYPES[0],
        description: maintenanceRecord?.description || '',
        scheduledDate: maintenanceRecord?.scheduledDate ? maintenanceRecord.scheduledDate.split('T')[0] : '',
        estimatedCost: maintenanceRecord?.estimatedCost || 0,
        actualCost: maintenanceRecord?.actualCost || '',
        performedBy: maintenanceRecord?.performedBy || '',
      });
    }
  }, [isOpen, maintenanceRecord, reset]);

  const handleFormSubmit = async (data: any) => {
    const payload = !maintenanceRecord 
      ? {
          shipId: data.shipId!,
          type: data.type!,
          description: data.description,
          scheduledDate: new Date(data.scheduledDate).toISOString(),
          estimatedCost: data.estimatedCost,
          performedBy: data.performedBy || undefined
        } as CreateMaintenanceRecordPayload
      : {
          description: data.description,
          scheduledDate: new Date(data.scheduledDate).toISOString(),
          estimatedCost: data.estimatedCost,
          actualCost: data.actualCost !== '' && data.actualCost !== undefined ? Number(data.actualCost) : undefined,
          performedBy: data.performedBy || undefined
        } as UpdateMaintenanceRecordPayload;
        
    await onSubmit(payload);
    onClose();
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={!maintenanceRecord ? 'Schedule Maintenance' : 'Edit Maintenance Record'}
    >
      <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
        {!maintenanceRecord ? (
          <>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Ship</label>
              <Controller
                name="shipId"
                control={control}
                render={({ field }) => (
                  <ShipSelect value={field.value || ''} onChange={field.onChange} />
                )}
              />
              {errors.shipId && <p id="shipId-error" className="mt-1 text-sm text-red-500">{errors.shipId.message as string}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Type</label>
              <select
                {...register('type')} aria-invalid={!!(errors as any)?.type} aria-describedby={(errors as any)?.type ? 'type-error' : undefined}
                className={`w-full rounded-md border p-2 ${errors.type ? 'border-red-500' : 'border-slate-300'}`}
              >
                {MAINTENANCE_TYPES.map(t => (
                  <option key={t} value={t}>{t}</option>
                ))}
              </select>
              {errors.type && <p id="type-error" className="mt-1 text-sm text-red-500">{errors.type.message as string}</p>}
            </div>
          </>
        ) : (
          <>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Ship</label>
              <input type="text" value={maintenanceRecord?.shipName} disabled className="w-full rounded-md border p-2 bg-slate-100 text-text-muted" />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Type</label>
              <input type="text" value={maintenanceRecord?.type} disabled className="w-full rounded-md border p-2 bg-slate-100 text-text-muted" />
            </div>
          </>
        )}

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Description</label>
          <textarea
            {...register('description')} aria-invalid={!!(errors as any)?.description} aria-describedby={(errors as any)?.description ? 'description-error' : undefined}
            rows={3}
            className={`w-full rounded-md border p-2 ${errors.description ? 'border-red-500' : 'border-slate-300'}`}
          />
          {errors.description && <p id="description-error" className="mt-1 text-sm text-red-500">{errors.description.message as string}</p>}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Scheduled Date</label>
            <input
              type="date"
              {...register('scheduledDate')} aria-invalid={!!(errors as any)?.scheduledDate} aria-describedby={(errors as any)?.scheduledDate ? 'scheduledDate-error' : undefined}
              className={`w-full rounded-md border p-2 ${errors.scheduledDate ? 'border-red-500' : 'border-slate-300'}`}
            />
            {errors.scheduledDate && <p id="scheduledDate-error" className="mt-1 text-sm text-red-500">{errors.scheduledDate.message as string}</p>}
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Estimated Cost</label>
            <input
              type="number"
              step="0.01"
              {...register('estimatedCost')} aria-invalid={!!(errors as any)?.estimatedCost} aria-describedby={(errors as any)?.estimatedCost ? 'estimatedCost-error' : undefined}
              className={`w-full rounded-md border p-2 ${errors.estimatedCost ? 'border-red-500' : 'border-slate-300'}`}
            />
            {errors.estimatedCost && <p id="estimatedCost-error" className="mt-1 text-sm text-red-500">{errors.estimatedCost.message as string}</p>}
          </div>
        </div>

        {!!maintenanceRecord && (
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Actual Cost (Optional)</label>
            <input
              type="number"
              step="0.01"
              {...register('actualCost')} aria-invalid={!!(errors as any)?.actualCost} aria-describedby={(errors as any)?.actualCost ? 'actualCost-error' : undefined}
              className={`w-full rounded-md border p-2 ${errors.actualCost ? 'border-red-500' : 'border-slate-300'}`}
            />
            {errors.actualCost && <p id="actualCost-error" className="mt-1 text-sm text-red-500">{errors.actualCost.message as string}</p>}
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Performed By (Optional)</label>
          <input
            type="text"
            {...register('performedBy')} aria-invalid={!!(errors as any)?.performedBy} aria-describedby={(errors as any)?.performedBy ? 'performedBy-error' : undefined}
            className={`w-full rounded-md border p-2 ${errors.performedBy ? 'border-red-500' : 'border-slate-300'}`}
          />
          {errors.performedBy && <p id="performedBy-error" className="mt-1 text-sm text-red-500">{errors.performedBy.message as string}</p>}
        </div>

        <div className="mt-6 flex justify-end space-x-3">
          <Button type="button" variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isLoading}>
            {!maintenanceRecord ? 'Schedule' : 'Save Changes'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};
