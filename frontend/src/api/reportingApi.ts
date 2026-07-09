import { apiClient } from './client';
import { FleetUtilizationReportRow, VoyageManifestReportRow, FuelEfficiencyReportRow } from '../types/reporting';

export const reportingApi = {
  getFleetUtilizationReport: async (): Promise<FleetUtilizationReportRow[]> => {
    const response = await apiClient.get<FleetUtilizationReportRow[]>('/reporting/fleet-utilization');
    return response.data;
  },

  getVoyageManifestReport: async (voyageId: string): Promise<VoyageManifestReportRow[]> => {
    const response = await apiClient.get<VoyageManifestReportRow[]>(`/reporting/voyage-manifest/${voyageId}`);
    return response.data;
  },

  getFuelEfficiencyReport: async (trailingDays: number): Promise<FuelEfficiencyReportRow[]> => {
    const response = await apiClient.get<FuelEfficiencyReportRow[]>('/reporting/fuel-efficiency', {
      params: { trailingDays }
    });
    return response.data;
  }
};
