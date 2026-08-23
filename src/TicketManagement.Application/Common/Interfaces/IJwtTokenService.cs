using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Common.Interfaces;

public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenService
{
    AccessTokenResult GenerateAccessToken(User user);

    /// <summary>Generates a cryptographically random opaque refresh token (the raw value returned to the client).</summary>
    string GenerateRefreshTokenValue();

    /// <summary>Hashes a raw refresh token value for storage/comparison, so raw tokens never sit in the database.</summary>
    string HashRefreshToken(string rawToken);
}
