import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { maintenanceApi } from '../api/maintenanceApi';
import { 
  MaintenanceQuery, 
  CreateMaintenanceRecordPayload, 
  UpdateMaintenanceRecordPayload,
  UpdateMaintenanceStatusPayload
} from '../types/maintenance';

export const useMaintenanceQuery = (query?: MaintenanceQuery) => {
  return useQuery({
    queryKey: ['maintenance', query],
    queryFn: () => maintenanceApi.getMaintenanceRecords(query),
    placeholderData: (prev: any) => prev,
  });
};

export const useMaintenanceRecordQuery = (id: string) => {
  return useQuery({
    queryKey: ['maintenance', id],
    queryFn: () => maintenanceApi.getMaintenanceRecordById(id),
    enabled: !!id,
  });
};

export const useCreateMaintenanceRecordMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateMaintenanceRecordPayload) => maintenanceApi.createMaintenanceRecord(payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['maintenance'] });
      queryClient.invalidateQueries({ queryKey: ['ships', variables.shipId, 'maintenance'] });
    },
  });
};

export const useUpdateMaintenanceRecordMutation = (id: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateMaintenanceRecordPayload) => maintenanceApi.updateMaintenanceRecord(id, payload),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['maintenance'] });
      queryClient.invalidateQueries({ queryKey: ['ships', data.shipId, 'maintenance'] });
    },
  });
};

export const useDeleteMaintenanceRecordMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => maintenanceApi.deleteMaintenanceRecord(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['maintenance'] });
      queryClient.invalidateQueries({ queryKey: ['ships'] });
    },
  });
};

export const useUpdateMaintenanceStatusMutation = (id: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateMaintenanceStatusPayload) => maintenanceApi.updateMaintenanceStatus(id, payload),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['maintenance'] });
      queryClient.invalidateQueries({ queryKey: ['ships', data.shipId, 'maintenance'] });
    },
  });
};

export const useMaintenanceForShipQuery = (shipId: string, query?: MaintenanceQuery) => {
  return useQuery({
    queryKey: ['ships', shipId, 'maintenance', query],
    queryFn: () => maintenanceApi.getMaintenanceForShip(shipId, query),
    enabled: !!shipId,
    placeholderData: (prev: any) => prev,
  });
};
