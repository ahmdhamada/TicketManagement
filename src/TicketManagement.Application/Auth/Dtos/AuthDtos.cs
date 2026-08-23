using System.ComponentModel.DataAnnotations;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Auth.Dtos;

public record RegisterRequest(
    [Required, StringLength(200, MinimumLength = 2)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    UserRole Role = UserRole.Customer);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RefreshTokenRequest([Required] string RefreshToken);

public record RevokeTokenRequest([Required] string RefreshToken);

public record UserDto(int Id, string FullName, string Email, UserRole Role, bool IsActive, DateTime CreatedAtUtc);

public record AuthResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, UserDto User);
