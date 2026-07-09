import React from 'react';
import { useFinancialSummaryTrendQuery } from '../../hooks/useAnalytics';
import { ResponsiveContainer, BarChart, Bar, XAxis, YAxis, Tooltip, Legend, CartesianGrid } from 'recharts';

interface FinancialSummaryChartProps {
  months: number;
}

export const FinancialSummaryChart: React.FC<FinancialSummaryChartProps> = ({ months }) => {
  const { data: trendData, isLoading, error } = useFinancialSummaryTrendQuery(months);

  if (isLoading) {
    return <div className="h-64 flex items-center justify-center bg-gray-50 rounded-xl border border-gray-100">Loading chart...</div>;
  }

  if (error || !trendData) {
    return <div className="h-64 flex items-center justify-center bg-red-50 text-red-600 rounded-xl border border-red-100">Error loading data</div>;
  }

  // Format currency
  const formatCurrency = (value: number) => {
    if (value >= 1000000) return `$${(value / 1000000).toFixed(1)}M`;
    if (value >= 1000) return `$${(value / 1000).toFixed(0)}k`;
    return `$${value}`;
  };

  return (
    <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex flex-col h-full">
      <div className="mb-6">
        <h3 className="text-lg font-semibold text-gray-900">Monthly Operating Cost (Fuel + Maintenance)</h3>
      </div>
      
      <div className="flex-1 min-h-[250px]">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={trendData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E5E7EB" />
            <XAxis dataKey="month" stroke="#9CA3AF" fontSize={12} tickLine={false} axisLine={false} />
            <YAxis stroke="#9CA3AF" fontSize={12} tickLine={false} axisLine={false} tickFormatter={formatCurrency} />
            <Tooltip 
              contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
              labelStyle={{ color: '#4B5563', fontWeight: 'bold', marginBottom: '4px' }}
              formatter={(value: any, name: any) => {
                const formattedValue = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(Number(value));
                if (name === 'fuelCost') return [formattedValue, 'Fuel Cost'];
                if (name === 'maintenanceCost') return [formattedValue, 'Maintenance Cost'];
                return [formattedValue, name];
              }}
            />
            <Legend verticalAlign="top" height={36} iconType="circle" />
            <Bar dataKey="fuelCost" name="Fuel Cost" stackId="a" fill="#3B82F6" />
            <Bar dataKey="maintenanceCost" name="Maintenance Cost" stackId="a" fill="#F59E0B" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};
