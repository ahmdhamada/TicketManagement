using TicketManagement.Application.Auth.Dtos;

namespace TicketManagement.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default);
    Task<AuthResponse> RefreshAsync(string rawRefreshToken, string? ipAddress, CancellationToken ct = default);
    Task RevokeAsync(string rawRefreshToken, string? ipAddress, CancellationToken ct = default);
}
