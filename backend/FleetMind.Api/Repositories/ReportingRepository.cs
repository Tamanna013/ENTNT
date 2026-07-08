using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Reporting;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories;

public class ReportingRepository : IReportingRepository
{
    private readonly FleetMindDbContext _context;

    public ReportingRepository(FleetMindDbContext context)
    {
        _context = context;
    }

    public async Task<List<FleetUtilizationReportRowDto>> GetFleetUtilizationReportAsync()
    {
        return await _context.Set<FleetUtilizationReportRowDto>()
            .FromSqlInterpolated($"EXEC sp_GetFleetUtilizationReport")
            .ToListAsync();
    }

    public async Task<List<VoyageManifestReportRowDto>> GetVoyageManifestReportAsync(Guid voyageId)
    {
        return await _context.Set<VoyageManifestReportRowDto>()
            .FromSqlInterpolated($"EXEC sp_GetVoyageManifestReport @VoyageId = {voyageId}")
            .ToListAsync();
    }

    public async Task<List<FuelEfficiencyReportRowDto>> GetFuelEfficiencyReportAsync(int trailingDays)
    {
        return await _context.Set<FuelEfficiencyReportRowDto>()
            .FromSqlInterpolated($"EXEC sp_GetFuelEfficiencyReport @TrailingDays = {trailingDays}")
            .ToListAsync();
    }
}
