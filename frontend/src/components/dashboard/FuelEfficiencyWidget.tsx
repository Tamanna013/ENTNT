import React, { useState } from 'react';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';
import { useFuelEfficiencyReportQuery } from '../../hooks/useReporting';

export const FuelEfficiencyWidget: React.FC = () => {
  const [trailingDays, setTrailingDays] = useState(90);
  const { data, isLoading } = useFuelEfficiencyReportQuery(trailingDays);

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 0,
    }).format(value);
  };

  const chartData = data?.map(row => ({
    name: row.shipName,
    TotalCost: row.totalCost,
    Quantity: row.totalQuantityLiters,
  })) || [];

  // Calculate weighted average client-side
  let fleetAverage = 0;
  if (data && data.length > 0) {
    const totalCostSum = data.reduce((sum, row) => sum + row.totalCost, 0);
    const totalQtySum = data.reduce((sum, row) => sum + row.totalQuantityLiters, 0);
    if (totalQtySum > 0) {
      fleetAverage = totalCostSum / totalQtySum;
    }
  }

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <div className="flex justify-between items-center mb-4">
        <h3 className="text-lg font-medium text-gray-900">Fleet Fuel Efficiency</h3>
        <select
          className="block pl-3 pr-10 py-1 text-sm border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 rounded-md shadow-sm"
          value={trailingDays}
          onChange={(e) => setTrailingDays(Number(e.target.value))}
        >
          <option value={30}>Last 30 Days</option>
          <option value={90}>Last 90 Days</option>
          <option value={180}>Last 180 Days</option>
        </select>
      </div>

      {isLoading ? (
        <div className="h-64 flex items-center justify-center text-gray-500">
          Loading report...
        </div>
      ) : data?.length === 0 ? (
        <div className="h-64 flex items-center justify-center text-gray-500">
          No fuel data available.
        </div>
      ) : (
        <>
          <div className="h-64 mb-4">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={chartData} margin={{ top: 10, right: 10, left: 20, bottom: 20 }}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis 
                  dataKey="name" 
                  angle={-45}
                  textAnchor="end"
                  height={60}
                  tick={{ fontSize: 12 }}
                />
                <YAxis 
                  tickFormatter={formatCurrency}
                  tick={{ fontSize: 12 }}
                />
                <Tooltip 
                  formatter={(value: any) => formatCurrency(value)}
                />
                <Legend verticalAlign="top" />
                <Bar dataKey="TotalCost" name="Total Fuel Cost" fill="#3b82f6" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
          
          <div className="bg-blue-50 p-3 rounded-md">
            <p className="text-sm text-blue-800 text-center">
              <strong>Fleet Average Cost:</strong> {new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 4 }).format(fleetAverage)} per liter
            </p>
          </div>
        </>
      )}
    </div>
  );
};
