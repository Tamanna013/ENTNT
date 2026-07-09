import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { shipsApi } from '../api/shipsApi';
import { ShipQuery, CreateShipPayload, UpdateShipPayload } from '../types/ship';

export const useShipsQuery = (query: ShipQuery) => {
  return useQuery({
    queryKey: ['ships', query],
    queryFn: () => shipsApi.getShips(query),
  });
};

export const useShipsByFleetQuery = (fleetId: string, query: Omit<ShipQuery, "fleetId">) => {
  return useQuery({
    queryKey: ['ships', { ...query, fleetId }],
    queryFn: () => shipsApi.getShips({ ...query, fleetId }),
    enabled: !!fleetId,
  });
};

export const useShipQuery = (id: string) => {
  return useQuery({
    queryKey: ['ships', id],
    queryFn: () => shipsApi.getShipById(id),
    enabled: !!id,
  });
};

export const useShipAiMaintenanceRecommendationsQuery = (id: string) => {
  return useQuery({
    queryKey: ['ships', id, 'ai-maintenance-recommendations'],
    queryFn: () => shipsApi.getAiMaintenanceRecommendations(id),
    staleTime: 30000,
    refetchOnWindowFocus: true,
  });
};

export const useCreateShipMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateShipPayload) => shipsApi.createShip(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ships'] });
      // Invalidate fleets as well since creating a ship affects FleetDto.ShipCount
      queryClient.invalidateQueries({ queryKey: ['fleets'] });
    },
  });
};

export const useUpdateShipMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateShipPayload }) => shipsApi.updateShip(id, payload),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['ships'] });
      queryClient.invalidateQueries({ queryKey: ['ships', id] });
    },
  });
};

export const useDeactivateShipMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => shipsApi.deactivateShip(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['ships'] });
      queryClient.invalidateQueries({ queryKey: ['ships', id] });
      queryClient.invalidateQueries({ queryKey: ['fleets'] });
    },
  });
};

export const useShipAttachmentsQuery = (shipId: string) => {
  return useQuery({
    queryKey: ['ships', shipId, 'attachments'],
    queryFn: () => shipsApi.getShipAttachments(shipId),
    enabled: !!shipId,
  });
};

export const useUploadShipAttachmentMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ shipId, file }: { shipId: string; file: File }) => shipsApi.uploadShipAttachment(shipId, file),
    onSuccess: (_, { shipId }) => {
      queryClient.invalidateQueries({ queryKey: ['ships', shipId, 'attachments'] });
    },
  });
};

export const useSetPrimaryPhotoMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ shipId, attachmentId }: { shipId: string; attachmentId: string }) => shipsApi.setPrimaryPhoto(shipId, attachmentId),
    onSuccess: (_, { shipId }) => {
      queryClient.invalidateQueries({ queryKey: ['ships'] });
      queryClient.invalidateQueries({ queryKey: ['ships', shipId] });
    },
  });
};
