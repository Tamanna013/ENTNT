import { apiClient as client } from './client';
import { PagedResult, PaginationQuery } from '../types/pagination';
import { User, CreateUserPayload, UpdateUserPayload, AssignRolesPayload } from '../types/user';

export const usersApi = {
  getUsers: async (query: PaginationQuery): Promise<PagedResult<User>> => {
    const params = new URLSearchParams();
    params.append('pageNumber', query.pageNumber.toString());
    params.append('pageSize', query.pageSize.toString());
    
    if (query.sortBy) params.append('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params.append('sortDescending', query.sortDescending.toString());
    if (query.searchTerm) params.append('searchTerm', query.searchTerm);

    const response = await client.get<PagedResult<User>>(`/users?${params.toString()}`);
    return response.data;
  },

  getUserById: async (id: string): Promise<User> => {
    const response = await client.get<User>(`/users/${id}`);
    return response.data;
  },

  createUser: async (payload: CreateUserPayload): Promise<User> => {
    const response = await client.post<User>('/users', payload);
    return response.data;
  },

  updateUser: async (id: string, payload: UpdateUserPayload): Promise<User> => {
    const response = await client.put<User>(`/users/${id}`, payload);
    return response.data;
  },

  deactivateUser: async (id: string): Promise<void> => {
    await client.delete(`/users/${id}`);
  },

  assignRoles: async (id: string, payload: AssignRolesPayload): Promise<User> => {
    const response = await client.put<User>(`/users/${id}/roles`, payload);
    return response.data;
  },

  getCurrentUser: async (): Promise<User> => {
    const response = await client.get<User>('/users/me');
    return response.data;
  }
};
