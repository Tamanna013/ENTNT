import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { useDocumentQuery, useVersionsQuery, useUploadNewVersionMutation } from '../../hooks/useDocuments';
import { Button } from '../../components/ui/Button';
import { Badge } from '../../components/ui/Badge';
import { ArrowLeft, Upload } from 'lucide-react';
import { VersionHistoryList } from '../../components/documents/VersionHistoryList';
import { UploadVersionModal } from '../../components/documents/UploadVersionModal';

export const DocumentDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const user = useAuthStore(state => state.user);
  
  const canWrite = !!(user?.roles?.includes('Admin') || user?.roles?.includes('FleetManager'));
  
  const { data: document, isLoading: isLoadingDoc, error: docError } = useDocumentQuery(id!);
  const { data: versions, isLoading: isLoadingVersions } = useVersionsQuery(id!);
  const uploadVersionMutation = useUploadNewVersionMutation(id!);

  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false);

  if (isLoadingDoc) return <div className="p-8 text-center text-text-muted">Loading document details...</div>;
  if (docError || !document) return <div className="p-8 text-center text-red-500">Document not found or error loading.</div>;

  const handleUploadVersion = async (file: File, changeNotes?: string) => {
    await uploadVersionMutation.mutateAsync({ file, changeNotes });
    setIsUploadModalOpen(false);
  };

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <Button variant="secondary" onClick={() => navigate(-1)} className="mb-6 -ml-4 text-text-muted border-none hover:bg-slate-100">
        <ArrowLeft className="w-4 h-4 mr-2" /> Back
      </Button>

      <div className="bg-white rounded-lg border border-slate-200 overflow-hidden mb-8 shadow-sm">
        <div className="p-6 border-b border-slate-200">
          <div className="flex justify-between items-start gap-4">
            <div>
              <h1 className="text-2xl font-bold text-slate-900 mb-2">{document.title}</h1>
              <div className="flex flex-wrap items-center gap-3">
                <Badge text={document.category} color="blue" />
                {document.entityName && (
                  <span className="text-sm font-medium text-slate-600 bg-slate-100 px-2 py-1 rounded">
                    Linked to: {document.entityName}
                  </span>
                )}
                <span className="text-sm text-text-muted">
                  Created {new Date(document.createdAt).toLocaleDateString()}
                </span>
              </div>
            </div>
            
            {canWrite && (
              <Button onClick={() => setIsUploadModalOpen(true)} className="flex-shrink-0">
                <Upload className="w-4 h-4 mr-2" /> Upload New Version
              </Button>
            )}
          </div>
          
          {document.description && (
            <div className="mt-4 text-slate-600 prose prose-sm max-w-none">
              <p>{document.description}</p>
            </div>
          )}
        </div>
      </div>

      <div className="space-y-4">
        <h2 className="text-xl font-semibold text-slate-900">Version History</h2>
        
        {isLoadingVersions ? (
          <div className="text-center py-8 text-text-muted">Loading versions...</div>
        ) : (
          <VersionHistoryList 
            versions={versions || []} 
            currentVersionNumber={document.currentVersionNumber} 
          />
        )}
      </div>

      <UploadVersionModal
        isOpen={isUploadModalOpen}
        onClose={() => setIsUploadModalOpen(false)}
        onSubmit={handleUploadVersion}
        isLoading={uploadVersionMutation.isPending}
      />
    </div>
  );
};
