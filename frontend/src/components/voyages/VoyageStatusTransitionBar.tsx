import React from 'react';
import { Voyage } from '../../types/voyage';
import { getLegalNextStates, isTerminalStatus } from '../../lib/voyageStatusTransitions';
import { useUpdateVoyageStatusMutation } from '../../hooks/useVoyages';

interface VoyageStatusTransitionBarProps {
  voyage: Voyage;
}

export const VoyageStatusTransitionBar: React.FC<VoyageStatusTransitionBarProps> = ({ voyage }) => {
  const [actualArrivalDate, setActualArrivalDate] = React.useState<string>(
    new Date().toISOString().split('T')[0]
  );
  
  const updateStatusMutation = useUpdateVoyageStatusMutation(voyage.id);
  const legalNextStates = getLegalNextStates(voyage.status);

  if (isTerminalStatus(voyage.status) || legalNextStates.length === 0) {
    return (
      <div className="rounded-md bg-surface-hover p-4 border border-white/10 flex items-center justify-center">
        <p className="text-sm text-text-muted">This voyage has reached a final state.</p>
      </div>
    );
  }

  const handleTransition = (targetStatus: string) => {
    if (targetStatus === 'Completed') {
      updateStatusMutation.mutate({ status: targetStatus, actualArrivalDate: new Date(actualArrivalDate).toISOString() });
    } else {
      updateStatusMutation.mutate({ status: targetStatus });
    }
  };

  const getButtonLabel = (status: string) => {
    switch (status) {
      case 'InTransit': return 'Mark as InTransit';
      case 'Delayed': return 'Mark as Delayed';
      case 'Completed': return 'Mark as Completed';
      case 'Cancelled': return 'Cancel Voyage';
      default: return `Mark as ${status}`;
    }
  };

  const getButtonClass = (status: string) => {
    switch (status) {
      case 'Completed': return 'bg-emerald-600 hover:bg-emerald-500 focus-visible:outline-emerald-600';
      case 'Cancelled': return 'bg-rose-600 hover:bg-rose-500 focus-visible:outline-rose-600';
      case 'Delayed': return 'bg-amber-600 hover:bg-amber-500 focus-visible:outline-amber-600';
      case 'InTransit': return 'bg-purple-600 hover:bg-purple-500 focus-visible:outline-purple-600';
      default: return 'bg-indigo-600 hover:bg-indigo-500 focus-visible:outline-indigo-600';
    }
  };

  return (
    <div className="rounded-md bg-surface-hover p-4 border border-white/10 flex flex-wrap gap-4 items-center">
      <span className="text-sm font-medium text-text-primary">Update Status:</span>
      {legalNextStates.map((status) => (
        <div key={status} className="flex items-center gap-2">
          {status === 'Completed' && (
            <input
              type="date"
              className="block rounded-md border-0 bg-surface-hover py-1.5 px-3 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
              value={actualArrivalDate}
              onChange={(e) => setActualArrivalDate(e.target.value)}
              title="Actual Arrival Date"
            />
          )}
          <button
            onClick={() => handleTransition(status)}
            disabled={updateStatusMutation.isPending}
            className={`rounded-md px-3 py-2 text-sm font-semibold text-text-primary shadow-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 ${getButtonClass(status)} disabled:opacity-50`}
          >
            {updateStatusMutation.isPending ? 'Updating...' : getButtonLabel(status)}
          </button>
        </div>
      ))}
    </div>
  );
};
