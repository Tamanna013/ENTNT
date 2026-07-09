import React from 'react';
import { User } from '../../types/user';
import { Mail, Shield, Activity } from 'lucide-react';

interface ReadOnlyAccountInfoProps {
  user: User;
}

export const ReadOnlyAccountInfo: React.FC<ReadOnlyAccountInfoProps> = ({ user }) => {
  return (
    <div className="bg-slate-50 border border-slate-200 rounded-xl p-6">
      <h3 className="text-lg font-semibold text-slate-800 mb-4">Account Information</h3>
      
      <div className="space-y-4">
        <div>
          <label className="flex items-center text-sm font-medium text-text-muted mb-1">
            <Mail size={16} className="mr-1.5" /> Email
          </label>
          <div className="text-slate-900 bg-slate-100 px-3 py-2 rounded-md font-mono text-sm inline-block">
            {user.email}
          </div>
        </div>

        <div>
          <label className="flex items-center text-sm font-medium text-text-muted mb-1">
            <Shield size={16} className="mr-1.5" /> Roles
          </label>
          <div className="flex flex-wrap gap-2">
            {user.roles && user.roles.length > 0 ? (
              user.roles.map(role => (
                <span key={role} className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800 border border-blue-200">
                  {role}
                </span>
              ))
            ) : (
              <span className="text-text-muted text-sm">No roles assigned</span>
            )}
          </div>
        </div>

        <div>
          <label className="flex items-center text-sm font-medium text-text-muted mb-1">
            <Activity size={16} className="mr-1.5" /> Status
          </label>
          <div>
            <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${ user.isActive ? 'bg-emerald-100 text-emerald-800 border-emerald-200' : 'bg-slate-100 text-slate-800 border-slate-200' }`}>
              {user.isActive ? 'Active' : 'Inactive'}
            </span>
          </div>
        </div>
      </div>

      <div className="mt-6 pt-4 border-t border-slate-200">
        <p className="text-xs text-text-muted flex items-start gap-1.5 leading-relaxed">
          <Shield size={14} className="flex-shrink-0 mt-0.5" />
          These details are managed by an administrator. Contact your administrator to request a change to your email, roles, or account status.
        </p>
      </div>
    </div>
  );
};
