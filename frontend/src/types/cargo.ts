import { PaginationQuery } from './pagination';

export interface Cargo {
  id: string;
  voyageId: string;
  voyageNumber: string | null;
  description: string;
  type: string;
  status: string;
  weightKg: number;
  declaredValue: number;
  consigneeName: string;
  hazardNotes: string | null;
  createdAt: string;
  warnings: string[];
}

export interface CreateCargoPayload {
  voyageId: string;
  description: string;
  type: string;
  weightKg: number;
  declaredValue: number;
  consigneeName: string;
  hazardNotes?: string;
}

export interface UpdateCargoPayload {
  description: string;
  status: string;
  weightKg: number;
  declaredValue: number;
  consigneeName: string;
  hazardNotes?: string;
}

export interface CargoQuery extends PaginationQuery {
  voyageId?: string;
  status?: string;
  type?: string;
}
