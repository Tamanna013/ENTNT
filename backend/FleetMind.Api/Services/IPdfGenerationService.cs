using System;
using System.Threading.Tasks;

namespace FleetMind.Api.Services;

public interface IPdfGenerationService
{
    Task<byte[]> GenerateVoyageManifestAsync(Guid voyageId);
    Task<byte[]> GenerateIncidentReportAsync(Guid incidentId);
    Task<byte[]> GenerateAnalyticsReportAsync(int months);
}
