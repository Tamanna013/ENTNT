import { useQuery } from '@tanstack/react-query';
import { reportingApi } from '../api/reportingApi';

export const useFleetUtilizationReportQuery = () => {
  return useQuery({
    queryKey: ['reporting', 'fleet-utilization'],
    queryFn: reportingApi.getFleetUtilizationReport,
  });
};

export function useVoyageManifestReportQuery(voyageId: string) {
  return useQuery({
    queryKey: ['reporting', 'voyage-manifest', voyageId],
    queryFn: () => reportingApi.getVoyageManifestReport(voyageId),
    enabled: !!voyageId,
  });
}

export function useFuelEfficiencyReportQuery(trailingDays: number) {
  return useQuery({
    queryKey: ['reporting', 'fuel-efficiency', trailingDays],
    queryFn: () => reportingApi.getFuelEfficiencyReport(trailingDays),
  });
};
