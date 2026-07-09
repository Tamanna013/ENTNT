import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useChangePasswordMutation } from '../../hooks/useAuth';
import { CheckCircle2, Lock } from 'lucide-react';
import { AxiosError } from 'axios';

const passwordSchema = z.object({
  currentPassword: z.string().min(1, 'Current password is required'),
  newPassword: z.string()
    .min(8, 'New password must be at least 8 characters long')
    .regex(/[A-Z]/, 'New password must contain at least one uppercase letter')
    .regex(/\d/, 'New password must contain at least one digit'),
  confirmNewPassword: z.string().min(1, 'Please confirm your new password')
}).refine(data => data.newPassword === data.confirmNewPassword, {
  message: "Passwords don't match",
  path: ['confirmNewPassword']
});

type PasswordFormData = z.infer<typeof passwordSchema>;

export const ChangePasswordForm: React.FC = () => {
  const { register, handleSubmit, reset, formState: { errors, isDirty } } = useForm<PasswordFormData>({
    resolver: zodResolver(passwordSchema)
  });

  const changePassword = useChangePasswordMutation();
  const [showSuccess, setShowSuccess] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const onSubmit = async (data: PasswordFormData) => {
    setErrorMessage(null);
    try {
      await changePassword.mutateAsync({
        currentPassword: data.currentPassword,
        newPassword: data.newPassword
      });
      setShowSuccess(true);
      reset();
      setTimeout(() => setShowSuccess(false), 3000);
    } catch (error) {
      if (error instanceof AxiosError && error.response?.status === 400) {
        // Backend specific error message parsing if present, otherwise default
        const serverMsg = error.response.data?.message || error.response.data?.title;
        setErrorMessage(serverMsg === "Current password is incorrect." ? "Current password is incorrect." : (serverMsg || 'Failed to change password.'));
      } else if (error instanceof AxiosError && error.response?.status === 401) {
        setErrorMessage("Current password is incorrect.");
      } else {
        setErrorMessage('An unexpected error occurred.');
      }
    }
  };

  return (
    <div className="bg-white border border-slate-200 rounded-xl p-6 shadow-sm mt-6">
      <h3 className="text-lg font-semibold text-slate-800 mb-6 flex items-center gap-2">
        <Lock size={20} className="text-indigo-600" />
        Change Password
      </h3>
      
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Current Password</label>
          <input 
            type="password"
            {...register('currentPassword')} 
            className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 max-w-md" 
          />
          {errors.currentPassword && <p className="text-red-500 text-xs mt-1">{errors.currentPassword.message}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">New Password</label>
          <input 
            type="password"
            {...register('newPassword')} 
            className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 max-w-md" 
          />
          {errors.newPassword && <p className="text-red-500 text-xs mt-1">{errors.newPassword.message}</p>}
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Confirm New Password</label>
          <input 
            type="password"
            {...register('confirmNewPassword')} 
            className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 max-w-md" 
          />
          {errors.confirmNewPassword && <p className="text-red-500 text-xs mt-1">{errors.confirmNewPassword.message}</p>}
        </div>

        <div className="pt-4 flex items-center justify-between border-t border-slate-100 mt-6">
          <div className="h-6">
            {showSuccess && (
              <span className="flex items-center text-sm text-emerald-600 bg-emerald-50 px-2 py-1 rounded">
                <CheckCircle2 size={16} className="mr-1.5" />
                Password changed successfully
              </span>
            )}
            {errorMessage && (
              <span className="text-sm text-red-600">{errorMessage}</span>
            )}
          </div>
          
          <button
            type="submit"
            disabled={!isDirty || changePassword.isPending}
            className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {changePassword.isPending ? 'Changing...' : 'Change Password'}
          </button>
        </div>
      </form>
    </div>
  );
};
