import React, { useState } from 'react';
import { useFleetSummaryQuery, useAiInsightsQuery } from '../../hooks/useAnalytics';
import { TrailingWindowSelector } from '../../components/analytics/TrailingWindowSelector';
import { DataFreshnessIndicator } from '../../components/analytics/DataFreshnessIndicator';
import { AnalyticsKpiCard } from '../../components/analytics/AnalyticsKpiCard';
import { ShipUtilizationTrendChart } from '../../components/analytics/ShipUtilizationTrendChart';
import { VoyagePerformanceTrendChart } from '../../components/analytics/VoyagePerformanceTrendChart';
import { CrewComplianceTrendChart } from '../../components/analytics/CrewComplianceTrendChart';
import { MaintenanceCostVarianceChart } from '../../components/analytics/MaintenanceCostVarianceChart';
import { FinancialSummaryChart } from '../../components/analytics/FinancialSummaryChart';
import { AiSummaryCard } from '../../components/assistant/AiSummaryCard';
import { Anchor, Ship, Users, FileText, FileSpreadsheet } from 'lucide-react';
import { Button } from '../../components/ui/Button';
import { downloadAuthenticatedFile } from '../../lib/downloadFile';

export const AnalyticsPage: React.FC = () => {
  const [trailingMonths, setTrailingMonths] = useState<number>(12);
  const { data: fleetSummary, isLoading, error } = useFleetSummaryQuery();
  const { data: aiInsights, isLoading: isAiInsightsLoading } = useAiInsightsQuery(trailingMonths);

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 text-red-600 p-4 rounded-lg">
        Error loading analytics data. Please try again later.
      </div>
    );
  }

  const handleExportPdf = async () => {
    try {
      await downloadAuthenticatedFile(`/analytics/export-pdf?months=${trailingMonths}`, `analytics-report-${trailingMonths}mo.pdf`);
    } catch (err) {
      console.error('Failed to export PDF', err);
    }
  };

  const handleExportExcel = async () => {
    try {
      await downloadAuthenticatedFile(`/analytics/export-excel?months=${trailingMonths}`, `analytics-export-${trailingMonths}mo.xlsx`);
    } catch (err) {
      console.error('Failed to export Excel', err);
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      {/* Header Row */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Analytics Dashboard</h1>
          {fleetSummary && (
            <div className="mt-1">
              {/* Note: Once multiple data sources with potentially different freshness exist on this page, this indicator's exact meaning may need revisiting. */}
              <DataFreshnessIndicator generatedAt={fleetSummary.generatedAt} />
            </div>
          )}
        </div>
        
        <div className="flex items-center gap-4 flex-wrap">
          <TrailingWindowSelector 
            value={trailingMonths} 
            onChange={setTrailingMonths} 
          />
          <Button variant="secondary" onClick={handleExportPdf} className="flex items-center gap-2 text-sm px-3 py-1.5 h-auto">
            <FileText className="h-4 w-4" />
            <span>Export PDF</span>
          </Button>
          <Button variant="secondary" onClick={handleExportExcel} className="flex items-center gap-2 text-sm px-3 py-1.5 h-auto">
            <FileSpreadsheet className="h-4 w-4" />
            <span>Export Excel</span>
          </Button>
        </div>
      </div>

      {/* KPI Cards Row */}
      {fleetSummary && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <AnalyticsKpiCard
            title="Total Fleets"
            value={fleetSummary.totalFleets}
            icon={Anchor}
            color="indigo"
          />
          <AnalyticsKpiCard
            title="Total Ships"
            value={fleetSummary.totalShips}
            icon={Ship}
            color="emerald"
            description={`${fleetSummary.activeShips} Active`}
          />
          <AnalyticsKpiCard
            title="Total Crew"
            value={fleetSummary.totalCrew}
            icon={Users}
            color="blue"
            description={`${fleetSummary.assignedCrew} Assigned`}
          />
        </div>
      )}
      
      {/* AI Insights Row */}
      <div className="mb-6">
        <AiSummaryCard 
          title="AI Insights" 
          data={aiInsights} 
          isLoading={isAiInsightsLoading} 
        />
      </div>
      
      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ShipUtilizationTrendChart months={trailingMonths} />
        <VoyagePerformanceTrendChart months={trailingMonths} />
        <CrewComplianceTrendChart months={trailingMonths} />
        <MaintenanceCostVarianceChart months={trailingMonths} />
        <FinancialSummaryChart months={trailingMonths} />
      </div>
    </div>
  );
};
