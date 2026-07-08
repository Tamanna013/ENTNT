using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Reporting;
using FleetMind.Api.Services;

namespace FleetMind.Api.IntegrationTests.TestHelpers;

public class MockReportingService : IReportingService
{
    public Task<List<FleetUtilizationReportRowDto>> GetFleetUtilizationReportAsync()
    {
        return Task.FromResult(new List<FleetUtilizationReportRowDto>());
    }

    public Task<List<FuelEfficiencyReportRowDto>> GetFuelEfficiencyReportAsync(int trailingDays)
    {
        return Task.FromResult(new List<FuelEfficiencyReportRowDto>
        {
            new FuelEfficiencyReportRowDto
            {
                ShipId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                ShipName = "Test Ship",
                LogCount = 5,
                AverageCostPerLiter = 1.0m,
                TotalQuantityLiters = 5000,
                TotalCost = 5000
            }
        });
    }

    public Task<List<VoyageManifestReportRowDto>> GetVoyageManifestReportAsync(Guid voyageId)
    {
        return Task.FromResult(new List<VoyageManifestReportRowDto>());
    }
}
