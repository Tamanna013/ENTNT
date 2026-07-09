import { PaginationQuery } from './pagination';

export interface MaintenanceRecord {
  id: string;
  shipId: string;
  shipName: string;
  type: string;
  status: string;
  description: string;
  scheduledDate: string;
  completedDate: string | null;
  estimatedCost: number;
  actualCost: number | null;
  performedBy: string | null;
  createdAt: string;
}

export interface CreateMaintenanceRecordPayload {
  shipId: string;
  type: string;
  description: string;
  scheduledDate: string;
  estimatedCost: number;
  performedBy?: string;
}

export interface UpdateMaintenanceRecordPayload {
  description: string;
  scheduledDate: string;
  estimatedCost: number;
  actualCost?: number;
  performedBy?: string;
}

export interface UpdateMaintenanceStatusPayload {
  status: string;
  actualCost?: number;
  completedDate?: string;
}

export interface MaintenanceQuery extends PaginationQuery {
  shipId?: string;
  status?: string;
  type?: string;
  dueBefore?: string;
}
