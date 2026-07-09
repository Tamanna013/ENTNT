import React from 'react';
import { useCrewComplianceTrendQuery } from '../../hooks/useAnalytics';
import { ResponsiveContainer, LineChart, Line, XAxis, YAxis, Tooltip, CartesianGrid } from 'recharts';

interface CrewComplianceTrendChartProps {
  months: number;
}

export const CrewComplianceTrendChart: React.FC<CrewComplianceTrendChartProps> = ({ months }) => {
  const { data: trendData, isLoading, error } = useCrewComplianceTrendQuery(months);

  if (isLoading) {
    return <div className="h-64 flex items-center justify-center bg-gray-50 rounded-xl border border-gray-100">Loading chart...</div>;
  }

  if (error || !trendData) {
    return <div className="h-64 flex items-center justify-center bg-red-50 text-red-600 rounded-xl border border-red-100">Error loading data</div>;
  }

  return (
    <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex flex-col h-full">
      <div className="mb-6">
        <h3 className="text-lg font-semibold text-gray-900">Crew Certification Compliance Trend</h3>
      </div>
      
      <div className="flex-1 min-h-[250px]">
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={trendData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E5E7EB" />
            <XAxis dataKey="month" stroke="#9CA3AF" fontSize={12} tickLine={false} axisLine={false} />
            <YAxis stroke="#9CA3AF" fontSize={12} tickLine={false} axisLine={false} domain={[0, 100]} tickFormatter={(value) => `${value}%`} />
            <Tooltip 
              contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
              formatter={(value: any) => [`${value}%`, 'Compliance Rate']}
              labelStyle={{ color: '#4B5563', fontWeight: 'bold', marginBottom: '4px' }}
            />
            <Line type="monotone" dataKey="complianceRate" stroke="#F59E0B" strokeWidth={3} dot={{ r: 4, fill: '#F59E0B', strokeWidth: 0 }} activeDot={{ r: 6 }} />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};
