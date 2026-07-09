import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Modal } from '../ui/Modal';
import { User, AssignRolesPayload } from '../../types/user';
import { AppRoles } from '../../lib/constants';
import { useAssignRolesMutation } from '../../hooks/useUsers';

const assignRolesSchema = z.object({
  roleNames: z.array(z.string()).min(1, 'At least one role is required')
});

interface AssignRolesModalProps {
  isOpen: boolean;
  onClose: () => void;
  user?: User;
}

export const AssignRolesModal: React.FC<AssignRolesModalProps> = ({ isOpen, onClose, user }) => {
  const assignRolesMutation = useAssignRolesMutation();

  const { handleSubmit, control, reset, formState: { errors } } = useForm({
    resolver: zodResolver(assignRolesSchema),
    defaultValues: {
      roleNames: [] as string[]
    }
  });

  useEffect(() => {
    if (isOpen && user) {
      reset({ roleNames: user.roles });
    }
  }, [isOpen, user, reset]);

  const onSubmit = async (data: any) => {
    if (!user) return;
    try {
      const payload: AssignRolesPayload = {
        roleNames: data.roleNames
      };
      await assignRolesMutation.mutateAsync({ id: user.id, payload });
      onClose();
    } catch (error: any) {
      console.error('Failed to assign roles:', error);
      alert(error.response?.data?.message || 'Failed to assign roles');
    }
  };

  if (!user) return null;

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Assign Roles">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        
        <p className="text-sm text-text-muted mb-4">
          Manage roles for <span className="font-semibold text-text-primary">{user.email}</span>
        </p>

        <div>
          <div className="space-y-2">
            <Controller
              name="roleNames"
              control={control}
              render={({ field }) => (
                <>
                  {Object.values(AppRoles).map(role => (
                    <label key={role} className="flex items-center gap-2 text-sm text-text-primary cursor-pointer">
                      <input
                        type="checkbox"
                        value={role}
                        checked={field.value.includes(role)}
                        onChange={(e) => {
                          const updated = e.target.checked
                            ? [...field.value, role]
                            : field.value.filter(r => r !== role);
                          field.onChange(updated);
                        }}
                        className="rounded border-border bg-surface text-blue-500 focus:ring-blue-500/20"
                      />
                      {role}
                    </label>
                  ))}
                </>
              )}
            />
          </div>
          {errors.roleNames && <p className="mt-1 text-sm text-red-400">{errors.roleNames.message as string}</p>}
        </div>

        <div className="mt-6 flex justify-end gap-3">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-text-primary hover:text-slate-100 transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={assignRolesMutation.isPending}
            className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50"
          >
            {assignRolesMutation.isPending ? 'Saving...' : 'Save Roles'}
          </button>
        </div>
      </form>
    </Modal>
  );
};
