/*
 * This mirrors the backend's VoyageStatusTransitions graph as a UI convenience for
 * showing only relevant status buttons. The backend is the actual source of truth
 * and independently enforces this same graph - if the backend's transitions ever change,
 * this file must be updated to match, or the UI will show buttons for transitions
 * the server will reject. A future improvement could have VoyageDto expose
 * legalNextStates directly from the backend to eliminate this duplication risk.
 */

export const VOYAGE_STATUS_TRANSITIONS: Record<string, string[]> = {
  Scheduled: ["InTransit", "Cancelled", "Delayed"],
  Delayed: ["InTransit", "Cancelled"],
  InTransit: ["Completed", "Delayed"],
  Completed: [],
  Cancelled: []
};

export function getLegalNextStates(currentStatus: string): string[] {
  return VOYAGE_STATUS_TRANSITIONS[currentStatus] || [];
}

export function isTerminalStatus(status: string): boolean {
  return status === "Completed" || status === "Cancelled";
}
