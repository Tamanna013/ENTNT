import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useShipUtilizationTrendQuery } from '../../hooks/useAnalytics';
import { ResponsiveContainer, LineChart, Line, XAxis, YAxis, Tooltip, CartesianGrid } from 'recharts';
import { Info } from 'lucide-react';

interface ShipUtilizationTrendChartProps {
  months: number;
}

export const ShipUtilizationTrendChart: React.FC<ShipUtilizationTrendChartProps> = ({ months }) => {
  const { data: trendData, isLoading, error } = useShipUtilizationTrendQuery(months);
  const navigate = useNavigate();

  const handleChartClick = (state: any) => {
    if (state && state.activePayload && state.activePayload.length > 0) {
      // APPROXIMATE DRILL-DOWN: Navigates to /maintenance?status=InProgress.
      // This is approximate for the same reason the chart's own data is approximate 
      // (Ship.Status has no historical log to trace month-exact active/inactive).
      navigate('/maintenance?status=InProgress');
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
      <div className="flex items-center justify-between mb-6">
        <h3 className="text-lg font-semibold text-gray-900">Ship Utilization Trend</h3>
        <div className="group relative flex items-center cursor-help text-amber-500">
          <Info className="w-5 h-5" />
          <span className="invisible group-hover:visible absolute right-0 top-full mt-2 w-64 p-2 bg-surface text-text-primary text-xs rounded shadow-lg z-10">
            Approximate - inferred from maintenance records, not a precise historical log.
          </span>
        </div>
      </div>
      
      <div className="flex-1 min-h-[250px] cursor-pointer">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={trendData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }} onClick={handleChartClick}>
            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E5E7EB" />
            <XAxis dataKey="month" stroke="#9CA3AF" fontSize={12} tickLine={false} axisLine={false} />
            <YAxis stroke="#9CA3AF" fontSize={12} tickLine={false} axisLine={false} domain={[0, 100]} tickFormatter={(value) => `${value}%`} />
            <Tooltip 
              contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
              formatter={(value: any) => [`${value}%`, 'Utilization']}
              labelStyle={{ color: '#4B5563', fontWeight: 'bold', marginBottom: '4px' }}
            />
            <Line type="monotone" dataKey="utilizationPercentage" stroke="#4F46E5" strokeWidth={3} dot={{ r: 4, fill: '#4F46E5', strokeWidth: 0 }} activeDot={{ r: 6 }} />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};
