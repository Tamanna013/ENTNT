import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { containersApi } from '../api/containersApi';
import { ContainerQuery, CreateContainerPayload, UpdateContainerPayload, RecordTrackingEventPayload } from '../types/container';
import { useToast } from './useToast';

export const useContainersQuery = (query: ContainerQuery) => {
  return useQuery({
    queryKey: ['containers', query],
    queryFn: () => containersApi.getContainers(query),
  });
};

export const useContainersForVoyageQuery = (voyageId: string, query: Omit<ContainerQuery, 'voyageId'>) => {
  return useQuery({
    queryKey: ['containers', { ...query, voyageId }],
    queryFn: () => containersApi.getContainers({ ...query, voyageId }),
    enabled: !!voyageId,
  });
};

export const useContainerQuery = (id: string) => {
  return useQuery({
    queryKey: ['containers', id],
    queryFn: () => containersApi.getContainerById(id),
    enabled: !!id,
  });
};

export const useCreateContainerMutation = () => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  return useMutation({
    mutationFn: (payload: CreateContainerPayload) => containersApi.createContainer(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['containers'] });
      showToast('Container created successfully', 'success');
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to create container', 'error');
    }
  });
};

export const useUpdateContainerMutation = (id: string) => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  return useMutation({
    mutationFn: (payload: UpdateContainerPayload) => containersApi.updateContainer(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['containers'] });
      queryClient.invalidateQueries({ queryKey: ['containers', id] });
      showToast('Container updated successfully', 'success');
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to update container', 'error');
    }
  });
};

export const useDeleteContainerMutation = () => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  return useMutation({
    mutationFn: (id: string) => containersApi.deleteContainer(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['containers'] });
      showToast('Container deleted successfully', 'success');
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to delete container', 'error');
    }
  });
};

export const useLinkCargoMutation = (containerId: string) => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  return useMutation({
    mutationFn: (cargoId: string) => containersApi.linkCargo(containerId, cargoId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['containers'] });
      queryClient.invalidateQueries({ queryKey: ['containers', containerId] });
      showToast('Cargo linked successfully', 'success');
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to link cargo', 'error');
    }
  });
};

export const useUnlinkCargoMutation = (containerId: string) => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  return useMutation({
    mutationFn: (cargoId: string) => containersApi.unlinkCargo(containerId, cargoId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['containers'] });
      queryClient.invalidateQueries({ queryKey: ['containers', containerId] });
      showToast('Cargo unlinked successfully', 'success');
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to unlink cargo', 'error');
    }
  });
};

export const useRecordTrackingEventMutation = (containerId: string) => {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  return useMutation({
    mutationFn: (payload: RecordTrackingEventPayload) => containersApi.recordTrackingEvent(containerId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['containers', containerId, 'tracking-events'] });
      showToast('Tracking event recorded successfully', 'success');
    },
    onError: (error: any) => {
      showToast(error.response?.data?.message || 'Failed to record tracking event', 'error');
    }
  });
};

export const useTrackingEventsQuery = (containerId: string) => {
  return useQuery({
    queryKey: ['containers', containerId, 'tracking-events'],
    queryFn: () => containersApi.getTrackingEvents(containerId),
    enabled: !!containerId,
  });
};
