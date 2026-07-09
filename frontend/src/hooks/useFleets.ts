import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fleetsApi } from '../api/fleetsApi';
import { FleetQuery, CreateFleetPayload, UpdateFleetPayload } from '../types/fleet';

export const useFleetsQuery = (query: FleetQuery) => {
  return useQuery({
    queryKey: ['fleets', query],
    queryFn: () => fleetsApi.getFleets(query)
  });
};

export const useFleetQuery = (id: string) => {
  return useQuery({
    queryKey: ['fleets', id],
    queryFn: () => fleetsApi.getFleetById(id),
    enabled: !!id
  });
};

export const useCreateFleetMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateFleetPayload) => fleetsApi.createFleet(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fleets'] });
    }
  });
};

export const useUpdateFleetMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateFleetPayload }) => 
      fleetsApi.updateFleet(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fleets'] });
    }
  });
};

export const useDeactivateFleetMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => fleetsApi.deactivateFleet(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fleets'] });
    }
  });
};
