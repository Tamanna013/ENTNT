import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useVoyagePerformanceTrendQuery } from '../../hooks/useAnalytics';
import { ResponsiveContainer, ComposedChart, Bar, Line, XAxis, YAxis, Tooltip, CartesianGrid } from 'recharts';

interface VoyagePerformanceTrendChartProps {
  months: number;
}

export const VoyagePerformanceTrendChart: React.FC<VoyagePerformanceTrendChartProps> = ({ months }) => {
  const { data: trendData, isLoading, error } = useVoyagePerformanceTrendQuery(months);
  const navigate = useNavigate();

  const handleChartClick = (state: any) => {
    if (state && state.activePayload && state.activePayload.length > 0) {
      const monthStr = state.activePayload[0].payload.month; // e.g., "2026-04"
      const [year, month] = monthStr.split('-');
      
      const firstDay = new Date(Date.UTC(Number(year), Number(month) - 1, 1));
      const lastDay = new Date(Date.UTC(Number(year), Number(month), 0, 23, 59, 59));
      
      navigate(`/voyages?departureFrom=${firstDay.toISOString()}&departureTo=${lastDay.toISOString()}`);
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
        <h3 className="text-lg font-semibold text-gray-900">Voyage Performance Trend</h3>
      </div>
      
      <div className="flex-1 min-h-[250px] cursor-pointer">
        <ResponsiveContainer width="100%" height="100%">
          <ComposedChart data={trendData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }} onClick={handleChartClick}>
            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E5E7EB" />
            <XAxis dataKey="month" stroke="#9CA3AF" fontSize={12} tickLine={false} axisLine={false} />
            <YAxis yAxisId="left" stroke="#9CA3AF" fontSize={12} tickLine={false} axisLine={false} orientation="left" />
            <YAxis yAxisId="right" stroke="#10B981" fontSize={12} tickLine={false} axisLine={false} orientation="right" domain={[0, 100]} tickFormatter={(value) => `${value}%`} />
            <Tooltip 
              contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
              labelStyle={{ color: '#4B5563', fontWeight: 'bold', marginBottom: '4px' }}
            />
            <Bar yAxisId="left" dataKey="completedVoyages" name="Completed Voyages" fill="#93C5FD" radius={[4, 4, 0, 0]} barSize={20} />
            <Line yAxisId="right" type="monotone" dataKey="onTimePercentage" name="On-Time Rate" stroke="#10B981" strokeWidth={3} dot={{ r: 4, fill: '#10B981', strokeWidth: 0 }} activeDot={{ r: 6 }} />
          </ComposedChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};
