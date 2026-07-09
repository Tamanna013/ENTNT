import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Modal } from '../ui/Modal';
import { Port, CreatePortPayload, UpdatePortPayload } from '../../types/port';
import { useCreatePortMutation, useUpdatePortMutation } from '../../hooks/usePorts';

const portSchema = z.object({
  name: z.string().min(1, 'Name is required').max(150, 'Name must be at most 150 characters'),
  unLocode: z.string().regex(/^[A-Z0-9]{5}$/, 'UN/LOCODE must be exactly 5 uppercase alphanumeric characters'),
  country: z.string().min(1, 'Country is required').max(100, 'Country must be at most 100 characters'),
  city: z.string().min(1, 'City is required').max(100, 'City must be at most 100 characters'),
  latitude: z.number().min(-90).max(90).optional().nullable(),
  longitude: z.number().min(-180).max(180).optional().nullable()
});

interface PortFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  port?: Port;
}

export const PortFormModal: React.FC<PortFormModalProps> = ({ isOpen, onClose, port }) => {
  const isEditMode = !!port;
  
  const createMutation = useCreatePortMutation();
  const updateMutation = useUpdatePortMutation();

  const { register, handleSubmit, reset, formState: { errors } } = useForm({
    resolver: zodResolver(portSchema),
    defaultValues: {
      name: '',
      unLocode: '',
      country: '',
      city: '',
      latitude: null as number | null,
      longitude: null as number | null
    }
  });

  useEffect(() => {
    if (isOpen) {
      if (isEditMode && port) {
        reset({
          name: port.name,
          unLocode: port.unLocode,
          country: port.country,
          city: port.city,
          latitude: port.latitude,
          longitude: port.longitude
        });
      } else {
        reset({
          name: '',
          unLocode: '',
          country: '',
          city: '',
          latitude: null,
          longitude: null
        });
      }
    }
  }, [isOpen, isEditMode, port, reset]);

  const onSubmit = async (data: any) => {
    try {
      if (isEditMode && port) {
        const payload: UpdatePortPayload = {
          name: data.name,
          country: data.country,
          city: data.city,
          latitude: data.latitude !== null && data.latitude !== undefined && data.latitude !== '' ? Number(data.latitude) : undefined,
          longitude: data.longitude !== null && data.longitude !== undefined && data.longitude !== '' ? Number(data.longitude) : undefined
        };
        await updateMutation.mutateAsync({ id: port.id, payload });
      } else {
        const payload: CreatePortPayload = {
          name: data.name,
          unLocode: data.unLocode,
          country: data.country,
          city: data.city,
          latitude: data.latitude !== null && data.latitude !== undefined && data.latitude !== '' ? Number(data.latitude) : undefined,
          longitude: data.longitude !== null && data.longitude !== undefined && data.longitude !== '' ? Number(data.longitude) : undefined
        };
        await createMutation.mutateAsync(payload);
      }
      onClose();
    } catch (error: any) {
      console.error('Failed to save port:', error);
      alert(error.response?.data?.message || 'Failed to save port');
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={isEditMode ? 'Edit Port' : 'Create Port'}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        
        <div>
          <label className="block text-sm font-medium text-text-primary">Name</label>
          <input
            type="text"
            {...register('name')} aria-invalid={!!(errors as any)?.name} aria-describedby={(errors as any)?.name ? 'name-error' : undefined}
            className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
          />
          {errors.name && <p id="name-error" className="mt-1 text-sm text-red-400">{errors.name.message as string}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium text-text-primary">UN/LOCODE</label>
          <input
            type="text"
            {...register('unLocode')} aria-invalid={!!(errors as any)?.unLocode} aria-describedby={(errors as any)?.unLocode ? 'unLocode-error' : undefined}
            disabled={isEditMode}
            onChange={(e) => {
              // Convert to uppercase
              e.target.value = e.target.value.toUpperCase();
            }}
            className={`mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2 ${isEditMode ? 'opacity-60 cursor-not-allowed' : ''}`}
          />
          {errors.unLocode && <p id="unLocode-error" className="mt-1 text-sm text-red-400">{errors.unLocode.message as string}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium text-text-primary">Country</label>
          <input
            type="text"
            {...register('country')} aria-invalid={!!(errors as any)?.country} aria-describedby={(errors as any)?.country ? 'country-error' : undefined}
            className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
          />
          {errors.country && <p id="country-error" className="mt-1 text-sm text-red-400">{errors.country.message as string}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium text-text-primary">City</label>
          <input
            type="text"
            {...register('city')} aria-invalid={!!(errors as any)?.city} aria-describedby={(errors as any)?.city ? 'city-error' : undefined}
            className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
          />
          {errors.city && <p id="city-error" className="mt-1 text-sm text-red-400">{errors.city.message as string}</p>}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-text-primary">Latitude</label>
            <input
              type="number"
              step="any"
              {...register('latitude', { valueAsNumber: true })}
              className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
            />
            {errors.latitude && <p id="latitude-error" className="mt-1 text-sm text-red-400">{errors.latitude.message as string}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-text-primary">Longitude</label>
            <input
              type="number"
              step="any"
              {...register('longitude', { valueAsNumber: true })}
              className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
            />
            {errors.longitude && <p id="longitude-error" className="mt-1 text-sm text-red-400">{errors.longitude.message as string}</p>}
          </div>
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
            {createMutation.isPending || updateMutation.isPending ? 'Saving...' : 'Save Port'}
          </button>
        </div>
      </form>
    </Modal>
  );
};
