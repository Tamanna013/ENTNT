import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  useCrewMemberQuery, 
  useUnassignFromShipMutation,
  useCertificationsQuery,
  useUploadCertificationMutation,
  useDeleteCertificationMutation
} from '../../hooks/useCrew';
import { useAuthStore } from '../../store/authStore';
import { Button } from '../../components/ui/Button';
import { Badge } from '../../components/ui/Badge';
import { AssignToShipModal } from '../../components/crew/AssignToShipModal';
import { FileUpload } from '../../components/ui/FileUpload';

export const CrewDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const user = useAuthStore(state => state.user);
  const canWrite = user?.roles.includes('Admin') || user?.roles.includes('CrewManager') || false;

  const { data: crew, isLoading: isCrewLoading } = useCrewMemberQuery(id!);
  const { mutateAsync: unassign } = useUnassignFromShipMutation();
  const { data: certifications, isLoading: isCertsLoading } = useCertificationsQuery(id!);
  const { mutateAsync: uploadCert, isPending: isLoading } = useUploadCertificationMutation(id!);
  const { mutateAsync: deleteCert } = useDeleteCertificationMutation(id!);

  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);
  
  // Cert form state
  const [certName, setCertName] = useState('');
  const [expiryDate, setExpiryDate] = useState('');
  const [certFile, setCertFile] = useState<File | null>(null);

  if (isCrewLoading) return <div className="p-8 text-text-muted">Loading crew member...</div>;
  if (!crew) return <div className="p-8 text-red-400">Crew member not found</div>;

  const handleUnassign = async () => {
    if (window.confirm(`Are you sure you want to unassign ${crew.firstName} from ${crew.shipName}?`)) {
      await unassign(crew.id);
    }
  };

  const handleUploadCert = async () => {
    if (!certFile || !certName || !expiryDate) return;
    try {
      await uploadCert({
        file: certFile,
        certificationName: certName,
        expiryDate
      });
      setCertName('');
      setExpiryDate('');
      setCertFile(null);
    } catch (e) {
      console.error('Failed to upload certification', e);
    }
  };

  const handleDeleteCert = async (certId: string) => {
    if (window.confirm('Are you sure you want to delete this certification?')) {
      await deleteCert(certId);
    }
  };

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      <div className="flex items-center gap-4">
        <Button variant="secondary" onClick={() => navigate('/crew')}>
          ← Back to Crew
        </Button>
        <h1 className="text-2xl font-bold text-text-primary">
          {crew.firstName} {crew.lastName}
        </h1>
        <Badge text={crew.status} color={
          crew.status === 'Active' ? 'green' : 
          crew.status === 'Terminated' ? 'red' : 
          crew.status === 'OnLeave' ? 'yellow' : 'gray'
        } />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="md:col-span-2 space-y-6">
          <div className="bg-surface rounded-lg border border-border p-6">
            <h2 className="text-lg font-semibold text-text-primary mb-4">Profile Information</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-y-4 gap-x-8">
              <div>
                <div className="text-sm text-text-muted">Rank</div>
                <div className="text-text-primary font-medium">{crew.rank}</div>
              </div>
              <div>
                <div className="text-sm text-text-muted">Nationality</div>
                <div className="text-text-primary font-medium">{crew.nationality}</div>
              </div>
              <div>
                <div className="text-sm text-text-muted">License Number</div>
                <div className="text-text-primary font-medium">{crew.licenseNumber}</div>
              </div>
              <div>
                <div className="text-sm text-text-muted">Date of Birth</div>
                <div className="text-text-primary font-medium">{crew.dateOfBirth}</div>
              </div>
              <div>
                <div className="text-sm text-text-muted">Hire Date</div>
                <div className="text-text-primary font-medium">{crew.hireDate}</div>
              </div>
              <div>
                <div className="text-sm text-text-muted">Contact Email</div>
                <div className="text-text-primary font-medium">{crew.contactEmail || '—'}</div>
              </div>
              <div>
                <div className="text-sm text-text-muted">Contact Phone</div>
                <div className="text-text-primary font-medium">{crew.contactPhone || '—'}</div>
              </div>
            </div>
          </div>

          <div className="bg-surface rounded-lg border border-border p-6">
            <h2 className="text-lg font-semibold text-text-primary mb-4">Certifications</h2>
            
            {canWrite && (
              <div className="mb-6 p-4 bg-background rounded-lg border border-border">
                <h3 className="text-sm font-medium text-text-primary mb-3">Upload New Certification</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                  <div>
                    <label className="block text-xs text-text-muted mb-1">Certification Name</label>
                    <input 
                      type="text" 
                      className="w-full bg-surface border border-border rounded px-3 py-1.5 text-text-primary text-sm"
                      value={certName}
                      onChange={e => setCertName(e.target.value)}
                      placeholder="e.g. STCW Basic Safety"
                    />
                  </div>
                  <div>
                    <label className="block text-xs text-text-muted mb-1">Expiry Date</label>
                    <input 
                      type="date" 
                      className="w-full bg-surface border border-border rounded px-3 py-1.5 text-text-primary text-sm"
                      value={expiryDate}
                      onChange={e => setExpiryDate(e.target.value)}
                    />
                  </div>
                </div>
                
                <FileUpload
                  accept=".pdf,.jpg,.jpeg,.png"
                  onFileSelected={setCertFile}
                />
                
                {certFile && (
                  <div className="mt-3 flex justify-end">
                    <Button 
                      onClick={handleUploadCert} 
                      disabled={!certName || !expiryDate || isLoading}
                    >
                      {isLoading ? 'Uploading...' : 'Upload Certification'}
                    </Button>
                  </div>
                )}
              </div>
            )}

            {isCertsLoading ? (
              <div className="text-text-muted">Loading certifications...</div>
            ) : certifications?.length === 0 ? (
              <div className="text-text-muted italic">No certifications found.</div>
            ) : (
              <div className="space-y-3">
                {certifications?.map(cert => (
                  <div key={cert.id} className="flex items-center justify-between p-3 bg-background rounded-lg border border-border">
                    <div>
                      <div className="font-medium text-text-primary flex items-center gap-2">
                        {cert.certificationName}
                        {cert.isExpired && <Badge color="red" text="Expired" />}
                      </div>
                      <div className="text-xs text-text-muted mt-1">
                        Expires: {cert.expiryDate} • File: {cert.fileName}
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <a 
                        href={`http://localhost:5000${cert.downloadUrl}`} 
                        target="_blank" 
                        rel="noopener noreferrer"
                        className="text-primary-400 hover:text-primary-300 text-sm font-medium"
                      >
                        Download
                      </a>
                      {canWrite && (
                        <button 
                          onClick={() => handleDeleteCert(cert.id)}
                          className="text-red-400 hover:text-red-300 text-sm font-medium"
                        >
                          Delete
                        </button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        <div className="space-y-6">
          <div className="bg-surface rounded-lg border border-border p-6">
            <h2 className="text-lg font-semibold text-text-primary mb-4">Current Assignment</h2>
            
            {crew.shipId ? (
              <div className="text-center p-4 bg-background rounded-lg border border-border">
                <div className="text-sm text-text-muted mb-1">Assigned to</div>
                <div className="text-xl font-bold text-text-primary mb-4">{crew.shipName}</div>
                {canWrite && (
                  <Button variant="secondary" onClick={handleUnassign}>
                    Unassign from Ship
                  </Button>
                )}
              </div>
            ) : (
              <div className="text-center p-4 bg-background rounded-lg border border-border border-dashed">
                <div className="text-text-muted mb-4 font-medium italic">Currently Unassigned</div>
                {canWrite && (
                  <Button className="w-full" onClick={() => setIsAssignModalOpen(true)}>
                    Assign to Ship
                  </Button>
                )}
              </div>
            )}
          </div>
        </div>
      </div>

      {isAssignModalOpen && (
        <AssignToShipModal
          isOpen={isAssignModalOpen}
          onClose={() => setIsAssignModalOpen(false)}
          crewMemberId={crew.id}
        />
      )}
    </div>
  );
};
