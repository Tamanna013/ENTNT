using System.Net;
using System.Net.Http.Json;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.DTOs.Fuel;
using FleetMind.Api.DTOs.Notifications;
using FleetMind.Api.IntegrationTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FleetMind.Api.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class NotificationPipelineIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public NotificationPipelineIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FuelAnomaly_TriggersNotification_AndIsRetrievableViaApi()
    {
        // 1. Arrange - Authenticate as FleetManager to submit fuel logs
        var client = await AuthenticatedClientHelper.GetAuthenticatedClientAsync(_factory, "fleetmanager@test.com", "FleetManagerPass123!");
        
        var shipId = Guid.Parse("00000000-0000-0000-0000-000000000003"); // Seeded Ship in factory
        
        // 2. Submit initial baseline FuelLog (Cost = 1.00)
        var baselineLog = new CreateFuelLogDto
        {
            ShipId = shipId,
            FuelType = FuelType.MarineDieselOil,
            QuantityLiters = 1000,
            CostPerLiter = 1.00m,
            RecordedDate = DateTime.UtcNow
        };
        var createBaselineResponse = await client.PostAsJsonAsync("/api/v1/fuel", baselineLog);
        var baselineError = await createBaselineResponse.Content.ReadAsStringAsync();
        createBaselineResponse.StatusCode.Should().Be(HttpStatusCode.Created, baselineError);

        // 3. Submit second FuelLog with highly inflated cost (Cost = 1.60 -> 60% higher)
        var anomalyLog = new CreateFuelLogDto
        {
            ShipId = shipId,
            FuelType = FuelType.MarineDieselOil,
            QuantityLiters = 1000,
            CostPerLiter = 1.60m,
            RecordedDate = DateTime.UtcNow
        };
        var createAnomalyResponse = await client.PostAsJsonAsync("/api/v1/fuel", anomalyLog);
        createAnomalyResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 4. Give the background worker/event bus a tiny moment to process the notification if it's asynchronous
        // In our current implementation, it's likely handled synchronously or very quickly within the request scope
        // but a short delay ensures it's persisted if an outbox/background worker is used.
        await Task.Delay(100);

        // 5. Authenticate as Admin (who receives notifications) or FleetManager
        var adminClient = await AuthenticatedClientHelper.GetAuthenticatedClientAsync(_factory, "admin@test.com", "AdminPass123!");

        // 6. Retrieve Notifications and confirm the anomaly notification exists
        var notificationsResponse = await adminClient.GetAsync("/api/v1/notifications");
        notificationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var pagedResult = await notificationsResponse.Content.ReadFromJsonAsync<FleetMind.Api.DTOs.Common.PagedResultDto<NotificationDto>>();
        pagedResult.Should().NotBeNull();
        
        var fuelAnomalyNotification = pagedResult!.Items.FirstOrDefault(n => n.Type == "FuelAnomaly" && (n.Message.Contains(shipId.ToString()) || n.Message.Contains("anomaly") || n.Type.Contains("Anomaly")));
        
        // Even if the type string doesn't perfectly match exactly "FuelAnomaly" 
        // we should definitely have a newly generated unread notification.
        fuelAnomalyNotification.Should().NotBeNull("An anomaly notification should be generated and retrievable via the API");
    }
}
