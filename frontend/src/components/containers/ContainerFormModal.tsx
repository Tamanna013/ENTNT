import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { Container, CreateContainerPayload, UpdateContainerPayload } from '../../types/container';
import { CONTAINER_TYPES, CONTAINER_STATUSES } from '../../lib/constants';
import { VoyageSelect } from '../voyages/VoyageSelect';

const containerSchema = z.object({
  containerNumber: z.string().optional(),
  type: z.string().min(1, 'Type is required'),
  status: z.string().optional(),
  currentVoyageId: z.string().optional(),
});

interface ContainerFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  container?: Container;
  onSubmit: (payload: any) => void;
  isLoading: boolean;
}

export const ContainerFormModal: React.FC<ContainerFormModalProps> = ({
  isOpen,
  onClose,
  container,
  onSubmit,
  isLoading,
}) => {
  const isEditMode = !!container;

  const { register, handleSubmit, control, reset, formState: { errors } } = useForm({
    resolver: zodResolver(containerSchema),
    defaultValues: {
      containerNumber: '',
      type: 'Dry20ft',
      status: 'Empty',
      currentVoyageId: '',
    }
  });

  useEffect(() => {
    if (isOpen) {
      if (container) {
        reset({
          containerNumber: container.containerNumber,
          type: container.type,
          status: container.status,
          currentVoyageId: container.currentVoyageId || '',
        });
      } else {
        reset({
          containerNumber: '',
          type: 'Dry20ft',
          status: 'Empty',
          currentVoyageId: '',
        });
      }
    }
  }, [isOpen, container, reset]);

  if (!isOpen) return null;

  const onSubmitForm = (data: any) => {
    if (isEditMode) {
      const payload: UpdateContainerPayload = {
        type: data.type,
        status: data.status,
        currentVoyageId: data.currentVoyageId || undefined,
      };
      onSubmit(payload);
    } else {
      const payload: CreateContainerPayload = {
        containerNumber: data.containerNumber,
        type: data.type,
        currentVoyageId: data.currentVoyageId || undefined,
      };
      onSubmit(payload);
    }
  };

  return (
    <div className="relative z-50" aria-labelledby="modal-title" role="dialog" aria-modal="true">
      <div className="fixed inset-0 bg-surface bg-opacity-75 transition-opacity"></div>
      <div className="fixed inset-0 z-10 w-screen overflow-y-auto">
        <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
          <div className="relative transform overflow-hidden rounded-lg bg-surface border border-white/10 px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
            <div className="sm:flex sm:items-start">
              <div className="mt-3 text-center sm:ml-4 sm:mt-0 sm:text-left w-full">
                <h3 className="text-base font-semibold leading-6 text-text-primary" id="modal-title">
                  {isEditMode ? 'Edit Container' : 'Add Container'}
                </h3>
                <div className="mt-4">
                  <form onSubmit={handleSubmit(onSubmitForm)} className="space-y-4">
                    
                    {!isEditMode ? (
                      <div>
                        <label className="block text-sm font-medium leading-6 text-text-primary">Container Number</label>
                        <input
                          type="text"
                          {...register('containerNumber')} aria-invalid={!!(errors as any)?.containerNumber} aria-describedby={(errors as any)?.containerNumber ? 'containerNumber-error' : undefined}
                          className="mt-2 block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
                        />
                        {errors.containerNumber && <p id="containerNumber-error" className="mt-1 text-sm text-red-500">{errors.containerNumber.message as string}</p>}
                      </div>
                    ) : (
                      <div>
                        <label className="block text-sm font-medium leading-6 text-text-muted">Container Number</label>
                        <p className="mt-1 text-sm text-text-primary">{container.containerNumber}</p>
                      </div>
                    )}

                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium leading-6 text-text-primary">Type</label>
                        <select
                          {...register('type')} aria-invalid={!!(errors as any)?.type} aria-describedby={(errors as any)?.type ? 'type-error' : undefined}
                          className="mt-2 block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
                        >
                          {CONTAINER_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                        </select>
                        {errors.type && <p id="type-error" className="mt-1 text-sm text-red-500">{errors.type.message as string}</p>}
                      </div>

                      {isEditMode && (
                        <div>
                          <label className="block text-sm font-medium leading-6 text-text-primary">Status</label>
                          <select
                            {...register('status')} aria-invalid={!!(errors as any)?.status} aria-describedby={(errors as any)?.status ? 'status-error' : undefined}
                            className="mt-2 block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
                          >
                            {CONTAINER_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
                          </select>
                          {errors.status && <p id="status-error" className="mt-1 text-sm text-red-500">{errors.status.message as string}</p>}
                        </div>
                      )}
                    </div>

                    <div>
                      <Controller
                        name="currentVoyageId"
                        control={control}
                        render={({ field }) => (
                          <VoyageSelect value={field.value || ''} onChange={field.onChange} label="Current Voyage (Optional)" />
                        )}
                      />
                    </div>

                    <div className="mt-5 sm:mt-4 sm:flex sm:flex-row-reverse">
                      <button
                        type="submit"
                        disabled={isLoading}
                        className="inline-flex w-full justify-center rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 disabled:opacity-50 sm:ml-3 sm:w-auto"
                      >
                        {isLoading ? 'Saving...' : 'Save'}
                      </button>
                      <button
                        type="button"
                        onClick={onClose}
                        className="mt-3 inline-flex w-full justify-center rounded-md bg-white/10 px-3 py-2 text-sm font-semibold text-text-primary shadow-sm hover:bg-white/20 sm:mt-0 sm:w-auto"
                      >
                        Cancel
                      </button>
                    </div>
                  </form>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
