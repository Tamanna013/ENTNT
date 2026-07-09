import React, { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useContainerQuery, useTrackingEventsQuery, useUnlinkCargoMutation } from '../../hooks/useContainers';
import { useCargoItemQuery } from '../../hooks/useCargo';
import { useAuthStore } from '../../store/authStore';
import { AppRoles } from '../../lib/constants';
import { Badge } from '../../components/ui/Badge';
import { Button } from '../../components/ui/Button';
import { ArrowLeft, Package, Link as LinkIcon, Unlink } from 'lucide-react';
import { LinkCargoModal } from '../../components/containers/LinkCargoModal';
import { TrackingTimeline } from '../../components/containers/TrackingTimeline';
import { RecordTrackingEventForm } from '../../components/containers/RecordTrackingEventForm';

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Empty': return 'gray';
    case 'Loaded': return 'blue';
    case 'InTransit': return 'purple';
    case 'AtPort': return 'amber';
    case 'Delivered': return 'green';
    default: return 'gray';
  }
};

const LinkedCargoItem: React.FC<{ cargoId: string; containerId: string; canWrite: boolean }> = ({ cargoId, containerId, canWrite }) => {
  const { data: cargo, isLoading } = useCargoItemQuery(cargoId);
  const unlinkMutation = useUnlinkCargoMutation(containerId);

  if (isLoading) return <li className="py-3 text-gray-500 text-sm">Loading cargo...</li>;
  if (!cargo) return null;

  return (
    <li className="py-3 flex justify-between items-center">
      <div className="flex items-center gap-3">
        <div className="bg-surface-hover p-2 rounded-md">
          <Package className="h-5 w-5 text-indigo-400" />
        </div>
        <div>
          <p className="text-sm font-medium text-text-primary">{cargo.description}</p>
          <p className="text-xs text-text-muted">Voyage: {cargo.voyageNumber || 'Unassigned'} • Type: {cargo.type}</p>
        </div>
      </div>
      {canWrite && (
        <Button 
          variant="secondary" 
          className="text-red-400 hover:text-red-300"
          onClick={() => {
            if (window.confirm('Are you sure you want to unlink this cargo?')) {
              unlinkMutation.mutate(cargoId);
            }
          }}
          isLoading={unlinkMutation.isPending}
        >
          <Unlink className="h-4 w-4 mr-1" />
          Unlink
        </Button>
      )}
    </li>
  );
};

export const ContainerDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const canWrite = user?.roles?.includes(AppRoles.Admin) || user?.roles?.includes(AppRoles.FleetManager) || false;

  const { data: container, isLoading: isLoadingContainer } = useContainerQuery(id!);
  const { data: trackingEvents, isLoading: isLoadingTracking } = useTrackingEventsQuery(id!);

  const [isLinkCargoModalOpen, setIsLinkCargoModalOpen] = useState(false);

  if (isLoadingContainer) {
    return (
      <div className="flex justify-center py-8">
        <div className="h-8 w-8 animate-spin rounded-full border-b-2 border-white"></div>
      </div>
    );
  }

  if (!container) {
    return <div className="text-red-400">Failed to load container details.</div>;
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/containers')}
          className="rounded-full p-2 text-text-muted hover:bg-surface-hover hover:text-text-primary"
        >
          <ArrowLeft className="h-5 w-5" />
        </button>
        <div className="flex-1 flex items-center justify-between">
          <div className="flex items-center gap-4">
            <h1 className="text-2xl font-bold leading-7 text-text-primary sm:truncate sm:tracking-tight">
              Container {container.containerNumber}
            </h1>
            <Badge text={container.status} color={getStatusColor(container.status) as any} />
          </div>
        </div>
      </div>

      {/* Metadata */}
      <div className="bg-surface shadow sm:rounded-lg border border-border">
        <div className="px-4 py-5 sm:px-6">
          <h3 className="text-base font-semibold leading-6 text-text-primary">Container Details</h3>
        </div>
        <div className="border-t border-border px-4 py-5 sm:p-0">
          <dl className="sm:divide-y sm:divide-slate-700">
            <div className="py-4 sm:grid grid-cols-1 sm:grid-cols-3 sm:gap-4 sm:px-6">
              <dt className="text-sm font-medium text-text-muted">Type</dt>
              <dd className="mt-1 text-sm text-text-primary sm:col-span-2 sm:mt-0">{container.type}</dd>
            </div>
            <div className="py-4 sm:grid grid-cols-1 sm:grid-cols-3 sm:gap-4 sm:px-6">
              <dt className="text-sm font-medium text-text-muted">Current Voyage</dt>
              <dd className="mt-1 text-sm text-text-primary sm:col-span-2 sm:mt-0">
                {container.currentVoyageId ? (
                  <Link to={`/voyages/${container.currentVoyageId}`} className="text-indigo-400 hover:underline">
                    {container.voyageNumber}
                  </Link>
                ) : (
                  <span className="text-gray-500">— Unassigned —</span>
                )}
              </dd>
            </div>
            <div className="py-4 sm:grid grid-cols-1 sm:grid-cols-3 sm:gap-4 sm:px-6">
              <dt className="text-sm font-medium text-text-muted">Created At</dt>
              <dd className="mt-1 text-sm text-text-primary sm:col-span-2 sm:mt-0">
                {new Date(container.createdAt).toLocaleDateString()}
              </dd>
            </div>
          </dl>
        </div>
      </div>

      {/* Linked Cargo Section */}
      <div className="bg-surface shadow sm:rounded-lg border border-border">
        <div className="px-4 py-5 sm:px-6 flex justify-between items-center border-b border-border">
          <h3 className="text-base font-semibold leading-6 text-text-primary">Linked Cargo</h3>
          {canWrite && (
            <Button onClick={() => setIsLinkCargoModalOpen(true)}>
              <LinkIcon className="h-4 w-4 mr-1.5" />
              Link Cargo
            </Button>
          )}
        </div>
        <div className="px-4 py-2 sm:px-6">
          {container.linkedCargoIds.length === 0 ? (
            <p className="py-4 text-sm text-text-muted">No cargo is currently linked to this container.</p>
          ) : (
            <ul className="divide-y divide-border">
              {container.linkedCargoIds.map(cargoId => (
                <LinkedCargoItem key={cargoId} cargoId={cargoId} containerId={container.id} canWrite={canWrite} />
              ))}
            </ul>
          )}
        </div>
      </div>

      {/* Tracking History Section */}
      <div className="bg-surface shadow sm:rounded-lg border border-border">
        <div className="px-4 py-5 sm:px-6 border-b border-border">
          <h3 className="text-base font-semibold leading-6 text-text-primary">Tracking History</h3>
        </div>
        <div className="px-4 py-6 sm:px-6">
          {canWrite && <RecordTrackingEventForm containerId={container.id} />}
          
          <div className="mt-6">
            {isLoadingTracking ? (
              <p className="text-text-muted text-sm">Loading history...</p>
            ) : (
              <TrackingTimeline events={trackingEvents || []} />
            )}
          </div>
        </div>
      </div>

      {isLinkCargoModalOpen && (
        <LinkCargoModal
          isOpen={isLinkCargoModalOpen}
          onClose={() => setIsLinkCargoModalOpen(false)}
          containerId={container.id}
          linkedCargoIds={container.linkedCargoIds}
        />
      )}
    </div>
  );
};
