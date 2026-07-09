import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { incidentsApi } from '../api/incidentsApi';
import {
  CreateIncidentPayload,
  UpdateIncidentPayload,
  UpdateIncidentStatusPayload,
  IncidentQuery,
} from '../types/incident';

export const useIncidentsQuery = (query: IncidentQuery) => {
  return useQuery({
    queryKey: ['incidents', query],
    queryFn: () => incidentsApi.getIncidents(query),
  });
};

export const useIncidentQuery = (id: string) => {
  return useQuery({
    queryKey: ['incident', id],
    queryFn: () => incidentsApi.getIncidentById(id),
    enabled: !!id,
  });
};

export const useIncidentAiReportQuery = (id: string) => {
  return useQuery({
    queryKey: ['incidents', id, 'ai-report'],
    queryFn: () => incidentsApi.getAiReport(id),
    staleTime: 30000,
    refetchOnWindowFocus: true,
  });
};

export const useIncidentsForShipQuery = (shipId: string, query: IncidentQuery) => {
  return useQuery({
    queryKey: ['incidents-for-ship', shipId, query],
    queryFn: () => incidentsApi.getIncidentsForShip(shipId, query),
    enabled: !!shipId,
  });
};

export const useCreateIncidentMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateIncidentPayload) => incidentsApi.createIncident(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] });
      queryClient.invalidateQueries({ queryKey: ['incidents-for-ship'] });
    },
  });
};

export const useUpdateIncidentMutation = (id: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateIncidentPayload) => incidentsApi.updateIncident(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] });
      queryClient.invalidateQueries({ queryKey: ['incident', id] });
      queryClient.invalidateQueries({ queryKey: ['incidents-for-ship'] });
    },
  });
};

export const useUpdateIncidentStatusMutation = (id: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateIncidentStatusPayload) => incidentsApi.updateIncidentStatus(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] });
      queryClient.invalidateQueries({ queryKey: ['incident', id] });
      queryClient.invalidateQueries({ queryKey: ['incidents-for-ship'] });
    },
  });
};

export const useDeleteIncidentMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => incidentsApi.deleteIncident(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['incidents'] });
      queryClient.invalidateQueries({ queryKey: ['incidents-for-ship'] });
    },
  });
};
