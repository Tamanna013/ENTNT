import React, { useState } from 'react';
import { Plus, Search } from 'lucide-react';
import { useAuthStore } from '../../store/authStore';
import { useFuelLogsQuery, useCreateFuelLogMutation, useUpdateFuelLogMutation, useDeleteFuelLogMutation } from '../../hooks/useFuel';
import { FuelTable } from '../../components/fuel/FuelTable';
import { FuelFormModal } from '../../components/fuel/FuelFormModal';
import { Pagination } from '../../components/ui/Pagination';
import { Button } from '../../components/ui/Button';
import { FuelLog, FuelLogQuery } from '../../types/fuel';
import { FUEL_TYPES } from '../../lib/constants';

export const FuelPage: React.FC = () => {
  const user = useAuthStore(state => state.user);
  const canWrite = !!(user?.roles?.includes('Admin') || user?.roles?.includes('FleetManager'));

  const [query, setQuery] = useState<FuelLogQuery>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'recordedDate',
    sortDescending: true,
  });

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [selectedLog, setSelectedLog] = useState<FuelLog | null>(null);

  const { data, isLoading } = useFuelLogsQuery(query);
  const createMutation = useCreateFuelLogMutation();
  const updateMutation = useUpdateFuelLogMutation();
  const deleteMutation = useDeleteFuelLogMutation();

  const handleCreate = () => {
    setSelectedLog(null);
    setIsFormOpen(true);
  };

  const handleEdit = (log: FuelLog) => {
    setSelectedLog(log);
    setIsFormOpen(true);
  };

  const handleSubmit = async (formData: any) => {
    try {
      if (selectedLog) {
        await updateMutation.mutateAsync({ id: selectedLog.id, payload: formData });
      } else {
        await createMutation.mutateAsync(formData);
      }
      setIsFormOpen(false);
    } catch (error) {
      console.error('Failed to save fuel log', error);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-900">Fuel Management</h1>
        {canWrite && (
          <Button onClick={handleCreate} className="flex items-center">
            <Plus className="h-4 w-4 mr-2" />
            Add Fuel Log
          </Button>
        )}
      </div>

      <div className="bg-white p-4 rounded-lg shadow space-y-4">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1">
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <Search className="h-5 w-5 text-text-muted" />
              </div>
              <input
                type="text"
                placeholder="Search by ship name or voyage number..."
                className="pl-10 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm py-2"
                value={query.searchTerm || ''}
                onChange={(e) => setQuery({ ...query, searchTerm: e.target.value, pageNumber: 1 })}
              />
            </div>
          </div>
          <div className="w-full sm:w-48">
            <select
              className="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm rounded-md shadow-sm"
              value={query.fuelType || ''}
              onChange={(e) => setQuery({ ...query, fuelType: e.target.value || undefined, pageNumber: 1 })}
            >
              <option value="">All Fuel Types</option>
              {FUEL_TYPES.map(type => (
                <option key={type} value={type}>{type}</option>
              ))}
            </select>
          </div>
        </div>

        {isLoading ? (
          <div className="text-center py-4">Loading...</div>
        ) : (
          <>
            <FuelTable
              fuelLogs={data?.items || []}
              canWrite={canWrite}
              onEdit={handleEdit}
              onDelete={(id) => deleteMutation.mutate(id)}
            />
            {data && (
              <Pagination
                pageNumber={data.pageNumber}
                totalPages={data.totalPages}
                onPageChange={(page) => setQuery({ ...query, pageNumber: page })}
              />
            )}
          </>
        )}
      </div>

      <FuelFormModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        onSubmit={handleSubmit}
        fuelLog={selectedLog}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
};
