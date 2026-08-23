using System.Net.Http.Headers;
using System.Net.Http.Json;
using TicketManagement.Application.Auth.Dtos;

namespace TicketManagement.IntegrationTests;

public static class TestClientExtensions
{
    public static async Task<HttpClient> AsUserAsync(this HttpClient client, string email, string password = "Passw0rd!")
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return client;
    }
}
