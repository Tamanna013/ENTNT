import { PaginationQuery } from './pagination';

export interface Port {
  id: string;
  name: string;
  unLocode: string;
  country: string;
  city: string;
  latitude: number | null;
  longitude: number | null;
  createdAt: string;
}

export interface CreatePortPayload {
  name: string;
  unLocode: string;
  country: string;
  city: string;
  latitude?: number;
  longitude?: number;
}

export interface UpdatePortPayload {
  name: string;
  country: string;
  city: string;
  latitude?: number;
  longitude?: number;
}

export interface PortQuery extends PaginationQuery {
  country?: string;
}
