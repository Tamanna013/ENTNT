import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useCargoItemQuery, useCargoAiRiskAssessmentQuery } from '../../hooks/useCargo';
import { useAuthStore } from '../../store/authStore';
import { AiRecommendationCard } from '../../components/assistant/AiRecommendationCard';
import { Button } from '../../components/ui/Button';
import { Badge } from '../../components/ui/Badge';
import { ArrowLeft } from 'lucide-react';

export const CargoDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: cargo, isLoading, error } = useCargoItemQuery(id || '');
  const { data: aiRiskAssessment, isLoading: isRiskAssessmentLoading } = useCargoAiRiskAssessmentQuery(id || '');
  
  const user = useAuthStore(state => state.user);
  const canWrite = user?.roles?.includes('Admin') || user?.roles?.includes('FleetManager');

  if (isLoading) return <div className="p-8 text-center text-text-muted">Loading cargo details...</div>;
  if (error || !cargo) return <div className="p-8 text-center text-red-500">Failed to load cargo details.</div>;

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      <div className="flex items-center space-x-4">
        <Button variant="secondary" onClick={() => navigate(-1)} className="px-3 py-2 flex items-center">
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back
        </Button>
        <h1 className="text-2xl font-bold text-slate-800">Cargo Details</h1>
        <Badge 
          text={cargo.status} 
          color={cargo.status === 'Delivered' ? 'green' : cargo.status === 'InTransit' ? 'blue' : 'yellow'} 
        />
        {cargo.type === 'Hazardous' && (
          <Badge text="HAZARDOUS" color="red" />
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-white shadow-sm rounded-lg border border-slate-200 overflow-hidden">
            <div className="p-6">
              <h2 className="text-lg font-semibold text-slate-800 mb-4">Description</h2>
              <p className="text-slate-600 whitespace-pre-wrap">{cargo.description}</p>
              
              <div className="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <span className="block text-sm font-medium text-text-muted">Voyage Number</span>
                  <span className="block text-slate-800">{cargo.voyageNumber}</span>
                </div>
                <div>
                  <span className="block text-sm font-medium text-text-muted">Type</span>
                  <span className="block text-slate-800">{cargo.type}</span>
                </div>
                <div>
                  <span className="block text-sm font-medium text-text-muted">Weight</span>
                  <span className="block text-slate-800">{cargo.weightKg.toLocaleString()} kg</span>
                </div>
                <div>
                  <span className="block text-sm font-medium text-text-muted">Declared Value</span>
                  <span className="block text-slate-800">${cargo.declaredValue.toLocaleString()}</span>
                </div>
                <div className="col-span-2">
                  <span className="block text-sm font-medium text-text-muted">Consignee</span>
                  <span className="block text-slate-800">{cargo.consigneeName}</span>
                </div>
              </div>
            </div>
            
            {cargo.hazardNotes && (
              <div className="bg-red-50 p-6 border-t border-red-100">
                <h3 className="text-sm font-semibold text-red-800 mb-2">Hazard Notes</h3>
                <p className="text-red-700 whitespace-pre-wrap text-sm">{cargo.hazardNotes}</p>
              </div>
            )}
            
            {canWrite && (
              <div className="bg-slate-50 p-4 border-t border-slate-200 flex justify-end">
                <Button variant="primary">Edit Cargo (Demo)</Button>
              </div>
            )}
          </div>
        </div>

        <div>
          <AiRecommendationCard 
            title="AI Risk Assessment"
            data={aiRiskAssessment}
            isLoading={isRiskAssessmentLoading}
          />
        </div>
      </div>
    </div>
  );
};
