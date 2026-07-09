import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { Cargo, CreateCargoPayload, UpdateCargoPayload } from '../../types/cargo';
import { useCreateCargoMutation, useUpdateCargoMutation } from '../../hooks/useCargo';
import { useToast } from '../../hooks/useToast';
import { Modal } from '../ui/Modal';
import { Button } from '../ui/Button';
import { VoyageSelect } from '../voyages/VoyageSelect';
import { CARGO_TYPES, CARGO_STATUSES } from '../../lib/constants';

interface CargoFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  cargo?: Cargo;
}

const formSchema = z.object({
  description: z.string().min(1, 'Description is required'),
  voyageId: z.string().optional(),
  type: z.string().min(1, 'Type is required'),
  status: z.string().optional(),
  weightKg: z.coerce.number().min(0, 'Weight must be positive'),
  declaredValue: z.coerce.number().min(0, 'Value must be positive'),
  consigneeName: z.string().min(1, 'Consignee is required'),
  hazardNotes: z.string().optional(),
}).superRefine((data, ctx) => {
  if (data.type === 'Hazardous' && (!data.hazardNotes || data.hazardNotes.trim() === '')) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      path: ['hazardNotes'],
      message: 'Hazard notes are required for hazardous cargo',
    });
  }
});


export const CargoFormModal: React.FC<CargoFormModalProps> = ({ isOpen, onClose, cargo }) => {
  const isEditMode = !!cargo;
  const { showToast } = useToast();
  
  const createMutation = useCreateCargoMutation();
  const updateMutation = useUpdateCargoMutation();

  const {
    register,
    handleSubmit,
    control,
    watch,
    reset,
    formState: { errors }
  } = useForm({
    resolver: zodResolver(formSchema),
    defaultValues: {
      description: '',
      voyageId: '',
      type: 'GeneralGoods',
      status: 'Pending',
      weightKg: 0,
      declaredValue: 0,
      consigneeName: '',
      hazardNotes: '',
    }
  });

  const selectedType = watch('type');
  const showHazardNotes = selectedType === 'Hazardous';

  useEffect(() => {
    if (isOpen) {
      if (cargo) {
        reset({
          description: cargo.description,
          voyageId: cargo.voyageId || '',
          type: cargo.type,
          status: cargo.status,
          weightKg: cargo.weightKg,
          declaredValue: cargo.declaredValue,
          consigneeName: cargo.consigneeName,
          hazardNotes: cargo.hazardNotes || '',
        });
      } else {
        reset({
          description: '',
          voyageId: '',
          type: 'GeneralGoods',
          status: 'Pending',
          weightKg: 0,
          declaredValue: 0,
          consigneeName: '',
          hazardNotes: '',
        });
      }
    }
  }, [isOpen, cargo, reset]);

  const onSubmit = async (data: any) => {
    try {
      if (isEditMode) {
        const payload: UpdateCargoPayload = {
          description: data.description,
          status: data.status || 'Pending',
          weightKg: data.weightKg,
          declaredValue: data.declaredValue,
          consigneeName: data.consigneeName,
          hazardNotes: data.hazardNotes,
        };
        await updateMutation.mutateAsync({ id: cargo.id, payload });
        showToast('Cargo updated successfully', 'success');
      } else {
        if (!data.voyageId) {
          showToast('Voyage is required when creating cargo', 'error');
          return;
        }
        const payload: CreateCargoPayload = {
          voyageId: data.voyageId,
          description: data.description,
          type: data.type,
          weightKg: data.weightKg,
          declaredValue: data.declaredValue,
          consigneeName: data.consigneeName,
          hazardNotes: data.hazardNotes,
        };
        await createMutation.mutateAsync(payload);
        showToast('Cargo created successfully', 'success');
      }
      onClose();
    } catch (error: any) {
      showToast(error.response?.data?.message || 'An error occurred', 'error');
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={isEditMode ? 'Edit Cargo' : 'Add Cargo'}
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        
        {!isEditMode && (
          <Controller
            name="voyageId"
            control={control}
            render={({ field }) => (
              <VoyageSelect
                value={field.value || ''}
                onChange={field.onChange}
                error={errors.voyageId?.message}
                label="Voyage *"
              />
            )}
          />
        )}

        <div>
          <label className="block text-sm font-medium text-text-primary mb-1">Description *</label>
          <textarea
            {...register('description')} aria-invalid={!!(errors as any)?.description} aria-describedby={(errors as any)?.description ? 'description-error' : undefined}
            className="block w-full rounded-md border-0 py-1.5 text-slate-100 bg-surface shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
            rows={2}
          />
          {errors.description && <p id="description-error" className="mt-1 text-sm text-red-500">{errors.description.message}</p>}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">Type {isEditMode ? '' : '*'}</label>
            <select
              {...register('type')} aria-invalid={!!(errors as any)?.type} aria-describedby={(errors as any)?.type ? 'type-error' : undefined}
              disabled={isEditMode}
              className="block w-full rounded-md border-0 py-1.5 text-slate-100 bg-surface shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6 disabled:opacity-50"
            >
              {CARGO_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
            </select>
            {errors.type && <p id="type-error" className="mt-1 text-sm text-red-500">{errors.type.message}</p>}
          </div>

          {isEditMode && (
            <div>
              <label className="block text-sm font-medium text-text-primary mb-1">Status</label>
              <select
                {...register('status')} aria-invalid={!!(errors as any)?.status} aria-describedby={(errors as any)?.status ? 'status-error' : undefined}
                className="block w-full rounded-md border-0 py-1.5 text-slate-100 bg-surface shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
              >
                {CARGO_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
              </select>
              {errors.status && <p id="status-error" className="mt-1 text-sm text-red-500">{errors.status.message}</p>}
            </div>
          )}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">Weight (Kg) *</label>
            <input
              type="number"
              {...register('weightKg')} aria-invalid={!!(errors as any)?.weightKg} aria-describedby={(errors as any)?.weightKg ? 'weightKg-error' : undefined}
              className="block w-full rounded-md border-0 py-1.5 text-slate-100 bg-surface shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
            />
            {errors.weightKg && <p id="weightKg-error" className="mt-1 text-sm text-red-500">{errors.weightKg.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">Declared Value (USD) *</label>
            <input
              type="number"
              {...register('declaredValue')} aria-invalid={!!(errors as any)?.declaredValue} aria-describedby={(errors as any)?.declaredValue ? 'declaredValue-error' : undefined}
              className="block w-full rounded-md border-0 py-1.5 text-slate-100 bg-surface shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
            />
            {errors.declaredValue && <p id="declaredValue-error" className="mt-1 text-sm text-red-500">{errors.declaredValue.message}</p>}
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-text-primary mb-1">Consignee Name *</label>
          <input
            type="text"
            {...register('consigneeName')} aria-invalid={!!(errors as any)?.consigneeName} aria-describedby={(errors as any)?.consigneeName ? 'consigneeName-error' : undefined}
            className="block w-full rounded-md border-0 py-1.5 text-slate-100 bg-surface shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
          />
          {errors.consigneeName && <p id="consigneeName-error" className="mt-1 text-sm text-red-500">{errors.consigneeName.message}</p>}
        </div>

        {showHazardNotes && (
          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">Hazard Notes *</label>
            <textarea
              {...register('hazardNotes')} aria-invalid={!!(errors as any)?.hazardNotes} aria-describedby={(errors as any)?.hazardNotes ? 'hazardNotes-error' : undefined}
              className="block w-full rounded-md border-0 py-1.5 text-slate-100 bg-surface shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
              rows={3}
              placeholder="Provide detailed hazard information..."
            />
            {errors.hazardNotes && <p id="hazardNotes-error" className="mt-1 text-sm text-red-500">{errors.hazardNotes.message}</p>}
          </div>
        )}

        <div className="mt-6 flex justify-end gap-3">
          <Button variant="secondary" onClick={onClose} type="button">
            Cancel
          </Button>
          <Button type="submit" isLoading={isPending}>
            {isEditMode ? 'Save Changes' : 'Create Cargo'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};
