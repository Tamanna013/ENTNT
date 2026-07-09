import { PaginationQuery } from './pagination';

export interface Voyage {
  id: string;
  shipId: string;
  shipName: string;
  voyageNumber: string;
  originPortId: string;
  originPortName: string;
  destinationPortId: string;
  destinationPortName: string;
  departureDate: string;
  estimatedArrivalDate: string;
  actualArrivalDate: string | null;
  status: string;
  notes: string | null;
  createdAt: string;
}

export interface CreateVoyagePayload {
  shipId: string;
  voyageNumber: string;
  originPortId: string;
  destinationPortId: string;
  departureDate: string;
  estimatedArrivalDate: string;
  notes?: string;
}

export interface UpdateVoyagePayload {
  originPortId: string;
  destinationPortId: string;
  departureDate: string;
  estimatedArrivalDate: string;
  notes?: string;
}

export interface UpdateVoyageStatusPayload {
  status: string;
  actualArrivalDate?: string;
}

export interface VoyageQuery extends PaginationQuery {
  shipId?: string;
  status?: string;
  departureFrom?: string;
  departureTo?: string;
}
