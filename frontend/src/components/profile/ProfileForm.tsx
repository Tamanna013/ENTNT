import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { User } from '../../types/user';
import { useUpdateMyProfileMutation } from '../../hooks/useUserProfile';
import { CheckCircle2, User as UserIcon } from 'lucide-react';

const profileSchema = z.object({
  firstName: z.string().min(1, 'First name is required').max(100, 'Must not exceed 100 characters'),
  lastName: z.string().min(1, 'Last name is required').max(100, 'Must not exceed 100 characters'),
  phoneNumber: z.string().optional().refine(val => !val || /^\+?[\d\s\-\(\)]{7,20}$/.test(val), {
    message: 'Phone number format is invalid'
  })
});

type ProfileFormData = z.infer<typeof profileSchema>;

interface ProfileFormProps {
  user: User;
}

export const ProfileForm: React.FC<ProfileFormProps> = ({ user }) => {
  const { register, handleSubmit, formState: { errors, isDirty } } = useForm<ProfileFormData>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      firstName: user.firstName,
      lastName: user.lastName,
      phoneNumber: user.phoneNumber || ''
    }
  });

  const updateProfile = useUpdateMyProfileMutation();
  const [showSuccess, setShowSuccess] = useState(false);

  const onSubmit = async (data: ProfileFormData) => {
    try {
      await updateProfile.mutateAsync(data);
      setShowSuccess(true);
      setTimeout(() => setShowSuccess(false), 3000);
    } catch (error) {
      console.error('Failed to update profile', error);
    }
  };

  return (
    <div className="bg-white border border-slate-200 rounded-xl p-6 shadow-sm">
      <h3 className="text-lg font-semibold text-slate-800 mb-6 flex items-center gap-2">
        <UserIcon size={20} className="text-indigo-600" />
        Basic Profile
      </h3>
      
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">First Name</label>
            <input 
              {...register('firstName')} 
              className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500" 
            />
            {errors.firstName && <p className="text-red-500 text-xs mt-1">{errors.firstName.message}</p>}
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Last Name</label>
            <input 
              {...register('lastName')} 
              className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500" 
            />
            {errors.lastName && <p className="text-red-500 text-xs mt-1">{errors.lastName.message}</p>}
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Phone Number (Optional)</label>
          <input 
            {...register('phoneNumber')} 
            className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500 max-w-md" 
          />
          {errors.phoneNumber && <p className="text-red-500 text-xs mt-1">{errors.phoneNumber.message}</p>}
        </div>

        <div className="pt-4 flex items-center justify-between border-t border-slate-100 mt-6">
          <div className="h-6">
            {showSuccess && (
              <span className="flex items-center text-sm text-emerald-600 bg-emerald-50 px-2 py-1 rounded">
                <CheckCircle2 size={16} className="mr-1.5" />
                Profile updated successfully
              </span>
            )}
            {updateProfile.isError && (
              <span className="text-sm text-red-600">Failed to update profile</span>
            )}
          </div>
          
          <button
            type="submit"
            disabled={!isDirty || updateProfile.isPending}
            className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            {updateProfile.isPending ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  );
};
