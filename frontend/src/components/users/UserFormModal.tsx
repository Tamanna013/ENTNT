import React, { useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Modal } from '../ui/Modal';
import { User, CreateUserPayload, UpdateUserPayload } from '../../types/user';
import { AppRoles } from '../../lib/constants';
import { useCreateUserMutation, useUpdateUserMutation } from '../../hooks/useUsers';

const userSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  email: z.string().email('Invalid email address'),
  phoneNumber: z.string().optional(),
  isActive: z.boolean(),
  password: z.string().optional(),
  roleNames: z.array(z.string()).min(1, 'At least one role is required'),
  isCreate: z.boolean().optional()
}).superRefine((data, ctx) => {
  // If no existing user, password is required
  if (data.isCreate && !data.password) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: 'Password is required for new users',
      path: ['password']
    });
  }
});

interface UserFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  user?: User; // If provided, edit mode. Else create mode.
}

export const UserFormModal: React.FC<UserFormModalProps> = ({ isOpen, onClose, user }) => {
  const isEditMode = !!user;
  
  const createMutation = useCreateUserMutation();
  const updateMutation = useUpdateUserMutation();

  const { register, handleSubmit, control, reset, formState: { errors } } = useForm({
    resolver: zodResolver(userSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      email: '',
      password: '',
      phoneNumber: '',
      isActive: true,
      roleNames: ['User']
    }
  });

  useEffect(() => {
    if (isOpen) {
      if (isEditMode && user) {
        reset({
          firstName: user.firstName,
          lastName: user.lastName,
          email: user.email,
          phoneNumber: user.phoneNumber || '',
          isActive: user.isActive,
          roleNames: user.roles,
          password: '', // Password is not editable here
          // @ts-ignore
          isCreate: false
        });
      } else {
        reset({
          firstName: '',
          lastName: '',
          email: '',
          password: '',
          phoneNumber: '',
          isActive: true,
          roleNames: ['User'],
          // @ts-ignore
          isCreate: true
        });
      }
    }
  }, [isOpen, isEditMode, user, reset]);

  const onSubmit = async (data: any) => {
    try {
      if (isEditMode && user) {
        const payload: UpdateUserPayload = {
          firstName: data.firstName,
          lastName: data.lastName,
          phoneNumber: data.phoneNumber || undefined,
          isActive: data.isActive,
          roleNames: data.roleNames
        };
        await updateMutation.mutateAsync({ id: user.id, payload });
      } else {
        const payload: CreateUserPayload = {
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          password: data.password,
          phoneNumber: data.phoneNumber || undefined,
          roleNames: data.roleNames
        };
        await createMutation.mutateAsync(payload);
      }
      onClose();
    } catch (error: any) {
      console.error('Failed to save user:', error);
      alert(error.response?.data?.message || 'Failed to save user');
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={isEditMode ? 'Edit User' : 'Create User'}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-text-primary">First Name</label>
            <input
              type="text"
              {...register('firstName')} aria-invalid={!!(errors as any)?.firstName} aria-describedby={(errors as any)?.firstName ? 'firstName-error' : undefined}
              className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
            />
            {errors.firstName && <p id="firstName-error" className="mt-1 text-sm text-red-400">{errors.firstName.message as string}</p>}
          </div>
          <div>
            <label className="block text-sm font-medium text-text-primary">Last Name</label>
            <input
              type="text"
              {...register('lastName')} aria-invalid={!!(errors as any)?.lastName} aria-describedby={(errors as any)?.lastName ? 'lastName-error' : undefined}
              className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
            />
            {errors.lastName && <p id="lastName-error" className="mt-1 text-sm text-red-400">{errors.lastName.message as string}</p>}
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-text-primary">Email</label>
          <input
            type="email"
            {...register('email')} aria-invalid={!!(errors as any)?.email} aria-describedby={(errors as any)?.email ? 'email-error' : undefined}
            disabled={isEditMode} // Cannot change email here
            className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2 disabled:opacity-50"
          />
          {errors.email && <p id="email-error" className="mt-1 text-sm text-red-400">{errors.email.message as string}</p>}
        </div>

        {!isEditMode && (
          <div>
            <label className="block text-sm font-medium text-text-primary">Temporary Password</label>
            <input
              type="password"
              {...register('password')} aria-invalid={!!(errors as any)?.password} aria-describedby={(errors as any)?.password ? 'password-error' : undefined}
              className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
            />
            {errors.password && <p id="password-error" className="mt-1 text-sm text-red-400">{errors.password.message as string}</p>}
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-text-primary">Phone Number (Optional)</label>
          <input
            type="text"
            {...register('phoneNumber')} aria-invalid={!!(errors as any)?.phoneNumber} aria-describedby={(errors as any)?.phoneNumber ? 'phoneNumber-error' : undefined}
            className="mt-1 block w-full rounded-md bg-surface border-border text-slate-100 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-text-primary mb-2">Roles</label>
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
          {errors.roleNames && <p id="roleNames-error" className="mt-1 text-sm text-red-400">{errors.roleNames.message as string}</p>}
        </div>

        {isEditMode && (
          <div>
            <label className="flex items-center gap-2 text-sm text-text-primary cursor-pointer mt-4">
              <input
                type="checkbox"
                {...register('isActive')} aria-invalid={!!(errors as any)?.isActive} aria-describedby={(errors as any)?.isActive ? 'isActive-error' : undefined}
                className="rounded border-border bg-surface text-blue-500 focus:ring-blue-500/20"
              />
              User is Active
            </label>
          </div>
        )}

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
            disabled={createMutation.isPending || updateMutation.isPending}
            className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50"
          >
            {createMutation.isPending || updateMutation.isPending ? 'Saving...' : 'Save User'}
          </button>
        </div>
      </form>
    </Modal>
  );
};
