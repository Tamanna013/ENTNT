import { PaginationQuery } from './pagination';

export interface AuditLog {
  id: string;
  userId: string | null;
  userName: string;
  action: string;
  entityName: string;
  entityId: string;
  changes: string | null;
  timestamp: string;
}

export interface AuditLogQuery extends PaginationQuery {
  entityName?: string;
  action?: string;
  userId?: string;
  dateFrom?: string;
  dateTo?: string;
}
