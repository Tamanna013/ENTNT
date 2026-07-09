import React, { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { MaintenanceTable } from '../../components/maintenance/MaintenanceTable';
import { MaintenanceFormModal } from '../../components/maintenance/MaintenanceFormModal';
import { ExportButton } from '../../components/ui/ExportButton';
import { useMaintenanceQuery, useCreateMaintenanceRecordMutation, useUpdateMaintenanceRecordMutation, useDeleteMaintenanceRecordMutation } from '../../hooks/useMaintenance';
import { MaintenanceQuery, MaintenanceRecord, CreateMaintenanceRecordPayload, UpdateMaintenanceRecordPayload } from '../../types/maintenance';
import { Button } from '../../components/ui/Button';
import { MAINTENANCE_STATUSES, MAINTENANCE_TYPES } from '../../lib/constants';
import { Pagination } from '../../components/ui/Pagination';
import { useAuthStore } from '../../store/authStore';

export const MaintenancePage: React.FC = () => {
  const user = useAuthStore(state => state.user);
  const canWrite = !!(user?.roles?.includes('Admin') || user?.roles?.includes('MaintenanceOfficer'));

  const [searchParams] = useSearchParams();
  const status = (searchParams.get('status') as MaintenanceQuery['status']) || undefined;

  const [query, setQuery] = useState<MaintenanceQuery>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'scheduledDate',
    sortDescending: false,
    status,
  });

  const { data, isLoading } = useMaintenanceQuery(query);
  const createMutation = useCreateMaintenanceRecordMutation();
  const updateMutation = useUpdateMaintenanceRecordMutation(query.shipId || ''); // shipId hack for hook signature, not used here, we instantiate per ID
  const deleteMutation = useDeleteMaintenanceRecordMutation();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState<MaintenanceRecord | null>(null);

  const handleOpenCreate = () => {
    setEditingRecord(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (record: MaintenanceRecord) => {
    setEditingRecord(record);
    setIsModalOpen(true);
  };

  const handleSubmit = async (payload: any) => {
    if (editingRecord) {
      await updateMutation.mutateAsync(payload as UpdateMaintenanceRecordPayload);
    } else {
      await createMutation.mutateAsync(payload as CreateMaintenanceRecordPayload);
    }
  };

  const handleDelete = async (record: MaintenanceRecord) => {
    if (window.confirm('Are you sure you want to delete this maintenance record?')) {
      await deleteMutation.mutateAsync(record.id);
    }
  };

  return (
    <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Maintenance Management</h1>
        </div>
        <div className="flex items-center gap-2">
          <ExportButton exportPath="/maintenance/export" filters={query as unknown as Record<string, unknown>} />
          {canWrite && (
            <Button onClick={handleOpenCreate}>
              Schedule Maintenance
            </Button>
          )}
        </div>
      </div>

      <div className="bg-white p-4 rounded-lg shadow mb-6 flex flex-wrap gap-4 items-center">
        <input
          type="text"
          placeholder="Search descriptions..."
          className="rounded-md border border-slate-300 p-2 text-sm w-64"
          value={query.searchTerm || ''}
          onChange={(e) => setQuery({ ...query, searchTerm: e.target.value, pageNumber: 1 })}
        />
        <select
          className="rounded-md border border-slate-300 p-2 text-sm"
          value={query.status || ''}
          onChange={(e) => setQuery({ ...query, status: e.target.value || undefined, pageNumber: 1 })}
        >
          <option value="">All Statuses</option>
          {MAINTENANCE_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
        </select>
        <select
          className="rounded-md border border-slate-300 p-2 text-sm"
          value={query.type || ''}
          onChange={(e) => setQuery({ ...query, type: e.target.value || undefined, pageNumber: 1 })}
        >
          <option value="">All Types</option>
          {MAINTENANCE_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
        </select>
        <div className="flex items-center gap-2">
          <label className="text-sm text-slate-600">Due Before:</label>
          <input
            type="date"
            className="rounded-md border border-slate-300 p-2 text-sm"
            value={query.dueBefore || ''}
            onChange={(e) => setQuery({ ...query, dueBefore: e.target.value || undefined, pageNumber: 1 })}
          />
        </div>
      </div>

      <div className="bg-white shadow rounded-lg overflow-hidden">
        <MaintenanceTable
          records={data?.items || []}
          isLoading={isLoading || false}
          canWrite={canWrite}
          onEdit={handleOpenEdit}
          onDelete={handleDelete}
        />
        {data && data.totalPages > 1 && (
          <div className="p-4 border-t border-slate-200">
            <Pagination
              pageNumber={data.pageNumber}
              totalPages={data.totalPages}
              onPageChange={(p) => setQuery({ ...query, pageNumber: p })}
            />
          </div>
        )}
      </div>

      <MaintenanceFormModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        
        maintenanceRecord={editingRecord}
        onSubmit={handleSubmit}
      />
    </div>
  );
};
