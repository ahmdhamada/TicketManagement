using System.ComponentModel.DataAnnotations;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Auth.Dtos;

public record RegisterRequest(
    [property: Required, StringLength(200, MinimumLength = 2)] string FullName,
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(8)] string Password,
    UserRole Role = UserRole.Customer);

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);

public record RefreshTokenRequest([property: Required] string RefreshToken);

public record RevokeTokenRequest([property: Required] string RefreshToken);

public record UserDto(int Id, string FullName, string Email, UserRole Role, bool IsActive, DateTime CreatedAtUtc);

public record AuthResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, UserDto User);
