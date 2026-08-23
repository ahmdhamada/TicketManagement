using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.UnitTests.Application.TestDoubles;

public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
