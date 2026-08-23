using TicketManagement.Application.Activities.Dtos;

namespace TicketManagement.Application.Activities;

public interface IActivityService
{
    Task<IReadOnlyList<TicketActivityDto>> GetTimelineAsync(int ticketId, CancellationToken ct = default);
}
