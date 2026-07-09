import React, { useState } from 'react';
import { useContainersQuery, useCreateContainerMutation, useUpdateContainerMutation } from '../../hooks/useContainers';
import { ContainerQuery, Container } from '../../types/container';
import { ContainersTable } from '../../components/containers/ContainersTable';
import { ContainerFormModal } from '../../components/containers/ContainerFormModal';
import { useAuthStore } from '../../store/authStore';
import { AppRoles, CONTAINER_STATUSES, CONTAINER_TYPES } from '../../lib/constants';
import { Button } from '../../components/ui/Button';
import { Pagination } from '../../components/ui/Pagination';
import { VoyageSelect } from '../../components/voyages/VoyageSelect';

export const ContainersPage: React.FC = () => {
  const { user } = useAuthStore();
  const canWrite = user?.roles?.includes(AppRoles.Admin) || user?.roles?.includes(AppRoles.FleetManager) || false;

  const [query, setQuery] = useState<ContainerQuery>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'createdAt',
    sortDescending: true,
  });
  
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingContainer, setEditingContainer] = useState<Container | undefined>();

  const { data } = useContainersQuery(query);
  const createMutation = useCreateContainerMutation();
  const updateMutation = useUpdateContainerMutation(editingContainer?.id || '');

  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    setQuery({ ...query, searchTerm: e.target.value, pageNumber: 1 });
  };

  const handleStatusFilter = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setQuery({ ...query, status: e.target.value || undefined, pageNumber: 1 });
  };

  const handleTypeFilter = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setQuery({ ...query, type: e.target.value || undefined, pageNumber: 1 });
  };
  
  const handleVoyageFilter = (voyageId: string) => {
    setQuery({ ...query, voyageId: voyageId || undefined, pageNumber: 1 });
  };

  const handlePageChange = (page: number) => {
    setQuery({ ...query, pageNumber: page });
  };

  const handleEdit = (container: Container) => {
    setEditingContainer(container);
    setIsCreateModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsCreateModalOpen(false);
    setTimeout(() => setEditingContainer(undefined), 200);
  };

  const handleFormSubmit = async (payload: any) => {
    if (editingContainer) {
      await updateMutation.mutateAsync(payload);
    } else {
      await createMutation.mutateAsync(payload);
    }
    handleCloseModal();
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Container Management</h1>
          <p className="text-text-muted text-sm mt-1">Manage shipping containers, assignments, and tracking.</p>
        </div>
        
        {canWrite && (
          <Button onClick={() => setIsCreateModalOpen(true)}>
            Add Container
          </Button>
        )}
      </div>

      <div className="bg-surface rounded-lg shadow-sm border border-border">
        <div className="p-4 border-b border-border flex flex-col sm:flex-row gap-4">
          <input
            type="text"
            placeholder="Search container number..."
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
            {CONTAINER_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
          </select>
          <select
            value={query.type || ''}
            onChange={handleTypeFilter}
            className="rounded-md border-0 py-1.5 text-slate-100 bg-background shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
          >
            <option value="">All Types</option>
            {CONTAINER_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
          </select>
          <div className="w-full sm:w-64">
            <VoyageSelect 
              value={query.voyageId || ''} 
              onChange={handleVoyageFilter} 
              label="" 
            />
          </div>
        </div>

        <ContainersTable 
          containers={data?.items || []} 
          canWrite={canWrite}
          onEdit={handleEdit}
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

      <ContainerFormModal
        isOpen={isCreateModalOpen}
        onClose={handleCloseModal}
        container={editingContainer}
        onSubmit={handleFormSubmit}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
};
