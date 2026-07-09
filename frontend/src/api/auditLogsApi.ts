import { apiClient } from './client';
import { PagedResult } from '../types/pagination';
import { AuditLog, AuditLogQuery } from '../types/auditLog';

export const getAuditLogs = async (query: AuditLogQuery): Promise<PagedResult<AuditLog>> => {
  const params = new URLSearchParams();
  if (query.pageNumber) params.append('pageNumber', query.pageNumber.toString());
  if (query.pageSize) params.append('pageSize', query.pageSize.toString());
  if (query.entityName) params.append('entityName', query.entityName);
  if (query.action) params.append('action', query.action);
  if (query.userId) params.append('userId', query.userId);
  if (query.dateFrom) params.append('dateFrom', query.dateFrom);
  if (query.dateTo) params.append('dateTo', query.dateTo);

  const response = await apiClient.get<PagedResult<AuditLog>>(`/audit-logs?${params.toString()}`);
  return response.data;
};
