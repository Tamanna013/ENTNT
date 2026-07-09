import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fuelApi } from '../api/fuelApi';
import { FuelLogQuery, CreateFuelLogPayload, UpdateFuelLogPayload } from '../types/fuel';

export function useFuelLogsQuery(query?: FuelLogQuery) {
  return useQuery({
    queryKey: ['fuel', query],
    queryFn: () => fuelApi.getFuelLogs(query),
  });
}

export function useFuelLogQuery(id: string) {
  return useQuery({
    queryKey: ['fuel', id],
    queryFn: () => fuelApi.getFuelLogById(id),
    enabled: !!id,
  });
}

export function useCreateFuelLogMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateFuelLogPayload) => fuelApi.createFuelLog(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fuel'] });
    },
  });
}

export function useUpdateFuelLogMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateFuelLogPayload }) =>
      fuelApi.updateFuelLog(id, payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['fuel'] });
      queryClient.invalidateQueries({ queryKey: ['fuel', variables.id] });
    },
  });
}

export function useDeleteFuelLogMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => fuelApi.deleteFuelLog(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['fuel'] });
    },
  });
}

export function useFuelLogsForShipQuery(shipId: string, query?: FuelLogQuery) {
  return useQuery({
    queryKey: ['ships', shipId, 'fuel', query],
    queryFn: () => fuelApi.getFuelLogsForShip(shipId, query),
    enabled: !!shipId,
  });
}
