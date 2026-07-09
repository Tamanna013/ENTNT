export interface FleetUtilizationReportRow {
  fleetId: string;
  fleetName: string;
  totalShips: number;
  activeShips: number;
  shipsInMaintenance: number;
  totalCrewAssigned: number;
  totalVoyagesLast90Days: number;
  totalCargoWeightLast90Days: number;
}

export interface VoyageManifestReportRow {
  voyageId: string;
  cargoId: string;
  description: string;
  type: string;
  status: string;
  weightKg: number;
  declaredValue: number;
  consigneeName: string;
  containerId: string | null;
  containerNumber: string | null;
}

export interface FuelEfficiencyReportRow {
  shipId: string;
  shipName: string;
  totalQuantityLiters: number;
  totalCost: number;
  averageCostPerLiter: number;
  logCount: number;
}
