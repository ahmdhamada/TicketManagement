using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}
