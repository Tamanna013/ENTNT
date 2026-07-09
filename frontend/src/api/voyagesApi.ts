import { apiClient as axiosInstance } from './client';
import { Voyage, CreateVoyagePayload, UpdateVoyagePayload, UpdateVoyageStatusPayload, VoyageQuery } from '../types/voyage';
import { PagedResult } from '../types/pagination';
import { AiSummaryResult } from '../types/ai';

export const voyagesApi = {
  getVoyages: async (query: VoyageQuery): Promise<PagedResult<Voyage>> => {
    const response = await axiosInstance.get('/voyages', { params: query });
    return response.data;
  },

  getVoyageById: async (id: string): Promise<Voyage> => {
    const response = await axiosInstance.get(`/voyages/${id}`);
    return response.data;
  },

  createVoyage: async (payload: CreateVoyagePayload): Promise<Voyage> => {
    const response = await axiosInstance.post('/voyages', payload);
    return response.data;
  },

  updateVoyage: async (id: string, payload: UpdateVoyagePayload): Promise<Voyage> => {
    const response = await axiosInstance.put(`/voyages/${id}`, payload);
    return response.data;
  },

  deleteVoyage: async (id: string): Promise<void> => {
    await axiosInstance.delete(`/voyages/${id}`);
  },

  updateVoyageStatus: async (id: string, payload: UpdateVoyageStatusPayload): Promise<Voyage> => {
    const response = await axiosInstance.put(`/voyages/${id}/status`, payload);
    return response.data;
  },

  getAiSummary: async (id: string): Promise<AiSummaryResult> => {
    const response = await axiosInstance.get<AiSummaryResult>(`/voyages/${id}/ai-summary`);
    return response.data;
  },

  getOverdueVoyages: async (): Promise<string[]> => {
    const response = await axiosInstance.get<string[]>(`/voyages/overdue`);
    return response.data;
  }
};
