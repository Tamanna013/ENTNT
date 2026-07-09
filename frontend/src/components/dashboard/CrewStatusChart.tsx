import React, { useMemo } from 'react';
import { ResponsiveContainer, PieChart, Pie, Cell, Legend, Tooltip } from 'recharts';
import { useCrewQuery } from '../../hooks/useCrew';

const STATUS_COLORS: Record<string, string> = {
  Active: '#10b981',      // Green
  Unassigned: '#64748b',  // Gray
  OnLeave: '#f59e0b',     // Yellow
  Terminated: '#f43f5e'   // Red
};

export const CrewStatusChart: React.FC = () => {
  const { data, isLoading } = useCrewQuery({ pageNumber: 1, pageSize: 100 });

  const chartData = useMemo(() => {
    if (!data?.items) return [];

    const counts: Record<string, number> = {};
    data.items.forEach(crew => {
      counts[crew.status] = (counts[crew.status] || 0) + 1;
    });

    return Object.entries(counts).map(([name, value]) => ({
      name,
      value
    }));
  }, [data]);

  if (isLoading) {
    return <div className="h-64 flex items-center justify-center text-text-muted">Loading chart...</div>;
  }

  return (
    <div className="bg-surface rounded-lg p-6 border border-border h-80 flex flex-col">
      <h2 className="text-lg font-semibold text-text-primary mb-4">Crew Status</h2>
      <div className="flex-1 w-full min-h-0">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie
              data={chartData}
              cx="50%"
              cy="50%"
              innerRadius={60}
              outerRadius={80}
              paddingAngle={2}
              dataKey="value"
            >
              {chartData.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={STATUS_COLORS[entry.name] || '#94a3b8'} />
              ))}
            </Pie>
            <Tooltip 
              contentStyle={{ backgroundColor: '#1e293b', borderColor: '#475569', color: '#f8fafc' }}
              itemStyle={{ color: '#f8fafc' }}
            />
            <Legend 
              verticalAlign="bottom" 
              height={36} 
              wrapperStyle={{ fontSize: '12px', color: '#cbd5e1' }}
            />
          </PieChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};
