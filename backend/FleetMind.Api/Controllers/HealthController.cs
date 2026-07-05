using Asp.Versioning;
using FleetMind.Api.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FleetMind.Api.Controllers;

/// <summary>
/// Health check endpoints for monitoring and load-balancer probes.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HealthController : ControllerBase
{
    private readonly DatabaseOptions _dbOptions;

    public HealthController(IOptions<DatabaseOptions> dbOptions)
    {
        _dbOptions = dbOptions.Value;
    }

    /// <summary>
    /// Returns the current health status of the API.
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "ok",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Verifies database connectivity using a raw ADO.NET connection.
    /// Returns 200 on success, 503 if the database is unreachable.
    /// </summary>
    [HttpGet("db")]
    public async Task<IActionResult> GetDatabaseHealth()
    {
        try
        {
            await using var connection = new SqlConnection(_dbOptions.DefaultConnection);
            await connection.OpenAsync();

            return Ok(new
            {
                status = "ok",
                database = "connected"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "error",
                database = "disconnected",
                detail = ex.Message
            });
        }
    }
}
