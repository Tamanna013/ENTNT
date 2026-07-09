// Mirrors backend FleetMind.Api.Common.IncidentStatusTransitions
// IMPORTANT: Keep in sync with backend logic

const LegalTransitions: Record<string, string[]> = {
  Reported: ['UnderInvestigation', 'Closed'],
  UnderInvestigation: ['Resolved', 'Closed'],
  Resolved: ['Closed'],
  Closed: [],
};

export const getLegalNextStates = (currentStatus: string): string[] => {
  return LegalTransitions[currentStatus] || [];
};

export const isLegalTransition = (currentStatus: string, nextStatus: string): boolean => {
  if (currentStatus === nextStatus) return true;
  const nextStates = getLegalNextStates(currentStatus);
  return nextStates.includes(nextStatus);
};

export const isTerminalStatus = (status: string): boolean => {
  return getLegalNextStates(status).length === 0;
};
