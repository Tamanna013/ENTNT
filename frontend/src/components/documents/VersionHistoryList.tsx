import React from 'react';
import { Download } from 'lucide-react';
import { DocumentVersion } from '../../types/document';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';
import { downloadAuthenticatedFile } from '../../lib/downloadFile';

interface VersionHistoryListProps {
  versions: DocumentVersion[];
  currentVersionNumber: number;
}

export const VersionHistoryList: React.FC<VersionHistoryListProps> = ({
  versions,
  currentVersionNumber
}) => {
  if (!versions || versions.length === 0) {
    return <div className="text-text-muted text-sm">No versions found.</div>;
  }

  return (
    <div className="space-y-4">
      {versions.map((version) => {
        const isCurrent = version.versionNumber === currentVersionNumber;
        return (
          <div 
            key={version.id} 
            className={`p-4 rounded-lg border flex items-start justify-between ${ isCurrent ? 'bg-blue-50/50 border-blue-200' : 'bg-white border-slate-200' }`}
          >
            <div className="flex-1 min-w-0 pr-4">
              <div className="flex items-center gap-3 mb-1">
                <span className="font-semibold text-slate-900 text-lg">
                  Version {version.versionNumber}
                </span>
                {isCurrent && (
                  <Badge text="Current" color="blue" />
                )}
              </div>
              <div className="text-sm text-text-muted mb-2">
                Uploaded by <span className="font-medium text-slate-700">{version.uploadedByUserName}</span> on {new Date(version.createdAt).toLocaleString()}
              </div>
              {version.changeNotes && (
                <div className="text-sm text-slate-700 bg-slate-50 p-3 rounded border border-slate-100">
                  {version.changeNotes}
                </div>
              )}
            </div>
            
            <div className="flex-shrink-0">
              <Button
                variant="secondary"
                onClick={() => downloadAuthenticatedFile(version.downloadUrl, version.fileName)}
                className="flex items-center gap-2"
              >
                <Download className="w-4 h-4" />
                Download
              </Button>
            </div>
          </div>
        );
      })}
    </div>
  );
};
