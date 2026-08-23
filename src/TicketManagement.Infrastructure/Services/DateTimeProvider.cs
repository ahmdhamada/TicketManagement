using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
