using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Fleets;
using FleetMind.Api.IntegrationTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace FleetMind.Api.IntegrationTests.Controllers;

public class FleetsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public FleetsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateFleet_AsAdmin_Returns201Created_WithFleetDto()
    {
        // Arrange
        var client = await AuthenticatedClientHelper.GetAuthenticatedClientAsync(_factory, "admin@test.com", "AdminPass123!");
        var createDto = new CreateFleetDto
        {
            Name = "Integration Test Fleet",
            Description = "A fleet created during integration testing.",
            HomePortId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Status = FleetMind.Api.Common.Constants.FleetStatus.Active
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/fleets", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var fleet = await response.Content.ReadFromJsonAsync<FleetDto>();
        fleet.Should().NotBeNull();
        fleet!.Name.Should().Be("Integration Test Fleet");
        fleet.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetFleet_AfterCreation_Returns200OK_WithMatchingData()
    {
        // Arrange
        var client = await AuthenticatedClientHelper.GetAuthenticatedClientAsync(_factory, "admin@test.com", "AdminPass123!");
        
        // Create a fleet for this test
        var createDto = new CreateFleetDto { 
            Name = "Get Test Fleet", 
            Description = "Desc",
            HomePortId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Status = FleetMind.Api.Common.Constants.FleetStatus.Active 
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/fleets", createDto);
        var createdFleet = await createResponse.Content.ReadFromJsonAsync<FleetDto>();
        
        // Act
        var getResponse = await client.GetAsync($"/api/v1/fleets/{createdFleet!.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var fetchedFleet = await getResponse.Content.ReadFromJsonAsync<FleetDto>();
        fetchedFleet.Should().NotBeNull();
        fetchedFleet!.Id.Should().Be(createdFleet.Id);
        fetchedFleet.Name.Should().Be("Get Test Fleet");
    }

    [Fact]
    public async Task UpdateFleet_Returns200OK_AndSubsequentGetReflectsChange()
    {
        // Arrange
        var client = await AuthenticatedClientHelper.GetAuthenticatedClientAsync(_factory, "admin@test.com", "AdminPass123!");
        
        // Create a fleet for this test
        var createDto = new CreateFleetDto { 
            Name = "Put Test Fleet Original", 
            Description = "Desc",
            HomePortId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Status = FleetMind.Api.Common.Constants.FleetStatus.Active 
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/fleets", createDto);
        var createdFleet = await createResponse.Content.ReadFromJsonAsync<FleetDto>();

        var updateDto = new UpdateFleetDto
        {
            Name = "Put Test Fleet Updated",
            Description = "Updated Desc",
            HomePortId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Status = FleetMind.Api.Common.Constants.FleetStatus.Active 
        };

        // Act - Update
        var putResponse = await client.PutAsJsonAsync($"/api/v1/fleets/{createdFleet!.Id}", updateDto);
        
        // Assert - Update response
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Subsequent Get
        var getResponse = await client.GetAsync($"/api/v1/fleets/{createdFleet.Id}");
        
        // Assert - Subsequent Get response
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedFleet = await getResponse.Content.ReadFromJsonAsync<FleetDto>();
        fetchedFleet!.Name.Should().Be("Put Test Fleet Updated");
        fetchedFleet.Description.Should().Be("Updated Desc");
    }

    [Fact]
    public async Task DeleteFleet_Returns204NoContent_AndSubsequentGetReturns404()
    {
        // Arrange
        var client = await AuthenticatedClientHelper.GetAuthenticatedClientAsync(_factory, "admin@test.com", "AdminPass123!");
        
        // Create a fleet for this test
        var createDto = new CreateFleetDto { 
            Name = "Delete Test Fleet", 
            Description = "Desc",
            HomePortId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Status = FleetMind.Api.Common.Constants.FleetStatus.Active 
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/fleets", createDto);
        var createdFleet = await createResponse.Content.ReadFromJsonAsync<FleetDto>();

        // Act - Delete
        var deleteResponse = await client.DeleteAsync($"/api/v1/fleets/{createdFleet!.Id}");

        // Assert - Delete response
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act - Subsequent Get
        var getResponse = await client.GetAsync($"/api/v1/fleets/{createdFleet.Id}");
        
        // Assert - Subsequent Get response returns 404 (due to soft delete)
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateFleet_NoAuthHeader_Returns401Unauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(); // NO auth header
        var createDto = new CreateFleetDto { 
            Name = "Unauthorized Fleet",
            HomePortId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Status = FleetMind.Api.Common.Constants.FleetStatus.Active 
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/fleets", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateFleet_AsPlainUser_Returns403Forbidden()
    {
        // Arrange
        // Using "user@test.com" who is just a plain User, not Admin/FleetManager
        var client = await AuthenticatedClientHelper.GetAuthenticatedClientAsync(_factory, "user@test.com", "UserPass123!");
        var createDto = new CreateFleetDto { 
            Name = "Forbidden Fleet",
            HomePortId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Status = FleetMind.Api.Common.Constants.FleetStatus.Active 
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/fleets", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
