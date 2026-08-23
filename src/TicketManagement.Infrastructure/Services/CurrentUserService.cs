using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public int? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(value, out var role) ? role : null;
        }
    }
}
