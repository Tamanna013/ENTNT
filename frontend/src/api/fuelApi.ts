import { apiClient } from './client';
import { PagedResult } from '../types/pagination';
import { FuelLog, CreateFuelLogPayload, UpdateFuelLogPayload, FuelLogQuery } from '../types/fuel';

export const fuelApi = {
  getFuelLogs: async (query?: FuelLogQuery) => {
    const response = await apiClient.get<PagedResult<FuelLog>>('/fuel', { params: query });
    return response.data;
  },

  getFuelLogById: async (id: string) => {
    const response = await apiClient.get<FuelLog>(`/fuel/${id}`);
    return response.data;
  },

  createFuelLog: async (payload: CreateFuelLogPayload) => {
    const response = await apiClient.post<FuelLog>('/fuel', payload);
    return response.data;
  },

  updateFuelLog: async (id: string, payload: UpdateFuelLogPayload) => {
    const response = await apiClient.put<FuelLog>(`/fuel/${id}`, payload);
    return response.data;
  },

  deleteFuelLog: async (id: string) => {
    await apiClient.delete(`/fuel/${id}`);
  },

  getFuelLogsForShip: async (shipId: string, query?: FuelLogQuery) => {
    const response = await apiClient.get<PagedResult<FuelLog>>(`/ships/${shipId}/fuel-logs`, { params: query });
    return response.data;
  },
};
