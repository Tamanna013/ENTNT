using System.Net;
using System.Net.Http.Json;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.DTOs.Fleets;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.DTOs.Maintenance;
using FleetMind.Api.IntegrationTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FleetMind.Api.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class RoleBasedAccessMatrixTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RoleBasedAccessMatrixTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public static IEnumerable<object[]> GetMatrixData()
    {
        var users = new[]
        {
            new { Role = AppRoles.Admin, Email = "admin@test.com", Password = "AdminPass123!" },
            new { Role = AppRoles.FleetManager, Email = "fleetmanager@test.com", Password = "FleetManagerPass123!" },
            new { Role = AppRoles.MaintenanceOfficer, Email = "maintenanceofficer@test.com", Password = "MaintenancePass123!" },
            new { Role = AppRoles.CrewManager, Email = "crewmanager@test.com", Password = "CrewPass123!" },
            new { Role = AppRoles.User, Email = "user@test.com", Password = "UserPass123!" }
        };

        var endpoints = new[]
        {
            new { Route = "/api/v1/fleets", Method = HttpMethod.Post, Payload = (object?)new CreateFleetDto { Name = "Matrix Fleet", HomePortId = Guid.Parse("00000000-0000-0000-0000-000000000001"), Status = FleetStatus.Active } },
            new { Route = "/api/v1/maintenance", Method = HttpMethod.Post, Payload = (object?)new CreateMaintenanceRecordDto { ShipId = Guid.Parse("00000000-0000-0000-0000-000000000003"), Type = MaintenanceType.Routine, Description = "Test", ScheduledDate = DateTime.UtcNow.AddDays(1), EstimatedCost = 100 } },
            new { Route = "/api/v1/audit-logs", Method = HttpMethod.Get, Payload = (object?)null },
            new { Route = "/api/v1/incidents", Method = HttpMethod.Post, Payload = (object?)new CreateIncidentDto { Title = "Matrix Incident", Description = "Test", Severity = IncidentSeverity.Low, OccurredAt = DateTime.UtcNow, ShipId = Guid.Parse("00000000-0000-0000-0000-000000000003") } }
        };

        foreach (var user in users)
        {
            foreach (var endpoint in endpoints)
            {
                HttpStatusCode expectedStatus;
                if (user.Role == AppRoles.Admin)
                {
                    expectedStatus = endpoint.Method == HttpMethod.Post ? HttpStatusCode.Created : HttpStatusCode.OK;
                }
                else if (endpoint.Route == "/api/v1/incidents")
                {
                    // Open-incident-reporting exception: ALL roles can create an incident
                    expectedStatus = HttpStatusCode.Created;
                }
                else if (user.Role == AppRoles.FleetManager && endpoint.Route == "/api/v1/fleets")
                {
                    expectedStatus = HttpStatusCode.Created;
                }
                else if (user.Role == AppRoles.MaintenanceOfficer && endpoint.Route == "/api/v1/maintenance")
                {
                    expectedStatus = HttpStatusCode.Created;
                }
                else
                {
                    expectedStatus = HttpStatusCode.Forbidden;
                }

                yield return new object[] { user.Email, user.Password, user.Role, endpoint.Method.ToString(), endpoint.Route, endpoint.Payload, expectedStatus };
            }
        }
    }

    [Theory]
    [MemberData(nameof(GetMatrixData))]
    public async Task RoleBasedAccessMatrix_Endpoint_ReturnsExpectedStatus(
        string email, 
        string password, 
        string role, 
        string methodStr, 
        string route, 
        object payload, 
        HttpStatusCode expectedStatus)
    {
        // Arrange
        var client = await AuthenticatedClientHelper.GetAuthenticatedClientAsync(_factory, email, password);
        var method = new HttpMethod(methodStr);

        // Act
        HttpResponseMessage response;
        if (method == HttpMethod.Post && payload is CreateFleetDto createFleetDto)
        {
            var uniquePayload = new CreateFleetDto
            {
                Name = createFleetDto.Name + " " + Guid.NewGuid().ToString(),
                HomePortId = createFleetDto.HomePortId,
                Status = createFleetDto.Status
            };
            response = await client.PostAsJsonAsync(route, uniquePayload);
        }
        else if (method == HttpMethod.Post)
        {
            response = await client.PostAsJsonAsync(route, payload);
        }
        else
        {
            var request = new HttpRequestMessage(method, route);
            if (payload != null)
            {
                request.Content = JsonContent.Create(payload);
            }
            response = await client.SendAsync(request);
        }

        // Assert
        if (expectedStatus == HttpStatusCode.Forbidden)
        {
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden, $"because user {email} with role {role} should not access {methodStr} {route}");
        }
        else
        {
            response.StatusCode.Should().Be(expectedStatus, $"because user {email} with role {role} should successfully execute {methodStr} {route}. Error (if any): {await response.Content.ReadAsStringAsync()}");
        }
    }
}
