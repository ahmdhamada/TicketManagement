using System.ComponentModel.DataAnnotations;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Users.Dtos;

public record AdminUserDto(int Id, string FullName, string Email, UserRole Role, bool IsActive, DateTime CreatedAtUtc);

public record CreateUserRequest(
    [property: Required, StringLength(200, MinimumLength = 2)] string FullName,
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(8)] string Password,
    [property: Required] UserRole Role);

public record UpdateUserRequest(
    [property: Required, StringLength(200, MinimumLength = 2)] string FullName,
    [property: Required] UserRole Role,
    [property: Required] bool IsActive);
