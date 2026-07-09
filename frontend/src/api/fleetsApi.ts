import { apiClient as client } from './client';
import { PagedResult } from '../types/pagination';
import { Fleet, CreateFleetPayload, UpdateFleetPayload, FleetQuery } from '../types/fleet';

export const fleetsApi = {
  getFleets: async (query: FleetQuery): Promise<PagedResult<Fleet>> => {
    const params = new URLSearchParams();
    params.append('pageNumber', query.pageNumber.toString());
    params.append('pageSize', query.pageSize.toString());
    
    if (query.sortBy) params.append('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params.append('searchTerm', query.searchTerm);
    if (query.status) params.append('status', query.status);

    const response = await client.get<PagedResult<Fleet>>(`/fleets?${params.toString()}`);
    return response.data;
  },

  getFleetById: async (id: string): Promise<Fleet> => {
    const response = await client.get<Fleet>(`/fleets/${id}`);
    return response.data;
  },

  createFleet: async (payload: CreateFleetPayload): Promise<Fleet> => {
    const response = await client.post<Fleet>('/fleets', payload);
    return response.data;
  },

  updateFleet: async (id: string, payload: UpdateFleetPayload): Promise<Fleet> => {
    const response = await client.put<Fleet>(`/fleets/${id}`, payload);
    return response.data;
  },

  deactivateFleet: async (id: string): Promise<void> => {
    await client.delete(`/fleets/${id}`);
  }
};
