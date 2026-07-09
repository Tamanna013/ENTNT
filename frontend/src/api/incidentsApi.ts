import { apiClient } from './client';
import { PagedResult } from '../types/pagination';
import { AiSummaryResult } from '../types/ai';
import {
  Incident,
  CreateIncidentPayload,
  UpdateIncidentPayload,
  UpdateIncidentStatusPayload,
  IncidentQuery,
} from '../types/incident';

export const incidentsApi = {
  getIncidents: async (query: IncidentQuery) => {
    const params = new URLSearchParams();
    if (query.pageNumber) params.append('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params.append('pageSize', query.pageSize.toString());
    if (query.sortBy) params.append('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params.append('searchTerm', query.searchTerm);
    if (query.shipId) params.append('shipId', query.shipId);
    if (query.status) params.append('status', query.status);
    if (query.severity) params.append('severity', query.severity);

    const response = await apiClient.get<PagedResult<Incident>>(`/incidents?${params.toString()}`);
    return response.data;
  },

  getIncidentById: async (id: string) => {
    const response = await apiClient.get<Incident>(`/incidents/${id}`);
    return response.data;
  },

  createIncident: async (payload: CreateIncidentPayload) => {
    const response = await apiClient.post<Incident>('/incidents', payload);
    return response.data;
  },

  updateIncident: async (id: string, payload: UpdateIncidentPayload) => {
    const response = await apiClient.put<Incident>(`/incidents/${id}`, payload);
    return response.data;
  },

  updateIncidentStatus: async (id: string, payload: UpdateIncidentStatusPayload) => {
    const response = await apiClient.put<Incident>(`/incidents/${id}/status`, payload);
    return response.data;
  },

  getAiReport: async (id: string): Promise<AiSummaryResult> => {
    const response = await apiClient.get<AiSummaryResult>(`/incidents/${id}/ai-report`);
    return response.data;
  },

  deleteIncident: async (id: string) => {
    await apiClient.delete(`/incidents/${id}`);
  },

  getIncidentsForShip: async (shipId: string, query: IncidentQuery) => {
    const params = new URLSearchParams();
    if (query.pageNumber) params.append('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params.append('pageSize', query.pageSize.toString());
    if (query.sortBy) params.append('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params.append('searchTerm', query.searchTerm);
    if (query.status) params.append('status', query.status);
    if (query.severity) params.append('severity', query.severity);

    const response = await apiClient.get<PagedResult<Incident>>(`/ships/${shipId}/incidents?${params.toString()}`);
    return response.data;
  },
};
