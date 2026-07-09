export const AppRoles = {
  Admin: 'Admin',
  FleetManager: 'FleetManager',
  CrewManager: 'CrewManager',
  MaintenanceOfficer: 'MaintenanceOfficer',
  User: 'User'
} as const;

export type AppRole = typeof AppRoles[keyof typeof AppRoles];

export const FLEET_STATUSES = ['Active', 'Inactive', 'UnderReview'] as const;

export const SHIP_STATUSES = ['Active', 'InMaintenance', 'Decommissioned', 'Docked'] as const;
export const SHIP_TYPES = ['ContainerShip', 'BulkCarrier', 'Tanker', 'RoRo', 'GeneralCargo'] as const;

export const DOCUMENT_CATEGORIES = [
  'Regulatory',
  'Insurance',
  'Contract',
  'Certificate',
  'Policy',
  'Other'
] as const;

export const CREW_RANKS = ["Captain", "ChiefOfficer", "ChiefEngineer", "SecondOfficer", "Deckhand", "Cook", "Cadet"] as const;
export const CREW_STATUSES = ["Active", "OnLeave", "Unassigned", "Terminated"] as const;

export const VOYAGE_STATUSES = [
  'Scheduled',
  'InTransit',
  'Completed',
  'Delayed',
  'Cancelled'
] as const;

export const MAINTENANCE_TYPES = [
  'Routine',
  'Emergency',
  'Scheduled',
  'Regulatory'
] as const;

export const MAINTENANCE_STATUSES = ['Scheduled', 'InProgress', 'Completed', 'Overdue', 'Cancelled'] as const;

export const FUEL_TYPES = [
  'HeavyFuelOil',
  'MarineDieselOil',
  'LNG',
  'LowSulfurFuelOil',
] as const;

export const INCIDENT_SEVERITIES = [
  'Low',
  'Medium',
  'High',
  'Critical'
] as const;

export const INCIDENT_STATUSES = [
  'Reported',
  'UnderInvestigation',
  'Resolved',
  'Closed'
] as const;

export const NOTIFICATION_TYPES = [
  'MaintenanceOverdue',
  'VoyageDelayed',
  'CertificationExpiring',
  'FuelAnomaly',
  'General',
] as const;

export const CARGO_TYPES = [
  'GeneralGoods',
  'Hazardous',
  'Perishable',
  'Bulk',
  'Liquid',
  'Vehicles',
] as const;

export const CARGO_STATUSES = [
  'Pending',
  'Loaded',
  'InTransit',
  'Delivered',
  'Damaged',
  'Lost',
] as const;

export const CONTAINER_TYPES = [
  'Dry20ft',
  'Dry40ft',
  'Refrigerated',
  'OpenTop',
  'Tank'
] as const;

export const CONTAINER_STATUSES = [
  'Empty',
  'Loaded',
  'InTransit',
  'AtPort',
  'Delivered'
] as const;
