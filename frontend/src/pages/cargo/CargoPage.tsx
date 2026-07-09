import React, { useState } from 'react';
import { useCargoQuery } from '../../hooks/useCargo';
import { CargoQuery } from '../../types/cargo';
import { CargoTable } from '../../components/cargo/CargoTable';
import { CargoFormModal } from '../../components/cargo/CargoFormModal';
import { useAuthStore } from '../../store/authStore';
import { AppRoles, CARGO_STATUSES, CARGO_TYPES } from '../../lib/constants';
import { Button } from '../../components/ui/Button';
import { ExportButton } from '../../components/ui/ExportButton';
import { Pagination } from '../../components/ui/Pagination';

export const CargoPage: React.FC = () => {
  const { user } = useAuthStore();
  const canWrite = user?.roles?.includes(AppRoles.Admin) || user?.roles?.includes(AppRoles.FleetManager) || false;

  const [query, setQuery] = useState<CargoQuery>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'createdAt',
    sortDescending: true,
  });
  
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  const { data, isLoading } = useCargoQuery(query);

  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    setQuery({ ...query, searchTerm: e.target.value, pageNumber: 1 });
  };

  const handleStatusFilter = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setQuery({ ...query, status: e.target.value || undefined, pageNumber: 1 });
  };

  const handleTypeFilter = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setQuery({ ...query, type: e.target.value || undefined, pageNumber: 1 });
  };

  const handlePageChange = (page: number) => {
    setQuery({ ...query, pageNumber: page });
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Cargo Management</h1>
          <p className="text-text-muted text-sm mt-1">Manage cargo assignments across all voyages.</p>
        </div>
        
        <div className="flex items-center gap-2">
          <ExportButton exportPath="/cargo/export" filters={query as unknown as Record<string, unknown>} />
          {canWrite && (
            <Button onClick={() => setIsCreateModalOpen(true)}>
              Add Cargo
            </Button>
          )}
        </div>
      </div>

      <div className="bg-surface rounded-lg shadow-sm border border-border">
        <div className="p-4 border-b border-border flex flex-col sm:flex-row gap-4">
          <input
            type="text"
            placeholder="Search description..."
            value={query.searchTerm || ''}
            onChange={handleSearch}
            className="flex-1 rounded-md border-0 py-1.5 text-slate-100 bg-background shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
          />
          <select
            value={query.status || ''}
            onChange={handleStatusFilter}
            className="rounded-md border-0 py-1.5 text-slate-100 bg-background shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
          >
            <option value="">All Statuses</option>
            {CARGO_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
          </select>
          <select
            value={query.type || ''}
            onChange={handleTypeFilter}
            className="rounded-md border-0 py-1.5 text-slate-100 bg-background shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
          >
            <option value="">All Types</option>
            {CARGO_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
          </select>
        </div>

        <CargoTable 
          data={data?.items || []} 
          isLoading={isLoading} 
          canWrite={canWrite}
        />

        {data && (
          <div className="p-4 border-t border-border">
            <Pagination
              pageNumber={data.pageNumber}
              totalPages={Math.ceil(data.totalCount / data.pageSize)}
              onPageChange={handlePageChange}
            />
          </div>
        )}
      </div>

      {isCreateModalOpen && (
        <CargoFormModal
          isOpen={isCreateModalOpen}
          onClose={() => setIsCreateModalOpen(false)}
        />
      )}
    </div>
  );
};
