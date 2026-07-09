import { useMutation } from '@tanstack/react-query';
import { authApi } from '../api/authApi';

export const useChangePasswordMutation = () => {
    return useMutation({
        mutationFn: (payload: { currentPassword: string; newPassword: string }) => authApi.changePassword(payload)
    });
};
