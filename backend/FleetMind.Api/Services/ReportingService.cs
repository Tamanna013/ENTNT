using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Reporting;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Services;

public class ReportingService : IReportingService
{
    private readonly IReportingRepository _repository;

    public ReportingService(IReportingRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<FleetUtilizationReportRowDto>> GetFleetUtilizationReportAsync()
    {
        return await _repository.GetFleetUtilizationReportAsync();
    }

    public async Task<List<VoyageManifestReportRowDto>> GetVoyageManifestReportAsync(Guid voyageId)
    {
        return await _repository.GetVoyageManifestReportAsync(voyageId);
    }

    public async Task<List<FuelEfficiencyReportRowDto>> GetFuelEfficiencyReportAsync(int trailingDays)
    {
        return await _repository.GetFuelEfficiencyReportAsync(trailingDays);
    }
}
