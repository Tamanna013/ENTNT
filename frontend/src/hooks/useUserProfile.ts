import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getMySettings, updateMySettings, updateMyProfile } from '../api/userProfileApi';
import { UpdateUserSettingsPayload, UpdateOwnProfilePayload } from '../types/userSettings';
import { useAuthStore } from '../store/authStore';

export const useMySettingsQuery = () => {
  return useQuery({
    queryKey: ['my-settings'],
    queryFn: getMySettings,
  });
};

export const useUpdateMySettingsMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateUserSettingsPayload) => updateMySettings(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-settings'] });
    },
  });
};

export const useUpdateMyProfileMutation = () => {
  const updateUserDisplayFields = useAuthStore(state => state.updateUserDisplayFields);
  
  return useMutation({
    mutationFn: (payload: UpdateOwnProfilePayload) => updateMyProfile(payload),
    onSuccess: (updatedUser, variables) => {
      // Instantly update topbar by patching the auth store in-place
      updateUserDisplayFields(variables.firstName, variables.lastName, variables.phoneNumber);
    },
  });
};
