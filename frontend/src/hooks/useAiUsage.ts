import { useQuery } from '@tanstack/react-query';
import { getUsageReport } from '../api/aiUsageApi';

export const useAiUsageReportQuery = (dateFrom?: string, dateTo?: string) => {
  return useQuery({
    queryKey: ['ai-usage-report', dateFrom, dateTo],
    queryFn: () => getUsageReport(dateFrom, dateTo),
  });
};
