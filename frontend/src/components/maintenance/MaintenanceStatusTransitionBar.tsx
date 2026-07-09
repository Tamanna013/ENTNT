import React, { useState } from 'react';
import { MaintenanceRecord, UpdateMaintenanceStatusPayload } from '../../types/maintenance';
import { getLegalNextMaintenanceStates } from '../../lib/maintenanceStatusTransitions';
import { Button } from '../ui/Button';

interface MaintenanceStatusTransitionBarProps {
  record: MaintenanceRecord;
  onUpdateStatus: (payload: UpdateMaintenanceStatusPayload) => Promise<void>;
  isLoading?: boolean;
}

export const MaintenanceStatusTransitionBar: React.FC<MaintenanceStatusTransitionBarProps> = ({
  record,
  onUpdateStatus,
  isLoading
}) => {
  const [actualCost, setActualCost] = useState<string>('');
  const [completedDate, setCompletedDate] = useState<string>(new Date().toISOString().split('T')[0]);
  const [showCompletionFields, setShowCompletionFields] = useState(false);

  const legalNextStates = getLegalNextMaintenanceStates(record.status);

  if (legalNextStates.length === 0 && record.status !== 'Overdue') {
    return null;
  }

  const handleTransition = async (status: string) => {
    if (status === 'Completed' && !showCompletionFields) {
      setShowCompletionFields(true);
      return;
    }

    const payload: UpdateMaintenanceStatusPayload = { status };
    if (status === 'Completed') {
      if (actualCost) payload.actualCost = Number(actualCost);
      if (completedDate) payload.completedDate = new Date(completedDate).toISOString();
    }
    
    await onUpdateStatus(payload);
    setShowCompletionFields(false);
  };

  return (
    <div className="bg-slate-50 border rounded-lg p-4 mb-6">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <h3 className="text-sm font-medium text-slate-900">Update Status</h3>
          <p className="text-sm text-text-muted">
            Current status is <span className="font-semibold">{record.status}</span>.
          </p>
          {record.status === 'Overdue' && (
            <p className="text-sm text-red-600 font-medium mt-1">
              This maintenance is overdue. Please update its status once work begins or is completed.
            </p>
          )}
        </div>
        
        <div className="flex flex-wrap gap-2">
          {showCompletionFields ? (
            <div className="flex items-center gap-2 mr-4">
              <input 
                type="number" 
                placeholder="Actual Cost" 
                className="rounded-md border p-2 text-sm w-32"
                value={actualCost}
                onChange={(e) => setActualCost(e.target.value)}
              />
              <input 
                type="date" 
                className="rounded-md border p-2 text-sm"
                value={completedDate}
                onChange={(e) => setCompletedDate(e.target.value)}
              />
            </div>
          ) : null}

          {legalNextStates.map(state => {
            // CRITICAL: Ensure Overdue is never rendered as a target button
            if (state === 'Overdue') return null;

            let label = state;
            let variant: 'primary' | 'secondary' | undefined = 'primary';
            
            if (state === 'InProgress') {
              label = 'Start Maintenance';
            } else if (state === 'Completed') {
              label = showCompletionFields ? 'Confirm Completion' : 'Mark as Completed';
              variant = 'primary';
            } else if (state === 'Cancelled') {
              label = 'Cancel';
              variant = 'secondary';
            }

            return (
              <Button
                key={state}
                onClick={() => handleTransition(state)}
                isLoading={isLoading}
                variant={variant}
              >
                {label}
              </Button>
            );
          })}

          {showCompletionFields && (
            <Button variant="secondary" onClick={() => setShowCompletionFields(false)}>
              Back
            </Button>
          )}
        </div>
      </div>
    </div>
  );
};
