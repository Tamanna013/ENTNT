using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Reporting;

namespace FleetMind.Api.Services;

public interface IReportingService
{
    Task<List<FleetUtilizationReportRowDto>> GetFleetUtilizationReportAsync();
    Task<List<VoyageManifestReportRowDto>> GetVoyageManifestReportAsync(Guid voyageId);
    Task<List<FuelEfficiencyReportRowDto>> GetFuelEfficiencyReportAsync(int trailingDays);
}
