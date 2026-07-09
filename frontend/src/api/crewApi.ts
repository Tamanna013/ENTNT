import { apiClient as client } from './client';
import { CrewMember, CreateCrewMemberPayload, UpdateCrewMemberPayload, CrewQuery, CrewCertification } from '../types/crew';
import { PagedResult } from '../types/pagination';

export const crewApi = {
  getCrewMembers: async (query: CrewQuery): Promise<PagedResult<CrewMember>> => {
    const params = new URLSearchParams();
    if (query.pageNumber) params.append('pageNumber', query.pageNumber.toString());
    if (query.pageSize) params.append('pageSize', query.pageSize.toString());
    if (query.sortBy) params.append('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params.append('searchTerm', query.searchTerm);
    if (query.shipId) params.append('shipId', query.shipId);
    if (query.status) params.append('status', query.status);
    if (query.rank) params.append('rank', query.rank);
    if (query.unassigned !== undefined) params.append('unassigned', query.unassigned.toString());

    const { data } = await client.get(`/api/v1/crew?${params.toString()}`);
    return data;
  },

  getCrewMemberById: async (id: string): Promise<CrewMember> => {
    const { data } = await client.get(`/api/v1/crew/${id}`);
    return data;
  },

  createCrewMember: async (payload: CreateCrewMemberPayload): Promise<CrewMember> => {
    const { data } = await client.post('/api/v1/crew', payload);
    return data;
  },

  updateCrewMember: async (id: string, payload: UpdateCrewMemberPayload): Promise<CrewMember> => {
    const { data } = await client.put(`/api/v1/crew/${id}`, payload);
    return data;
  },

  deactivateCrewMember: async (id: string): Promise<void> => {
    await client.delete(`/api/v1/crew/${id}`);
  },

  assignToShip: async (id: string, shipId: string): Promise<CrewMember> => {
    const { data } = await client.put(`/api/v1/crew/${id}/assign`, { shipId });
    return data;
  },

  unassignFromShip: async (id: string): Promise<CrewMember> => {
    const { data } = await client.put(`/api/v1/crew/${id}/unassign`);
    return data;
  },

  getCertifications: async (crewMemberId: string): Promise<CrewCertification[]> => {
    const { data } = await client.get(`/api/v1/crew/${crewMemberId}/certifications`);
    return data;
  },

  uploadCertification: async (crewMemberId: string, file: File, certificationName: string, expiryDate: string): Promise<CrewCertification> => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('certificationName', certificationName);
    formData.append('expiryDate', expiryDate);

    const { data } = await client.post(`/api/v1/crew/${crewMemberId}/certifications`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return data;
  },

  deleteCertification: async (crewMemberId: string, certificationId: string): Promise<void> => {
    await client.delete(`/api/v1/crew/${crewMemberId}/certifications/${certificationId}`);
  },
};
