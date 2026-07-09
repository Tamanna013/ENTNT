import { apiClient } from './client';
import { PagedResult } from '../types/pagination';
import { Ship, ShipQuery, CreateShipPayload, UpdateShipPayload, Attachment } from '../types/ship';
import { AiRecommendationResult } from '../types/ai';

export const shipsApi = {
  getShips: async (query: ShipQuery): Promise<PagedResult<Ship>> => {
    const params = new URLSearchParams();
    if (query.pageNumber) params.append('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params.append('pageSize', query.pageSize.toString());
    if (query.sortBy) params.append('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params.append('searchTerm', query.searchTerm);
    if (query.fleetId) params.append('fleetId', query.fleetId);
    if (query.status) params.append('status', query.status);
    if (query.type) params.append('type', query.type);

    const response = await apiClient.get(`/ships?${params.toString()}`);
    return response.data;
  },

  getShipById: async (id: string): Promise<Ship> => {
    const response = await apiClient.get(`/ships/${id}`);
    return response.data;
  },

  createShip: async (payload: CreateShipPayload): Promise<Ship> => {
    const response = await apiClient.post('/ships', payload);
    return response.data;
  },

  updateShip: async (id: string, payload: UpdateShipPayload): Promise<Ship> => {
    const response = await apiClient.put(`/ships/${id}`, payload);
    return response.data;
  },

  deactivateShip: async (id: string): Promise<void> => {
    await apiClient.delete(`/ships/${id}`);
  },

  getShipAttachments: async (shipId: string): Promise<Attachment[]> => {
    const response = await apiClient.get(`/ships/${shipId}/attachments`);
    return response.data;
  },

  uploadShipAttachment: async (shipId: string, file: File): Promise<Attachment> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiClient.post(`/ships/${shipId}/attachments`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  setPrimaryPhoto: async (shipId: string, attachmentId: string): Promise<Ship> => {
    const response = await apiClient.put(`/ships/${shipId}/primary-photo`, { attachmentId });
    return response.data;
  },

  getAiMaintenanceRecommendations: async (id: string): Promise<AiRecommendationResult> => {
    const response = await apiClient.get<AiRecommendationResult>(`/ships/${id}/ai-maintenance-recommendations`);
    return response.data;
  }
};
