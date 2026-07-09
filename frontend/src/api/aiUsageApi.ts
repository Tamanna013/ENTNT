import { AiUsageReportRow } from '../types/aiUsage';
import { apiClient as client } from './client';

export const getUsageReport = async (dateFrom?: string, dateTo?: string): Promise<AiUsageReportRow[]> => {
  const params = new URLSearchParams();
  if (dateFrom) params.append('dateFrom', dateFrom);
  if (dateTo) params.append('dateTo', dateTo);

  const response = await client.get<AiUsageReportRow[]>(`/ai/usage-report?${params.toString()}`);
  return response.data;
};
