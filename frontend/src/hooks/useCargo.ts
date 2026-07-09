import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { cargoApi } from '../api/cargoApi';
import { CargoQuery, CreateCargoPayload, UpdateCargoPayload } from '../types/cargo';
import { useToast } from '../hooks/useToast';

export const useCargoQuery = (query: CargoQuery) => {
  return useQuery({
    queryKey: ['cargo', query],
    queryFn: () => cargoApi.getCargoItems(query)
  });
};

export const useCargoForVoyageQuery = (voyageId: string, query: Omit<CargoQuery, 'voyageId'>) => {
  return useQuery({
    queryKey: ['cargo', 'voyage', voyageId, query],
    queryFn: () => cargoApi.getCargoItems({ ...query, voyageId })
  });
};

export const useCargoItemQuery = (id: string) => {
  return useQuery({
    queryKey: ['cargo', id],
    queryFn: () => cargoApi.getCargoById(id),
    enabled: !!id
  });
};

export const useCargoAiRiskAssessmentQuery = (id: string) => {
  return useQuery({
    queryKey: ['cargo', id, 'ai-risk-assessment'],
    queryFn: () => cargoApi.getAiRiskAssessment(id),
    staleTime: 30000,
    refetchOnWindowFocus: true,
  });
};

export const useCreateCargoMutation = () => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: (payload: CreateCargoPayload) => cargoApi.createCargo(payload),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['cargo'] });
      if (data.warnings && data.warnings.length > 0) {
        data.warnings.forEach((warning: string) => {
          showToast(warning, 'warning');
        });
      }
    }
  });
};

export const useUpdateCargoMutation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateCargoPayload }) =>
      cargoApi.updateCargo(id, payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['cargo'] });
      queryClient.invalidateQueries({ queryKey: ['cargo', variables.id] });
    }
  });
};

export const useDeleteCargoMutation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => cargoApi.deleteCargo(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cargo'] });
    }
  });
};
