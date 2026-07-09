import React from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useRecordTrackingEventMutation } from '../../hooks/useContainers';
import { Button } from '../ui/Button';

const eventSchema = z.object({
  eventType: z.string().min(1, 'Event type is required').max(100),
  location: z.string().min(1, 'Location is required').max(150),
  timestamp: z.string().min(1, 'Timestamp is required'),
  notes: z.string().max(1000).optional(),
});

type EventFormValues = z.infer<typeof eventSchema>;

interface RecordTrackingEventFormProps {
  containerId: string;
}

export const RecordTrackingEventForm: React.FC<RecordTrackingEventFormProps> = ({ containerId }) => {
  const mutation = useRecordTrackingEventMutation(containerId);

  // Default timestamp to current local time in "YYYY-MM-DDThh:mm" format for datetime-local input
  const now = new Date();
  const defaultTimestamp = new Date(now.getTime() - now.getTimezoneOffset() * 60000)
    .toISOString()
    .slice(0, 16);

  const { register, handleSubmit, reset, formState: { errors } } = useForm<EventFormValues>({
    resolver: zodResolver(eventSchema),
    defaultValues: {
      eventType: '',
      location: '',
      timestamp: defaultTimestamp,
      notes: '',
    }
  });

  const onSubmit = async (data: EventFormValues) => {
    await mutation.mutateAsync({
      eventType: data.eventType,
      location: data.location,
      timestamp: new Date(data.timestamp).toISOString(),
      notes: data.notes || undefined,
    });
    reset({ ...data, eventType: '', notes: '', timestamp: defaultTimestamp }); // reset to defaults keeping location maybe? actually reset all
  };

  return (
    <div className="bg-surface rounded-lg shadow-sm border border-border p-4 mb-6">
      <h4 className="text-sm font-semibold text-text-primary mb-4">Record New Tracking Event</h4>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div>
            <label className="block text-xs font-medium text-text-muted mb-1">Event Type</label>
            <input
              type="text"
              list="event-types"
              placeholder="e.g. Arrived, Inspected"
              {...register('eventType')}
              className="block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
            />
            <datalist id="event-types">
              <option value="Loaded" />
              <option value="Departed" />
              <option value="In Transit" />
              <option value="Arrived" />
              <option value="At Port" />
              <option value="Inspected" />
              <option value="Delivered" />
            </datalist>
            {errors.eventType && <p className="mt-1 text-xs text-red-500">{errors.eventType.message}</p>}
          </div>
          <div>
            <label className="block text-xs font-medium text-text-muted mb-1">Location</label>
            <input
              type="text"
              placeholder="e.g. Port of Seattle"
              {...register('location')}
              className="block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
            />
            {errors.location && <p className="mt-1 text-xs text-red-500">{errors.location.message}</p>}
          </div>
          <div>
            <label className="block text-xs font-medium text-text-muted mb-1">Timestamp</label>
            <input
              type="datetime-local"
              {...register('timestamp')}
              className="block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
            />
            {errors.timestamp && <p className="mt-1 text-xs text-red-500">{errors.timestamp.message}</p>}
          </div>
        </div>
        <div>
          <label className="block text-xs font-medium text-text-muted mb-1">Notes (Optional)</label>
          <input
            type="text"
            {...register('notes')}
            className="block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
          />
          {errors.notes && <p className="mt-1 text-xs text-red-500">{errors.notes.message}</p>}
        </div>
        <div className="flex justify-end">
          <Button type="submit" isLoading={mutation.isPending}>
            Record Event
          </Button>
        </div>
      </form>
    </div>
  );
};
