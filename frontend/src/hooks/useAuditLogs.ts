import { useQuery } from '@tanstack/react-query';
import { getAuditLogs } from '../api/auditLogsApi';
import { AuditLogQuery } from '../types/auditLog';

export const useAuditLogsQuery = (query: AuditLogQuery) => {
  return useQuery({
    queryKey: ['auditLogs', query],
    queryFn: () => getAuditLogs(query),
  });
};
