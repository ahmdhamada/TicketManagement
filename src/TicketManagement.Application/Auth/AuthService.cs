using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Auth.Dtos;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Application.Auth;

public class AuthService : IAuthService
{
    private const int RefreshTokenLifetimeDays = 7;

    private readonly IApplicationDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _clock;

    public AuthService(
        IApplicationDbContext db,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IDateTimeProvider clock)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
        if (exists)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return await IssueTokensAsync(user, ipAddress: null, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return await IssueTokensAsync(user, ipAddress, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string rawRefreshToken, string? ipAddress, CancellationToken ct = default)
    {
        var tokenHash = _jwtTokenService.HashRefreshToken(rawRefreshToken);

        var existingToken = await _db.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

        if (existingToken is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (!existingToken.IsActive)
        {
            // Reuse of a revoked/expired token is a strong signal of token theft:
            // revoke every other active token for this user as a precaution.
            if (existingToken.IsRevoked)
            {
                await RevokeAllActiveTokensAsync(existingToken.UserId, ipAddress, ct);
            }

            throw new UnauthorizedAccessException("Refresh token is no longer valid.");
        }

        var user = existingToken.User;
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("This account has been deactivated.");
        }

        // Rotate: issue a new refresh token and revoke the old one, pointing to the new one.
        var newRawRefreshToken = _jwtTokenService.GenerateRefreshTokenValue();
        var newTokenHash = _jwtTokenService.HashRefreshToken(newRawRefreshToken);

        existingToken.RevokedAtUtc = _clock.UtcNow;
        existingToken.RevokedByIp = ipAddress;
        existingToken.ReplacedByTokenHash = newTokenHash;

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newTokenHash,
            ExpiresAtUtc = _clock.UtcNow.AddDays(RefreshTokenLifetimeDays),
            CreatedByIp = ipAddress
        });

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        await _db.SaveChangesAsync(ct);

        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            newRawRefreshToken,
            ToDto(user));
    }

    public async Task RevokeAsync(string rawRefreshToken, string? ipAddress, CancellationToken ct = default)
    {
        var tokenHash = _jwtTokenService.HashRefreshToken(rawRefreshToken);
        var token = await _db.RefreshTokens.SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

        if (token is null || !token.IsActive)
        {
            throw new NotFoundException(nameof(RefreshToken), "provided token");
        }

        token.RevokedAtUtc = _clock.UtcNow;
        token.RevokedByIp = ipAddress;
        await _db.SaveChangesAsync(ct);
    }

    private async Task RevokeAllActiveTokensAsync(int userId, string? ipAddress, CancellationToken ct)
    {
        var activeTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null)
            .ToListAsync(ct);

        foreach (var t in activeTokens)
        {
            t.RevokedAtUtc = _clock.UtcNow;
            t.RevokedByIp = ipAddress;
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, string? ipAddress, CancellationToken ct)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var rawRefreshToken = _jwtTokenService.GenerateRefreshTokenValue();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _jwtTokenService.HashRefreshToken(rawRefreshToken),
            ExpiresAtUtc = _clock.UtcNow.AddDays(RefreshTokenLifetimeDays),
            CreatedByIp = ipAddress
        });

        await _db.SaveChangesAsync(ct);

        return new AuthResponse(accessToken.Token, accessToken.ExpiresAtUtc, rawRefreshToken, ToDto(user));
    }

    private static UserDto ToDto(User user) =>
        new(user.Id, user.FullName, user.Email, user.Role, user.IsActive, user.CreatedAtUtc);
}
