import { PaginationQuery } from './pagination';

export interface Ship {
  id: string;
  fleetId: string;
  fleetName: string;
  name: string;
  imo: string;
  type: string;
  status: string;
  yearBuilt: number;
  grossTonnage: number;
  flag: string;
  primaryPhotoUrl: string | null;
  createdAt: string;
}

export interface CreateShipPayload {
  fleetId: string;
  name: string;
  imo: string;
  type: string;
  status: string;
  yearBuilt: number;
  grossTonnage: number;
  flag: string;
}

export interface UpdateShipPayload {
  name: string;
  type: string;
  status: string;
  yearBuilt: number;
  grossTonnage: number;
  flag: string;
}

export interface ShipQuery extends PaginationQuery {
  fleetId?: string;
  status?: string;
  type?: string;
}

export interface Attachment {
  id: string;
  entityName: string;
  entityId: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  downloadUrl: string;
  uploadedByUserId: string;
  createdAt: string;
}
