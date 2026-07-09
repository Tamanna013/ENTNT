import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useMaintenanceCostTrendQuery } from '../../hooks/useAnalytics';
import { ResponsiveContainer, BarChart, Bar, Cell, XAxis, YAxis, Tooltip, CartesianGrid } from 'recharts';

interface MaintenanceCostVarianceChartProps {
  months: number;
}

export const MaintenanceCostVarianceChart: React.FC<MaintenanceCostVarianceChartProps> = ({ months }) => {
  const { data: trendData, isLoading, error } = useMaintenanceCostTrendQuery(months);
  const navigate = useNavigate();

  const handleChartClick = (state: any) => {
    if (state && state.activePayload && state.activePayload.length > 0) {
      // APPROXIMATE DRILL-DOWN: We navigate to /maintenance?status=Completed since cost variance is 
      // only meaningful for completed records. A precise date-range filter for the exact month is not 
      // supported by the current MaintenanceQuery shape (which only has dueBefore).
      navigate('/maintenance?status=Completed');
    }
  };

  if (isLoading) {
    return <div className="h-64 flex items-center justify-center bg-gray-50 rounded-xl border border-gray-100">Loading chart...</div>;
  }

  if (error || !trendData) {
    return <div className="h-64 flex items-center justify-center bg-red-50 text-red-600 rounded-xl border border-red-100">Error loading data</div>;
  }

  return (
    <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex flex-col h-full">
      <div className="mb-6">
        <h3 className="text-lg font-semibold text-gray-900">Maintenance Cost Variance (Actual vs Estimated)</h3>
      </div>
      
      <div className="flex-1 min-h-[250px] cursor-pointer">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={trendData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }} onClick={handleChartClick}>
            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E5E7EB" />
            <XAxis dataKey="month" stroke="#9CA3AF" fontSize={12} tickLine={false} axisLine={false} />
            <YAxis stroke="#9CA3AF" fontSize={12} tickLine={false} axisLine={false} tickFormatter={(value) => `${value}%`} />
            <Tooltip 
              contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
              labelStyle={{ color: '#4B5563', fontWeight: 'bold', marginBottom: '4px' }}
              formatter={(value: any, name: any) => {
                if (name === 'variancePercentage') {
                  return [`${value > 0 ? '+' : ''}${value}%`, 'Variance'];
                }
                return [value, name];
              }}
            />
            <Bar dataKey="variancePercentage">
              {trendData.map((entry, index) => (
                <Cell 
                  key={`cell-${index}`} 
                  // Red (rose-400) for over budget (> 0), Green (emerald-400) for under budget (<= 0)
                  fill={entry.variancePercentage > 0 ? '#FB7185' : '#34D399'} 
                />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};
