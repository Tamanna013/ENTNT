import { useState } from 'react';
import { z } from 'zod';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Modal } from '../ui/Modal';
import { Button } from '../ui/Button';
import { Input } from '../ui/Input';
import { ShipSelect } from '../ships/ShipSelect';
import { useVoyagesQuery } from '../../hooks/useVoyages';
import { useCreateIncidentMutation } from '../../hooks/useIncidents';
import { INCIDENT_SEVERITIES } from '../../lib/constants';

const reportIncidentSchema = z.object({
  shipId: z.string().min(1, 'Ship is required'),
  voyageId: z.string().optional().nullable(),
  title: z.string().min(1, 'Title is required').max(200, 'Title is too long'),
  description: z.string().min(1, 'Description is required').max(2000, 'Description is too long'),
  severity: z.string().min(1, 'Severity is required'),
  occurredAt: z.string().min(1, 'Occurrence time is required')
}).refine((data) => {
    const occurred = new Date(data.occurredAt);
    const nowPlusGrace = new Date(Date.now() + 5 * 60000);
    return occurred <= nowPlusGrace;
}, {
    message: "Occurred time cannot be in the future",
    path: ["occurredAt"]
});

type ReportIncidentFormData = z.infer<typeof reportIncidentSchema>;

interface ReportIncidentModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function ReportIncidentModal({ isOpen, onClose }: ReportIncidentModalProps) {
  const createMutation = useCreateIncidentMutation();
  const [selectedShipId, setSelectedShipId] = useState<string>('');

  const { data: voyagesData } = useVoyagesQuery({
    pageNumber: 1,
    pageSize: 100,
    shipId: selectedShipId || undefined,
    sortBy: 'departureDate',
    sortDescending: true,
  });

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ReportIncidentFormData>({
    resolver: zodResolver(reportIncidentSchema),
    defaultValues: {
      occurredAt: new Date().toISOString().slice(0, 16),
      severity: 'Low',
    },
  });

  const handleClose = () => {
    reset();
    setSelectedShipId('');
    onClose();
  };

  const onSubmit = async (data: ReportIncidentFormData) => {
    try {
      await createMutation.mutateAsync({
        ...data,
        voyageId: data.voyageId || null,
      });
      handleClose();
    } catch (error) {
      console.error('Failed to report incident:', error);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title="Report Incident"
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div className="space-y-2">
            <label className="text-sm font-medium text-gray-700">Ship *</label>
            <ShipSelect
              value={watch('shipId') || ''}
              onChange={(value) => {
                setValue('shipId', value);
                setSelectedShipId(value);
                setValue('voyageId', null); // Reset voyage when ship changes
              }}
            />
            {errors.shipId && <p className="mt-1 text-sm text-red-600">{errors.shipId.message as string}</p>}
          </div>

          <div className="space-y-2">
            <label className="text-sm font-medium text-gray-700">Voyage (Optional)</label>
            <select
              className="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md shadow-sm"
              {...register('voyageId')}
              disabled={!selectedShipId}
            >
              <option value="">— None —</option>
              {voyagesData?.items.map((voyage) => (
                <option key={voyage.id} value={voyage.id}>
                  {voyage.voyageNumber} ({voyage.originPortName} to {voyage.destinationPortName})
                </option>
              ))}
            </select>
            {errors.voyageId && <p className="mt-1 text-sm text-red-600">{errors.voyageId.message as string}</p>}
            {!selectedShipId && <p className="mt-1 text-xs text-gray-500">Select a ship first to view its voyages.</p>}
          </div>
        </div>

        <Input
          label="Title *"
          {...register('title')}
          error={errors.title?.message}
        />

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Description *</label>
          <textarea
            className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
            rows={4}
            {...register('description')}
          />
          {errors.description && <p className="mt-1 text-sm text-red-600">{errors.description.message as string}</p>}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="space-y-2">
            <label className="text-sm font-medium text-gray-700">Severity *</label>
            <select
              className={`block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm ${ errors.severity ? 'border-red-300 focus:border-red-500 focus:ring-red-500' : '' }`}
              {...register('severity')}
            >
              <option value="">Select Severity...</option>
              {INCIDENT_SEVERITIES.map((severity) => (
                <option key={severity} value={severity}>
                  {severity}
                </option>
              ))}
            </select>
            {errors.severity && (
              <p className="mt-1 text-sm text-red-600">{errors.severity.message}</p>
            )}
          </div>

          <Input
            label="Occurred At *"
            type="datetime-local"
            {...register('occurredAt')}
            error={errors.occurredAt?.message}
          />
        </div>

        <div className="flex justify-end gap-3 pt-4 border-t">
          <Button type="button" variant="secondary" onClick={handleClose}>
            Cancel
          </Button>
          <Button
            type="submit"
            disabled={isSubmitting || createMutation.isPending}
            variant="primary"
          >
            {isSubmitting || createMutation.isPending ? 'Reporting...' : 'Report Incident'}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
