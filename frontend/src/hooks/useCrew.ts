import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { crewApi } from '../api/crewApi';
import { CrewQuery, UpdateCrewMemberPayload } from '../types/crew';

export const useCrewQuery = (query: CrewQuery) => {
  return useQuery({
    queryKey: ['crew', query],
    queryFn: () => crewApi.getCrewMembers(query),
  });
};

export const useCrewMemberQuery = (id: string) => {
  return useQuery({
    queryKey: ['crew', id],
    queryFn: () => crewApi.getCrewMemberById(id),
    enabled: !!id,
  });
};

export const useCreateCrewMemberMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: crewApi.createCrewMember,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['crew'] });
    },
  });
};

export const useUpdateCrewMemberMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateCrewMemberPayload }) => crewApi.updateCrewMember(id, payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['crew'] });
      queryClient.invalidateQueries({ queryKey: ['crew', variables.id] });
    },
  });
};

export const useDeactivateCrewMemberMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: crewApi.deactivateCrewMember,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['crew'] });
    },
  });
};

export const useAssignToShipMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, shipId }: { id: string; shipId: string }) => crewApi.assignToShip(id, shipId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['crew'] });
      queryClient.invalidateQueries({ queryKey: ['crew', variables.id] });
    },
  });
};

export const useUnassignFromShipMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: crewApi.unassignFromShip,
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['crew'] });
      queryClient.invalidateQueries({ queryKey: ['crew', id] });
    },
  });
};

export const useCertificationsQuery = (crewMemberId: string) => {
  return useQuery({
    queryKey: ['crew', crewMemberId, 'certifications'],
    queryFn: () => crewApi.getCertifications(crewMemberId),
    enabled: !!crewMemberId,
  });
};

export const useUploadCertificationMutation = (crewMemberId: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ file, certificationName, expiryDate }: { file: File; certificationName: string; expiryDate: string }) => 
      crewApi.uploadCertification(crewMemberId, file, certificationName, expiryDate),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['crew', crewMemberId, 'certifications'] });
    },
  });
};

export const useDeleteCertificationMutation = (crewMemberId: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (certificationId: string) => crewApi.deleteCertification(crewMemberId, certificationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['crew', crewMemberId, 'certifications'] });
    },
  });
};

export const useCrewForShipQuery = (shipId: string, query: Omit<CrewQuery, "shipId">) => {
  return useQuery({
    queryKey: ['crew', { ...query, shipId }],
    queryFn: () => crewApi.getCrewMembers({ ...query, shipId }),
    enabled: !!shipId,
  });
};
