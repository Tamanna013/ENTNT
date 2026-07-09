import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { voyagesApi } from '../api/voyagesApi';
import { VoyageQuery, CreateVoyagePayload, UpdateVoyagePayload, UpdateVoyageStatusPayload } from '../types/voyage';
import { useToast } from './useToast';

export const useVoyagesQuery = (query: VoyageQuery) => {
  return useQuery({
    queryKey: ['voyages', query],
    queryFn: () => voyagesApi.getVoyages(query),
  });
};

export const useVoyagesForShipQuery = (shipId: string, query: VoyageQuery) => {
  return useQuery({
    queryKey: ['voyages', 'ship', shipId, query],
    queryFn: () => voyagesApi.getVoyages({ ...query, shipId }),
    enabled: !!shipId,
  });
};

export const useVoyageQuery = (id: string) => {
  return useQuery({
    queryKey: ['voyages', id],
    queryFn: () => voyagesApi.getVoyageById(id),
    enabled: !!id,
  });
};

export const useVoyageAiSummaryQuery = (id: string) => {
  return useQuery({
    queryKey: ['voyages', id, 'ai-summary'],
    queryFn: () => voyagesApi.getAiSummary(id),
    staleTime: 30000,
    refetchOnWindowFocus: true,
  });
};

export const useCreateVoyageMutation = () => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  return useMutation({
    mutationFn: (payload: CreateVoyagePayload) => voyagesApi.createVoyage(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['voyages'] });
      showToast('Voyage scheduled successfully', 'success');
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to schedule voyage', 'error');
    }
  });
};

export const useUpdateVoyageMutation = (id: string) => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  return useMutation({
    mutationFn: (payload: UpdateVoyagePayload) => voyagesApi.updateVoyage(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['voyages'] });
      showToast('Voyage updated successfully', 'success');
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to update voyage', 'error');
    }
  });
};

export const useDeleteVoyageMutation = () => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  return useMutation({
    mutationFn: (id: string) => voyagesApi.deleteVoyage(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['voyages'] });
      showToast('Voyage deleted successfully', 'success');
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to delete voyage', 'error');
    }
  });
};

export const useUpdateVoyageStatusMutation = (id: string) => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  return useMutation({
    mutationFn: (payload: UpdateVoyageStatusPayload) => voyagesApi.updateVoyageStatus(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['voyages'] });
      showToast('Voyage status updated', 'success');
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to update voyage status', 'error');
    }
  });
};
