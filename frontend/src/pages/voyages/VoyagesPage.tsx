import React, { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useVoyagesQuery, useCreateVoyageMutation, useUpdateVoyageMutation, useDeleteVoyageMutation } from '../../hooks/useVoyages';
import { VoyagesTable } from '../../components/voyages/VoyagesTable';
import { VoyageFormModal } from '../../components/voyages/VoyageFormModal';
import { Voyage, CreateVoyagePayload, UpdateVoyagePayload, VoyageQuery } from '../../types/voyage';
import { Pagination } from '../../components/ui/Pagination';
import { Plus } from 'lucide-react';
import { useAuthStore } from '../../store/authStore';
import { AppRoles, VOYAGE_STATUSES } from '../../lib/constants';
import { ShipSelect } from '../../components/ships/ShipSelect';
import { DateRangePicker } from '../../components/ui/DateRangePicker';
import { ExportButton } from '../../components/ui/ExportButton';

export const VoyagesPage: React.FC = () => {
  const { user } = useAuthStore();
  const canWrite = user?.roles.includes(AppRoles.Admin) || user?.roles.includes(AppRoles.FleetManager) || false;

  const [searchParams] = useSearchParams();
  const departureFrom = searchParams.get('departureFrom') || undefined;
  const departureTo = searchParams.get('departureTo') || undefined;

  const [query, setQuery] = useState<VoyageQuery>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'departureDate',
    sortDescending: true,
    departureFrom,
    departureTo,
  });

  const { data, isLoading } = useVoyagesQuery(query);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingVoyage, setEditingVoyage] = useState<Voyage | undefined>();

  const createMutation = useCreateVoyageMutation();
  const updateMutation = useUpdateVoyageMutation(editingVoyage?.id || '');
  const deleteMutation = useDeleteVoyageMutation();

  const handleCreateClick = () => {
    setEditingVoyage(undefined);
    setIsModalOpen(true);
  };

  const handleEditClick = (voyage: Voyage) => {
    setEditingVoyage(voyage);
    setIsModalOpen(true);
  };

  const handleDeleteClick = (voyage: Voyage) => {
    if (window.confirm(`Are you sure you want to delete voyage ${voyage.voyageNumber}?`)) {
      deleteMutation.mutate(voyage.id);
    }
  };

  const handleSubmit = (payload: CreateVoyagePayload | UpdateVoyagePayload) => {
    if (editingVoyage) {
      updateMutation.mutate(payload as UpdateVoyagePayload, {
        onSuccess: () => setIsModalOpen(false)
      });
    } else {
      createMutation.mutate(payload as CreateVoyagePayload, {
        onSuccess: () => setIsModalOpen(false)
      });
    }
  };

  return (
    <div className="space-y-6">
      <div className="sm:flex sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold leading-7 text-text-primary sm:truncate sm:tracking-tight">
            Voyages
          </h1>
          <p className="mt-2 text-sm text-text-muted">
            Schedule and track ship voyages across the fleet.
          </p>
        </div>
        <div className="flex items-center gap-2 mt-4 sm:mt-0">
          <ExportButton exportPath="/voyages/export" filters={query as unknown as Record<string, unknown>} />
          {canWrite && (
            <button
              onClick={handleCreateClick}
              className="inline-flex items-center rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
            >
              <Plus className="-ml-0.5 mr-1.5 h-5 w-5" aria-hidden="true" />
              Schedule Voyage
            </button>
          )}
        </div>
      </div>

      <div className="flex flex-col sm:flex-row gap-4 items-end">
        <div className="flex-1">
          <label htmlFor="search" className="sr-only">Search</label>
          <input
            type="search"
            name="search"
            id="search"
            className="block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
            placeholder="Search voyages..."
            value={query.searchTerm || ''}
            onChange={(e) => setQuery(prev => ({ ...prev, searchTerm: e.target.value, pageNumber: 1 }))}
          />
        </div>
        <div className="w-full sm:w-64">
          <ShipSelect value={query.shipId || ''} onChange={(val) => setQuery(prev => ({ ...prev, shipId: val || undefined, pageNumber: 1 }))} placeholder="All Ships" />
        </div>
        <div className="w-full sm:w-48">
          <select
            value={query.status || ''}
            onChange={(e) => setQuery(prev => ({ ...prev, status: e.target.value || undefined, pageNumber: 1 }))}
            className="block w-full rounded-md border-0 bg-surface-hover py-1.5 text-text-primary shadow-sm ring-1 ring-inset ring-border focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
          >
            <option value="">All Statuses</option>
            {VOYAGE_STATUSES.map(s => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        </div>
      </div>

      <div className="flex flex-col sm:flex-row gap-4 items-center">
        <div className="flex-shrink-0">
          <DateRangePicker 
            fromValue={query.departureFrom} 
            toValue={query.departureTo} 
            onFromChange={(val) => setQuery(prev => ({ ...prev, departureFrom: val, pageNumber: 1 }))} 
            onToChange={(val) => setQuery(prev => ({ ...prev, departureTo: val, pageNumber: 1 }))} 
          />
        </div>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-8">
          <div className="h-8 w-8 animate-spin rounded-full border-b-2 border-white"></div>
        </div>
      ) : (
        <>
          <VoyagesTable
            voyages={data?.items || []}
            canWrite={canWrite}
            onEdit={handleEditClick}
            onDelete={handleDeleteClick}
          />
          {data && (
            <Pagination
              pageNumber={data.pageNumber}
              totalPages={Math.ceil(data.totalCount / query.pageSize)}
              onPageChange={(page) => setQuery(prev => ({ ...prev, pageNumber: page }))}
            />
          )}
        </>
      )}

      {isModalOpen && (
        <VoyageFormModal
          isOpen={isModalOpen}
          onClose={() => setIsModalOpen(false)}
          voyage={editingVoyage}
          onSubmit={handleSubmit}
          isLoading={createMutation.isPending || updateMutation.isPending}
        />
      )}
    </div>
  );
};
