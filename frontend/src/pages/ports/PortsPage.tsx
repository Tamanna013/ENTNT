import React, { useState } from 'react';
import { usePortsQuery, useDeactivatePortMutation } from '../../hooks/usePorts';
import { PortsTable } from '../../components/ports/PortsTable';
import { PortFormModal } from '../../components/ports/PortFormModal';
import { SearchInput } from '../../components/ui/SearchInput';
import { Pagination } from '../../components/ui/Pagination';
import { Port } from '../../types/port';
import { useAuthStore } from '../../store/authStore';
import { AppRoles } from '../../lib/constants';
import { Plus } from 'lucide-react';

export const PortsPage: React.FC = () => {
  const user = useAuthStore((state) => state.user);
  const canWrite = user?.roles?.some(r => r === AppRoles.Admin || r === AppRoles.FleetManager) || false;

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [sortBy, setSortBy] = useState<string>('createdAt');
  const [sortDescending, setSortDescending] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [countryFilter, setCountryFilter] = useState<string>('');

  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [selectedPort, setSelectedPort] = useState<Port | undefined>(undefined);

  const { data, isLoading } = usePortsQuery({
    pageNumber,
    pageSize,
    sortBy,
    sortDescending,
    searchTerm,
    country: countryFilter || undefined
  });

  const deactivateMutation = useDeactivatePortMutation();

  const handleSortChange = (key: string) => {
    if (sortBy === key) {
      setSortDescending(!sortDescending);
    } else {
      setSortBy(key);
      setSortDescending(false);
    }
  };

  const openCreateModal = () => {
    setSelectedPort(undefined);
    setIsFormModalOpen(true);
  };

  const openEditModal = (port: Port) => {
    setSelectedPort(port);
    setIsFormModalOpen(true);
  };

  const handleDeactivate = async (portId: string) => {
    try {
      await deactivateMutation.mutateAsync(portId);
    } catch (err: any) {
      alert('Failed to deactivate port: ' + (err.response?.data?.title || err.message));
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Port Management</h1>
          <p className="text-sm text-text-muted mt-1">Manage global maritime ports master data.</p>
        </div>
        {canWrite && (
          <button
            onClick={openCreateModal}
            className="inline-flex items-center justify-center gap-2 rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:ring-offset-slate-900 transition-colors"
          >
            <Plus className="h-4 w-4" />
            Create Port
          </button>
        )}
      </div>

      <div className="flex flex-col sm:flex-row items-center gap-4 p-4 bg-slate-900/50 rounded-lg border border-border">
        <SearchInput
          value={searchTerm}
          onChange={(val) => {
            setSearchTerm(val);
            setPageNumber(1);
          }}
          placeholder="Search ports by name or UN/LOCODE..."
        />
        <input
          type="text"
          value={countryFilter}
          onChange={(e) => {
            setCountryFilter(e.target.value);
            setPageNumber(1);
          }}
          placeholder="Filter by country..."
          className="rounded-lg border-0 bg-surface py-2 px-3 text-text-primary ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-blue-500 sm:text-sm sm:leading-6 min-w-[200px]"
        />
      </div>

      <PortsTable
        ports={data?.items || []}
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
        <PortFormModal
          isOpen={isFormModalOpen}
          onClose={() => setIsFormModalOpen(false)}
          port={selectedPort}
        />
      )}
    </div>
  );
};
