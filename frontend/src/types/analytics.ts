export interface FleetSummaryAnalytics {
  totalFleets: number;
  totalShips: number;
  activeShips: number;
  totalCrew: number;
  assignedCrew: number;
  generatedAt: string;
}

export interface ShipUtilizationTrendPoint {
  month: string;
  totalShips: number;
  activeShips: number;
  utilizationPercentage: number;
}

export interface VoyagePerformanceTrendPoint {
  month: string;
  completedVoyages: number;
  onTimeVoyages: number;
  onTimePercentage: number;
}

export interface CrewComplianceTrendPoint {
  month: string;
  totalActiveCertifications: number;
  expiredCount: number;
  complianceRate: number;
}

export interface MaintenanceCostTrendPoint {
  month: string;
  totalEstimatedCost: number;
  totalActualCost: number;
  variancePercentage: number;
}

export interface FinancialSummaryTrendPoint {
  month: string;
  fuelCost: number;
  maintenanceCost: number;
  totalOperatingCost: number;
}


