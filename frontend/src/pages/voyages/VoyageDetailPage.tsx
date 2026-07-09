import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useVoyageQuery, useUpdateVoyageMutation, useVoyageAiSummaryQuery } from '../../hooks/useVoyages';
import { VoyageStatusTransitionBar } from '../../components/voyages/VoyageStatusTransitionBar';
import { AiSummaryCard } from '../../components/assistant/AiSummaryCard';
import { useCargoForVoyageQuery } from '../../hooks/useCargo';
import { CargoTable } from '../../components/cargo/CargoTable';
import { ContainersTable } from '../../components/containers/ContainersTable';
import { VoyageFormModal } from '../../components/voyages/VoyageFormModal';
import { UpdateVoyagePayload } from '../../types/voyage';
import { Badge } from '../../components/ui/Badge';
import { useAuthStore } from '../../store/authStore';
import { AppRoles } from '../../lib/constants';
import { isTerminalStatus } from '../../lib/voyageStatusTransitions';
import { Pencil, ArrowLeft, Download } from 'lucide-react';
import { useContainersForVoyageQuery, useUpdateContainerMutation } from '../../hooks/useContainers';
import { downloadAuthenticatedFile } from '../../lib/downloadFile';
import { ContainerQuery, Container } from '../../types/container';
import { Pagination } from '../../components/ui/Pagination';
import { ContainerFormModal } from '../../components/containers/ContainerFormModal';

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Scheduled': return 'blue';
    case 'Delayed': return 'amber';
    case 'InTransit': return 'purple';
    case 'Completed': return 'green';
    case 'Cancelled': return 'red';
    default: return 'gray';
  }
};

export const VoyageDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: voyage, isLoading, error } = useVoyageQuery(id!);
  const { data: aiSummary, isLoading: isLoadingAiSummary } = useVoyageAiSummaryQuery(id!);
  
  const { data: cargoData, isLoading: isLoadingCargo } = useCargoForVoyageQuery(id!, {
    pageNumber: 1,
    pageSize: 20
  });

  const [containerQuery, setContainerQuery] = useState<Omit<ContainerQuery, 'voyageId'>>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'createdAt',
    sortDescending: true,
  });
  
  const { data: containersData } = useContainersForVoyageQuery(id!, containerQuery);

  const [isContainerEditModalOpen, setIsContainerEditModalOpen] = useState(false);
  const [editingContainer, setEditingContainer] = useState<Container | undefined>();

  const { user } = useAuthStore();
  const canWrite = user?.roles.includes(AppRoles.Admin) || user?.roles.includes(AppRoles.FleetManager) || false;

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const updateMutation = useUpdateVoyageMutation(id!);
  
  const updateContainerMutation = useUpdateContainerMutation(editingContainer?.id || '');

  if (isLoading) {
    return (
      <div className="flex justify-center py-8">
        <div className="h-8 w-8 animate-spin rounded-full border-b-2 border-white"></div>
      </div>
    );
  }

  if (error || !voyage) {
    return <div className="text-red-400">Failed to load voyage details.</div>;
  }

  const isTerminal = isTerminalStatus(voyage.status);
  const canEdit = !isTerminal && canWrite;

  const handleEditSubmit = (payload: any) => {
    updateMutation.mutate(payload as UpdateVoyagePayload, {
      onSuccess: () => setIsEditModalOpen(false)
    });
  };

  const handleContainerPageChange = (page: number) => {
    setContainerQuery({ ...containerQuery, pageNumber: page });
  };

  const handleContainerEdit = (container: Container) => {
    setEditingContainer(container);
    setIsContainerEditModalOpen(true);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/voyages')}
          className="rounded-full p-2 text-text-muted hover:bg-surface-hover hover:text-text-primary"
        >
          <ArrowLeft className="h-5 w-5" />
        </button>
        <div className="flex-1 flex items-center justify-between">
          <div className="flex items-center gap-4">
            <h1 className="text-2xl font-bold leading-7 text-text-primary sm:truncate sm:tracking-tight">
              Voyage {voyage.voyageNumber}
            </h1>
            {!canWrite && <Badge text={voyage.status} color={getStatusColor(voyage.status) as any} />}
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => downloadAuthenticatedFile(`/voyages/${id}/manifest-pdf`, `voyage-manifest-${voyage.voyageNumber}.pdf`)}
              className="inline-flex items-center rounded-md bg-white/10 px-3 py-2 text-sm font-semibold text-text-primary shadow-sm hover:bg-white/20"
            >
              <Download className="-ml-0.5 mr-1.5 h-4 w-4" />
              Manifest PDF
            </button>
            {canEdit && (
              <button
                onClick={() => setIsEditModalOpen(true)}
                className="inline-flex items-center rounded-md bg-white/10 px-3 py-2 text-sm font-semibold text-text-primary shadow-sm hover:bg-white/20"
              >
                <Pencil className="-ml-0.5 mr-1.5 h-4 w-4" />
                Edit details
              </button>
            )}
          </div>
        </div>
      </div>

      {canWrite && <VoyageStatusTransitionBar voyage={voyage} />}

      <AiSummaryCard title="AI Voyage Summary" data={aiSummary} isLoading={isLoadingAiSummary} />

      <div className="overflow-hidden bg-surface shadow sm:rounded-lg border border-white/10">
        <div className="px-4 py-6 sm:px-6">
          <h3 className="text-base font-semibold leading-7 text-text-primary">Voyage Information</h3>
          <p className="mt-1 max-w-2xl text-sm leading-6 text-text-muted">Detailed schedule and routing data.</p>
        </div>
        <div className="border-t border-white/10">
          <dl className="divide-y divide-white/10">
            <div className="px-4 py-6 sm:grid grid-cols-1 sm:grid-cols-3 sm:gap-4 sm:px-6">
              <dt className="text-sm font-medium text-text-muted">Ship</dt>
              <dd className="mt-1 text-sm leading-6 text-text-primary sm:col-span-2 sm:mt-0">{voyage.shipName}</dd>
            </div>
            <div className="px-4 py-6 sm:grid grid-cols-1 sm:grid-cols-3 sm:gap-4 sm:px-6">
              <dt className="text-sm font-medium text-text-muted">Route</dt>
              <dd className="mt-1 text-sm leading-6 text-text-primary sm:col-span-2 sm:mt-0">
                {voyage.originPortName} &rarr; {voyage.destinationPortName}
              </dd>
            </div>
            <div className="px-4 py-6 sm:grid grid-cols-1 sm:grid-cols-3 sm:gap-4 sm:px-6">
              <dt className="text-sm font-medium text-text-muted">Departure</dt>
              <dd className="mt-1 text-sm leading-6 text-text-primary sm:col-span-2 sm:mt-0">
                {new Date(voyage.departureDate).toLocaleDateString()}
              </dd>
            </div>
            <div className="px-4 py-6 sm:grid grid-cols-1 sm:grid-cols-3 sm:gap-4 sm:px-6">
              <dt className="text-sm font-medium text-text-muted">Estimated Arrival</dt>
              <dd className="mt-1 text-sm leading-6 text-text-primary sm:col-span-2 sm:mt-0">
                {new Date(voyage.estimatedArrivalDate).toLocaleDateString()}
              </dd>
            </div>
            <div className="px-4 py-6 sm:grid grid-cols-1 sm:grid-cols-3 sm:gap-4 sm:px-6">
              <dt className="text-sm font-medium text-text-muted">Actual Arrival</dt>
              <dd className="mt-1 text-sm leading-6 text-text-primary sm:col-span-2 sm:mt-0">
                {voyage.actualArrivalDate ? new Date(voyage.actualArrivalDate).toLocaleDateString() : '-'}
              </dd>
            </div>
            <div className="px-4 py-6 sm:grid grid-cols-1 sm:grid-cols-3 sm:gap-4 sm:px-6">
              <dt className="text-sm font-medium text-text-muted">Status</dt>
              <dd className="mt-1 text-sm leading-6 text-text-primary sm:col-span-2 sm:mt-0">
                <Badge text={voyage.status} color={getStatusColor(voyage.status) as any} />
              </dd>
            </div>
            {voyage.notes && (
              <div className="px-4 py-6 sm:grid grid-cols-1 sm:grid-cols-3 sm:gap-4 sm:px-6">
                <dt className="text-sm font-medium text-text-muted">Notes</dt>
                <dd className="mt-1 text-sm leading-6 text-text-primary sm:col-span-2 sm:mt-0 whitespace-pre-wrap">
                  {voyage.notes}
                </dd>
              </div>
            )}
          </dl>
        </div>
      </div>

      {/* Cargo Manifest Section */}
      <div className="bg-surface rounded-lg shadow-sm border border-border mt-6">
        <div className="px-6 py-5 border-b border-border flex justify-between items-center">
          <h3 className="text-base font-semibold leading-7 text-slate-100">Cargo Manifest</h3>
        </div>
        <div className="p-0">
          <CargoTable 
            data={cargoData?.items || []} 
            isLoading={isLoadingCargo}
            canWrite={false}
          />
        </div>
      </div>

      {/* Containers Section */}
      <div className="bg-surface rounded-lg shadow-sm border border-border mt-6">
        <div className="px-6 py-5 border-b border-border flex justify-between items-center">
          <h3 className="text-base font-semibold leading-7 text-slate-100">Containers on this Voyage</h3>
        </div>
        <div className="p-0">
          <ContainersTable 
            containers={containersData?.items || []} 
            canWrite={canWrite}
            onEdit={handleContainerEdit}
          />
          {containersData && (
            <div className="p-4 border-t border-border">
              <Pagination
                pageNumber={containersData.pageNumber}
                totalPages={Math.ceil(containersData.totalCount / containersData.pageSize)}
                onPageChange={handleContainerPageChange}
              />
            </div>
          )}
        </div>
      </div>

      {isEditModalOpen && (
        <VoyageFormModal
          isOpen={isEditModalOpen}
          onClose={() => setIsEditModalOpen(false)}
          voyage={voyage}
          onSubmit={handleEditSubmit}
          isLoading={updateMutation.isPending}
        />
      )}
      
      {isContainerEditModalOpen && editingContainer && (
        <ContainerFormModal
          isOpen={isContainerEditModalOpen}
          onClose={() => setIsContainerEditModalOpen(false)}
          container={editingContainer}
          onSubmit={async (payload) => {
             await updateContainerMutation.mutateAsync(payload);
             setIsContainerEditModalOpen(false);
          }}
          isLoading={updateContainerMutation.isPending}
        />
      )}
    </div>
  );
};
