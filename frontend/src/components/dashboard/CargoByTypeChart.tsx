import React from 'react';
import { ResponsiveContainer, PieChart, Pie, Cell, Tooltip, Legend } from 'recharts';
import { useCargoQuery } from '../../hooks/useCargo';

const COLORS: Record<string, string> = {
  'Hazardous': '#ef4444', // Red for hazardous
  'GeneralGoods': '#3b82f6', // Blue
  'Perishable': '#10b981', // Emerald
  'Bulk': '#f59e0b', // Amber
  'Liquid': '#8b5cf6', // Violet
  'Vehicles': '#64748b', // Slate
};

export const CargoByTypeChart: React.FC = () => {
  const { data, isLoading } = useCargoQuery({ pageNumber: 1, pageSize: 100 });

  if (isLoading) {
    return <div className="h-80 flex items-center justify-center text-text-muted bg-surface rounded-lg border border-border">Loading chart...</div>;
  }

  const typeCounts = (data?.items || []).reduce((acc, cargo) => {
    acc[cargo.type] = (acc[cargo.type] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  const chartData = Object.entries(typeCounts).map(([name, value]) => ({
    name,
    value
  }));

  return (
    <div className="bg-surface rounded-lg p-6 border border-border h-80 flex flex-col">
      <h2 className="text-lg font-semibold text-text-primary mb-4">Cargo by Type</h2>
      <div className="flex-1 w-full min-h-0">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie
              data={chartData}
              cx="50%"
              cy="50%"
              innerRadius={60}
              outerRadius={80}
              paddingAngle={5}
              dataKey="value"
            >
              {chartData.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={COLORS[entry.name] || '#94a3b8'} />
              ))}
            </Pie>
            <Tooltip 
              contentStyle={{ backgroundColor: '#1e293b', borderColor: '#475569', color: '#f8fafc' }}
              itemStyle={{ color: '#e2e8f0' }}
            />
            <Legend verticalAlign="bottom" height={36} wrapperStyle={{ fontSize: '12px', color: '#cbd5e1' }} />
          </PieChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};
