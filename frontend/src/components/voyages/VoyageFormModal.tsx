import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Voyage, CreateVoyagePayload, UpdateVoyagePayload } from '../../types/voyage';
import { ShipSelect } from '../ships/ShipSelect';
import { PortSelect } from '../ports/PortSelect';
import { isTerminalStatus } from '../../lib/voyageStatusTransitions';
import { useToast } from '../../hooks/useToast';

const voyageSchema = z.object({
  shipId: z.string().min(1, 'Ship is required'),
  voyageNumber: z.string().min(1, 'Voyage number is required').max(30),
  originPortId: z.string().min(1, 'Origin port is required'),
  destinationPortId: z.string().min(1, 'Destination port is required'),
  departureDate: z.string().min(1, 'Departure date is required'),
  estimatedArrivalDate: z.string().min(1, 'Estimated arrival date is required'),
  notes: z.string().max(2000).optional(),
}).refine(data => data.originPortId !== data.destinationPortId, {
  message: "Origin port and destination port cannot be identical",
  path: ["destinationPortId"],
}).refine(data => new Date(data.estimatedArrivalDate) > new Date(data.departureDate), {
  message: "Estimated arrival date must be after departure date",
  path: ["estimatedArrivalDate"],
});

type VoyageFormData = z.infer<typeof voyageSchema>;

interface VoyageFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  voyage?: Voyage; // If provided, edit mode. Otherwise, create mode.
  onSubmit: (data: CreateVoyagePayload | UpdateVoyagePayload) => void;
  isLoading: boolean;
}

export const VoyageFormModal: React.FC<VoyageFormModalProps> = ({
  isOpen,
  onClose,
  voyage,
  onSubmit,
  isLoading
}) => {
  const isEditMode = !!voyage;
  const isTerminal = voyage ? isTerminalStatus(voyage.status) : false;
  const { showToast } = useToast();

  const { register, handleSubmit, control, reset, formState: { errors } } = useForm<VoyageFormData>({
    resolver: zodResolver(voyageSchema),
    defaultValues: {
      shipId: '',
      voyageNumber: '',
      originPortId: '',
      destinationPortId: '',
      departureDate: '',
      estimatedArrivalDate: '',
      notes: '',
    }
  });

  useEffect(() => {
    if (isOpen) {
      if (voyage) {
        reset({
          shipId: voyage.shipId,
          voyageNumber: voyage.voyageNumber,
          originPortId: voyage.originPortId,
          destinationPortId: voyage.destinationPortId,
          departureDate: voyage.departureDate.split('T')[0],
          estimatedArrivalDate: voyage.estimatedArrivalDate.split('T')[0],
          notes: voyage.notes || '',
        });
      } else {
        reset({
          shipId: '',
          voyageNumber: '',
          originPortId: '',
          destinationPortId: '',
          departureDate: '',
          estimatedArrivalDate: '',
          notes: '',
        });
      }
    }
  }, [isOpen, voyage, reset]);

  if (!isOpen) return null;

  const onSubmitForm = (data: VoyageFormData) => {
    if (isTerminal) {
      showToast('Cannot modify a voyage that has already completed or been cancelled.', 'error');
      return;
    }

    if (isEditMode) {
      const payload: UpdateVoyagePayload = {
        originPortId: data.originPortId,
        destinationPortId: data.destinationPortId,
        departureDate: new Date(data.departureDate).toISOString(),
        estimatedArrivalDate: new Date(data.estimatedArrivalDate).toISOString(),
        notes: data.notes,
      };
      onSubmit(payload);
    } else {
      const payload: CreateVoyagePayload = {
        shipId: data.shipId,
        voyageNumber: data.voyageNumber,
        originPortId: data.originPortId,
        destinationPortId: data.destinationPortId,
        departureDate: new Date(data.departureDate).toISOString(),
        estimatedArrivalDate: new Date(data.estimatedArrivalDate).toISOString(),
        notes: data.notes,
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
            <div className="absolute right-0 top-0 hidden pr-4 pt-4 sm:block">
              <button
                type="button"
                className="rounded-md bg-surface text-text-muted hover:text-gray-300 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                onClick={onClose}
              >
                <span className="sr-only">Close</span>
                <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <div className="sm:flex sm:items-start">
              <div className="mt-3 text-center sm:ml-4 sm:mt-0 sm:text-left w-full">
                <h3 className="text-base font-semibold leading-6 text-text-primary" id="modal-title">
                  {isEditMode ? 'Edit Voyage' : 'Schedule Voyage'}
                </h3>
                
                {isTerminal && (
                  <div className="mt-2 bg-red-500/10 border border-red-500/20 rounded-md p-3">
                    <p className="text-sm text-red-400 font-medium">This voyage is in a terminal state and cannot be edited.</p>
                  </div>
                )}

                <div className="mt-4">
                  <form onSubmit={handleSubmit(onSubmitForm)} className="space-y-4">
                    
                    {!isEditMode && (
                      <>
                        <div>
                          <label className="block text-sm font-medium leading-6 text-text-primary">Ship</label>
                          <div className="mt-2">
                            <Controller
                              name="shipId"
                              control={control}
                              render={({ field }) => (
                                <ShipSelect value={field.value} onChange={field.onChange} />
                              )}
                            />
                            {errors.shipId && <p id="shipId-error" className="mt-1 text-sm text-red-500">{errors.shipId.message}</p>}
                          </div>
                        </div>

                        <div>
                          <label htmlFor="voyageNumber" className="block text-sm font-medium leading-6 text-text-primary">Voyage Number</label>
                          <div className="mt-2">
                            <input
                              type="text"
                              id="voyageNumber"
                              disabled={isTerminal}
                              {...register('voyageNumber')} aria-invalid={!!(errors as any)?.voyageNumber} aria-describedby={(errors as any)?.voyageNumber ? 'voyageNumber-error' : undefined}
                              className="block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6 disabled:opacity-50"
                            />
                            {errors.voyageNumber && <p id="voyageNumber-error" className="mt-1 text-sm text-red-500">{errors.voyageNumber.message}</p>}
                          </div>
                        </div>
                      </>
                    )}

                    {isEditMode && (
                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <label className="block text-sm font-medium leading-6 text-text-muted">Ship</label>
                          <p className="mt-1 text-sm text-text-primary">{voyage?.shipName}</p>
                        </div>
                        <div>
                          <label className="block text-sm font-medium leading-6 text-text-muted">Voyage Number</label>
                          <p className="mt-1 text-sm text-text-primary">{voyage?.voyageNumber}</p>
                        </div>
                      </div>
                    )}

                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <PortSelect
                          label="Origin Port"
                          disabled={isTerminal}
                          {...register('originPortId')}
                          error={errors.originPortId?.message}
                        />
                      </div>

                      <div>
                        <PortSelect
                          label="Destination Port"
                          disabled={isTerminal}
                          {...register('destinationPortId')}
                          error={errors.destinationPortId?.message}
                        />
                      </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label htmlFor="departureDate" className="block text-sm font-medium leading-6 text-text-primary">Departure Date</label>
                        <div className="mt-2">
                          <input
                            type="date"
                            id="departureDate"
                            disabled={isTerminal}
                            {...register('departureDate')} aria-invalid={!!(errors as any)?.departureDate} aria-describedby={(errors as any)?.departureDate ? 'departureDate-error' : undefined}
                            className="block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6 disabled:opacity-50"
                          />
                          {errors.departureDate && <p id="departureDate-error" className="mt-1 text-sm text-red-500">{errors.departureDate.message}</p>}
                        </div>
                      </div>

                      <div>
                        <label htmlFor="estimatedArrivalDate" className="block text-sm font-medium leading-6 text-text-primary">Estimated Arrival</label>
                        <div className="mt-2">
                          <input
                            type="date"
                            id="estimatedArrivalDate"
                            disabled={isTerminal}
                            {...register('estimatedArrivalDate')} aria-invalid={!!(errors as any)?.estimatedArrivalDate} aria-describedby={(errors as any)?.estimatedArrivalDate ? 'estimatedArrivalDate-error' : undefined}
                            className="block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6 disabled:opacity-50"
                          />
                          {errors.estimatedArrivalDate && <p id="estimatedArrivalDate-error" className="mt-1 text-sm text-red-500">{errors.estimatedArrivalDate.message}</p>}
                        </div>
                      </div>
                    </div>

                    <div>
                      <label htmlFor="notes" className="block text-sm font-medium leading-6 text-text-primary">Notes</label>
                      <div className="mt-2">
                        <textarea
                          id="notes"
                          rows={3}
                          disabled={isTerminal}
                          {...register('notes')} aria-invalid={!!(errors as any)?.notes} aria-describedby={(errors as any)?.notes ? 'notes-error' : undefined}
                          className="block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6 disabled:opacity-50"
                        />
                        {errors.notes && <p id="notes-error" className="mt-1 text-sm text-red-500">{errors.notes.message}</p>}
                      </div>
                    </div>

                    <div className="mt-5 sm:mt-4 sm:flex sm:flex-row-reverse">
                      <button
                        type="submit"
                        disabled={isLoading || isTerminal}
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
