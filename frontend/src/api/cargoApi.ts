import { apiClient } from './client';
import { Cargo, CargoQuery, CreateCargoPayload, UpdateCargoPayload } from '../types/cargo';
import { PagedResult } from '../types/pagination';
import { AiRecommendationResult } from '../types/ai';

export const cargoApi = {
  getCargoItems: async (query: CargoQuery) => {
    const params = new URLSearchParams();
    if (query.pageNumber) params.append('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params.append('pageSize', query.pageSize.toString());
    if (query.sortBy) params.append('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params.append('searchTerm', query.searchTerm);
    if (query.status) params.append('status', query.status);
    if (query.type) params.append('type', query.type);
    if (query.voyageId) params.append('voyageId', query.voyageId);

    const response = await apiClient.get<PagedResult<Cargo>>(`/cargo?${params.toString()}`);
    return response.data;
  },

  getCargoById: async (id: string) => {
    const response = await apiClient.get<Cargo>(`/cargo/${id}`);
    return response.data;
  },

  createCargo: async (payload: CreateCargoPayload) => {
    const response = await apiClient.post<Cargo>('/cargo', payload);
    return response.data;
  },

  updateCargo: async (id: string, payload: UpdateCargoPayload) => {
    const response = await apiClient.put<Cargo>(`/cargo/${id}`, payload);
    return response.data;
  },

  deleteCargo: async (id: string): Promise<void> => {
    await apiClient.delete(`/cargo/${id}`);
  },

  getAiRiskAssessment: async (id: string): Promise<AiRecommendationResult> => {
    const response = await apiClient.get<AiRecommendationResult>(`/cargo/${id}/ai-risk-assessment`);
    return response.data;
  }
};
