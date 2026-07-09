import { PaginationQuery } from './pagination';

export interface Fleet {
  id: string;
  name: string;
  description: string | null;
  homePortId: string;
  homePortName: string;
  status: string;
  shipCount: number;
  createdAt: string;
}

export interface CreateFleetPayload {
  name: string;
  description?: string;
  homePortId: string;
  status: string;
}

export interface UpdateFleetPayload {
  name: string;
  description?: string;
  homePortId: string;
  status: string;
}

export interface FleetQuery extends PaginationQuery {
  status?: string;
}
