import { apiClient } from './client';
import { 
  Container, 
  ContainerQuery, 
  CreateContainerPayload, 
  UpdateContainerPayload,
  ContainerTrackingEvent,
  RecordTrackingEventPayload
} from '../types/container';
import { PagedResult } from '../types/pagination';

export const containersApi = {
  getContainers: async (query: ContainerQuery): Promise<PagedResult<Container>> => {
    const params = new URLSearchParams();
    if (query.pageNumber) params.append('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params.append('pageSize', query.pageSize.toString());
    if (query.sortBy) params.append('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params.append('searchTerm', query.searchTerm);
    if (query.status) params.append('status', query.status);
    if (query.type) params.append('type', query.type);
    if (query.voyageId) params.append('voyageId', query.voyageId);

    const response = await apiClient.get(`/containers?${params.toString()}`);
    return response.data;
  },

  getContainerById: async (id: string): Promise<Container> => {
    const response = await apiClient.get(`/containers/${id}`);
    return response.data;
  },

  createContainer: async (payload: CreateContainerPayload): Promise<Container> => {
    const response = await apiClient.post('/containers', payload);
    return response.data;
  },

  updateContainer: async (id: string, payload: UpdateContainerPayload): Promise<Container> => {
    const response = await apiClient.put(`/containers/${id}`, payload);
    return response.data;
  },

  deleteContainer: async (id: string): Promise<void> => {
    await apiClient.delete(`/containers/${id}`);
  },

  linkCargo: async (containerId: string, cargoId: string): Promise<Container> => {
    const response = await apiClient.post(`/containers/${containerId}/cargo`, { cargoId });
    return response.data;
  },

  unlinkCargo: async (containerId: string, cargoId: string): Promise<Container> => {
    const response = await apiClient.delete(`/containers/${containerId}/cargo/${cargoId}`);
    return response.data;
  },

  recordTrackingEvent: async (containerId: string, payload: RecordTrackingEventPayload): Promise<ContainerTrackingEvent> => {
    const response = await apiClient.post(`/containers/${containerId}/tracking`, payload);
    return response.data;
  },

  getTrackingEvents: async (containerId: string): Promise<ContainerTrackingEvent[]> => {
    const response = await apiClient.get(`/containers/${containerId}/tracking`);
    return response.data;
  }
};
