export const MAINTENANCE_STATUS_GRAPH: Record<string, string[]> = {
  Scheduled: ['InProgress', 'Cancelled'],
  InProgress: ['Completed', 'Cancelled'],
  Completed: [],
  Cancelled: [],
  Overdue: ['InProgress', 'Cancelled']
};

/**
 * WARNING: This frontend status graph mirrors the backend MaintenanceStatusTransitions.cs.
 * If you add new states or transitions to the backend, you MUST update them here to ensure
 * the UI presents the correct buttons.
 * 
 * IMPORTANT: "Overdue" is system-only and must never be offered as a transition button target
 * anywhere in this UI, matching the backend's validator-level exclusion.
 * It is included as a key above so that a record currently IN Overdue status can still be
 * transitioned to InProgress or Cancelled.
 */
export const getLegalNextMaintenanceStates = (currentStatus: string): string[] => {
  return MAINTENANCE_STATUS_GRAPH[currentStatus] || [];
};

export const isTerminalMaintenanceStatus = (status: string): boolean => {
  const nextStates = getLegalNextMaintenanceStates(status);
  return nextStates.length === 0;
};
