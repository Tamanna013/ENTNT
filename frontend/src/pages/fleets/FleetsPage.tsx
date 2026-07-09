import React, { useState } from 'react';
import { useFleetsQuery, useDeactivateFleetMutation } from '../../hooks/useFleets';
import { FleetsTable } from '../../components/fleets/FleetsTable';
import { FleetFormModal } from '../../components/fleets/FleetFormModal';
import { SearchInput } from '../../components/ui/SearchInput';
import { Pagination } from '../../components/ui/Pagination';
import { Fleet } from '../../types/fleet';
import { useAuthStore } from '../../store/authStore';
import { AppRoles, FLEET_STATUSES } from '../../lib/constants';
import { Plus } from 'lucide-react';
import { ExportButton } from '../../components/ui/ExportButton';

export const FleetsPage: React.FC = () => {
  const user = useAuthStore((state) => state.user);
  const canWrite = user?.roles?.some(r => r === AppRoles.Admin || r === AppRoles.FleetManager) || false;

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [sortBy, setSortBy] = useState<string>('createdAt');
  const [sortDescending, setSortDescending] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('');

  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [selectedFleet, setSelectedFleet] = useState<Fleet | undefined>(undefined);

  const { data, isLoading } = useFleetsQuery({
    pageNumber,
    pageSize,
    sortBy,
    sortDescending,
    searchTerm,
    status: statusFilter || undefined
  });

  const deactivateMutation = useDeactivateFleetMutation();

  const handleSortChange = (key: string) => {
    if (sortBy === key) {
      setSortDescending(!sortDescending);
    } else {
      setSortBy(key);
      setSortDescending(false);
    }
  };

  const openCreateModal = () => {
    setSelectedFleet(undefined);
    setIsFormModalOpen(true);
  };

  const openEditModal = (fleet: Fleet) => {
    setSelectedFleet(fleet);
    setIsFormModalOpen(true);
  };

  const handleDeactivate = async (fleetId: string) => {
    try {
      await deactivateMutation.mutateAsync(fleetId);
    } catch (err: any) {
      alert('Failed to deactivate fleet: ' + (err.response?.data?.title || err.message));
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Fleet Management</h1>
          <p className="text-sm text-text-muted mt-1">Manage platform fleets, home ports, and status.</p>
        </div>
        <div className="flex items-center gap-2">
          <ExportButton 
            exportPath="/fleets/export" 
            filters={{ searchTerm, status: statusFilter || undefined }} 
          />
          {canWrite && (
            <button
              onClick={openCreateModal}
              className="inline-flex items-center justify-center gap-2 rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:ring-offset-slate-900 transition-colors"
            >
              <Plus className="h-4 w-4" />
              Create Fleet
            </button>
          )}
        </div>
      </div>

      <div className="flex flex-col sm:flex-row items-center gap-4 p-4 bg-slate-900/50 rounded-lg border border-border">
        <SearchInput
          value={searchTerm}
          onChange={(val) => {
            setSearchTerm(val);
            setPageNumber(1);
          }}
          placeholder="Search fleets..."
        />
        <select
          value={statusFilter}
          onChange={(e) => {
            setStatusFilter(e.target.value);
            setPageNumber(1);
          }}
          className="rounded-lg border-0 bg-surface py-2 pl-3 pr-10 text-text-primary ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-blue-500 sm:text-sm sm:leading-6"
        >
          <option value="">All Statuses</option>
          {FLEET_STATUSES.map(status => (
            <option key={status} value={status}>{status}</option>
          ))}
        </select>
      </div>

      <FleetsTable
        fleets={data?.items || []}
        isLoading={isLoading}
        sortBy={sortBy}
        sortDescending={sortDescending}
        onSortChange={handleSortChange}
        onEdit={openEditModal}
        onDeactivate={handleDeactivate}
        canWrite={canWrite}
      />

      {data && (
        <Pagination
          pageNumber={pageNumber}
          totalPages={data.totalPages}
          onPageChange={setPageNumber}
        />
      )}

      {isFormModalOpen && (
        <FleetFormModal
          isOpen={isFormModalOpen}
          onClose={() => setIsFormModalOpen(false)}
          fleet={selectedFleet}
        />
      )}
    </div>
  );
};
