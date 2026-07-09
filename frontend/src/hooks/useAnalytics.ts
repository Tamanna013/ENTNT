import { useQuery } from '@tanstack/react-query';
import { analyticsApi } from '../api/analyticsApi';
import { 
  FleetSummaryAnalytics,
  ShipUtilizationTrendPoint,
  VoyagePerformanceTrendPoint,
  CrewComplianceTrendPoint,
  MaintenanceCostTrendPoint
} from '../types/analytics';

export const useFleetSummaryQuery = () => {
  return useQuery<FleetSummaryAnalytics, Error>({
    queryKey: ['analytics', 'fleet-summary'],
    queryFn: () => analyticsApi.getFleetSummary(),
    staleTime: 60000, // 1 minute stale time given it's cached server-side
  });
};

export const useShipUtilizationTrendQuery = (months: number) => {
  return useQuery<ShipUtilizationTrendPoint[], Error>({
    queryKey: ['analytics', 'ship-utilization-trend', months],
    queryFn: () => analyticsApi.getShipUtilizationTrend(months),
    staleTime: 60000,
  });
};

export const useVoyagePerformanceTrendQuery = (months: number) => {
  return useQuery<VoyagePerformanceTrendPoint[], Error>({
    queryKey: ['analytics', 'voyage-performance', months],
    queryFn: () => analyticsApi.getVoyagePerformanceTrend(months),
    staleTime: 60000,
  });
};

export const useCrewComplianceTrendQuery = (months: number) => {
  return useQuery<CrewComplianceTrendPoint[], Error>({
    queryKey: ['analytics', 'crew-compliance', months],
    queryFn: () => analyticsApi.getCrewComplianceTrend(months),
    staleTime: 60000,
  });
};

export const useMaintenanceCostTrendQuery = (months: number) => {
  return useQuery<MaintenanceCostTrendPoint[], Error>({
    queryKey: ['analytics', 'maintenance-cost-trend', months],
    queryFn: () => analyticsApi.getMaintenanceCostTrend(months),
    staleTime: 60000,
  });
};

export const useFinancialSummaryTrendQuery = (months: number = 12) => {
  return useQuery({
    queryKey: ['analytics', 'financial-summary', months],
    queryFn: () => analyticsApi.getFinancialSummaryTrend(months),
    staleTime: 5 * 60 * 1000, 
  });
};

export const useAiInsightsQuery = (months: number = 12) => {
  return useQuery({
    queryKey: ['analytics', 'ai-insights', months],
    queryFn: () => analyticsApi.getAiInsights(months),
    staleTime: 30000,
    refetchOnWindowFocus: true,
  });
};
