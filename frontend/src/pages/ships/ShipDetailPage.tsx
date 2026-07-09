import React, { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { 
  useShipQuery, 
  useShipAttachmentsQuery, 
  useUploadShipAttachmentMutation, 
  useSetPrimaryPhotoMutation,
  useShipAiMaintenanceRecommendationsQuery 
} from '../../hooks/useShips';
import { useCrewForShipQuery } from '../../hooks/useCrew';
import { useVoyagesForShipQuery } from '../../hooks/useVoyages';
import { useMaintenanceForShipQuery } from '../../hooks/useMaintenance';
import { useFuelLogsForShipQuery } from '../../hooks/useFuel';
import { useAuthStore } from '../../store/authStore';
import { FileUpload } from '../../components/ui/FileUpload';
import { useAuthenticatedImage } from '../../hooks/useAuthenticatedImage';
import { Badge } from '../../components/ui/Badge';
import { Button } from '../../components/ui/Button';
import { CrewTable } from '../../components/crew/CrewTable';
import { VoyagesTable } from '../../components/voyages/VoyagesTable';
import { MaintenanceTable } from '../../components/maintenance/MaintenanceTable';
import { FuelTable } from '../../components/fuel/FuelTable';
import { AiRecommendationCard } from '../../components/assistant/AiRecommendationCard';
import { Attachment } from '../../types/ship';
import { apiClient } from '../../api/client';

export const ShipDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { data: ship, isLoading, error } = useShipQuery(id || '');
  const { data: aiRecommendations, isLoading: isLoadingAiRecommendations } = useShipAiMaintenanceRecommendationsQuery(id || '');
  const { data: attachments, isLoading: isAttachmentsLoading } = useShipAttachmentsQuery(id || '');
  
  const uploadMutation = useUploadShipAttachmentMutation();
  const setPrimaryPhotoMutation = useSetPrimaryPhotoMutation();
  const primaryPhotoUrl = useAuthenticatedImage(ship?.primaryPhotoUrl || null);

  const [crewPage, setCrewPage] = useState(1);
  const { data: crewData, isLoading: isCrewLoading } = useCrewForShipQuery(id || '', {
    pageNumber: crewPage,
    pageSize: 10
  });

  const { data: voyagesData, isLoading: isVoyagesLoading } = useVoyagesForShipQuery(id || '', {
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'departureDate',
    sortDescending: true
  });

  const { data: maintenanceData, isLoading: isMaintenanceLoading } = useMaintenanceForShipQuery(id || '', {
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'scheduledDate'
  });

  const { data: fuelLogsData, isLoading: fuelLogsLoading } = useFuelLogsForShipQuery(id || '', {
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'recordedDate',
    sortDescending: true
  });

  const user = useAuthStore(state => state.user);
  const canWrite = user?.roles?.includes('Admin') || user?.roles?.includes('FleetManager');

  const [uploadError, setUploadError] = useState<string | null>(null);

  const handleFileUpload = async (file: File) => {
    setUploadError(null);
    try {
      await uploadMutation.mutateAsync({ shipId: id!, file });
    } catch (err: any) {
      setUploadError(err.response?.data?.title || err.message || 'Upload failed');
    }
  };

  const handleSetPrimary = async (attachmentId: string) => {
    try {
      await setPrimaryPhotoMutation.mutateAsync({ shipId: id!, attachmentId });
    } catch (err: any) {
      alert('Failed to set primary photo: ' + (err.response?.data?.title || err.message));
    }
  };

  const handleDownload = async (attachment: Attachment) => {
    try {
      const response = await apiClient.get(attachment.downloadUrl, {
        responseType: 'blob',
      });
      const url = URL.createObjectURL(response.data);
      const a = document.createElement('a');
      a.href = url;
      a.download = attachment.fileName;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch (err) {
      alert('Download failed');
    }
  };

  if (isLoading) return <div className="text-text-muted">Loading ship details...</div>;
  if (error || !ship) return <div className="text-red-500">Failed to load ship</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-4">
        <Link to="/ships" className="text-text-muted hover:text-text-primary transition-colors">
          <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
        </Link>
        <h1 className="text-2xl font-bold text-text-primary">{ship.name}</h1>
        <Badge 
          text={ship.status} 
          color={ship.status === 'Active' ? 'green' : ship.status === 'InMaintenance' ? 'yellow' : 'gray'} 
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-surface rounded-lg p-6 border border-border">
            <h2 className="text-lg font-semibold text-text-primary mb-4">Ship Details</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <span className="block text-sm text-text-muted">Fleet</span>
                <span className="block text-text-primary">{ship.fleetName}</span>
              </div>
              <div>
                <span className="block text-sm text-text-muted">IMO Number</span>
                <span className="block text-text-primary">{ship.imo}</span>
              </div>
              <div>
                <span className="block text-sm text-text-muted">Type</span>
                <span className="block text-text-primary">{ship.type}</span>
              </div>
              <div>
                <span className="block text-sm text-text-muted">Flag</span>
                <span className="block text-text-primary">{ship.flag}</span>
              </div>
              <div>
                <span className="block text-sm text-text-muted">Year Built</span>
                <span className="block text-text-primary">{ship.yearBuilt}</span>
              </div>
              <div>
                <span className="block text-sm text-text-muted">Gross Tonnage</span>
                <span className="block text-text-primary">{ship.grossTonnage}</span>
              </div>
            </div>
          </div>

          <div className="bg-surface rounded-lg p-6 border border-border">
            <h2 className="text-lg font-semibold text-text-primary mb-4">Attachments</h2>
            
            {canWrite && (
              <div className="mb-6">
                <FileUpload 
                  onFileSelected={handleFileUpload} 
                  isLoading={uploadMutation.isPending}
                />
                {uploadError && <p className="text-sm text-red-500 mt-2">{uploadError}</p>}
              </div>
            )}

            {isAttachmentsLoading ? (
              <div className="text-text-muted">Loading attachments...</div>
            ) : attachments && attachments.length > 0 ? (
              <div className="space-y-3">
                {attachments.map(att => (
                  <div key={att.id} className="flex items-center justify-between p-3 bg-slate-900/50 rounded border border-border">
                    <div>
                      <div className="text-text-primary font-medium">{att.fileName}</div>
                      <div className="text-xs text-text-muted">
                        {new Date(att.createdAt).toLocaleDateString()} • {(att.fileSizeBytes / 1024 / 1024).toFixed(2)} MB
                      </div>
                    </div>
                    <div className="flex space-x-3">
                      {canWrite && att.contentType.startsWith('image/') && (
                        <button 
                          onClick={() => handleSetPrimary(att.id)}
                          disabled={setPrimaryPhotoMutation.isPending}
                          className="text-xs text-primary-400 hover:text-primary-300"
                        >
                          Set as Primary
                        </button>
                      )}
                      <button 
                        onClick={() => handleDownload(att)}
                        className="text-xs text-text-primary hover:text-text-primary flex items-center space-x-1"
                      >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                        </svg>
                        <span>Download</span>
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-text-muted text-sm text-center py-4">No attachments uploaded yet.</div>
            )}
          </div>

          <div className="bg-surface rounded-lg border border-border p-6">
            <h2 className="text-lg font-semibold text-text-primary mb-4">Crew Members</h2>
            <CrewTable
              crew={crewData?.items || []}
              isLoading={isCrewLoading}
              canWrite={false}
            />
            {crewData && crewData.totalCount > 10 && (
              <div className="mt-4 flex justify-between items-center text-sm text-text-muted">
                <div>
                  Showing {(crewPage - 1) * 10 + 1} to {Math.min(crewPage * 10, crewData.totalCount)} of {crewData.totalCount} crew
                </div>
                <div className="flex gap-2">
                  <Button 
                    variant="secondary" 
                    className="px-2 py-1 text-xs"
                    disabled={crewPage === 1}
                    onClick={() => setCrewPage(p => p - 1)}
                  >
                    Previous
                  </Button>
                  <Button 
                    variant="secondary" 
                    className="px-2 py-1 text-xs"
                    disabled={crewPage >= Math.ceil(crewData.totalCount / 10)}
                    onClick={() => setCrewPage(p => p + 1)}
                  >
                    Next
                  </Button>
                </div>
              </div>
            )}
          </div>

          <div className="bg-surface rounded-lg border border-border p-6">
            <h2 className="text-lg font-semibold text-text-primary mb-4">Voyage History</h2>
            {isVoyagesLoading ? (
              <div className="text-text-muted">Loading voyages...</div>
            ) : (
              <VoyagesTable
                voyages={voyagesData?.items || []}
                canWrite={false}
                onEdit={() => {}}
                onDelete={() => {}}
              />
            )}
          </div>

          <div className="bg-surface rounded-lg border border-border p-6">
            <h2 className="text-lg font-semibold text-text-primary mb-4">Maintenance Recommendations</h2>
            <AiRecommendationCard
                title="AI Maintenance Recommendations"
                data={aiRecommendations}
                isLoading={isLoadingAiRecommendations}
            />
          </div>

          <div className="bg-surface rounded-lg border border-border p-6">
            <h2 className="text-lg font-semibold text-text-primary mb-4">Maintenance History</h2>
            {isMaintenanceLoading ? (
              <div className="text-text-muted">Loading maintenance records...</div>
            ) : (
              <MaintenanceTable
                records={maintenanceData?.items || []}
                isLoading={isMaintenanceLoading}
                canWrite={false}
              />
            )}
          </div>

          <div className="bg-surface rounded-lg border border-border p-6">
            <h2 className="text-lg font-semibold text-text-primary mb-4">Fuel Logs</h2>
            {fuelLogsLoading ? (
              <div className="text-text-muted">Loading fuel logs...</div>
            ) : (
              <FuelTable
                fuelLogs={fuelLogsData?.items || []}
                canWrite={false}
                onEdit={() => {}}
                onDelete={() => {}}
              />
            )}
          </div>
        </div>

        <div>
          <div className="bg-surface rounded-lg p-6 border border-border sticky top-6">
            <h2 className="text-lg font-semibold text-text-primary mb-4">Primary Photo</h2>
            {primaryPhotoUrl ? (
              <div className="rounded-lg overflow-hidden border border-border">
                <img src={primaryPhotoUrl} alt="Ship Primary" className="w-full h-auto object-cover" />
              </div>
            ) : (
              <div className="w-full aspect-video bg-background rounded-lg border border-border flex flex-col items-center justify-center text-text-muted">
                <svg className="w-12 h-12 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <span>No primary photo</span>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
