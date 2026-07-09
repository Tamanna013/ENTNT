import { apiClient as client } from './client';
import { User } from '../types/user';
import { UserSettings, UpdateUserSettingsPayload, UpdateOwnProfilePayload } from '../types/userSettings';

export const getMySettings = async (): Promise<UserSettings> => {
  const response = await client.get<UserSettings>('/users/me/settings');
  return response.data;
};

export const updateMySettings = async (payload: UpdateUserSettingsPayload): Promise<UserSettings> => {
  const response = await client.put<UserSettings>('/users/me/settings', payload);
  return response.data;
};

export const updateMyProfile = async (payload: UpdateOwnProfilePayload): Promise<User> => {
  const response = await client.put<User>('/users/me/profile', payload);
  return response.data;
};
