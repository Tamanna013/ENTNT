import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { portsApi } from '../api/portsApi';
import { PortQuery, CreatePortPayload, UpdatePortPayload } from '../types/port';

export const usePortsQuery = (query: PortQuery) => {
  return useQuery({
    queryKey: ['ports', query],
    queryFn: () => portsApi.getPorts(query)
  });
};

export const usePortQuery = (id: string) => {
  return useQuery({
    queryKey: ['ports', id],
    queryFn: () => portsApi.getPortById(id),
    enabled: !!id
  });
};

export const useCreatePortMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreatePortPayload) => portsApi.createPort(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ports'] });
    }
  });
};

export const useUpdatePortMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdatePortPayload }) => 
      portsApi.updatePort(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ports'] });
    }
  });
};

export const useDeactivatePortMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => portsApi.deactivatePort(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ports'] });
    }
  });
};
