import { apiClient } from './client';
import { 
  FleetSummaryAnalytics, 
  ShipUtilizationTrendPoint,
  VoyagePerformanceTrendPoint,
  CrewComplianceTrendPoint,
  MaintenanceCostTrendPoint,
  FinancialSummaryTrendPoint
} from '../types/analytics';
import { AiSummaryResult } from '../types/ai';

export const analyticsApi = {
  getFleetSummary: async (): Promise<FleetSummaryAnalytics> => {
    const response = await apiClient.get('/analytics/fleet-summary');
    return response.data;
  },
  
  getShipUtilizationTrend: async (months: number): Promise<ShipUtilizationTrendPoint[]> => {
    const response = await apiClient.get(`/analytics/ship-utilization-trend?months=${months}`);
    return response.data;
  },

  getVoyagePerformanceTrend: async (months: number): Promise<VoyagePerformanceTrendPoint[]> => {
    const response = await apiClient.get(`/analytics/voyage-performance?months=${months}`);
    return response.data;
  },

  exportAnalyticsExcel: async (months: number): Promise<Blob> => {
    const response = await apiClient.get(`/analytics/export-excel?months=${months}`, {
      responseType: 'blob'
    });
    return response.data;
  },

  getAiInsights: async (months: number): Promise<AiSummaryResult> => {
    const response = await apiClient.get<AiSummaryResult>(`/analytics/ai-insights?months=${months}`);
    return response.data;
  },

  getCrewComplianceTrend: async (months: number): Promise<CrewComplianceTrendPoint[]> => {
    const response = await apiClient.get(`/analytics/crew-compliance?months=${months}`);
    return response.data;
  },

  getMaintenanceCostTrend: async (months: number): Promise<MaintenanceCostTrendPoint[]> => {
    const response = await apiClient.get(`/analytics/maintenance-cost-trend?months=${months}`);
    return response.data;
  },

  getFinancialSummaryTrend: async (months: number): Promise<FinancialSummaryTrendPoint[]> => {
    const response = await apiClient.get(`/analytics/financial-summary?months=${months}`);
    return response.data;
  }
};
