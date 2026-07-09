import { apiClient as client } from './client';
import { PagedResult } from '../types/pagination';
import { Port, CreatePortPayload, UpdatePortPayload, PortQuery } from '../types/port';

export const portsApi = {
  getPorts: async (query: PortQuery): Promise<PagedResult<Port>> => {
    const params = new URLSearchParams();
    params.append('pageNumber', query.pageNumber.toString());
    params.append('pageSize', query.pageSize.toString());
    
    if (query.sortBy) params.append('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params.append('searchTerm', query.searchTerm);
    if (query.country) params.append('country', query.country);

    const response = await client.get<PagedResult<Port>>(`/ports?${params.toString()}`);
    return response.data;
  },

  getPortById: async (id: string): Promise<Port> => {
    const response = await client.get<Port>(`/ports/${id}`);
    return response.data;
  },

  createPort: async (payload: CreatePortPayload): Promise<Port> => {
    const response = await client.post<Port>('/ports', payload);
    return response.data;
  },

  updatePort: async (id: string, payload: UpdatePortPayload): Promise<Port> => {
    const response = await client.put<Port>(`/ports/${id}`, payload);
    return response.data;
  },

  deactivatePort: async (id: string): Promise<void> => {
    await client.delete(`/ports/${id}`);
  }
};
