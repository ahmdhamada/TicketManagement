using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Enums;

namespace TicketManagement.UnitTests.Application.TestDoubles;

public class FakeCurrentUserService : ICurrentUserService
{
    public FakeCurrentUserService(int userId, UserRole role)
    {
        UserId = userId;
        Role = role;
    }

    public int? UserId { get; }
    public UserRole? Role { get; }
    public bool IsAuthenticated => true;
}
