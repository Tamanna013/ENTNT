import { apiClient as api } from './client';
import { PagedResult } from '../types/pagination';
import { 
  MaintenanceRecord, 
  CreateMaintenanceRecordPayload, 
  UpdateMaintenanceRecordPayload, 
  UpdateMaintenanceStatusPayload,
  MaintenanceQuery
} from '../types/maintenance';

export const maintenanceApi = {
  getMaintenanceRecords: async (query?: MaintenanceQuery) => {
    const params = new URLSearchParams();
    if (query) {
      if (query.pageNumber) params.append('pageNumber', query.pageNumber.toString());
      if (query.pageSize) params.append('pageSize', query.pageSize.toString());
      if (query.sortBy) params.append('sortBy', query.sortBy);
      if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
      if (query.searchTerm) params.append('searchTerm', query.searchTerm);
      if (query.shipId) params.append('shipId', query.shipId);
      if (query.status) params.append('status', query.status);
      if (query.type) params.append('type', query.type);
      if (query.dueBefore) params.append('dueBefore', query.dueBefore);
    }
    const response = await api.get<PagedResult<MaintenanceRecord>>(`/maintenance?${params.toString()}`);
    return response.data;
  },

  getMaintenanceRecordById: async (id: string) => {
    const response = await api.get<MaintenanceRecord>(`/maintenance/${id}`);
    return response.data;
  },

  createMaintenanceRecord: async (payload: CreateMaintenanceRecordPayload) => {
    const response = await api.post<MaintenanceRecord>('/maintenance', payload);
    return response.data;
  },

  updateMaintenanceRecord: async (id: string, payload: UpdateMaintenanceRecordPayload) => {
    const response = await api.put<MaintenanceRecord>(`/maintenance/${id}`, payload);
    return response.data;
  },

  deleteMaintenanceRecord: async (id: string) => {
    await api.delete(`/maintenance/${id}`);
  },

  updateMaintenanceStatus: async (id: string, payload: UpdateMaintenanceStatusPayload) => {
    const response = await api.put<MaintenanceRecord>(`/maintenance/${id}/status`, payload);
    return response.data;
  },

  getMaintenanceForShip: async (shipId: string, query?: MaintenanceQuery) => {
    const params = new URLSearchParams();
    if (query) {
      if (query.pageNumber) params.append('pageNumber', query.pageNumber.toString());
      if (query.pageSize) params.append('pageSize', query.pageSize.toString());
      if (query.sortBy) params.append('sortBy', query.sortBy);
      if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
      if (query.searchTerm) params.append('searchTerm', query.searchTerm);
      if (query.status) params.append('status', query.status);
      if (query.type) params.append('type', query.type);
      if (query.dueBefore) params.append('dueBefore', query.dueBefore);
    }
    const response = await api.get<PagedResult<MaintenanceRecord>>(`/ships/${shipId}/maintenance?${params.toString()}`);
    return response.data;
  }
};
