import React, { useEffect, useState } from 'react';
import { useCrewQuery } from '../../hooks/useCrew';
import { crewApi } from '../../api/crewApi';
import { CrewCertification, CrewMember } from '../../types/crew';
import { Badge } from '../ui/Badge';

/**
 * EXPLICIT LIMITATION NOTICE:
 * This component intentionally uses an N+1 fetching pattern (fetching all crew, 
 * then fetching certifications for each crew member individually).
 * 
 * At the current scale of the project, this client-side composition is a known, 
 * acceptable approach. There is currently no backend endpoint to get 
 * "all expiring certifications across all crew".
 * 
 * In Phase 6 (Analytics & Reporting), a dedicated aggregation endpoint should be 
 * built to return this data efficiently. Do NOT over-engineer a complex 
 * client-side cache to solve this here; it is a deliberate backend omission 
 * deferred to a later milestone.
 */

interface AggregatedCert {
  crewName: string;
  crewId: string;
  cert: CrewCertification;
}

export const ExpiringCertificationsWidget: React.FC = () => {
  const { data: crewData, isLoading: isCrewLoading } = useCrewQuery({ pageNumber: 1, pageSize: 100 });
  const [certs, setCerts] = useState<AggregatedCert[]>([]);
  const [isLoadingCerts, setIsLoadingCerts] = useState(false);

  useEffect(() => {
    const fetchAllCerts = async (crewList: CrewMember[]) => {
      setIsLoadingCerts(true);
      try {
        const promises = crewList.map(async (crew) => {
          const crewCerts = await crewApi.getCertifications(crew.id);
          return crewCerts.map(cert => ({
            crewName: `${crew.firstName} ${crew.lastName}`,
            crewId: crew.id,
            cert
          }));
        });
        
        const results = await Promise.all(promises);
        const allCerts = results.flat();
        
        // Filter to next 30 days (or already expired)
        const now = new Date();
        const thirtyDaysFromNow = new Date();
        thirtyDaysFromNow.setDate(now.getDate() + 30);
        
        const expiringCerts = allCerts.filter(item => {
          const expiry = new Date(item.cert.expiryDate);
          return expiry <= thirtyDaysFromNow;
        });
        
        // Sort soonest expiring first
        expiringCerts.sort((a, b) => {
          return new Date(a.cert.expiryDate).getTime() - new Date(b.cert.expiryDate).getTime();
        });
        
        setCerts(expiringCerts.slice(0, 10)); // Top 10
      } catch (error) {
        console.error("Failed to fetch certifications for widget", error);
      } finally {
        setIsLoadingCerts(false);
      }
    };

    if (crewData?.items && crewData.items.length > 0) {
      fetchAllCerts(crewData.items);
    } else if (crewData?.items && crewData.items.length === 0) {
      setCerts([]);
    }
  }, [crewData]);

  const isLoading = isCrewLoading || isLoadingCerts;

  return (
    <div className="bg-surface rounded-lg p-6 border border-border">
      <h2 className="text-lg font-semibold text-text-primary mb-4">Needs Attention: Expiring Certifications (Next 30 Days)</h2>
      
      {isLoading ? (
        <div className="py-8 text-center text-text-muted">Loading certifications...</div>
      ) : certs.length === 0 ? (
        <div className="py-8 text-center text-text-muted italic">No certifications expiring soon. All good!</div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-text-primary">
            <thead className="bg-slate-900/50 text-xs uppercase text-text-muted border-b border-border">
              <tr>
                <th className="px-4 py-3">Crew Member</th>
                <th className="px-4 py-3">Certification</th>
                <th className="px-4 py-3">Expiry Date</th>
                <th className="px-4 py-3">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-700/50">
              {certs.map((item) => (
                <tr key={item.cert.id} className="hover:bg-slate-700/30">
                  <td className="px-4 py-3 font-medium text-text-primary">
                    <a href={`/crew/${item.crewId}`} className="hover:text-primary-400 transition-colors">
                      {item.crewName}
                    </a>
                  </td>
                  <td className="px-4 py-3">{item.cert.certificationName}</td>
                  <td className="px-4 py-3">{item.cert.expiryDate}</td>
                  <td className="px-4 py-3">
                    {item.cert.isExpired ? (
                      <Badge color="red" text="Expired" />
                    ) : (
                      <Badge color="yellow" text="Expiring Soon" />
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};
