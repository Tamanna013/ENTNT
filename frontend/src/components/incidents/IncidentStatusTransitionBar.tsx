import { useState } from 'react';
import { useUpdateIncidentStatusMutation } from '../../hooks/useIncidents';
import { getLegalNextStates } from '../../lib/incidentStatusTransitions';
import { Button } from '../ui/Button';
import { ArrowRight, AlertCircle } from 'lucide-react';
import { Modal } from '../ui/Modal';

interface IncidentStatusTransitionBarProps {
  incidentId: string;
  currentStatus: string;
}

export function IncidentStatusTransitionBar({ incidentId, currentStatus }: IncidentStatusTransitionBarProps) {
  const updateStatusMutation = useUpdateIncidentStatusMutation(incidentId);
  const nextStates = getLegalNextStates(currentStatus);
  const [isResolveModalOpen, setIsResolveModalOpen] = useState(false);
  const [resolutionNotes, setResolutionNotes] = useState('');

  if (nextStates.length === 0) {
    return (
      <div className="bg-gray-50 border border-gray-200 rounded-lg p-4 flex items-center text-gray-500">
        <AlertCircle className="h-5 w-5 mr-2" />
        This incident is closed and cannot transition further.
      </div>
    );
  }

  const handleTransition = async (nextStatus: string, notes?: string) => {
    try {
      await updateStatusMutation.mutateAsync({ 
        status: nextStatus,
        resolutionNotes: notes
      });
      if (nextStatus === 'Resolved') {
        setIsResolveModalOpen(false);
      }
    } catch (error) {
      console.error('Failed to transition status:', error);
    }
  };

  const onStateClick = (state: string) => {
    if (state === 'Resolved') {
      setIsResolveModalOpen(true);
    } else {
      handleTransition(state);
    }
  };

  const getButtonVariant = (): 'primary' | 'secondary' => {
    return 'primary';
  };

  return (
    <>
      <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm flex items-center justify-between">
        <div>
          <h4 className="text-sm font-medium text-gray-700 mb-1">Update Status</h4>
          <p className="text-xs text-gray-500">
            Current status: <span className="font-semibold">{currentStatus}</span>
          </p>
        </div>
        <div className="flex gap-2">
          {nextStates.map((state) => (
            <Button
              key={state}
              variant={getButtonVariant()}
              className="text-sm py-1 px-3"
              onClick={() => onStateClick(state)}
              disabled={updateStatusMutation.isPending}
            >
              Mark as {state}
              <ArrowRight className="h-4 w-4 ml-1.5" />
            </Button>
          ))}
        </div>
      </div>

      <Modal
        isOpen={isResolveModalOpen}
        onClose={() => {
          setIsResolveModalOpen(false);
          setResolutionNotes('');
        }}
        title="Resolve Incident"
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Resolution Notes (Optional)</label>
            <textarea
              className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              rows={4}
              placeholder="Explain how the incident was resolved..."
              value={resolutionNotes}
              onChange={(e) => setResolutionNotes(e.target.value)}
            />
          </div>
          <div className="flex justify-end gap-3 pt-4 border-t">
            <Button
              variant="secondary"
              onClick={() => {
                setIsResolveModalOpen(false);
                setResolutionNotes('');
              }}
            >
              Cancel
            </Button>
            <Button
              variant="primary"
              onClick={() => handleTransition('Resolved', resolutionNotes)}
              disabled={updateStatusMutation.isPending}
            >
              Resolve Incident
            </Button>
          </div>
        </div>
      </Modal>
    </>
  );
}
