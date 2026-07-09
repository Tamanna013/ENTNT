import React, { useEffect } from 'react';
import { Modal } from '../ui/Modal';
import { Button } from '../ui/Button';
import { CrewMember, CreateCrewMemberPayload, UpdateCrewMemberPayload } from '../../types/crew';
import { useCreateCrewMemberMutation, useUpdateCrewMemberMutation } from '../../hooks/useCrew';
import { CREW_RANKS, CREW_STATUSES } from '../../lib/constants';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm as useReactHookForm } from 'react-hook-form';

const createSchema = z.object({
  firstName: z.string().min(1, 'First name is required').max(100),
  lastName: z.string().min(1, 'Last name is required').max(100),
  rank: z.string().min(1, 'Rank is required'),
  nationality: z.string().min(1, 'Nationality is required').max(100),
  dateOfBirth: z.string().min(1, 'Date of birth is required'),
  licenseNumber: z.string().min(1, 'License number is required').max(50),
  hireDate: z.string().min(1, 'Hire date is required'),
  contactEmail: z.string().email('Invalid email').optional().or(z.literal('')),
  contactPhone: z.string().optional(),
}).refine((data) => {
  if (!data.dateOfBirth || !data.hireDate) return true;
  const dob = new Date(data.dateOfBirth);
  const hire = new Date(data.hireDate);
  let age = hire.getFullYear() - dob.getFullYear();
  const m = hire.getMonth() - dob.getMonth();
  if (m < 0 || (m === 0 && hire.getDate() < dob.getDate())) {
    age--;
  }
  return age >= 16 && age <= 80;
}, {
  message: "Age at hire date must be between 16 and 80",
  path: ["dateOfBirth"],
});

const updateSchema = z.object({
  firstName: z.string().min(1, 'First name is required').max(100),
  lastName: z.string().min(1, 'Last name is required').max(100),
  rank: z.string().min(1, 'Rank is required'),
  status: z.string().min(1, 'Status is required'),
  nationality: z.string().min(1, 'Nationality is required').max(100),
  contactEmail: z.string().email('Invalid email').optional().or(z.literal('')),
  contactPhone: z.string().optional(),
});

interface CrewFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  crewMember?: CrewMember;
}

export const CrewFormModal: React.FC<CrewFormModalProps> = ({ isOpen, onClose, crewMember }) => {
  const isEdit = !!crewMember;
  const schema = isEdit ? updateSchema : createSchema;
  
  const { register, handleSubmit, reset, formState: { errors } } = useReactHookForm({
    resolver: zodResolver(schema),
    defaultValues: isEdit ? {
      firstName: crewMember.firstName,
      lastName: crewMember.lastName,
      rank: crewMember.rank,
      status: crewMember.status,
      nationality: crewMember.nationality,
      contactEmail: crewMember.contactEmail || '',
      contactPhone: crewMember.contactPhone || '',
    } : {
      firstName: '',
      lastName: '',
      rank: '',
      nationality: '',
      dateOfBirth: '',
      licenseNumber: '',
      hireDate: '',
      contactEmail: '',
      contactPhone: '',
    }
  });

  useEffect(() => {
    if (isOpen) {
      reset(isEdit ? {
        firstName: crewMember.firstName,
        lastName: crewMember.lastName,
        rank: crewMember.rank,
        status: crewMember.status,
        nationality: crewMember.nationality,
        contactEmail: crewMember.contactEmail || '',
        contactPhone: crewMember.contactPhone || '',
      } : {
        firstName: '',
        lastName: '',
        rank: '',
        nationality: '',
        dateOfBirth: '',
        licenseNumber: '',
        hireDate: '',
        contactEmail: '',
        contactPhone: '',
      });
    }
  }, [isOpen, crewMember, reset, isEdit]);

  const { mutateAsync: createCrew, isPending: isCreating } = useCreateCrewMemberMutation();
  const { mutateAsync: updateCrew, isPending: isUpdating } = useUpdateCrewMemberMutation();
  
  const isPending = isCreating || isUpdating;

  const onSubmit = async (data: any) => {
    try {
      if (isEdit) {
        await updateCrew({ id: crewMember.id, payload: data as UpdateCrewMemberPayload });
      } else {
        await createCrew(data as CreateCrewMemberPayload);
      }
      onClose();
    } catch (error) {
      console.error('Failed to save crew member', error);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={isEdit ? 'Edit Crew Member' : 'Add Crew Member'}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">First Name</label>
            <input
              {...register('firstName')} aria-invalid={!!(errors as any)?.firstName} aria-describedby={(errors as any)?.firstName ? 'firstName-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
            />
            {errors && (errors as any).firstName && <p className="text-red-500 text-sm mt-1">{(errors as any).firstName.message as string}</p>}
          </div>
          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">Last Name</label>
            <input
              {...register('lastName')} aria-invalid={!!(errors as any)?.lastName} aria-describedby={(errors as any)?.lastName ? 'lastName-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
            />
            {errors && (errors as any).lastName && <p className="text-red-500 text-sm mt-1">{(errors as any).lastName.message as string}</p>}
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">Rank</label>
            <select
              {...register('rank')} aria-invalid={!!(errors as any)?.rank} aria-describedby={(errors as any)?.rank ? 'rank-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
            >
              <option value="">Select Rank</option>
              {CREW_RANKS.map(r => <option key={r} value={r}>{r}</option>)}
            </select>
            {errors && (errors as any).rank && <p className="text-red-500 text-sm mt-1">{(errors as any).rank.message as string}</p>}
          </div>
          
          {isEdit ? (
            <div>
              <label className="block text-sm font-medium text-text-primary mb-1">Status</label>
              <select
                {...register('status')} aria-invalid={!!(errors as any)?.status} aria-describedby={(errors as any)?.status ? 'status-error' : undefined}
                className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
              >
                {CREW_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
              </select>
              {errors && (errors as any).status && <p className="text-red-500 text-sm mt-1">{(errors as any).status.message as string}</p>}
            </div>
          ) : (
            <div>
              <label className="block text-sm font-medium text-text-primary mb-1">Nationality</label>
              <input
                {...register('nationality')} aria-invalid={!!(errors as any)?.nationality} aria-describedby={(errors as any)?.nationality ? 'nationality-error' : undefined}
                className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
              />
              {errors && (errors as any).nationality && <p className="text-red-500 text-sm mt-1">{(errors as any).nationality.message as string}</p>}
            </div>
          )}
        </div>

        {isEdit && (
          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">Nationality</label>
            <input
              {...register('nationality')} aria-invalid={!!(errors as any)?.nationality} aria-describedby={(errors as any)?.nationality ? 'nationality-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
            />
            {errors && (errors as any).nationality && <p className="text-red-500 text-sm mt-1">{(errors as any).nationality.message as string}</p>}
          </div>
        )}

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">Date of Birth</label>
            {isEdit ? (
              <div className="px-3 py-2 bg-surface border border-border rounded-md text-text-muted">
                {crewMember?.dateOfBirth}
              </div>
            ) : (
              <input
                type="date"
                {...register('dateOfBirth')} aria-invalid={!!(errors as any)?.dateOfBirth} aria-describedby={(errors as any)?.dateOfBirth ? 'dateOfBirth-error' : undefined}
                className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
              />
            )}
            {errors && (errors as any).dateOfBirth && <p className="text-red-500 text-sm mt-1">{(errors as any).dateOfBirth.message as string}</p>}
          </div>
          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">Hire Date</label>
            {isEdit ? (
              <div className="px-3 py-2 bg-surface border border-border rounded-md text-text-muted">
                {crewMember?.hireDate}
              </div>
            ) : (
              <input
                type="date"
                {...register('hireDate')} aria-invalid={!!(errors as any)?.hireDate} aria-describedby={(errors as any)?.hireDate ? 'hireDate-error' : undefined}
                className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
              />
            )}
            {errors && (errors as any).hireDate && <p className="text-red-500 text-sm mt-1">{(errors as any).hireDate.message as string}</p>}
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-text-primary mb-1">License Number</label>
          {isEdit ? (
            <div className="px-3 py-2 bg-surface border border-border rounded-md text-text-muted">
              {crewMember?.licenseNumber}
            </div>
          ) : (
            <input
              {...register('licenseNumber')} aria-invalid={!!(errors as any)?.licenseNumber} aria-describedby={(errors as any)?.licenseNumber ? 'licenseNumber-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
            />
          )}
          {errors && (errors as any).licenseNumber && <p className="text-red-500 text-sm mt-1">{(errors as any).licenseNumber.message as string}</p>}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">Email</label>
            <input
              type="email"
              {...register('contactEmail')} aria-invalid={!!(errors as any)?.contactEmail} aria-describedby={(errors as any)?.contactEmail ? 'contactEmail-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
            />
            {errors && (errors as any).contactEmail && <p className="text-red-500 text-sm mt-1">{(errors as any).contactEmail.message as string}</p>}
          </div>
          <div>
            <label className="block text-sm font-medium text-text-primary mb-1">Phone</label>
            <input
              {...register('contactPhone')} aria-invalid={!!(errors as any)?.contactPhone} aria-describedby={(errors as any)?.contactPhone ? 'contactPhone-error' : undefined}
              className="w-full bg-background border border-border rounded-md px-3 py-2 text-text-primary"
            />
            {errors && (errors as any).contactPhone && <p className="text-red-500 text-sm mt-1">{(errors as any).contactPhone.message as string}</p>}
          </div>
        </div>

        <div className="flex justify-end gap-3 pt-4 border-t border-border">
          <Button type="button" variant="secondary" onClick={onClose} disabled={isPending}>
            Cancel
          </Button>
          <Button type="submit" disabled={isPending}>
            {isPending ? 'Saving...' : 'Save'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};
