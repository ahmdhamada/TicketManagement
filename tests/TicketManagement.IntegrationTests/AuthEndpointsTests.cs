using System.Net;
using System.Net.Http.Json;
using TicketManagement.Application.Auth.Dtos;

namespace TicketManagement.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithSeededAdminCredentials_ReturnsTokens()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@invento.sa", "Passw0rd!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.User.Role.Should().Be(TicketManagement.Domain.Enums.UserRole.Admin);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@invento.sa", "WrongPassword1"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_RotatesToken_AndOldTokenCannotBeReused()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("customer1@invento.sa", "Passw0rd!"));
        var loginBody = await login.Content.ReadFromJsonAsync<AuthResponse>();

        var firstRefresh = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(loginBody!.RefreshToken));
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);

        // Reusing the now-rotated-away token must fail.
        var reuse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(loginBody.RefreshToken));
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Another Admin", "admin@invento.sa", "Passw0rd!"));
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
