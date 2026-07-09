import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useMaintenanceRecordQuery, useUpdateMaintenanceRecordMutation, useUpdateMaintenanceStatusMutation } from '../../hooks/useMaintenance';
import { MaintenanceFormModal } from '../../components/maintenance/MaintenanceFormModal';
import { MaintenanceStatusTransitionBar } from '../../components/maintenance/MaintenanceStatusTransitionBar';
import { Button } from '../../components/ui/Button';
import { Badge } from '../../components/ui/Badge';
import { useAuthStore } from '../../store/authStore';

export const MaintenanceDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const user = useAuthStore(state => state.user);
  const canWrite = user?.roles?.includes('Admin') || user?.roles?.includes('MaintenanceOfficer');

  const { data: record, isLoading } = useMaintenanceRecordQuery(id!);
  const updateMutation = useUpdateMaintenanceRecordMutation(id!);
  const statusMutation = useUpdateMaintenanceStatusMutation(id!);

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  if (isLoading) return <div className="p-8">Loading...</div>;
  if (!record) return <div className="p-8">Record not found.</div>;

  const getStatusBadgeColor = (status: string) => {
    switch (status) {
      case 'Scheduled': return 'blue';
      case 'InProgress': return 'yellow';
      case 'Completed': return 'green';
      case 'Overdue': return 'red';
      case 'Cancelled': return 'gray';
      default: return 'gray';
    }
  };

  return (
    <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <div className="mb-6 flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Button variant="secondary" onClick={() => navigate('/maintenance')}>&larr; Back</Button>
          <h1 className="text-2xl font-bold text-slate-900">Maintenance Details</h1>
          <Badge text={record.status} color={getStatusBadgeColor(record.status) as any} />
        </div>
        {canWrite && (record.status === 'Scheduled' || record.status === 'InProgress') && (
          <Button onClick={() => setIsEditModalOpen(true)}>Edit Record</Button>
        )}
      </div>

      {canWrite && (
        <MaintenanceStatusTransitionBar
          record={record}
          onUpdateStatus={async (payload) => { await statusMutation.mutateAsync(payload); }}
          isLoading={statusMutation.isPending}
        />
      )}

      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="px-4 py-5 sm:p-6">
          <dl className="grid grid-cols-1 gap-x-4 gap-y-8 grid-cols-1 sm:grid-cols-2">
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-text-muted">Ship</dt>
              <dd className="mt-1 text-sm text-slate-900 font-semibold">{record.shipName}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-text-muted">Type</dt>
              <dd className="mt-1 text-sm text-slate-900">{record.type}</dd>
            </div>
            <div className="sm:col-span-2">
              <dt className="text-sm font-medium text-text-muted">Description</dt>
              <dd className="mt-1 text-sm text-slate-900">{record.description}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-text-muted">Scheduled Date</dt>
              <dd className="mt-1 text-sm text-slate-900">{new Date(record.scheduledDate).toLocaleDateString()}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-text-muted">Completed Date</dt>
              <dd className="mt-1 text-sm text-slate-900">{record.completedDate ? new Date(record.completedDate).toLocaleDateString() : '-'}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-text-muted">Estimated Cost</dt>
              <dd className="mt-1 text-sm text-slate-900">${record.estimatedCost.toFixed(2)}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-text-muted">Actual Cost</dt>
              <dd className="mt-1 text-sm text-slate-900">{record.actualCost !== null ? `$${record.actualCost.toFixed(2)}` : '-'}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-text-muted">Performed By</dt>
              <dd className="mt-1 text-sm text-slate-900">{record.performedBy || '-'}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-text-muted">Created At</dt>
              <dd className="mt-1 text-sm text-slate-900">{new Date(record.createdAt).toLocaleDateString()}</dd>
            </div>
          </dl>
        </div>
      </div>

      <MaintenanceFormModal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        maintenanceRecord={record}
        onSubmit={async (payload) => { await updateMutation.mutateAsync(payload as any); }}
      />
    </div>
  );
};
