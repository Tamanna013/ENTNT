using System.Net;
using System.Net.Http.Json;
using FleetMind.Api.DTOs.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Text.Json;

namespace FleetMind.Api.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthLifecycle_EndToEnd_WorksAsExpected()
    {
        // 1. Setup client
        var client = _factory.CreateClient();

        // 2. Register new user
        var registerDto = new RegisterDto
        {
            Email = "newuser@test.com",
            Password = "StrongPassword123!",
            FirstName = "New",
            LastName = "User"
        };
        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        registerResult.Should().NotBeNull();
        registerResult!.AccessToken.Should().NotBeNullOrEmpty();

        // 3. Register duplicate email -> 409
        var duplicateRegisterResponse = await client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        duplicateRegisterResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // 4. Login -> 200 and sets refresh cookie
        var loginDto = new LoginDto
        {
            Email = "newuser@test.com",
            Password = "StrongPassword123!"
        };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        loginResponse.Headers.TryGetValues("Set-Cookie", out var setCookieValues).Should().BeTrue("Login must set the refresh cookie");
        var refreshTokenCookie = setCookieValues!.FirstOrDefault(c => c.StartsWith("fleetmind_refresh_token="));
        refreshTokenCookie.Should().NotBeNull();
        
        // Extract the exact cookie value
        var rawCookieValue = refreshTokenCookie!.Split(';')[0].Split('=')[1];

        // 5. Refresh -> 200 and NEW access token
        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        refreshRequest.Headers.Add("Cookie", $"fleetmind_refresh_token={rawCookieValue}");
        var refreshResponse = await client.SendAsync(refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        refreshResult!.AccessToken.Should().NotBeNullOrEmpty();
        refreshResult.AccessToken.Should().NotBe(loginResult!.AccessToken, "Refresh should issue a new access token");

        // Extract the new cookie value
        refreshResponse.Headers.TryGetValues("Set-Cookie", out var newSetCookieValues).Should().BeTrue("Refresh must set a NEW refresh cookie");
        var newRefreshTokenCookie = newSetCookieValues!.FirstOrDefault(c => c.StartsWith("fleetmind_refresh_token="));
        var newRawCookieValue = newRefreshTokenCookie!.Split(';')[0].Split('=')[1];

        // 6. Refresh AGAIN with the OLD, rotated-away cookie -> 401
        var replayRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        replayRequest.Headers.Add("Cookie", $"fleetmind_refresh_token={rawCookieValue}");
        var replayResponse = await client.SendAsync(replayRequest);
        replayResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "Reusing a rotated refresh token should be rejected");

        // 7. Logout -> 204
        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        logoutRequest.Headers.Add("Cookie", $"fleetmind_refresh_token={newRawCookieValue}");
        logoutRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", refreshResult.AccessToken);
        var logoutResponse = await client.SendAsync(logoutRequest);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 8. Refresh after logout -> 401
        var postLogoutRefreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        postLogoutRefreshRequest.Headers.Add("Cookie", $"fleetmind_refresh_token={newRawCookieValue}");
        var postLogoutRefreshResponse = await client.SendAsync(postLogoutRefreshRequest);
        postLogoutRefreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "Refresh token should be invalid after logout");
    }
}
