import { useParams, useNavigate } from 'react-router-dom';
import { useIncidentQuery, useIncidentAiReportQuery } from '../../hooks/useIncidents';
import { IncidentStatusTransitionBar } from '../../components/incidents/IncidentStatusTransitionBar';
import { useAuthStore } from '../../store/authStore';
import { Button } from '../../components/ui/Button';
import { AiSummaryCard } from '../../components/assistant/AiSummaryCard';
import { ArrowLeft, AlertTriangle, Download } from 'lucide-react';
import { Badge } from '../../components/ui/Badge';
import { downloadAuthenticatedFile } from '../../lib/downloadFile';

export function IncidentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: incident, isLoading, error } = useIncidentQuery(id!);
  const { data: aiSummary, isLoading: isLoadingAiSummary } = useIncidentAiReportQuery(id!);
  const user = useAuthStore(state => state.user);
  
  const canWrite = user?.roles?.includes('Admin') || user?.roles?.includes('FleetManager');

  if (isLoading) return <div className="p-8 text-center">Loading incident details...</div>;
  if (error || !incident) return <div className="p-8 text-center text-red-600">Failed to load incident.</div>;

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="flex items-center justify-between gap-4">
        <Button variant="secondary" onClick={() => navigate('/incidents')} className="w-auto px-4 py-2 flex items-center">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>
        <Button variant="secondary" onClick={() => downloadAuthenticatedFile(`/incidents/${id}/report-pdf`, `incident-report-${id}.pdf`)} className="flex items-center gap-2">
          <Download className="w-4 h-4" />
          Download Report PDF
        </Button>
      </div>

      <AiSummaryCard title="AI Incident Narrative" data={aiSummary} isLoading={isLoadingAiSummary} />

      <div className="bg-white shadow-sm rounded-lg border border-gray-200 overflow-hidden">
        <div className="p-6 border-b border-gray-200">
          <div className="flex justify-between items-start">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">{incident.title}</h1>
              <p className="text-sm text-gray-500 mt-1">
                Reported by {incident.reportedByUserName} at {new Date(incident.createdAt).toLocaleString()}
              </p>
            </div>
            <div className="flex flex-col items-end gap-2">
              <Badge color={incident.status === 'Closed' ? 'gray' : 'blue'} text={incident.status} />
              <div className="flex items-center gap-1.5">
                <Badge 
                  color={incident.severity === 'Critical' ? 'red' : incident.severity === 'High' ? 'yellow' : 'gray'} 
                  text={incident.severity} 
                />
                {incident.severity === 'Critical' && (
                  <AlertTriangle className="h-4 w-4 text-red-600 animate-pulse" />
                )}
              </div>
            </div>
          </div>
        </div>

        <div className="p-6 grid grid-cols-1 md:grid-cols-2 gap-8">
          <div className="space-y-6">
            <div>
              <h3 className="text-sm font-medium text-gray-500 uppercase tracking-wider mb-2">Details</h3>
              <dl className="grid grid-cols-1 gap-3 text-sm">
                <div className="flex justify-between py-1 border-b">
                  <dt className="text-gray-500">Ship</dt>
                  <dd className="font-medium">{incident.shipName}</dd>
                </div>
                <div className="flex justify-between py-1 border-b">
                  <dt className="text-gray-500">Voyage</dt>
                  <dd className="font-medium">{incident.voyageNumber || 'None'}</dd>
                </div>
                <div className="flex justify-between py-1 border-b">
                  <dt className="text-gray-500">Occurred At</dt>
                  <dd className="font-medium">{new Date(incident.occurredAt).toLocaleString()}</dd>
                </div>
                {incident.resolvedAt && (
                  <div className="flex justify-between py-1 border-b">
                    <dt className="text-gray-500">Resolved At</dt>
                    <dd className="font-medium">{new Date(incident.resolvedAt).toLocaleString()}</dd>
                  </div>
                )}
              </dl>
            </div>
          </div>

          <div className="space-y-6">
            <div>
              <h3 className="text-sm font-medium text-gray-500 uppercase tracking-wider mb-2">Description</h3>
              <div className="bg-gray-50 p-4 rounded-md text-sm text-gray-800 whitespace-pre-wrap min-h-[100px]">
                {incident.description}
              </div>
            </div>
            
            {incident.resolutionNotes && (
              <div>
                <h3 className="text-sm font-medium text-gray-500 uppercase tracking-wider mb-2">Resolution Notes</h3>
                <div className="bg-green-50 p-4 rounded-md text-sm text-green-900 whitespace-pre-wrap border border-green-100">
                  {incident.resolutionNotes}
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {canWrite && incident.status !== 'Closed' && (
        <IncidentStatusTransitionBar 
          incidentId={incident.id} 
          currentStatus={incident.status} 
        />
      )}
    </div>
  );
}
