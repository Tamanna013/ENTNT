import React from 'react';
import { ResponsiveContainer, BarChart, Bar, XAxis, YAxis, Tooltip, CartesianGrid } from 'recharts';
import { useFleetUtilizationReportQuery } from '../../hooks/useReporting';

export const ShipsPerFleetChart: React.FC = () => {
  const { data, isLoading } = useFleetUtilizationReportQuery();

  if (isLoading) {
    return <div className="h-64 flex items-center justify-center text-text-muted">Loading chart...</div>;
  }

  const chartData = data?.map(fleet => ({
    name: fleet.fleetName,
    ships: fleet.totalShips
  })) || [];

  return (
    <div className="bg-surface rounded-lg p-6 border border-border h-80 flex flex-col">
      <h2 className="text-lg font-semibold text-text-primary mb-4">Ships per Fleet</h2>
      <div className="flex-1 w-full min-h-0">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={chartData} margin={{ top: 10, right: 10, left: -20, bottom: 20 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#334155" vertical={false} />
            <XAxis 
              dataKey="name" 
              tick={{ fill: '#94a3b8', fontSize: 12 }} 
              axisLine={{ stroke: '#475569' }} 
              tickLine={{ stroke: '#475569' }} 
            />
            <YAxis 
              allowDecimals={false} 
              tick={{ fill: '#94a3b8', fontSize: 12 }} 
              axisLine={{ stroke: '#475569' }} 
              tickLine={{ stroke: '#475569' }}
            />
            <Tooltip 
              cursor={{ fill: '#334155', opacity: 0.4 }}
              contentStyle={{ backgroundColor: '#1e293b', borderColor: '#475569', color: '#f8fafc' }}
              itemStyle={{ color: '#818cf8' }}
            />
            <Bar dataKey="ships" fill="#6366f1" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};
