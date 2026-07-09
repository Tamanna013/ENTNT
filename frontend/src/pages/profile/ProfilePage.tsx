import React from 'react';
import { useAuthStore } from '../../store/authStore';
import { ProfileForm } from '../../components/profile/ProfileForm';
import { ReadOnlyAccountInfo } from '../../components/profile/ReadOnlyAccountInfo';
import { ChangePasswordForm } from '../../components/profile/ChangePasswordForm';
import { UserCircle } from 'lucide-react';

export const ProfilePage: React.FC = () => {
  const user = useAuthStore(state => state.user);

  if (!user) return null;

  return (
    <div className="max-w-4xl mx-auto p-6 space-y-6">
      <div className="flex items-center gap-3 border-b border-slate-200 pb-4">
        <UserCircle size={32} className="text-indigo-600" />
        <h1 className="text-2xl font-bold text-slate-800">My Profile</h1>
      </div>
      
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">
        <div className="lg:col-span-2">
          <ProfileForm user={user} />
          <ChangePasswordForm />
        </div>
        <div className="lg:col-span-1">
          <ReadOnlyAccountInfo user={user} />
        </div>
      </div>
    </div>
  );
};
