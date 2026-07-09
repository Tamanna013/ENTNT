import { PaginationQuery } from './pagination';

export interface Incident {
  id: string;
  shipId: string;
  shipName: string;
  voyageId: string | null;
  voyageNumber: string | null;
  title: string;
  description: string;
  severity: string;
  status: string;
  reportedByUserId: string;
  reportedByUserName: string;
  occurredAt: string;
  resolvedAt: string | null;
  resolutionNotes: string | null;
  createdAt: string;
}

export interface CreateIncidentPayload {
  shipId: string;
  voyageId?: string | null;
  title: string;
  description: string;
  severity: string;
  occurredAt: string;
}

export interface UpdateIncidentPayload {
  title: string;
  description: string;
  severity: string;
  resolutionNotes?: string | null;
}

export interface UpdateIncidentStatusPayload {
  status: string;
  resolvedAt?: string | null;
  resolutionNotes?: string | null;
}

export interface IncidentQuery extends PaginationQuery {
  shipId?: string;
  status?: string;
  severity?: string;
}
