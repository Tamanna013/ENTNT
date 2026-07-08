using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FleetMind.Api.IntegrationTests.TestHelpers;

public static class AuthenticatedClientHelper
{
    public static async Task<HttpClient> GetAuthenticatedClientAsync(
        WebApplicationFactory<Program> factory, 
        string email, 
        string password)
    {
        var client = factory.CreateClient();

        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new System.Exception($"Login failed with status {response.StatusCode}: {error}");
        }
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        return client;
    }
}
