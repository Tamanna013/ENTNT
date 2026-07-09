import { PaginationQuery } from './pagination';

export interface FuelLog {
  id: string;
  shipId: string;
  shipName: string;
  voyageId: string | null;
  voyageNumber: string | null;
  fuelType: string;
  quantityLiters: number;
  costPerLiter: number;
  totalCost: number;
  recordedDate: string;
  notes: string | null;
  createdAt: string;
}

export interface CreateFuelLogPayload {
  shipId: string;
  voyageId?: string;
  fuelType: string;
  quantityLiters: number;
  costPerLiter: number;
  recordedDate: string;
  notes?: string;
}

export interface UpdateFuelLogPayload {
  quantityLiters: number;
  costPerLiter: number;
  recordedDate: string;
  notes?: string;
}

export interface FuelLogQuery extends PaginationQuery {
  shipId?: string;
  voyageId?: string;
  fuelType?: string;
}
