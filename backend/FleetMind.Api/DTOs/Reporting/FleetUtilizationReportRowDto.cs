using System;

namespace FleetMind.Api.DTOs.Reporting;

public class FleetUtilizationReportRowDto
{
    public Guid FleetId { get; set; }
    public string FleetName { get; set; } = string.Empty;
    public int TotalShips { get; set; }
    public int ActiveShips { get; set; }
    public int ShipsInMaintenance { get; set; }
    public int TotalCrewAssigned { get; set; }
    public int TotalVoyagesLast90Days { get; set; }
    public decimal TotalCargoWeightLast90Days { get; set; }
}
