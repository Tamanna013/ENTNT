import React, { useState } from 'react';
import { useAiUsageReportQuery } from '../../hooks/useAiUsage';
import { AiUsageTable } from '../../components/ai-usage/AiUsageTable';
import { DateRangePicker } from "../../components/ui/DateRangePicker";

export const AiUsagePage: React.FC = () => {
  const [dateRange, setDateRange] = useState<{ start?: Date; end?: Date }>({});
  
  const dateFrom = dateRange.start?.toISOString();
  const dateTo = dateRange.end?.toISOString();

  const { data, isLoading, error } = useAiUsageReportQuery(dateFrom, dateTo);

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-900">AI Usage Report</h1>
        <div className="w-72">
          <DateRangePicker 
            onChange={(start, end) => setDateRange({ start, end })}
          />
        </div>
      </div>

      {isLoading ? (
        <div className="flex justify-center p-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
        </div>
      ) : error ? (
        <div className="bg-red-50 text-red-600 p-4 rounded-md">
          Failed to load AI usage report. Please try again.
        </div>
      ) : (
        <AiUsageTable data={data || []} />
      )}
    </div>
  );
};

export default AiUsagePage;
